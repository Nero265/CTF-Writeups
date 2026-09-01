# CanYouSee

**Category:** Forensics  
**Difficulty:** Easy  
**Platform:** picoCTF 2024  
**Author:** Mubarak Mikail  
**Tools:** `unzip`, `exiftool`, `base64`  

## Challenge Description
> How about some hide and seek?

We're given a zip archive (`unknown.zip`) containing a single JPEG image and told to find a hidden flag.

## Recon

First, extract the archive:

```bash
unzip unknown.zip
```

This produces a single file, `ukn_reality.jpg`. A quick `ls` confirms the extraction:

```bash
$ ls
ukn_reality.jpg  unknown.zip
```

The filename and challenge title ("hide and seek") both hint that the flag isn't visible in the image itself, but hidden somewhere in the file — most likely in its metadata.

## Vulnerability

JPEG files can carry a large amount of embedded metadata (EXIF, XMP, IPTC) beyond the pixel data itself — camera info, timestamps, GPS coordinates, software tags, and arbitrary custom fields. This metadata is rarely checked or scrubbed by casual viewers, making it a common (and easy) place to stash a flag in forensics challenges.

`exiftool` reads and displays all of this embedded metadata in one pass:

```bash
exiftool ukn_reality.jpg
```

Output (relevant field highlighted):

```
...
XMP Toolkit                     : Image::ExifTool 11.88
Attribution URL                 : cGljb0NURntNRTc0RDQ3QV9ISUREM05fNmE5ZjVhYzR9Cg==
Image Width                     : 4308
...
```

The `Attribution URL` field stands out — it's not a real URL, but a Base64-looking string sitting in a field that would normally hold a link. That's the hidden flag.

## Exploit

Decode the Base64 string found in the `Attribution URL` field:

```bash
echo cGljb0NURntNRTc0RDQ3QV9ISUREM05fNmE5ZjVhYzR9Cg== | base64 -d
```

Output:

```
picoCTF{ME74D47A_HIDD3N_6a9f5ac4}
```

**Flag:** `picoCTF{ME74D47A_HIDD3N_6a9f5ac4}`

## Lessons Learned

- **Always check file metadata first** in forensics challenges — `exiftool` should be a reflex step right after extracting/opening any media file, not a last resort.
- **Unusual field values are the tell.** A Base64 string sitting inside a field that's supposed to hold a plain URL (`Attribution URL`) is a strong signal something's been stashed there — legitimate URLs don't look like that.
- **Flag encoding ≠ flag hiding.** The Base64 encoding here isn't a security measure, just a convenient way to store the flag as printable text inside a metadata field without breaking the file format.
