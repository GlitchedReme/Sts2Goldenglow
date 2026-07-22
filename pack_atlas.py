"""
Pack images from multiple directories into Godot AtlasTextures.
Supports simultaneous card + power atlas packing.

Usage:
    python pack_atlas.py                    # Pack all defaults (card + power)
    python pack_atlas.py card               # Pack card only
    python pack_atlas.py power              # Pack power only
    python pack_atlas.py card power         # Pack both explicitly
    python pack_atlas.py <dir> [options]    # Pack custom directory

Options:
    --padding N          Padding between images in pixels (default: 4)
    --max-size N         Max atlas dimension in pixels (default: 2048)
    --output-dir DIR     Output dir (default: input_dir/../<atlas_name>)
"""

import sys
import argparse
import random
import string
from pathlib import Path

try:
    from PIL import Image
except ImportError:
    print("Pillow is required: pip install Pillow")
    sys.exit(1)

# Default pack targets: (name, input_subdir, output_subdir, strip_suffix)
DEFAULTS = {
    "card": {
        "input": "Goldenglow/image/card",
        "output": "Goldenglow/image/card_atlas",
        "atlas_name": "card_atlas",
        "strip_suffix": None,
    },
    "power": {
        "input": "Goldenglow/image/power",
        "output": "Goldenglow/image/power_atlas",
        "atlas_name": "power_atlas",
        "strip_suffix": "32",
    },
}


def generate_uid() -> str:
    chars = string.ascii_lowercase + string.digits
    return "uid://" + "".join(random.choices(chars, k=6))


def shelf_pack(images, padding: int, max_size: int):
    sorted_imgs = sorted(images, key=lambda x: -x[1].height)

    atlases = []
    current = []
    shelf_y = padding
    shelf_h = 0
    cursor_x = padding

    for name, img in sorted_imgs:
        w = img.width
        h = img.height

        if w + padding * 2 > max_size or h + padding * 2 > max_size:
            print(f"WARNING: {name} ({w}x{h}) exceeds max_size {max_size}, skipping")
            continue

        if cursor_x + w + padding <= max_size:
            x, y = cursor_x, shelf_y
            cursor_x += w + padding
            shelf_h = max(shelf_h, h + padding)
            current.append((name, img, x, y))
        else:
            shelf_y += shelf_h
            cursor_x = padding
            shelf_h = h + padding

            if shelf_y + h + padding > max_size:
                atlases.append(current)
                current = []
                shelf_y = padding
                cursor_x = padding
                shelf_h = h + padding

            x, y = cursor_x, shelf_y
            cursor_x += w + padding
            current.append((name, img, x, y))

    if current:
        atlases.append(current)

    return atlases


def pack_images(input_dir: Path, atlas_name: str, padding: int, max_size: int, output_dir: Path, strip_suffix: str | None = None):
    png_files = sorted(input_dir.glob("*.png"))
    if not png_files:
        print(f"No PNG files found in {input_dir}")
        return

    print(f"Found {len(png_files)} images in {input_dir}")

    images = []
    for f in png_files:
        stem = f.stem
        if strip_suffix and stem.endswith(strip_suffix):
            stem = stem[: -len(strip_suffix)]
        img = Image.open(f).convert("RGBA")
        images.append((stem, img))

    atlases = shelf_pack(images, padding, max_size)
    print(f"Packed into {len(atlases)} atlas(es)\n")

    total_tres = 0
    for atlas_idx, atlas_items in enumerate(atlases):
        atlas_w = max(x + img.width for _, img, x, _ in atlas_items) + padding
        atlas_h = max(y + img.height for _, img, _, y in atlas_items) + padding

        atlas = Image.new("RGBA", (atlas_w, atlas_h), (0, 0, 0, 0))

        atlas_file = f"{atlas_name}_{atlas_idx}"
        atlas_res_path = f"res://Goldenglow/image/{output_dir.name}/{atlas_file}.png"

        print(f"[{atlas_file}] {len(atlas_items)} images, {atlas_w}x{atlas_h}px")

        for name, img, x, y in atlas_items:
            atlas.paste(img, (x, y))

            tres_path = output_dir / f"{name}.tres"
            uid = generate_uid()
            content = (
                f'[gd_resource type="AtlasTexture" load_steps=2 format=3 uid="{uid}"]\n'
                f"\n"
                f'[ext_resource type="Texture2D" path="{atlas_res_path}" id="1"]\n'
                f"\n"
                f"[resource]\n"
                f'atlas = ExtResource("1")\n'
                f"region = Rect2({x}, {y}, {img.width}, {img.height})\n"
            )
            tres_path.write_text(content, encoding="utf-8")
            print(f"  {name}.tres -> {atlas_file}.png region=({x}, {y}, {img.width}, {img.height})")
            total_tres += 1

        atlas_path = output_dir / f"{atlas_file}.png"
        atlas.save(atlas_path)
        print(f"  Saved: {atlas_path}\n")

    print(f"Done! {len(atlases)} atlas(es), {total_tres} .tres files generated.\n")


def main():
    parser = argparse.ArgumentParser(description="Pack images into Godot AtlasTextures")
    parser.add_argument("targets", nargs="*", help="Targets to pack: 'card', 'power', or custom directory paths (default: card power)")
    parser.add_argument("--padding", type=int, default=4, help="Padding between images (default: 4)")
    parser.add_argument("--max-size", type=int, default=2048, help="Max atlas dimension in pixels (default: 2048)")
    args = parser.parse_args()

    targets = args.targets if args.targets else ["card", "power"]
    project_root = Path.cwd()

    for target in targets:
        if target in DEFAULTS:
            cfg = DEFAULTS[target]
            input_dir = project_root / cfg["input"]
            output_dir = project_root / cfg["output"]
            atlas_name = cfg["atlas_name"]
            strip_suffix = cfg["strip_suffix"]
        else:
            input_dir = Path(target)
            if not input_dir.is_absolute():
                input_dir = project_root / input_dir
            atlas_name = input_dir.name + "_atlas"
            output_dir = input_dir.parent / atlas_name
            strip_suffix = None

        if not input_dir.exists():
            print(f"Input directory not found: {input_dir}")
            continue

        output_dir.mkdir(parents=True, exist_ok=True)

        print(f"=== Packing {target} ===")
        pack_images(input_dir, atlas_name, args.padding, args.max_size, output_dir, strip_suffix)


if __name__ == "__main__":
    main()
