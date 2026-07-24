using Goldenglow.Orb;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Combat.CardTargeting;
using STS2RitsuLib.Interop.AutoRegistration;
using static Goldenglow.Card.GoldenglowOrbCmd;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class DroneGroup() : AbstractGoldenglowCard(2, CardType.Skill, CardRarity.Rare, CustomTargetType.Anyone)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Computed("CalculatedChannels", 0, card => card == null ? 0 : CombatManager.Instance.History.Entries.OfType<OrbChanneledEntry>().Count(e => e.Actor.Player == card.Owner && e.Orb is BuoyOrb) + CombatManager.Instance.History.Entries.OfType<MonsterOrbChanneledEntry>().Count(e => e.Actor.Player == card.Owner && e.Orb is BuoyOrb))
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromOrb<BuoyOrb>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var amount = DynamicVars.ComputeDynamicValue("CalculatedChannels");
        var target = cardPlay.Target;
        for (var i = 0; i < amount; i++)
        {
            await ChannelBuoy(Owner, target);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
