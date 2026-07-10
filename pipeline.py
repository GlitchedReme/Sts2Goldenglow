"""
Goldenglow VFX Pipeline v2 — extract ALL particle parameters + generate Godot .tscn.
Single-pass JSON read + enhanced extraction + proper Godot 4 format.
"""
import json, hashlib, itertools
from pathlib import Path

AB_JSON = Path(r"D:\Files\Games\Arknights\Hypergryph Launcher\games\Arknights\Arknights_Data\StreamingAssets\AB\battle\Unpacked_gdglow_mat\TT_gdglow.ab.json")
VFX_DATA = Path(r"D:\Files\Projects\godot\sts2mods\Goldenglow\vfx_data")
VFX_DATA.mkdir(parents=True, exist_ok=True)
OUT_SCENES = Path(r"D:\Files\Projects\godot\sts2mods\Goldenglow\Goldenglow\scene\vfx")
OUT_SCENES.mkdir(parents=True, exist_ok=True)

# ============================================================
# MATERIAL → GODOT SHADER CONFIG (from extract_matmap.py data)
# ============================================================
MATERIAL_CONFIG = {
    "char_gdglow_01": {"shader": "Standard", "blend": "alpha", "self_modulate_color": (1,1,1,1), "tex_source": "external_12", "tex_pid": "-5044353931323911249"},
    "char_gdglow_02": {"shader": "Standard", "blend": "alpha", "self_modulate_color": (1,1,1,1), "tex_source": "external_12", "tex_pid": "-5044353931323911249"},
    "char_gdglow_03": {"shader": "Standard", "blend": "alpha", "self_modulate_color": (1,1,1,1), "tex_source": "external_12", "tex_pid": "-7351782834978909771"},
    "char_gdglow_04": {"shader": "StandardHighlight", "blend": "alpha", "self_modulate_color": (0.28,0.17,0.40,0.27), "tex_source": "external_9"},
    "char_gdglow_05": {"shader": "Dissolve", "blend": "alpha", "self_modulate_color": (0.34,0.20,0.38,0.20), "tex_source": "external_7", "_Amount": 0.085, "_BorderWidth": 0.584},
    "char_gdglow_06": {"shader": "Standard", "blend": "alpha", "self_modulate_color": (1,1,1,1), "tex_source": "external_11"},
    "char_gdglow_08": {"shader": "Dissolve", "blend": "alpha", "self_modulate_color": (1,1,1,1), "tex_source": "local", "tex_name": "char_gdglow_02", "_Amount": 0.074, "_BorderWidth": 0.485},
    "char_gdglow_09": {"shader": "DirectionalDissolve", "blend": "additive", "self_modulate_color": (0.10,0.09,0.09,0.58), "tint_color": (0.56,0.56,0.56,0.86), "tex_source": "local", "tex_name": "char_gdglow_06", "_Amount": 0.155},
    "char_gdglow_10": {"shader": "DirectionalDissolve", "blend": "additive", "self_modulate_color": (0.10,0.09,0.09,0.58), "tint_color": (0.56,0.56,0.56,0.77), "tex_source": "local", "tex_name": "char_gdglow_06", "_Amount": 0.155},
    "char_gdglow_11": {"shader": "DirectionalDissolve", "blend": "additive", "self_modulate_color": (0.10,0.09,0.09,0.58), "tint_color": (0.56,0.56,0.56,0.77), "tex_source": "local", "tex_name": "char_gdglow_06", "_Amount": 0.155},
    "char_gdglow_14": {"shader": "StandardHighlight", "blend": "alpha", "self_modulate_color": (0.50,0.50,0.50,0.50), "tex_source": "external_15"},
    "char_gdglow_15": {"shader": "StandardHighlight", "blend": "alpha", "self_modulate_color": (1,1,1,1), "tex_source": "local", "tex_name": "char_gdglow_05"},
    "char_gdglow_16": {"shader": "Standard", "blend": "alpha", "self_modulate_color": (0.50,0.50,0.50,0.20), "tex_source": "external_9"},
    "char_gdglow_17": {"shader": "Standard", "blend": "alpha", "self_modulate_color": (1,1,1,1), "tex_source": "external_11"},
    "char_gdglow_18": {"shader": "Standard", "blend": "alpha", "self_modulate_color": (1,1,1,1), "tex_source": "external_11"},
}

# ============================================================
# TEXTURE PATH RESOLUTION (77 available PNGs → resource paths)
# ============================================================
TEXTURE_PATHS = {
    # Internal gdglow textures (21)
    "char_gdglow_02": "res://Goldenglow/image/vfx/char_gdglow_02.png",
    "char_gdglow_05": "res://Goldenglow/image/vfx/char_gdglow_05.png",
    "char_gdglow_06": "res://Goldenglow/image/vfx/char_gdglow_06.png",
    "star_02": "res://Goldenglow/image/vfx/star_02.png",
    "star_05": "res://Goldenglow/image/vfx/star_05.png",
    "ray_94_2": "res://Goldenglow/image/vfx/ray_94_2.png",
    "flow_91_C_1": "res://Goldenglow/image/vfx/flow_91_C_1.png",
    "mask_11": "res://Goldenglow/image/vfx/mask_11.png",
    "flipbook_154": "res://Goldenglow/image/vfx/flipbook_154.png",
    "flipbook_155": "res://Goldenglow/image/vfx/flipbook_155.png",
    "flipbook_216": "res://Goldenglow/image/vfx/flipbook_216.png",
    # Common textures (22)
    "fx": "res://Goldenglow/image/vfx/common/fx.png",
    "img_fx_light_01": "res://Goldenglow/image/vfx/common/img_fx_light_01.png",
    "img_fx_light_02": "res://Goldenglow/image/vfx/common/img_fx_light_02.png",
    "mask_08": "res://Goldenglow/image/vfx/common/mask_08.png",
    "mask_09": "res://Goldenglow/image/vfx/common/mask_09.png",
    "trail_11": "res://Goldenglow/image/vfx/common/trail_11.png",
    "AreaTex": "res://Goldenglow/image/vfx/common/AreaTex.png",
    "SearchTex": "res://Goldenglow/image/vfx/common/SearchTex.png",
    # Fallback for external (unavailable)
    "_fallback": "res://Goldenglow/image/vfx/char_gdglow_02.png",
}

