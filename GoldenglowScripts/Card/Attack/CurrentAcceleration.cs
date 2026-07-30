using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Goldenglow.Vfx;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class CurrentAcceleration() : AbstractGoldenglowCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    private const float StaggerInterval = 0.1f;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3, ValueProp.Move),
        new RepeatVar(4)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
        var basePos = targetNode?.VfxSpawnPosition ?? Vector2.Zero;
        var hits = (int)DynamicVars.Repeat.BaseValue;

        var delay = 0f;
        var tasks = new List<Task>();
        for (int i = 0; i < hits; i++)
        {
            var pos = basePos + Vector2.Right.Rotated(MathF.PI + Random.Shared.NextSingle() * MathF.PI) * (200f + Random.Shared.NextSingle() * 200f);
            var vfx = BuoyCardAttackVfx.Create(pos, Owner, target, async () =>
            {
                await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                    .FromCardCompat(this, cardPlay)
                    .Targeting(target)
                    .Execute(choiceContext);
            }, delay);

            if (vfx != null)
            {
                NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
                tasks.Add(vfx.CompletionTask!);
            }

            delay += StaggerInterval;
        }

        await Task.WhenAll(tasks);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
        // DynamicVars.Repeat.UpgradeValueBy(1);
    }
}
