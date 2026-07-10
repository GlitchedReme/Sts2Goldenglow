using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Goldenglow.Card;

public interface ICardOnAttracted
{
    Task OnAttracted(PlayerChoiceContext choiceContext, Player player);
}