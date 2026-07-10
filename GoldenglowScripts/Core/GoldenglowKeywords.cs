using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace Goldenglow.Core;

[RegisterOwnedCardKeyword(nameof(Bidirection), CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public class GoldenglowKeywords
{
    public static readonly CardKeyword Bidirection = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Bidirection)).GetModCardKeyword();
}
