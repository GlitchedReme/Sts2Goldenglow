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

    private const int SlotCount = 4;
    private readonly Node2D[] _nodes = new Node2D[SlotCount];
    private readonly MegaSprite[] _sprites = new MegaSprite[SlotCount];
    private int _leftIdx, _centerIdx, _rightIdx, _spareIdx;

    private MegaLabel _label = null!;

    private Vector2 _leftHome;
    private Vector2 _centerHome;
    private Vector2 _rightHome;
    private float _slotWidth;

    private bool _isAnimating;

    private const float AnimDuration = 0.15f;

    public override void _Ready()
    {
        var leftBtn = GetNode<GGoldArrowButton>("LeftButton");
        var rightBtn = GetNode<GGoldArrowButton>("RightButton");

        _nodes[0] = GetNode<Node2D>("NinePatchRect/Mask/Node/SpineSprite");
        _nodes[1] = GetNode<Node2D>("NinePatchRect/Mask/Node/SpineSprite2");
        _nodes[2] = GetNode<Node2D>("NinePatchRect/Mask/Node/SpineSprite3");
        _nodes[3] = GetNode<Node2D>("NinePatchRect/Mask/Node/SpineSprite4");

        for (int i = 0; i < SlotCount; i++)
            _sprites[i] = new MegaSprite(_nodes[i]);

        _label = GetNode<GMegaLabel>("NinePatchRect/Mask/Label");

        leftBtn.Released += OnLeftPressed;
        rightBtn.Released += OnRightPressed;

        _centerHome = _nodes[0].Position;
        _rightHome = _nodes[1].Position;
        _leftHome = _nodes[2].Position;
        _slotWidth = _centerHome.X - _leftHome.X;

        _leftIdx = 2;
        _centerIdx = 0;
        _rightIdx = 1;
        _spareIdx = 3;
        _nodes[_spareIdx].Visible = false;

        _currentIndex = SkinResources.IndexOfKey(SkinResources.SelectedSkinKey);
        CurrentSkinIndex = _currentIndex;

        var c = SkinResources.Keys.Count;
        ApplySkin(_sprites[_leftIdx], (_currentIndex - 1 + c) % c);
        ApplySkin(_sprites[_centerIdx], _currentIndex);
        ApplySkin(_sprites[_rightIdx], (_currentIndex + 1) % c);

        UpdateLabel();
    }

    private void OnLeftPressed(NClickableControl _)
    {
        if (_isAnimating) return;
        _isAnimating = true;

        var c = SkinResources.Keys.Count;
        var targetIndex = (_currentIndex - 1 + c) % c;

        ApplySkin(_sprites[_spareIdx], (_currentIndex - 2 + c) % c);
        _nodes[_spareIdx].Position = new Vector2(_leftHome.X - _slotWidth, _leftHome.Y);
        _nodes[_spareIdx].Visible = true;

        var tween = CreateTween();
        tween.SetParallel(true);
        for (int i = 0; i < SlotCount; i++)
            tween.TweenProperty(_nodes[i], "position:x", _nodes[i].Position.X + _slotWidth, AnimDuration);
        tween.SetParallel(false);
        tween.SetTrans(Tween.TransitionType.Cubic);
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenCallback(Callable.From(() => OnTweenComplete(targetIndex, rightward: false)));
    }

    private void OnRightPressed(NClickableControl _)
    {
        if (_isAnimating) return;
        _isAnimating = true;

        var c = SkinResources.Keys.Count;
        var targetIndex = (_currentIndex + 1) % c;

        ApplySkin(_sprites[_spareIdx], (_currentIndex + 2) % c);
        _nodes[_spareIdx].Position = new Vector2(_rightHome.X + _slotWidth, _rightHome.Y);
        _nodes[_spareIdx].Visible = true;

        var tween = CreateTween();
        tween.SetParallel(true);
        for (int i = 0; i < SlotCount; i++)
            tween.TweenProperty(_nodes[i], "position:x", _nodes[i].Position.X - _slotWidth, AnimDuration);
        tween.SetParallel(false);
        tween.SetTrans(Tween.TransitionType.Quad);
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenCallback(Callable.From(() => OnTweenComplete(targetIndex, rightward: true)));
    }

    private void OnTweenComplete(int newIndex, bool rightward)
    {
        _currentIndex = newIndex;
        CurrentSkinIndex = _currentIndex;

        if (rightward)
        {
            var oldLeft = _leftIdx;
            _leftIdx = _centerIdx;
            _centerIdx = _rightIdx;
            _rightIdx = _spareIdx;
            _spareIdx = oldLeft;
        }
        else
        {
            var oldRight = _rightIdx;
            _rightIdx = _centerIdx;
            _centerIdx = _leftIdx;
            _leftIdx = _spareIdx;
            _spareIdx = oldRight;
        }

        _nodes[_spareIdx].Visible = false;

        UpdateLabel();
        _isAnimating = false;
    }

    private void UpdateLabel()
    {
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
