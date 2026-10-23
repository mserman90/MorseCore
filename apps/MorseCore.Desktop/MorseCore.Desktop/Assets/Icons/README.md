# Application icon

`../Brand/morsecore.svg` is the shared white vector logo. `icon.svg` copies it
without a background tile or clipping. `icon_foreground.svg` deliberately has
no artwork. Uno Resizetizer builds the Windows executable icon from these
sources; Linux packages use the same SVG. The macOS iconset and in-app logos
use the transparent PNG rendered from the shared vector.

The native SVG preserves the original four contours with white fills and no
background geometry. PNG copies retain alpha transparency, including the gaps.

After changing the source SVG, regenerate platform copies (Pillow is required
for PNG rendering):

```sh
python scripts/sync-brand-assets.py
python scripts/sync-macos-resources.py
```

Both scripts accept `--check` to verify generated assets without writing files.
