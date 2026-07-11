using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Goldenglow.Card;

public interface IOnCardAttracted
{
    Task OnCardAttracted(PlayerChoiceContext choiceContext, Player player);
}