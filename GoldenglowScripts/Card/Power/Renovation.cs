using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class Renovation() : AbstractGoldenglowCard(0, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("OrbSlot", 1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int count = (int)DynamicVars["OrbSlot"].BaseValue;

        // Give orb slots to all players
        foreach (var player in CombatState!.Players)
            await OrbCmd.AddSlots(player, count);

        var enemies = CombatState!.Enemies;
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].Monster == null) continue;
            await OrbCmd.AddSlots(enemies[i].Player!, count);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["OrbSlot"].UpgradeValueBy(1);
    }
}
