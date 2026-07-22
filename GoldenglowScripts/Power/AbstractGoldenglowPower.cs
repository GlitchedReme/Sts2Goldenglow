using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Power;

public abstract class AbstractGoldenglowPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Goldenglow/image/power_atlas/{GetType().Name}.tres",
        BigIconPath: $"res://Goldenglow/image/power_atlas/{GetType().Name}84.tres"
    );
}
