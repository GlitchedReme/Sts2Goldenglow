using Goldenglow.Utils;
using Goldenglow.Patch;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Orbs;

namespace Goldenglow.Orb;

public partial class NBuoyOrb : Sprite2D
{
    public NOrb Orb { get; set; } = null!;
    private double _time;

    public override void _Ready()
    {
        Orb = this.FindParent<NOrb>()!;
    }

    public override void _Process(double delta)
    {
        _time += delta;
        Offset = new Vector2(0, (float)(Math.Sin(_time * 2) * 5));

        if (Orb.Model == null) return;

        var creature = Orb.Model!.Owner?.Creature
            ?? MonsterOrbPatch.OwnerState[Orb.Model!];
        if (creature == null) return;

        var creatureNode = creature.GetCreatureNode();
        if (creatureNode != null)
        {
            var dir = creatureNode.GlobalPosition - GlobalPosition;
            Rotation = dir.Angle() - Mathf.Pi / 2;
        }
    }
}
