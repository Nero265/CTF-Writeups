# Rogue Tower

**Category:** Forensics  
**Difficulty:** Medium  
**Platform:** picoCTF 2026  
**Author:** Samuel Dinesh  

## Challenge Description

> A suspicious cell tower has been detected in the network. Analyze the captured network traffic to identify the rogue tower, find the compromised device, and recover the exfiltrated flag.

**Files provided:** `rogue_tower.pcap`

**Hints:**
1. The encryption key is derived from the victim device's IMSI.
2. The exfiltrated data is split across multiple HTTP POST requests.

## Tools Used

- `capinfos` / `tshark` (Wireshark CLI) — packet inspection
- `xxd`, `base64` — encoding/decoding
- `python3` — XOR decryption

## Recon

Started by identifying the capture file type and getting a high-level summary before opening anything in a GUI:

```bash
file rogue_tower.pcap
# rogue_tower.pcap: pcap capture file, microsecond ts (little-endian) - version 2.4 (Raw IPv4, capture length 65535)

capinfos rogue_tower.pcap
# Number of packets: 24, Capture duration: ~22.7s, Raw IPv4 encapsulation
```

A small 24-packet capture over Raw IPv4 — no radio-layer (GSMTAP) framing, so the "cell tower" concept is simulated entirely over IP/UDP/HTTP traffic.

Listed every packet to get the full picture:

```bash
tshark -r rogue_tower.pcap
```

This revealed three distinct traffic patterns:

1. **UDP broadcast beacons** on port 55000 (`x.x.x.x → 255.255.255.255:55000`) — simulated cell tower advertisements.
2. **Device registration flow**: each device does a DNS lookup for `device-<IMSI>.network.com`, followed by `GET /api/register` to a network server.
3. **One device** (`10.100.246.233`) additionally sends six `POST /upload` requests to a *different* server than everyone else.

## Identifying the Rogue Tower

Dumped the payload of the three UDP beacon packets to compare them:

```bash
tshark -r rogue_tower.pcap -Y "udp.port==55000" -x
```

| Packet | Source | Carrier | PLMN | Cell ID |
|---|---|---|---|---|
| #1 | 192.168.1.1 | Verizon | 310410 | 13323 |
| #2 | 192.168.1.1 | AT&T | 310410 | 13324 |
| #14 | **192.168.99.1** | **UNAUTHORIZED-TEST-NETWORK** | **00101** | **92130** |

The first two beacons come from `192.168.1.1` and advertise real, well-known US carriers under a legitimate PLMN (`310410`). The third beacon appears much later in the capture (t=17.1s vs t=0–1s), originates from a **different subnet** (`192.168.99.1`), and explicitly identifies itself as `UNAUTHORIZED-TEST-NETWORK` using PLMN `00101` — the reserved MCC/MNC test range commonly used by rogue base stations / IMSI catchers.

**Rogue tower:** `192.168.99.1` (PLMN 00101, CellID 92130)

## Finding the Compromised Device

Immediately after the rogue beacon (packet #14), device `10.100.246.233` performs its registration DNS lookup (`device-310410868411126.network.com`) and `GET /api/register` — but to `198.51.100.155`, while every other device registers to `198.51.100.227`. This is the only device that talks to the rogue server's IP.

That same device then sends six consecutive `POST /upload` requests to `198.51.100.155` — clear exfiltration behavior tied directly to the rogue tower's appearance.

**Compromised device:** `10.100.246.233`
**IMSI (from DNS query):** `310410868411126`

## Extracting the Exfiltrated Data

Pulled the raw payload from all six POST requests directly from the pcap (avoiding manual hex transcription errors):

```bash
tshark -r rogue_tower.pcap -Y "http.request.method==POST" -T fields -e http.file_data
```

This returned six hex-encoded chunks. Concatenated and decoded them:

```bash
tshark -r rogue_tower.pcap -Y "http.request.method==POST" -T fields -e http.file_data \
  | tr -d '\n' | xxd -r -p | base64 -d
```

The result was **not** plain text — a garbled 34-byte string. Since each `POST` body is ASCII text, the six chunks reassemble into a base64 string, but the base64-decoded bytes are still encrypted: the flag was XOR-encrypted *before* being base64-encoded for transport.

## Decrypting the Flag

Per the first hint, the XOR key is derived from the victim device's IMSI (`310410868411126`). Several key-derivation strategies were tested (full IMSI, reversed IMSI, MD5/SHA1/SHA256 hashes, first/last N digits). The last **8 digits** of the IMSI produced a fully printable result:

```python
import base64

b64 = "RlFXXnJldE1ECFNEAm5RBVpUa0UBRgFEaV4EBwlQUAUCRQ=="
data = base64.b64decode(b64)

imsi = "310410868411126"
key = imsi[-8:].encode()   # "68411126"

flag = bytes([data[i] ^ key[i % len(key)] for i in range(len(data))])
print(flag.decode())
```

**Output:**

```
picoCTF{r0gu3_c3ll_t0w3r_f068ab34}
```

## Flag

```
picoCTF{r0gu3_c3ll_t0w3r_f068ab34}
```

## Lessons Learned

- **A "cell tower" doesn't require radio hardware to analyze in forensics challenges.** Here, GSM-style concepts (PLMN, Cell ID, carrier beacons) were simulated entirely over UDP/HTTP, so standard `tshark`/Wireshark IP analysis was sufficient.
- **Compare beacons/advertisements side by side.** The rogue tower stood out through multiple independent signals at once: a different subnet, a later timestamp, a nonsensical carrier name, and a reserved test PLMN (`00101`) — enumerating all beacon payloads rather than assuming made this obvious.
- **Follow the odd one out.** Among near-identical device registration flows, the single device that talked to a *different* server IP was the compromised one — anomaly detection by diffing repetitive traffic patterns is a reliable forensics technique.
- **Extract payloads programmatically, not by hand.** Manually transcribing hex/ASCII from a terminal dump is error-prone (confirmed firsthand — a manually retyped base64 string failed to decode correctly). Using `tshark -T fields -e http.file_data` piped through `xxd -r -p` reproduces the exact bytes with no transcription risk.
- **XOR key derivation may need brute-forcing across reasonable candidates.** When a hint says a key is "derived from" a value, don't assume the value itself is the key — test substrings, hashes, and transformations systematically (a small Python script checking for printable ASCII output made this fast).
- **IMSI structure is informative**: the first 3 digits (MCC) and next 2–3 (MNC) tie directly back to the PLMN seen in tower beacons, which helped confirm the device-to-network relationship, and its trailing digits ended up being the actual encryption key material.
