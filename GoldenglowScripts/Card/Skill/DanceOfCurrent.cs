using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using Goldenglow.Core;
using MegaCrit.Sts2.Core.HoverTips;
using Goldenglow.Patch;
using STS2RitsuLib;
using Godot;

namespace Goldenglow.Card;

/// <summary>
/// Attracts X cards. Gain [E] for each card that overflows past your hand limit.
/// </summary>
[RegisterCard(typeof(GoldenglowCardPool))]
public class DanceOfCurrent() : AbstractGoldenglowCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self), IHovertipShownInInspectOnly
{
    public IEnumerable<IHoverTip> HoverTipsShownInInspectOnly => [
        GoldenglowUtils.CreateReference("Watersnake (水蛇)")
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(1),
        GoldenglowUtils.CreateAttractVar(4)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int attractCount = (int)DynamicVars["Goldenglow_Attract"].BaseValue;
        int discardCount = PileType.Discard.GetPile(Owner).Cards.Count;
        int wouldAttract = Math.Min(attractCount, discardCount);
        int handBefore = PileType.Hand.GetPile(Owner).Cards.Count;

        await GoldenglowCmd.Attract(choiceContext, Owner, this);

        int actuallyAttracted = PileType.Hand.GetPile(Owner).Cards.Count - handBefore;
        int overflow = wouldAttract - actuallyAttracted;
        if (overflow > 0)
            await PlayerCmd.GainEnergy(overflow, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Goldenglow_Attract"].UpgradeValueBy(1);
    }
}
