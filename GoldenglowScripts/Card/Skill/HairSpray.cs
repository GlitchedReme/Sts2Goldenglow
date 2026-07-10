using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;
using STS2RitsuLib.Scaffolding.Content;
using Goldenglow.Capabilities;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class HairSpray() : AbstractGoldenglowCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Buff", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hand = PileType.Hand.GetPile(Owner).Cards;
        var candidates = new List<CardModel>();
        for (int i = 0; i < hand.Count; i++)
            if (hand[i].Rarity == CardRarity.Basic || hand[i].Rarity == CardRarity.Common)
                candidates.Add(hand[i]);
        if (candidates.Count == 0) return;

        var selected = (await CardSelectCmd.FromHand(
            choiceContext, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1),
            c => c.Rarity == CardRarity.Basic || c.Rarity == CardRarity.Common, this
        )).FirstOrDefault();
        if (selected == null) return;

        var clone = selected.CreateClone();
        int buff = (int)DynamicVars["Buff"].BaseValue;
        var cap = clone.GetOrCreateCapability<BuffDamageOrBlockCapability>();
        cap.DynamicVars["Buff"].UpgradeValueBy(buff);

        await CardPileCmd.AddGeneratedCardToCombat(clone, PileType.Hand, Owner, CardPilePosition.Top);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Buff"].UpgradeValueBy(2);
    }
}
