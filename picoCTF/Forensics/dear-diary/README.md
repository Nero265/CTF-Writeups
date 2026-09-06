# Dear Diary

**Category:** Forensics  
**Difficulty:** Medium  
**Platform:** picoCTF 2024  
**Author:** syreal  

## Challenge Description

> If you can find the flag on this disk image, we can close the case for good!

Provided file: [`disk.flag.img`](https://artifacts.picoctf.net/c_titan/63/disk.flag.img.gz)

## Tools

- Autopsy Forensic Browser (TSK-based GUI front-end)
- Underlying TSK CLI tools invoked by Autopsy: `img_stat`, `mmls`, `fsstat`, `blkls`, `blkcat`, `srch_strings`

## Recon

Created a new Autopsy case (`flag_inv`) and added the image as a host (`host1`), then ran the standard image/volume identification sequence before touching file contents:

```bash
img_stat -t disk.flag.img
mmstat -i raw disk.flag.img
mmls -a -i raw -aM -t dos -r disk.flag.img
```

`mmls` revealed a DOS partition table with three volumes:

| Volume | Offset (sectors) | Size (sectors) | Filesystem | Mount |
|---|---|---|---|---|
| vol2 | 2048 | 616447 | `ext` | `/1/` |
| vol3 | 616448 | 1140735 | `raw` | `/2/` |
| vol4 | 1140736 | 2097151 | `ext` | `/3/` |

The middle partition (`vol3`) reporting as `raw` (no recognizable filesystem) stood out — confirmed with `fsstat` on each offset, which only returned meaningful output for the two `ext` volumes:

```bash
fsstat -o 2048    -i raw -f ext disk.flag.img   # vol2 — OK
fsstat -o 1140736 -i raw -f ext disk.flag.img   # vol4 — OK
fsstat -o 616448   -i raw          disk.flag.img   # vol3 — no valid filesystem
```

## Investigation

Rather than trust the ext4 directory trees alone, the search went straight at the **raw block layer** so nothing hidden in unallocated/unlinked space would be missed. This dumps every block of the entire raw device (not just live files) and greps for a filename pattern:

```bash
blkls -e -f raw -o 0 -i raw disk.flag.img | srch_strings -a -t d | grep '\.txt'
```

The first pass (case-sensitive) and a second pass (`grep -i '\.txt'`) both returned noise, so the search was iterated — testing Unicode vs. ASCII string extraction, minimum string length (`-3`), and encoding flags (`-e l`) — before settling on a clean **case-insensitive ASCII** search for `.txt`. That search surfaced two categories of hits:

- **Red herrings**: `clock.txt`, `root.txt`, `misc.txt`, `headline.txt`, `startup.txt` — plausible-looking filenames scattered through unallocated space, each a single isolated hit.
- **The real lead**: a tight, repeating cluster of **14 hits all named `file.txt`**, at raw block units `1423302, 1423328, 1423344, 1423356, 1423374, 1423392, 1423410, 1423422, 1423440, 1423452, 1423470, 1423488, 1423500` — plus one earlier, isolated `file.txt` hit at unit `1171940`.

That repeating filename, densely packed in a narrow, otherwise-unallocated block range, was the signature of a deliberately fragmented file: many small unlinked/deleted `file.txt` instances sitting back-to-back rather than one normal file.

## Exploit / Extraction

Each hit was pulled and displayed directly from its raw block unit with `blkcat`, piping the type check through `file` first before dumping ASCII content:

```bash
blkcat -f raw -o 0 -i raw disk.flag.img <unit> | file -z -b -
blkcat -f raw -a -o 0 -i raw disk.flag.img <unit> 1
```

Repeating this for the lone hit at unit `1171940` and then, in ascending order, for the `1423302`–`1423500` cluster, each unit yielded a short ASCII fragment. Concatenating the fragments in the same ascending block order they were found reassembled the flag:

```
picoCTF{1_533_n4m35_80d24b30}
```

## Lessons Learned

- **`mmls` first, always** — spotting the middle partition reporting as `raw` immediately flagged it as worth extra attention, even though the flag ultimately sat in unallocated space within one of the `ext` volumes rather than inside the unrecognized partition itself.
- **Search the raw device, not just the filesystem tree.** `blkls -e` (allocated + unallocated blocks) piped through `srch_strings`/`grep` catches file fragments that were deleted, unlinked, or never had a directory entry at all — invisible to a normal file browser pass.
- **Repeated identical filenames clustered tightly together is a strong signal of intentional file fragmentation** — a real, single `file.txt` wouldn't appear a dozen times back-to-back at evenly-spaced block offsets; that pattern itself was the clue to keep pulling every hit in the cluster rather than stopping at the first one.
- **Iterate on string-search parameters deliberately.** Case sensitivity, encoding (ASCII vs. Unicode), and minimum string length each changed the noise floor significantly — the first one or two attempts returning nothing useful didn't mean the technique was wrong, just under-tuned.
