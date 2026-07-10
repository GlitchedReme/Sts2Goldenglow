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
/// Attracts X cards. Gain [E] for each card that overflows past your hand limit.
/// </summary>
[RegisterCard(typeof(GoldenglowCardPool))]
public class DanceOfCurrent() : AbstractGoldenglowCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(1),
        GoldenglowUtils.CreateAttractVar(4)
    ];


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var beforeHand = PileType.Hand.GetPile(Owner).Cards.Count;
        int attractCount = (int)DynamicVars["Goldenglow_Attract"].BaseValue;
        var attracted = await GoldenglowCmd.Attract(choiceContext, Owner, this);

        int overflow = Math.Max(0, beforeHand + attractCount - CardPile.MaxCardsInHand);
        if (overflow > 0)
            await PlayerCmd.GainEnergy(overflow, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Goldenglow_Attract"].UpgradeValueBy(1);
    }
}
