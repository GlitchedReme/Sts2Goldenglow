using System.Collections;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using HarmonyLib;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace Goldenglow.Bootstrap;

[ModInitializer(nameof(Initialize))]
public static class Bootstrap
{
    private const string ModId = "Goldenglow";

    private static readonly List<Assembly> variantAssemblies = [];
    private static readonly Logger logger = new("Goldenglow.Bootstrap", LogType.Generic);

    public static void Initialize()
    {
        var dir = Path.GetDirectoryName(typeof(Bootstrap).Assembly.Location);
        var libRoot = dir is null ? null : Path.Combine(dir, "lib");
        if (libRoot is null || !Directory.Exists(libRoot))
        {
            logger.Error("Cannot resolve lib directory.");
            return;
        }

        var host = DetectHostVersion();
        var hostText = host?.ToString() ?? "unknown";

        if (!TryPickVariant(libRoot, host, out var variant))
        {
            logger.Error($"No compatible variant (host={hostText}).");
            return;
        }

        logger.Info($"Host={hostText}, variant={variant.Compat}.");

        var asm = LoadVariantAssembly(variant.Dll);
        if (asm is null) return;

        variantAssemblies.Add(asm);

        PatchModTypes();
        AssociateAssembly(asm);
        InvokeRealInitializer(asm);
    }

    // Probe three sources in order: the live ReleaseInfoManager, the on-disk
    // release_info.json (via Godot.OS.GetDataDir), then the ReleaseInfo assembly version.
    private static Version? DetectHostVersion()
    {
        try
        {
            if (TryParseVersion(ReleaseInfoManager.Instance.ReleaseInfo?.Version, out var v))
                return v;
        }
        catch (Exception ex) { logger.Warn($"ReleaseInfoManager unavailable: {ex.Message}"); }

        try
        {
            var osType = Type.GetType("Godot.OS, GodotSharp", false)
                ?? Type.GetType("Godot.OS, GodotSharpEditor", false)
                ?? AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => a.GetType("Godot.OS", false))
                    .FirstOrDefault(t => t is not null);

            var dataDir = osType?.GetMethod("GetDataDir", BindingFlags.Public | BindingFlags.Static)
                ?.Invoke(null, null) as string;

            if (!string.IsNullOrWhiteSpace(dataDir))
            {
                var path = Path.Combine(dataDir, "game", "release_info.json");
                if (File.Exists(path))
                {
                    using var doc = JsonDocument.Parse(File.ReadAllText(path));
                    if (doc.RootElement.TryGetProperty("version", out var el)
                        && TryParseVersion(el.GetString(), out var v))
                        return v;
                }
            }
        }
        catch (Exception ex) { logger.Warn($"release_info.json fallback failed: {ex.Message}"); }

        var av = typeof(ReleaseInfo).Assembly.GetName().Version;
        if (av is not null && !(av.Major == 0 && av is { Minor: 0, Build: 0, Revision: 0 }))
            return av;

