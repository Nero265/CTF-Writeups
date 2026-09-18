# Rocket Science

**Category:** Forensics  
**Platform:** CyberHero Playground (scc2024-quals)  
**Tools:** Wireshark/tshark, Volatility 3, ILSpy (ilspycmd), Python (pycryptodome), strings  

## Challenge Description

> CyberHero has just optimized their network with a cutting-edge internet acceleration program. However, amidst the boost in cyber speed, a critical file has mysteriously disappeared. Can you delve into the digital depths and retrieve the lost asset before it's too late?

Provided files: `dump.pcapng` (network capture), `memdump.mem` (5GB Windows memory dump), and `link.txt` (a Google Drive link to the original `rocketScience.zip` containing both).

## Recon

Started with a protocol hierarchy overview of the capture:

```bash
tshark -r dump.pcapng -q -z io,phs
```

Most traffic was on a non-standard port, which tshark's heuristics misidentified as the Monotone VCS `netsync` protocol — a red herring caused by port-based dissector guessing. A TCP conversation summary confirmed the real point of interest:

```bash
tshark -r dump.pcapng -q -z conv,tcp
```

This revealed dozens of short-lived TCP connections from `192.168.5.128` to `192.168.0.12:5253`, roughly one per second (heartbeat/polling pattern), plus one standout stream carrying ~40 kB — far larger than the rest.

## Investigation

### Recovering the HTTP layer

Since port 5253 isn't standard HTTP, Wireshark never dissected it as such. Forcing the decode and exporting HTTP objects revealed the true picture:

```bash
tshark -r dump.pcapng -d tcp.port==5253,http --export-objects http,http_objects
```

This produced dozens of `get_command`, `register`, and `send_result` HTTP objects — the traffic pattern of a custom C2-style "agent" checking in periodically. The largest file, **`send_result(22)`** (39,853 bytes), matched the `Content-Length` header seen in the raw stream and was clearly the payload of interest:

```json
{"Guid":"475cdca4-8d21-472a-8eca-0ee95ebbf30d","Result":"<base64 blob>"}
```

### First (wrong) assumptions

Base64-decoding `Result` gave 27,632 bytes of high-entropy data with no recognizable magic bytes. Since the length is an exact multiple of 16 (AES block size), the initial hypothesis was "AES-encrypted, key derived from the `register` password." Attempts with MD5/SHA256-derived keys (from the `Password: "cyberhero"` seen in the `register` request) all failed, as did raw DEFLATE decompression — the ciphertext turned out to have an extra encoding layer that wasn't accounted for yet.

### Pivoting to memory forensics

To find the real encryption logic, the `speedrocket.exe` process (named in `memdump.mem` — the "internet acceleration program" from the challenge description) needed to be examined directly.

```bash
vol -f memdump.mem windows.info
vol -f memdump.mem windows.pslist | grep -i speedrocket
# → PID 6680
```

Listing loaded modules showed `speedrocket.exe` (a thin .NET apphost stub) alongside the actual application logic in `speedrocket.dll`:

```bash
vol -f memdump.mem windows.dlllist --pid 6680
```

Both were dumped directly from memory:

```bash
vol -f memdump.mem -o dumped_proc windows.dlllist --pid 6680 --dump
```

`speedrocket.dll` (32 KB) was small enough to decompile in full.

### Decompiling the agent

```bash
dotnet tool install -g ilspycmd --version 7.2.1.6856   # net6.0-compatible version
ilspycmd speedrocket.dll -p -o decompiled_proj/         # project mode, full source tree
ilspycmd -il speedrocket.dll > program_il.txt            # raw IL, for the parts ILSpy's
                                                           # C# reconstruction dropped
```

The decompiled source revealed the full crypto stack:

```csharp
public class SpeedCrypt
{
    public static string Decrypt(string b64encrpyted, string key, string xorKey)
    {
        return AESCrypt.Decrypt(XORCrypt.XORDecrypt(b64encrpyted, xorKey), key);
    }
}

public class XORCrypt
{
    public static byte[] XORDecrypt(string input, string key)
    {
        byte[] array = Convert.FromBase64String(input);
        byte[] bytes = Encoding.UTF8.GetBytes(key);
        byte[] array2 = new byte[array.Length];
        for (int i = 0; i < array.Length; i++)
            array2[i] = (byte)(array[i] ^ bytes[i % bytes.Length]);
        return array2;
    }
}

public class AESCrypt
{
    public static string Decrypt(byte[] combined, string key)
    {
        byte[] cipherText = ExtractCipherText(combined); // combined[16..]
        byte[] iv = ExtractIv(combined);                 // combined[0..16]
        return DecryptStringFromBytes_Aes(cipherText, key, iv);
        // AES-CBC, Key = UTF8 bytes of `key` (used directly, no hashing/KDF)
    }
}
```

