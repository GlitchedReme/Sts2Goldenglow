"""
VFX Scene Generator v5 — emits UniParticles3D (Node3D) nodes for the
GDScript UniParticles3D plugin (original), mapping the actual vfx_data
JSON schema to the plugin's exported properties.
"""
import json, re, hashlib
from pathlib import Path

VFX_DATA = Path(r"D:\Files\Projects\godot\sts2mods\Goldenglow\vfx_data")
OUT_SCENES = Path(r"D:\Files\Projects\godot\sts2mods\Goldenglow\Goldenglow\scene\vfx")
OUT_SCENES.mkdir(parents=True, exist_ok=True)

# texture PathID -> name  (from _asset_map.json)
_ASSET_MAP = json.loads((VFX_DATA / "_asset_map.json").read_text(encoding="utf-8"))
TEXTURE_MAP_PID = {str(k): v for k, v in _ASSET_MAP.get("texture_map", {}).items()}

# material name -> texture base name.
# Built by cross-referencing `Unpacked_gdglow_mat/TT_gdglow.ab.json` and
# `gdglow_extracted/materials.md` against the 21 in-bundle Texture2D m_Names.
#   - Base-skin character sprites (char_gdglow_01/02/03/08/17/18) all share the
#     same _MainTex (pathID -5044353931323911249) — only `char_gdglow_02.png`
#     exists in the bundle, so they all fold to it. Verified via materials.md:
#     char_gdglow_01 and char_gdglow_02 both reference _MainTex=-5044353931323911249.
#   - char_gdglow_15 uses char_gdglow_05.png (pathID 8939370247722037650).
#   - char_gdglow_17 and char_gdglow_18 share _MainTex=8468650278168530988.
#   - Skin variants (snow#5_*, summer#12_*, sanrio#1_*) are folded to their
#     texture base: char_gdglow_snow_05/11/12/13, char_gdglow_summer_01/11,
#     flipbook_154/155/216, gdglow_sanrio#1_*, flow_91_C_1, ray_94_2, mask_11.
MATERIAL_NAME_TO_TEX = {
    # base-skin character sprites (folded to existing png)
    "char_gdglow_01": "char_gdglow_02", "char_gdglow_02": "char_gdglow_02",
    "char_gdglow_03": "char_gdglow_02", "char_gdglow_08": "char_gdglow_02",
    "char_gdglow_17": "char_gdglow_02", "char_gdglow_18": "char_gdglow_02",
    "char_gdglow_05": "char_gdglow_05", "char_gdglow_15": "char_gdglow_05",
    "char_gdglow_06": "char_gdglow_06",
    "char_gdglow_11": "char_gdglow_06", "char_gdglow_09": "char_gdglow_06", "char_gdglow_10": "char_gdglow_06",
    # stars / rays / flow / mask
    "star_02": "star_02", "star_02_ab": "star_02", "star_02_add": "star_02",
    "star_05": "star_05", "star_05_add": "star_05", "star_05_add_ZT": "star_05",
    "ray_94_2": "ray_94_2",
    "flow_91_C_1": "flow_91_C_1", "mask_11": "mask_11",
    # snow skin
    "char_gdglow_snow#5_11": "char_gdglow_snow_05",
    "char_gdglow_snow#5_22_2": "char_gdglow_snow_13",
    "char_gdglow_snow#5_23": "char_gdglow_snow_11",
    "char_gdglow_snow#5_24": "char_gdglow_snow_12",
    # summer skin
    "char_gdglow_summer#12_01": "char_gdglow_summer_11",
    "char_gdglow_summer#12_01_1": "flow_91_C_1",
    "char_gdglow_summer#12_01_2": "flow_91_C_1",
    "char_gdglow_summer#12_07": "char_gdglow_summer_01",
    "char_gdglow_summer#12_13": "flipbook_155",
    "char_gdglow_summer#12_14": "flipbook_154",
    "char_gdglow_summer#12_15": "flipbook_155",
    "char_gdglow_summer#12_15_1": "flipbook_155",
    "char_gdglow_summer#12_04_1": "mask_11", "char_gdglow_summer#12_04_2": "mask_11",
    "char_gdglow_summer#12_04_3": "mask_11", "char_gdglow_summer#12_04_4": "mask_11",
    # sanrio skin
    "gdglow_sanrio#1_token_01": "gdglow_sanrio#1_01",
    "gdglow_sanrio#1_ketty_01": "gdglow_sanrio#1_kitty_01",
    "gdglow_sanrio#1_kitty_01": "gdglow_sanrio#1_kitty_01",
    "gdglow_sanrio#1_ray_01": "gdglow_sanrio#1_ray_01",
    "gdglow_sanrio#1_ray_01_1": "gdglow_sanrio#1_ray_01",
    "gdglow_sanrio#1_lizi_02": "gdglow_sanrio#1_lizi_02",
    "gdglow_sanrio#1_sweet_01": "gdglow_sanrio#1_lizi_02",
    "gdglow_sanrio#1_sweet_02": "gdglow_sanrio#1_lizi_02",
    "gdglow_sanrio#1_sweet_03": "gdglow_sanrio#1_lizi_02",
    "gdglow_sanrio#1_sweet_04": "gdglow_sanrio#1_lizi_02",
    "gdglow_sanrio#1_sweet_05": "gdglow_sanrio#1_lizi_02",
    "gdglow_sanrio#1_caihong_01": "ray_94_2",
    "gdglow_sanrio#1_trail_02": "mask_11", "gdglow_sanrio#1_trail_02_1": "mask_11",
}

