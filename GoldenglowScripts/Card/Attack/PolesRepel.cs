using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

/// <summary>
/// Deals {Damage} damage to ALL enemies once for each enemy.
/// </summary>
[RegisterCard(typeof(GoldenglowCardPool))]
public class PolesRepel() : AbstractGoldenglowCard(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
{

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enemyCount = CombatState!.HittableEnemies.Count;
        for (int i = 0; i < enemyCount; i++)
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .TargetingAllOpponents(CombatState!)
                .WithHitFx("vfx/vfx_attack_lightning")
                .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}
