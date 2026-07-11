using Godot;
using Goldenglow.Utils;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.TestSupport;

namespace Goldenglow.Vfx;

public partial class BuoyLightning : Node2D
{
    public const string ScenePath = "res://Goldenglow/scene/vfx/buoy_lightning.tscn";

    public override void _Ready()
    {
        this.PlayAllParticles();
    }

    public static BuoyLightning? Create(Node2D source, NCreature target)
    {
        if (TestMode.IsOn)
            return null;

        var vfx = PreloadManager.Cache.GetScene(ScenePath).Instantiate<BuoyLightning>();
        vfx.Rotation = (target.GlobalPosition - source.GlobalPosition).Angle();
        vfx.Scale = Vector2.One * (target.GlobalPosition - source.GlobalPosition).Length() / 500f;
        return vfx;
    }

}
