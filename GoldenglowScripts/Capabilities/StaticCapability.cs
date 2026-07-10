using System.Collections.Generic;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;

namespace Goldenglow.Capabilities;

/// <summary>
/// Tracks how many times this specific card has been played this turn.
/// Each card tracks its own static layers independently; all reset at turn start.
/// </summary>
[RegisterModelCapability]
public sealed class StaticCapability : CardCapability
{
    private static readonly HashSet<StaticCapability> _active = [];

    public int TimesPlayedThisTurn { get; private set; }

    public void Increment()
    {
        _active.Add(this);
        TimesPlayedThisTurn++;
    }

    public void Reset()
    {
        TimesPlayedThisTurn = 0;
    }

    public static void ResetAll()
    {
        foreach (var cap in _active)
            cap.Reset();
        _active.Clear();
    }
}
