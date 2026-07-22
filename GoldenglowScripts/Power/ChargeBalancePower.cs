using System.Collections.Generic;
using System.Threading.Tasks;
using Goldenglow.Capabilities;
using Goldenglow.Card;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;

namespace Goldenglow.Power;

[RegisterPower]
public sealed class ChargeBalancePower : AbstractGoldenglowPower
{
    public override async Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets)
    {
        if (orb.Owner?.Creature != Owner) return;

        var orbs = orb.Owner.PlayerCombatState?.OrbQueue.Orbs ?? GoldenglowOrbCmd.GetMonsterOrbManager(orb.Owner.Creature)?.GetOrbs();
        if (orbs == null) return;

        for (int i = 0; i < orbs.Count; i++)
        {
            var o = orbs[i];
            var cap = ModelCapabilityRegistry.Create<OrbBoostCapability>();
            cap.DynamicVars["Amount"].BaseValue = Amount;
            o.AddCapability(cap);
        }
        Flash();
    }
}
