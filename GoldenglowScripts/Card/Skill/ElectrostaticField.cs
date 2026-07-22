using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Goldenglow.Power;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Combat.CardTargeting;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class ElectrostaticField() : AbstractGoldenglowCard(0, CardType.Skill, CardRarity.Rare, CustomTargetType.Anyone)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ElectrostaticFieldPower>(choiceContext, cardPlay.Target!, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
