using Godot;
using Goldenglow.Core;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.TestSupport;

namespace Goldenglow.Vfx;

public partial class BuoyCardAttackVfx : Node2D
{
    public const string scenePath = "res://Goldenglow/scene/vfx/buoy_card_attack_vfx.tscn";

    private const float FadeInDuration = 0.3f;
    private const float ParticleDelay = 0.5f;
    private const float ParticleRiseGap = 0.1f;
    private const float RiseDuration = 0.1f;
    private const float FallDuration = 0.5f;

    private Creature _target = null!;
    private Player _source = null!;
    private Node2D _buoy = null!;
    private Node2D _particles = null!;
    private Func<Task>? _onAttack;
    private float _startDelay;
    private CancellationTokenSource? _cts;
    private Task? _sequenceTask;

    public Task? CompletionTask => _sequenceTask;

    public override void _ExitTree() => _cts?.Cancel();

    public static BuoyCardAttackVfx? Create(Vector2 position, Player source, Creature target, Func<Task>? onAttack, float startDelay = 0f)
    {
        if (TestMode.IsOn) return null;
        var vfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<BuoyCardAttackVfx>();
        vfx.GlobalPosition = position;
        vfx._source = source;
        vfx._onAttack = onAttack;
        vfx._startDelay = startDelay;
        vfx._target = target;
        var creatureNode = target.GetCreatureNode();
        if (creatureNode != null)
        {
            var dir = creatureNode.GlobalPosition - vfx.GlobalPosition;
            vfx.Rotation = dir.Angle() - Mathf.Pi / 2;
        }
        return vfx;
    }

    public override void _Ready()
    {
        _buoy = GetNode<Node2D>("%Buoy");
        _particles = GetNode<Node2D>("%Particles");
        _sequenceTask = TaskHelper.RunSafely(PlaySequence());
    }

    private async Task PlaySequence()
    {
        _cts = new CancellationTokenSource();

        _buoy.Rotation = (Random.Shared.NextSingle() * Mathf.Pi) - (Mathf.Pi / 2f);
        _buoy.Modulate = new Color(1, 1, 1, 0);

        if (_startDelay > 0f)
            await Cmd.Wait(_startDelay, _cts.Token);

        var fadeIn = CreateTween().SetParallel(true);
        fadeIn.TweenProperty(_buoy, "rotation", 0f, FadeInDuration)
            .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        fadeIn.TweenProperty(_buoy, "modulate:a", 1f, FadeInDuration * 0.25f)
            .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.In);

        await Cmd.Wait(ParticleDelay);

        foreach (var p in _particles.GetChildren().OfType<GpuParticles2D>())
        {
            p.Restart();
            p.Emitting = true;
        }

        await Cmd.Wait(ParticleRiseGap);
        CreateTween().TweenProperty(_buoy, "position:y", -30f, RiseDuration)
            .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);

        var lightning = BuoyLightning.Create(this, _target.GetCreatureNode()!)!;
        GoldenglowUtils.PlayVfx(_target, lightning, GlobalPosition);
        var skin = SkinResources.GetSkinKey(_source);
        GoldenglowUtils.PlayVfx(_target, BuoyAttackVfx.Create(skin));

        SfxCmd.Play("event:/goldenglow/sfx/buoy_evoke");

        await Cmd.Wait(RiseDuration);

        if (_onAttack != null)
            await _onAttack();

        CreateTween().TweenProperty(_buoy, "position:y", 0f, FallDuration)
            .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut);
        CreateTween().TweenProperty(_buoy, "modulate:a", 0f, FallDuration)
            .SetTrans(Tween.TransitionType.Linear);

        await Cmd.Wait(FallDuration, _cts.Token);
        this.QueueFreeSafely();
    }
}
