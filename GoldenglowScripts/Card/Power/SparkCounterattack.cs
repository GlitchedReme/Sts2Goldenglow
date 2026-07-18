using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Goldenglow.Orb;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;

using Goldenglow.Power;
using Goldenglow.Core;
using Goldenglow.Patch;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class SparkCounterattack() : AbstractGoldenglowCard(2, CardType.Power, CardRarity.Uncommon, TargetType.Self), IHovertipShownInInspectOnly
{
    public IEnumerable<IHoverTip> HoverTipsShownInInspectOnly => [
        GoldenglowUtils.CreateReference("鹰角网络 (hypergryph)")
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Buoy", 1)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromOrb<BuoyOrb>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SparkCounterattackPower>(choiceContext, Owner.Creature, DynamicVars["Buoy"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Buoy"].UpgradeValueBy(1);
    }
}
