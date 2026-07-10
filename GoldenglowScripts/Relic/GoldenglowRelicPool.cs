using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Relic;

public class GoldenglowRelicPool : TypeListRelicPoolModel
{
    public override string? TextEnergyIconPath => "res://Goldenglow/image/character/energy_orb_small.png";
    public override string? BigEnergyIconPath => "res://Goldenglow/image/character/energy_orb.png";

    public override string EnergyColorName => "goldenglow";
}
