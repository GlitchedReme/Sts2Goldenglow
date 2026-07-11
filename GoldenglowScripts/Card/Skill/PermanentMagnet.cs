using STS2RitsuLib.Combat.CardTargeting;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class PermanentMagnet() : AbstractGoldenglowCard(0, CardType.Skill, CardRarity.Uncommon, CustomTargetType.Anyone)
{

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Focus", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<FocusPower>(choiceContext, Owner.Creature, DynamicVars["Focus"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<FocusPower>(choiceContext, cardPlay.Target!, DynamicVars["Focus"].BaseValue, Owner.Creature, this);
    }
    protected override void OnUpgrade()
    {
        DynamicVars["Focus"].UpgradeValueBy(1);
    }
}
