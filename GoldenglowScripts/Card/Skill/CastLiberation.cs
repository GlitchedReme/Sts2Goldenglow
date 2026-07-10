using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using Goldenglow.Core;
using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Card;

/// <summary>
/// Attracts X cards from the discard pile, then plays them. Exhaust.
/// </summary>
[RegisterCard(typeof(GoldenglowCardPool))]
public class CastLiberation() : AbstractGoldenglowCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        GoldenglowUtils.CreateAttractVar(3)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var attracted = await GoldenglowCmd.Attract(choiceContext, Owner, this);
        foreach (var card in attracted)
            await CardCmd.AutoPlay(choiceContext, card, target: null);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Goldenglow_Attract"].UpgradeValueBy(1);
    }
}
