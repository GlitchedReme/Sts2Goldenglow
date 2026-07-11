using Goldenglow.Ui;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Goldenglow.Orb;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.CardTargeting;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class PreciseDiversion() : AbstractGoldenglowCard(0, CardType.Skill, CardRarity.Basic, CustomTargetType.Anyone)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target!;
        await GoldenglowOrbCmd.ChannelBuoy(target, 1);

        if (IsUpgraded)
        {
            var orb = GetLastBuoyOrb(target);
            if (orb != null)
                await orb.Passive(choiceContext, null);
        }
    }

    private static OrbModel? GetLastBuoyOrb(Creature target)
    {
        if (target.IsPlayer)
        {
            var queue = target.Player?.PlayerCombatState?.OrbQueue;
            if (queue == null || queue.Orbs.Count == 0) return null;
            return queue.Orbs[^1];
        }

        var mgr = MonsterOrbManager.MonsterOrbManagerState[target];
        if (mgr == null) return null;
        var orbs = mgr.GetOrbs();
        return orbs.Count > 0 ? orbs[^1] : null;
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromOrb<BuoyOrb>()];

    protected override void OnUpgrade()
    {
    }
}