# ============================================================
# PARTICLE SHAPE TYPE → GODOT ENUM
# ============================================================
SHAPE_GODOT = {
    0: ("emission_shape = 0", "emission_sphere_radius"),     # Sphere
    1: ("emission_shape = 2", "emission_box_extents"),       # Hemisphere → Box
    4: ("emission_shape = 3", "emission_ring_radius"),       # Circle → Ring
    5: ("emission_shape = 3", "emission_ring_radius"),       # Circle edge → Ring
    10: ("emission_shape = 1", "emission_cone_angle"),       # Cone
}

# ============================================================
# PHASE 1: ENHANCED PARTICLE PARAMETER EXTRACTION
# ============================================================
def get_float_minmax(mms, default=1.0):
    """Resolve Unity minMaxState+scalar+minScalar to (min, max)."""
    if not isinstance(mms, dict):
        return (default, default)
    state = mms.get("minMaxState", 0)
    s = mms.get("scalar", default)
    ms = mms.get("minScalar", s)
    if state == 0:
        return (s, s)
    elif state in (1, 2):
        return (s, s)
    elif state == 3:
        return (min(s, ms), max(s, ms))
    return (s, s)

def extract_curve_keys(curve):
    """Extract keyframe list from Unity AnimationCurve."""
    if not isinstance(curve, dict):
        return []
    curve_data = curve.get("m_Curve", [])
    if not curve_data or not isinstance(curve_data, list):
        return []
    keys = []
    for kf in curve_data:
        if isinstance(kf, dict):
            keys.append({
                "time": kf.get("time", 0),
                "value": kf.get("value", 1),
                "inSlope": kf.get("inSlope", 0),
                "outSlope": kf.get("outSlope", 0),
            })
    return keys

def extract_gradient(gradient):
    """Extract color+alpha keys from Unity Gradient."""
    if not isinstance(gradient, dict):
        return None
    nck = gradient.get("m_NumColorKeys", 2)
    nak = gradient.get("m_NumAlphaKeys", 2)
    mode = gradient.get("m_Mode", 0)  # 0=Blend, 1=Fixed
    
    # Color keys: stored as key0-key7 and ctime0-ctime7
    color_keys = []
    for i in range(nck):
        key = gradient.get(f"key{i}", {})
        ctime = gradient.get(f"ctime{i}", 0)
        if isinstance(key, dict):
            color_keys.append({
                "r": key.get("r", 1), "g": key.get("g", 1),
                "b": key.get("b", 1), "a": key.get("a", 1),
                "time": ctime / 65535.0 if ctime else 0
            })
    
    # Alpha keys: also stored in key0-key7 and atime0-atime7
    alpha_keys = []
    for i in range(nak):
        key = gradient.get(f"key{i}", {})
        atime = gradient.get(f"atime{i}", 0)
        if isinstance(key, dict):
            alpha_keys.append({
                "alpha": key.get("a", 1),
                "time": atime / 65535.0 if atime else 0
            })
    
    return {"mode": mode, "colorKeys": color_keys, "alphaKeys": alpha_keys,
            "numColorKeys": nck, "numAlphaKeys": nak}

