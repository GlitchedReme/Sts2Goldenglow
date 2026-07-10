using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class Selection() : AbstractGoldenglowCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hand = PileType.Hand.GetPile(Owner).Cards;
        var candidates = hand.Where(c => !c.EnergyCost.CostsX && c.EnergyCost.Canonical == 0
            && (IsUpgraded || c.Type != CardType.Power)).ToList();
        if (candidates.Count == 0) return;

        var selected = (await CardSelectCmd.FromHand(
            choiceContext, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1),
            c => !c.EnergyCost.CostsX && c.EnergyCost.Canonical == 0
                && (IsUpgraded || c.Type != CardType.Power), this
        )).FirstOrDefault();
        if (selected == null) return;

        var clone = selected.CreateClone();
        await CardCmd.AutoPlay(choiceContext, clone, target: null);
    }

    protected override void OnUpgrade()
    {
    }
}
