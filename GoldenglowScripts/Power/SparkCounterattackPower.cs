using Goldenglow.Card;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Power;

/// <summary>
/// Whenever the owner takes attack damage, channel BuoyOrbs to the dealer.
/// Amount = BuoyOrbs to channel per hit (stacks).
/// </summary>
[RegisterPower]
public sealed class SparkCounterattackPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner) return;
        if (dealer == null || dealer.IsPlayer) return;
        if (result.BlockedDamage + result.UnblockedDamage <= 0) return;

        for (int i = 0; i < Amount; i++)
            await GoldenglowOrbCmd.ChannelBuoy(target.Player!, dealer, 1);
        await Task.CompletedTask;
    }
}