def extract_full_particle(ps_entry, rend_entry, mat_entry):
    """Extract COMPLETE particle system parameters."""
    if not ps_entry:
        return None
    
    p = {}
    init = ps_entry.get("InitialModule", {})
    if init and init.get("enabled", True):
        p["looping"] = ps_entry.get("looping", False)
        p["prewarm"] = ps_entry.get("prewarm", False)
        p["duration"] = ps_entry.get("lengthInSec", 1.0)
        p["maxParticles"] = init.get("maxNumParticles", 100)
        
        lt = init.get("startLifetime", {})
        lt_min, lt_max = get_float_minmax(lt, 1.0)
        p["lifetimeMin"] = lt_min
        p["lifetimeMax"] = lt_max
        
        sp = init.get("startSpeed", {})
        sp_min, sp_max = get_float_minmax(sp, 0)
        p["speedMin"] = sp_min
        p["speedMax"] = sp_max
        
        sz = init.get("startSize", {})
        sz_min, sz_max = get_float_minmax(sz, 1.0)
        p["sizeMin"] = sz_min
        p["sizeMax"] = sz_max
        
        sc = init.get("startColor", {})
        mc = sc.get("minColor", {}) if isinstance(sc.get("minColor"), dict) else {}
        mx = sc.get("maxColor", {}) if isinstance(sc.get("maxColor"), dict) else {}
        p["startColorMin"] = {"r": mc.get("r",1), "g": mc.get("g",1), "b": mc.get("b",1), "a": mc.get("a",1)}
        p["startColorMax"] = {"r": mx.get("r",1), "g": mx.get("g",1), "b": mx.get("b",1), "a": mx.get("a",1)}
        p["startColorMode"] = sc.get("minMaxState", 0)
        
        sr = init.get("startRotation", {})
        sr_min, sr_max = get_float_minmax(sr, 0)
        p["rotationMin"] = sr_min
        p["rotationMax"] = sr_max
        
        p["gravity"] = init.get("gravityModifier", {}).get("scalar", 0)
    
    # Shape
    shape = ps_entry.get("ShapeModule", {})
    if shape and shape.get("enabled", False):
        p["shapeType"] = shape.get("type", -1)
        p["shapeAngle"] = shape.get("angle", 0)
        p["shapeRadius"] = shape.get("radius", {}).get("value", 0.5) if isinstance(shape.get("radius"), dict) else shape.get("radius", 0.5)
        p["shapeArc"] = shape.get("arc", {}).get("value", 360) if isinstance(shape.get("arc"), dict) else shape.get("arc", 360)
        sd = shape.get("donutRadius", 0.2)
        p["shapeDonutRadius"] = sd if isinstance(sd, (int, float)) else sd.get("value", 0.2) if isinstance(sd, dict) else 0.2
        mscale = shape.get("m_Scale", {})
        p["shapeScale"] = {"x": mscale.get("x",1), "y": mscale.get("y",1), "z": mscale.get("z",1)}
    
    # Emission
    emission = ps_entry.get("EmissionModule", {})
    if emission and emission.get("enabled", True):
        p["rateOverTime"] = emission.get("rateOverTime", {}).get("scalar", 0)
        bursts = emission.get("m_Bursts", [])
        p["burstCount"] = emission.get("m_BurstCount", 0)
        p["bursts"] = []
        for b in bursts:
            if isinstance(b, dict):
                cc = b.get("countCurve", {})
                p["bursts"].append({
                    "time": b.get("time", 0),
                    "count": cc.get("scalar", 1) if isinstance(cc, dict) else b.get("countCurve", 1),
                    "cycles": b.get("cycleCount", 1),
                    "probability": b.get("probability", 1),
                })
    
    # Size over lifetime
    size_m = ps_entry.get("SizeModule", {})
    if size_m and size_m.get("enabled", False):
        curve = size_m.get("curve", {})
        if isinstance(curve, dict):
            mc = curve.get("maxCurve", {})
            if isinstance(mc, dict):
                p["sizeCurve"] = extract_curve_keys(mc)
                p["sizeCurveScalar"] = curve.get("scalar", 1.0)
    
    # Color over lifetime
    color_m = ps_entry.get("ColorModule", {})
    if color_m and color_m.get("enabled", False):
        grad = color_m.get("gradient", {})
        if isinstance(grad, dict):
            mg = grad.get("maxGradient", {})
            if isinstance(mg, dict):
                p["colorGradient"] = extract_gradient(mg)
    
    # Rotation over lifetime
    rot_m = ps_entry.get("RotationModule", {})
    if rot_m and rot_m.get("enabled", False):
        curve = rot_m.get("curve", {})
        cr_min, cr_max = get_float_minmax(curve, 0)
        p["rotationCurveMin"] = cr_min
        p["rotationCurveMax"] = cr_max
    
    # UV / Flipbook
    uv_m = ps_entry.get("UVModule", {})
    if uv_m and uv_m.get("enabled", False):
        p["flipbook"] = {
            "tilesX": uv_m.get("tilesX", 1),
            "tilesY": uv_m.get("tilesY", 1),
            "fps": uv_m.get("fps", 30),
            "cycles": uv_m.get("cycles", 1),
            "startFrame": uv_m.get("startFrame", {}).get("scalar", 0) if isinstance(uv_m.get("startFrame"), dict) else 0,
            "animationType": uv_m.get("animationType", 0),
            "rowMode": uv_m.get("rowMode", 1),
            "flipU": uv_m.get("flipU", 0),
            "flipV": uv_m.get("flipV", 0),
        }
        frame_ot = uv_m.get("frameOverTime", {})
        if isinstance(frame_ot, dict):
            mc = frame_ot.get("maxCurve", {})
            if isinstance(mc, dict):
                p["flipbook"]["frameCurve"] = extract_curve_keys(mc)
    
    # Noise
    noise = ps_entry.get("NoiseModule", {})
    if noise and noise.get("enabled", False):
        p["noise"] = {
            "strength": noise.get("strength", {}).get("scalar", 1),
            "frequency": noise.get("frequency", 1),
            "scrollSpeed": noise.get("scrollSpeed", {}).get("scalar", 0),
            "octaves": noise.get("octaves", 1),
            "quality": noise.get("quality", 2),
            "damping": noise.get("damping", True),
            "positionAmount": noise.get("positionAmount", {}).get("scalar", 1),
            "rotationAmount": noise.get("rotationAmount", {}).get("scalar", 0),
            "sizeAmount": noise.get("sizeAmount", {}).get("scalar", 0),
        }
    
    # Trail
    trail = ps_entry.get("TrailModule", {})
    if trail and trail.get("enabled", False):
        tl = trail.get("lifetime", {})
        p["trail"] = {
            "lifetime": get_float_minmax(tl, 0.5)[0] if isinstance(tl, dict) else (tl if isinstance(tl, (int, float)) else 0.5),
            "minVertexDistance": trail.get("minVertexDistance", 0.2),
            "sizeAffectsWidth": trail.get("sizeAffectsWidth", True),
            "inheritParticleColor": trail.get("inheritParticleColor", True),
            "textureMode": trail.get("textureMode", 0),
            "ribbonCount": trail.get("ribbonCount", 1),
            "dieWithParticles": trail.get("dieWithParticles", True),
        }
        wot = trail.get("widthOverTrail", {})
        if isinstance(wot, dict):
            mc = wot.get("maxCurve", {})
            if isinstance(mc, dict):
                p["trail"]["widthCurve"] = extract_curve_keys(mc)
    
    # Renderer
    if rend_entry:
        p["renderer"] = {
            "renderMode": rend_entry.get("m_RenderMode", 0),
            "sortingOrder": rend_entry.get("m_SortingOrder", 0),
            "sortingLayer": rend_entry.get("m_SortingLayer", 0),
            "sortMode": rend_entry.get("m_SortMode", 0),
            "lengthScale": rend_entry.get("m_LengthScale", 2),
            "velocityScale": rend_entry.get("m_VelocityScale", 0),
            "maxParticleSize": rend_entry.get("m_MaxParticleSize", 0.5),
            "normalDirection": rend_entry.get("m_NormalDirection", 1),
            "renderAlignment": rend_entry.get("m_RenderAlignment", 0),
        }
        # Material on renderer
        mats = rend_entry.get("m_Materials", [])
        if isinstance(mats, list) and len(mats) > 0 and isinstance(mats[0], dict):
            p["renderer"]["materialFileID"] = mats[0].get("m_FileID", 0)
            p["renderer"]["materialPathID"] = str(mats[0].get("m_PathID", ""))
    
    # Material (named materials from bundle)
    if mat_entry:
        p["material"] = {
            "name": mat_entry.get("m_Name", ""),
            "shaderPathID": str(mat_entry.get("m_Shader", {}).get("m_PathID", "")),
        }
        props = mat_entry.get("m_SavedProperties", {})
        if isinstance(props, dict):
            # Colors
            colors = props.get("m_Colors", [])
            if isinstance(colors, list):
                for c in colors:
                    if isinstance(c, list) and len(c) >= 2:
                        name = c[0]
                        val = c[1]
                        if isinstance(val, dict):
                            p["material"][f"col_{name}"] = {
                                "r": val.get("r", 1), "g": val.get("g", 1),
                                "b": val.get("b", 1), "a": val.get("a", 1)
                            }
            # Floats
            floats = props.get("m_Floats", [])
            if isinstance(floats, list):
                for fv in floats:
                    if isinstance(fv, list) and len(fv) >= 2:
                        p["material"][f"float_{fv[0]}"] = fv[1]
            # Texture refs
            texenvs = props.get("m_TexEnvs", [])
            if isinstance(texenvs, list):
                for te in texenvs:
                    if isinstance(te, list) and len(te) >= 2:
                        tex_name = te[0]
                        tex_val = te[1]
                        if isinstance(tex_val, dict):
                            tref = tex_val.get("m_Texture", {})
                            if isinstance(tref, dict):
                                p["material"]["tex_" + tex_name] = {
                                    "fileID": tref.get("m_FileID", 0),
                                    "pathID": str(tref.get("m_PathID", "")),
                                    "scale": tex_val.get("m_Scale", {"x":1,"y":1}),
                                    "offset": tex_val.get("m_Offset", {"x":0,"y":0}),
                                }
    
    return p

