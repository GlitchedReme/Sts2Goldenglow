using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;

namespace Goldenglow.Capabilities;

[RegisterModelCapability]
public sealed class BuffDamageOrBlockCapability : CardCapability
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Buff", 0)
    ];

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (cardSource != Owner) return 0m;
        return DynamicVars["Buff"].BaseValue;
    }

    public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (cardSource != Owner) return 0m;
        return DynamicVars["Buff"].BaseValue;
    }
}
