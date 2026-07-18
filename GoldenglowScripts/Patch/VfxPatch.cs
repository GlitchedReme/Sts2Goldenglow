using System.Reflection;
using Godot;
using GoldenglowCharacter = Goldenglow.Character.Goldenglow;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Runs;
using Goldenglow.Core;
using STS2RitsuLib.Patching.Models;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.InspectScreens;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace Goldenglow.Patch;

internal class ScenePathPatch : IPatchMethod
{
    public static string PatchId => "goldenglow_scene_path_patch";
    public static string PatchDescription => "Patch scene paths for Goldenglow assets";

    public static ModPatchTarget[] GetTargets() => [new(typeof(VfxCmd), "PlayVfx")];

    internal static bool Prefix(Vector2 position, ref string path, Control? vfxContainer)
    {
        if (path.StartsWith("res://Goldenglow/"))
        {
            Node2D node2D = PreloadManager.Cache.GetScene(path).Instantiate<Node2D>();
            vfxContainer?.AddChildSafely(node2D);
            node2D.GlobalPosition = position;
            return false;
        }
        return true;
    }
}

internal class CreatureSkinReadyPatch : IPatchMethod
{
    public static string PatchId => "goldenglow_creature_skin_ready_patch";
    public static string PatchDescription => "Apply custom skin to Goldenglow creatures on ready";

    public static ModPatchTarget[] GetTargets() => [new(typeof(NCreature), "_Ready")];

    internal static void Postfix(NCreature __instance)
    {
        var creature = __instance.Entity;
        if (!creature.IsPlayer) return;

        var player = creature.Player;
        if (player == null) return;

        if (player.Character is not GoldenglowCharacter) return;

        var skinKey = SkinResources.GetSkinKey(player);
        var res = SkinResources.GetSpine(skinKey).Combat;
        var spine = __instance.Visuals?.SpineBody;
        if (spine == null || res == null) return;

        spine.SetSkeletonDataRes(new MegaSkeletonDataResource(res));
        spine.TryGetAnimationState()?.SetAnimation("Idle", loop: true);
    }
}

internal class MerchantSkinPatch : IPatchMethod
{
    public static string PatchId => "goldenglow_merchant_skin_patch";
    public static string PatchDescription => "Apply custom skin to merchant characters";

    public static ModPatchTarget[] GetTargets() => [new(typeof(NMerchantRoom), "AfterRoomIsLoaded")];

    private static readonly FieldInfo _playersField = AccessTools.Field(typeof(NMerchantRoom), "_players");
    private static readonly FieldInfo _playerVisualsField = AccessTools.Field(typeof(NMerchantRoom), "_playerVisuals");

    internal static void Postfix(NMerchantRoom __instance)
    {
        if (_playersField.GetValue(__instance) is not List<Player> players || _playerVisualsField.GetValue(__instance) is not List<NMerchantCharacter> visuals) return;

        for (int i = 0; i < players.Count && i < visuals.Count; i++)
        {
            var player = players[i];
            if (player.Character is not GoldenglowCharacter) continue;

            var skinKey = SkinResources.GetSkinKey(player);
            var res = SkinResources.GetSpine(skinKey).Combat;
            if (res == null) continue;

            var spine = new MegaSprite(visuals[i].GetChild(0));
            spine.SetSkeletonDataRes(new MegaSkeletonDataResource(res));
        }
    }
}

internal class RestSiteSkinPatch : IPatchMethod
{
    public static string PatchId => "goldenglow_restsite_skin_patch";
    public static string PatchDescription => "Apply custom skin to rest-site characters";

    public static ModPatchTarget[] GetTargets() => [new(typeof(NRestSiteCharacter), "_Ready")];

    internal static void Postfix(NRestSiteCharacter __instance)
    {
        var player = __instance.Player;
        if (player == null) return;
        if (player.Character is not GoldenglowCharacter) return;

        var skinKey = SkinResources.GetSkinKey(player);
        var res = SkinResources.GetSpine(skinKey).RestSite;
        if (res == null) return;

        var spine = new MegaSprite(__instance.GetChild(0));
        spine.SetSkeletonDataRes(new MegaSkeletonDataResource(res));
    }
}

