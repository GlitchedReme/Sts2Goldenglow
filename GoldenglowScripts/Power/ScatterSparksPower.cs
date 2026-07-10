using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;

using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Power;

/// <summary>
/// Whenever the owner shuffles, gain block and discard from the draw pile.
/// Amount = block per shuffle (stacks). DiscardCount = cards to discard.
/// 每当洗牌时，获得格挡并丢弃抽牌堆顶的牌。
/// </summary>
[RegisterPower]
public sealed class ScatterSparksPower : ModPowerTemplate
{
    public decimal DiscardCount { get; set; } = 1;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterShuffle(PlayerChoiceContext choiceContext, Player shuffler)
    {
        if (shuffler != base.Owner.Player) return;

        await CreatureCmd.GainBlock(base.Owner, Amount, ValueProp.Move, null);

        var drawPile = PileType.Draw.GetPile(shuffler);
        int toDiscard = Math.Min((int)DiscardCount, drawPile.Cards.Count);
        for (int i = 0; i < toDiscard; i++)
            await CardCmd.Discard(choiceContext, drawPile.Cards[^1]);
    }
}
