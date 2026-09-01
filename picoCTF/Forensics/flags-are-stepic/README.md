# flags are stepic

**Category:** Forensics  
**Difficulty:** Medium  
**Platform:** picoCTF 2025  
**Tools:** `wget`, `file`, Python `venv`, `stepic`  

## Challenge Description

> A group of underground hackers might be using this legit site to communicate. Use your forensic techniques to uncover their message.

The challenge provides a website displaying a grid of world flags, each with a name and image.

## Recon

The site (`http://standard-pizzas.picoctf.net:<port>/`) renders a grid of country flags from a JavaScript array of `{ name, img }` objects. Reviewing the page's inline `<script>` source revealed one entry that stands out from the rest:

```js
{ name: "Upanzi, Republic The", img: "flags/upz.png", style:"width: 120px!important; height: 90px!important;" },
```

Three anomalies flagged this entry as suspicious:

1. **Nonexistent country** — "Upanzi" is not a real nation, and the name ordering ("Republic The") is grammatically inverted from the standard convention.
2. **Non-standard filename** — every other flag uses a two-letter ISO 3166 country code (`rs.png`, `us.png`, etc.); `upz.png` does not follow this pattern.
3. **Inline style override** — this is the only entry with a manually forced `width`/`height`, suggesting the underlying image differs significantly in size/dimensions from the rest and had to be visually forced to fit the grid.

Checking the browser's Network tab confirmed the image was **1.79 MB**, versus a few KB for every other flag in the gallery — a strong signal that extra data was embedded in the file.

Downloading and inspecting the file confirmed this:

```bash
wget http://standard-pizzas.picoctf.net:<port>/flags/upz.png
file upz.png
```

```
upz.png: PNG image data, 14173 x 10630, 8-bit/color RGBA, non-interlaced
```

A ~150-million-pixel image being displayed as a 120×90px icon is not a rendering choice — it's evidence the image was artificially inflated to carry a hidden payload.

## Vulnerability

The challenge title itself — **"flags are stepic"** — is the hint: `stepic` is a Python library that performs LSB (Least Significant Bit) steganography via Pillow, hiding arbitrary data inside the low-order bits of an image's pixel data. Because LSB embedding needs a large number of pixels to carry a nontrivial payload, the carrier image (`upz.png`) was blown up to an unusually large resolution to accommodate the hidden flag.

## Exploit

Set up an isolated environment and install `stepic`:

```bash
python3 -m venv venv
source venv/bin/activate
pip install stepic
```

Decode the hidden payload directly from the image:

```bash
stepic -d -i upz.png
```

Output:

```
/.../PIL/Image.py:3578: DecompressionBombWarning: Image size (150658990 pixels) exceeds limit of 89478485 pixels, could be decompression bomb DOS attack.
  warnings.warn(
picoCTF{fl4g_h45_fl4g0e590975}
```

**Flag:** `picoCTF{fl4g_h45_fl4g0e590975}`

Note: Pillow's `DecompressionBombWarning` is a non-fatal `warnings.warn()` call, not a raised exception — execution continues and the decode completes normally despite the warning.

## Lessons Learned

- A large disparity in file size between visually similar assets (KB vs MB) is a strong steganography indicator, worth checking before reaching for any specific tool.
- Challenge titles often contain a direct, literal hint toward the required tool or technique — worth taking at face value rather than overthinking.
- `stepic` is a legacy LSB steganography library for Pillow images; install it in an isolated `venv` since it depends on an older Pillow API surface.
- `DecompressionBombWarning` only blocks execution if explicitly escalated to an error — by default it's informational and safe to ignore for legitimate, expected-large images.
