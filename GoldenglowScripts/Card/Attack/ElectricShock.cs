using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using Goldenglow.Core;
using MegaCrit.Sts2.Core.HoverTips;
using Goldenglow.Patch;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class ElectricShock() : AbstractGoldenglowCard(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy), IHovertipShownInInspectOnly
{
    public IEnumerable<IHoverTip> HoverTipsShownInInspectOnly => [
        GoldenglowUtils.CreateReference("Namie")
    ];

    protected override HashSet<CardTag> CanonicalTags => [GoldenglowTags.Static];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Buff", 3),
        ModCardVars.ComputedDamage("Damage", 3, card => card!.DynamicVars["Damage"].BaseValue + GoldenglowCmd.GetStaticStacks(card!) * card!.DynamicVars["Buff"].BaseValue, ValueProp.Move),
        ModCardVars.ComputedBlock("Block", 3, card => card!.DynamicVars["Block"].BaseValue + GoldenglowCmd.GetStaticStacks(card!) * card!.DynamicVars["Buff"].BaseValue)
    ];

    public override bool GainsBlock => true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [GoldenglowUtils.Static];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.ComputeDynamicValue("Damage"))
            .FromCardCompat(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.ComputeDynamicValue("Block"), ValueProp.Move, cardPlay);
        await GoldenglowCmd.ApplyStatic(cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Buff"].UpgradeValueBy(1);
        DynamicVars["Damage"].UpgradeValueBy(1);
        DynamicVars["Block"].UpgradeValueBy(1);
    }
}
