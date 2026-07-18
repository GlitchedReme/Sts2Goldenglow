using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using Goldenglow.Core;

namespace Goldenglow.Ui;

public partial class SkinSelect : HBoxContainer
{
    public static int CurrentSkinIndex { get; set; }

    private int _currentIndex;

    private MegaSprite _leftSprite = null!;
    private MegaSprite _centerSprite = null!;
    private MegaSprite _rightSprite = null!;
    private MegaSprite _condidateSprite = null!;
    private MegaLabel _label = null!;

    private Node2D _leftNode = null!;
    private Node2D _centerNode = null!;
    private Node2D _rightNode = null!;
    private Node2D _candidateNode = null!;

    private Vector2 _leftHome;
    private Vector2 _centerHome;
    private Vector2 _rightHome;
    private float _slotWidth;

    private bool _isAnimating;

    private const float AnimDuration = 0.25f;

    public override void _Ready()
    {
        var leftBtn = GetNode<GGoldArrowButton>("LeftButton");
        var rightBtn = GetNode<GGoldArrowButton>("RightButton");

        _leftNode = GetNode<Node2D>("NinePatchRect/Mask/Node/SpineSprite3");
        _centerNode = GetNode<Node2D>("NinePatchRect/Mask/Node/SpineSprite");
        _rightNode = GetNode<Node2D>("NinePatchRect/Mask/Node/SpineSprite2");
        _candidateNode = GetNode<Node2D>("NinePatchRect/Mask/Node/SpineSprite4");

        _leftSprite = new MegaSprite(_leftNode);
        _centerSprite = new MegaSprite(_centerNode);
        _rightSprite = new MegaSprite(_rightNode);
        _condidateSprite = new MegaSprite(_candidateNode);
        _label = GetNode<GMegaLabel>("NinePatchRect/Mask/Label");

        leftBtn.Released += OnLeftPressed;
        rightBtn.Released += OnRightPressed;

        _leftHome = _leftNode.Position;
        _centerHome = _centerNode.Position;
        _rightHome = _rightNode.Position;
        _slotWidth = _centerHome.X - _leftHome.X;

        _candidateNode.Visible = false;

        _currentIndex = SkinResources.IndexOfKey(SkinResources.SelectedSkinKey);
        CurrentSkinIndex = _currentIndex;
        UpdateSkin();
    }

    private void OnLeftPressed(NClickableControl _)
    {
        if (_isAnimating) return;
        _isAnimating = true;

        var c = SkinResources.Keys.Count;
        var targetIndex = (_currentIndex - 1 + c) % c;

        ApplySkin(_condidateSprite, (_currentIndex - 2 + c) % c);
        _candidateNode.Position = new Vector2(_leftHome.X - _slotWidth, _leftHome.Y);
        _candidateNode.Visible = true;

        var tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(_leftNode, "position:x", _leftNode.Position.X + _slotWidth, AnimDuration);
        tween.TweenProperty(_centerNode, "position:x", _centerNode.Position.X + _slotWidth, AnimDuration);
        tween.TweenProperty(_rightNode, "position:x", _rightNode.Position.X + _slotWidth, AnimDuration);
        tween.TweenProperty(_candidateNode, "position:x", _candidateNode.Position.X + _slotWidth, AnimDuration);
        tween.SetParallel(false);
        tween.SetTrans(Tween.TransitionType.Cubic);
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenCallback(Callable.From(() => OnTweenComplete(targetIndex)));
    }

    private void OnRightPressed(NClickableControl _)
    {
        if (_isAnimating) return;
        _isAnimating = true;

        var c = SkinResources.Keys.Count;
        var targetIndex = (_currentIndex + 1) % c;

        ApplySkin(_condidateSprite, (_currentIndex + 2) % c);
        _candidateNode.Position = new Vector2(_rightHome.X + _slotWidth, _rightHome.Y);
        _candidateNode.Visible = true;

        var tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(_leftNode, "position:x", _leftNode.Position.X - _slotWidth, AnimDuration);
        tween.TweenProperty(_centerNode, "position:x", _centerNode.Position.X - _slotWidth, AnimDuration);
        tween.TweenProperty(_rightNode, "position:x", _rightNode.Position.X - _slotWidth, AnimDuration);
        tween.TweenProperty(_candidateNode, "position:x", _candidateNode.Position.X - _slotWidth, AnimDuration);
        tween.SetParallel(false);
        tween.SetTrans(Tween.TransitionType.Cubic);
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenCallback(Callable.From(() => OnTweenComplete(targetIndex)));
    }

    private void OnTweenComplete(int newIndex)
    {
        _currentIndex = newIndex;
        CurrentSkinIndex = _currentIndex;

        _leftNode.Position = _leftHome;
        _centerNode.Position = _centerHome;
        _rightNode.Position = _rightHome;
        _candidateNode.Visible = false;

        UpdateSkin();
        _isAnimating = false;
    }

    private void UpdateSkin()
    {
        var c = SkinResources.Keys.Count;
        ApplySkin(_leftSprite, (_currentIndex - 1 + c) % c);
        ApplySkin(_centerSprite, _currentIndex);
        ApplySkin(_rightSprite, (_currentIndex + 1) % c);

        var key = SkinResources.Keys[_currentIndex];
        SkinResources.SelectedSkinKey = key;
        _label.Text = new LocString("settings_ui", SkinResources.GetLocKey(key)).GetFormattedText();
    }

    private static void ApplySkin(MegaSprite sprite, int index)
    {
        var res = SkinResources.GetSpine(SkinResources.Keys[index]).Combat;
        sprite.SetSkeletonDataRes(new MegaSkeletonDataResource(res));
        sprite.TryGetAnimationState()?.SetAnimation("Idle", loop: true);
    }
}
