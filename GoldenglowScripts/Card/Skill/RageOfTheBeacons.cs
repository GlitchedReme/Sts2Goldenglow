using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Combat.CardTargeting;
using STS2RitsuLib.Interop.AutoRegistration;
using Goldenglow.Core;
using Goldenglow.Orb;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Cards.DynamicVars;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class RageOfTheBeacons() : AbstractGoldenglowCard(0, CardType.Skill, CardRarity.Rare, CustomTargetType.Anyone)
{
    protected override HashSet<CardTag> CanonicalTags => [GoldenglowTags.Static];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Computed("Buoy", 1, card => card == null ? 0 : card.DynamicVars["Buoy"].BaseValue + GoldenglowCmd.GetStaticStacks(card))
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [GoldenglowUtils.Static, HoverTipFactory.FromOrb<BuoyOrb>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var count = (int)DynamicVars.ComputeDynamicValue("Buoy");
        for (var i = 0; i < count; i++)
        {
            await GoldenglowOrbCmd.ChannelBuoy(Owner, cardPlay.Target);
        }
        await GoldenglowCmd.ApplyStatic(cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
        // DynamicVars["Buoy"].UpgradeValueBy(1);
    }
}
