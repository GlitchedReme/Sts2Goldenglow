using Goldenglow.Card;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Power;

public abstract class TempHandSizePower<T> : ModTemporaryAppliedPowerTemplate<T, CapacityExpansionPower>
    where T : AbstractModel
{
    public override LocString Title => new("powers", "GOLDENGLOW_POWER_TEMP_HAND_SIZE.title");
    public override LocString Description => new("powers", "GOLDENGLOW_POWER_TEMP_HAND_SIZE.description");
}

[RegisterPower]
public sealed class CapacitorTempPower : TempHandSizePower<Capacitor>
{
}

[RegisterPower]
public sealed class DegaussingTempPower : TempHandSizePower<Degaussing>
{
}