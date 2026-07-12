using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class BarberProblem() : AbstractGoldenglowCard(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{

    protected override bool IsPlayable
    {
        get
        {
            if (Owner == null) return false;
            return !PileContainsExhaust(PileType.Hand.GetPile(Owner).Cards)
                && !PileContainsExhaust(PileType.Draw.GetPile(Owner).Cards)
                && !PileContainsExhaust(PileType.Discard.GetPile(Owner).Cards);
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Multiplier", 6),
        ModCardVars.ComputedDamage("Damage", 0, ExhaustCount, ValueProp.Move)
    ];

    private decimal ExhaustCount(CardModel? card, Creature? _)
    {
        if (card?.Owner?.PlayerCombatState == null) return 0;
        return PileType.Exhaust.GetPile(card.Owner).Cards.Count * card.DynamicVars["Multiplier"].BaseValue;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.ComputeDynamicValue("Damage"))
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Multiplier"].UpgradeValueBy(1);
    }

    private static bool PileContainsExhaust(IReadOnlyList<CardModel> cards)
    {
        for (int i = 0; i < cards.Count; i++)
            if (cards[i].Keywords.Contains(CardKeyword.Exhaust))
                return true;
        return false;
    }
}
