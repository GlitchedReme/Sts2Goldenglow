using Goldenglow.Card;
using Goldenglow.Core;
using Goldenglow.Patch;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Relic;

[RegisterRelic(typeof(GoldenglowRelicPool))]
public class TechniqueNotes : ModRelicTemplate, IRelicCustomTextProvider
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    public override bool ShowCounter => true;
    public override int DisplayAmount => (int)DynamicVars["Counter"].BaseValue;
    public string CustomText => $"{DisplayAmount}/2";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Counter", 0)
    ];

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Goldenglow/images/relics/TechniqueNotes.png",
        IconOutlinePath: "res://Goldenglow/images/relics/TechniqueNotes.png",
        BigIconPath: "res://Goldenglow/images/relics/TechniqueNotes.png"
    );

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [GoldenglowUtils.Pulse];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;

        DynamicVars["Counter"].BaseValue++;
        InvokeDisplayAmountChanged();
        if (DynamicVars["Counter"].BaseValue < 2) return;
        DynamicVars["Counter"].BaseValue = 0;

        await GoldenglowCmd.Pulse(Owner, null, null);
        InvokeDisplayAmountChanged();
    }
}