# CTF Writeups

A collection of Capture the Flag (CTF) challenge solutions, covering topics such as **network forensics, cryptography, web exploitation, and binary analysis**.  
Each folder contains a dedicated writeup with methodology, tools used, and final flag extraction.

---

## 📂 Structure

### 🌐 Web Exploitation
- **[picoCTF - Old Sessions](./picoCTF/Web-Exploitation/Old-Sessions)** 
  Exploiting misconfigured session expiration and public endpoints to perform Session Hijacking.
- **04-web-len5**  
  Web exploitation challenge — exploiting type confusion in Express.js validation.

### 🔍 Forensics & Steganography
- **01-ftp-forensics**  
  FTP traffic analysis using Wireshark, packet filtering, and hex decoding to extract hidden flag.
- **02-dns-exfiltration**  
  DNS tunneling challenge — detecting exfiltrated data through custom queries.
- **06-hidden-cat**  
  Steganography challenge — extracting hidden flag from PNG image using LSB bit plane analysis.

### ⚙️ Binary Exploitation & Reverse Engineering
- **03-login-form-bof**  
  Binary exploitation challenge — abusing off-by-one overflow in C struct to gain admin shell.
- **05-weaponizedkey**  
  Reverse engineering challenge — analyzing obfuscated PowerShell script to reconstruct hidden API key.
  
*(More challenges will be added as the repository grows.)*

---

## 🛠️ Tools & Techniques
- Wireshark & packet filtering  
- Hex editors & string decoding  
- Linux CLI utilities  
- Cryptography basics (hashing, encoding)  
- Web exploitation methods (Cookie manipulation, Session Hijacking)  
- Python socket scripting  
- Buffer overflow exploitation in C binaries
- Express.js request handling & JSON manipulation  
- Input validation bypass techniques  
- Type confusion exploitation in JavaScript
- PowerShell reverse engineering & deobfuscation  
- CyberChef for decoding and analysis  
- Steganography tools (stegOnline, steghide, zsteg, binwalk)  
- Bit plane analysis of images
  
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
