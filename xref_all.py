"""
Cross-reference ALL unpacked bundles for needed texture PathIDs.
"""
import json, os
from pathlib import Path

BATTLE = Path(r"D:\Files\Games\Arknights\Hypergryph Launcher\games\Arknights\Arknights_Data\StreamingAssets\AB\battle")

NEEDED = {
    "-5044353931323911249": "char_gdglow_01/02 _MainTex",
    "-7351782834978909771": "char_gdglow_03 _MainTex",
    "-1074696917881622342": "char_gdglow_04/snow#5_01/04/20 _MainTex",
    "832498771628188572": "char_gdglow_06 _MainTex",
    "-4064428261868403559": "char_gdglow_05/snow#5_03 _MainTex",
    "3132423431712639009": "char_gdglow_05/snow#5_03 _DissolveTex",
    "-4627928249647999789": "char_gdglow_08/09 _DissolveTex",
    "6269284052906868649": "char_gdglow_10/11 _DissolveTex",
    "-5177635814654685401": "char_gdglow_16 _MainTex",
    "8468650278168530988": "char_gdglow_17/18 _MainTex",
    "-8596330510433126077": "char_gdglow_14 _MainTex",
}

BUNDLES = {
    "gdglow_mat": BATTLE / "Unpacked_gdglow_mat" / "TT_gdglow.ab.json",
    "skinpack": BATTLE / "Unpacked_skinpack" / "TT_char_377_gdglow.ab.json",
    "chararts": BATTLE / "Unpacked_chararts" / "TT_char_377_gdglow.ab.json",
    "common": BATTLE / "Unpacked_common" / "TT_[pack]common.ab.json",
}

def load_textures(json_path):
    if not json_path.exists():
        return {}
    with open(json_path, "r", encoding="utf-8", errors="replace") as f:
        data = json.load(f)
    textures = {}
    for top_key, bundle in data.items():
        if not isinstance(bundle, dict):
            continue
        for pid, entry in bundle.items():
            if isinstance(entry, dict) and "m_TextureSettings" in entry:
                name = entry.get("m_Name", "") or "(unnamed)"
                w = entry.get("m_Width", 0) or 0
                h = entry.get("m_Height", 0) or 0
                textures[pid] = {"name": name, "width": w, "height": h}
    return textures

all_tex = {}
for label, path in BUNDLES.items():
    textures = load_textures(path)
    for pid, info in textures.items():
        all_tex[pid] = (label, info)
    print(f"{label}: {len(textures)} textures")

print(f"\n=== MATCHING NEEDED PathIDs ===")
found = 0
for pid, ref in NEEDED.items():
    if pid in all_tex:
        label, t = all_tex[pid]
        print(f"  FOUND [{label}] {pid} = {t['name']} ({t['width']}x{t['height']})  <{ref}>")
        found += 1
    else:
        print(f"  MISSING {pid}  <{ref}>")

print(f"\nFound: {found}/{len(NEEDED)}")

# Print chararts textures
print(f"\n=== CHARARTS TEXTURES ===")
arts = load_textures(BUNDLES["chararts"])
for pid, info in sorted(arts.items(), key=lambda x: x[1]["name"]):
    print(f"  {pid}: {info['name']} ({info['width']}x{info['height']})")
