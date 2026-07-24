using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Unlocks;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Relic;

[RegisterRelic(typeof(GoldenglowRelicPool))]
public class NightSkyProjector : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Goldenglow/image/relics/NightSkyProjector.png",
        IconOutlinePath: "res://Goldenglow/image/relics/NightSkyProjector.png",
        BigIconPath: "res://Goldenglow/image/relics/NightSkyProjector.png"
    );

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    public override bool TryModifyCardRewardOptions(Player player, List<CardCreationResult> cardRewardOptions, CardCreationOptions creationOptions)
    {
        var pool = player.Character.CardPool
            .GetUnlockedCards(UnlockState.all, player.RunState.CardMultiplayerConstraint)
            .Where(c => c.Keywords.Contains(CardKeyword.Exhaust))
            .ToList();
        if (pool.Count == 0) return false;

        var template = player.PlayerRng.Rewards.NextItem(pool);
        if (template == null) return false;

        var card = player.RunState.CreateCard(template, player);
        cardRewardOptions.Add(new CardCreationResult(card));
        return true;
    }
}