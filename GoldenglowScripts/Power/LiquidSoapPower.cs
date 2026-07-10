using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Power;

/// <summary>
/// After every 3 Basic or Common cards played by the owner, draw cards.
/// Amount = cards to draw per 3-card cycle (stacks).
/// </summary>
[RegisterPower]
public sealed class LiquidSoapPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private int _counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner.Player) return;
        if (cardPlay.Card.Rarity != CardRarity.Basic && cardPlay.Card.Rarity != CardRarity.Common) return;

        _counter++;
        if (_counter >= 3)
        {
            _counter = 0;
            await CardPileCmd.Draw(choiceContext, Amount, base.Owner.Player);
            Flash();
        }
    }
}
