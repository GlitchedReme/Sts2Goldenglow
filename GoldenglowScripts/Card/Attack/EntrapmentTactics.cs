using Goldenglow.Orb;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;
using Goldenglow.Patch;
using Goldenglow.Core;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class EntrapmentTactics() : AbstractGoldenglowCard(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies), IHovertipShownInInspectOnly
{
    public IEnumerable<IHoverTip> HoverTipsShownInInspectOnly => [
        GoldenglowUtils.CreateReference("Watersnake (水蛇)")
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5, ValueProp.Move),
        new DynamicVar("Buoy", 1)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromOrb<BuoyOrb>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).TargetingAllOpponents(CombatState!).Execute(choiceContext);

        int count = (int)DynamicVars["Buoy"].BaseValue;
        for (int k = 0; k < count; k++)
            await OrbCmd.Channel<BuoyOrb>(choiceContext, Owner);
        for (int i = 0; i < CombatState!.Enemies.Count; i++)
        {
            var e = CombatState.Enemies[i];
            if (!e.IsDead && !e.IsPlayer)
                for (int j = 0; j < count; j++)
                    await GoldenglowOrbCmd.ChannelBuoy(Owner, e, 1);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
