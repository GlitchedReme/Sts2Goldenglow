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
    private int Reduction
    {
        get => (int)DynamicVars["Reduction"].BaseValue;
        set => DynamicVars["Reduction"].BaseValue = value;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(28, ValueProp.Move),
        new DynamicVar("Reduction", 0)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompat(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        if (card != this) return;
        if (IsClone) return;

        var state = Owner?.Creature?.CombatState;
        if (state == null) return;

        Reduction = CombatManager.Instance.History.CardPlaysFinished.Count(e => e.HappenedThisTurn(state));
        if (Reduction > 0)
            EnergyCost.AddThisCombat(-Reduction);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner) return;

        Reduction++;
        EnergyCost.AddThisCombat(-1);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;

        if (Reduction > 0)
            EnergyCost.AddThisCombat(Reduction);
        Reduction = 0;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        EnergyCost.UpgradeBy(-1);
    }
}
