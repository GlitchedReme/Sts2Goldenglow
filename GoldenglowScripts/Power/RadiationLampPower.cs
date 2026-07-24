using Goldenglow.Capabilities;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;
using Goldenglow.Orb;

namespace Goldenglow.Power;

[RegisterPower]
public sealed class RadiationLampPower : AbstractGoldenglowPower
{
    public override async Task AfterOrbChanneled(PlayerChoiceContext choiceContext, Player player, OrbModel orb)
    {
        if (orb is not BuoyOrb) return;
        var cap = ModelCapabilityRegistry.Create<OrbBoostCapability>();
        cap.DynamicVars["Amount"].BaseValue = Amount;
        orb.AddCapability(cap);
        Flash();
    }
}
