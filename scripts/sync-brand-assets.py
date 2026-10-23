"""Generate/check shared branding. Rendering needs Pillow; --check uses only stdlib."""

import argparse
import hashlib
import io
import json
import xml.etree.ElementTree as ET
from pathlib import Path


def sync(check=False):
    root = Path(__file__).resolve().parents[1]
    assets = root / 'apps/MorseCore.Desktop/MorseCore.Desktop/Assets'
    source = assets / 'Brand/morsecore.svg'
    # Windows checkouts may use CRLF; hash the same canonical vector everywhere.
    icon = source.read_text(encoding='utf-8-sig').encode('utf-8')
    vector = ET.fromstring(icon)
    polygons = list(vector)
    if len(polygons) != 4 or any(p.tag != '{http://www.w3.org/2000/svg}polygon'
                               or p.attrib.get('fill') != '#FFFFFF' for p in polygons):
        raise ValueError('Expected exactly four white brand polygons without background geometry.')
    manifest = assets / 'Brand/generated.json'
    png = assets / 'Brand/morsecore.png'
    if check:
        recorded = json.loads(manifest.read_text(encoding='utf-8'))
        actual = {'source_sha256': hashlib.sha256(icon).hexdigest(),
                  'png_sha256': hashlib.sha256(png.read_bytes()).hexdigest()}
        if recorded != actual:
            raise SystemExit('Stale branding; run python scripts/sync-brand-assets.py')
        logo = png.read_bytes()
    else:
        from PIL import Image, ImageDraw
        width, height = int(vector.attrib['width']), int(vector.attrib['height'])
        scale = 4
        canvas = Image.new('RGBA', (width * scale, height * scale), (0, 0, 0, 0))
        draw = ImageDraw.Draw(canvas)
        for polygon in polygons:
            points = [tuple(float(v) * scale for v in pair.split(','))
                      for pair in polygon.attrib['points'].split()]
            draw.polygon(points, fill=(255, 255, 255, 255))
        output = io.BytesIO()
        canvas.resize((width, height), Image.Resampling.LANCZOS).save(output, format='PNG')
        logo = output.getvalue()
        manifest.write_text(json.dumps({'source_sha256': hashlib.sha256(icon).hexdigest(),
                                        'png_sha256': hashlib.sha256(logo).hexdigest()}, indent=2) + '\n',
                            encoding='utf-8')
    generated = {assets / 'Icons/icon.svg': icon, png: logo,
                 assets / 'Splash/splash_screen.png': logo,
                 root / 'docs/media/morsecore.svg': icon}
    stale = []
    for target, content in generated.items():
        if check:
            actual_content = (target.read_text(encoding='utf-8-sig').encode('utf-8')
                              if target.suffix == '.svg' and target.is_file()
                              else target.read_bytes() if target.is_file() else None)
            if actual_content != content:
                stale.append(str(target.relative_to(root)))
        else:
            target.write_bytes(content)
    if stale:
        raise SystemExit('Run python scripts/sync-brand-assets.py:\n' + '\n'.join(stale))
    print('Shared brand assets are current.' if check else 'Generated shared brand assets.')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true', help='Check generated copies without writing.')
    sync(parser.parse_args().check)
