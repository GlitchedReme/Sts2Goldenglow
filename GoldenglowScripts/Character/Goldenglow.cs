using System.Reflection;
using Godot;
using Goldenglow;
using Goldenglow.Card;
using Goldenglow.Potion;
using Goldenglow.Relic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Godot;
using STS2RitsuLib.Scaffolding.Visuals.StateMachine;

namespace Goldenglow.Character;

[RegisterCharacter]
public class Goldenglow : ModCharacterTemplate<GoldenglowCardPool, GoldenglowRelicPool, GoldenglowPotionPool>
{
    public override Color NameColor => new(1f, 0.14f, 0.321f);
    public override Color EnergyLabelOutlineColor => new(0.71f, 0.014f, 0.16f);
    public override Color MapDrawingColor => new(1f, 0.14f, 0.321f);

    public override CharacterGender Gender => CharacterGender.Feminine;

    public override int StartingHp => 75;
    public override int StartingGold => 99;
    public override int BaseOrbSlotCount => 3;

    public override CharacterAssetProfile AssetProfile => CharacterAssetProfiles.Merge(
        CharacterAssetProfiles.Defect(),
        new(
            Scenes: new(
            // 战斗模型场景
            VisualsPath: "res://Goldenglow/scene/gg_combat.tscn",
            // 能量表盘场景
            EnergyCounterPath: "res://Goldenglow/scene/gg_energy.tscn",
            // 商店人物场景
            MerchantAnimPath: "res://Goldenglow/scene/gg_merchant.tscn",
            // 篝火休息场景
            RestSiteAnimPath: "res://Goldenglow/scene/gg_restsite.tscn"
            ),
            Ui: new(
            // 人物头像
            IconTexturePath: "res://Goldenglow/image/character/icon.png",
            // 游戏左上角头像场景
            IconPath: "res://Goldenglow/scene/gg_icon.tscn",
            // 人物选择背景
            CharacterSelectBgPath: "res://Goldenglow/scene/gg_char_select.tscn",
            // 人物选择图标
            CharacterSelectIconPath: "res://Goldenglow/image/character/select.png",
            // 人物选择图标-锁定
            CharacterSelectLockedIconPath: "res://Goldenglow/image/character/select_unlocked.png",
            // 人物选择过渡
            CharacterSelectTransitionPath: "res://Goldenglow/image/character/translation.tres",
            // 地图标记
            MapMarkerPath: "res://Goldenglow/image/character/icon.png"
            ),
        Audio: new(
            CharacterSelectSfx: "event:/goldenglow/sfx/char_select"
        ),
        // Vfx: new(...),
        Multiplayer: new(
            ArmPointingTexturePath: "res://Goldenglow/image/character/multiplayer_hand_pointer.png",
            ArmPaperTexturePath: "res://Goldenglow/image/character/multiplayer_hand_paper.png",
            ArmRockTexturePath: "res://Goldenglow/image/character/multiplayer_hand_rock.png",
            ArmScissorsTexturePath: "res://Goldenglow/image/character/multiplayer_hand_scissors.png"
        ))
    );

    public override float AttackAnimDelay => 0f;
    public override float CastAnimDelay => 0f;

    public override bool RequiresEpochAndTimeline => false;

    protected override NCreatureVisuals? TryCreateCreatureVisuals() => RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.Scenes!.VisualsPath!);

    public override List<string> GetArchitectAttackVfx() => [
        "vfx/vfx_attack_slash",
        "vfx/vfx_attack_lightning",
        "vfx/vfx_attack_slash",
        "vfx/vfx_attack_lightning",
        "vfx/vfx_attack_lightning"
    ];

#pragma warning disable CS0672
    protected override IEnumerable<StartingDeckEntry> StartingDeckEntries => [
        StartingDeckEntry.Of<Strike_Goldenglow>(4),
        StartingDeckEntry.Of<Defend_Goldenglow>(4),
        StartingDeckEntry.Of<ElectrostaticSpark>(),
        StartingDeckEntry.Of<PreciseDiversion>()
    ];

    protected override IEnumerable<Type> StartingRelicTypes => [
        typeof(InsulatingComb)
    ];

#pragma warning restore CS0672

    private static readonly FieldInfo CurrentStateField = AccessTools.Field(typeof(CreatureAnimator), "_currentState");

    protected override CreatureAnimator? SetupCustomCreatureAnimator(MegaSprite controller)
    {
        AnimState idle1 = new("Idle", isLooping: true);
        AnimState idle2Start = new("Skill2_Start");
        AnimState idle2 = new("Skill2_Idle", isLooping: true);

        AnimState attack01 = new("Attack_Start");
        AnimState attack02 = new("Attack");
        AnimState attack03 = new("Attack_End");

        AnimState cast = new("Skill2_Loop");
        AnimState attack = new("Skill2_Loop");
        AnimState die = new("Die");
        AnimState relaxed = new("Idle", isLooping: true);

        attack01.NextState = attack02;
        attack02.NextState = attack03;
        attack03.NextState = idle2Start;
        idle2Start.NextState = idle2;

        cast.NextState = idle2;
        attack.NextState = idle2;
        // hurt.NextState = idle;
        relaxed.AddBranch("Idle", idle1);

        CreatureAnimator creatureAnimator = new(idle1, controller);
        creatureAnimator.AddAnyState("Idle", idle1);
        creatureAnimator.AddAnyState("Dead", die);
        // creatureAnimator.AddAnyState("Hit", hurt);
        creatureAnimator.AddAnyState("Attack", attack01, () => CurrentStateField.GetValue(creatureAnimator) is AnimState currentState && (currentState.Id == idle1.Id || currentState.Id == attack01.Id || currentState.Id == attack02.Id || currentState.Id == attack03.Id));
        creatureAnimator.AddAnyState("Attack", attack, () => CurrentStateField.GetValue(creatureAnimator) is AnimState currentState && (currentState.Id == idle2.Id || currentState.Id == attack.Id));
        creatureAnimator.AddAnyState("Cast", cast);
        creatureAnimator.AddAnyState("Relaxed", relaxed);
        return creatureAnimator;
    }

    protected override ModAnimStateMachine? SetupCustomMerchantAnimationStateMachine(Node merchantRoot, CharacterModel character)
    {
        var spine = new MegaSprite(merchantRoot.GetChild(0));
        return ModAnimStateMachineBuilder.Create()
            .AddState("Idle", loop: true)
            .Done()
            .BuildSpine(spine);
    }

    protected override ModAnimStateMachine? SetupCustomRestSiteAnimationStateMachine(Node restSiteRoot, CharacterModel character)
    {
        var spine = new MegaSprite(restSiteRoot.GetChild(0));
        return ModAnimStateMachineBuilder.Create()
            .AddState("Sit", loop: true)
            .Done()
            .BuildSpine(spine);
    }
}
