using Goldenglow.Core;
using Goldenglow.Orb;
using Goldenglow.Patch;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class SupportTactics() : AbstractGoldenglowCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self), IHovertipShownInInspectOnly
{
    public IEnumerable<IHoverTip> HoverTipsShownInInspectOnly => [
        GoldenglowUtils.CreateReference("Namie")
    ];

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Buoy", 2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromOrb<BuoyOrb>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var count = (int)DynamicVars["Buoy"].BaseValue;
        foreach (var player in CombatState!.Players)
            for (var i = 0; i < count; i++)
                await GoldenglowOrbCmd.ChannelBuoy(Owner, player.Creature);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Buoy"].UpgradeValueBy(1);
    }
}
