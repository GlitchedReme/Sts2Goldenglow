using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;

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
            Node2D node2D = PreloadManager.Cache.GetScene(path).Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
            vfxContainer?.AddChildSafely(node2D);
            node2D.GlobalPosition = position;
            return false;
        }
        return true;
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
        if (__instance.Model is not IPowerCustomTextProvider customTextProvider)
            return true;

        if (_amountLabelField.GetValue(__instance) is not MegaLabel label) return true;
        label.AddThemeColorOverride(ThemeConstants.Label.FontColor, __instance.Model.AmountLabelColor);
        label.SetTextAutoSize((__instance.Model.StackType == PowerStackType.Counter) ? customTextProvider.CustomText : string.Empty);
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
