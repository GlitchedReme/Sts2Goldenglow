using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Power;

[RegisterPower]
public sealed class FreshPerfumePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(base.Owner)) return;
        if (base.Owner.Player == null) return;

        Flash();
        await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), 1, base.Owner.Player);

        if (Amount > 1)
            await PowerCmd.Apply<FreshPerfumePower>(new ThrowingPlayerChoiceContext(), base.Owner, -1, base.Owner, null!);
        else
            await PowerCmd.Remove(this);
    }
}
