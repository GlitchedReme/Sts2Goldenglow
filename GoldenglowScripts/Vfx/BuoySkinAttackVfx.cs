using Godot;
using Goldenglow.Utils;

namespace Goldenglow.Vfx;

public partial class BuoySkinAttackVfx : Node2D
{
    public const string ScenePath = "res://Goldenglow/scene/vfx/buoy_skin_attack_vfx.tscn";

    public Texture2D? ParticleTexture;
    public GpuParticles2D Particle { get; set; } = null!;

    public override void _Ready()
    {
        Particle = GetNode<GpuParticles2D>("GPUParticles2D");
        if (ParticleTexture != null)
        {
            Particle.Texture = ParticleTexture;
        }
        this.PlayAllParticles();
    }
}
