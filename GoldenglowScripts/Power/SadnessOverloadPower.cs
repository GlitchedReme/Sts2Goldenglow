using System.Threading.Tasks;
using Goldenglow.Capabilities;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Power;

[RegisterPower]
public sealed class SadnessOverloadPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner != base.Owner.Player) return;
        if (!card.TryGetCapability<StaticCapability>(out var cap)) return;
        cap.Increment();
        Flash();
        await Task.CompletedTask;
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (card.Owner != base.Owner.Player) return;
        if (oldPileType == PileType.Hand) return; // already in hand, not entering

        // Check if card is now in hand
        if (!PileType.Hand.GetPile(base.Owner.Player).Cards.Contains(card)) return;
        if (!card.TryGetCapability<StaticCapability>(out var cap)) return;
        cap.Increment();
        Flash();
        await Task.CompletedTask;
    }
}
