using Goldenglow.Card;
using Goldenglow.Orb;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Relic;

[RegisterRelic(typeof(GoldenglowRelicPool))]
public class InsulatingScissors : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Goldenglow/image/relics/InsulatingScissors.png",
        IconOutlinePath: "res://Goldenglow/image/relics/InsulatingScissors.png",
        BigIconPath: "res://Goldenglow/image/relics/InsulatingScissors.png"
    );

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromOrb<BuoyOrb>()];

    public override async Task BeforeCombatStart()
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return;

        foreach (var player in combatState.Players)
        {
            if (player.Creature.IsDead) continue;
            await GoldenglowOrbCmd.ChannelBuoy(Owner, player.Creature, 1);
        }

        foreach (var enemy in combatState.Enemies)
        {
            if (enemy.IsDead) continue;
            await GoldenglowOrbCmd.ChannelBuoy(Owner, enemy, 1);
        }
    }

    public override decimal ModifyOrbValue(OrbModel orb, decimal value)
    {
        if (orb is not BuoyOrb)
            return value;

        return Math.Max(value + 1, 0m);
    }
}
