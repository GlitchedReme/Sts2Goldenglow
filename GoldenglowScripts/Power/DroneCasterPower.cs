using System.Threading.Tasks;
using Goldenglow.Capabilities;
using Goldenglow.Orb;
using Goldenglow.Ui;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Power;

/// <summary>
/// Whenever a buoy orb deals damage, boost its future evoke value.
/// Amount = bonus per damage event (stacks).
/// </summary>
[RegisterPower]
public sealed class DroneCasterPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != base.Owner) return;
        if (result.UnblockedDamage + result.BlockedDamage <= 0) return;

        var enemies = CombatManager.Instance.DebugOnlyGetState()?.Enemies;
        if (enemies == null) return;

        for (int i = 0; i < enemies.Count; i++)
        {
            var mgr = MonsterOrbManager.MonsterOrbManagerState[enemies[i]];
            if (mgr == null) continue;
            var orbs = mgr.GetOrbs();
            for (int j = 0; j < orbs.Count; j++)
            {
                if (orbs[j] is BuoyOrb bo)
                    bo.GetOrCreateCapability<OrbBoostCapability>().BonusEvoke += Amount;
            }
        }
        await Task.CompletedTask;
    }
}
