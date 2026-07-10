"""
Extract textures from skinpack/char_377_gdglow.ab using UnityPy.
This bundle contains the _MainTex textures referenced by base-skin materials.
"""
import UnityPy
import os
from pathlib import Path

SKINPACK = r"D:\Files\Games\Arknights\Hypergryph Launcher\games\Arknights\Arknights_Data\StreamingAssets\AB\Windows\skinpack\char_377_gdglow.ab"
CHARARTS = r"D:\Files\Games\Arknights\Hypergryph Launcher\games\Arknights\Arknights_Data\StreamingAssets\AB\Windows\chararts\char_377_gdglow.ab"
CHAR_ARTS_DYN = r"D:\Files\Games\Arknights\Hypergryph Launcher\games\Arknights\Arknights_Data\StreamingAssets\AB\Windows\arts\dynchars\char_377_gdglow_summer#12.ab"
OUT_DIR = Path(r"D:\Files\Projects\godot\sts2mods\Goldenglow\Goldenglow\image\vfx\skinpack")
OUT_DIR.mkdir(parents=True, exist_ok=True)

def extract_textures(bundle_path, label):
    print(f"\n{'='*60}")
    print(f"Extracting: {label}")
    print(f"  File: {bundle_path}")
    print(f"  Size: {os.path.getsize(bundle_path):,} bytes")
    
    try:
        env = UnityPy.load(bundle_path)
    except Exception as e:
        print(f"  ERROR loading: {e}")
        return
    
    tex_count = 0
    mesh_count = 0
    other_count = 0
    all_objs = []
    
    for obj in env.objects:
        data = obj.read()
        if data.type.name == "Texture2D":
            tex_count += 1
            # Get the actual name of the texture
            name = data.name if data.name else f"tex_{obj.path_id}"
            # Try to get from m_Name
            try:
                if hasattr(data, 'm_Name') and data.m_Name:
                    name = data.m_Name
            except:
                pass
            
            all_objs.append((obj.path_id, "Texture2D", name, data))
        elif data.type.name in ("Mesh", "MeshFilter", "SkinnedMeshRenderer"):
            mesh_count += 1
            all_objs.append((obj.path_id, "Mesh", data.name if data.name else f"mesh_{obj.path_id}", data))
        else:
            other_count += 1
            all_objs.append((obj.path_id, data.type.name, "", data))
    
    print(f"  Textures: {tex_count}, Meshes: {mesh_count}, Other: {other_count}")
    
    # Save textures as PNG
    saved = 0
    by_pathid = {}  # PathID -> filename
    for path_id, obj_type, name, data in all_objs:
        if obj_type == "Texture2D":
            try:
                img_data = data.image
                if img_data is None:
                    continue
                
                safe_name = name.replace("/", "_").replace("#", "_").replace(" ", "_")
                if not safe_name:
                    safe_name = f"tex_{path_id}"
                
                fname = f"{safe_name}.png"
                out_path = OUT_DIR / fname
                img_data.save(out_path)
                saved += 1
                by_pathid[str(path_id)] = fname
                print(f"    [{path_id}] {safe_name}.png")
            except Exception as e:
                print(f"    [{path_id}] ERROR saving {name}: {e}")
    
    print(f"  Saved: {saved} textures")
    
    # Print all object types for reference
    type_counts = {}
    for path_id, obj_type, name, data in all_objs:
        type_counts[obj_type] = type_counts.get(obj_type, 0) + 1
    print(f"  Object types:")
    for t, c in sorted(type_counts.items(), key=lambda x: -x[1]):
        if t != "Texture2D":
            print(f"    {t}: {c}")
    
    return by_pathid

# Extract from skinpack (largest bundle - likely contains all textures)
skinpack_map = extract_textures(SKINPACK, "skinpack/char_377_gdglow.ab")

# Also try chararts 
# chararts_map = extract_textures(CHARARTS, "chararts/char_377_gdglow.ab")

# Print summary matching known texture PathIDs
print(f"\n{'='*60}")
print("MATCHING KNOWN TEXTURE PathIDs FROM MATERIALS:")
missing_pathids = {
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
    "4343095108389510930": "char_gdglow_snow#5_09 _MainTex",
    "-4903909177795013460": "char_gdglow_snow#5_10 _MainTex",
    "-8335021126123278372": "char_gdglow_snow#5_19 _MainTex",
    "4217536898810482573": "char_gdglow_summer#12_03 _MainTex",
    "-286602781271088591": "summer#12_04_3/4 _MainTex",
    "3501380825813970091": "char_gdglow_snow#5_05/06/07 _MainTex",
    "-2942761766999300976": "char_gdglow_snow#5_12/13 _MainTex",
    "-4659213201470387487": "char_gdglow_snow#5_14/15/16/17 _MainTex",
}

if skinpack_map:
    for pid, ref in missing_pathids.items():
        if pid in skinpack_map:
            print(f"  ✅ {pid} = {skinpack_map[pid]}  ({ref})")
        else:
            print(f"  ❌ {pid} NOT FOUND  ({ref})")
else:
    print("  No textures extracted from skinpack (bundle might be encrypted or wrong format)")

print(f"\nTotal textures found: {len(skinpack_map) if skinpack_map else 0}")
