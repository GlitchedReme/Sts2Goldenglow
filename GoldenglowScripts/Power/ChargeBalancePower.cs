using System.Collections.Generic;
using System.Threading.Tasks;
using Goldenglow.Capabilities;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Power;

/// <summary>
/// Whenever the owner evokes an orb, all their orbs gain +2 evoke bonus.
/// </summary>
[RegisterPower]
public sealed class ChargeBalancePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets)
    {
        if (orb.Owner?.Creature != base.Owner) return;

        var orbQueue = orb.Owner.PlayerCombatState?.OrbQueue;
        if (orbQueue == null) return;

        for (int i = 0; i < orbQueue.Orbs.Count; i++)
        {
            orbQueue.Orbs[i].GetOrCreateCapability<OrbBoostCapability>().BonusEvoke += 2;
        }
        Flash();
        await Task.CompletedTask;
    }
}
