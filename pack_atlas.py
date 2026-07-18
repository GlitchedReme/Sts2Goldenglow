"""
Pack images from a directory into Godot AtlasTextures (atlas PNGs + .tres files).
Uses shelf-based bin packing for tight packing of mixed-size images.
Automatically splits into multiple atlases when exceeding max atlas size.

Usage:
    python pack_atlas.py <input_dir> [options]

Options:
    --atlas-name NAME    Base name for atlas PNGs (default: card_atlas)
    --padding N          Padding between images in pixels (default: 4)
    --max-size N         Max atlas dimension in pixels (default: 2048)
    --output-dir DIR     Output directory (default: input_dir/../<atlas-name>)

Example:
    python pack_atlas.py image/card --atlas-name card_atlas --max-size 2048
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


def generate_uid() -> str:
    chars = string.ascii_lowercase + string.digits
    return "uid://" + "".join(random.choices(chars, k=6))


def shelf_pack(images, padding: int, max_size: int):
    """
    Shelf-based bin packing (NFDH - Next Fit Decreasing Height).
    Returns list of atlases; each atlas is list of (name, img, x, y).
    """
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

        # Try current shelf
        if cursor_x + w + padding <= max_size:
            x, y = cursor_x, shelf_y
            cursor_x += w + padding
            shelf_h = max(shelf_h, h + padding)
            current.append((name, img, x, y))
        else:
            # New shelf
            shelf_y += shelf_h
            cursor_x = padding
            shelf_h = h + padding

            if shelf_y + h + padding > max_size:
                # New atlas
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


def pack_images(input_dir: Path, atlas_name: str, padding: int, max_size: int, output_dir: Path):
    png_files = sorted(input_dir.glob("*.png"))
    if not png_files:
        print(f"No PNG files found in {input_dir}")
        return

    print(f"Found {len(png_files)} images")

    images = []
    for f in png_files:
        img = Image.open(f).convert("RGBA")
        images.append((f.stem, img))

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

    print(f"Done! {len(atlases)} atlas(es), {total_tres} .tres files generated.")


def main():
    parser = argparse.ArgumentParser(description="Pack images into Godot AtlasTextures")
    parser.add_argument("input_dir", type=str, nargs="?", default="Goldenglow/image/card", help="Directory containing PNG files (default: Goldenglow/image/card)")
    parser.add_argument("--atlas-name", type=str, default="card_atlas", help="Base name for atlas PNGs (default: card_atlas)")
    parser.add_argument("--padding", type=int, default=4, help="Padding between images (default: 4)")
    parser.add_argument("--max-size", type=int, default=2048, help="Max atlas dimension in pixels (default: 2048)")
    parser.add_argument("--output-dir", type=str, default=None, help="Output dir (default: input_dir/../<atlas-name>)")
    args = parser.parse_args()

    input_dir = Path(args.input_dir)
    if not input_dir.is_absolute():
        input_dir = Path.cwd() / input_dir

    output_dir = Path(args.output_dir) if args.output_dir else input_dir.parent / args.atlas_name
    if not output_dir.is_absolute():
        output_dir = Path.cwd() / output_dir

    if not input_dir.exists():
        print(f"Input directory not found: {input_dir}")
        sys.exit(1)

    output_dir.mkdir(parents=True, exist_ok=True)

    pack_images(input_dir, args.atlas_name, args.padding, args.max_size, output_dir)


if __name__ == "__main__":
    main()
