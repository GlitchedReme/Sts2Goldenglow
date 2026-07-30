using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using Goldenglow.Patch;
using MegaCrit.Sts2.Core.HoverTips;
using Goldenglow.Core;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Combat;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class FreshPerfume() : AbstractGoldenglowCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self), IHovertipShownInInspectOnly
{
    public IEnumerable<IHoverTip> HoverTipsShownInInspectOnly => [
        GoldenglowUtils.CreateReference("鹰角网络 (hypergryph)")
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        GoldenglowUtils.CreateAttractVar(2),
        new DynamicVar("Times", 1)
    ];

    protected override bool IsPlayable => CombatManager.Instance.History.Entries.OfType<CardPlayFinishedEntry>().Count(e => e.HappenedThisTurn(CombatState) && e.CardPlay.Card == this) < DynamicVars["Times"].IntValue;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GoldenglowCmd.Attract(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Goldenglow_Attract"].UpgradeValueBy(1);
        // DynamicVars["Times"].UpgradeValueBy(1);
    }
}
