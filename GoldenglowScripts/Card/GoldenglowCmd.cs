using Goldenglow.Capabilities;
using Goldenglow.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Models.Capabilities;

namespace Goldenglow.Card;

public static class GoldenglowCmd
{
    public static async Task<List<CardModel>> Attract(PlayerChoiceContext choiceContext, Player player, CardModel card)
    {
        var attracted = new List<CardModel>();
        var discardPile = PileType.Discard.GetPile(player);
        int amount = (int)card.DynamicVars["Goldenglow_Attract"].BaseValue;
        int count = Math.Min(amount, discardPile.Cards.Count);

        for (int i = 0; i < count; i++)
        {
            var c = discardPile.Cards[discardPile.Cards.Count - 1];
            await CardPileCmd.Add(c, PileType.Hand);
            attracted.Add(c);
            await NotifyAttracted(choiceContext, player);
        }

        foreach (var c in attracted.ToArray())
        {
            if (c is ICardOnAttracted cardOnAttracted)
            {
                await cardOnAttracted.OnAttracted(choiceContext, player);
            }
        }

        return attracted;
    }

    internal static async Task NotifyAttracted(PlayerChoiceContext choiceContext, Player player)
    {
        foreach (var relic in player.Relics)
        {
            if (relic is IOnCardAttracted onCardAttracted)
                await onCardAttracted.OnCardAttracted(choiceContext, player);
        }
    }

    public static async Task Pulse(Player player)
    {
        var hittableEnemies = player.Creature.CombatState?.HittableEnemies;
        if (hittableEnemies != null && hittableEnemies.Count != 0)
        {
            var amount = GoldenglowSingleton.GetPulse(player);
            var target = player.RunState.Rng.CombatTargets.NextItem(hittableEnemies);
            if (target != null)
            {
                await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), target, amount, ValueProp.Unpowered, player.Creature, null, null);
                GoldenglowSingleton.IncrementPulse(player);
            }
        }
    }

    public static void ApplyStatic(CardModel card)
    {
        var cap = card.GetOrCreateCapability<StaticCapability>();
        cap.Increment();
    }

    public static int GetStaticStacks(CardModel? card)
    {
        if (card == null) return 0;
        return card.GetOrCreateCapability<StaticCapability>().TimesPlayedThisTurn;
    }

    /// <summary>
    /// Finds a card matching <paramref name="predicate"/> and draws it.
    /// Searches draw pile first. If not found, shuffles discard into draw and tries again.
    /// Returns the drawn card, or null if no match exists.
    /// </summary>
    public static async Task<CardModel?> DrawFiltered(PlayerChoiceContext choiceContext, Player player, Func<CardModel, bool> predicate)
    {
        var drawPile = PileType.Draw.GetPile(player);

        CardModel? match = drawPile.Cards.LastOrDefault(predicate);
        if (match == null)
        {
            var discardPile = PileType.Discard.GetPile(player);
            match = discardPile.Cards.LastOrDefault(predicate);
            if (match == null) return null;

            await CardPileCmd.Shuffle(choiceContext, player);
        }

        await CardPileCmd.Add(match, PileType.Draw, CardPilePosition.Top);
        return (await CardPileCmd.Draw(choiceContext, 1, player)).FirstOrDefault();
    }
}
