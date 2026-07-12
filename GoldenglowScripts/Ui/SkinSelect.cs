using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Runs;

namespace Goldenglow.Ui;

public partial class SkinSelect : HBoxContainer
{
    private struct SkinDef
    {
        public string DisplayName;
        public string ResourcePath;
    }

    private static readonly List<SkinDef> _skinDefs =
    [
        new() { DisplayName = "default", ResourcePath = "res://Goldenglow/image/character/default.tres" },
        new() { DisplayName = "snow", ResourcePath = "res://Goldenglow/image/character/snow.tres" },
        new() { DisplayName = "sanrio", ResourcePath = "res://Goldenglow/image/character/sanrio.tres" },
        new() { DisplayName = "summer", ResourcePath = "res://Goldenglow/image/character/summer.tres" },
    ];

    private static Resource[]? _skeletonResources;
    private static Resource[] SkeletonResources
    {
        get
        {
            if (_skeletonResources == null)
            {
                _skeletonResources = new Resource[_skinDefs.Count];
                for (int i = 0; i < _skinDefs.Count; i++)
                    _skeletonResources[i] = PreloadManager.Cache.GetAsset<Resource>(_skinDefs[i].ResourcePath);
            }
            return _skinDefs.Count == 0 ? [] : _skeletonResources;
        }
    }

    public static int CurrentSkinIndex { get; set; }

    private int _currentIndex;

    private MegaSprite _leftSprite = null!;
    private MegaSprite _centerSprite = null!;
    private MegaSprite _rightSprite = null!;
    private MegaLabel _label = null!;

    public override void _Ready()
    {
        var leftBtn = GetNode<GGoldArrowButton>("LeftButton");
        var rightBtn = GetNode<GGoldArrowButton>("RightButton");

        _leftSprite = new MegaSprite(GetNode("NinePatchRect/Mask/Node/SpineSprite3"));
        _centerSprite = new MegaSprite(GetNode("NinePatchRect/Mask/Node/SpineSprite"));
        _rightSprite = new MegaSprite(GetNode("NinePatchRect/Mask/Node/SpineSprite2"));
        _label = GetNode<GMegaLabel>("NinePatchRect/Mask/Label");

        leftBtn.Released += OnLeftPressed;
        rightBtn.Released += OnRightPressed;

        _currentIndex = CurrentSkinIndex;
        UpdateSkin();
    }

    private void OnLeftPressed(NClickableControl _)
    {
        _currentIndex = (_currentIndex - 1 + _skinDefs.Count) % _skinDefs.Count;
        CurrentSkinIndex = _currentIndex;
        UpdateSkin();
    }

    private void OnRightPressed(NClickableControl _)
    {
        _currentIndex = (_currentIndex + 1) % _skinDefs.Count;
        CurrentSkinIndex = _currentIndex;
        UpdateSkin();
    }

    private void UpdateSkin()
    {
        var c = _skinDefs.Count;
        ApplySkin(_leftSprite, (_currentIndex - 1 + c) % c);
        ApplySkin(_centerSprite, _currentIndex);
        ApplySkin(_rightSprite, (_currentIndex + 1) % c);
        _label.Text = new LocString("settings_ui", $"GOLDENGLOW_SETTINGS_SKIN.{_skinDefs[_currentIndex].DisplayName}").GetFormattedText();

        var player = LocalContext.GetMe(RunManager.Instance.DebugOnlyGetState());
        if (player == null)
            return;
        Entry.Skin.Modify(player, data =>
        {
            data.Skin = _skinDefs[_currentIndex].DisplayName;
        });
    }

    private static void ApplySkin(MegaSprite sprite, int index)
    {
        var res = SkeletonResources[index];
        if (res == null)
            return;

        sprite.SetSkeletonDataRes(new MegaSkeletonDataResource(res));
        sprite.TryGetAnimationState()?.SetAnimation("Idle", loop: true);
    }
}
