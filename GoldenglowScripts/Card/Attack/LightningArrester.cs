using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class LightningArrester() : AbstractGoldenglowCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target!).Execute(choiceContext);
        var drawPile = PileType.Draw.GetPile(Owner).Cards.ToList();
        bool foundPower = false;
        foreach (var card in drawPile)
        {
            await CardCmd.Discard(choiceContext, card);
            if (card.Type == CardType.Power)
            {
                await CardPileCmd.Add(card, PileType.Hand);
                foundPower = true;
                break;
            }
        }
        if (!foundPower)
        {
            await CardPileCmd.Shuffle(choiceContext, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}
