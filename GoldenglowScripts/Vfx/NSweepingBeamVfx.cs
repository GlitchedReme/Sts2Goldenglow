using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.TestSupport;

namespace Goldenglow.Vfx;

public partial class NSweepingBeamVfx : Node2D
{
    public const string scenePath = "res://Goldenglow/scene/vfx/pulse_vfx.tscn";

    private IReadOnlyList<GpuParticles2D> _emittingParticles = [];
    private IReadOnlyList<GpuParticles2D> _startParticles = [];
    private IReadOnlyList<GpuParticles2D> _endParticles = [];

    private CancellationTokenSource? _cts;

    public override void _ExitTree() => _cts?.Cancel();

    public static NSweepingBeamVfx? Create()
    {
        if (TestMode.IsOn) return null;
        var vfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<NSweepingBeamVfx>(PackedScene.GenEditState.Disabled);
        return vfx;
    }

    public override void _Ready()
    {
        _emittingParticles =
        [
            GetNode<GpuParticles2D>("emitting/vfx_hyperbeam_core"),
            GetNode<GpuParticles2D>("emitting/vfx_common_hit_flare_transparent"),
            GetNode<GpuParticles2D>("emitting/vfx_common_hit_flare"),
            GetNode<GpuParticles2D>("emitting/vfx_common_glow"),
        ];
        _startParticles =
        [
            GetNode<GpuParticles2D>("start/vfx_common_ring_polar_b_1"),
        ];
        _endParticles =
        [
            GetNode<GpuParticles2D>("end/vfx_common_ring_polar_b_2"),
            GetNode<GpuParticles2D>("end/vfx_common_specks"),
            GetNode<GpuParticles2D>("end/vfx_lightning1"),
            GetNode<GpuParticles2D>("end/vfx_lightning2"),
        ];

        foreach (var p in _emittingParticles) p.Emitting = false;
        TaskHelper.RunSafely(PlaySequence());
    }

    private async Task PlaySequence()
    {
        _cts = new CancellationTokenSource();

        foreach (var p in _startParticles) p.Restart();
        foreach (var p in _emittingParticles) { p.Restart(); p.Emitting = true; }
        
        await Cmd.Wait(0.2f);

        foreach (var p in _endParticles) p.Restart();
        foreach (var p in _emittingParticles) p.Emitting = false;

        await Cmd.Wait(2f, _cts.Token);
        this.QueueFreeSafely();
    }
}
