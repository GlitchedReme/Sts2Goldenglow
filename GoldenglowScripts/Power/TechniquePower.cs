using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Power;

[RegisterPower]
public sealed class TechniquePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    private CardModel? _storedCard;
    private int _turnCounter;

    public void StoreCard(CardModel card)
    {
        _storedCard = card;
        _turnCounter = 0;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        if (_storedCard == null) return;

        _turnCounter++;
        if (_turnCounter >= (int)Amount)
        {
            _turnCounter = 0;
            var clone = _storedCard.CreateClone();
            await CardCmd.AutoPlay(choiceContext, clone, target: null);
            Flash();
        }
    }
}
