using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

using STS2RitsuLib.Interop.AutoRegistration;

using Goldenglow.Power;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class ScatterSparks() : AbstractGoldenglowCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("GainBlock", 6),
        new CardsVar(2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ScatterSparksBlockPower>(choiceContext, Owner.Creature, DynamicVars["GainBlock"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<ScatterSparksDiscardPower>(choiceContext, Owner.Creature, DynamicVars.Cards.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["GainBlock"].UpgradeValueBy(2);
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
