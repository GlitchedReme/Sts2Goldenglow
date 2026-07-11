using Goldenglow.Orb;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class SupportTactics() : AbstractGoldenglowCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Buoy", 2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromOrb<BuoyOrb>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int count = (int)DynamicVars["Buoy"].BaseValue;
        foreach (var player in CombatState!.Players)
            await GoldenglowOrbCmd.ChannelBuoy(player.Creature, count);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Buoy"].UpgradeValueBy(1);
    }
}
