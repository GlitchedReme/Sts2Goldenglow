using Goldenglow.Orb;
using Goldenglow.Patch;
using Goldenglow.Ui;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Goldenglow.Card;

public static class GoldenglowOrbCmd
{
    public static async Task Channel(Creature target, OrbModel orb)
    {
        if (target.IsPlayer)
        {
            var player = target.Player;
            if (player == null) return;
            orb.Owner = null!;
            await OrbCmd.Channel(new ThrowingPlayerChoiceContext(), orb, player);
        }
        else
        {
            var mgr = GetOrCreateMonsterOrbManager(target);
            var orbs = mgr.GetOrbs();
            if (mgr.Capacity > 0 && orbs.Count >= mgr.Capacity)
            {
                await EvokeOldestOrb(orbs[0], target);
                if (target.IsDead) return;
                mgr.EvokeOrb(orbs[0]);
            }
            orb.Owner = null!;
            MonsterOrbPatch.OwnerState[orb] = target;
            mgr.ChannelOrb(orb);
        }
    }

    public static async Task Channel<TOrb>(Creature target, int count) where TOrb : OrbModel
    {
        if (target.IsPlayer)
        {
            var player = target.Player;
            if (player == null) return;
            for (int i = 0; i < count; i++)
                await OrbCmd.Channel<TOrb>(new ThrowingPlayerChoiceContext(), player);
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                if (target.IsDead) return;
                var mgr = GetOrCreateMonsterOrbManager(target);
                var orbs = mgr.GetOrbs();
                if (mgr.Capacity > 0 && orbs.Count >= mgr.Capacity)
                {
                    await EvokeOldestOrb(orbs[0], target);
                    if (target.IsDead) return;
                    mgr.EvokeOrb(orbs[0]);
                }
                var orb = ModelDb.Orb<TOrb>().ToMutable();
                orb.Owner = null!;
                MonsterOrbPatch.OwnerState[orb] = target;
                mgr.ChannelOrb(orb);
            }
        }
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
    public static async Task ChannelBuoy(Creature target, int count) => await Channel<BuoyOrb>(target, count);

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

    public static async Task TransferOrbs(Creature source, Creature target, int count)
    {
        if (CombatManager.Instance.IsOverOrEnding) return;
        if (source == target || count <= 0) return;

        var orbs = PopOrbs(source, count);
        for (int i = 0; i < orbs.Count; i++)
            await Channel(target, orbs[i]);
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
