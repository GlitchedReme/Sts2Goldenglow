using Goldenglow.Card;
using MegaCrit.Sts2.Core.Entities.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Relic;

[RegisterRelic(typeof(GoldenglowRelicPool))]
public class InsulatedScissors : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Goldenglow/images/relics/InsulatedScissors.png",
        IconOutlinePath: "res://Goldenglow/images/relics/InsulatedScissors.png",
        BigIconPath: "res://Goldenglow/images/relics/InsulatedScissors.png"
    );

    public override async Task BeforeCombatStart()
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return;

        var enemies = combatState.HittableEnemies;
        if (enemies.Count == 0) return;

        var target = Owner.RunState.Rng.CombatTargets.NextItem(enemies);
        if (target != null)
            await GoldenglowOrbCmd.ChannelBuoy(target, 1);
    }
}
