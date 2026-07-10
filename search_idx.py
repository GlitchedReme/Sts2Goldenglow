"""
Search for cab hash in .idx manifest
"""
import struct, os, re

IDX_PATH = r"D:\Files\Games\Arknights\Hypergryph Launcher\games\Arknights\Arknights_Data\StreamingAssets\AB\Windows\9e65dfefe2e10ddbecaad217f48a7548.idx"

TARGETS = {
    "44d0abb60db2170b734217b0b04fcf36": "FileID=6 Shaders",
    "b2739f24071ea30492cad3b1c540173a": "FileID=12 Base tex",
    "92fdf60cd1676328e17f73f0bf852e97": "FileID=9 Base tex",
    "9e18acd2831f8cef6fdd0163df5a3104": "FileID=11 Base tex",
    "c2c688f710a565b7cce0cc93f7bd219f": "FileID=13 Dissolve",
    "fe644cd0cf84f312dc9d6de3bfc17d34": "FileID=15 Dissolve",
}

data = open(IDX_PATH, "rb").read()
print(f"File size: {len(data)} bytes")

for target_hex, label in TARGETS.items():
    target_bytes = bytes.fromhex(target_hex)
    idx = data.find(target_bytes)
    if idx >= 0:
        start = max(0, idx - 80)
        end = min(len(data), idx + len(target_bytes) + 300)
        ctx = data[start:end]
        
        # Find readable ASCII strings
        strings = []
        for m in re.finditer(rb"[\x20-\x7e]{4,}", ctx):
            s = m.group().decode("ascii", errors="replace")
            strings.append(s)
        
        print(f"\n{label} (offset {idx}):")
        for s in strings:
            print(f"  '{s}'")
    else:
        print(f"\n{label}: NOT FOUND")
