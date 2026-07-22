using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Combat.HandSize;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Power;

[RegisterPower]
public sealed class CapacityExpansionPower : AbstractGoldenglowPower, IMaxHandSizeModifier
{
    public int ModifyMaxHandSize(Player player, int currentMaxHandSize)
    {
        if (player.Creature != Owner)
            return currentMaxHandSize;
        return currentMaxHandSize + Amount;
    }
}
