using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Card;

public abstract class AbstractGoldenglowCard(int cost, CardType type, CardRarity rarity, TargetType target) : ModCardTemplate(cost, type, rarity, target)
{
    public override CardAssetProfile AssetProfile => new(
        FramePath: Type switch
        {
            CardType.Attack => "res://Goldenglow/image/character/card_frame_attack_s.png",
            CardType.Power => "res://Goldenglow/image/character/card_frame_power_s.png",
            _ => "res://Goldenglow/image/character/card_frame_skill_s.png",
        }
    );
}
