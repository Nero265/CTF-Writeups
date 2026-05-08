# CTF: FTP Network Forensics

**Category:** Network Forensics  
**Platform:** CyberHero Playground  
**Tools:** Wireshark, Python  

---

## Challenge Description

A `.pcap` capture file is provided. The goal is to analyze the network traffic, identify the FTP communication, extract transferred files, and recover the hidden flag.

---

## Tools Used

- **Wireshark** — packet capture analysis and FTP object export
- **Python 3** — hex decoding of the flag

---

## Methodology

### Step 1 — Open the capture and filter FTP traffic

Open `chall.pcap` in Wireshark and apply the display filter:

```
ftp || ftp-data
```

This isolates all FTP control messages and data transfers from the rest of the traffic (ARP, SSDP, TCP handshakes).

### Step 2 — Inspect FTP control stream

Examining the FTP command sequence reveals:
- Server: `192.168.116.135` running **vsFTPd 3.0.5**
- Client: `192.168.116.1`

Critically, because FTP transmits credentials in plaintext, they are fully visible in the capture:

```
USER CyberHero
PASS sajberheroj
```

This is the core vulnerability the challenge is built around — FTP provides zero confidentiality.

The client then navigates the server and retrieves several files:

| File | Size | Location |
|------|------|----------|
| `objective.txt` | 421 bytes | `/` |
| `plans.txt` | 473 bytes | `/` |
| `secret.txt` | 499 bytes | `/` |
| `sajberheroj.png` | 165538 bytes | `/images/` |
| `flag.png` | 59364 bytes | `/images/` |

### Step 3 — Export FTP objects

In Wireshark, go to:

```
File → Export Objects → FTP-DATA
```

Save all listed files. This reconstructs the files from the raw TCP stream.

### Step 4 — Analyze extracted files

Inspecting the text files:

- `objective.txt` — describes a fictional mission to infiltrate a secure server
- `plans.txt` — step-by-step operation plan
- `secret.txt` — contains the encoded flag:

```
Encoded Secret Flag: 5343437b6637705f377234666631635f6d7535375f62335f336e637279703733647d
```

### Step 5 — Decode the flag

The string is hex-encoded. Decode it with Python:

```python
bytes.fromhex("5343437b6637705f377234666631635f6d7535375f62335f336e637279703733647d").decode()
```

**Output:**

```
SCC{f7p_7r4ff1c_mu57_b3_3ncryp73d}
```

---

## Flag

```
SCC{f7p_7r4ff1c_mu57_b3_3ncryp73d}
```

Decoded from leet speak: **"ftp traffic must be encrypted"** — which is exactly the lesson this challenge demonstrates. All credentials and file contents were fully exposed in plaintext because FTP does not encrypt its communication. The secure alternative is **SFTP** or **FTPS**.

---

## Key Wireshark Filters Used

| Filter | Purpose |
|--------|---------|
| `ftp` | Show only FTP control messages (commands and responses) |
| `ftp-data` | Show only FTP data transfers |
| `ftp \|\| ftp-data` | Show all FTP-related traffic |
| `ip.addr == 192.168.116.135` | Filter by server IP |

---

## Lessons Learned

- FTP is an unencrypted protocol — credentials and file contents are fully visible in any packet capture
- Passive mode FTP (EPSV/PASV) opens separate data channels on dynamic ports — these are captured as `ftp-data` in Wireshark
- Wireshark's Export Objects feature can reconstruct transferred files directly from a pcap
- Hex encoding is not encryption — always check for encoded strings in captured files
