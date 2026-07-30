using Goldenglow.Card;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Power;

[RegisterPower]
public sealed class AlternatorPower : AbstractGoldenglowPower
{
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner)) return;

        var allCreatures = new List<Creature>();
        for (int i = 0; i < combatState.Players.Count; i++)
            if (combatState.Players[i].Creature is { } && !combatState.Players[i].Creature.IsDead)
                allCreatures.Add(combatState.Players[i].Creature);
        var enemies = combatState.Enemies;
        for (int i = 0; i < enemies.Count; i++)
            if (!enemies[i].IsDead)
                allCreatures.Add(enemies[i]);

        if (allCreatures.Count == 0) return;

        for (int i = 0; i < Amount; i++)
        {
            var target = Owner.CombatState?.RunState.Rng.CombatTargets.NextItem(allCreatures);
            if (target == null) continue;
            if (target.IsPlayer && target.Player != null)
            {
                await OrbCmd.AddSlots(target.Player, 1);
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
