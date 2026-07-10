using Goldenglow.Core;
using MegaCrit.Sts2.Core.HoverTips;
using Goldenglow.Ui;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Combat.CardTargeting;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class TransmissionChannel() : AbstractGoldenglowCard(0, CardType.Skill, CardRarity.Uncommon, CustomTargetType.Anyone)
{

    protected override IEnumerable<DynamicVar> CanonicalVars => [
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [GoldenglowUtils.Transfer];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target!;
        int count = target.IsPlayer
            ? (target.Player?.PlayerCombatState?.OrbQueue.Orbs.Count ?? 0)
            : MonsterOrbManager.MonsterOrbManagerState[target]?.GetOrbs().Count ?? 0;
        if (count > 0)
            await GoldenglowOrbCmd.TransferOrbs(target, Owner.Creature, count);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
