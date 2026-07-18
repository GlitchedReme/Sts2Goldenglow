using System.Reflection;
using System.Text.Json;
using Godot;
using Goldenglow.Core;
using Goldenglow.Patch;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib;
using STS2RitsuLib.Audio;
using STS2RitsuLib.Data;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Networking.Sidecar;
using STS2RitsuLib.Patching.Core;
using STS2RitsuLib.Utils.Persistence;

namespace Goldenglow;

public sealed class SkinState
{
    public string? Skin { get; set; }
}

public record SkinSyncMessage(ulong NetId, string SkinKey);

[ModInitializer(nameof(Init))]
public class Entry
{
    public const string ModId = "Goldenglow";
    public static readonly MegaCrit.Sts2.Core.Logging.Logger Logger = RitsuLibFramework.CreateLogger(ModId);

    public static RitsuLibSidecarMessageDescriptor<SkinSyncMessage> SkinSyncDescriptor = null!;

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
        patcher.RegisterPatch<CreatureSkinReadyPatch>();
        patcher.RegisterPatch<MerchantSkinPatch>();
        patcher.RegisterPatch<RestSiteSkinPatch>();
        patcher.RegisterPatch<HoverTipShownInInspectPatch>();
        if (!patcher.PatchAll())
            throw new InvalidOperationException("Critical patches failed.");

        var assembly = Assembly.GetExecutingAssembly();
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);

        FmodStudioDeferredBankRegistration.RegisterBank("res://Goldenglow/audio/Goldenglow.bank");
        FmodStudioDeferredBankRegistration.RegisterStudioGuidMappings("res://Goldenglow/audio/GUIDs.txt");

        var profileStore = ModDataStore.For(ModId);
        profileStore.Register<SkinState>(
            key: "skin_prefs",
            fileName: "goldenglow_skin.json",
            scope: SaveScope.Profile,
            defaultFactory: () => new SkinState());

        SkinSyncDescriptor = new RitsuLibSidecarMessageDescriptor<SkinSyncMessage>(
            ModuleId: ModId,
            MessageKey: "skin_sync_v1",
            Serialize: msg => JsonSerializer.SerializeToUtf8Bytes(msg),
            Deserialize: p => JsonSerializer.Deserialize<SkinSyncMessage>(p)!,
            Delivery: RitsuLibSidecarDeliverySemantics.StableSync);

        RitsuLibSidecarTypedMessageRegistry.Subscribe(SkinSyncDescriptor, ctx =>
        {
            SkinResources.RemoteSkins[ctx.Message.NetId] = ctx.Message.SkinKey;

            if (ctx.IsHostIngest && ctx.Message.NetId != ctx.SenderNetId)
            {
                RitsuLibSidecarTypedMessageRegistry.Broadcast(
                    RunManager.Instance?.NetService, SkinSyncDescriptor, ctx.Message);
            }

            GoldenglowSingleton.ApplySkinByNetId(ctx.Message.NetId);
        });

        RitsuLibFramework.SubscribeLifecycle<RunStartedEvent>(evt =>
        {
            SendSkinSync(SkinResources.SelectedSkinKey);
        });

        RitsuLibFramework.SubscribeLifecycle<RunLoadedEvent>(evt =>
        {
            SkinResources.RemoteSkins.Clear();
            SendSkinSync(SkinResources.SelectedSkinKey);
        });

        Logger.Info("[Goldenglow] Sidecar skin sync registered");
    }

    public static void SendSkinSync(string skinKey)
    {
        var netService = RunManager.Instance?.NetService;
        if (netService == null)
            return;

        var netId = netService.NetId;
        var msg = new SkinSyncMessage(netId, skinKey);
        Logger.Info($"[Goldenglow] SendSkinSync: netId={netId}, skin='{skinKey}', netType={netService.Type}");

        SkinResources.RemoteSkins[netId] = skinKey;

        switch (netService.Type)
        {
            case NetGameType.Host:
            case NetGameType.Singleplayer:
                RitsuLibSidecarTypedMessageRegistry.Broadcast(netService, SkinSyncDescriptor, msg);
                break;
            case NetGameType.Client:
                RitsuLibSidecarTypedMessageRegistry.SendToHost(netService, SkinSyncDescriptor, msg);
                break;
        }
    }
}
