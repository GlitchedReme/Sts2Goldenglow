using Godot;
using Goldenglow.Core;
using Goldenglow.Patch;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class SatelliteLanding() : AbstractGoldenglowCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies), IHovertipShownInInspectOnly
{
    public IEnumerable<IHoverTip> HoverTipsShownInInspectOnly => [
        GoldenglowUtils.CreateReference("From Internet (来自于网络)")
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(24, ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var pos = VfxCmd.GetSideCenterFloor(CombatSide.Enemy, CombatState!);
        if (pos is Vector2 p)
        {
            var nLargeMagicMissileVfx = NLargeMagicMissileVfx.Create(p, new Color("50b598"));
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(nLargeMagicMissileVfx);
            await Cmd.Wait(nLargeMagicMissileVfx?.WaitTime ?? 0f);
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCardCompat(this, cardPlay).TargetingAllOpponents(CombatState!).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6);
    }
}
