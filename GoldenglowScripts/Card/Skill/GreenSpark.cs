using Goldenglow.Core;
using Goldenglow.Patch;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class GreenSpark() : AbstractGoldenglowCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self), IHovertipShownInInspectOnly
{
    public IEnumerable<IHoverTip> HoverTipsShownInInspectOnly => [
        GoldenglowUtils.CreateReference("鹰角网络 (hypergryph)")
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int count = (int)DynamicVars.Cards.BaseValue;
        int drawn = 0;
        for (int i = 0; i < count; i++)
        {
            var card = await GoldenglowCmd.DrawFiltered(choiceContext, Owner, c => c.Keywords.Contains(CardKeyword.Exhaust));
            if (card == null) break;
            drawn++;
        }

        if (drawn < count)
        {
            var shortfall = count - drawn;
            var pool = Owner.Character.CardPool
                .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
                .Where(c => c.Keywords.Contains(CardKeyword.Exhaust) && c is not GreenSpark)
                .ToList();
            if (pool.Count == 0) return;

            var rng = Owner.RunState.Rng.CombatCardGeneration;
            var picks = CardFactory.GetDistinctForCombat(Owner, pool, shortfall, rng).ToList();

            foreach (var card in picks)
                await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
