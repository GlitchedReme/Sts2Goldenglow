using Goldenglow.Core;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class OutOfControl() : AbstractGoldenglowCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
{

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new RepeatVar(2),
        GoldenglowUtils.CreatePulseVar()
    ];


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        for (int i = 0; i < DynamicVars.Repeat.BaseValue; i++)
            await GoldenglowCmd.Pulse(Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1);
    }
}
