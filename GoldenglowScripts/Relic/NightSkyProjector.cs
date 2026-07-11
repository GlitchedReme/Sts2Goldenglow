using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
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
        IconPath: "res://Goldenglow/images/relics/NightSkyProjector.png",
        IconOutlinePath: "res://Goldenglow/images/relics/NightSkyProjector.png",
        BigIconPath: "res://Goldenglow/images/relics/NightSkyProjector.png"
    );

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    public override bool TryModifyCardRewardOptions(Player player, List<CardCreationResult> cardRewardOptions, CardCreationOptions creationOptions)
    {
        var pool = player.Character.CardPool
            .GetUnlockedCards(UnlockState.all, player.RunState.CardMultiplayerConstraint)
            .Where(c => c.Keywords.Contains(CardKeyword.Exhaust))
            .ToList();
        if (pool.Count == 0) return false;

        var rng = player.PlayerRng.Rewards;
        var cards = CardFactory.GetDistinctForCombat(player, pool, 1, rng).ToList();
        if (cards.Count == 0) return false;

        cardRewardOptions.Add(new CardCreationResult(cards[0]));
        return true;
    }
}