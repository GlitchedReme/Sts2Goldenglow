using System.Linq;
using Godot;
using GoldenglowCharacter = Goldenglow.Character.Goldenglow;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;
using STS2RitsuLib.Models.Capabilities;
using Goldenglow.Capabilities;
using Goldenglow.Orb;
using Goldenglow.Patch;
using Goldenglow.Ui;
using Goldenglow.Card;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace Goldenglow.Core;

[RegisterSingleton]
public class GoldenglowSingleton() : HookedSingletonModel(HookType.Combat)
{
    private static readonly Dictionary<Player, int> _pulseValues = [];

    public static int GetPulse(Player? p) => p != null ? _pulseValues.GetValueOrDefault(p, 1) : 1;
    public static void IncrementPulse(Player p) => _pulseValues[p] = GetPulse(p) + 1;

    public override bool TryModifyCardRewardOptionsLate(Player player, List<CardCreationResult> cardRewardOptions, CardCreationOptions creationOptions)
    {
        bool hasModified = false;
        foreach (var option in cardRewardOptions)
        {
            if (option.Card is ICardOnGeneratedAsReward callback)
                hasModified |= callback.OnGeneratedAsReward(player, creationOptions);
        }
        return hasModified;
    }

    public override decimal ModifyOrbValue(OrbModel orb, decimal value)
    {
        if (!MonsterOrbPatch.OwnerState.TryGetValue(orb, out _))
            return value;

        if (!ModelCapabilities.TryGet(orb, out var caps))
            return value;

        foreach (var cap in caps.All)
        {
            if (cap is AbstractModel model)
                value = model.ModifyOrbValue(orb, value);
        }
        return value;
    }

    public override Task BeforeCombatStart()
    {
        RefreshAllOrbs();
        return Task.CompletedTask;
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player)
        {
            StaticCapability.ResetAll();
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player)
        {
            var combatState = participants.FirstOrDefault()?.CombatState;
            if (combatState == null) return;
            var enemies = combatState.GetCreaturesOnSide(CombatSide.Enemy).ToList();
            foreach (var enemy in enemies)
            {
                if (enemy.IsDead) continue;
                if (MonsterOrbManager.MonsterOrbManagerState.TryGetValue(enemy, out var manager) && manager != null)
                {
                    await manager.BeforeTurnEnd(choiceContext);
                    await manager.AfterTurnStart(choiceContext);
                }
            }
        }
    }

    public override async Task BeforeDeath(Creature creature)
    {
        if (MonsterOrbManager.MonsterOrbManagerState.TryGetValue(creature, out var manager) && manager != null)
        {
            manager.Clear();
        }
    }

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        _pulseValues.Clear();
        MonsterOrbManager.ClearAllInstances();
    }

    public static void ApplySkinByNetId(ulong netId)
    {
        var runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null) return;

        var player = runState.Players.FirstOrDefault(p => p.NetId == netId);
        if (player == null) return;

        if (player.Character is not GoldenglowCharacter || player.Creature == null) return;

        var skinKey = SkinResources.GetSkinKey(player);
        var res = SkinResources.GetSpine(skinKey).Combat;
        var node = NCombatRoom.Instance?.GetCreatureNode(player.Creature);
        var spine = node?.Visuals?.SpineBody;
        if (spine == null || res == null) return;

        spine.SetSkeletonDataRes(new MegaSkeletonDataResource(res));
        spine.TryGetAnimationState()?.SetAnimation("Idle", loop: true);

        RefreshOrbsForOwner(netId);
    }

    public static void RefreshOrbsForOwner(ulong netId)
    {
        var room = NCombatRoom.Instance;
        if (room == null) return;
        foreach (var child in room.FindChildren("*", "", true, false))
        {
            if (child is NBuoyOrb orb && (orb.Orb?.Model as BuoyOrb)?.Source?.NetId == netId)
                orb.RefreshSkin();
        }
    }

    public static void RefreshAllOrbs()
    {
        var room = NCombatRoom.Instance;
        if (room == null) return;
        foreach (var child in room.FindChildren("*", "", true, false))
        {
            if (child is NBuoyOrb orb)
                orb.RefreshSkin();
        }
    }
}
