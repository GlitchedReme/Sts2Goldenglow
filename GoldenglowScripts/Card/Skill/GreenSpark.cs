using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class GreenSpark() : AbstractGoldenglowCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(3)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int count = (int)DynamicVars.Cards.BaseValue;
        for (int i = 0; i < count; i++)
        {
            var drawn = await GoldenglowCmd.DrawFiltered(choiceContext, Owner, c => c.Keywords.Contains(CardKeyword.Exhaust));
            if (drawn == null) break;
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
        // DynamicVars.Cards.UpgradeValueBy(1);
    }
}
