using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Goldenglow.Power;

[RegisterPower]
public sealed class BlinkPower : AbstractGoldenglowPower
{
    public class InternalData
    {
        public List<CardModel> _exiled = [];
    }

    public override PowerStackType StackType => PowerStackType.Single;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new StringVar("Exiled", "")
    ];

    protected override object? InitInternalData() => new InternalData();

    public void StoreExiled(List<CardModel> cards)
    {
        GetInternalData<InternalData>()._exiled.AddRange(cards);
        ((StringVar)DynamicVars["Exiled"]).StringValue = string.Join(", ", cards.Select(c => $"[gold]{c.Title}[/gold]"));
    }

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (Owner.Player != player) return;
        foreach (var card in GetInternalData<InternalData>()._exiled)
            await CardPileCmd.Add(card, PileType.Hand);
        GetInternalData<InternalData>()._exiled.Clear();
        await PowerCmd.Remove(this);
    }
}
