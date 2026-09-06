# Blast from the Past

**Category:** Forensics  
**Difficulty:** Medium  
**Platform:** picoCTF  

## Challenge Description

> The judge for these pictures is a real fan of antiques. Can you age this photo to the specifications?
> Set the timestamps on this picture to `1970:01:01 00:00:00.001+00:00` with as much precision as possible for each timestamp. Any timezone is acceptable as long as the time is equivalent. For timestamps without a timezone adjustment, put them in GMT time (+00:00).

Provided file: [`original.jpg`](https://artifacts.picoctf.net/c_mimas/73/original.jpg)

Submission/verification was handled entirely by the challenge's own netcat service, not by any local metadata tool:

```bash
# Submit the modified picture
nc -w 2 mimas.picoctf.net 52764 < original_modified.jpg

# Check the result / retrieve the flag
nc mimas.picoctf.net 56629
```

## Tools

- `exiftool` — bulk EXIF date/time editing
- Hex editor — manual patch for the one field `exiftool` wouldn't touch
- `piexif` (Python) — local sanity-check of final tag values before submission
- `nc` (netcat) — official submission/verification channel provided by the challenge

## Recon

Baseline EXIF metadata on the untouched file showed a Samsung SM-A326U photo with three date/time fields, each carrying sub-second precision:

```
DateTime:            2023:11:20 15:46:23
DateTimeOriginal:    2023:11:20 15:46:23
DateTimeDigitized:   2023:11:20 15:46:23
SubSecTime:          703
SubSecTimeOriginal:  703
SubSecTimeDigitized: 703
```

No `OffsetTime*` (timezone) tags were present in the original, and no GPS or thumbnail (`1st` IFD) date fields existed either — so only these three date/time + three sub-second fields needed to match the target.

## Approach

`exiftool` handled the bulk of the rewrite cleanly:

```bash
exiftool "-DateTimeOriginal=1970:01:01 00:00:00" \
         "-DateTimeDigitized=1970:01:01 00:00:00" \
         "-DateTime=1970:01:01 00:00:00" \
         "-SubSecTime=001" \
         "-SubSecTimeOriginal=001" \
         "-SubSecTimeDigitized=001" \
         original.jpg
```

One field refused to update through `exiftool` directly (it kept rejecting/ignoring the write for that particular tag). Rather than fight the tool further, the fix was applied by hand: located the tag's byte offset in the file with a hex editor and overwrote the value directly in the EXIF/TIFF structure.

No `OffsetTime` tags were added — per the challenge's own rule, an absent timezone tag is interpreted as GMT (+00:00), which is exactly the target value, so leaving them unset was the correct (and simplest) choice rather than a gap.

## Pitfall / Bug Hunt

After the hex edit, the file no longer opened as a JPEG (`file` reported it as generic `data` instead of `JPEG image data`). Comparing the patched file byte-for-byte against the original at the head of the file isolated the cause immediately:

```
original:  ff d8 ff e1 ...
patched:   2f d8 ff e1 ...
```

The JPEG SOI (Start of Image) marker `FF D8` had been corrupted to `2F D8` — the leading `FF` was overwritten with `2F` (`/`), almost certainly an off-by-one cursor position in the hex editor while editing the nearby EXIF field. Every other byte in the file, including the trailing EOI marker, was untouched.

The fix was a single-byte patch: restore offset `0x00` to `FF`. Reloading the file afterward with `piexif` (a local, informal check — not the actual grader) confirmed a fully valid JPEG (4000x3000) with the target EXIF values intact:

```
DateTime:            1970:01:01 00:00:00
DateTimeOriginal:    1970:01:01 00:00:00
DateTimeDigitized:   1970:01:01 00:00:00
SubSecTime:          001
SubSecTimeOriginal:  001
SubSecTimeDigitized: 001
```

Combined, these give the required `1970:01:01 00:00:00.001+00:00` for all three timestamp fields.

## Lessons Learned

- **`exiftool` doesn't always cooperate with every tag** — some fields (depending on how the maker-note/TIFF structure is laid out) may need to be patched directly in a hex editor when the high-level tool refuses the write.
- **Manual hex edits carry real risk of off-by-one corruption.** Always re-run `file` (or reload with an image library) immediately after a manual patch to confirm the container format is still intact — a single misplaced byte at the SOI marker is enough to make an otherwise-perfect edit unreadable.
- **Read the spec's fallback rules carefully.** The challenge explicitly stated that a missing timezone offset defaults to GMT — recognizing that meant no `OffsetTime` tags needed to be added at all, avoiding unnecessary extra edits.
- **Diffing the patched file against the original byte-for-byte** (rather than re-reading everything with `exiftool`) was the fastest way to pinpoint exactly which single byte broke the container after the hex edit.
- **The actual grader was a netcat service, not a metadata library** — the challenge only cares whether its own parser on the other end of the socket accepts the timestamps, so any local tool (`exiftool`, `piexif`) is only useful for sanity-checking before submission, not as a stand-in for the real check.
