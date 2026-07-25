using System.Reflection;
using Goldenglow.Orb;
using Goldenglow.Patch;
using Goldenglow.Ui;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Goldenglow.Card;

public static class GoldenglowOrbCmd
{
    private static readonly MethodInfo AddHistoryEntryMethod = AccessTools.Method(typeof(CombatHistory), "Add", [typeof(ICombatState), typeof(CombatHistoryEntry)]);

    public class MonsterOrbChanneledEntry(Creature generator, OrbModel orb, int roundNumber, CombatSide currentSide, CombatHistory history, IEnumerable<Player> players) : CombatHistoryEntry(generator, roundNumber, currentSide, history, players)
    {
        public OrbModel Orb { get; } = orb;

        public override string Description => Actor.ModelId.Entry + " channeled " + Orb.Id.Entry;
    }

    private static void MonsterOrbChanneled(ICombatState combatState, Player player, OrbModel orb)
    {
        AddHistoryEntryMethod.Invoke(CombatManager.Instance.History, [combatState, new MonsterOrbChanneledEntry(player.Creature, orb, combatState.RoundNumber, combatState.CurrentSide, CombatManager.Instance.History, combatState.Players)]);
    }

    public static async Task<OrbModel?> Channel(Player player, Creature? target, OrbModel orb)
    {
        if (CombatManager.Instance.IsOverOrEnding || target is null || orb is null) return null;

        if (target.IsPlayer)
        {
            var p = target.Player;
            if (p == null) return null;
            orb.Owner = null!;
            if (orb is BuoyOrb buoy)
                buoy.Source ??= player;
            await OrbCmd.Channel(new ThrowingPlayerChoiceContext(), orb, p);
        }
        else
        {
            var mgr = GetOrCreateMonsterOrbManager(target);
            var orbs = mgr.GetOrbs();
            if (mgr.Capacity > 0 && orbs.Count >= mgr.Capacity)
            {
                await EvokeOldestOrb(orbs[0], target);
                if (target == null || target.IsDead) return null;
                mgr.EvokeOrb(orbs[0]);
            }
            orb.Owner = null!;
            if (orb is BuoyOrb buoyMonster)
                buoyMonster.Source ??= player;
            MonsterOrbPatch.OwnerState[orb] = target;
            mgr.ChannelOrb(orb);
            MonsterOrbChanneled(target.CombatState!, player, orb);
            await Hook.AfterOrbChanneled(target.CombatState!, new ThrowingPlayerChoiceContext(), player, orb);
        }

        return orb;
    }

    public static async Task<OrbModel?> Channel<TOrb>(Player player, Creature? target) where TOrb : OrbModel
    {
        return await Channel(player, target, ModelDb.Orb<TOrb>().ToMutable());
    }

    public static async Task Passive(PlayerChoiceContext choiceContext, OrbModel orb, Creature? target)
    {
        if (CombatManager.Instance.IsOverOrEnding) return;
        choiceContext.PushModel(orb);
        await orb.Passive(choiceContext, target);
        choiceContext.PopModel(orb);
    }

    private static async Task EvokeOldestOrb(OrbModel orb, Creature monster)
    {
        var ctx = new ThrowingPlayerChoiceContext();
        if (await VanillaOrbMonsterHandler.TryHandleEvoke(orb, monster, ctx))
            return;
        var targets = await orb.Evoke(ctx);
        if (monster.CombatState != null)
            await Hook.AfterOrbEvoked(ctx, monster.CombatState, orb, targets);
    }

    /// <summary>
    /// Channels buoy orbs to a target, dispatching to the player OrbQueue or the monster MonsterOrbManager.
    /// Used by bidirectional cards that can target either side.
    /// </summary>
    public static async Task<OrbModel?> ChannelBuoy(Player player, Creature? target) => await Channel<BuoyOrb>(player, target);

    public static MonsterOrbManager GetOrCreateMonsterOrbManager(Creature target)
    {
        var existing = MonsterOrbManager.MonsterOrbManagerState[target];
        if (existing != null) return existing;

        var creatureNode = NCombatRoom.Instance?.GetCreatureNode(target)
            ?? throw new InvalidOperationException($"No creature node for {target}");
        var mgr = MonsterOrbManager.Create(creatureNode);
        creatureNode.AddChild(mgr);
        mgr.SetCapacity(3);
        return mgr;
    }

    /// <summary>
    /// Finds an existing MonsterOrbManager for a creature via the static Instances list.
    /// </summary>
    public static MonsterOrbManager? GetMonsterOrbManager(Creature target)
    {
        return MonsterOrbManager.Instances.FirstOrDefault(m => m.Creature == target);
    }

    public static async Task TransferOrbs(Player player, Creature? source, Creature? target, int count)
    {
        if (CombatManager.Instance.IsOverOrEnding || target is null || source is null) return;
        if (source == target || count <= 0) return;

        var orbs = PopOrbs(source, count);
        for (int i = 0; i < orbs.Count; i++)
            await Channel(player, target, orbs[i]);
    }

    private static List<OrbModel> PopOrbs(Creature source, int count)
    {
        var result = new List<OrbModel>();
        if (source.IsPlayer)
        {
            var player = source.Player!;
            var orbQueue = player.PlayerCombatState!.OrbQueue;
            var orbs = orbQueue.Orbs;
            int n = Math.Min(count, orbs.Count);
            var orbMgr = NCombatRoom.Instance?.GetCreatureNode(source)?.OrbManager;
            for (int i = 0; i < n; i++)
            {
                var orb = orbs[0];
                orbQueue.Remove(orb);
                orbMgr?.EvokeOrbAnim(orb);
                result.Add(orb);
            }
        }
        else
        {
            var mgr = GetMonsterOrbManager(source);
            if (mgr == null) return result;
            var orbs = mgr.GetOrbs();
            int n = Math.Min(count, orbs.Count);
            var snapshot = orbs.ToList();
            for (int i = 0; i < n; i++)
            {
                var orb = snapshot[i];
                mgr.EvokeOrb(orb);
                MonsterOrbPatch.OwnerState[orb] = null;
                result.Add(orb);
            }
        }
        return result;
    }
}
