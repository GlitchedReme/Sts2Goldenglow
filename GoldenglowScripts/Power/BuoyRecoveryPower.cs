using System.Collections.Generic;
using System.Threading.Tasks;
using Goldenglow.Orb;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Power;

/// <summary>
/// Whenever a BuoyOrb is evoked on an enemy, channel a BuoyOrb to the player.
/// </summary>
[RegisterPower]
public sealed class BuoyRecoveryPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

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
