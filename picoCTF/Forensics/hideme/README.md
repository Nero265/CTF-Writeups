# picoCTF - hideme

**Category:** Forensics  
**Difficulty:** Medium  
**Platform:** picoCTF 2023  
**Author:** Geoffrey Njogu  
**Tools used:** `zsteg`, `foremost`, `7z`, `eog`  

## Challenge Description

> Every file gets a flag.
>
> The SOC analyst saw one image been sent back and forth between two people. They decided to investigate and found out that there was more than what meets the eye.

We are given a single image, `flag.png`.

## Recon

Started by scanning the PNG with `zsteg` to check for common steganography techniques:

```bash
zsteg flag.png
```

`zsteg` immediately flagged something unusual — not in the pixel data itself, but in the raw file structure:

```
[?] 3191 bytes of extra data after image end (IEND), offset = 0x9b3b
extradata:0  .. file: Zip archive data, made by v3.0 UNIX...
```

This is the key finding: PNG files are supposed to terminate at the `IEND` chunk, but `flag.png` had **3191 extra bytes appended after IEND**, and the byte signature (`PK 03 04`) identified this trailing data as a **ZIP archive** embedded inside the image file. The archive's internal listing already hints at the payload — a file called `secret/flag.png`.

## Vulnerability

The challenge exploited the fact that many image viewers and parsers stop reading a PNG as soon as they hit the `IEND` marker, ignoring any bytes appended afterward. This makes it possible to concatenate an entire secondary file (in this case a ZIP archive containing another PNG) onto the end of a valid PNG without breaking the image — file carving is needed to recover the appended data.

## Exploit

Used `foremost` to carve out embedded file signatures from `flag.png` (rather than manually calculating offsets and using `dd`):

```bash
foremost flag.png
```

`foremost` extracted the embedded ZIP into `output/zip/00000077.zip`. Extracting it revealed the hidden path structure:

```bash
cd output/zip
7z x 00000077.zip
cd secret
ls
# flag.png
```

Opened the extracted (inner) `flag.png` to inspect it visually:

```bash
eog flag.png
```

**Flag:**
``` Find some text from image convertor to get flag from flag.png 
```

## Lessons Learned

- **`IEND` doesn't mean end-of-file.** PNG parsers only care about the `IEND` chunk to know where the *image* ends — nothing stops extra bytes from being appended after it, and most viewers will silently ignore them. `zsteg`'s "extra data after image end" warning is a strong signal to always check for this.
- **File carving tools (`foremost`, `binwalk`) beat manual offset hunting.** Instead of computing the exact byte offset of the embedded ZIP header and slicing it out with `dd`, `foremost` automatically detects and extracts embedded file signatures.
- **Nested payloads are common in forensics challenges** — a PNG containing a ZIP containing another PNG is a "matryoshka" pattern worth expecting whenever a file's size looks larger than its visible content would justify.
- Always sanity-check file size vs. expected content size early — an oddly large file for a simple image is often the first clue.
