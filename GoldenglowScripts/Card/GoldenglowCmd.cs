using Godot;
using Goldenglow.Capabilities;
using Goldenglow.Core;
using Goldenglow.Vfx;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Models.Capabilities;

namespace Goldenglow.Card;

public static class GoldenglowCmd
{
    public static async Task<List<CardModel>> Attract(PlayerChoiceContext choiceContext, Player player, int amount = 1)
    {
        var attracted = new List<CardModel>();
        var discardPile = PileType.Discard.GetPile(player);
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

    public static async Task<List<CardModel>> Attract(PlayerChoiceContext choiceContext, Player player, CardModel card) => await Attract(choiceContext, player, (int)card.DynamicVars["Goldenglow_Attract"].BaseValue);

    internal static async Task NotifyAttracted(PlayerChoiceContext choiceContext, Player player)
    {
        foreach (var relic in player.Relics)
        {
            if (relic is IOnCardAttracted onCardAttracted)
                await onCardAttracted.OnCardAttracted(choiceContext, player);
        }
    }

    public static async Task Pulse(Player player, CardModel? cardSource, CardPlay? cardPlay, Creature? hit = null)
    {
        var hittableEnemies = player.Creature.CombatState?.HittableEnemies;
        if (hittableEnemies != null && hittableEnemies.Count != 0)
        {
            var amount = GoldenglowSingleton.GetPulse(player);
            var target = hit ?? player.RunState.Rng.CombatTargets.NextItem(hittableEnemies);
            if (target != null)
            {
#if STS2_AT_LEAST_110_0
                await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), target, amount, ValueProp.Unpowered, player.Creature, cardSource, cardPlay);
#else
                await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), target, amount, ValueProp.Unpowered, player.Creature, cardSource);
#endif
                var vfx = NSweepingBeamVfx.Create();
                var pos = target.GetCreatureNode()?.VfxSpawnPosition + Vector2.Right.Rotated(Random.Shared.NextSingle() * MathF.PI * 2) * (MathF.Sqrt(Random.Shared.NextSingle()) * 80f);
                if (vfx != null)
                    GoldenglowUtils.PlayVfx(target, vfx, pos);
                SfxCmd.Play("event:/sfx/characters/defect/defect_lightning_evoke", 1.5f);
                GoldenglowSingleton.IncrementPulse(player);
                await Cmd.Wait(0.1f);
            }
        }
    }

    public static async Task ApplyStatic(CardModel card, bool wait = true)
    {
        var cap = card.GetOrCreateCapability<StaticCapability>();
        cap.Increment();

        var cardNode = NCard.FindOnTable(card);
        if (cardNode != null)
        {
            StaticIncrementVfx.Create(cardNode);
            SfxCmd.Play("event:/sfx/characters/defect/defect_lightning_passive");
            if (wait)
                await Cmd.CustomScaledWait(0.5f, 0.6f);
        }
    }

    public static int GetStaticStacks(CardModel? card)
    {
        if (card == null) return 0;
        return card.TryGetCapability<StaticCapability>(out var cap) ? cap.TimesPlayedThisTurn : 0;
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

        await CardPileCmd.Add(match, PileType.Draw, CardPilePosition.Top, skipVisuals: true);
        return (await CardPileCmd.Draw(choiceContext, 1, player)).FirstOrDefault();
    }
}
