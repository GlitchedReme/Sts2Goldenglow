using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Goldenglow.Core;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class Renovation() : AbstractGoldenglowCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override HashSet<CardTag> CanonicalTags => [GoldenglowTags.Static];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Buff", 8),
        ModCardVars.ComputedBlock("Block", 8,
            card => DynamicVars["Block"].BaseValue + GoldenglowCmd.GetStaticStacks(card!) * card!.DynamicVars["Buff"].BaseValue)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.ComputeDynamicValue("Block"), ValueProp.Move, cardPlay);
        await GoldenglowCmd.ApplyStatic(cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        // DynamicVars["Buff"].UpgradeValueBy(2);
        // DynamicVars["ExtraBlock"].UpgradeValueBy(2);
    }
}
