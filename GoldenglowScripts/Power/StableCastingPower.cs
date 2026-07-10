using System.Collections.Generic;
using System.Threading.Tasks;
using Goldenglow.Card;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Power;

/// <summary>
/// Each turn start, pulses once.
/// Amount = number of pulses per turn (stacks).
/// </summary>
[RegisterPower]
public sealed class StableCastingPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(base.Owner)) return;
        if (base.Owner.Player == null) return;

        Flash();
        for (int i = 0; i < Amount; i++)
            await GoldenglowCmd.Pulse(base.Owner.Player);
    }
}