# Exact node-name -> texture. vfx_data has 228 particles with `material=null`
# (the extractor dropped the renderer's Material reference for base-skin
# prefabs whose materials live in an external bundle). These node names are
# stable across scenes and were cross-referenced against
# `Unpacked_gdglow_mat/TT_gdglow.ab.json` ParticleSystemRenderer table:
# when a renderer with the same GameObject name used an in-bundle Material,
# its m_Name's _MainTex png is used. Names that consistently appear with
# `<external>` Material refs (char_gdglow_02/05/06 group) are mapped by their
# semantic intent — `glow_*`→char_gdglow_06 (verified: glow_01 (2) in ab uses
# char_gdglow_06 Material), `lighting_*`→char_gdglow_02 (char_gdglow_08 → _02),
# `xingdian/baodian/star_*`→star_02, `suduxian/ray/spark_*`→ray_94_2,
# `hit/splash/baoshan/shandian/canliu/ring/tri/sphere_*`→char_gdglow_02
# (character-aura / hit-flash / ring-pulse generic sprite).
NODE_NAME_TEX = {
    # stars / dots / flash
    "baodian": "star_05", "baodian (1)": "star_05", "baodian (2)": "star_05",
    "baodian (3)": "star_05",
    "xingdian": "star_02", "xingdian (1)": "star_02", "xingdian (2)": "star_02",
    "xingdian (3)": "star_02",
    "star": "star_02", "star (1)": "star_02",
    "flash_01 (2)": "star_05",
    "decay_01 (1)": "star_02",
    # rays / sparks / speed-lines / lightning
    "suduxian (2)": "ray_94_2", "suduxian (3)": "ray_94_2",
    "ray_add_01 (4)": "ray_94_2",
    "spark_add_01": "ray_94_2", "spark_add_01 (1)": "ray_94_2",
    "trail_spark_add_01": "ray_94_2",
    "lighting_01": "ray_94_2", "lighting_01 (1)": "ray_94_2",
    # glow (char_gdglow_06 — verified from ab: glow_01 (2) uses char_gdglow_06 Material)
    "glow_01 (1)": "char_gdglow_06", "glow_01 (2)": "char_gdglow_06",
    # ring (ab shows char_gdglow_snow#5_12/13 / summer#12_20 — these are skin
    # variants of the same ring pulse; base skin equivalent is the generic
    # char_gdglow_02 sheet which contains ring sprites)
    "ring_01 (3)": "char_gdglow_02", "ring_01 (4)": "char_gdglow_02",
    "ring_01 (5)": "char_gdglow_02", "ring_01 (6)": "char_gdglow_02",
    "ring_01_center (1)": "char_gdglow_02",
    # hit / splash / sphere (character-aura / impact-flash sprite)
    "hit": "char_gdglow_02",
    "splash_01 (2)": "char_gdglow_02", "splash_01 (3)": "char_gdglow_02",
    "sphere_01 (4)": "char_gdglow_02",
    "baoshan (1)": "char_gdglow_02", "baoshan (2)": "char_gdglow_02", "baoshan (3)": "char_gdglow_02",
    "shandian (4)": "char_gdglow_02", "shandian (5)": "char_gdglow_02",
    "shandianlizi": "char_gdglow_02",
    "canliushandian": "char_gdglow_02", "canliushandian (1)": "char_gdglow_02",
    "token": "char_gdglow_02",
    # skeleton transform nodes (rotation_y/fixed/start/end have particle data
    # but act as lifecycle containers in Unity — their renderer is usually a
    # dummy; default to the main character sheet for any visible emission).
    "rotation_y": "char_gdglow_02",
    "fixed": "char_gdglow_02",
    "start": "char_gdglow_02",
    "end": "char_gdglow_02",
}

KEYWORD_TEX = {
    "baodian": "star_05", "xingdian": "star_02", "star": "star_02",
    "spark": "ray_94_2", "ray": "ray_94_2", "suduxian": "ray_94_2",
    "lighting": "ray_94_2",
    "glow": "char_gdglow_06",
    "shandian": "char_gdglow_02", "baoshan": "char_gdglow_02",
    "splash": "char_gdglow_02", "hit": "char_gdglow_02", "tri": "char_gdglow_02",
    "ring": "char_gdglow_02", "ball": "char_gdglow_02", "canliu": "char_gdglow_02",
    "char": "char_gdglow_02", "trail": "char_gdglow_06",
    "flow": "flow_91_C_1", "bg": "flow_91_C_1", "mask": "mask_11",
}
DEFAULT_TEX = "char_gdglow_02"

TEXTURE_PATHS = {
    "char_gdglow_02": "res://Goldenglow/image/vfx/char_gdglow_02.png",
    "char_gdglow_05": "res://Goldenglow/image/vfx/char_gdglow_05.png",
    "char_gdglow_06": "res://Goldenglow/image/vfx/char_gdglow_06.png",
    "flow_91_C_1": "res://Goldenglow/image/vfx/flow_91_C_1.png",
    "mask_11": "res://Goldenglow/image/vfx/mask_11.png",
    "ray_94_2": "res://Goldenglow/image/vfx/ray_94_2.png",
    "star_02": "res://Goldenglow/image/vfx/star_02.png",
    "star_05": "res://Goldenglow/image/vfx/star_05.png",
    "flipbook_154": "res://Goldenglow/image/vfx/flipbook_154.png",
    "flipbook_155": "res://Goldenglow/image/vfx/flipbook_155.png",
    "flipbook_216": "res://Goldenglow/image/vfx/flipbook_216.png",
    "char_gdglow_snow_05": "res://Goldenglow/image/vfx/char_gdglow_snow_05.png",
    "char_gdglow_snow_11": "res://Goldenglow/image/vfx/char_gdglow_snow_11.png",
    "char_gdglow_snow_12": "res://Goldenglow/image/vfx/char_gdglow_snow_12.png",
    "char_gdglow_snow_13": "res://Goldenglow/image/vfx/char_gdglow_snow_13.png",
    "char_gdglow_summer_01": "res://Goldenglow/image/vfx/char_gdglow_summer_01.png",
    "char_gdglow_summer_11": "res://Goldenglow/image/vfx/char_gdglow_summer_11.png",
    "gdglow_sanrio#1_01": "res://Goldenglow/image/vfx/gdglow_sanrio#1_01.png",
    "gdglow_sanrio#1_kitty_01": "res://Goldenglow/image/vfx/gdglow_sanrio#1_kitty_01.png",
    "gdglow_sanrio#1_lizi_02": "res://Goldenglow/image/vfx/gdglow_sanrio#1_lizi_02.png",
    "gdglow_sanrio#1_ray_01": "res://Goldenglow/image/vfx/gdglow_sanrio#1_ray_01.png",
}

