using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.CardTags;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Core;

[RegisterOwnedCardTag(nameof(Static))]
public class GoldenglowTags
{
    public static readonly CardTag Static = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(Static)).GetModCardTag();
}
