# Timeline 1

**Category:** Forensics  
**Difficulty:** Medium  
**Platform:** picoCTF 2026  
**Author:** LT 'syreal' Jones  

## Challenge Description

> Can you find the flag in this disk image?

Provided file: `partition4.img.gz`

## Tools

- `file` — identify filesystem type
- The Sleuth Kit: `fsstat`, `fls`, `mactime`, `icat`
- `base64`

## Recon

Extracted the archive and identified the image with `file`:

```bash
file partition4.img
```

```
partition4.img: Linux rev 1.0 ext4 filesystem data, UUID=7a00e9da-98f8-4f0f-b257-95edf422d902 (extents) (64bit) (large files) (huge files)
```

This confirmed the provided file is a raw ext4 partition (not a full disk image with a partition table), so no `mmls` step was needed — TSK tools could target it directly.

Ran `fsstat` to get filesystem-level metadata before doing anything else:

```bash
fsstat partition4.img
```

Key findings:

```
Last Written at:   2025-12-01 22:42:39 (CET)
Last Mounted at:   2025-12-01 22:41:31 (CET)
Unmounted properly
```

This gave a strong time anchor: any suspicious activity should fall in the narrow window right around **2025-12-01, ~21:41–21:50 UTC**.

## Vulnerability / Investigation

Since the challenge name is a direct hint, the approach was to build a full MAC(B) timeline of the filesystem rather than grep for files by name.

**1. Generate the body file** (list every inode with MAC-B timestamps):

```bash
fls -r -m / partition4.img > body.txt
```

**2. Convert to a human-readable timeline:**

```bash
mactime -b body.txt -d -z UTC > timeline.csv
```

**Lesson learned mid-investigation:** `mactime -d` prints dates as `Mon Dec 01 2025`, not ISO format (`2025-12-01`). An initial `grep "2025-12-01" timeline.csv` returned nothing because of this — the correct pattern needed the `Mon Dec 01 2025` style, or better, filtering on the `macb` column to isolate freshly-created files with identical Modify/Access/Change/Birth times:

```bash
grep "macb" timeline.csv
```

Scrolling through the `macb`-only view (files created and never touched again) surfaced two anomalies right after the tail end of normal Alpine/OpenRC boot + `apk` package provisioning (which ran from `21:41:33` to `21:42:05`):

```
Mon Dec 01 2025 21:50:07,49,macb,r/rrw-r--r--,0,0,32716,"/etc/chat"
Mon Dec 01 2025 21:50:23,9,macb,r/rr--------,0,0,4943,"/root/.ash_history"
```

Both stood out for the same reasons:

- **Not part of the OS/package baseline** — `/etc/chat` has no business existing on a stock Alpine image, and both files were created ~5–8 minutes after the system finished booting/provisioning, in their own isolated timestamp cluster.
- **Identical MACB timestamps** (`macb`) — created once, never modified/accessed again afterward. Classic signature of a file being dropped in a single action rather than being part of normal OS activity.
- **Tiny sizes** (49 and 9 bytes) — consistent with a short embedded flag rather than any legitimate config or log content.

`/etc/chat` (inode 32716) was the clear candidate to pull first.

## Exploit / Extraction

Extracted the file content directly from the raw filesystem image by inode number, without mounting:

```bash
icat partition4.img 32716
```

Output:

```
NTczNDE3aDEzcl83aDRuXzdoM18xNDU3XzU4NTI3YmIyMjIK
```

Base64-decoded:

```bash
echo NTczNDE3aDEzcl83aDRuXzdoM18xNDU3XzU4NTI3YmIyMjIK | base64 -d
```

```
573417h13r_7h4n_7h3_1457_58527bb222
```

Wrapped in the picoCTF flag format:

```
picoCTF{573417h13r_7h4n_7h3_1457_58527bb222}
```

(Leet-speak for "smarter than the last" — a nod to timeline analysis catching what a naive file search would miss.)

## Lessons Learned

- **`mactime -d` date format**: prints `Mon Dec 01 2025`, not ISO — `grep` patterns must account for this, or filter by other fields instead (e.g. the `macb` flag column) to sidestep the issue entirely.
- **Filtering on the `macb` column** is a fast way to spot files that were created once and never touched again — a strong signal for planted/dropped files versus normal filesystem churn.
- **`fsstat`'s "Last Mounted"/"Last Written" timestamps** are a cheap first step to scope the time window before diving into the full timeline — they immediately separate "OS provisioning noise" from the narrow post-boot window worth focusing on.
- **`icat` reads by inode, not by path** — once a suspicious inode is found in the timeline, content can be pulled straight from the raw image without ever mounting the filesystem.
