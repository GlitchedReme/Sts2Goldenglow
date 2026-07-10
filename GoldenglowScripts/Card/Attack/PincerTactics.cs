using Goldenglow.Core;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;
using Goldenglow.Ui;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class PincerTactics() : AbstractGoldenglowCard(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move),
        new RepeatVar(2)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [GoldenglowUtils.Transfer];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        for (int i = 0; i < DynamicVars.Repeat.BaseValue; i++)
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target!).Execute(choiceContext);

        var target = cardPlay.Target!;
        int playerOrbs = Owner.PlayerCombatState!.OrbQueue.Orbs.Count;
        int targetOrbs = MonsterOrbManager.MonsterOrbManagerState[target]?.GetOrbs().Count ?? 0;

        if (playerOrbs > targetOrbs)
            await GoldenglowOrbCmd.TransferOrbs(Owner.Creature, target, playerOrbs - targetOrbs);
        else if (targetOrbs > playerOrbs)
            await GoldenglowOrbCmd.TransferOrbs(target, Owner.Creature, targetOrbs - playerOrbs);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}
