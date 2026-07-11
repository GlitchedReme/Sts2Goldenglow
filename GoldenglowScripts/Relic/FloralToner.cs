using Goldenglow.Card;
using Goldenglow.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Relic;

[RegisterRelic(typeof(GoldenglowRelicPool))]
public class FloralToner : ModRelicTemplate, IOnCardAttracted
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Goldenglow/images/relics/FloralToner.png",
        IconOutlinePath: "res://Goldenglow/images/relics/FloralToner.png",
        BigIconPath: "res://Goldenglow/images/relics/FloralToner.png"
    );

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [GoldenglowUtils.Attract];

    public async Task OnCardAttracted(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;

        var enemies = Owner.Creature.CombatState?.HittableEnemies;
        if (enemies == null || enemies.Count == 0) return;

        var target = Owner.RunState.Rng.CombatTargets.NextItem(enemies);
        if (target != null)
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), target, 1, ValueProp.Unpowered, Owner.Creature);
    }
}