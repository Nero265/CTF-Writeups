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

### 🔍 Forensics & Steganography
- **01-ftp-forensics**  
  FTP traffic analysis using Wireshark, packet filtering, and hex decoding to extract hidden flag.
- **02-dns-exfiltration**  
  DNS tunneling challenge — detecting exfiltrated data through custom queries.
- **06-hidden-cat**  
  Steganography challenge — extracting hidden flag from PNG image using LSB bit plane analysis.
- **07-git-history-forensics**  
Git repository forensics — recovering a dangling commit removed from history to extract leaked production credentials.
- **08-keefarce-registry-forensics** — Windows Registry forensics — reconstructing a credential-theft and data-exfiltration timeline from NTUSER.DAT hives (UserAssist, RecentDocs, 7-Zip MRU keys) after identifying a KeeFarce memory-dumping attack against KeePass.
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

### Binary Analysis & Reverse Engineering
* Linux binary symbol analysis (`nm`, `objdump`)**
* Buffer overflow exploitation in C binaries (off-by-one)
* PowerShell reverse engineering & deobfuscation

### Programming & Cryptography
* Python socket programming for dynamic network automation**
* Understanding Endianness (Little-Endian memory mapping)**
* Cryptography basics (hashing, encoding, ROT13)
* Steganography tools (`stegOnline`, `steghide`, `zsteg`, `binwalk`) with Bit plane analysis
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
