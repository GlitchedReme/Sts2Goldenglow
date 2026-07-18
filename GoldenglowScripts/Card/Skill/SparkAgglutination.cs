using Goldenglow.Capabilities;
using Goldenglow.Core;
using Goldenglow.Orb;
using Goldenglow.Patch;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class SparkAgglutination() : AbstractGoldenglowCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self), IHovertipShownInInspectOnly
{
    public IEnumerable<IHoverTip> HoverTipsShownInInspectOnly => [
        GoldenglowUtils.CreateReference("Watersnake (水蛇)")
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Buff", 1)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromOrb<BuoyOrb>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await OrbCmd.Channel<BuoyOrb>(choiceContext, Owner);

        var queue = Owner.PlayerCombatState?.OrbQueue;
        if (queue == null) return;
        int buff = (int)DynamicVars["Buff"].BaseValue;
        foreach (var orb in queue.Orbs)
        {
            var cap = ModelCapabilityRegistry.Create<OrbBoostCapability>();
            cap.DynamicVars["Amount"].BaseValue = buff;
            orb.AddCapability(cap);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Buff"].UpgradeValueBy(1);
    }
}
