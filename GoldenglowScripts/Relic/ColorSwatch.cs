using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Unlocks;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Relic;

[RegisterRelic(typeof(GoldenglowRelicPool))]
public class ColorSwatch : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Shop;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Goldenglow/images/relics/ColorSwatch.png",
        IconOutlinePath: "res://Goldenglow/images/relics/ColorSwatch.png",
        BigIconPath: "res://Goldenglow/images/relics/ColorSwatch.png"
    );

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    public override async Task BeforeCombatStart()
    {
        var pool = Owner.Character.CardPool
            .GetUnlockedCards(UnlockState.all, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.Keywords.Contains(CardKeyword.Exhaust))
            .ToList();
        if (pool.Count == 0) return;

        var rng = Owner.PlayerRng.Rewards;
        var cards = CardFactory.GetDistinctForCombat(Owner, pool, 1, rng).ToList();
        if (cards.Count == 0) return;

        var card = cards[0];
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
    }
}