using Godot;
using Goldenglow.Utils;
using MegaCrit.Sts2.Core.Assets;

namespace Goldenglow.Vfx;

public partial class BuoyAttackVfx : Node2D
{
    public const string ScenePath = "res://Goldenglow/scene/vfx/buoy_attack_vfx.tscn";

    public override void _Ready()
    {
        this.PlayAllParticles();
    }

    public static BuoyAttackVfx Create() => PreloadManager.Cache.GetScene(ScenePath).Instantiate<BuoyAttackVfx>();
}
