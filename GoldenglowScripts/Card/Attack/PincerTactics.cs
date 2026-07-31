using Goldenglow.Core;
using MegaCrit.Sts2.Core.HoverTips;
using Goldenglow.Ui;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using Goldenglow.Patch;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class PincerTactics() : AbstractGoldenglowCard(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy), IHovertipShownInInspectOnly
{
    public IEnumerable<IHoverTip> HoverTipsShownInInspectOnly => [
        GoldenglowUtils.CreateReference("Watersnake (水蛇)")
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(7, ValueProp.Move),
        new RepeatVar(2)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [GoldenglowUtils.Transfer];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(DynamicVars.Repeat.IntValue).FromCardCompat(this, cardPlay).Targeting(cardPlay.Target!).Execute(choiceContext);

        var target = cardPlay.Target!;
        int playerOrbs = Owner.PlayerCombatState!.OrbQueue.Orbs.Count;
        int targetOrbs = MonsterOrbManager.MonsterOrbManagerState[target]?.GetOrbs().Count ?? 0;

        if (playerOrbs > targetOrbs)
            await GoldenglowOrbCmd.TransferOrbs(Owner, Owner.Creature, target, playerOrbs);
        else if (targetOrbs > playerOrbs)
            await GoldenglowOrbCmd.TransferOrbs(Owner, target, Owner.Creature, targetOrbs);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}
