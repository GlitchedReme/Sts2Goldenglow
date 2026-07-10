using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace Goldenglow.Card;

public interface ICardOnGeneratedAsReward
{
    bool OnGeneratedAsReward(Player player, CardCreationOptions options);
}
