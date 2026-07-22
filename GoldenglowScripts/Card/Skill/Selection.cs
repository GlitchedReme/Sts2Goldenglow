using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Goldenglow.Core;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class Selection() : AbstractGoldenglowCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override bool IsPlayable
    {
        get
        {
            if (Owner == null) return false;
            var hand = PileType.Hand.GetPile(Owner).Cards;
            var candidates = hand.Where(c => GoldenglowUtils.IsZeroCost(c)
                && (IsUpgraded || c.Type != CardType.Power)).ToList();
            return candidates.Count > 0;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hand = PileType.Hand.GetPile(Owner).Cards;
        var candidates = hand.Where(c => GoldenglowUtils.IsZeroCost(c)
            && (IsUpgraded || c.Type != CardType.Power)).ToList();
        if (candidates.Count == 0) return;

        var selected = (await CardSelectCmd.FromHand(
            choiceContext, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1),
            c => GoldenglowUtils.IsZeroCost(c) && (IsUpgraded || c.Type != CardType.Power), this
        )).FirstOrDefault();
        if (selected == null) return;

        var clone = selected.CreateDupeCompat(Owner);
        await CardCmd.AutoPlay(choiceContext, clone, target: null);
    }

    protected override void OnUpgrade()
    {
    }
}
