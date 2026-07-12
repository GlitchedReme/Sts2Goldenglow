using MegaCrit.Sts2.Core.Commands;
using Goldenglow.Core;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class HalfWaveRectifier() : AbstractGoldenglowCard(0, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        GoldenglowUtils.CreatePulseVar(),
        new CardsVar(0)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        for (int i = 0; i < 2; i++)
            await GoldenglowCmd.Pulse(Owner, this, cardPlay);
        if (DynamicVars.Cards.BaseValue > 0)
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        // EnergyCost.UpgradeBy(-1);
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