# ============================================================
# PHASE 2: ENHANCED HIERARCHY EXTRACTION
# ============================================================
def classify(entry):
    if not isinstance(entry, dict): return None
    if "m_Component" in entry and "m_Name" in entry and "m_IsActive" in entry: return "GameObject"
    if "m_LocalPosition" in entry and "m_Father" in entry: return "Transform"
    if "InitialModule" in entry: return "ParticleSystem"
    if "m_RenderMode" in entry and "m_Materials" in entry and "m_GameObject" in entry: return "Renderer"
    if "m_Shader" in entry and "m_SavedProperties" in entry: return "Material"
    return None

def extract_prefab_tree(bundle, prefab_name):
    
    # Classify
    gobjs = {}; transforms = {}; particles = {}; renderers = {}; materials = {}
    for pid, entry in bundle.items():
        t = classify(entry)
        if t == "GameObject":
            gobjs[pid] = entry
        elif t == "Transform":
            transforms[pid] = entry
        elif t == "ParticleSystem":
            particles[pid] = entry
        elif t == "Renderer":
            renderers[pid] = entry
        elif t == "Material":
            materials[pid] = entry
    
    # Component → GameObject
    comp_to_go = {}
    for cpid, c in {**transforms, **particles, **renderers}.items():
        mg = c.get("m_GameObject")
        if mg:
            comp_to_go[cpid] = str(mg["m_PathID"])
    
    # GameObject → components
    go_to_comps = {}
    for cpid, gpid in comp_to_go.items():
        go_to_comps.setdefault(gpid, []).append(cpid)
    
    # Transform hierarchy
    child_to_parent = {}
    parent_to_children = {}
    for tpid, tr in transforms.items():
        father = tr.get("m_Father", {})
        if father and father.get("m_PathID", 0) != 0:
            fpid = str(father["m_PathID"])
            child_to_parent[tpid] = fpid
            parent_to_children.setdefault(fpid, []).append(tpid)
    
    # Find root GameObject
    root_pid = None
    for pid, go in gobjs.items():
        if go.get("m_Name") == prefab_name:
            root_pid = pid
            break
    
    if not root_pid:
        print(f"  Prefab '{prefab_name}' not found!")
        return None
    
    # Find root transform
    root_transform = None
    for tpid, tr in transforms.items():
        if comp_to_go.get(tpid) == root_pid:
            root_transform = tpid
            break
    
    # Map material PathID → index in gobjs lookup
    mat_pid_to_entry = {}
    for pid, mat in materials.items():
        mat_pid_to_entry[pid] = mat
    
    def build_children(transform_pid):
        children = []
        for ctpid in parent_to_children.get(transform_pid, []):
            cgpid = comp_to_go.get(ctpid)
            if not cgpid:
                continue
            cgo = gobjs.get(cgpid)
            if not cgo:
                continue
            cname = cgo.get("m_Name", "?")
            ctr = transforms.get(ctpid, {})
            
            # Find particle, renderer, material for this GameObject
            ccomp_ids = go_to_comps.get(cgpid, [])
            ps = None; rend = None; mat = None
            for ccid in ccomp_ids:
                p = particles.get(ccid)
                if p:
                    ps = p; continue
                r = renderers.get(ccid)
                if r:
                    rend = r
                    # Get material
                    rmats = r.get("m_Materials", [])
                    if isinstance(rmats, list) and len(rmats) > 0 and isinstance(rmats[0], dict):
                        mpid = str(rmats[0].get("m_PathID", ""))
                        mat = mat_pid_to_entry.get(mpid)
                    continue
            
            particle_data = extract_full_particle(ps, rend, mat)
            
            node = {
                "name": cname,
                "active": cgo.get("m_IsActive", True),
                "position": {"x": ctr.get("m_LocalPosition", {}).get("x", 0),
                             "y": ctr.get("m_LocalPosition", {}).get("y", 0),
                             "z": ctr.get("m_LocalPosition", {}).get("z", 0)},
                "scale": {"x": ctr.get("m_LocalScale", {}).get("x", 1),
                          "y": ctr.get("m_LocalScale", {}).get("y", 1),
                          "z": ctr.get("m_LocalScale", {}).get("z", 1)},
                "particle": particle_data,
                "children": build_children(ctpid),
            }
            children.append(node)
        return children
    
    root_go = gobjs[root_pid]
    tree = {
        "name": prefab_name,
        "active": root_go.get("m_IsActive", True),
        "children": build_children(root_transform) if root_transform else [],
    }
    return tree

