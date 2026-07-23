using System.Collections.Generic;
using System.Threading.Tasks;
using Goldenglow.Orb;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Power;

[RegisterPower]
public sealed class BuoyRecoveryPower : AbstractGoldenglowPower
{
    public override async Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets)
    {
        if (orb is not BuoyOrb buoy) return;
        if (buoy.Holder == null || buoy.Holder.IsPlayer) return;
        if (Owner.Player == null) return;

        Flash();
        for (int i = 0; i < Amount; i++)
            await OrbCmd.Channel<BuoyOrb>(choiceContext, Owner.Player);
    }
}
