using Goldenglow.Card;
using Goldenglow.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Relic;

[RegisterRelic(typeof(GoldenglowRelicPool))]
public class WindChime : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Counter", 0)
    ];

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Goldenglow/images/relics/WindChime.png",
        IconOutlinePath: "res://Goldenglow/images/relics/WindChime.png",
        BigIconPath: "res://Goldenglow/images/relics/WindChime.png"
    );

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [GoldenglowUtils.Attract];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;

        DynamicVars["Counter"].BaseValue++;
        if (DynamicVars["Counter"].BaseValue < 3) return;
        DynamicVars["Counter"].BaseValue = 0;

        var discardPile = PileType.Discard.GetPile(Owner);
        if (discardPile.Cards.Count == 0) return;

        var c = discardPile.Cards[^1];
        await CardPileCmd.Add(c, PileType.Hand);
        await GoldenglowCmd.NotifyAttracted(choiceContext, Owner);
    }
}