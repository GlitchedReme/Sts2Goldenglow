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
public class WarmHope() : AbstractGoldenglowCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self), IHovertipShownInInspectOnly
{
    public IEnumerable<IHoverTip> HoverTipsShownInInspectOnly => [
        GoldenglowUtils.CreateReference("Watersnake (水蛇)")
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(4)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var pool = Owner.Character.CardPool.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint).ToList();
        var rng = Owner.RunState.Rng.CombatCardGeneration;
        var cards = CardFactory.GetDistinctForCombat(Owner, pool, (int)DynamicVars.Cards.BaseValue, rng);
        foreach (var c in cards)
        {
            CardCmd.ApplyKeyword(c, CardKeyword.Exhaust);
            c.EnergyCost.SetThisCombat(0);
            await CardPileCmd.AddGeneratedCardToCombat(c, PileType.Discard, Owner, CardPilePosition.Top);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
