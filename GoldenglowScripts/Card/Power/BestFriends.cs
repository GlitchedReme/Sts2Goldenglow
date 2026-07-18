using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;

using Goldenglow.Power;
using Goldenglow.Core;
using Goldenglow.Patch;
using MegaCrit.Sts2.Core.HoverTips;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class BestFriends() : AbstractGoldenglowCard(3, CardType.Power, CardRarity.Rare, TargetType.Self), IHovertipShownInInspectOnly
{
    public IEnumerable<IHoverTip> HoverTipsShownInInspectOnly => [
        GoldenglowUtils.CreateReference("鹰角网络 (hypergryph)")
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BestFriendsPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}
