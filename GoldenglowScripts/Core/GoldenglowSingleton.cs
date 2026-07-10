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
using Goldenglow.Capabilities;
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

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player)
        {
            StaticCapability.ResetAll();
        }

        if (side == CombatSide.Enemy)
        {
            foreach (var enemy in participants)
            {
                if (MonsterOrbManager.MonsterOrbManagerState.TryGetValue(enemy, out var manager) && manager != null)
                {
                    await manager.BeforeTurnEnd(choiceContext);
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
