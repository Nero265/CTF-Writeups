# CTF Writeups

A collection of Capture the Flag (CTF) challenge solutions, covering topics such as **network forensics, cryptography, web exploitation, steganography, general skills, and binary analysis**.  
Each folder contains a dedicated writeup with methodology, tools used, and final flag extraction.

---

## 📂 Structure

### 🌐 Web Exploitation
- **[picoCTF - Old Sessions](./picoCTF/Web-Exploitation/Old-Sessions)** 
  Exploiting misconfigured session expiration and public endpoints to perform Session Hijacking.
- **[picoCTF - Crack the Gate 1](./picoCTF/Web-Exploitation/Crack-the-Gate-1)** 
  Analyzing HTML comments, decoding ROT13 cipher, and injecting custom HTTP headers via `curl` to bypass authentication.
- **[picoCTF - SSTI1](./picoCTF/Web-Exploitation/SSTI1)** 
  Identifying Server-Side Template Injection (SSTI) in a Flask/Jinja2 application and escalating it to Remote Code Execution (RCE) to read server files.
- **04-web-len5**  
  Web exploitation challenge — exploiting type confusion in Express.js validation.
- **[picoCTF - Credential Stuffing](./picoCTF/Web-Exploitation/Credential-Stuffing)**  
  Automating login brute-forcing with leaked credential dumps over raw TCP sockets; debugging false positives from partial reads and connection resets under concurrency.
- **[picoCTF - Cookies](./picoCTF/Web-Exploitation/Cookies)**  
  Enumerating a client-controlled cookie value used as an unvalidated server-side array index to leak the flag.
- **[picoCTF - n0s4n1ty 1](./picoCTF/Web-Exploitation/n0s4n1ty-1)**  
  Exploiting an unrestricted file upload to plant a PHP webshell, then escalating via a misconfigured passwordless `sudo` to read the root flag.

### 🔐 Cryptography
- **[picoCTF - New Caesar](./picoCTF/Cryptography/new-caesar)**  
  Reversing a custom Base16 encoding paired with a single-character Caesar shift; exploiting the tiny 16-value keyspace via brute force to recover the flag.

### 🔍 Forensics & Steganography
- **01-ftp-forensics**  
  FTP traffic analysis using Wireshark, packet filtering, and hex decoding to extract hidden flag.
- **02-dns-exfiltration**  
  DNS tunneling challenge — detecting exfiltrated data through custom queries.
- **06-hidden-cat**  
  Steganography challenge — extracting hidden flag from PNG image using LSB bit plane analysis.
- **07-git-history-forensics**  
 Git repository forensics — recovering a dangling commit removed from history to extract leaked production credentials.
- **08-keefarce-registry-forensics**  
  Windows Registry forensics — reconstructing a credential-theft and data-exfiltration timeline from NTUSER.DAT hives (UserAssist, RecentDocs, 7-Zip MRU keys) after identifying a KeeFarce memory-dumping attack against KeePass.
- **[picoCTF - Flag in Flame](./picoCTF/Forensics/Flag%20in%20Flame)**  
  Encoded binary data disguised with a misleading extension
- **[picoCTF - Forensics Git 1](./picoCTF/Forensics/forensics-git-1)**  
  Git repository forensics — recovering a flag from a deleted file via commit history (`git log --all`, `git show`).
- **[picoCTF - Forensics Git 0](./picoCTF/Forensics/forensics-git-0)**  
  Git repository forensics — recovering a flag hidden inside a commit message rather than file content.
- **[picoCTF - Forensics Git 2](./picoCTF/Forensics/forensics-git-2)**  
  Git repository forensics — recovering a flag from a dangling commit unreachable via `git log --all`, using `git fsck --unreachable`.
- **[picoCTF - Rogue Tower](./picoCTF/Forensics/rogue-tower)**  
  Simulated rogue cell tower detection — identifying a fake PLMN beacon and decrypting IMSI-derived XOR-encoded exfiltrated data from HTTP traffic.
- **[picoCTF - Flags are Stepic](./picoCTF/Forensics/flags-are-stepic)**  
  LSB steganography — extracting a flag hidden in an oversized PNG using the Python `stepic` library.

### ⚙️ Binary Exploitation & Reverse Engineering
- **03-login-form-bof**  
  Binary exploitation challenge — abusing off-by-one overflow in C struct to gain admin shell.
- **05-weaponizedkey**  
  Reverse engineering challenge — analyzing obfuscated PowerShell script to reconstruct hidden API key.


 ### 🧠 General Skills & Automation
* **picoCTF - Bytemancy 3** - Reverse engineering and network automation challenge — extracting function memory addresses from a compiled binary and automating dynamic responses with raw Little-Endian bytes via Python sockets.

*(More challenges will be added as the repository grows.)*

---

## 🛠️ Tools & Techniques

### Network & Forensics
* Wireshark & packet filtering
* DNS Tunneling detection
* Hex editors & string decoding
* CyberChef for decoding and analysis

### Git & Version Control Forensics
* `git log --all` for uncovering commits across all branches
* `git show <commit>` for inspecting historical file contents without checkout
* Disk image mounting via `losetup` + `mount -o ro` for read-only forensic analysis

### Windows REgistry & Host Forensics
* Windows Registry hive parsing (regipy, RegRipper)
* UserAssist, RecentDocs, TypedPaths, ShellBags MRU analysis
* Transaction log recovery (NTUSER.DAT.LOG1/LOG2 replay)
* Credential-theft artifact identification(KeeFarce style memory dumping)

### Web Exploitation
* Cookie manipulation & Session Hijacking
* Server-Side Template Injection (SSTI) & RCE
* Express.js request handling & JSON manipulation
* Input validation bypass & Type confusion exploitation
* Credential stuffing & automated login brute-forcing via raw sockets
* Unrestricted file upload → PHP webshell (RCE)
* Privilege escalation via misconfigured `sudo` (`NOPASSWD: ALL`)

### Binary Analysis & Reverse Engineering
* Linux binary symbol analysis (`nm`, `objdump`)**
* Buffer overflow exploitation in C binaries (off-by-one)
* PowerShell reverse engineering & deobfuscation

### Programming & Cryptography
* Python socket programming for dynamic network automation
* Understanding Endianness (Little-Endian memory mapping)
* Cryptography basics (hashing, encoding, ROT13)
* Custom encoding schemes & Caesar cipher brute-forcing (small keyspace analysis)
* Steganography tools (`stegOnline`, `steghide`, `zsteg`, `binwalk`, `stepic`) with bit plane and LSB (Least Significant Bit) analysis
---

## 🎯 Purpose
This repository serves as:
- A **learning resource** for security enthusiasts.  
- A **portfolio showcase** of practical problem‑solving in cybersecurity.  
- A **reference** for common forensic and exploitation techniques.

---

## 🔗 Author
Maintained by **Nenad Bogdanović**  
GitHub: [Nero265](https://github.com/Nero265)
