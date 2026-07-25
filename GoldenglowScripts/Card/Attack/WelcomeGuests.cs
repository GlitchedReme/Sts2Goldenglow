using Goldenglow.Core;
using Goldenglow.Patch;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class WelcomeGuests() : AbstractGoldenglowCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies), IHovertipShownInInspectOnly
{
    public IEnumerable<IHoverTip> HoverTipsShownInInspectOnly => [
        GoldenglowUtils.CreateReference("dogdogbhh (白花花)")
    ];

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
                var cost = selected.EnergyCost.CostsX ? Owner.GetEnergy() : selected.EnergyCost.GetAmountToSpend();
                await CardCmd.Exhaust(choiceContext, selected);
                await DamageCmd.Attack(cost * DynamicVars["Multiplier"].BaseValue)
                    .FromCardCompat(this, cardPlay)
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
