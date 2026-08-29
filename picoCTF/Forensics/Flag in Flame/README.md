# Flag in Flame
 
**Category:** Forensics
 
**Platform:** picoCTF (picoMini by CMU-Africa)
 
**Tools:** Base64, CyberChef
 
## Challenge Description
 
The SOC team discovered a suspiciously large log file after a recent breach. Instead of typical log entries, the file contained an enormous block of encoded text. The task was to inspect the file and uncover the concealed information hidden inside it.
 
**Provided file:** `logs.txt`
 
## Recon
 
The downloaded `logs.txt` file was not a standard log — it consisted almost entirely of a single, very large block of Base64-encoded text rather than readable log lines. This strongly suggested that a binary file had been encoded and disguised as a log file.
 
## Vulnerability
 
The "log" was simply a Base64-encoded binary file with a `.txt` extension. There was no actual log content — the entire file was an encoded payload, relying on the misleading filename and extension to avoid suspicion. Decoding the Base64 content directly reveals the original binary data.
 
## Exploit
 
1. Load `logs.txt` into CyberChef.
2. Apply the **From Base64** recipe to decode the content.
3. Set the output to render as a **PNG** file (the decoded bytes are valid PNG image data).
4. Save/export the decoded output as an image file.
5. Open the resulting PNG — the image visually displays a hex-encoded string.
6. Convert the visible hex text to ASCII (e.g. using CyberChef's **From Hex** recipe) to reveal the flag.
```
Recipe used:
From Base64
(output rendered/saved as PNG)
 
Resulting image contained a readable hex string, e.g.:
70 69 63 6f 43 54 46 7b ...
 
From Hex -> ASCII to obtain the final flag
```
 
**Flag:** `picoCTF{...}`
 
## Lessons Learned
 
- File extensions (`.txt`, `.log`) mean nothing on their own — always check the actual content/magic bytes of a suspicious file before trusting its label.
- A file consisting of one massive encoded block instead of normal structured content (like log lines) is a strong indicator of hidden/encoded data.
- Base64-decoded output should be checked against known file signatures (e.g. PNG headers) — CyberChef can render decoded bytes directly as an image, which speeds up detection.
- Steganography/hidden-data challenges often stack multiple encodings (Base64 → image → hex text) — peel back one layer at a time and re-inspect the output after each step.
