using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Patching.Models;

namespace Goldenglow.Patch;

internal class FocusPowerModifyOrbValuePatch : IPatchMethod
{
    public static string PatchId => "goldenglow_focus_power_modify_orb_value_patch";
    public static string PatchDescription => "Fix FocusPower orb ownership check for monster orbs";

    public static ModPatchTarget[] GetTargets() => [new(typeof(FocusPower), "ModifyOrbValue", [typeof(OrbModel), typeof(decimal)])];

    internal static bool Prefix(FocusPower __instance, OrbModel orb, decimal value, ref decimal __result)
    {
        var owner = __instance.Owner;

        if (owner.IsPlayer)
            return true;

        if (!MonsterOrbPatch.OwnerState.TryGetValue(orb, out var orbOwner) || orbOwner != owner)
        {
            __result = value;
            return false;
        }

        __result = Math.Max(value + __instance.Amount, 0m);
        return false;
    }
}