# ============================================================
# PHASE 3: GODOT 4 .tscn GENERATION
# ============================================================
def resolve_texture(mat_name, node_name, particle_data):
    """Determine which texture file to use based on material data."""
    # Check material config
    cfg = MATERIAL_CONFIG.get(mat_name, {})
    if cfg.get("tex_source") == "local" and cfg.get("tex_name"):
        tex = cfg["tex_name"]
        if tex in TEXTURE_PATHS:
            return tex
    
    # Try node name keywords
    name_l = node_name.lower()
    keyword_map = {
        "star": "star_02", "spark": "ray_94_2", "glow": "char_gdglow_02",
        "lighting": "ray_94_2", "flash": "char_gdglow_02",
        "xingdian": "star_02", "baodian": "star_02", "baoshan": "char_gdglow_02",
        "shandian": "char_gdglow_02", "canliu": "char_gdglow_02",
        "tri": "char_gdglow_02", "ring": "char_gdglow_02",
        "splash": "char_gdglow_02", "trail": "char_gdglow_06",
        "ray": "ray_94_2", "flow": "flow_91_C_1",
        "suduxian": "ray_94_2", "ball": "char_gdglow_02",
        "hit": "char_gdglow_02", "bg": "flow_91_C_1",
    }
    for kw, tex in keyword_map.items():
        if kw in name_l:
            return tex
    
    # Particle renderer material-based
    if particle_data:
        renderer = particle_data.get("renderer", {})
        pmat = particle_data.get("material", {})
        mat_name_p = pmat.get("name", "")
        cfg2 = MATERIAL_CONFIG.get(mat_name_p, {})
        if cfg2.get("tex_source") == "local" and cfg2.get("tex_name"):
            return cfg2["tex_name"]
    
    return "char_gdglow_02"  # default fallback

def curve_data_str(keys):
    """Build Godot Curve _data string."""
    if not keys:
        return ""
    pts = []
    for k in keys:
        pts.append(f"Vector2({k['time']}, {k['value']})")
        # flat tangents
        sl = k.get("inSlope", 0)
        pts.append(str(sl))
        sl2 = k.get("outSlope", 0)
        pts.append(str(sl2))
        pts.append("0")  # left mode
        pts.append("0")  # right mode
    return "[" + ", ".join(pts) + "]"

