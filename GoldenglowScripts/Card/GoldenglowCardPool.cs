using Godot;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;
namespace Goldenglow.Card;

public class GoldenglowCardPool : TypeListCardPoolModel
{
    public override string Title => "goldenglow";
    public override string EnergyColorName => "goldenglow";

    public override string? TextEnergyIconPath => "res://Goldenglow/image/character/energy_orb_small.png";
    public override string? BigEnergyIconPath => "res://Goldenglow/image/character/energy_orb.png";

    public override Color DeckEntryCardColor => new(1f, 0.14f, 0.321f);
    public override Color EnergyOutlineColor => new(1f, 0.14f, 0.321f);

    private static readonly Material? _poolFrameMaterial = MaterialUtils.CreateUnmodulatedHsvShaderMaterial();
    public override Material? PoolFrameMaterial => _poolFrameMaterial;

    public override bool IsColorless => false;
}
