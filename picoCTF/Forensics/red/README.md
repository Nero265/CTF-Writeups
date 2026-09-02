# picoCTF - RED

**Category:** Forensics  
**Difficulty:** Easy  
**Platform:** picoCTF 2025  
**Author:** Shuailin Pan (LeConjuror)  
**Tools used:** `exiftool`, `strings`, `zsteg`, `base64`  

## Challenge Description

> RED, RED, RED, RED
>
> Download the image: `red.png`

We are given a single PNG image (`red.png`, 128x128, 796 bytes) with no further hints beyond the title.

## Recon

Started with metadata inspection using `exiftool`:

```bash
exiftool red.png
```

This revealed a custom PNG text chunk called **Poem**, containing a short poem about the color red:

```
Poem : Crimson heart, vibrant and bold,.Hearts flutter at your sight..Evenings glow softly red,...
```

A `strings` pass on the file confirmed the same poem text embedded as a `tEXt` chunk, but no flag was visible in plaintext:

```bash
strings red.png
```

At this point the poem itself looked like a red herring — a thematic nod to the "RED" title rather than the actual hiding spot for the flag. Since a PNG with no flag in plaintext or metadata strongly suggests pixel-level steganography, the next step was LSB (Least Significant Bit) analysis.

## Vulnerability

The flag was hidden using **LSB steganography** inside the image's pixel data (RGBA channels), not in the visible metadata. This required a dedicated steg tool rather than generic string/metadata inspection.

`zsteg` wasn't installed by default on Kali, so it had to be installed manually via `gem`:

```bash
sudo apt install ruby ruby-dev -y
sudo gem install zsteg
```

## Exploit

Running `zsteg` against the image scans all bit-plane/channel/order combinations automatically:

```bash
zsteg red.png
```

Among the many outputs (most were noise or false-positive file-type guesses), one channel combination — **`b1,rgba,lsb,xy`** (bit plane 1, RGBA channels, LSB order, XY scan) — returned a clean, repeating Base64 string:

```
b1,rgba,lsb,xy .. text: "cGljb0NURntyM2RfMXNfdGgzX3VsdDFtNHQzX2N1cjNfZjByXzU0ZG4zNTVffQ==..." (repeated 4x)
```

Decoding it with `base64`:

```bash
echo "cGljb0NURntyM2RfMXNfdGgzX3VsdDFtNHQzX2N1cjNfZjByXzU0ZG4zNTVffQ==" | base64 -d
```

**Flag:**
```
picoCTF{r3d_1s_th3_ult1m4t3_cur3_f0r_54dn355_}
```

## Lessons Learned

- **Metadata isn't always the payload.** The embedded "Poem" text chunk was thematically relevant (red = feelings, sadness) but was a distraction; the real data lived in the pixel LSBs.
- **`zsteg` brute-forces channel/bit-plane combinations automatically** — instead of manually guessing R/G/B/A and LSB/MSB order, let the tool enumerate them and scan the output for anything that looks like readable text or a known file signature.
- **Repeated/duplicated output is a good sign**, not noise — when the same Base64 blob appears multiple times across a scan, it usually means the encoding correctly captured the full embedded message across the image dimensions.
- Kali's default toolset doesn't include everything; installing missing steg tools via `gem`/`pip`/`apt` on demand is a normal part of the workflow.
