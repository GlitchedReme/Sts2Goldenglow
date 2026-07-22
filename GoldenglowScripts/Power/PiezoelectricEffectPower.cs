using Goldenglow.Card;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Power;

/// <summary>
/// Each turn start, pulses once.
/// Amount = number of pulses per turn (stacks).
/// </summary>
[RegisterPower]
public sealed class PiezoelectricEffectPower : AbstractGoldenglowPower
{
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner)) return;
        if (Owner.Player == null) return;

        Flash();
        for (int i = 0; i < Amount; i++)
            await GoldenglowCmd.Pulse(Owner.Player, null, null);
    }
}
