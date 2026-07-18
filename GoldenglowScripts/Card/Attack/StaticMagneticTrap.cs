using Godot;
using Goldenglow.Core;
using Goldenglow.Patch;
using Goldenglow.Vfx;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class StaticMagneticTrap() : AbstractGoldenglowCard(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy), IHovertipShownInInspectOnly
{
    public IEnumerable<IHoverTip> HoverTipsShownInInspectOnly => [
        GoldenglowUtils.CreateReference("Watersnake (水蛇)")
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        GoldenglowUtils.CreateAttractVar(99),
        new DamageVar(12, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target is { } target)
        {
            var vfx = NSweepingBeamVfx.Create();
            var pos = target.GetCreatureNode()?.VfxSpawnPosition + Vector2.Right.Rotated(Random.Shared.NextSingle() * MathF.PI * 2) * (MathF.Sqrt(Random.Shared.NextSingle()) * 80f);
            if (vfx != null)
                GoldenglowUtils.PlayVfx(target, vfx, pos);
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        await GoldenglowCmd.Attract(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8);
    }
}
