using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using Goldenglow.Power;
using Goldenglow.Patch;
using MegaCrit.Sts2.Core.HoverTips;
using Goldenglow.Core;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class FreshPerfume() : AbstractGoldenglowCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self), IHovertipShownInInspectOnly
{
    public IEnumerable<IHoverTip> HoverTipsShownInInspectOnly => [
        GoldenglowUtils.CreateReference("鹰角网络 (hypergryph)")
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Turns", 3),
        new CardsVar(0)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<FreshPerfumePower>(choiceContext, Owner.Creature, DynamicVars["Turns"].BaseValue, Owner.Creature, this);
        if (DynamicVars.Cards.BaseValue > 0)
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
