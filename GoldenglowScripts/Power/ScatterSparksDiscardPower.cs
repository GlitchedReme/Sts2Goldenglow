using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Power;

[RegisterPower]
public sealed class ScatterSparksDiscardPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterShuffle(PlayerChoiceContext choiceContext, Player shuffler)
    {
        if (shuffler != Owner.Player) return;

        Flash();
        var drawPile = PileType.Draw.GetPile(shuffler);
        int toDiscard = Math.Min(Amount, drawPile.Cards.Count);
        for (int i = 0; i < toDiscard; i++)
            await CardCmd.Discard(choiceContext, drawPile.Cards[^1]);
    }
}