def resolve_texture(node):
    p = node.get("particle") or {}
    m = p.get("material")
    if m:
        # 1) Direct pathID lookup (texture_map covers in-bundle Texture2Ds).
        mt = m.get("tex__MainTex")
        if isinstance(mt, dict):
            pid = str(mt.get("pathID", ""))
            if pid and pid in TEXTURE_MAP_PID:
                return TEXTURE_MAP_PID[pid]
        # 2) Material name lookup (covers cross-bundle materials whose pathID
        #    isn't in texture_map; the name carries the texture semantics).
        mname = (m.get("name") or "").strip()
        base = re.sub(r'(_add|_mul|_screen|_alpha)$', '', mname)
        if base in MATERIAL_NAME_TO_TEX:
            return MATERIAL_NAME_TO_TEX[base]
    # 3) Exact node-name lookup (for the 228 particles whose material was
    #    dropped by extract_vfx.py — node name is the only signal, and it's
    #    stable across scenes; verified against ab's ParticleSystemRenderer
    #    table).
    nname = node.get("name") or ""
    if nname in NODE_NAME_TEX:
        return NODE_NAME_TEX[nname]
    # 4) Substring keyword fallback for unanticipated variants.
    name_l = nname.lower()
    for kw, tex in KEYWORD_TEX.items():
        if kw in name_l:
            return tex
    return DEFAULT_TEX

# Unity ParticleSystemShapeType -> plugin EmissionShape
#   plugin: Cone=0 Sphere=1 Hemisphere=2 Box=3 Circle=4 Edge=5
SHAPE_MAP = {0: 1, 2: 2, 4: 4, 5: 5, 10: 0}
# Unity ParticleSystemRenderMode -> plugin BillboardMode
#   plugin: None=2 Standard=0 Vertical=3 Stretched=1 StretchedVertical=4
#   Unity: 0=Billboard 1=Stretch 2=Horizontal 3=Vertical 4=Mesh 5=None
#   - Mesh (Unity 4): the plugin has no mesh particles; map to Standard so
#     particles stay *visible* (mesh-grid sprite approximation) rather than
#     vanishing (None=2, which suppresses rendering entirely). Visual gap
#     vs Unity is acceptable; donut/pulse revealed instead of invisible.
#   - None (Unity 5): preserve Unity's "no rendering" intent — these nodes
#     act as emission/timing containers; billboard_mode=2 (None) hides them.
RENDER_MODE_MAP = {0: 0, 1: 1, 2: 0, 3: 3, 4: 0, 5: 2}

# ---------------------------------------------------------------------------
# Unity -> GDScript UniParticles3D field mapping (reference for vfx_data JSON)
# ---------------------------------------------------------------------------
# Covered (mapped to exported plugin property):
#   duration, lifetimeMin/Max, speedMin/Max, gravity, sizeMin/Max,
#   rotationMin/Max, startColorMode/Min/Max, maxParticles, rateOverTime,
#   bursts (flat 9-elem array: time,count_mode,min,max,cycle_mode,min_c,
#            max_c,interval,probability, ...), shapeType, shapeRadius,
#   shapeAngle, shapeArc, shapeScale (Box->box_extents),
#   shapeDonutRadius (-> radius_thickness), sizeCurve + sizeCurveScalar,
#   rotationCurveMin/Max (-> rotation_over_lifetime, rad→deg multiplier),
#   colorGradient, noise, trail, flipbook (tiles/frameCurve/startFrame),
#   renderer.renderMode -> billboard_mode (Mesh→Standard for visibility,
#                                          None→None preserved),
#   lengthScale -> length_stretch, velocityScale -> velocity_stretch,
#   sortingOrder -> render_priority, material.name -> texture.
#
# Deliberately unmapped (plugin lacks equivalent; minimal visual impact for
# the gdglow data set):
#   prewarm            — Unity prewarming; ignored, plugin emits fresh.
#   renderer.sortingLayer/sortMode — 2D-only layering keys; render_priority
#                         already covers draw order; Layer/sortMode are
#                         tooltips for the Unity SortingLayer database.
#   renderer.normalDirection, maxParticleSize — unused/blocked constants.
#   renderer.renderAlignment=2/4 (World/Facing, 101 occurrences) — plugin has
#                         no World/Facing billboard option; all sprites render
#                         View-facing. Buff rings may look slightly off-axis.
#   material.float__SrcBlend/DstBlend/Mode — gdglow data uniformly uses
#                         SrcB=1,DstB=0,Mode=0 (mix/normal), so the plugin's
#                         default BlendMode.Mix is correct; add detection here
#                         if a dataset with additive _add materials shows up.
#   flipbook.flipU/flipV — all zero in the data set.
#   trail.textureMode/ribbonCount — plugin uses GPU trails, no ribbon mode.
#   shapeScale for non-Box shapes — Cone/Circle/Sphere only expose radius;
#                         scale-based radius would need shapeScale.x -> radius.

