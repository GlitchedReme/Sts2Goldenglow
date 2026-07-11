using Goldenglow.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class ChargeRelease() : AbstractGoldenglowCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(7, ValueProp.Move),
        new CardsVar(1),
        new DynamicVar("StaticDamage", 7),
        new DynamicVar("StaticDraw", 1)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [GoldenglowUtils.Static];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var staticStacks = GoldenglowCmd.GetStaticStacks(cardPlay.Card);
        var damage = DynamicVars.Damage.BaseValue + staticStacks * DynamicVars["StaticDamage"].BaseValue;
        var draw = (int)DynamicVars.Cards.BaseValue + staticStacks * (int)DynamicVars["StaticDraw"].BaseValue;

        await CreatureCmd.Damage(choiceContext, cardPlay.Target!, damage, ValueProp.Unpowered, Owner.Creature);
        await CardPileCmd.Draw(choiceContext, draw, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
        DynamicVars["StaticDamage"].UpgradeValueBy(1);
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
