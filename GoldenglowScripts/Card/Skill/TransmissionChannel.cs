using Goldenglow.Core;
using Goldenglow.Ui;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Combat.CardTargeting;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class TransmissionChannel() : AbstractGoldenglowCard(0, CardType.Skill, CardRarity.Uncommon, CustomTargetType.Anyone)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [GoldenglowUtils.Transfer];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target!;

        if (target.IsPlayer)
        {
            if (target.Player?.PlayerCombatState?.OrbQueue.Orbs is not { Count: > 0 }) return;
            var enemies = CombatState?.HittableEnemies;
            if (enemies is not { Count: > 0 }) return;
            var enemy = Owner.RunState.Rng.CombatTargets.NextItem(enemies);
            if (enemy == null) return;
            await GoldenglowOrbCmd.TransferOrbs(target, enemy, 1);
        }
        else
        {
            var count = MonsterOrbManager.MonsterOrbManagerState[target]?.GetOrbs().Count ?? 0;
            if (count > 0)
                await GoldenglowOrbCmd.TransferOrbs(target, Owner.Creature, count);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}