SRE = re.compile(r'[^a-zA-Z0-9_]')
def safe(s):
    return SRE.sub('_', s).strip('_') or 'fx'

def fmt(v):
    if isinstance(v, bool):
        return "true" if v else "false"
    if isinstance(v, int):
        return str(v)
    if isinstance(v, float):
        return f"{v:.6g}"
    return str(v)

def _ffmt(v):
    """Float-only format: always emits a decimal point so Godot parses a float."""
    if isinstance(v, bool):
        return "1.0" if v else "0.0"
    if isinstance(v, int):
        return f"{v}.0"
    if isinstance(v, float):
        if v != v:  # NaN guard
            return "0.0"
        s = f"{v:.6g}"
        if "." not in s and "e" not in s and "inf" not in s:
            s += ".0"
        return s
    return str(v)

def curve_data(points):
    """points: [(t,v,in,out)]. Returns Godot Curve _data string.
    Godot Curve._data layout per point: Vector2(pos,val), left_tan(float),
    right_tan(float), left_mode(int), right_mode(int). Tangents must be floats."""
    parts = []
    for t, v, ins, outs in points:
        parts.append(f"Vector2({_ffmt(t)}, {_ffmt(v)})")
        parts.append(_ffmt(ins)); parts.append(_ffmt(outs))
        parts.append("0"); parts.append("0")
    return "[" + ", ".join(parts) + "]"

def gradient_data(stops):
    """stops: [(offset,(r,g,b,a))]. Returns (colors_str, offsets_str)."""
    stops = sorted(stops, key=lambda s: s[0])
    offsets = [fmt(s[0]) for s in stops]
    colors = []
    for _, (r, g, b, a) in stops:
        colors.extend([fmt(r), fmt(g), fmt(b), fmt(a)])
    return (f"PackedColorArray({', '.join(colors)})",
            f"PackedFloat32Array({', '.join(offsets)})")

