using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Random;

namespace Goldenglow.Ui;

public partial class SpinePlayer : Node
{
    public override void _Ready()
    {
        this.RunWhenSpineReady(new MegaSprite(GetParent()), state =>
        {
            Callable.From(() =>
            {
                var animationState = new MegaSprite(GetParent()).GetAnimationState();
                animationState.SetAnimation("Idle", true);
                var megaTrackEntry = animationState.GetCurrent(0);
                megaTrackEntry?.SetTrackTime(megaTrackEntry.GetAnimationEnd() * Rng.Chaotic.NextFloat());
            }).CallDeferred();
        });
    }
}
