using System.Reflection;
using Goldenglow.Patch;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Audio;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;

namespace Goldenglow;

[ModInitializer(nameof(Init))]
public class Entry
{
    public const string ModId = "Goldenglow";
    public static readonly Logger Logger = RitsuLibFramework.CreateLogger(ModId);

    public static void Init()
    {
        var patcher = RitsuLibFramework.CreatePatcher(ModId, ModId);
        patcher.RegisterPatch<ShowAttractPreviewPatch>();
        patcher.RegisterPatch<HideAttractPreviewPatch>();

        patcher.RegisterPatch<InitializeOrbManagerPatch>();
        patcher.RegisterPatch<OrbTipOnMonsterPatch>();
        patcher.RegisterPatch<MonsterOrbCombatStatePatch>();
        patcher.RegisterPatch<MonsterOrbModifyValuePatch>();

        patcher.RegisterPatch<ScenePathPatch>();
        patcher.RegisterPatch<CustomPowerTextPatch>();
        if (!patcher.PatchAll())
            throw new InvalidOperationException("Critical patches failed.");

        var assembly = Assembly.GetExecutingAssembly();
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);

        FmodStudioDeferredBankRegistration.RegisterBank("res://Goldenglow/audio/Goldenglow.bank");
        FmodStudioDeferredBankRegistration.RegisterStudioGuidMappings("res://Goldenglow/audio/GUIDs.txt");
    }
}
