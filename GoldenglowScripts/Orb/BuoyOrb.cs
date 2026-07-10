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

namespace Goldenglow.Orb;

/// <summary>
/// Buoy orb — parasitic orb that channels onto enemies and protects allies.
/// Evoke: if the holder is friendly, grants 2 block; otherwise deals 2 damage.
/// </summary>
[RegisterOrb]
public class BuoyOrb : ModOrbTemplate
{
    public Creature? Holder => Owner?.Creature ?? MonsterOrbPatch.OwnerState[this];

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
    }

    public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
    {
        var holder = Holder ?? throw new InvalidOperationException("BuoyOrb has no Holder set");

        ActivatePassive();
        if (IsFriendly(holder))
        {
            await CreatureCmd.GainBlock(holder, PassiveVal, ValueProp.Unpowered, null);
        }
        else
        {
            await CreatureCmd.Damage(choiceContext, holder, PassiveVal, ValueProp.Unpowered, holder);
        }
    }

    public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext choiceContext)
    {
        var holder = Holder ?? throw new InvalidOperationException("BuoyOrb has no Holder set");
        var boost = this.GetOrCreateCapability<OrbBoostCapability>();
        var totalEvoke = EvokeVal + boost.BonusEvoke;

        PlayEvokeSfx();
        if (IsFriendly(holder))
        {
            await CreatureCmd.GainBlock(holder, totalEvoke, ValueProp.Unpowered, null);
        }
        else
        {
            await CreatureCmd.Damage(choiceContext, holder, totalEvoke, ValueProp.Unpowered, holder);
        }
        return [holder];
    }

    private static bool IsFriendly(Creature c) => c.IsPlayer || c.PetOwner != null;
}
