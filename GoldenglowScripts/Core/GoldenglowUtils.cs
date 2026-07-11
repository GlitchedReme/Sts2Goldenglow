using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.TestSupport;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Content;

namespace Goldenglow.Core;

public static class GoldenglowUtils
{
    public static IHoverTip Attract => CreateStaticHoverTip("attract");

    public static IHoverTip Transfer => CreateStaticHoverTip("transfer");

    public static IHoverTip Static => CreateStaticHoverTip("static");

    public static IHoverTip Pulse => CreateStaticHoverTip("pulse");

    public static IHoverTip CreateStaticHoverTip(string tip, params DynamicVar[] vars)
    {
        var text = ModContentRegistry.GetCompoundId(Entry.ModId, "KEYWORD", tip);
        var title = new LocString("static_hover_tips", text + ".title");
        var desc = new LocString("static_hover_tips", text + ".description");
        for (int i = 0; i < vars.Length; i++)
        {
            title.Add(vars[i]);
            desc.Add(vars[i]);
        }
        return new HoverTip(title, desc);
    }

    public static ComputedDynamicVar CreatePulseVar()
    {
        var v = ModCardVars.Computed("Pulse", 1, card =>
            GoldenglowSingleton.GetPulse(card?.Owner));
        v.WithSharedTooltip("GOLDENGLOW_KEYWORD_PULSE");
        return v;
    }

    public static DynamicVar CreateAttractVar(int baseValue) => new DynamicVar("Goldenglow_Attract", baseValue)
    .WithSharedTooltip("GOLDENGLOW_KEYWORD_ATTRACT");


    public static void PlayVfx(Creature target, Node2D vfx, Vector2? position = null)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(vfx);
        
        if (TestMode.IsOn)
            return;
        var creatureNode = target.GetCreatureNode();
        if (creatureNode == null)
            return;
        target.GetVfxContainer()?.AddChildSafely(vfx);
        vfx.GlobalPosition = position ?? creatureNode.VfxSpawnPosition;
    }
}
