using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Goldenglow.Power;

[RegisterPower]
public sealed class BeautifulHairstylePower : AbstractGoldenglowPower
{
#if STS2_AT_LEAST_110_0
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
#else
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
#endif
    {
        if (dealer != Owner) return 0;
        if (cardSource == null) return 0;
        if (cardSource.Rarity != CardRarity.Basic && cardSource.Rarity != CardRarity.Common) return 0;
        return Amount;
    }

    public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (cardSource == null) return 0;
        if (cardSource.Owner != Owner.Player) return 0;
        if (cardSource.Rarity != CardRarity.Basic && cardSource.Rarity != CardRarity.Common) return 0;
        return Amount;
    }
}
