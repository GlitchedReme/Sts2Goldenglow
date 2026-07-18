using Goldenglow.Card;
using Goldenglow.Orb;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Relic;

[RegisterRelic(typeof(GoldenglowRelicPool))]
[RegisterTouchOfOrobasRefinement(typeof(InsulatingScissors))]
public class InsulatingComb : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Goldenglow/images/relics/InsulatingComb.png",
        IconOutlinePath: "res://Goldenglow/images/relics/InsulatingComb.png",
        BigIconPath: "res://Goldenglow/images/relics/InsulatingComb.png"
    );

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromOrb<BuoyOrb>()];

    public override async Task BeforeCombatStart()
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return;

        var enemies = combatState.HittableEnemies;
        if (enemies.Count == 0) return;

        var target = Owner.RunState.Rng.CombatTargets.NextItem(enemies);
        if (target != null)
            await GoldenglowOrbCmd.ChannelBuoy(Owner, target, 1);
    }
}