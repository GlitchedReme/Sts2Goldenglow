using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Combat.CardTargeting;
using Goldenglow.Orb;
using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class AidAI() : AbstractGoldenglowCard(-1, CardType.Skill, CardRarity.Uncommon, CustomTargetType.Anyone)
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Channel", 0)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromOrb<BuoyOrb>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var xValue = ResolveEnergyXValue();
        var count = xValue + (int)DynamicVars["Channel"].BaseValue;
        await GoldenglowOrbCmd.ChannelBuoy(Owner, cardPlay.Target!, count);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Channel"].UpgradeValueBy(1);
    }
}
