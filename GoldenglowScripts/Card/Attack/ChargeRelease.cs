using Goldenglow.Ui;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class ChargeRelease() : AbstractGoldenglowCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
{

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(7, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enemies = CombatState!.Enemies;
        var damage = DynamicVars.Damage.BaseValue;
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].IsDead || enemies[i].IsPlayer) continue;
            var mgr = MonsterOrbManager.MonsterOrbManagerState[enemies[i]];
            if (mgr == null) continue;
            int orbCount = mgr.GetOrbs().Count;
            if (orbCount > 0)
            {
                await CreatureCmd.Damage(choiceContext, enemies[i],
                    damage * orbCount, ValueProp.Unpowered, enemies[i]);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}
