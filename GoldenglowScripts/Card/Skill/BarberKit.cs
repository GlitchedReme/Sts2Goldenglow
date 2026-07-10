using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Card;

/// <summary>
/// Choose 1 of 3 random Exhaust cards, add to hand with cost 0 this turn.
/// </summary>
[RegisterCard(typeof(GoldenglowCardPool))]
public class BarberKit() : AbstractGoldenglowCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{

    protected override IEnumerable<DynamicVar> CanonicalVars => [
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var pool = Owner.Character.CardPool
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.Keywords.Contains(CardKeyword.Exhaust))
            .ToList();
        if (pool.Count == 0) return;

        var rng = Owner.RunState.Rng.CombatCardGeneration;
        var options = CardFactory.GetDistinctForCombat(Owner, pool, 3, rng).ToList();

        var prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1);
        var selected = await CardSelectCmd.FromSimpleGrid(choiceContext, options, Owner, prefs);

        foreach (var card in selected)
        {
            card.EnergyCost.SetThisCombat(0);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
