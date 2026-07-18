using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;
using MegaCrit.Sts2.Core.Assets;

namespace Goldenglow.Ui;

/// <summary>
/// Attract preview UI — shows discard pile top cards with slide-in animation.
/// </summary>
[RegisterNodeAttachment(typeof(NCombatUi), "AttractPreview", NodeName = "AttractPreview", DuplicatePolicy = NodeAttachmentDuplicatePolicy.ReuseExistingByName)]
public partial class AttractUi : Control
{
    private static AttractUi? _instance;

    private readonly List<Control> _containers = [];
    public bool IsShowing { get; private set; }

    private const float CardSpacing = 30f;
    private const float SlideDuration = 0.2f;

    public override void _Ready()
    {
        _instance = this;
        var viewSize = GetViewportRect().Size;
        Position = new Vector2(viewSize.X - 260f, viewSize.Y - 550f);
    }

    private Control GetOrCreate(int index)
    {
        if (index < _containers.Count)
            return _containers[index];

        var scene = PreloadManager.Cache.GetScene("res://scenes/ui/card_hover_tip.tscn");
        var container = scene.Instantiate<Control>();
        container.Visible = false;
        AddChild(container);
        _containers.Add(container);
        return container;
    }

    public void UpdatePreview(CardModel sourceCard)
    {
        var dv = sourceCard.DynamicVars;
        var owner = sourceCard.Owner;
        var discardPile = owner != null ? PileType.Discard.GetPile(owner) : null;
        int count = dv.TryGetValue("Goldenglow_Attract", out var v) ? (int)v.BaseValue : 0;
        int maxShow = discardPile != null ? Mathf.Min(count, discardPile.Cards.Count) : 0;

        if (maxShow <= 0)
        {
            Hide();
            return;
        }

        IsShowing = true;

        for (int i = 0; i < maxShow; i++)
        {
            int idx = discardPile!.Cards.Count - i - 1;
            var cardModel = discardPile.Cards[idx];
            var targetPos = new Vector2(0, i * CardSpacing);
            var container = GetOrCreate(i);
            var nCard = container.GetNode<NCard>("%Card");
            nCard.Scale = Vector2.One * 0.5f;
            nCard.Model = cardModel;
            Callable.From(() => nCard.UpdateVisuals(PileType.None, CardPreviewMode.Normal)).CallDeferred();
            container.Position = targetPos + Vector2.Right * 200f;
            container.Visible = true;

            var tween = CreateTween();
            tween.SetParallel(false);
            tween.SetTrans(Tween.TransitionType.Quint);
            tween.SetEase(Tween.EaseType.Out);
            tween.TweenProperty(container, "position", targetPos, SlideDuration);
        }
    }

    public new void Hide()
    {
        IsShowing = false;
        foreach (var container in _containers)
            container.Visible = false;
    }

    public static AttractUi? Get() => _instance;
}
