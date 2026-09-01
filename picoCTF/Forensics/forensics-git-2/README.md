# Forensics Git 2 — picoCTF 2026

**Category:** Forensics  
**Difficulty:** Medium  
**Platform:** picoCTF  
**Tools:** `losetup`, `mount`, `fdisk`, `git`  

## Challenge Description

> The agents interrupted the perpetrator's disk deletion routine. Can you recover this git repo?

Same disk image structure as the previous Forensics Git challenges, but this time the flag is hidden behind a **dangling commit** — a commit that exists in the object database but isn't reachable from any branch or ref.

## Recon

Decompressed the image and confirmed the same partition layout as prior challenges:

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

## Mounting the Image

Mounted partition 3 (offset `1140736 × 512 = 584056832`) read-only:

```bash
sudo mkdir -p /mnt/part3
sudo losetup -o 584056832 /dev/loop0 disk.img
sudo mount -o ro /dev/loop0 /mnt/part3
```

Located the git repository:

```bash
sudo find /mnt/part3 -maxdepth 6 -name ".git" -type d
```

```
/mnt/part3/home/ctf-player/Code/killer-chat-app/.git
```

## Vulnerability

The repository contained a simple netcat-based chat app (`client`, `server` scripts) and a `logs/` directory with chat transcripts:

```bash
cd /mnt/part3/home/ctf-player/Code/killer-chat-app
ls logs
```

```
1.txt  2.txt  4.txt
```

`3.txt` was conspicuously missing from the sequence — a strong signal that a file had been deleted at some point in the project's history.

Running `git log --all --oneline` returned **nothing**, and `git status` showed `No commits yet` with several files staged but uncommitted. This suggested the working tree itself had no commit history — but that didn't mean history didn't exist elsewhere in the object database.

Running `git fsck --lost-found --unreachable` revealed a **dangling commit** — a commit object that exists in `.git/objects/` but isn't pointed to by any branch, tag, or `HEAD`. This is exactly why `git log --all` found nothing: `--all` only walks *refs*, and a dangling commit has none.

```bash
git fsck --lost-found --unreachable
```

```
dangling commit 01533f718556a0e59f1467dae4fa462eed82c2a1
```

## Exploit

Since a dangling commit still has normal parent links, its full history can be walked directly by passing its hash to `git log`:

```bash
git log --oneline 01533f718556a0e59f1467dae4fa462eed82c2a1
```

```
01533f7 Add random chat log
2151ef0 Remove secret hideout log
e80b38b Add secret hideout chat log
5827632 Add TV show chat log
26b809e Add video game chat log
2c0a9b2 Add netcat scripts
```

The commit `2151ef0 Remove secret hideout log` matched the missing `3.txt`. Inspecting it directly:

```bash
git show 2151ef0
```

```diff
commit 2151ef0ccc15aed1ab88e1afdc7484aaeff211c4
Author: ctf-player <ctf-player@example.com>
Date:   Wed Nov 19 10:47:20 2025 +0000

    Remove secret hideout log

diff --git a/logs/3.txt b/logs/3.txt
deleted file mode 100644
index 7178644..0000000
--- a/logs/3.txt
+++ /dev/null
@@ -1,3 +0,0 @@
-Rex: Meet at the old arcade basement for the secret hideout.
-Jay: Ask Rusty at the door and use password picoCTF{g17_r35cu3_16ac6bf3}.
-Rex: Bring the decoder map so we can plan the route.
```

**Flag:** `picoCTF{g17_r35cu3_16ac6bf3}`

## Lessons Learned

- `git log --all` only walks commits reachable from **refs** (branches, tags, `HEAD`). A commit can exist in `.git/objects/` yet be invisible to `--all` if nothing points to it — this is called a **dangling commit**.
- `git fsck --unreachable` (and `--lost-found`) is the right tool to surface dangling/unreachable objects that normal history-walking commands miss.
- Once you have the hash of a dangling commit, `git log <hash>` walks its parent chain directly, revealing full history that isn't attached to any branch.
- A missing file in a numbered sequence (`1.txt`, `2.txt`, `4.txt` — no `3.txt`) is a strong forensic signal worth investigating; gaps in naming conventions often mark deliberately removed evidence.
- Staged-but-uncommitted files still create real blob objects in `.git/objects/` the moment `git add` runs — commit history and the object database are not the same thing.
