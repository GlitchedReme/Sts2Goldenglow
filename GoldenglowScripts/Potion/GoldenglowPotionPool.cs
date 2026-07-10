using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Potion;

public class GoldenglowPotionPool : TypeListPotionPoolModel
{
    public override string? TextEnergyIconPath => "res://Goldenglow/image/character/energy_orb_small.png";
    public override string? BigEnergyIconPath => "res://Goldenglow/image/character/energy_orb.png";

    public override string EnergyColorName => "goldenglow";
}
