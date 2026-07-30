using Goldenglow.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class ChargeRelease() : AbstractGoldenglowCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("StaticDamage", 7),
        new DynamicVar("StaticDraw", 1),
        ModCardVars.ComputedDamage("Damage", 7, card => DynamicVars["Damage"].BaseValue + GoldenglowCmd.GetStaticStacks(card!) * card!.DynamicVars["StaticDamage"].BaseValue, ValueProp.Move),
        ModCardVars.Computed("Cards", 1, card => (int)DynamicVars["Cards"].BaseValue + GoldenglowCmd.GetStaticStacks(card) * (int)DynamicVars["StaticDraw"].BaseValue)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [GoldenglowUtils.Static];

    protected override HashSet<CardTag> CanonicalTags => [GoldenglowTags.Static];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var damage = DynamicVars.ComputeDynamicValue("Damage");
        var draw = DynamicVars.ComputeDynamicValue("Cards");

        await CreatureCmd.Damage(choiceContext, cardPlay.Target!, damage, ValueProp.Unpowered, Owner.Creature);
        await CardPileCmd.Draw(choiceContext, draw, Owner);
        await GoldenglowCmd.ApplyStatic(cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Cards"].UpgradeValueBy(1);
    }
}
