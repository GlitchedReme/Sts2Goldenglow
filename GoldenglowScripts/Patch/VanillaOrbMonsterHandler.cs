using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Goldenglow.Patch;

internal static class VanillaOrbMonsterHandler
{
    private static void ActivatePassive(OrbModel orb) => Traverse.Create(orb).Method("ActivatePassive").GetValue();
    private static void PlayPassiveSfx(OrbModel orb) => Traverse.Create(orb).Method("PlayPassiveSfx").GetValue();
    private static void ActivateEvoke(OrbModel orb, Creature[] targets) => Traverse.Create(orb).Method("ActivateEvoke", [typeof(Creature[])]).GetValue(targets);
    private static void PlayEvokeSfx(OrbModel orb) => Traverse.Create(orb).Method("PlayEvokeSfx").GetValue();

    public static async Task<bool> TryHandleBeforeTurnEnd(OrbModel orb, Creature monster, PlayerChoiceContext choiceContext)
    {
        var combatState = monster.CombatState;
        if (combatState == null) return false;

        switch (orb)
        {
            case LightningOrb lightning:
                ActivatePassive(lightning);
                PlayPassiveSfx(lightning);
                await CreatureCmd.Damage(choiceContext, monster, lightning.PassiveVal, ValueProp.Unpowered, monster);
                return true;
            case FrostOrb frost:
                ActivatePassive(frost);
                PlayPassiveSfx(frost);
                foreach (var player in combatState.Players)
                    await CreatureCmd.GainBlock(player.Creature, frost.PassiveVal, ValueProp.Unpowered, null);
                return true;
            case DarkOrb dark:
                ActivatePassive(dark);
                return true;
            case GlassOrb glass:
            {
                var players = combatState.Players.Select(p => p.Creature).Where(c => c.IsHittable).ToList();
                if (players.Count == 0) return true;
                ActivatePassive(glass);
                PlayPassiveSfx(glass);
                var passiveVal = glass.PassiveVal;
                if (passiveVal > 0)
                    await CreatureCmd.Damage(choiceContext, players, passiveVal, ValueProp.Unpowered, monster);
                return true;
            }
        }

        return false;
    }

    public static async Task<bool> TryHandleAfterTurnStart(OrbModel orb, Creature monster, PlayerChoiceContext choiceContext)
    {
        var combatState = monster.CombatState;
        if (combatState == null) return false;

        if (orb is PlasmaOrb plasma)
        {
            ActivatePassive(plasma);
            foreach (var player in combatState.Players)
                await PlayerCmd.GainEnergy(plasma.PassiveVal, player);
            return true;
        }

        return false;
    }

    public static async Task<bool> TryHandleEvoke(OrbModel orb, Creature monster, PlayerChoiceContext choiceContext)
    {
        switch (orb)
        {
            case LightningOrb lightning:
                ActivateEvoke(lightning, [monster]);
                PlayEvokeSfx(lightning);
                await CreatureCmd.Damage(choiceContext, monster, lightning.EvokeVal, ValueProp.Unpowered, monster);
                return true;
            case FrostOrb frost:
            {
                var combatState = monster.CombatState;
                if (combatState == null) return true;
                var targets = combatState.Players.Select(p => p.Creature).ToArray();
                ActivateEvoke(frost, targets);
                PlayEvokeSfx(frost);
                foreach (var player in combatState.Players)
                    await CreatureCmd.GainBlock(player.Creature, frost.EvokeVal, ValueProp.Unpowered, null);
                return true;
            }
            case DarkOrb dark:
                ActivateEvoke(dark, [monster]);
                PlayEvokeSfx(dark);
                await CreatureCmd.Damage(choiceContext, monster, dark.EvokeVal, ValueProp.Unpowered, monster);
                return true;
            case PlasmaOrb plasma:
            {
                var combatState = monster.CombatState;
                if (combatState == null) return true;
                var targets = combatState.Players.Select(p => p.Creature).ToArray();
                ActivateEvoke(plasma, targets);
                foreach (var player in combatState.Players)
                    await PlayerCmd.GainEnergy(plasma.EvokeVal, player);
                return true;
            }
            case GlassOrb glass:
            {
                var combatState = monster.CombatState;
                if (combatState == null) return true;
                var players = combatState.Players.Select(p => p.Creature).Where(c => c.IsHittable).ToList();
                if (players.Count == 0) return true;
                ActivateEvoke(glass, [.. players]);
                await CreatureCmd.Damage(choiceContext, players, glass.EvokeVal, ValueProp.Unpowered, monster);
                return true;
            }
        }

        return false;
    }
}