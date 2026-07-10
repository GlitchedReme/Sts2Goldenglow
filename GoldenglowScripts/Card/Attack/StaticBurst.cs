using Goldenglow.Core;
using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class StaticBurst() : AbstractGoldenglowCard(-1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        GoldenglowUtils.CreatePulseVar(),
        new DynamicVar("Extra", 2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var xValue = ResolveEnergyXValue();
        for (int i = 0; i < xValue + DynamicVars["Extra"].BaseValue; i++)
            await GoldenglowCmd.Pulse(Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Extra"].UpgradeValueBy(1);
    }
}
