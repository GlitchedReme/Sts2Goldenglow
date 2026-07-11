using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using STS2RitsuLib.Utils;

namespace Goldenglow.Ui;

public partial class MonsterOrbManager : Control
{
    private NCreature _creatureNode = null!;
    private Control _orbContainer = null!;
    private Tween? _curTween;

    private readonly List<OrbModel> _orbs = [];
    private readonly List<NOrb> _orbNodes = [];
    private int _capacity;

    /// <summary>
    /// All active MonsterOrbManager instances, for reliable lookup when Creature references are not stable.
    /// </summary>
    internal static readonly List<MonsterOrbManager> Instances = [];


    /// <summary>
    /// Whether this orb manager belongs to the local player. Monsters are always local.
    /// Mirrors NOrbManager.IsLocal.
    /// </summary>
    public bool IsLocal { get; set; } = true;

    private const float TotalSpread = 90f;
    private const float MinRadius = 225f;
    private const float MaxRadius = 300f;
    private const float TweenSpeed = 0.45f;

    public static readonly AttachedState<Creature, MonsterOrbManager?> MonsterOrbManagerState = new(() => null);

    public Creature Creature => _creatureNode.Entity;

    public static void ClearAllInstances()
    {
        var enemies = CombatManager.Instance.DebugOnlyGetState()?.Enemies;
        if (enemies != null)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                if (MonsterOrbManagerState.TryGetValue(enemies[i], out var mgr) && mgr != null)
                    mgr.Clear();
            }
        }
        MonsterOrbManagerState.Clear();
        Instances.Clear();

    }

    public override void _Ready()
    {
        _orbContainer = GetNode<Control>("Orbs");
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        CombatManager.Instance.StateTracker.CombatStateChanged += OnCombatStateChanged;
        CombatManager.Instance.CombatSetUp += OnCombatSetup;
        SetOrbManagerPosition();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        CombatManager.Instance.StateTracker.CombatStateChanged -= OnCombatStateChanged;
        CombatManager.Instance.CombatSetUp -= OnCombatSetup;
        MonsterOrbManagerState.Remove(_creatureNode.Entity);
        Instances.Remove(this);
    }

    public static MonsterOrbManager Create(NCreature creature)
    {
        var mgr = new MonsterOrbManager
        {
            Name = "MonsterOrbManager",
            _creatureNode = creature,
            IsLocal = true
        };

        var orbContainer = new Control { Name = "Orbs" };
        mgr.AddChild(orbContainer);
        MonsterOrbManagerState.Set(creature.Entity, mgr);
        Instances.Add(mgr);


        return mgr;
    }

    public async Task BeforeTurnEnd(PlayerChoiceContext choiceContext)
    {
        if (Creature.CombatState == null)
            return;

        foreach (OrbModel orb in _orbs.ToArray())
        {
            if (!_orbs.Contains(orb)) continue;
            var triggerCount = Hook.ModifyOrbPassiveTriggerCount(Creature.CombatState, orb, 1, out List<AbstractModel> modifyingModels);
            await Hook.AfterModifyingOrbPassiveTriggerCount(Creature.CombatState, orb, modifyingModels);
            for (int i = 0; i < triggerCount; i++)
            {
                if (!_orbs.Contains(orb)) break;
                await orb.BeforeTurnEndOrbTrigger(choiceContext);
                await Cmd.Wait(0.05f);
            }
        }
    }

    private void OnCombatSetup(CombatState _)
    {
        if (_capacity > 0)
            SyncSlotNodes();
        SetOrbManagerPosition();
    }

    private void OnCombatStateChanged(CombatState _)
    {
        UpdateVisuals(OrbEvokeType.None);
    }

    public void UpdateVisuals(OrbEvokeType evokeType)
    {
        foreach (NOrb orb in _orbNodes)
        {
            orb.UpdateVisuals(isEvoking: false);
        }
        switch (evokeType)
        {
            case OrbEvokeType.Front:
                _orbNodes.FirstOrDefault()?.UpdateVisuals(isEvoking: true);
                break;
            case OrbEvokeType.All:
                {
                    foreach (NOrb orb2 in _orbNodes)
                    {
                        orb2.UpdateVisuals(isEvoking: true);
                    }
                    break;
                }
            case OrbEvokeType.None:
                break;
        }
    }

    private void SetOrbManagerPosition()
    {
        var visuals = _creatureNode.Visuals;
        float absX = Mathf.Abs(visuals.Scale.X);
        Scale = (absX > 1f) ? Vector2.One : Vector2.One * Mathf.Lerp(absX, 1f, 0.5f);
        Scale *= 0.85f;
        Position = visuals.OrbPosition.Position * Mathf.Min(visuals.Scale.X, 1.25f);
    }

    /// <summary>Set how many orb slots this monster has. Creates or destroys visual slot nodes to match.</summary>
    public void SetCapacity(int slots)
    {
        int delta = slots - _capacity;
        _capacity = slots;

        if (delta > 0)
            AddSlotAnim(delta);
        else if (delta < 0)
            RemoveSlotAnim(-delta);
    }

    public int Capacity => _capacity;

    /// <summary>Channel an orb visually. Evokes the oldest orb if capacity is full.</summary>
    public void ChannelOrb(OrbModel orb)
    {
        _orbs.Add(orb);
        AddOrbAnim();
    }

    public void EvokeOrb(OrbModel orb)
    {
        _orbs.Remove(orb);
        EvokeOrbAnim(orb);
    }

    public IReadOnlyList<OrbModel> GetOrbs() => _orbs.AsReadOnly();

    public void Clear()
    {
        _orbs.Clear();
        ClearOrbsVisual();
    }

    /// <summary>
    /// Creates empty NOrb slot nodes. Mirrors NOrbManager.AddSlotAnim.
    /// </summary>
    private void AddSlotAnim(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            var nOrb = NOrb.Create(true);
            _orbContainer.AddChildSafely(nOrb);
            _orbNodes.Add(nOrb);
            nOrb.Position = Vector2.Zero;
        }
        TweenLayout();
    }

    /// <summary>
    /// Removes slot nodes from the end. Mirrors NOrbManager.RemoveSlotAnim.
    /// Slots with orbs already in them are also removed.
    /// </summary>
    private void RemoveSlotAnim(int amount)
    {
        if (amount > _orbNodes.Count)
            throw new InvalidOperationException("Not enough slots to remove.");

        for (int i = 0; i < amount; i++)
        {
            var last = _orbNodes[^1];
            _orbNodes.RemoveAt(_orbNodes.Count - 1);
            last.QueueFreeSafely();
        }
        TweenLayout();
    }

    /// <summary>
    /// Ensures _orbNodes count matches _capacity. Called when capacity was set before AddChild.
    /// </summary>
    private void SyncSlotNodes()
    {
        int delta = _capacity - _orbNodes.Count;
        if (delta > 0)
            AddSlotAnim(delta);
        else if (delta < 0)
            RemoveSlotAnim(-delta);
    }

    /// <summary>
    /// Fills the first empty slot with a new orb. Mirrors NOrbManager.AddOrbAnim.
    /// If all slots are full, evokes the oldest orb first, then retries.
    /// </summary>
    private void AddOrbAnim()
    {
        if (_orbNodes.Count == 0) return;

        var model = _orbs.Count > 0 ? _orbs[^1] : null;

        var empty = _orbNodes.Find(n => n.Model == null);
        if (empty == null)
        {
            var first = _orbNodes.Find(n => n.Model != null);
            if (first?.Model == null) return;
            EvokeOrbAnim(first.Model);
            empty = _orbNodes.Find(n => n.Model == null);
            if (empty == null) return;
        }

        var fresh = NOrb.Create(true, model);
        empty.AddSiblingSafely(fresh);
        fresh.Position = empty.Position;

        int idx = _orbNodes.IndexOf(empty);
        _orbNodes[idx] = fresh;
        _orbContainer.RemoveChildSafely(empty);
        empty.QueueFreeSafely();

        TweenLayout();
    }

    /// <summary>
    /// Fades out an orb and replaces it with an empty slot. Mirrors NOrbManager.EvokeOrbAnim.
    /// </summary>
    private void EvokeOrbAnim(OrbModel orb)
    {
        var target = _orbNodes.FindLast(n => n.Model == orb)!;

        var tween = CreateTween();
        tween.TweenProperty(target, "modulate:a", 0, 0.25);
        tween.Chain().TweenCallback(Callable.From(() =>
        {
            int idx = _orbNodes.IndexOf(target);
            if (idx >= 0)
            {
                target.QueueFreeSafely();
                _orbNodes.RemoveAt(idx);
            }
            TweenLayout();
        }));

        var replacement = NOrb.Create(true);
        _orbContainer.AddChildSafely(replacement);
        _orbNodes.Add(replacement);
        replacement.Position = Vector2.Zero;
        TweenLayout();
    }

    private void ClearOrbsVisual()
    {
        _curTween?.Kill();
        if (_orbNodes.Count == 0) return;

        _curTween = CreateTween();
        for (int i = 0; i < _orbNodes.Count; i++)
        {
            var orb = _orbNodes[i];
            _curTween.Parallel().TweenProperty(orb, "position", Vector2.Zero, 1.0)
                .SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
            _curTween.Parallel().TweenProperty(orb, "modulate:a", 0, 0.25);
        }
        for (int i = 0; i < _orbNodes.Count; i++)
            _curTween.Chain().TweenCallback(Callable.From(_orbNodes[i].QueueFreeSafely));
        _orbNodes.Clear();
    }

    private void TweenLayout()
    {
        if (_capacity < 1) return;

        if (_orbNodes.Count == 0) return;

        float flipX = _creatureNode.Visuals.Scale.X < 0f ? -1f : 1f;
        float step = _capacity > 1 ? TotalSpread / (_capacity - 1) : 0f;
        float radius = Mathf.Lerp(MinRadius, MaxRadius, (_capacity - 3f) / 7f);
        radius *= 0.65f;

        _curTween?.Kill();
        _curTween = CreateTween().SetParallel();

        for (int i = 0; i < _capacity && i < _orbNodes.Count; i++)
        {
            float deg = -87.5f - TotalSpread / 2f + step * i;
            float rad = float.DegreesToRadians(deg);
            var pos = new Vector2(-Mathf.Cos(rad) * flipX, Mathf.Sin(rad)) * radius;
            _curTween.TweenProperty(_orbNodes[i], "position", pos, TweenSpeed)
                .SetEase(Tween.EaseType.InOut)
                .SetTrans(Tween.TransitionType.Sine);
        }
    }
}
