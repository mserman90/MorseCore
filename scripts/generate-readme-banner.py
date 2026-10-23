"""Render the shared white logo and Morse animation used in all five READMEs."""

from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "docs" / "media" / "morsecore.gif"
WHITE, GREEN, MUTED, LINE = "#ffffff", "#5e6a42", "#b4baa6", "#77835c"
WIDTH, HEIGHT = 1040, 232
try:
    font = "C:/Windows/Fonts/georgia.ttf" if Path("C:/Windows/Fonts/georgia.ttf").exists() else "DejaVuSerif.ttf"
    title = ImageFont.truetype(font, 62)
except OSError:
    title = ImageFont.load_default(size=62)
signal = "-- --- .-. ... . / -.-. --- .-. ."
segments = []
x = 0
for character in signal:
    size = {".": 8, "-": 22, " ": 12, "/": 24}[character]
    if character in ".-":
        segments.append((x, size))
    x += size + 6
start = (WIDTH - x) // 2
frames = []
logo = Image.open(ROOT / "apps/MorseCore.Desktop/MorseCore.Desktop/Assets/Brand/morsecore.png").convert("RGBA")
logo = logo.crop(logo.getchannel("A").getbbox())
logo.thumbnail((80, 80), Image.Resampling.LANCZOS)
title_width = ImageDraw.Draw(Image.new("RGB", (1, 1))).textlength("MorseCore", font=title)
brand_x = round((WIDTH - logo.width - 24 - title_width) / 2)
for active in range(len(segments) + 6):
    frame = Image.new("RGB", (WIDTH, HEIGHT), GREEN)
    frame.paste(logo, (brand_x, 62), logo)
    draw = ImageDraw.Draw(frame)
    draw.line((120, 20, WIDTH - 120, 20), fill=LINE, width=1)
    draw.text((brand_x + logo.width + 24, 102), "MorseCore", font=title, fill=WHITE, anchor="lm")
    for index, (position, size) in enumerate(segments):
        draw.rounded_rectangle(
            (start + position, 172, start + position + size, 180),
            radius=4,
            fill=WHITE if index == active else MUTED,
        )
    frames.append(frame)
OUTPUT.parent.mkdir(parents=True, exist_ok=True)
frames[0].save(OUTPUT, save_all=True, append_images=frames[1:], duration=150, loop=0, optimize=True)
print(f"{OUTPUT}: {OUTPUT.stat().st_size:,} bytes, {len(frames)} frames")
