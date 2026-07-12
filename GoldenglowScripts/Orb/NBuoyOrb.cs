using Goldenglow.Utils;
using Goldenglow.Patch;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using Goldenglow.Vfx;
using Goldenglow.Core;

namespace Goldenglow.Orb;

public partial class NBuoyOrb : Sprite2D
{
    public NOrb? Orb { get; set; }
    private double _time;

    public override void _Ready()
    {
        Orb = this.FindParent<NOrb>();
        if (Orb == null || Orb.Model == null)
        {
            Entry.Logger.Error("NBuoyOrb: Could not find parent NOrb");
            return;
        }

        Orb.Model.EvokeActivated += OnEvokeActivated;
    }

    public override void _ExitTree()
    {
        if (Orb?.Model != null)
            Orb.Model.EvokeActivated -= OnEvokeActivated;
    }

    private void OnEvokeActivated(Creature[] e)
    {
        if (!IsInstanceValid(this)) return;
        foreach (var target in e)
        {
            var lightning = BuoyLightning.Create(this, target.GetCreatureNode()!)!;
            GoldenglowUtils.PlayVfx(target, lightning, GlobalPosition);
            GoldenglowUtils.PlayVfx(target, BuoyAttackVfx.Create());
        }
    }

    public override void _Process(double delta)
    {
        _time += delta;
        Offset = new Vector2(0, (float)(Math.Sin(_time * 2) * 5));

        if (Orb?.Model == null) return;

        var creature = MonsterOrbPatch.OwnerState.TryGetValue(Orb.Model, out var c) && c != null ? c : Orb.Model.Owner?.Creature;
        if (creature == null) return;

        var creatureNode = creature.GetCreatureNode();
        if (creatureNode != null)
        {
            var dir = creatureNode.GlobalPosition - GlobalPosition;
            Rotation = dir.Angle() - Mathf.Pi / 2;
        }
    }
}
