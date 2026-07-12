using System.Reflection;
using Godot;
using Godot.Collections;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace Goldenglow.Orb;

public partial class GParticleContainers : NParticlesContainer
{
    private static readonly FieldInfo? _particlesField = AccessTools.Field(typeof(NParticlesContainer), "_particles");

    public override void _Ready()
    {
        base._Ready();
        _particlesField?.SetValue(this, new Array<GpuParticles2D>(GetChildren().OfType<GpuParticles2D>()));
    }
}
