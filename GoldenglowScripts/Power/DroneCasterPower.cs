using Goldenglow.Card;
using Goldenglow.Orb;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Power;

[RegisterPower]
public sealed class DroneCasterPower : AbstractGoldenglowPower
{
    public override async Task AfterOrbChanneled(PlayerChoiceContext choiceContext, Player player, OrbModel orb)
    {
        if (orb is not BuoyOrb buoy) return;

        await GoldenglowOrbCmd.Passive(choiceContext, buoy, null);
        Flash();
    }
}
