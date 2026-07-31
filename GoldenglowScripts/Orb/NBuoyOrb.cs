using Goldenglow.Patch;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using Goldenglow.Vfx;
using Goldenglow.Core;
using MegaCrit.Sts2.Core.Assets;

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

        RefreshSkin();
#if STS2_AT_LEAST_110_0
        Orb.Model.EvokeActivated += OnEvokeActivated;
#else
        if (Orb.Model is BuoyOrb buoyOrb)
            buoyOrb.GgEvokeActivated += OnEvokeActivated;
#endif
    }

    public override void _ExitTree()
    {
#if STS2_AT_LEAST_110_0
        if (Orb?.Model != null)
            Orb.Model.EvokeActivated -= OnEvokeActivated;
#else
        if (Orb?.Model is BuoyOrb buoyOrb)
            buoyOrb.GgEvokeActivated -= OnEvokeActivated;
#endif
    }

    private void OnEvokeActivated(Creature[] e)
    {
        if (!IsInstanceValid(this)) return;
        foreach (var target in e)
        {
            var lightning = BuoyLightning.Create(this, target.GetCreatureNode()!);
            if (lightning != null)
                GoldenglowUtils.PlayVfx(target, lightning, GlobalPosition);
            if (Orb?.Model != null)
            {
                var source = (Orb.Model as BuoyOrb)?.Source;
                var skin = SkinResources.GetSkinKey(source);
                GoldenglowUtils.PlayVfx(target, BuoyAttackVfx.Create(skin));
            }
        }
    }

    public void RefreshSkin()
    {
        if (Orb?.Model == null) return;
        var source = (Orb.Model as BuoyOrb)?.Source;
        var skin = SkinResources.GetSkinKey(source);
        Texture = PreloadManager.Cache.GetTexture2D(SkinResources.GetBuoySkinPath(skin));
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
