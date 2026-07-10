using Goldenglow.Ui;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Combat.CardTargeting;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class StayAway() : AbstractGoldenglowCard(0, CardType.Skill, CardRarity.Common, CustomTargetType.Anyone)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target!;
        if (target.IsPlayer)
        {
            var player = target.Player;
            if (player?.PlayerCombatState == null) return;
            var orbQueue = player.PlayerCombatState.OrbQueue;
            var ctx = new ThrowingPlayerChoiceContext();
            for (int i = 0; i < orbQueue.Orbs.Count; i++)
                await OrbCmd.Passive(ctx, orbQueue.Orbs[i], null);
        }
        else
        {
            var mgr = MonsterOrbManager.MonsterOrbManagerState[target];
            if (mgr == null) return;
            var orbs = mgr.GetOrbs();
            var ctx = new ThrowingPlayerChoiceContext();
            for (int i = 0; i < orbs.Count; i++)
                await OrbCmd.Passive(ctx, orbs[i], null);
        }
        if (IsUpgraded)
            await CardPileCmd.Draw(choiceContext, Owner);
    }

    protected override void OnUpgrade()
    {
    }
}
