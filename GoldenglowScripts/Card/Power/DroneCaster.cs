using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Goldenglow.Orb;
using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

using Goldenglow.Power;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class DroneCaster() : AbstractGoldenglowCard(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("OrbBuff", 1)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromOrb<BuoyOrb>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<DroneCasterPower>(choiceContext, Owner.Creature, DynamicVars["OrbBuff"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["OrbBuff"].UpgradeValueBy(1);
    }
}
