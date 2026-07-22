global using Goldenglow.Utils;

using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Goldenglow.Utils;

public static class GoldenglowCompat
{
#if STS2_AT_LEAST_109_0
    public static AttackCommand FromCardCompat(this AttackCommand cmd, CardModel card, CardPlay? cardPlay)
        => cmd.FromCard(card, cardPlay);

    public static CardModel CreateDupeCompat(this CardModel card, Player newOwner)
        => card.CreateDupe(newOwner);
#else
    public static AttackCommand FromCardCompat(this AttackCommand cmd, CardModel card, CardPlay? cardPlay)
        => cmd.FromCard(card);

    public static CardModel CreateDupeCompat(this CardModel card, Player? newOwner)
        => card.CreateDupe();
#endif
}
