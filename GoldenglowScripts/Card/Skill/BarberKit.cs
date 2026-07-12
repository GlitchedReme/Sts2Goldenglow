using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

/// <summary>
/// Choose 1 of 3 random Exhaust cards, add to hand with cost 0 this turn.
/// </summary>
[RegisterCard(typeof(GoldenglowCardPool))]
public class BarberKit() : AbstractGoldenglowCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var pool = Owner.Character.CardPool
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.Keywords.Contains(CardKeyword.Exhaust))
            .ToList();
        if (pool.Count == 0) return;

        var rng = Owner.RunState.Rng.CombatCardGeneration;
        var options = CardFactory.GetDistinctForCombat(Owner, pool, 3, rng).ToList();

        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1);
        if (IsUpgraded)
            foreach (var card in options)
                CardCmd.Upgrade(card);
        var selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, Owner);

        if (selected == null) return;
        
        await CardPileCmd.AddGeneratedCardToCombat(selected, PileType.Hand, Owner);
    }

    protected override void OnUpgrade()
    {
        // EnergyCost.UpgradeBy(-1);
    }
}
