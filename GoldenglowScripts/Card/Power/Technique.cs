using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Goldenglow.Core;
using Goldenglow.Power;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.HoverTips;
using Goldenglow.Patch;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class Technique() : AbstractGoldenglowCard(2, CardType.Power, CardRarity.Rare, TargetType.Self), IHovertipShownInInspectOnly
{
    public IEnumerable<IHoverTip> HoverTipsShownInInspectOnly => [
        GoldenglowUtils.CreateReference("Watersnake (水蛇)")
    ];

    protected override bool IsPlayable
    {
        get
        {
            if (Owner == null) return false;
            var hand = PileType.Hand.GetPile(Owner).Cards;
            var candidates = hand.Where(GoldenglowUtils.IsZeroCost).ToList();
            return candidates.Count > 0;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hand = PileType.Hand.GetPile(Owner).Cards;
        var candidates = hand.Where(GoldenglowUtils.IsZeroCost).ToList();
        if (candidates.Count == 0) return;

        var selected = (await CardSelectCmd.FromHand(
            choiceContext, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1),
            GoldenglowUtils.IsZeroCost, this
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
