using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class WarmHope() : AbstractGoldenglowCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{

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
            await CardPileCmd.AddGeneratedCardToCombat(c, PileType.Discard, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
