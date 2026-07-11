using Goldenglow.Card;
using Goldenglow.Orb;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Relic;

[RegisterRelic(typeof(GoldenglowRelicPool))]
public class HairCape : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Goldenglow/images/relics/HairCape.png",
        IconOutlinePath: "res://Goldenglow/images/relics/HairCape.png",
        BigIconPath: "res://Goldenglow/images/relics/HairCape.png"
    );

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromOrb<BuoyOrb>()];

    public override async Task BeforeCombatStart()
    {
        await GoldenglowOrbCmd.ChannelBuoy(Owner.Creature, 1);
    }
}