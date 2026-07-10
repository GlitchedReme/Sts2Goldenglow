using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Card;

/// <summary>
/// Deals {Damage} damage, then plays a random 0-cost card from the draw pile.
/// </summary>
[RegisterCard(typeof(GoldenglowCardPool))]
public class ChainShock() : AbstractGoldenglowCard(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(4, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        var drawPileCards = PileType.Draw.GetPile(Owner).Cards;
        var zeroCostCards = new List<CardModel>();
        for (int i = 0; i < drawPileCards.Count; i++)
        {
            var c = drawPileCards[i];
            if (!c.EnergyCost.CostsX && c.EnergyCost.Canonical == 0)
                zeroCostCards.Add(c);
        }

        if (zeroCostCards.Count > 0)
        {
            var card = Owner.RunState.Rng.CombatCardGeneration.NextItem(zeroCostCards);
            if (card != null)
                await CardCmd.AutoPlay(choiceContext, card, target: null);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}
