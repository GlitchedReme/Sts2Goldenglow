using Goldenglow.Card;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Power;

[RegisterPower]
public sealed class SadnessOverloadPower : AbstractGoldenglowPower
{
    public override async Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;

        var cards = await GoldenglowCmd.Attract(choiceContext, player, Amount);
        foreach (var c in cards)
            c.GiveSingleTurnRetain();
        Flash();
    }
}
