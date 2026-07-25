using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Goldenglow.Orb;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Combat.CardTargeting;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class PreciseDiversion() : AbstractGoldenglowCard(1, CardType.Skill, CardRarity.Basic, CustomTargetType.Anyone)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        var orb = await GoldenglowOrbCmd.ChannelBuoy(Owner, target);
        if (orb is BuoyOrb { } buoy)
        {
            await GoldenglowOrbCmd.Passive(choiceContext, buoy, null);
        }
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromOrb<BuoyOrb>()];

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