public interface IPowerCustomTextProvider
{
    string CustomText { get; }
}

public interface IRelicCustomTextProvider
{
    string CustomText { get; }
}

internal class CustomPowerTextPatch : IPatchMethod
{
    public static string PatchId => "goldenglow_custom_power_text_patch";
    public static string PatchDescription => "Patch custom power text for Goldenglow powers";

    public static ModPatchTarget[] GetTargets() => [new(typeof(NPower), "RefreshAmount")];

    internal static FieldInfo _amountLabelField = AccessTools.Field(typeof(NPower), "_amountLabel");

    internal static bool Prefix(NPower __instance)
    {
        PowerModel model;
        try
        {
            model = __instance.Model;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
        if (model is not IPowerCustomTextProvider customTextProvider)
            return true;

        if (_amountLabelField.GetValue(__instance) is not MegaLabel label) return true;
        label.AddThemeColorOverride(ThemeConstants.Label.FontColor, model.AmountLabelColor);
        label.SetTextAutoSize((model.StackType == PowerStackType.Counter) ? customTextProvider.CustomText : string.Empty);
        return false;
    }
}

internal class CustomRelicTextPatch : IPatchMethod
{
    public static string PatchId => "goldenglow_custom_relic_text_patch";
    public static string PatchDescription => "Patch custom relic text for Goldenglow relics";

    public static ModPatchTarget[] GetTargets() => [new(typeof(NRelicInventoryHolder), "RefreshAmount")];

    internal static FieldInfo _relicField = AccessTools.Field(typeof(NRelicInventoryHolder), "_relic");
    internal static FieldInfo _amountLabelField = AccessTools.Field(typeof(NRelicInventoryHolder), "_amountLabel");

    internal static bool Prefix(NRelicInventoryHolder __instance)
    {
        var relic = _relicField.GetValue(__instance) as NRelic;
        if (relic?.Model is not IRelicCustomTextProvider customTextProvider) return true;

        if (_amountLabelField.GetValue(__instance) is not MegaLabel label) return true;

        if (relic.Model.ShowCounter && RunManager.Instance.IsInProgress)
        {
            label.Visible = true;
            label.SetTextAutoSize(customTextProvider.CustomText);
        }
        else
        {
            label.Visible = false;
        }
        return false;
    }
}

public interface IHovertipShownInInspectOnly
{
    IEnumerable<IHoverTip> HoverTipsShownInInspectOnly { get; }
}

internal sealed class HoverTipShownInInspectPatch : IPatchMethod
{
    private static readonly FieldInfo? CardsField = AccessTools.Field(typeof(NInspectCardScreen), "_cards");
    private static readonly FieldInfo? CardsIndexField = AccessTools.Field(typeof(NInspectCardScreen), "_index");
    public static string PatchId => "goldenglow_hover_tip_inspect_patch";

    public static string Description =>
        "Patch to show additional hover tips in the inspect card screen for cards that implement IHovertipShownInInspectOnly.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() => [
            new(typeof(NHoverTipSet), "CreateAndShow",
                [typeof(Control), typeof(IEnumerable<IHoverTip>), typeof(HoverTipAlignment)]),
    ];

    public static void Prefix(Control owner, ref IEnumerable<IHoverTip> hoverTips)
    {
        if (owner is NInspectCardScreen inspectCardScreen)
        {
            if (CardsField == null || CardsIndexField == null)
                return;

            int index = (int)(CardsIndexField.GetValue(inspectCardScreen) ?? 0);
            if (CardsField.GetValue(inspectCardScreen) is not List<CardModel> list || index < 0 || index >= list.Count)
                return;

            if (list[index] is not IHovertipShownInInspectOnly model)
                return;

            hoverTips = model.HoverTipsShownInInspectOnly.Concat(hoverTips);
        }
    }
}