def gradient_colors_str(gradient):
    """Build Gradient colors + offsets strings.
    Returns (colors_str, offsets_str) or None.
    Colors/offsets MUST be sorted and unique.
    """
    if not gradient:
        return None
    
    ck = gradient.get("colorKeys", [])
    ak = gradient.get("alphaKeys", [])
    
    # Build a timeline of (time, r, g, b, a) events
    # Merge color and alpha keys into sorted time points
    timeline = {}  # time -> [r, g, b, a]
    
    # Start with color keys
    for k in ck:
        t = max(0.0, min(1.0, k.get("time", 0)))
        timeline.setdefault(t, [1, 1, 1, 1])
        timeline[t][0] = k.get("r", 1)
        timeline[t][1] = k.get("g", 1)
        timeline[t][2] = k.get("b", 1)
        timeline[t][3] = k.get("a", 1)
    
    # Overlay alpha keys
    for k in ak:
        t = max(0.0, min(1.0, k.get("time", 0)))
        if t not in timeline:
            # Find nearest color key
            if ck:
                nearest = min(ck, key=lambda x: abs(x["time"] - t))
                timeline[t] = [nearest.get("r", 1), nearest.get("g", 1),
                               nearest.get("b", 1), k.get("alpha", 1)]
            else:
                timeline[t] = [1, 1, 1, k.get("alpha", 1)]
        else:
            timeline[t][3] = k.get("alpha", 1)
    
    if not timeline:
        return None
    
    # Sort and ensure endpoints
    sorted_times = sorted(timeline.keys())
    if sorted_times[0] > 0.001:
        timeline[0.0] = [1, 1, 1, 0]
    if sorted_times[-1] < 0.999:
        timeline[1.0] = [1, 1, 1, 0]
    
    sorted_times = sorted(timeline.keys())
    offsets = [str(t) for t in sorted_times]
    colors = []
    for t in sorted_times:
        r, g, b, a = timeline[t]
        colors.extend([str(r), str(g), str(b), str(a)])
    
    return (f"PackedColorArray({', '.join(colors)})",
            f"PackedFloat32Array({', '.join(offsets)})")

