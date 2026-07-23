using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Power;

[RegisterPower]
public sealed class ExcitationPower : AbstractGoldenglowPower
{
    public override async Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets)
    {
        if (orb.Owner?.Creature != Owner) return;
        
        Flash();
        var enemies = CombatState.HittableEnemies;
        for (int i = 0; i < enemies.Count; i++)
        {
            await CreatureCmd.Damage(choiceContext, enemies[i], Amount,
                ValueProp.Unpowered, enemies[i]);
        }
    }
}
