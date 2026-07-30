using Goldenglow.Card;
using Goldenglow.Patch;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Power;

[RegisterPower]
public sealed class SubharmonicResonancePower : AbstractGoldenglowPower, IPowerCustomTextProvider
{
    public string CustomText => $"{Counter}/2";

    private int Counter
    {
        get => (int)DynamicVars["Counter"].BaseValue;
        set
        {
            DynamicVars["Counter"].BaseValue = value;
            InvokeDisplayAmountChanged();
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Counter", 0),
    ];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player) return;

        Counter++;
        if (Counter >= Amount)
        {
            await GoldenglowCmd.Pulse(Owner.Player, null, null);
            Flash();
            Counter -= Amount;
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        Counter = 0;
    }
}