So the real transform is: **base64 → XOR (repeating key) → [first 16 bytes = IV][rest = AES-CBC ciphertext] → AES-128-CBC decrypt (raw UTF-8 key, no KDF)**.

### Recovering the key material

The `Program` class's `MoveNext()` state machine (visible only in the raw IL — ILSpy's high-level reconstruction failed to render it) showed exactly how the AES key is built during registration:

```
key_raw = userName + Encoding.UTF8.GetString(Convert.FromBase64String(registerResponse.key))
if key_raw.Length > 16: key_raw = key_raw.Substring(0, 16)
key = key_raw.PadRight(16, 'x')
```

From the captured traffic:
- `register` request: `{"Guid": "...", "Password": "cyberhero"}` → this `Password` value is reused as `userName`
- `register` response: `{"key":"N2IzYjhmNmE="}` → base64-decodes to the ASCII string `"7b3b8f6a"`

```
key_raw = "cyberhero" + "7b3b8f6a" = "cyberhero7b3b8f6a"  (17 chars)
key     = key_raw[:16]                = "cyberhero7b3b8f6"  (16 chars, AES-128 key)
```

The XOR key was found as a plain string literal (`ldstr`) in the IL, right next to the `executeCommand` call:

```
xorKey = "nEjge&5j2,zl"
```

## Exploit / Extraction

With both keys known, the full decryption chain:

```python
import base64, json
from Crypto.Cipher import AES

with open("http_objects/send_result(22)") as f:
    data = json.load(f)

def xor_decrypt(b64_input, key):
    raw = base64.b64decode(b64_input)
    kb = key.encode("utf-8")
    return bytes(b ^ kb[i % len(kb)] for i, b in enumerate(raw))

xor_key = "nEjge&5j2,zl"
aes_key = "cyberhero7b3b8f6"

combined = xor_decrypt(data["Result"], xor_key)   # IV(16) + AES-CBC ciphertext
iv, ct = combined[:16], combined[16:]

cipher = AES.new(aes_key.encode("utf-8"), AES.MODE_CBC, iv)
pt = cipher.decrypt(ct)

pad_len = pt[-1]
unpadded = pt[:-pad_len]           # PKCS7 unpad
```

The decrypted plaintext started with `JVBERi0xLjQK...` — base64 for `%PDF-1.4`. One more decode layer recovered the original file:

```python
import re
b64_clean = re.sub(r'[^A-Za-z0-9+/=]', '', unpadded.decode())
pdf_bytes = base64.b64decode(b64_clean)

with open("recovered.pdf", "wb") as f:
    f.write(pdf_bytes)
```

`recovered.pdf` — the file that "mysteriously disappeared" — opened to reveal a base64-encoded flag:

```
U0NDe2MyXzV1cjNfNXAzM2RzX3VwX3cxbmQwdzV9
```

```bash
echo U0NDe2MyXzV1cjNfNXAzM2RzX3VwX3cxbmQwdzV9 | base64 -d
```

**Flag:** `SCC{c2_5ur3_5p33ds_up_w1nd0w5}`

## Lessons Learned

- **Protocol dissector heuristics can mislead**: tshark's port-based guessing (`netsync`) had nothing to do with the actual protocol (custom HTTP on a non-standard port). Always verify with `-d <port>,<protocol>` decode-as rather than trusting automatic detection on unusual ports.
- **Small process memory dumps beat full memory images**: rather than grepping a 5GB dump, isolating and dumping just the target process's modules via Volatility (`windows.dlllist --pid <pid> --dump`) made static analysis (strings, decompilation) tractable.
- **Custom crypto is a soft target**: this agent layered XOR + AES-CBC, but the XOR key was a hardcoded string literal, the AES key derivation was pure string concatenation/truncation (no real KDF like PBKDF2), and the IV was naively prepended to the ciphertext. Each of these is individually a well-known anti-pattern — combined, they made full key recovery from a decompiled 32 KB DLL straightforward.
- **PasswordDeriveBytes red herrings**: `strings` turned up `PasswordDeriveBytes`/`Rfc2898DeriveBytes` references early on, but these came from unrelated Windows Defender signature strings embedded elsewhere in memory, not the target application. Cross-referencing hits against the actual process module (not the whole memory image) avoided chasing a dead end.
- **`.NET` async state machines can defeat high-level decompilers**: ILSpy's default C# reconstruction silently dropped the `Program.MoveNext()` body (likely due to IL layout quirks from being extracted mid-process rather than read from disk). Falling back to raw IL disassembly (`ilspycmd -il`) was necessary to recover the exact key-derivation logic.
