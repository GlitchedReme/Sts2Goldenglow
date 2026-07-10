using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class FireExit() : AbstractGoldenglowCard(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Multipler", 2),
        ModCardVars.ComputedDamage("Damage", 2,
            card => card?.Owner == null ? 0 : CardPile.Get(PileType.Hand, card.Owner) == null ? 0 : PileType.Hand.GetPile(card.Owner).Cards.Count * (int)DynamicVars["Multipler"].BaseValue,
            ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(((ComputedDynamicVar)DynamicVars["Damage"]).Calculate())
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Multipler"].UpgradeValueBy(1);
    }
}
