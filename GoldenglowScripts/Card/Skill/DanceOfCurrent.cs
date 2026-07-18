using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using Goldenglow.Core;
using MegaCrit.Sts2.Core.HoverTips;
using Goldenglow.Patch;

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
        var attracted = await GoldenglowCmd.Attract(choiceContext, Owner, this);

        int overflow = Math.Abs(attractCount - attracted.Count);
        if (overflow > 0)
            await PlayerCmd.GainEnergy(overflow, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Goldenglow_Attract"].UpgradeValueBy(1);
    }
}
