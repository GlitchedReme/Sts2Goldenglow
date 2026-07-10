using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;

namespace Goldenglow.Capabilities;

/// <summary>
/// Mutable evoke/passive bonus stored on an orb model.
/// Set by RadiationLamp/DroneCaster powers; consumed during Evoke.
/// </summary>
[RegisterModelCapability]
public sealed class OrbBoostCapability : OrbCapability
{
    public int BonusEvoke { get; set; }

    /// <summary>
    /// DroneCaster: after this orb evokes (deals damage), boost future evokes by 1.
    /// </summary>
    protected override Task OnOwnerOrbEvoked(OrbEvokeContext context)
    {
        BonusEvoke += 1;
        return Task.CompletedTask;
    }
}
