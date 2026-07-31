global using Goldenglow.Utils;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Goldenglow.Utils;

public static class GoldenglowCompat
{
#if STS2_AT_LEAST_110_0
    public static AttackCommand FromCardCompat(this AttackCommand cmd, CardModel card, CardPlay? cardPlay)
        => cmd.FromCard(card, cardPlay);

    public static CardModel CreateDupeCompat(this CardModel card, Player newOwner)
        => card.CreateDupe(newOwner);

    // ponytail: AttackContext.CreateAsync 第三参数 0.109 是 CardPlay，0.107 是 CardModel；统一封装
    public static Task<AttackContext> CreateAttackContextAsync(ICombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => AttackContext.CreateAsync(combatState, choiceContext, cardPlay);

    // ponytail: 0.109 的 Damage 带 CardPlay，0.107 不带；统一封装 + 自动 AddHit
    public static async Task DamageWithHit(this AttackContext ctx, PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature dealer, CardModel cardSource, CardPlay? cardPlay)
    {
        var results = await CreatureCmd.Damage(choiceContext, target, amount, props, dealer, cardSource, cardPlay);
        ctx.AddHit(results);
    }
#else
    public static AttackCommand FromCardCompat(this AttackCommand cmd, CardModel card, CardPlay? cardPlay)
        => cmd.FromCard(card);

    public static CardModel CreateDupeCompat(this CardModel card, Player? newOwner)
        => card.CreateDupe();

    public static Task<AttackContext> CreateAttackContextAsync(ICombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => AttackContext.CreateAsync(combatState, choiceContext, cardPlay.Card);

    public static async Task DamageWithHit(this AttackContext ctx, PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature dealer, CardModel cardSource, CardPlay? cardPlay)
    {
        var results = await CreatureCmd.Damage(choiceContext, target, amount, props, dealer, cardSource);
        ctx.AddHit(results);
    }
#endif
}
