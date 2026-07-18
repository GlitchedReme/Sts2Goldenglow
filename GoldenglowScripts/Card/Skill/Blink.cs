using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using Goldenglow.Power;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class Blink() : AbstractGoldenglowCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var handCards = PileType.Hand.GetPile(Owner).Cards.ToList();
        var count = handCards.Count;
        foreach (var card in handCards)
            await CardCmd.Exhaust(choiceContext, card);
        await CardPileCmd.Draw(choiceContext, count, Owner);
        await PowerCmd.Apply<BlinkPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        Owner.Creature.GetPower<BlinkPower>()?.StoreExiled(handCards);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
