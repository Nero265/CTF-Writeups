# CTF: Hidden Cat — Steganography

**Category:** Steganography  
**Platform:** CyberHero Playground  
**Tools:** stegOnline (georgeom.net/StegOnline)

---

## Challenge Description

A PNG image of a cat is provided. The image looks completely normal
to the naked eye. The goal is to find the flag hidden inside the image.

**Provided files:** `hidden_cat.png`

---

## Background: LSB Steganography

Steganography is the practice of hiding data inside other data.
In digital images, a common technique is **LSB (Least Significant Bit)**
steganography — hiding information in the lowest-order bits of each
pixel's color channels.

Each pixel in an image has color channels (Red, Green, Blue), each
stored as an 8-bit value (0-255). The **least significant bits**
contribute minimally to the visible color, so flipping them causes
no noticeable change to the image — but can carry hidden data.

**Bit plane 0** refers to the least significant bit of each pixel —
the most common hiding place in LSB steganography.

---

## Methodology

### Step 1 — Recognize the challenge type

The filename `hidden_cat.png` hints at hidden data. PNG is a
lossless format — unlike JPEG, it preserves exact pixel values,
making it suitable for LSB steganography.

### Step 2 — Open stegOnline

Navigate to [georgeom.net/StegOnline](https://georgeom.net/StegOnline)
and upload `hidden_cat.png`.

### Step 3 — Browse Bit Planes

Select **Browse Bit Planes** from the menu.

Examine the **Red channel, Bit Plane 0** (least significant bit).
The hidden data becomes visible as a pattern in the image — 
the flag text appears directly in the bit plane view.

### Step 4 — Extract the flag

```
SCC{h1dd3n_1n_pl41n_516h7}
```

Decoded from leet speak: **"hidden in plain sight"**

---

## Flag

```
SCC{h1dd3n_1n_pl41n_516h7}
```

---

## Tools Used

| Tool | Purpose | URL |
|------|---------|-----|
| stegOnline | Bit plane analysis and LSB visualization | georgeom.net/StegOnline |

---

## Lessons Learned

- Always check file names for hints — "hidden_cat" signals steganography
- PNG is preferred over JPEG for LSB steganography because JPEG
  compression destroys LSB data
- Bit plane 0 (LSB) is the most common hiding place — always check it first
- stegOnline is a quick first tool; for deeper analysis use
  `steghide`, `zsteg`, or `binwalk`
- Steganography is used in real-world scenarios for covert
  communication and data exfiltration — detecting it is a
  relevant forensics skill
