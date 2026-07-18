using Goldenglow.Capabilities;
using Goldenglow.Card;
using Goldenglow.Orb;
using Goldenglow.Ui;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Relic;

[RegisterRelic(typeof(GoldenglowRelicPool))]
public class InsulatingScissors : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Goldenglow/images/relics/InsulatingScissors.png",
        IconOutlinePath: "res://Goldenglow/images/relics/InsulatingScissors.png",
        BigIconPath: "res://Goldenglow/images/relics/InsulatingScissors.png"
    );

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromOrb<BuoyOrb>()];

    public override async Task BeforeCombatStart()
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return;

        await ChannelBuoyToPlayer();
        foreach (var enemy in combatState.Enemies)
        {
            if (enemy.IsDead) continue;
            await ChannelBuoyToEnemy(enemy);
        }
    }

    private async Task ChannelBuoyToPlayer()
    {
        var queue = Owner.Creature.Player!.PlayerCombatState!.OrbQueue;
        int prevCount = queue.Orbs.Count;
        await GoldenglowOrbCmd.ChannelBuoy(Owner, Owner.Creature, 1);
        if (queue.Orbs.Count > prevCount)
            queue.Orbs[^1].GetOrCreateCapability<OrbBoostCapability>();
    }

    private async Task ChannelBuoyToEnemy(Creature enemy)
    {
        var mgr = MonsterOrbManager.MonsterOrbManagerState[enemy];
        int prevCount = mgr?.GetOrbs().Count ?? 0;
        await GoldenglowOrbCmd.ChannelBuoy(Owner, enemy, 1);
        if (mgr != null)
        {
            var orbs = mgr.GetOrbs();
            if (orbs.Count > prevCount)
                orbs[^1].GetOrCreateCapability<OrbBoostCapability>();
        }
    }
}