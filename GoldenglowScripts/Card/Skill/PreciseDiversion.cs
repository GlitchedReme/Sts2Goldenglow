using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Goldenglow.Orb;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Combat.CardTargeting;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class PreciseDiversion() : AbstractGoldenglowCard(0, CardType.Skill, CardRarity.Basic, CustomTargetType.Anyone)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GoldenglowOrbCmd.ChannelBuoy(cardPlay.Target!, 1);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromOrb<BuoyOrb>()];

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
