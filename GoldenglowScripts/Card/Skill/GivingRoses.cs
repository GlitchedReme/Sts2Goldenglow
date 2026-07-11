using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class GivingRoses() : AbstractGoldenglowCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    
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
        var selected = await CardSelectCmd.FromSimpleGrid(choiceContext, options, Owner, prefs);

        foreach (var card in selected)
        {
            foreach (var player in CombatState!.Players)
            {
                var clone = card.CreateClone();
                await CardPileCmd.AddGeneratedCardToCombat(clone, PileType.Hand, player);
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
