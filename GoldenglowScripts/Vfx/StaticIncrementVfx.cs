using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace Goldenglow.Vfx;

public partial class StaticIncrementVfx : Control
{
    public static readonly string scenePath = "res://Goldenglow/scene/vfx/static_increment_vfx.tscn";

    [Export] private CurveXyzTexture _anticipationScaleCurve = null!;
    [Export] private CurveXyzTexture _revealScaleCurve = null!;

    private const float OverlayShowDuration = 0.25f;
    private const float OverlayIdleDuration = 0.125f;
    private const float OverlayHideDuration = 0.1f;
    private const float GlowFadeDuration = 0.25f;
    private const float GlowTopScale = 1.35f;
    private const float ShineDelay = 0.1f;
    private const float EndParticlesDelay = 0.165f;

    private Control _overlay = null!;
    private Control _borderGlow = null!;
    private IReadOnlyList<GpuParticles2D> _revealParticles = [];
    private IReadOnlyList<GpuParticles2D> _shineParticles = [];
    private IReadOnlyList<GpuParticles2D> _endParticles = [];
    private NCard _cardNode = null!;
    private CancellationTokenSource? _cts;

    private static readonly Color _whiteOpaque = new(1, 1, 1);
    private static readonly Color _whiteClear = new(1, 1, 1, 0);
    private static readonly Vector2 _originalCardScale = new(1, 1);

    public override void _ExitTree() => _cts?.Cancel();

    public static StaticIncrementVfx? Create(NCard cardNode)
    {
        if (TestMode.IsOn) return null;
        var vfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<StaticIncrementVfx>(PackedScene.GenEditState.Disabled);
        vfx._cardNode = cardNode;
        cardNode.CardVfxContainer.AddChildSafely(vfx);
        return vfx;
    }

    public override void _Ready()
    {
        _overlay = GetNode<Control>("card_mask/white_overlay");
        _borderGlow = GetNode<Control>("card_glow_container");
        _revealParticles = [.. GetNode<Node>("reveal_particles").GetChildren().OfType<GpuParticles2D>()];
        _shineParticles = [.. GetNode<Node>("card_mask/shine_particles").GetChildren().OfType<GpuParticles2D>()];
        _endParticles = [.. GetNode<Node>("end_particles").GetChildren().OfType<GpuParticles2D>()];
        _overlay.SelfModulate = _whiteClear;
        _borderGlow.SelfModulate = _whiteClear;
        _ = TaskHelper.RunSafely(PlayAnimation());

        Entry.Logger.Info($"revealParticles: {_revealParticles.Count}, shineParticles: {_shineParticles.Count}, endParticles: {_endParticles.Count}");
    }

    private static void RestartAll(IReadOnlyList<GpuParticles2D> particles)
    {
        for (int i = 0; i < particles.Count; i++)
            particles[i].Restart();
    }

    private async Task<bool> WaitAndInterruptIfNecessary(float seconds)
    {
        var num = 0f;
        while (num <= seconds)
        {
            if (!_cardNode.IsInsideTree())
                return false;
            num += await this.AwaitProcessFrame();
        }
        return true;
    }

    private async Task PlayUntilCardUpdate()
    {
        _ = TaskHelper.RunSafely(AnimatingCardScale(_anticipationScaleCurve, OverlayShowDuration + OverlayIdleDuration));

        var tween = CreateTween();
        tween.TweenProperty(_overlay, "self_modulate", _whiteOpaque, OverlayShowDuration)
            .SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.InOut);

        if (!await WaitAndInterruptIfNecessary(OverlayShowDuration + OverlayIdleDuration))
        {
            _cardNode.Scale = _originalCardScale;
            this.QueueFreeSafely();
            return;
        }
    }

    private async Task PlayShineAndReveal()
    {
        _ = TaskHelper.RunSafely(AnimatingCardScale(_revealScaleCurve, GlowFadeDuration));

        _borderGlow.SelfModulate = _whiteOpaque;
        _borderGlow.Scale = Vector2.One;

        RestartAll(_revealParticles);

        var tween = CreateTween().SetParallel(true);
        tween.TweenProperty(_overlay, "self_modulate", _whiteClear, OverlayHideDuration)
            .SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(_borderGlow, "scale", Vector2.One * GlowTopScale, GlowFadeDuration)
            .SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(_borderGlow, "self_modulate", _whiteClear, GlowFadeDuration)
            .SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.Out);

        if (!await WaitAndInterruptIfNecessary(ShineDelay))
        {
            _cardNode.Scale = _originalCardScale;
            this.QueueFreeSafely();
            return;
        }

        RestartAll(_shineParticles);

        if (!await WaitAndInterruptIfNecessary(EndParticlesDelay))
        {
            _cardNode.Scale = _originalCardScale;
            this.QueueFreeSafely();
            return;
        }

        RestartAll(_endParticles);
        _ = TaskHelper.RunSafely(DelayedFree());
    }

    public async Task PlayAnimation()
    {
        _cts = new CancellationTokenSource();
        await PlayUntilCardUpdate();
        await PlayShineAndReveal();
    }

    private async Task DelayedFree()
    {
        await Cmd.Wait(2f);
        this.QueueFreeSafely();
    }

    private async Task AnimatingCardScale(CurveXyzTexture curve, float duration)
    {
        var num = 0f;
        var scale = Vector2.One;
        while (num < duration)
        {
            var offset = num / duration;
            scale.X = curve.CurveX.Sample(offset);
            scale.Y = curve.CurveY.Sample(offset);
            _cardNode.Scale = _originalCardScale * scale;
            num += await this.AwaitProcessFrame();
        }
        scale.X = curve.CurveX.Sample(1f);
        scale.Y = curve.CurveY.Sample(1f);
        _cardNode.Scale = _originalCardScale * scale;
    }
}
