using Godot;
using Goldenglow.Core;
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

    public static Node2D Create(string? skin)
    {
        Node2D vfx = PreloadManager.Cache.GetScene(ScenePath).Instantiate<BuoyAttackVfx>();

        if (skin != null && skin != "default")
        {
            vfx = PreloadManager.Cache.GetScene(BuoySkinAttackVfx.ScenePath).Instantiate<BuoySkinAttackVfx>();
            var skinPath = SkinResources.GetBuoyAttackPath(skin);
            if (!string.IsNullOrEmpty(skinPath) && vfx is BuoySkinAttackVfx bvfx)
                bvfx.ParticleTexture = PreloadManager.Cache.GetTexture2D(skinPath);
        }

        return vfx;
    }

}
