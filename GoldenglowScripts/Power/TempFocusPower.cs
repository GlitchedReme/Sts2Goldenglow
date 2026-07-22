using Goldenglow.Card;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Power;

public abstract class TempFocusPower<T> : ModTemporaryAppliedPowerTemplate<T, FocusPower>
    where T : AbstractModel
{
    public override PowerType Type => PowerType.Debuff;
    public override LocString Title => new("powers", "GOLDENGLOW_POWER_TEMP_FOCUS_POWER.title");
    public override LocString Description => new("powers", "GOLDENGLOW_POWER_TEMP_FOCUS_POWER.description");
}

[RegisterPower]
public sealed class PermanentMagnetTempPower : TempFocusPower<PermanentMagnet>
{
}
