"""
Final cross-reference: search ALL unpacked bundle typetree JSONs for missing PathIDs.
"""
import json, os
from pathlib import Path

BATTLE = Path(r"D:\Files\Games\Arknights\Hypergryph Launcher\games\Arknights\Arknights_Data\StreamingAssets\AB\battle")

NEEDED_PIDS = {
    "-5044353931323911249", "-7351782834978909771", "-1074696917881622342",
    "832498771628188572", "-4064428261868403559", "3132423431712639009",
    "-4627928249647999789", "6269284052906868649", "-5177635814654685401",
    "8468650278168530988", "-8596330510433126077",
}

found_map = {}

# Find all TT_*.ab.json files in all Unpacked_* directories
for unpacked_dir in BATTLE.glob("Unpacked_*"):
    for json_file in unpacked_dir.glob("**/TT_*.ab.json"):
        try:
            with open(json_file, "r", encoding="utf-8", errors="replace") as f:
                data = json.load(f)
        except:
            continue
        
        for top_key, bundle in data.items():
            if not isinstance(bundle, dict):
                continue
            for pid, entry in bundle.items():
                if pid in NEEDED_PIDS and isinstance(entry, dict) and "m_TextureSettings" in entry:
                    name = entry.get("m_Name", "(unnamed)")
                    w = entry.get("m_Width", 0) or 0
                    h = entry.get("m_Height", 0) or 0
                    found_map[pid] = (unpacked_dir.name, name, w, h)

# Print results
print("=== RESULTS ===")
for pid in sorted(NEEDED_PIDS):
    if pid in found_map:
        d, n, w, h = found_map[pid]
        print(f"  FOUND {pid} in {d}: {n} ({w}x{h})")
    else:
        print(f"  MISSING {pid}")

print(f"\nFound: {len(found_map)}/{len(NEEDED_PIDS)}")
print(f"Unpacked dirs searched: {len(list(BATTLE.glob('Unpacked_*')))}")
