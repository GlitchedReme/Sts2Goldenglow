using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class GradualClippers() : AbstractGoldenglowCard(6, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    private int _turnReduction;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(28, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
    }

    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        if (card != this) return;
        if (IsClone) return;

        var state = Owner?.Creature?.CombatState;
        if (state == null) return;

        _turnReduction = CombatManager.Instance.History.CardPlaysFinished
            .Count(e => e.HappenedThisTurn(state));
        if (_turnReduction > 0)
            EnergyCost.AddThisCombat(-_turnReduction);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner) return;

        _turnReduction++;
        EnergyCost.AddThisCombat(-1);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;

        if (_turnReduction > 0)
            EnergyCost.AddThisCombat(_turnReduction);
        _turnReduction = 0;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        EnergyCost.UpgradeBy(-1);
    }
}
