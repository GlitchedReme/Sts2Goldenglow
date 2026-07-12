using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using Goldenglow.Power;
using STS2RitsuLib.Combat.CardTargeting;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class ChargeBalance() : AbstractGoldenglowCard(2, CardType.Power, CardRarity.Rare, CustomTargetType.Anyone)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<ChargeBalancePower>(2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        await PowerCmd.Apply<ChargeBalancePower>(choiceContext, cardPlay.Target, DynamicVars[nameof(ChargeBalancePower)].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
