using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Goldenglow.Power;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class Technique() : AbstractGoldenglowCard(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hand = PileType.Hand.GetPile(Owner).Cards;
        var candidates = hand.Where(c => !c.EnergyCost.CostsX && c.EnergyCost.Canonical == 0).ToList();
        if (candidates.Count == 0) return;

        var selected = (await CardSelectCmd.FromHand(
            choiceContext, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1),
            c => !c.EnergyCost.CostsX && c.EnergyCost.Canonical == 0, this
        )).FirstOrDefault();
        if (selected == null) return;

        var clone = selected.CreateClone();
        await CardCmd.Exhaust(choiceContext, selected);

        int interval = IsUpgraded ? 1 : 2;
        await PowerCmd.Apply<TechniquePower>(choiceContext, Owner.Creature, interval, Owner.Creature, this);
        Owner.Creature.GetPower<TechniquePower>()?.StoreCard(clone);
    }

    protected override void OnUpgrade()
    {
    }
}
