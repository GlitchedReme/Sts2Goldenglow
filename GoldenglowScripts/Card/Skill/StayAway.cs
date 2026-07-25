using Goldenglow.Core;
using Goldenglow.Patch;
using Goldenglow.Ui;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Combat.CardTargeting;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class StayAway() : AbstractGoldenglowCard(0, CardType.Skill, CardRarity.Common, CustomTargetType.Anyone), IHovertipShownInInspectOnly
{
    public IEnumerable<IHoverTip> HoverTipsShownInInspectOnly => [
        GoldenglowUtils.CreateReference("Namie")
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target!;
        if (target.IsPlayer)
        {
            var player = target.Player;
            if (player?.PlayerCombatState == null) return;
            var orbQueue = player.PlayerCombatState.OrbQueue;
            for (int i = 0; i < orbQueue.Orbs.Count; i++)
                await GoldenglowOrbCmd.Passive(choiceContext, orbQueue.Orbs[i], null);
        }
        else
        {
            var mgr = MonsterOrbManager.MonsterOrbManagerState[target];
            if (mgr == null) return;
            var orbs = mgr.GetOrbs();
            for (int i = 0; i < orbs.Count; i++)
                await GoldenglowOrbCmd.Passive(choiceContext, orbs[i], null);
        }
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
