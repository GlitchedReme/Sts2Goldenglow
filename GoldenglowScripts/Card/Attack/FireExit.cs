using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class FireExit() : AbstractGoldenglowCard(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Multipler", 3),
        ModCardVars.ComputedDamage("Damage", 0, CalculateDamage, ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.ComputeDynamicValue("Damage"))
            .FromCardCompat(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Multipler"].UpgradeValueBy(1);
    }

    public decimal CalculateDamage(CardModel? card)
    {
        if (card?.Owner == null || card.Owner.PlayerCombatState == null)
            return 0;
        else
        {
            if (CardPile.Get(PileType.Hand, card.Owner) == null)
                return 0;
            else
            {
                var amount = PileType.Hand.GetPile(card.Owner).Cards.Count;
                if (PileType.Hand.GetPile(card.Owner).Cards.Contains(card))
                    amount -= 1;
                return amount * DynamicVars["Multipler"].IntValue;
            }
        }
    }
}
