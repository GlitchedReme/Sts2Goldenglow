using System.Reflection;
using HarmonyLib;

namespace Goldenglow.Orb;

#if STS2_AT_LEAST_109_0
public partial class NBuoyVfx : NOrbVfx
{
    private static readonly FieldInfo? FocusedParticlesField = AccessTools.Field(typeof(NOrbVfx), "_focusedParticles");
    private static readonly FieldInfo? PassiveActivatedParticlesField = AccessTools.Field(typeof(NOrbVfx), "_passiveActivatedParticles");


    public override void _Ready()
    {
        base._Ready();
        FocusedParticlesField?.SetValue(this, GetNode<GParticleContainers>("FocusedParticles"));
        PassiveActivatedParticlesField?.SetValue(this, GetNode<GParticleContainers>("PassiveActivatedParticles"));
    }
}
#endif
