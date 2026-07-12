using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;
using STS2RitsuLib.Models.Capabilities;
using Goldenglow.Capabilities;
using Goldenglow.Patch;
using Goldenglow.Ui;
using Goldenglow.Card;

namespace Goldenglow.Core;

[RegisterSingleton]
public class GoldenglowSingleton() : HookedSingletonModel(HookType.Combat)
{
    private static readonly Dictionary<Player, int> _pulseValues = [];

    public static int GetPulse(Player? p) => p != null ? _pulseValues.GetValueOrDefault(p, 1) : 1;
    public static void IncrementPulse(Player p) => _pulseValues[p] = GetPulse(p) + 1;

    public override bool TryModifyCardRewardOptionsLate(Player player, List<CardCreationResult> cardRewardOptions, CardCreationOptions creationOptions)
    {
        bool hasModified = false;
        foreach (var option in cardRewardOptions)
        {
            if (option.Card is ICardOnGeneratedAsReward callback)
                hasModified |= callback.OnGeneratedAsReward(player, creationOptions);
        }
        return hasModified;
    }

    public override decimal ModifyOrbValue(OrbModel orb, decimal value)
    {
        if (!MonsterOrbPatch.OwnerState.TryGetValue(orb, out _))
            return value;

        if (!ModelCapabilities.TryGet(orb, out var caps))
            return value;

        foreach (var cap in caps.All)
        {
            if (cap is AbstractModel model)
                value = model.ModifyOrbValue(orb, value);
        }
        return value;
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player)
        {
            StaticCapability.ResetAll();
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player)
        {
            var combatState = participants.FirstOrDefault()?.CombatState;
            if (combatState == null) return;
            var enemies = combatState.GetCreaturesOnSide(CombatSide.Enemy).ToList();
            foreach (var enemy in enemies)
            {
                if (enemy.IsDead) continue;
                if (MonsterOrbManager.MonsterOrbManagerState.TryGetValue(enemy, out var manager) && manager != null)
                {
                    await manager.BeforeTurnEnd(choiceContext);
                    await manager.AfterTurnStart(choiceContext);
                }
            }
        }
    }

    public override async Task BeforeDeath(Creature creature)
    {
        if (MonsterOrbManager.MonsterOrbManagerState.TryGetValue(creature, out var manager) && manager != null)
        {
            manager.Clear();
        }
    }

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        _pulseValues.Clear();
        MonsterOrbManager.ClearAllInstances();
    }
}
