using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching.Models;
using MegaCrit.Sts2.Core.Models;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Combat;

namespace Goldenglow.Patch;

internal static class MonsterOrbPatch
{
    internal static AttachedState<OrbModel, Creature?> OwnerState = new(() => null);

    internal static bool IsMonster(NCreature creature)
        => !creature.Entity.IsPlayer && creature.Entity.PetOwner == null;
}

internal class InitializeOrbManagerPatch : IPatchMethod
{
    public static string PatchId => "goldenglow_initialize_orb_manager_patch";
    public static string PatchDescription => "Attach MonsterOrbManager to monsters";

    public static ModPatchTarget[] GetTargets() => [new(typeof(NCreature), "_Ready")];

    internal static void Prefix(NCreature __instance)
    {
        if (!MonsterOrbPatch.IsMonster(__instance)) return;
        Card.GoldenglowOrbCmd.GetOrCreateMonsterOrbManager(__instance.Entity);
        __instance.UpdateNavigation();
    }
}

internal class OrbTipOnMonsterPatch : IPatchMethod
{
    public static string PatchId => "goldenglow_orb_tip_on_monster_patch";
    public static string PatchDescription => "Render monster-friendly hover tips for orbs without a player owner";

    public static ModPatchTarget[] GetTargets() => [new(typeof(OrbModel), "HoverTips", MethodType.Getter)];

    internal static bool Prefix(OrbModel __instance, ref IEnumerable<IHoverTip> __result)
    {
        if (__instance.Owner != null && !MonsterOrbPatch.OwnerState.TryGetValue(__instance, out _))
            return true;

        var owner = MonsterOrbPatch.OwnerState[__instance];
        var extraHoverTips = Traverse.Create(__instance)
            .Property<IEnumerable<IHoverTip>>("ExtraHoverTips").Value;
        var list = extraHoverTips?.ToList() ?? [];

        var hasSmart = LocString.Exists("orbs", $"{__instance.Id.Entry}.smartDescriptionOnMonster");
        if (hasSmart && __instance.IsMutable)
        {
            var smart = new LocString("orbs", $"{__instance.Id.Entry}.smartDescriptionOnMonster");
            if (owner != null)
                smart.Add("Owner", owner.Name);
            smart.Add("Passive", __instance.PassiveVal);
            smart.Add("Evoke", __instance.EvokeVal);
            list.Add(new HoverTip(__instance, smart));
        }
        else
        {
            list.Add(__instance.DumbHoverTip);
        }
        __result = list;
        return false;
    }
}

internal class MonsterOrbModifyValuePatch : IPatchMethod
{
    public static string PatchId => "goldenglow_monster_orb_modify_value_patch";
    public static string PatchDescription => "Modify orb values for monsters";

    public static ModPatchTarget[] GetTargets() => [new(typeof(OrbModel), "ModifyOrbValue", [typeof(decimal)])];

    internal static bool Prefix(OrbModel __instance, decimal result, ref decimal __result)
    {
        if (__instance.Owner == null && MonsterOrbPatch.OwnerState.TryGetValue(__instance, out var owner) && owner != null && owner.CombatState != null)
        {
            __result = Hook.ModifyOrbValue(owner.CombatState, __instance, result);
            return false;
        }
        return true;
    }
}

internal class MonsterOrbCombatStatePatch : IPatchMethod
{
    public static string PatchId => "goldenglow_monster_orb_combat_state_patch";
    public static string PatchDescription => "Modify orb combat state for monsters";

    public static ModPatchTarget[] GetTargets() => [new(typeof(OrbModel), "CombatState", MethodType.Getter)];

    internal static bool Prefix(OrbModel __instance, ref ICombatState __result)
    {
        if (__instance.Owner == null && MonsterOrbPatch.OwnerState.TryGetValue(__instance, out var owner) && owner != null && owner.CombatState != null)
        {
            __result = owner.CombatState;
            return false;
        }
        return true;
    }
}
