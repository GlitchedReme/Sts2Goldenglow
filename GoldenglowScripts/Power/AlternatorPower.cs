using Goldenglow.Card;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Power;

/// <summary>
/// Each turn start, a random character gains 1 orb slot.
/// Amount stacks: each stack = one additional slot per turn.
/// </summary>
[RegisterPower]
public sealed class AlternatorPower : AbstractGoldenglowPower
{
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner)) return;

        var allCreatures = new List<Creature>();
        if (combatState.Players.Count > 0)
            allCreatures.Add(combatState.Players[0].Creature);
        var enemies = combatState.Enemies;
        for (int i = 0; i < enemies.Count; i++)
            if (!enemies[i].IsDead)
                allCreatures.Add(enemies[i]);

        if (allCreatures.Count == 0) return;

        for (int i = 0; i < Amount; i++)
        {
            var target = allCreatures[Random.Shared.Next(allCreatures.Count)];
            if (target.IsPlayer && target.Player != null)
            {
                await MegaCrit.Sts2.Core.Commands.OrbCmd.AddSlots(target.Player, 1);
            }
            else if (!target.IsPlayer)
            {
                var creatureNode = NCombatRoom.Instance?.GetCreatureNode(target);
                if (creatureNode == null) continue;

                var mgr = GoldenglowOrbCmd.GetOrCreateMonsterOrbManager(target);
                mgr.SetCapacity(mgr.Capacity + 1);
            }
        }
    }
}
