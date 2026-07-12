using System.Reflection;
using Goldenglow.Patch;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Audio;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;
using STS2RitsuLib.RunData;

namespace Goldenglow;

public sealed class SkinState
{
    public string? Skin { get; set; } = "default";
}

[ModInitializer(nameof(Init))]
public class Entry
{
    public const string ModId = "Goldenglow";
    public static readonly Logger Logger = RitsuLibFramework.CreateLogger(ModId);

    public static PlayerRunSavedData<SkinState> Skin { get; private set; } = null!;

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
        patcher.RegisterPatch<CustomRelicTextPatch>();
        patcher.RegisterPatch<MonsterOrbOwnerPatch>();
        if (!patcher.PatchAll())
            throw new InvalidOperationException("Critical patches failed.");

        var assembly = Assembly.GetExecutingAssembly();
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);

        FmodStudioDeferredBankRegistration.RegisterBank("res://Goldenglow/audio/Goldenglow.bank");
        FmodStudioDeferredBankRegistration.RegisterStudioGuidMappings("res://Goldenglow/audio/GUIDs.txt");

        using (RitsuLibFramework.BeginModDataRegistration(ModId))
        {
            var store = RitsuLibFramework.GetRunSavedDataStore(ModId);
            Skin = store.RegisterPerPlayer(
                key: "skin",
                defaultFactory: () => new SkinState(),
                options: new RunSavedDataOptions
                {
                    WritePolicy = RunSavedDataWritePolicy.WhenSet,
                    SyncLobbyOnChange = true,
                });
        }
    }
}
