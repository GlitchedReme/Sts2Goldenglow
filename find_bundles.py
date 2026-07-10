"""Find AB file paths for cab MD5 hashes from hot_update_list."""
import json

TARGETS = {
    "44d0abb60db2170b734217b0b04fcf36": "FileID=6 Shaders",
    "b2739f24071ea30492cad3b1c540173a": "FileID=12 Base tex",
    "92fdf60cd1676328e17f73f0bf852e97": "FileID=9 Base tex",
    "9e18acd2831f8cef6fdd0163df5a3104": "FileID=11 Base tex",
    "c2c688f710a565b7cce0cc93f7bd219f": "FileID=13 Dissolve tex",
    "fe644cd0cf84f312dc9d6de3bfc17d34": "FileID=15 Dissolve tex",
    "3fcda8e432a735264d43b49ecbf8fbee": "FileID=5",
    "0d0c905b03658da84d810b34eba903e0": "FileID=1",
}

BASE = r"D:\Files\Games\Arknights\Hypergryph Launcher\games\Arknights\Arknights_Data\StreamingAssets\AB\Windows"

for fname in ["hot_update_list.json", "persistent_res_list.json"]:
    path = BASE + "\\" + fname
    with open(path, "r") as f:
        data = json.load(f)
    print(f"=== {fname} ===")
    for ab in data.get("abInfos", []):
        md5 = ab.get("md5", "").lower()
        name = ab.get("name", "")
        if md5 in TARGETS:
            print(f"  MATCH: {name} -> {TARGETS[md5]}")
    print()

# Also print all gdglow files
print("=== All gdglow entries ===")
for fname in ["hot_update_list.json"]:
    path = BASE + "\\" + fname
    with open(path, "r") as f:
        data = json.load(f)
    for ab in data.get("abInfos", []):
        name = ab.get("name", "").lower()
        if "gdglow" in name:
            print(f"  {ab['name']} (md5={ab.get('md5','?')})")