class TscnBuilder:
    def __init__(self, name):
        self.name = name
        self.sub_lines = []
        self.ext_lines = []
        self.node_lines = []
        self.used_tex = set()
        self.sub_id = itertools.count(1)
        self.ext_id = itertools.count(1)
    
    def next_sub(self, prefix=""):
        return f"{prefix}_{next(self.sub_id)}"
    
    def next_ext(self):
        return f"tex_{next(self.ext_id)}"
    
    def add_sub(self, lines_block):
        for line in lines_block.strip().split("\n"):
            if line.strip():
                self.sub_lines.append(line)
    
    def add_sub_text(self, text):
        """Add sub-resource text lines (for Curve, Gradient, etc.)"""
        self.sub_lines.append(text)
    
    def add_node(self, line):
        self.node_lines.append(line)
    
    def walk_and_collect(self, nodes, parent_path, depth=0):
        """Recursive walk to create sub-resources and node entries."""
        for node in nodes:
            sname = "".join(c if c.isalnum() or c == "_" else "_" for c in node["name"]).strip("_")
            if not sname:
                sname = f"node_{next(itertools.count(1000))}"
            
            particle = node.get("particle")
            pp = parent_path if parent_path else "."
            child_pp = f"{pp}/{sname}" if pp != "." else sname
            
            is_bone = (particle is None)
            
            if is_bone:
                self.add_node("")
                self.add_node(f'[node name="{sname}" type="Node2D" parent="{pp}"]')
            else:
                p = particle
                mat_name = p.get("material", {}).get("name", "")
                tex_name = resolve_texture(mat_name, node["name"], p)
                self.used_tex.add(tex_name)
                tex_id = self.next_ext()
                self.used_tex = self.used_tex | {tex_name}  # mark for ext_resource
                pm_id = self.next_sub(f"pm_{sname}")
                
                # ============ Compute Godot particle amounts ============
                # Unity: maxParticles is a buffer limit (irrelevant for Godot)
                # Unity: rateOverTime = particles/sec, bursts = count at time
                # Godot: amount = total particles emitted
                # Godot: explosiveness = 0.0 (uniform), 1.0 (all at start)
                
                avg_lt = (p.get("lifetimeMin", 1) + p.get("lifetimeMax", 1)) / 2
                rate = p.get("rateOverTime", 0) or 0
                duration = p.get("duration", 1.0)
                
                burst_total = 0
                for b in p.get("bursts", []):
                    burst_total += int(b.get("count", 0))
                
                if burst_total > 0 and rate > 0:
                    # Mixed: burst + rate
                    rate_total = int(rate * duration)
                    amount = burst_total + max(rate_total, 1)
                    explosiveness = burst_total / max(amount, 1)
                elif burst_total > 0:
                    # Pure burst
                    amount = max(burst_total, 1)
                    explosiveness = 1.0
                elif rate > 0:
                    # Pure rate - continuous particles visible at once
                    amount = max(int(rate * avg_lt * 2), 4)  # x2 safety + min 4
                    explosiveness = 0.0
                else:
                    amount = 1
                    explosiveness = 1.0
                
                looping = p.get("looping", False)
                lifetime = max(avg_lt, 0.01)
                
                # ============ ParticleProcessMaterial ============
                pm = []
                pm.append(f'[sub_resource type="ParticleProcessMaterial" id="{pm_id}"]')
                pm.append("particle_flag_disable_z = true")
                pm.append("gravity = Vector3(0, 0, 0)")
                
                # Speed (allow zero speed for static particles)
                smin = p.get("speedMin", 0) or 0
                smax = p.get("speedMax", 0) or 0
                if abs(smax - smin) < 0.001:
                    smin = max(smin - 0.1, 0)
                    smax = max(smax + 0.1, 0.01)
                pm.append(f"initial_velocity_min = {max(smin, 0):.4f}")
                pm.append(f"initial_velocity_max = {max(smax, 0.01):.4f}")
                
                # Size (allow very small particles)
                szmin = p.get("sizeMin", 0.1) or 0.005
                szmax = p.get("sizeMax", 0.5) or 0.01
                pm.append(f"scale_min = {max(szmin, 0.001):.6f}")
                pm.append(f"scale_max = {max(szmax, 0.002):.6f}")
                
                # Rotation
                rmin = p.get("rotationMin", 0) or 0
                rmax = p.get("rotationMax", 0) or 0
                if abs(rmax - rmin) > 0.001 or rmax > 0.001:
                    pm.append(f"angular_velocity_min = {rmin:.4f}")
                    pm.append(f"angular_velocity_max = {rmax:.4f}")
                
                # Shape
                st = p.get("shapeType", -1)
                if st >= 0 and st in SHAPE_GODOT:
                    shape_line, shape_prop = SHAPE_GODOT[st]
                    pm.append(shape_line)
                    r = p.get("shapeRadius", 0.5)
                    if "cone" in shape_prop:
                        pm.append(f"{shape_prop} = {p.get('shapeAngle', 25):.4f}")
                    elif "sphere" in shape_prop:
                        pm.append(f"{shape_prop} = {max(r, 0.001):.4f}")
                    elif "ring" in shape_prop:
                        pm.append(f"{shape_prop} = {max(r, 0.001):.4f}")
                        if st == 5:
                            pm.append("emission_ring_inner_radius = 1.0")
                
                # Size curve (add BEFORE PM so dependencies come first)
                size_curve = p.get("sizeCurve", [])
                if size_curve and len(size_curve) >= 2:
                    sc_id = self.next_sub(f"sc_{sname}")
                    ctex_id = self.next_sub(f"ct_{sname}")
                    cd = curve_data_str(size_curve)
                    self.sub_lines.append(f'[sub_resource type="Curve" id="{sc_id}"]')
                    self.sub_lines.append(f"min_value = 0.0")
                    self.sub_lines.append(f"max_value = 1.0")
                    self.sub_lines.append(f"_data = {cd}")
                    self.sub_lines.append(f"point_count = {len(size_curve)}")
                    self.sub_lines.append("")
                    self.sub_lines.append(f'[sub_resource type="CurveTexture" id="{ctex_id}"]')
                    self.sub_lines.append(f'curve = SubResource("{sc_id}")')
                    self.sub_lines.append("")
                    pm.append(f'scale_curve = SubResource("{ctex_id}")')
                
                # Color gradient (add BEFORE PM so dependencies come first)
                cgrad = p.get("colorGradient")
                if cgrad:
                    gcs = gradient_colors_str(cgrad)
                    if gcs:
                        gid = self.next_sub(f"cr_{sname}")
                        gtid = self.next_sub(f"gt_{sname}")
                        cols, offs = gcs
                        self.sub_lines.append(f'[sub_resource type="Gradient" id="{gid}"]')
                        self.sub_lines.append(f"offsets = {offs}")
                        self.sub_lines.append(f"colors = {cols}")
                        self.sub_lines.append("")
                        self.sub_lines.append(f'[sub_resource type="GradientTexture1D" id="{gtid}"]')
                        self.sub_lines.append(f'gradient = SubResource("{gid}")')
                        self.sub_lines.append("")
                        pm.append(f'color_ramp = SubResource("{gtid}")')
                
                # Noise
                noise = p.get("noise")
                if noise:
                    pm.append("turbulence_enabled = true")
                    pm.append(f"turbulence_noise_strength = {noise.get('strength', 0.5):.4f}")
                    freq = max(noise.get("frequency", 1.0), 0.01)
                    pm.append(f"turbulence_noise_scale = {1.0/freq:.4f}")
                
                # Add PM AFTER all its dependencies
                self.add_sub("\n".join(pm))
                self.sub_lines.append("")
                
                # ============ GPUParticles2D node ============
                self.add_node("")
                self.add_node(f'[node name="{sname}" type="GPUParticles2D" parent="{pp}"]')
                
                self.add_node(f'emitting = true')
                self.add_node(f'amount = {amount}')
                self.add_node(f'lifetime = {lifetime:.4f}')
                self.add_node(f'one_shot = {"true" if not looping else "false"}')
                self.add_node(f'explosiveness = {explosiveness:.4f}')
                
                self.add_node("local_coords = true")
                
                # Texture (will be resolved in final assembly)
                self.add_node(f'texture = ExtResource("{tex_name}")')
                self.add_node(f'process_material = SubResource("{pm_id}")')
                
                # Sorting
                renderer = p.get("renderer", {})
                so = renderer.get("sortingOrder", 0)
                if so != 0:
                    self.add_node(f"z_index = {so}")
                
                # Trail
                tr = p.get("trail")
                if tr:
                    tl = tr.get("lifetime", lifetime * 1.5)
                    self.add_node(f"trail_lifetime = {tl:.4f}")
                
                # Self modulate from material
                mat_cfg = MATERIAL_CONFIG.get(mat_name, {})
                if mat_cfg.get("self_modulate_color"):
                    r, g, b, a = mat_cfg["self_modulate_color"]
                    self.add_node(f"self_modulate = Color({r:.4f}, {g:.4f}, {b:.4f}, {a:.4f})")
                
                # Flipbook detection (add hframes / vframes)
                fb = p.get("flipbook")
                if fb:
                    tx = fb.get("tilesX", 1)
                    ty = fb.get("tilesY", 1)
                    if tx > 1 or ty > 1:
                        self.add_node(f"h_frames = {ty}")
                        self.add_node(f"v_frames = {tx}")
            
            # Transform
            pos = node.get("position", {})
            scale = node.get("scale", {})
            px = float(pos.get("x", 0)); py = float(pos.get("y", 0))
            sx = float(scale.get("x", 1)); sy = float(scale.get("y", 1))
            
            if abs(px) > 0.0001 or abs(py) > 0.0001:
                self.add_node(f"position = Vector2({px:.4f}, {py:.4f})")
            if abs(sx - 1) > 0.0001 or abs(sy - 1) > 0.0001:
                self.add_node(f"scale = Vector2({sx:.4f}, {sy:.4f})")
            
            # Recurse
            self.walk_and_collect(node.get("children", []), child_pp, depth + 1)
    
    def finalize(self):
        """Assemble the complete .tscn file."""
        lines = []
        uid = hashlib.sha256(self.name.encode()).hexdigest()[:20]
        
        # Count resources
        ext_count = 0
        sub_count = 0
        for line in self.sub_lines:
            if line.startswith("[sub_resource"):
                sub_count += 1
        # We add ext_resources later
        for tex in self.used_tex:
            if tex in TEXTURE_PATHS:
                ext_count += 1
        
        load_steps = 1 + sub_count + ext_count
        
        lines.append(f'[gd_scene load_steps={load_steps} format=3 uid="uid://{uid}"]')
        lines.append("")
        
        # Ext resources
        for tex in sorted(self.used_tex):
            if tex in TEXTURE_PATHS:
                lines.append(f'[ext_resource type="Texture2D" path="{TEXTURE_PATHS[tex]}" id="{tex}"]')
        
        # Sub resources
        lines.extend(self.sub_lines)
        
        # Root node
        root_name = f"vfx_{self.name}"
        lines.append(f'[node name="{root_name}" type="Node2D"]')
        
        # Child nodes (first level children)
        for i_node in self.node_lines:
            lines.append(i_node)
        
        return "\n".join(lines)

