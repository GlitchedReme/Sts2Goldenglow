using System.Collections.ObjectModel;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Audio;
using STS2RitsuLib.Data;

namespace Goldenglow.Core;

public static class SkinResources
{
    public struct Entry
    {
        public string DisplayName;
        public string CharacterSkinPath;
        public string CharacterRestSitePath;
        public string BuoySkinPath;
        public string BuoyAttackPath;
    }

    private static readonly Dictionary<string, Entry> _defs = new()
    {
        ["default"] = new()
        {
            DisplayName = "default",
            CharacterSkinPath = "res://Goldenglow/image/character/default.tres",
            CharacterRestSitePath = "res://Goldenglow/image/character/build_default.tres",
            BuoySkinPath = "res://Goldenglow/image/orb/buoy_default.png",
        },
        ["snow"] = new()
        {
            DisplayName = "snow",
            CharacterSkinPath = "res://Goldenglow/image/character/snow.tres",
            CharacterRestSitePath = "res://Goldenglow/image/character/build_snow.tres",
            BuoySkinPath = "res://Goldenglow/image/orb/buoy_snow.png",
            BuoyAttackPath = "res://Goldenglow/image/vfx/flipbook_154.png"
        },
        ["sanrio"] = new()
        {
            DisplayName = "sanrio",
            CharacterSkinPath = "res://Goldenglow/image/character/sanrio.tres",
            CharacterRestSitePath = "res://Goldenglow/image/character/build_sanrio.tres",
            BuoySkinPath = "res://Goldenglow/image/orb/buoy_sanrio.png",
            BuoyAttackPath = "res://Goldenglow/image/vfx/flipbook_216.png"
        },
        ["summer"] = new()
        {
            DisplayName = "summer",
            CharacterSkinPath = "res://Goldenglow/image/character/summer.tres",
            CharacterRestSitePath = "res://Goldenglow/image/character/build_summer.tres",
            BuoySkinPath = "res://Goldenglow/image/orb/buoy_summer.png",
            BuoyAttackPath = "res://Goldenglow/image/vfx/flipbook_155.png"
        }
    };

    public static readonly ReadOnlyCollection<string> Keys = new([.. _defs.Keys]);

    public static readonly Dictionary<ulong, string> RemoteSkins = [];

    public static string SelectedSkinKey
    {
        get => ModDataStore.For(Goldenglow.Entry.ModId).Get<SkinState>("skin_prefs").Skin ?? "default";
        set
        {
            var store = ModDataStore.For(Goldenglow.Entry.ModId);
            store.Modify<SkinState>("skin_prefs", data => data.Skin = value);
            store.Save("skin_prefs");
        }
    }

    public static int IndexOfKey(string key)
    {
        int i = 0;
        foreach (var k in _defs.Keys)
        {
            if (k == key) return i;
            i++;
        }
        return 0;
    }

    public readonly struct SkinResource(Resource combat, Resource restsite)
    {
        public Resource Combat { get; } = combat;
        public Resource RestSite { get; } = restsite;
    }

    private static Dictionary<string, SkinResource>? _resources;

    public static SkinResource GetSpine(string key)
    {
        _resources ??= [];
        if (!_resources.TryGetValue(key, out var res))
        {
            if (!_defs.TryGetValue(key, out var def))
            {
                GD.PrintErr($"[Goldenglow] SkinResources.GetResource: key '{key}' not found, falling back to default");
                key = "default";
                def = _defs["default"];
            }
            res = new(ResourceLoader.Load(def.CharacterSkinPath), ResourceLoader.Load(def.CharacterRestSitePath));
            _resources[key] = res;
        }
        return res;
    }

    public static string GetDisplayName(string key) =>
        _defs.TryGetValue(key, out var def) ? def.DisplayName : "default";

    public static string GetBuoySkinPath(string key) =>
        _defs.TryGetValue(key, out var def) ? def.BuoySkinPath : _defs["default"].BuoySkinPath;

    public static string? GetBuoyAttackPath(string key) =>
        _defs.TryGetValue(key, out var def) ? def.BuoyAttackPath : _defs["default"].BuoyAttackPath;

    public static string GetLocKey(string key) =>
        $"GOLDENGLOW_SETTINGS_SKIN.{GetDisplayName(key)}";

    public static string GetSkinKey(Player? player)
    {
        if (player == null || LocalContext.IsMe(player))
            return SelectedSkinKey;

        if (RemoteSkins.TryGetValue(player.NetId, out var remoteSkin))
            return remoteSkin;

        return "default";
    }
}
