using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using Goldenglow.Core;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Entities.Players;
using System.Formats.Asn1;
using MegaCrit.Sts2.Core.Commands;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class Beacon() : AbstractGoldenglowCard(-2, CardType.Skill, CardRarity.Uncommon, TargetType.Self), ICardOnAttracted
{

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(8, ValueProp.Move)
    ];

    public override bool GainsBlock => true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [GoldenglowUtils.Attract];
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2);
    }

    public async Task OnAttracted(PlayerChoiceContext choiceContext, Player player)
    {
        await CreatureCmd.GainBlock(player.Creature, DynamicVars.Block, null);
    }

}
