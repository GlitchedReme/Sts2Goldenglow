using MegaCrit.Sts2.Core.Commands;
using Goldenglow.Orb;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Combat.CardTargeting;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class AlertTactics() : AbstractGoldenglowCard(0, CardType.Skill, CardRarity.Common, CustomTargetType.Anyone)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Buoy", 2),
        new CardsVar(0)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromOrb<BuoyOrb>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int count = (int)DynamicVars["Buoy"].BaseValue;
        await GoldenglowOrbCmd.ChannelBuoy(Owner, cardPlay.Target!, count);
        if (DynamicVars.Cards.BaseValue > 0)
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
