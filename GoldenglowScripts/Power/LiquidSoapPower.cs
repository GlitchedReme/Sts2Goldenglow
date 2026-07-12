using Goldenglow.Patch;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Power;

[RegisterPower]
public sealed class LiquidSoapPower : ModPowerTemplate, IPowerCustomTextProvider
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public string CustomText => $"{Counter}/{Threshold}";

    private int Threshold => (int)DynamicVars["BaseCards"].BaseValue;

    private int Counter
    {
        get => (int)DynamicVars["Counter"].BaseValue;
        set => DynamicVars["Counter"].BaseValue = value;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("BaseCards", 3),
        new DynamicVar("Counter", 0),
    ];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player) return;
        if (cardPlay.Card.Rarity != CardRarity.Basic && cardPlay.Card.Rarity != CardRarity.Common) return;

        Counter++;
        InvokeDisplayAmountChanged();
        if (Counter >= Threshold)
        {
            await CardPileCmd.Draw(choiceContext, Amount, Owner.Player);
            Flash();
            InvokeDisplayAmountChanged();
            Counter = 0;
        }
    }
}
