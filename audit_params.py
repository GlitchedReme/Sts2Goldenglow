"""
Audit Unity→Godot particle parameter mapping for one effect.
"""
import json

with open(r"D:\Files\Projects\godot\sts2mods\Goldenglow\vfx_data\gdglow_hit_01.json", "r") as f:
    data = json.load(f)

def show(node, depth=0):
    prefix = "  " * depth
    p = node.get("particle")
    if p:
        lt_min = p.get("lifetimeMin", 1)
        lt_max = p.get("lifetimeMax", 1)
        sp_min = p.get("speedMin", 0)
        sp_max = p.get("speedMax", 0)
        sz_min = p.get("sizeMin", 0.1)
        sz_max = p.get("sizeMax", 0.1)
        rate = p.get("rateOverTime", 0)
        bursts = p.get("burstCount", 0)
        rot_min = p.get("rotationMin", 0)
        rot_max = p.get("rotationMax", 0)
        g = p.get("gravity", 0)
        fb = p.get("flipbook")
        noise = p.get("noise")
        tr = p.get("trail")
        shape = p.get("shapeType", -1)
        loop = p.get("looping", False)
        
        # Unity maxNumParticles
        maxp = p.get("maxParticles", 100)
        
        # What Godot should use:
        # - If rate > 0 continuous: amount = rate * avg_lifetime (particles alive at once)
        # - If bursts: amount = sum(burst counts)
        avg_lt = (lt_min + lt_max) / 2
        needed_particles = max(int(rate * avg_lt), 1)
        burst_total = sum(b.get("count", 0) for b in p.get("bursts", []))
        
        lines = []
        lines.append(f"{prefix}{node['name']}:")
        lines.append(f"{prefix}  Unity: maxParticles={maxp}, rate={rate}/s, bursts={bursts}×{burst_total}, looping={loop}")
        lines.append(f"{prefix}  Godot: amount={needed_particles} (rate*avg_lt) or burst={burst_total}")
        lines.append(f"{prefix}  lifetime={lt_min:.3f}-{lt_max:.3f}, speed={sp_min:.3f}-{sp_max:.3f}")
        lines.append(f"{prefix}  size={sz_min:.4f}-{sz_max:.4f}, rotation={rot_min:.2f}-{rot_max:.2f}")
        lines.append(f"{prefix}  shape={shape}, gravity={g:.3f}")
        if fb:
            lines.append(f"{prefix}  flipbook: {fb['tilesX']}x{fb['tilesY']} @{fb['fps']}fps")
        if noise:
            lines.append(f"{prefix}  noise: strength={noise['strength']:.2f}, freq={noise['frequency']:.2f}")
        if tr:
            lines.append(f"{prefix}  trail: lifetime={tr.get('lifetime',0):.3f}")
        
        for line in lines:
            print(line)
    else:
        print(f"{prefix}{node['name']}: (bone)")

    for c in node.get("children", []):
        show(c, depth + 1)

for c in data.get("children", []):
    show(c)

print("\n=== GODOT MAPPING RULES ===")
print("1. amount = max(rateOverTime * avgLifetime, 1) for continuous; = sum(burst.counts) for bursts")
print("2. explosiveness = 0.0 for rate-based, 1.0 for burst-based")
print("3. one_shot = not looping")
print("4. emitting = True (auto-play on scene load)")
print("5. scale_min/max = Unity startSize.minScalar / scalar")
print("6. initial_velocity = Unity startSpeed (Godot uses min/max range)")
print("7. lifetime = avg(startLifetime min, max)")
print("8. angular_velocity = from RotationModule curve min/max")
print("9. gravity = Vector3(0, -Unity.gravityModifier, 0)")
print("10. emission_shape = mapped from Unity ShapeModule.type")
print("11. turbulence = mapped from NoiseModule")
print("12. trail_lifetime = from TrailModule.lifetime")
print("13. h_frames/v_frames = from UVModule.tilesY/tilesX")
print("14. self_modulate = from material _TintColor rgba")
print("15. color_ramp = from ColorModule.gradient (extract color + alpha keys)")
print("16. scale_curve = from SizeModule.curve (extract keyframes)")
