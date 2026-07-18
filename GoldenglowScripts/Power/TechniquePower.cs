using Goldenglow.Patch;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Power;

[RegisterPower]
public sealed class TechniquePower : ModPowerTemplate, IPowerCustomTextProvider
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public string CustomText => Amount > 1 ? $"{TurnCounter}/{Amount}" : Amount.ToString();

    private CardModel? _storedCard;
    
    private int TurnCounter
    {
        get => (int)DynamicVars["TurnCounter"].BaseValue;
        set
        {
            DynamicVars["TurnCounter"].BaseValue = value;
            InvokeDisplayAmountChanged();
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Discard", 1),
        new DynamicVar("TurnCounter", 0),
        new StringVar("StoredCardName", _storedCard?.Title ?? "")
    ];

    public void StoreCard(CardModel card)
    {
        _storedCard = card;
        TurnCounter = 0;
        ((StringVar)DynamicVars["StoredCardName"]).StringValue = card.Title;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        if (_storedCard == null) return;

        TurnCounter++;
        if (TurnCounter >= Amount)
        {
            TurnCounter = 0;
            var clone = _storedCard.CreateDupe(player);
            await CardCmd.AutoPlay(choiceContext, clone, target: null);
            Flash();
        }
    }
}
