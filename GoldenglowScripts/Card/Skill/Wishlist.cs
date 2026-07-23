using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Unlocks;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Utils;

namespace Goldenglow.Card;

[RegisterCard(typeof(GoldenglowCardPool))]
public class Wishlist() : AbstractGoldenglowCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self), ICardOnGeneratedAsReward
{
    private const int GiftAmount = 2;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    private static readonly SavedAttachedState<Wishlist, string> GiftCardIds = new("WishlistGiftCards", _ => "");

    private string _giftCardIdsBackup = "";

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..GiftCards.Select(c => new CardHoverTip(c))
    ];

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        GiftCardIds[this] = _giftCardIdsBackup;
        Entry.Logger.Info($"[Wishlist] DeepCloneFields: backup='{_giftCardIdsBackup}'");
    }

    public override void AfterCreated()
    {
        CreateGifts(Owner);
    }

    protected override void AfterDeserialized()
    {
        _giftCardIdsBackup = GiftCardIds[this];
        Entry.Logger.Info($"[Wishlist] AfterDeserialized: GiftCardIds='{GiftCardIds[this]}', backup='{_giftCardIdsBackup}'");
    }

    public bool OnGeneratedAsReward(Player player, CardCreationOptions options)
    {
        CreateGifts(player);
        return true;
    }

    private void CreateGifts(Player player)
    {
        if (!string.IsNullOrWhiteSpace(GiftCardIds[this]) && GiftCardIds[this].Split(",").Length > 0)
            return;

        var rng = player.PlayerRng.Rewards;
        var pool = player.Character.CardPool
            .GetUnlockedCards(UnlockState.all, player.RunState.CardMultiplayerConstraint)
            .Where(c => c.Keywords.Contains(CardKeyword.Exhaust) && c is not Wishlist)
            .ToList();

        if (pool.Count == 0) return;

        var takeCount = Math.Min(GiftAmount, pool.Count);
        var selected = new List<CardModel>();
        var available = pool.ToList();
        for (int i = 0; i < takeCount; i++)
        {
            var pick = rng.NextItem(available)!;
            available.Remove(pick);
            selected.Add(player.RunState.CreateCard(pick, player));
        }

        GiftCardIds[this] = string.Join(",", selected.Select(c => c.Id.ToString()));
        _giftCardIdsBackup = GiftCardIds[this];
    }

    private IEnumerable<CardModel> GiftCards => GiftCardIds[this].Split(",")
        .Select(id =>
        {
            var ids = id.Split('.');
            if (ids.Length != 2) return null;
            return ModelDb.GetById<CardModel>(new ModelId(ids[0], ids[1]));
        })
        .Where(c => c != null)!;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var ids = GiftCardIds.TryGetValue(this, out var v) ? v : "(none)";
        Entry.Logger.Info($"[Wishlist] OnPlay: GiftCardIds='{ids}', GiftCards.Count={GiftCards.Count()}");
        if (!GiftCards.Any())
            return;
        foreach (var card in GiftCards.Select(c => Owner.Creature.CombatState?.CreateCard(c, Owner)))
        {
            if (card == null)
                continue;
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
        }
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        description.Add("GiftList", [.. GiftCards.Select(c => c.Title)]);
        if (GiftCardIds.TryGetValue(this, out var ids) && !string.IsNullOrWhiteSpace(ids))
            description.Add("GiftCount", ids.Split(",").Length);
        else
            description.Add("GiftCount", 0);
    }

    protected override void OnUpgrade()
    {
        // EnergyCost.UpgradeBy(-1);
        AddKeyword(CardKeyword.Innate);
    }
}
