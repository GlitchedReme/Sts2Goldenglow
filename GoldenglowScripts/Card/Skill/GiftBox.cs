using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using Goldenglow.Core;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Commands;
using Goldenglow.Patch;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class GiftBox() : AbstractGoldenglowCard(-2, CardType.Skill, CardRarity.Uncommon, TargetType.Self), ICardOnAttracted, IHovertipShownInInspectOnly
{
    public IEnumerable<IHoverTip> HoverTipsShownInInspectOnly => [
        GoldenglowUtils.CreateReference("Watersnake (水蛇)")
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(2)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [GoldenglowUtils.Attract];

    public async Task OnAttracted(PlayerChoiceContext choiceContext, Player player)
    {
        for (int i = 0; i < DynamicVars.Cards.BaseValue; i++)
            await CardPileCmd.Draw(choiceContext, player);
    }


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
    }

    protected override void OnUpgrade()
    {
        // AddKeyword(CardKeyword.Retain);
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
