using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Goldenglow.Core;
using Goldenglow.Vfx;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class CrystalClearSparkle() : AbstractGoldenglowCard(0, CardType.Attack, CardRarity.Uncommon, TargetType.RandomEnemy)
{
    private const float StaggerInterval = 0.1f;

    protected override HashSet<CardTag> CanonicalTags => [GoldenglowTags.Static];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.ComputedDamage("Damage", 2,
            card => 2 + GoldenglowCmd.GetStaticStacks(card),
            ValueProp.Move),
        ModCardVars.Computed("Repeat", 2,
            card => 2 + GoldenglowCmd.GetStaticStacks(card))
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [GoldenglowUtils.Static];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enemies = CombatState!.HittableEnemies.ToList();
        if (enemies.Count == 0) return;

        var rng = Owner.RunState.Rng.CombatTargets;
        var hits = (int)DynamicVars.ComputeDynamicValue("Repeat");
        var damage = DynamicVars.ComputeDynamicValue("Damage");

        await using var attackContext = await GoldenglowCompat.CreateAttackContextAsync(CombatState!, choiceContext, cardPlay);

        var delay = 0f;
        var tasks = new List<Task>();
        for (var i = 0; i < hits; i++)
        {
            var target = rng.NextItem(enemies);
            if (target == null) continue;

            var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
            var pos = (targetNode?.VfxSpawnPosition ?? Vector2.Zero)
                + Vector2.Right.Rotated(MathF.PI + Random.Shared.NextSingle() * MathF.PI) * (200f + Random.Shared.NextSingle() * 200f);

            var vfx = BuoyCardAttackVfx.Create(pos, Owner, target, async () =>
            {
                if (target == null) return;
                await attackContext.DamageWithHit(choiceContext, target, damage, ValueProp.Move, Owner.Creature, this, cardPlay);
            }, delay);

            if (vfx != null)
            {
                NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
                tasks.Add(vfx.CompletionTask!);
            }

            delay += StaggerInterval;
        }
        await GoldenglowCmd.ApplyStatic(cardPlay.Card);

        await Task.WhenAll(tasks);
    }

    protected override void OnUpgrade()
    {
        // EnergyCost.UpgradeBy(-1);
        DynamicVars["Damage"].UpgradeValueBy(1);
        DynamicVars["Repeat"].UpgradeValueBy(1);
    }
}