        return null;
    }

    // Scan lib/<version>/ for validated variants and pick the newest one that is
    // still <= host (or the newest overall when host is unknown).
    private static bool TryPickVariant(string libRoot, Version? host, out (string Compat, Version Ver, string Dll) variant)
    {
        variant = default;
        var variants = new List<(string Compat, Version Ver, string Dll)>();
        foreach (var d in Directory.GetDirectories(libRoot))
        {
            var name = Path.GetFileName(d);
            if (!TryParseVersion(name, out var ver)) continue;

            var marker = Path.Combine(d, "compat-target.txt");
            if (!File.Exists(marker) || File.ReadAllText(marker).Trim() != name) continue;

            var dll = Path.Combine(d, $"{ModId}.dll");
            if (!File.Exists(dll)) continue;

            variants.Add((name, ver, dll));
        }

        if (variants.Count == 0) return false;

        variants.Sort((a, b) => a.Ver.CompareTo(b.Ver));
        variant = host is null ? variants[^1] : variants.LastOrDefault(v => v.Ver <= host, variants[^1]);
        return true;
    }

    private static Assembly? LoadVariantAssembly(string dll)
    {
        try
        {
            var alc = AssemblyLoadContext.GetLoadContext(typeof(Bootstrap).Assembly) ?? AssemblyLoadContext.Default;
            return alc.LoadFromAssemblyPath(dll);
        }
        catch (Exception ex)
        {
            logger.Error($"Load failed: {ex}");
            return null;
        }
    }

    // Targeted patch (not PatchAll) so the variant's types show up in ReflectionHelper.ModTypes.
    private static void PatchModTypes()
    {
        try
        {
            new Harmony("Goldenglow.Bootstrap").Patch(
                AccessTools.PropertyGetter(typeof(ReflectionHelper), nameof(ReflectionHelper.ModTypes)),
                postfix: new HarmonyMethod(typeof(Bootstrap), nameof(ModTypesPostfix)));
        }
        catch (Exception ex)
        {
            logger.Warn($"Harmony patch failed: {ex.Message}");
        }
    }

    // Prefer the public API; fall back to reflecting into Mod's assembly field(s) on older hosts.
    private static void AssociateAssembly(Assembly asm)
    {
        var mi = typeof(ModManager).GetMethod("AssociateAssemblyWithMod",
            BindingFlags.Public | BindingFlags.Static, null,
            [typeof(string), typeof(Assembly)], null);
        if (mi is not null)
        {
            try { mi.Invoke(null, [ModId, asm]); return; }
            catch (Exception ex) { logger.Warn($"AssociateAssemblyWithMod failed: {ex.Message}"); }
        }

        var mod = ModManager.Mods.FirstOrDefault(m => m.manifest?.id == ModId);
        if (mod is null) return;

        if (typeof(Mod).GetField("assemblies", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.GetValue(mod) is IList list
            && !list.Cast<object?>().Any(x => ReferenceEquals(x, asm)))
        {
            list.Add(asm);
        }
        else
        {
            var af = typeof(Mod).GetField("assembly",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (af is not null && af.GetValue(mod) is null)
                af.SetValue(mod, asm);
        }
    }

    private static void InvokeRealInitializer(Assembly asm)
    {
        foreach (var t in SafeGetTypes(asm))
        {
            var attr = t.GetCustomAttribute<ModInitializerAttribute>();
            if (attr is null) continue;

            var method = t.GetMethod(attr.initializerMethod,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (method is null)
            {
                logger.Error($"{t.FullName}: method '{attr.initializerMethod}' not found.");
                continue;
            }

            method.Invoke(null, null);
            return;
        }

        logger.Error($"No ModInitializerAttribute found in {asm.FullName}.");
    }

    // GetTypes() that survives a partial load by returning whatever types did resolve.
    private static IEnumerable<Type> SafeGetTypes(Assembly asm)
    {
        try { return asm.GetTypes(); }
        catch (ReflectionTypeLoadException ex)
        {
            logger.Warn($"Partial type load for {asm.FullName}: {ex.Message}");
            return ex.Types.OfType<Type>();
        }
    }

    internal static void ModTypesPostfix(ref Type[] __result)
    {
        var extra = variantAssemblies.SelectMany(SafeGetTypes).ToList();
        if (extra.Count > 0)
            __result = [.. __result.Concat(extra).Distinct()];
    }

    private static bool TryParseVersion(string? text, out Version version)
    {
        version = new(0, 0);
        if (string.IsNullOrWhiteSpace(text)) return false;

        var s = text.Trim();
        if (s.IndexOfAny(['-', '+']) is >= 0 and var cut) s = s[..cut].Trim();
        if (s.Length >= 2 && s[0] is 'v' or 'V' && char.IsDigit(s[1])) s = s[1..];

        if (Version.TryParse(s, out var v)) { version = v; return true; }
        return false;
    }
}
