using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class WelcomeGuests : AbstractGoldenglowCard
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AllEnemies;

    public WelcomeGuests()
        : base(energyCost, type, rarity, targetType)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Multiplier", 7)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var handCards = PileType.Hand.GetPile(Owner).Cards.ToList();
        if (handCards.Count > 0)
        {
            var selected = (await CardSelectCmd.FromHand(
                choiceContext, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1), _ => true, this
            )).FirstOrDefault();
            if (selected != null)
            {
                var cost = selected.EnergyCost.CostsX ? Owner.GetEnergy() : selected.EnergyCost.Canonical;
                await CardCmd.Exhaust(choiceContext, selected);
                await DamageCmd.Attack(cost * DynamicVars["Multiplier"].BaseValue)
                    .FromCard(this, cardPlay)
                    .TargetingAllOpponents(CombatState!)
                    .Execute(choiceContext);
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
