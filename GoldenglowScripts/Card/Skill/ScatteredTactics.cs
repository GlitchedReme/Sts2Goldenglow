using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using Goldenglow.Orb;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class ScatteredTactics() : AbstractGoldenglowCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
{

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Buoy", 5)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromOrb<BuoyOrb>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var state = CombatManager.Instance.DebugOnlyGetState();
        var allTargets = new List<Creature>();

        var players = state?.Players;
        Player? player = null;
        if (players != null && players.Count > 0)
            player = players[0];
        if (player != null) allTargets.Add(player.Creature);

        var enemies = state?.Enemies;
        if (enemies != null)
            for (int i = 0; i < enemies.Count; i++)
                if (!enemies[i].IsDead && !enemies[i].IsPlayer)
                    allTargets.Add(enemies[i]);

        int n = (int)DynamicVars["Buoy"].BaseValue;
        for (int i = 0; i < n; i++)
        {
            var target = allTargets[Random.Shared.Next(allTargets.Count)];
            await GoldenglowOrbCmd.ChannelBuoy(Owner, target);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Buoy"].UpgradeValueBy(1);
    }
}
