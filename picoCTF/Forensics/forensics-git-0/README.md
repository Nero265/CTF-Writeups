# Forensics Git 0 — picoCTF 2026
 
**Category:** Forensics  
**Difficulty:** Medium  
**Platform:** picoCTF  
**Tools:** `losetup`, `mount`, `fdisk`, `git`  
 
## Challenge Description
 
> Can you find the flag in this disk image?
 
We are given a compressed raw disk image (`disk.img.gz`), similar in structure to Forensics Git 1, and asked to recover a hidden flag.
 
## Recon
 
Decompressed the image and inspected its partition layout:
 
```bash
gunzip disk.img.gz
fdisk -lu disk.img
```
 
```
Device     Boot   Start     End Sectors  Size Id Type
disk.img1  *       2048  616447  614400  300M 83 Linux
disk.img2        616448 1140735  524288  256M 82 Linux swap / Solaris
disk.img3       1140736 2097151  956416  467M 83 Linux
```
 
Same layout as Forensics Git 1 — a 300M Linux partition, a swap partition, and a 467M Linux partition likely holding user data.
 
## Mounting the Image
 
Mounted the third partition (offset `1140736 × 512 = 584056832`) read-only using an explicit loop device:
 
```bash
sudo mkdir -p /mnt/part3
sudo losetup -o 584056832 /dev/loop1 disk.img
sudo mount -o ro /dev/loop1 /mnt/part3
```
 
Located the git repository:
 
```bash
sudo find /mnt/part3 -maxdepth 6 -name ".git" -type d
```
 
```
/mnt/part3/home/ctf-player/Code/secrets/.git
```
 
## Vulnerability
 
Inside the repository, a `note.txt` file gave a hint about the flag's format:
 
```bash
cd /mnt/part3/home/ctf-player/Code/secrets
cat note.txt
```
 
```
The picoCTF flag format is 'picoCTF{}' where there is some leetspeak phrase in between the curly braces
```
 
Unlike Forensics Git 1 (where the flag was in a deleted file's content), here the flag was hidden directly inside the **commit message** itself — a reminder that git forensics isn't limited to file diffs; commit metadata (messages, author info, timestamps) can also leak sensitive data.
 
## Exploit
 
```bash
git log --all --oneline
```
 
```
327681b (HEAD -> master) Wrap this phrase in the flag format: g17_1n_7h3_d15k_041217d8
```
 
The commit message explicitly instructs how to build the flag — take the leetspeak phrase and wrap it in the `picoCTF{}` format given by `note.txt`.
 
**Flag:** `picoCTF{g17_1n_7h3_d15k_041217d8}`
 
## Lessons Learned
 
- Sensitive data in git repositories isn't only hidden in file contents — **commit messages** are just as readable via `git log` and can leak secrets, credentials, or (in this case) flags directly.
- Always read any accompanying notes/README files in a repository before diving into history — `note.txt` here gave the exact flag format needed to correctly wrap the found phrase.
- `git log --all --oneline` remains the fastest first step for any git forensics challenge — it surfaces every commit across every branch in a compact, scannable form.
