using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching.Models;
using Goldenglow.Ui;

namespace Goldenglow.Patch;

/// <summary>
/// Shows discard pile top card previews on the right side of the screen when playing a card with Attract.
/// Patches into TryShowEvokingOrbs (triggered during target selection).
/// </summary>
internal class ShowAttractPreviewPatch : IPatchMethod
{
    public static string PatchId => "goldenglow_show_attract_preview_patch";
    public static string PatchDescription => "Show discard pile preview when playing an attract card";

    public static ModPatchTarget[] GetTargets() => [new(typeof(NCardPlay), "TryShowEvokingOrbs")];

    internal static void Postfix(NCardPlay __instance)
    {
        var card = __instance.Holder?.CardNode?.Model;
        if (card == null) return;
        Entry.Logger.Info($"ShowAttractPreviewPatch: card {card.Title} with Attract {card.DynamicVars.GetValueOrDefault("Goldenglow_Attract")?.BaseValue}");

        AttractUi.Get()?.UpdatePreview(card);
    }
}

/// <summary>
/// Hides the attract preview when the card is released or cancelled.
/// Patches into HideEvokingOrbs.
/// </summary>
internal class HideAttractPreviewPatch : IPatchMethod
{
    public static string PatchId => "goldenglow_hide_attract_preview_patch";
    public static string PatchDescription => "Hide attract preview on card release";

    public static ModPatchTarget[] GetTargets() => [new(typeof(NCardPlay), "HideEvokingOrbs")];

    internal static void Postfix(NCardPlay __instance)
    {
        AttractUi.Get()?.Hide();
    }
}
