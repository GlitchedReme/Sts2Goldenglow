using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;

namespace Goldenglow.Capabilities;

[RegisterModelCapability]
public sealed class OrbBoostCapability : OrbCapability
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Amount", 1)
    ];

    public override decimal ModifyOrbValue(OrbModel orb, decimal value)
    {
        if (Owner != orb)
            return value;
        return Math.Max(value + DynamicVars["Amount"].BaseValue, 0m);
    }
}
