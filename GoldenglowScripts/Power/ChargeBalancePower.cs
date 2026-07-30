using Goldenglow.Patch;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Power;

[RegisterPower]
public sealed class ChargeBalancePower : AbstractGoldenglowPower
{
    public override async Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets)
    {
        if (!(orb.Owner?.Creature == Owner || (MonsterOrbPatch.OwnerState.TryGetValue(orb, out var creature) && creature == Owner))) return;
        await PowerCmd.Apply<PermanentMagnetTempPower>(choiceContext, Owner, Amount, Applier, null);
        Flash();
    }
}