# ============================================================
# MAIN PIPELINE
# ============================================================
PREFAB_NAMES = [
    "gdglow_attack_start_01", "gdglow_attack_trail_01", "gdglow_hit_01",
    "gdglow_skill_02_start_01", "gdglow_skill_02_buff_front_01", "gdglow_skill_02_buff_back_01",
    "gdglow_skill_02_trail_01", "gdglow_skill_03_buff_01",
    "gdglow_token_01", "gdglow_token_exp_L_01", "gdglow_token_exp_L_02",
    "gdglow_token_exp_R_01", "gdglow_token_exp_R_02", "gdglow_token_skill_01",
    "gdglow_token_skill_02", "gdglow_token_skill_03", "gdglow_token_hit_L_01",
    "gdglow_token_hit_L_02", "gdglow_token_hit_R_01", "gdglow_token_hit_R_02",
    "gdglow_token_attack_trail_01", "gdglow_token_explosion_01",
    "gdglow_token_hide_01", "gdglow_token_hide_02", "gdglow_token_hide_03",
    "gdglow_weapon_01", "gdglow_weapon_02",
]

# Load JSON once
print("Loading TT_gdglow.ab.json (155MB)...")
with open(AB_JSON, "r", encoding="utf-8", errors="replace") as f:
    all_data = json.load(f)
json_bundle = all_data["gdglow.ab"]
print(f"Done. {len(json_bundle)} entries.")

for pname in PREFAB_NAMES:
    print(f"\nProcessing: {pname}")
    
    # Extract enhanced tree
    tree = extract_prefab_tree(json_bundle, pname)
    if not tree:
        print(f"  FAILED")
        continue
    
    # Save enhanced JSON
    json_out = VFX_DATA / f"{pname}.json"
    with open(json_out, "w", encoding="utf-8") as f:
        json.dump(tree, f, indent=2, ensure_ascii=False)
    print(f"  JSON saved: {json_out}")
    
    # Generate Godot scene
    builder = TscnBuilder(pname)
    builder.walk_and_collect(tree.get("children", []), f"vfx_{pname}")
    tscn = builder.finalize()
    
    tscn_out = OUT_SCENES / f"{pname}.tscn"
    with open(tscn_out, "w", encoding="utf-8") as f:
        f.write(tscn)
    
    node_count = tscn.count('[node name=')
    print(f"  TSCN: {tscn_out} ({node_count} nodes)")

print(f"\nDone! {len(PREFAB_NAMES)} prefabs extracted to {OUT_SCENES}")