class SceneBuilder:
    def __init__(self, prefab_name):
        self.name = prefab_name
        self.ext_resources = {}
        self.ext_counter = 1
        self.sub_resources = []  # (type, id, [lines])
        self.sub_counter = 0
        self.node_lines = []
        self.used_textures = set()

    # Path to the UniParticles3D plugin script. Registered as an ext_resource
    # on first use so particle nodes can bind via `script = ExtResource(...)`.
    # We use Node3D + script binding rather than type="UniParticles3D" because
    # the latter requires Godot's global_script_class_cache to have indexed
    # the addon's `class_name` declaration; referencing the script via
    # ExtResource always works regardless of class registration timing.
    SCRIPT_PATH = "res://addons/UniParticles3D/UniParticles3D.gd"
    _SCRIPT_KEY = "_uniparticles3d_script"

    def script_eid(self):
        """Lazily register the plugin Script ext_resource and return its id."""
        if self._SCRIPT_KEY not in self.ext_resources:
            self.ext_resources[self._SCRIPT_KEY] = f"{self.ext_counter}_script"
            self.ext_counter += 1
        return self.ext_resources[self._SCRIPT_KEY]

    def ext_id(self, tex_name):
        if tex_name not in self.ext_resources:
            self.ext_resources[tex_name] = f"{self.ext_counter}_{tex_name}"
            self.ext_counter += 1
        return self.ext_resources[tex_name]

    def sub_id(self, prefix):
        self.sub_counter += 1
        return f"{prefix}_{self.sub_counter}"

    def add_sub(self, stype, sid, lines):
        self.sub_resources.append((stype, sid, lines))

    def build_curve_sub(self, prefix, points, min_v=0.0, max_v=1.0):
        cid = self.sub_id(prefix)
        lines = []
        if min_v != 0.0:
            lines.append(f"min_value = {fmt(min_v)}")
        if max_v != 1.0:
            lines.append(f"max_value = {fmt(max_v)}")
        lines.append(f"_data = {curve_data(points)}")
        lines.append(f"point_count = {len(points)}")
        self.add_sub("Curve", cid, lines)
        return cid

    def build_gradient_sub(self, prefix, stops):
        gid = self.sub_id(f"g_{prefix}")
        colors, offsets = gradient_data(stops)
        self.add_sub("Gradient", gid, [f"offsets = {offsets}", f"colors = {colors}"])
        gtid = self.sub_id(f"gt_{prefix}")
        self.add_sub("GradientTexture1D", gtid, [f'gradient = SubResource("{gid}")'])
        return gtid

    def build_bursts_flat(self, bursts):
        """Emit bursts as a flat Array for the GDScript plugin.
        Plugin layout per burst (9 elements):
          time, count_mode(0=const), min, max, cycle_mode(0=const),
          min_cycles, max_cycles, particle_interval, probability"""
        parts = []
        for b in bursts:
            # All elements emitted as float literals (via _ffmt) so Godot's
            # tscn parser doesn't choke on mixed int/float types inside the
            # bare `Array([...])` literal. The plugin casts back with float()/int().
            count = b.get("count", 1)
            count_i = int(count) if isinstance(count, (int, float)) else 1
            cycles = int(b.get("cycles", 1))
            parts.append(_ffmt(b.get("time", 0.0)))     # time
            parts.append(_ffmt(0))                       # count_mode = constant
            parts.append(_ffmt(count_i))                 # min count
            parts.append(_ffmt(count_i))                 # max count
            parts.append(_ffmt(0))                        # cycle_mode = constant
            parts.append(_ffmt(cycles))                  # min cycles
            parts.append(_ffmt(cycles))                  # max cycles
            parts.append(_ffmt(0.0))                      # particle_interval
            parts.append(_ffmt(b.get("probability", 1.0)))  # probability
        return "[" + ", ".join(parts) + "]"

    def build_particle_props(self, node):
        """Return list of (key, value) property pairs for a UniParticles3D node."""
        p = node["particle"]
        sname = safe(node["name"])
        props = []

        # ---- Main module ----
        props.append(("enable_main_module", "Vector2i(1, 1)"))
        props.append(("duration", fmt(p.get("duration", 1.0))))

        lmin = p.get("lifetimeMin", 1.0); lmax = p.get("lifetimeMax", 1.0)
        if abs(lmin - lmax) < 1e-6:
            props.append(("start_lifetime_mode", "0"))
            props.append(("start_lifetime_constant", fmt(lmin)))
        else:
            props.append(("start_lifetime_mode", "1"))
            props.append(("start_lifetime_random", f"Vector2({fmt(lmin)}, {fmt(lmax)})"))

        smin = p.get("speedMin", 0.0); smax = p.get("speedMax", 0.0)
        if abs(smin - smax) < 1e-6:
            props.append(("start_speed_mode", "0"))
            props.append(("start_speed_constant", fmt(smin)))
        else:
            props.append(("start_speed_mode", "1"))
            props.append(("start_speed_random", f"Vector2({fmt(smin)}, {fmt(smax)})"))

        grav = p.get("gravity", 0.0)
        if abs(grav) > 1e-6:
            props.append(("gravity", f"Vector3(0, {fmt(-grav)}, 0)"))

        szmin = p.get("sizeMin", 0.15); szmax = p.get("sizeMax", 0.15)
        if abs(szmin - szmax) < 1e-6:
            props.append(("start_size_mode", "0"))
            props.append(("start_size_constant", f"Vector2({fmt(szmin)}, {fmt(szmin)})"))
        else:
            props.append(("start_size_mode", "1"))
            props.append(("start_size_random", f"Vector4({fmt(szmin)}, {fmt(szmin)}, {fmt(szmax)}, {fmt(szmax)})"))
        rmin = p.get("rotationMin", 0.0); rmax = p.get("rotationMax", 0.0)
        # Unity stores rotation as radians, plugin expects degrees.
        rmin_deg = float(rmin) * 180.0 / 3.141592653589793
        rmax_deg = float(rmax) * 180.0 / 3.141592653589793
        if abs(rmin - rmax) < 1e-6:
            props.append(("start_rotation_degrees_mode", "0"))
            if abs(rmin_deg) > 1e-3:
                props.append(("start_rotation_degrees_constant", fmt(rmin_deg)))
        else:
            props.append(("start_rotation_degrees_mode", "1"))
            props.append(("start_rotation_degrees_random", f"Vector2({fmt(rmin_deg)}, {fmt(rmax_deg)})"))

        # ---- Play behavior ----
        props.append(("enable_play_behavior", "Vector2i(1, 1)"))
        looping = bool(p.get("looping", False))
        props.append(("play_on_start", "true"))
        props.append(("loop", "true" if looping else "false"))

        # ---- Emission ----
        props.append(("enable_emission", "Vector2i(1, 1)"))
        props.append(("max_particles", str(int(p.get("maxParticles", 400)))))
        rate = p.get("rateOverTime", 0.0)
        if abs(float(rate)) > 1e-6:
            # Emit as per-time emission rate. NOTE: the previous code computed
            # `rate` but never appended it — silently dropping 93 non-zero rate
            # emitters. Also default-enable rate_over_time even when bursts
            # are present (the plugin supports both simultaneously, matching
            # Unity where rate+bursts coexist).
            props.append(("rate_over_time", fmt(float(rate))))
        bursts = p.get("bursts") or []
        if bursts:
            props.append(("bursts", self.build_bursts_flat(bursts)))

        # ---- Shape ----
        stype = p.get("shapeType")
        if stype is not None and stype in SHAPE_MAP:
            props.append(("enable_shape", "Vector2i(1, 1)"))
            props.append(("shape_type", str(SHAPE_MAP[stype])))
            radius = p.get("shapeRadius", 0.0)
            if radius and abs(radius) > 1e-6:
                props.append(("radius", fmt(radius)))
            # Unity shapeDonutRadius encodes "donut shell thickness ratio" —
            # 0.0 means "surface of the shape only", and Unity's docs describe
            # radius thickness as 0 = surface, 1 = full volume. Our gdglow data
            # uniformly uses 0.2 (a thin shell) — ring pulses rather than solid
            # spheres/balls. The plugin's radius_thickness uses the same 0..1
            # convention (0=surface, 1=volume), so map directly. When absent,
            # keep the plugin default (1.0 = solid) — that matches Unity too.
            donut = p.get("shapeDonutRadius")
            if donut is not None and abs(float(donut)) > 1e-6:
                props.append(("radius_thickness", fmt(float(donut))))
            angle = p.get("shapeAngle", 0.0)
            if angle and abs(angle) > 1e-6:
                props.append(("angle", fmt(angle)))
            arc = p.get("shapeArc", 360.0)
            if arc and abs(arc - 360.0) > 1e-3:
                props.append(("arc_degrees", fmt(arc)))
            sscale = p.get("shapeScale", {})
            if isinstance(sscale, dict):
                bx = sscale.get("x", 1.0); by = sscale.get("y", 1.0); bz = sscale.get("z", 1.0)
                if abs(bx-1) > 1e-3 or abs(by-1) > 1e-3 or abs(bz-1) > 1e-3:
                    # Box uses extents; edge/box shapes map scale -> box_extents
                    if SHAPE_MAP[stype] == 3:
                        props.append(("box_extents", f"Vector3({fmt(bx)}, {fmt(by)}, {fmt(bz)})"))

        # ---- Size over lifetime ----
        size_curve = p.get("sizeCurve")
        if size_curve:
            props.append(("enable_size_over_lifetime", "Vector2i(1, 1)"))
            pts = [(c.get("time", 0), c.get("value", 0),
                    c.get("inSlope", 0), c.get("outSlope", 0)) for c in size_curve]
            scalar = p.get("sizeCurveScalar", 1.0)
            cmin = min((v for _, v, _, _ in pts), default=0.0)
            cmax = max((v for _, v, _, _ in pts), default=1.0)
            if abs(scalar - 1.0) > 1e-3 and cmax > 0:
                cmin = 0.0
                cmax = scalar
            cid = self.build_curve_sub(f"sol_{sname}", pts, cmin, cmax)
            props.append(("size_over_lifetime", f'SubResource("{cid}")'))
        # ---- Rotation over lifetime ----
        # Unity `rotationCurveMin/Max` are angular-velocity curves (rad/s)
        # across the particle lifetime. In our gdglow data both are encoded as
        # single float scalars (not curves): rotationCurveMin=0, rotationCurveMax
        # ≈5.236 rad/s (300°/s) — a constant angular speed. If the value is a
        # float, we emit a constant multiplier and skip the curve; if a future
        # dataset ships actual curves (list-of-dict with time/value), we sample
        # the max as peak and emit a normalized curve alongside.
        rcm = p.get("rotationCurveMax")
        if rcm is not None:
            if isinstance(rcm, (int, float)):
                peak_rad = float(rcm)
                if abs(peak_rad) > 1e-3:
                    peak_deg = peak_rad * 180.0 / 3.141592653589793
                    props.append(("enable_rotation_over_lifetime", "Vector2i(1, 1)"))
                    props.append(("rotation_over_lifetime_multiplier_mode", "0"))
                    props.append(("rotation_over_lifetime_multiplier_constant", fmt(peak_deg)))
                    # Flat 1.0 envelope curve so the multiplier stays constant
                    # across the whole lifetime (matches Unity's constant curve).
                    cid = self.build_curve_sub(f"rol_{sname}",
                        [(0.0, 1.0, 0.0, 0.0), (1.0, 1.0, 0.0, 0.0)], 0.0, 1.0)
                    props.append(("rotation_over_lifetime", f'SubResource("{cid}")'))
            elif isinstance(rcm, list):
                vals = [float(c.get("value", 0.0)) for c in rcm if isinstance(c, dict)]
                if vals:
                    peak_rad = max(vals, key=abs)
                    if abs(peak_rad) > 1e-3:
                        peak_deg = abs(peak_rad) * 180.0 / 3.141592653589793
                        props.append(("enable_rotation_over_lifetime", "Vector2i(1, 1)"))
                        props.append(("rotation_over_lifetime_multiplier_mode", "0"))
                        props.append(("rotation_over_lifetime_multiplier_constant", fmt(peak_deg)))
                        pts = [(c.get("time", 0), c.get("value", 0) / peak_rad if peak_rad else 0,
                                c.get("inSlope", 0), c.get("outSlope", 0)) for c in rcm if isinstance(c, dict)]
                        cid = self.build_curve_sub(f"rol_{sname}", pts, 0.0, 1.0)
                        props.append(("rotation_over_lifetime", f'SubResource("{cid}")'))
        # ---- Color over lifetime ----
        cg = p.get("colorGradient")
        if cg:
            props.append(("enable_color_over_lifetime", "Vector2i(1, 1)"))
            stops = []
            for ck in cg.get("colorKeys", []):
                t = ck.get("time", 0)
                stops.append((t, (ck.get("r", 1), ck.get("g", 1), ck.get("b", 1), ck.get("a", 1))))
            for ak in cg.get("alphaKeys", []):
                t = ak.get("time", 0)
                stops.append((t, (1, 1, 1, ak.get("alpha", 1))))
            # merge stops at same offset by averaging
            if stops:
                merged = {}
                for off, col in stops:
                    if off in merged:
                        old = merged[off]
                        merged[off] = tuple((a+b)/2 for a, b in zip(old, col))
                    else:
                        merged[off] = col
                stops = sorted([(o, c) for o, c in merged.items()])
                gtid = self.build_gradient_sub(f"col_{sname}", stops)
                props.append(("color_over_lifetime", f'SubResource("{gtid}")'))

        # ---- Noise ----
        noise = p.get("noise")
        if noise:
            props.append(("enable_noise_module", "Vector2i(1, 1)"))
            props.append(("noise_strength_mode", "0"))
            props.append(("noise_strength_constant", fmt(noise.get("strength", 0))))
            props.append(("noise_frequency", fmt(noise.get("frequency", 0.5))))
            if "damping" in noise:
                props.append(("noise_damping", "true" if noise.get("damping") else "false"))
            if "octaves" in noise:
                props.append(("noise_octaves", str(int(noise.get("octaves", 1)))))
            if "scrollSpeed" in noise:
                props.append(("noise_scroll_speed_mode", "0"))
                props.append(("noise_scroll_speed_constant", fmt(noise.get("scrollSpeed", 0))))
            if "positionAmount" in noise:
                props.append(("noise_position_amount", fmt(noise.get("positionAmount", 1.0))))
            if "rotationAmount" in noise:
                props.append(("noise_rotation_amount", fmt(noise.get("rotationAmount", 0))))
            if "sizeAmount" in noise:
                props.append(("noise_size_amount", fmt(noise.get("sizeAmount", 0))))

        # ---- Trail ----
        trail = p.get("trail")
        if trail:
            props.append(("enable_trail_module", "Vector2i(1, 1)"))
            props.append(("trail_lifetime_mode", "0"))
            props.append(("trail_lifetime_constant", fmt(trail.get("lifetime", 0.5))))
            if "minVertexDistance" in trail:
                props.append(("trail_min_vertex_distance", fmt(trail.get("minVertexDistance", 0))))
            if "dieWithParticles" in trail:
                props.append(("trail_die_with_particles", "true" if trail.get("dieWithParticles") else "false"))
            if "sizeAffectsWidth" in trail:
                props.append(("trail_size_affects_width", "true" if trail.get("sizeAffectsWidth") else "false"))
            if "inheritParticleColor" in trail:
                props.append(("trail_inherit_particle_color", "true" if trail.get("inheritParticleColor") else "false"))
            wc = trail.get("widthCurve")
            if wc:
                pts = [(c.get("time", 0), c.get("value", 0),
                        c.get("inSlope", 0), c.get("outSlope", 0)) for c in wc]
                cid = self.build_curve_sub(f"tw_{sname}", pts, 0.0, 1.0)
                props.append(("trail_width_over_trail", f'SubResource("{cid}")'))

        # ---- Texture sheet (flipbook) ----------------------------------------
        # Unity flipbook -> plugin Texture Sheet Animation.
        #   rowMode=0 -> WHOLE_SHEET (0); rowMode=1 -> SINGLE_ROW (1).
        #   startFrame ∈ [0,1] is a *normalized* offset, NOT an integer tile
        #   index. For SINGLE_ROW it encodes the chosen row (rowIdx ≈ sf*vFrames);
        #   for WHOLE_SHEET it encodes the linear tile index (≈ sf*H*V).
        #   We disable use_random_starting_tile so start_index_tile actually
        #   takes effect — Unity-baked fx are deterministic animations, and the
        #   plugin would otherwise overwrite start_index_tile with randi().
        #   frameCurve (0..1 value) maps directly to frame_over_time Curve so
        #   non-uniform playback (e.g. 0->1->0 ping-pong) is preserved.
        fb = p.get("flipbook")
        if fb:
            nx = int(fb.get("tilesX", 1)); ny = int(fb.get("tilesY", 1))
            row_mode = int(fb.get("rowMode", 0))
            props.append(("enable_texture_sheet", "Vector2i(1, 1)"))
            props.append(("h_frames", str(nx)))
            props.append(("v_frames", str(ny)))
            props.append(("tiles_mode", "1" if row_mode else "0"))
            if "cycles" in fb:
                props.append(("animation_cycles", fmt(fb.get("cycles", 1.0))))
            # Deterministic animation — disable the plugin's random-tile pick.
            props.append(("use_random_starting_tile", "false"))
            # Resolve normalized startFrame to an integer tile/row index.
            sf = float(fb.get("startFrame", 0.0))
            if row_mode:
                start_idx = int(round(sf * ny)) % ny
            else:
                start_idx = int(round(sf * nx * ny)) % (nx * ny)
            if start_idx > 0:
                props.append(("start_index_tile", str(start_idx)))
            # frameCurve -> frame_over_time Curve (Unity frameCurve value ∈ [0,1]).
            fc = fb.get("frameCurve")
            if fc:
                pts = [(c.get("time", 0), c.get("value", 0),
                        c.get("inSlope", 0), c.get("outSlope", 0)) for c in fc]
                cid = self.build_curve_sub(f"fb_{sname}", pts, 0.0, 1.0)
                props.append(("frame_over_time", f'SubResource("{cid}")'))

        # ---- Rendering ----
        props.append(("enable_rendering", "Vector2i(1, 1)"))
        rend = p.get("renderer") or {}
        rmode = rend.get("renderMode", 0)
        bb = RENDER_MODE_MAP.get(rmode, 0)
        props.append(("billboard_mode", str(bb)))
        if "lengthScale" in rend and abs(rend.get("lengthScale", 0)) > 1e-6:
            props.append(("length_stretch", fmt(rend.get("lengthScale"))))
        if "velocityScale" in rend and abs(rend.get("velocityScale", 0)) > 1e-6:
            props.append(("velocity_stretch", fmt(rend.get("velocityScale"))))
        # Unity renderer.sortingOrder is the 2D-billboard draw order (lower =
        # behind). The plugin's render_priority maps directly; emit only when
        # non-zero to avoid stomping the plugin default. In Unity's 2D battle
        # context sortingOrder is the dominant layering cue — preserving it
        # back-to-front reproduces the original painter's order.
        so = rend.get("sortingOrder", 0)
        if so and abs(int(so)) > 0:
            props.append(("render_priority", str(int(so))))
        # Always set particle_texture — every Unity particle has a visible
        # texture, whether via an explicit material in the JSON (64/292 cases)
        # or via a cross-bundle material ref that extract_vfx.py couldn't
        # capture (228/292). The node-name / keyword fallback resolves those.
        tex_name = resolve_texture(node)
        self.used_textures.add(tex_name)
        tex_eid = self.ext_id(tex_name)
        props.append(("particle_texture", f'ExtResource("{tex_eid}")'))
        # Unity startColorMode: 0 = random between two colors, 2 = single color.
        #   - Mode 2: emit as tint_color (constant).
        #   - Mode 0: emit as a 2-stop start_color_gradient [cmin @ t=0, cmax @ t=1].
        #     The plugin's `_pick_start_color` samples the gradient at a uniform
        #     random t in [0,1], equivalent to lerp(cmin, cmax, rand) — which is
        #     exactly Unity's "random between two colors" semantics. No secondary
        #     gradient is emitted (would double-blend with another random).
        #   - Mode 1 (random between two gradients) is unsupported by vfx_data
        #     sample set and silently ignored here.
        cmode = p.get("startColorMode", 0)
        cmin = p.get("startColorMin", {}); cmax = p.get("startColorMax", {})
        if cmode == 2 and cg is None:
            props.append(("tint_color", f"Color({fmt(cmin.get('r',1))}, {fmt(cmin.get('g',1))}, {fmt(cmin.get('b',1))}, {fmt(cmin.get('a',1))})"))
        elif cmode == 0:
            gtid = self.build_gradient_sub(f"sc_{sname}", [
                (0.0, (cmin.get('r',1), cmin.get('g',1), cmin.get('b',1), cmin.get('a',1))),
                (1.0, (cmax.get('r',1), cmax.get('g',1), cmax.get('b',1), cmax.get('a',1))),
            ])
            props.append(("start_color_gradient", f'SubResource("{gtid}")'))

        return props

    def emit_node(self, node, parent_path):
        """Emit a node (particle -> Node3D + UniParticles3D script, else Node3D)."""
        sname = safe(node["name"])
        is_particle = node.get("particle") is not None
        pp = parent_path if parent_path else "."
        self.node_lines.append("")
        # Particle nodes are Node3D + the plugin script bound via ExtResource,
        # NOT `type="UniParticles3D"` — the class_name form only resolves once
        # Godot's global_script_class_cache has indexed the addon, which the
        # editor may not have done at scene-load time. Script binding always
        # resolves and preserves all exported properties.
        self.node_lines.append(f'[node name="{sname}" type="Node3D" parent="{pp}"]')

        if not node.get("active", True):
            self.node_lines.append("visible = false")

        if is_particle:
            self.node_lines.append(f'script = ExtResource("{self.script_eid()}")')
            for key, val in self.build_particle_props(node):
                self.node_lines.append(f"{key} = {val}")

        # transform (3D)
        pos = node.get("position") or {}
        scale = node.get("scale") or {}
        px = float(pos.get("x", 0)); py = float(pos.get("y", 0)); pz = float(pos.get("z", 0))
        sx = float(scale.get("x", 1)); sy = float(scale.get("y", 1)); sz = float(scale.get("z", 1))
        if abs(px) > 1e-6 or abs(py) > 1e-6 or abs(pz) > 1e-6:
            self.node_lines.append(f"position = Vector3({fmt(px)}, {fmt(py)}, {fmt(pz)})")
        if abs(sx-1) > 1e-3 or abs(sy-1) > 1e-3 or abs(sz-1) > 1e-3:
            self.node_lines.append(f"scale = Vector3({fmt(sx)}, {fmt(sy)}, {fmt(sz)})")

        child_path = f"{pp}/{sname}" if pp != "." else sname
        for c in node.get("children", []):
            self.emit_node(c, child_path)

    def generate(self, root_data):
        root_name = safe(root_data.get("name", "vfx"))
        root_node_name = f"vfx_{root_name}"
        # Root: Node3D + UniParticles3D script acting as top-level controller.
        # It cascades play/stop/preview to all child particle systems when
        # the user clicks Play on it in the editor preview toolbar.
        self.node_lines.append(f'[node name="{root_node_name}" type="Node3D"]')
        self.node_lines.append(f'script = ExtResource("{self.script_eid()}")')
        self.node_lines.append("enable_main_module = Vector2i(1, 1)")
        self.node_lines.append("enable_play_behavior = Vector2i(1, 1)")
        self.node_lines.append("play_on_start = true")
        self.node_lines.append("loop = false")
        self.node_lines.append("max_particles = 0")
        if not root_data.get("active", True):
            self.node_lines.append("visible = false")
        for c in root_data.get("children", []):
            self.emit_node(c, root_node_name)

        # assemble
        ext_count = len(self.ext_resources)
        sub_count = len(self.sub_resources)
        load_steps = 1 + ext_count + sub_count
        uid = hashlib.sha256(self.name.encode()).hexdigest()[:20]
        out = [f'[gd_scene load_steps={load_steps} format=3 uid="uid://{uid}"]', ""]
        # ext resources: Texture2D entries plus one reserved Script binding
        # (the special _SCRIPT_KEY emits type="Script" pointing at the plugin
        # .gd; everything else emits a Texture2D). Sorted by id for stability.
        for key, eid in sorted(self.ext_resources.items(), key=lambda kv: kv[1]):
            if key == self._SCRIPT_KEY:
                out.append(f'[ext_resource type="Script" path="{self.SCRIPT_PATH}" id="{eid}"]')
            else:
                path = TEXTURE_PATHS.get(key, f"res://Goldenglow/image/vfx/{key}.png")
                out.append(f'[ext_resource type="Texture2D" path="{path}" id="{eid}"]')
        # sub resources
        for stype, sid, lines in self.sub_resources:
            out.append("")
            out.append(f'[sub_resource type="{stype}" id="{sid}"]')
            out.extend(lines)
        # nodes
        out.extend(self.node_lines)
        return "\n".join(out) + "\n"

if __name__ == "__main__":
    all_prefabs = sorted(VFX_DATA.glob("gdglow_*.json"))
    total_nodes = 0
    for pfile in all_prefabs:
        name = pfile.stem
        data = json.loads(pfile.read_text(encoding="utf-8"))
        builder = SceneBuilder(name)
        content = builder.generate(data)
        out_path = OUT_SCENES / f"{name}.tscn"
        out_path.write_text(content, encoding="utf-8")
        # count particle nodes
        def count_p(n):
            c = 1 if n.get("particle") else 0
            for ch in n.get("children", []):
                c += count_p(ch)
            return c
        nodes = count_p(data)
        total_nodes += nodes
        print(f"  {name}: {nodes} particle nodes, load_steps={1 + len(builder.ext_resources) + len(builder.sub_resources)}")
    print(f"\nDone! {len(all_prefabs)} scenes regenerated, {total_nodes} particle nodes total.")
