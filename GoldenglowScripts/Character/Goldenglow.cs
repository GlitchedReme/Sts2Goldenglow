using Godot;
using Goldenglow.Card;
using Goldenglow.Potion;
using Goldenglow.Relic;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Godot;

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
        // 音效、特效等后续补充
        Audio: new(
            CharacterSelectSfx: "event:/goldenglow/sfx/char_select"
        )
        // Vfx: new(...),
        // Multiplayer: new(...)
        )
    );

    public override float AttackAnimDelay => 0f;
    public override float CastAnimDelay => 0f;

    public override bool RequiresEpochAndTimeline => false;

    protected override NCreatureVisuals? TryCreateCreatureVisuals() => RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.Scenes!.VisualsPath!);

    public override List<string> GetArchitectAttackVfx() => [
        "vfx/vfx_attack_slash",
        "vfx/vfx_heavy_blunt",
        "vfx/vfx_attack_blunt",
        "vfx/vfx_bloody_impact",
        "vfx/vfx_rock_shatter"
    ];

#pragma warning disable CS0672
    protected override IEnumerable<StartingDeckEntry> StartingDeckEntries => [
        StartingDeckEntry.Of<Strike_Goldenglow>(4),
        StartingDeckEntry.Of<Defend_Goldenglow>(3),
        StartingDeckEntry.Of<ElectrostaticSpark>(),
        StartingDeckEntry.Of<PreciseDiversion>()
    ];

    protected override IEnumerable<Type> StartingRelicTypes => [
        typeof(InsulatingComb)
    ];

#pragma warning restore CS0672

    protected override CreatureAnimator? SetupCustomCreatureAnimator(MegaSprite controller)
    {
        // 设定动画名和是否循环播放
        AnimState idle = new("Skill2_Idle", isLooping: true);
        AnimState cast = new("Skill2_Loop");
        AnimState attack = new("Skill2_Loop");
        AnimState die = new("Die");
        AnimState relaxed = new("Idle", isLooping: true);

        cast.NextState = idle;
        attack.NextState = idle;
        // hurt.NextState = idle;
        relaxed.AddBranch("Idle", idle);

        CreatureAnimator creatureAnimator = new(idle, controller);
        creatureAnimator.AddAnyState("Idle", idle);
        creatureAnimator.AddAnyState("Dead", die);
        // creatureAnimator.AddAnyState("Hit", hurt);
        creatureAnimator.AddAnyState("Attack", attack);
        creatureAnimator.AddAnyState("Cast", cast);
        creatureAnimator.AddAnyState("Relaxed", relaxed);
        return creatureAnimator;
    }
}
