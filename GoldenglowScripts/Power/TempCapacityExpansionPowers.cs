using Goldenglow.Card;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Power;

public abstract class TempHandSizePower<T> : ModTemporaryAppliedPowerTemplate<T, CapacityExpansionPower>
    where T : AbstractModel
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Goldenglow/image/power_atlas/{GetType().Name}.tres",
        BigIconPath: $"res://Goldenglow/image/power_atlas/{GetType().Name}84.tres"
    );
    public override PowerType Type => PowerType.Debuff;
    public override LocString Title => new("powers", "GOLDENGLOW_POWER_TEMP_HAND_SIZE.title");
    public override LocString Description => new("powers", "GOLDENGLOW_POWER_TEMP_HAND_SIZE.description");
}

[RegisterPower]
public sealed class CapacitorTempPower : TempHandSizePower<Capacitor>
{
}
