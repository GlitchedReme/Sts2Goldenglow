using Goldenglow.Card;
using Goldenglow.Core;
using Goldenglow.Orb;
using Goldenglow.Ui;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Relic;

[RegisterRelic(typeof(GoldenglowRelicPool))]
public class CleaningTools : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Goldenglow/image/relics/CleaningTools.png",
        IconOutlinePath: "res://Goldenglow/image/relics/CleaningTools.png",
        BigIconPath: "res://Goldenglow/image/relics/CleaningTools.png"
    );

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromOrb<BuoyOrb>(), GoldenglowUtils.Transfer];

    public override async Task BeforeDeath(Creature creature)
    {
        if (creature.IsPlayer) return;

        var mgr = MonsterOrbManager.MonsterOrbManagerState[creature];
        if (mgr == null) return;

        var orbs = mgr.GetOrbs();
        int count = orbs.Count;
        if (count == 0) return;

        await GoldenglowOrbCmd.TransferOrbs(Owner, creature, Owner.Creature, count);
    }
}