using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Goldenglow.Core;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Cards.DynamicVars;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class NewLife() : AbstractGoldenglowCard(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Threshold", 3),
        new DynamicVar("DamageBonus", 20),
        ModCardVars.ComputedDamage("Damage", 0,
            card => GoldenglowCmd.GetStaticStacks(card!) * card!.DynamicVars["DamageBonus"].BaseValue, ValueProp.Move),
    ];

    protected override bool IsPlayable => GoldenglowCmd.GetStaticStacks(this) >= DynamicVars["Threshold"].BaseValue;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [GoldenglowUtils.Static];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        GoldenglowCmd.ApplyStatic(cardPlay.Card);

        await DamageCmd.Attack(((ComputedDynamicVar)DynamicVars["Damage"]).Calculate())
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Threshold"].UpgradeValueBy(-1);
    }
}
