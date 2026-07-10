using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using Goldenglow.Core;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class CrystalClearSparkle() : AbstractGoldenglowCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.RandomEnemy)
{

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.ComputedDamage("Damage", 2,
            card => 2 + GoldenglowCmd.GetStaticStacks(card!),
            ValueProp.Move),
        ModCardVars.Computed("Repeat", 2,
            card => 2 + GoldenglowCmd.GetStaticStacks(card!))
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [GoldenglowUtils.Static];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enemies = CombatState!.HittableEnemies.ToList();
        if (enemies.Count == 0) return;

        var rng = Owner.RunState.Rng.CombatTargets;
        int hits = (int)((ComputedDynamicVar)DynamicVars["Repeat"]).Calculate();

        for (int i = 0; i < hits; i++)
        {
            var target = rng.NextItem(enemies);
            if (target != null)
                await DamageCmd.Attack(((ComputedDynamicVar)DynamicVars["Damage"]).Calculate())
                    .FromCard(this, cardPlay)
                    .Targeting(target)
                    .Execute(choiceContext);
        }
        GoldenglowCmd.ApplyStatic(cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
