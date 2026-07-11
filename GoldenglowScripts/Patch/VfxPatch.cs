using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
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

internal class CustomPowerTextPatch : IPatchMethod
{
    public static string PatchId => "goldenglow_custom_power_text_patch";
    public static string PatchDescription => "Patch custom power text for Goldenglow powers";

    public static ModPatchTarget[] GetTargets() => [new(typeof(NPower), "RefreshAmount")];

    internal static FieldInfo _amountLabelField = AccessTools.Field(typeof(NPower), "_amountLabel");

    internal static bool Prefix(NPower __instance)
    {
        if (__instance.Model is IPowerCustomTextProvider customTextProvider)
        {
            if (_amountLabelField.GetValue(__instance) is not MegaLabel label) return true;
            label.AddThemeColorOverride(ThemeConstants.Label.FontColor, __instance.Model.AmountLabelColor);
            label.SetTextAutoSize((__instance.Model.StackType == PowerStackType.Counter) ? customTextProvider.CustomText : string.Empty);
            return false;
        }
        return true;
    }
}
