using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class TracingTechnique() : AbstractGoldenglowCard(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(1),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var handCards = PileType.Hand.GetPile(Owner).Cards.ToList();
        var count = handCards.Count;
        foreach (var c in handCards)
            await CardCmd.Discard(choiceContext, c);
        await CardPileCmd.Draw(choiceContext, count, Owner);
        await PlayerCmd.GainEnergy(count, Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
