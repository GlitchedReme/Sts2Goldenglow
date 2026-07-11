using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class LimitingComb() : AbstractGoldenglowCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var drawn = new List<CardModel>();
        var foundCosts = new HashSet<int>();

        while (true)
        {
            var card = await GoldenglowCmd.DrawFiltered(choiceContext, Owner, c =>
            {
                int cost = c.EnergyCost.GetAmountToSpend();
                if (c.EnergyCost.CostsX) cost = -1;
                if (foundCosts.Add(cost) && !drawn.Contains(c))
                {
                    drawn.Add(c);
                    return true;
                }
                return false;
            });

            if (card == null) break;
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
