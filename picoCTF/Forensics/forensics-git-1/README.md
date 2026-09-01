# Forensics Git 1 — picoCTF 2026
 
**Category:** Forensics  
**Difficulty:** Medium  
**Platform:** picoCTF  
**Author:** LT 'syreal' Jones  
**Tools:** `losetup`, `mount`, `fdisk`, `git`  
 
## Challenge Description
 
> Can you find the flag in this disk image?
> Hint: How can you checkout the files of a previous commit?
 
We are given a compressed raw disk image (`disk.img.gz`) and asked to recover a flag hidden somewhere inside it.
 
## Recon
 
Downloaded and decompressed the disk image:
 
```bash
wget https://challenge-files.picoctf.net/.../disk.img.gz
gunzip disk.img.gz
```
 
Identified the partition layout with `fdisk`:
 
```bash
fdisk -lu disk.img
```
 
```
Device     Boot   Start     End Sectors  Size Id Type
disk.img1  *       2048  616447  614400  300M 83 Linux
disk.img2        616448 1140735  524288  256M 82 Linux swap / Solaris
disk.img3       1140736 2097151  956416  467M 83 Linux
```
 
Three partitions were present: a small Linux root/boot partition, a swap partition (irrelevant for our purposes), and a larger 467M Linux partition — the likely location of user data.
 
Calculated the byte offsets for mounting (`start_sector × 512`):
 
- Partition 1: `2048 × 512 = 1048576`
- Partition 3: `1140736 × 512 = 584056832`
## Mounting the Image
 
Mounted both non-swap partitions **read-only** to avoid altering forensic evidence. Using `mount -o loop` directly on the same image file for multiple partitions caused a loop-device conflict, so loop devices were created explicitly with `losetup` instead:
 
```bash
sudo losetup -o 1048576 /dev/loop0 disk.img
sudo losetup -o 584056832 /dev/loop1 disk.img
 
sudo mount -o ro /dev/loop0 /mnt/part1
sudo mount -o ro /dev/loop1 /mnt/part3
```
 
Searched both partitions for a git repository:
 
```bash
sudo find /mnt/part3 -maxdepth 6 -name ".git" -type d
```
 
```
/mnt/part3/home/ctf-player/Code/secrets/.git
```
 
## Vulnerability
 
The `secrets` repository contained a commit history revealing that a `flag.txt` file had been added and then later removed:
 
```bash
cd /mnt/part3/home/ctf-player/Code/secrets
git log --all --oneline
```
 
```
5fb8194 (HEAD -> master) Remove flag
177789a Add flag
```
 
Removing a file in a new commit does **not** delete its content from git's object database — it only updates the working-tree snapshot referenced by the new commit. The blob containing the flag remains fully intact and retrievable from the earlier commit.
 
## Exploit
 
Inspected the commit that introduced the flag directly, without needing to check it out onto disk (the partition was mounted read-only):
 
```bash
git show 177789a
```
 
```diff
commit 177789af0b300e043ea8f54ea57d6cee352291ae
Author: ctf-player <ctf-player@example.com>
Date:   Wed Nov 19 09:20:05 2025 +0000
 
    Add flag
 
diff --git a/flag.txt b/flag.txt
new file mode 100644
index 0000000..f150f47
--- /dev/null
+++ b/flag.txt
@@ -0,0 +1 @@
+picoCTF{g17_r3m3mb3r5_d4ddf904}
```
 
**Flag:** `picoCTF{g17_r3m3mb3r5_d4ddf904}`
 
## Lessons Learned
 
- Deleting a file and committing that deletion does **not** erase it from git history — the object remains in `.git` until it is explicitly pruned (garbage collected) and no longer referenced.
- `git log --all` is essential when a flag or artifact might be hidden on a branch or commit not currently checked out on `HEAD`.
- `git show <commit>` (or `git show <commit>:<path>`) lets you inspect historical file contents directly, without needing write access to check the commit out onto disk — useful when working with a read-only mounted image.
- When mounting multiple partitions from the same raw disk image file, use `losetup` to create named loop devices explicitly rather than relying on `mount -o loop`, which can produce "overlapping loop device" errors on the second and subsequent mounts.
 
