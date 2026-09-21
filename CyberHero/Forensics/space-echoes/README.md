# Space Echoes

**Category:** Forensics  
**Platform:** SCC 2024 Quals (Serbian Cybersecurity Challenge)  
**Tools:** `capinfos`, `tshark`, Python (hex / base64 decoding)  

## Challenge Description

> Amidst the silent void of the cosmos, a faint signal emerges, carrying secrets from an ancient civilization lost to time. You've stumbled upon coordinates encrypted with knowledge that predates the stars, a message beckoning from the edges of the unknown. Deciphering its contents could unravel the mysteries of the universe, or it could lead you down a path from which there is no return. The choice is yours: will you heed the call of the cosmos and unlock the secrets that lie beyond?

**Provided file:** `capture.pcapng`

## Recon

Started with a quick overview of the capture before diving into any filtering:

```
capinfos capture.pcapng
```

Key findings:
- 12,982 packets, 14 MB, ~36 seconds of traffic
- Average packet rate of 357 packets/sec — a short but dense capture, worth investigating further

Next, checked the protocol breakdown to decide where to focus:

```
tshark -r capture.pcapng -q -z io,phs
```
- `-r capture.pcapng` — read from the file instead of live-capturing
- `-q` — quiet mode, suppresses per-packet output so only the summary is shown
- `-z io,phs` — requests the "protocol hierarchy statistics" report

Result: mostly QUIC/TLS (encrypted, ordinary browsing traffic) and a notable spike of 274 DNS packets. Also present: 34 HTTP frames — small in number, but plaintext and worth a direct look.

## Investigation

Checked the DNS queries first, suspecting tunneling or exfiltration given the volume:

```
tshark -r capture.pcapng -Y "dns.flags.response == 0" -T fields -e dns.qry.name
```
- `-Y "dns.flags.response == 0"` — display filter, keeping only DNS *queries* (not responses)
- `-T fields -e dns.qry.name` — output just the queried domain name per line

The DNS traffic turned out to be ordinary background noise from everyday browsing (Reddit, YouTube, Google, Mozilla telemetry, OCSP checks, etc.) — a red herring, not the delivery mechanism for the flag.

Turned to the HTTP traffic instead:

```
tshark -r capture.pcapng -Y "http" -T fields -e http.request.full_uri -e http.file_data
```
- `-Y "http"` — filters to HTTP packets only
- `-e http.request.full_uri` — the full requested URL
- `-e http.file_data` — the raw response/request body, printed as a hex string

This revealed a small internal web app at `http://192.168.31.129/`, titled **"Alien Secret Login"**, with pages at `/`, `/home`, and static assets (`styles.css`, `home_styles.css`, `alien_background.jpg` — which 404'd). A POST to `/` also leaked submitted login credentials in cleartext form data:

```
username=admin&password=*Cosmic&Signal#42
```

These credentials turned out to be a narrative red herring — the flag parts were not gated behind authentication; they were already present in the plaintext page source captured in the HTTP responses.

## Extraction

`http.file_data` output is hex-encoded, one long string per response. Extracted the hex blob for the `/` and `/home` responses and decoded them back to HTML:

```python
decoded = bytes.fromhex(hex_blob).decode()
print(decoded)
```

Decoding the `/` response revealed the page's login form and, embedded in the HTML source:

- An HTML comment: `<!-- PART 1 : SCC{wH15P3r5_0f_ -->`
- An `<input>` placeholder containing a base64 string: `UEFSVCAyIDogYU5jMTNuN19hTDEzbjVf`

Decoding the base64 string:
```
python3 -c "import base64; print(base64.b64decode('UEFSVCAyIDogYU5jMTNuN19hTDEzbjVf').decode())"
```
→ `PART 2 : aNc13n7_aL13n5_`

Decoding the `/home` response revealed a third HTML comment:
```
<!-- PART 3 : 3Ch0_7hR0ugh_71m3} -->
```

Reassembling all three parts in order:

```
PART 1: SCC{wH15P3r5_0f_
PART 2: aNc13n7_aL13n5_
PART 3: 3Ch0_7hR0ugh_71m3}
```

### Flag

```
SCC{wH15P3r5_0f_aNc13n7_aL13n5_3Ch0_7hR0ugh_71m3}
```

## Lessons Learned

- `http.file_data` in `tshark` output is hex-encoded and needs a separate decode pass (`bytes.fromhex(...).decode()`) — it won't be human-readable directly in the terminal.
- Not every suspicious-looking element is the intended path: the high DNS packet count and the leaked login credentials both looked promising but were red herrings. The actual flag was split across plaintext HTML comments and a base64-encoded input placeholder, requiring no authentication at all.
- When a capture contains a visible login form, check the raw HTTP response body for hidden content (comments, placeholders, hidden fields) before assuming the flag is gated behind the login itself.
- Flag fragments split across multiple pages/encodings (plain text, base64) is a common forensics pattern — worth scanning *all* HTTP response bodies rather than stopping at the first hit.
