using Goldenglow.Capabilities;
using STS2RitsuLib.Models.Capabilities;
using Godot;
using Goldenglow.Patch;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Godot;
using MegaCrit.Sts2.Core.Combat;
using Goldenglow.Power;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Goldenglow.Orb;

[RegisterOrb]
public class BuoyOrb : ModOrbTemplate
{
    public Creature? Holder => MonsterOrbPatch.OwnerState.TryGetValue(this, out var creature) && creature != null ? creature : Owner?.Creature;

    public Player? Source { get; set; }

#if !STS2_AT_LEAST_109_0
    internal event Action<Creature[]>? GgEvokeActivated;
#endif

    protected override string PassiveSfx => "event:/goldenglow/sfx/buoy_evoke";

    protected override string EvokeSfx => "event:/goldenglow/sfx/buoy_evoke";

    protected override string ChannelSfx => "event:/sfx/characters/defect/defect_lightning_channel";

    public override decimal PassiveVal => ModifyOrbValue(2m);

    public override decimal EvokeVal => ModifyOrbValue(5m);

    public override Color DarkenedColor => Colors.Gray;

    public override ModOrbValueDisplayMode ValueDisplayMode => ModOrbValueDisplayMode.Contextual;

    public override OrbAssetProfile AssetProfile => new(
        IconPath: "res://Goldenglow/image/orb/buoy_default.png",
        VisualsScenePath: "res://Goldenglow/scene/gg_buoy.tscn"
    );

    protected override Node2D? TryCreateOrbSprite() => RitsuGodotNodeFactories.CreateFromScenePath<Node2D>(AssetProfile.VisualsScenePath!);

    public override async Task BeforeTurnEndOrbTrigger(PlayerChoiceContext choiceContext)
    {
        await Passive(choiceContext, null);
        await Cmd.Wait(0.1f);
    }

    public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
    {
        var holder = Holder ?? throw new InvalidOperationException("BuoyOrb has no Holder set");

        PlayPassiveSfx();
#if STS2_AT_LEAST_109_0
        ActivatePassive();
#endif
        if (IsFriendly(holder))
        {
            await CreatureCmd.GainBlock(holder, PassiveVal, ValueProp.Unpowered, null);
        }
        else
        {
#if STS2_AT_LEAST_109_0
            ActivateEvoke([holder]);
#else
            GgEvokeActivated?.Invoke([holder]);
#endif
            await CreatureCmd.Damage(choiceContext, holder, PassiveVal, ValueProp.Unpowered, holder);
            AfterDamage();
        }
    }

    public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext choiceContext)
    {
        var holder = Holder ?? throw new InvalidOperationException("BuoyOrb has no Holder set");

        PlayEvokeSfx();
        if (IsFriendly(holder))
        {
            await CreatureCmd.GainBlock(holder, EvokeVal, ValueProp.Unpowered, null);
        }
        else
        {
#if STS2_AT_LEAST_109_0
            ActivateEvoke([holder]);
#else
            GgEvokeActivated?.Invoke([holder]);
#endif
            await CreatureCmd.Damage(choiceContext, holder, EvokeVal, ValueProp.Unpowered, holder);
            AfterDamage();
        }
        return [holder];
    }

    private void AfterDamage()
    {
        var holder = Holder ?? throw new InvalidOperationException("BuoyOrb has no Holder set");
        var amount = holder.CombatState?.GetCreaturesOnSide(CombatSide.Player).Sum(c => c.GetPowerAmount<DroneCasterPower>()) ?? 0;
        if (amount > 0)
        {
            var cap = ModelCapabilityRegistry.Create<OrbBoostCapability>();
            cap.DynamicVars["Amount"].BaseValue = amount;
            this.AddCapability(cap);
        }
    }

    private static bool IsFriendly(Creature c) => c.IsPlayer || c.PetOwner != null;
}
