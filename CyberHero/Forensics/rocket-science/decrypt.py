"""
Rocket Science (CyberHero Playground, scc2024-quals) — full decryption chain.

Input : http_objects/send_result(22)  (extracted via tshark HTTP object export)
Output: recovered.pdf

Key material (recovered via memory forensics + decompilation of speedrocket.dll):
  - xorKey = "nEjge&5j2,zl"                     (hardcoded string literal in IL)
  - AES key = (register.Password + base64_decode(register_response.key))[:16]
            = ("cyberhero" + "7b3b8f6a")[:16]
            = "cyberhero7b3b8f6"
"""

import base64
import json
import re
from Crypto.Cipher import AES

XOR_KEY = "nEjge&5j2,zl"
AES_KEY = "cyberhero7b3b8f6"


def xor_decrypt(b64_input: str, key: str) -> bytes:
    raw = base64.b64decode(b64_input)
    kb = key.encode("utf-8")
    return bytes(b ^ kb[i % len(kb)] for i, b in enumerate(raw))


def main():
    with open("http_objects/send_result(22)") as f:
        data = json.load(f)

    # Layer 1: base64 -> XOR (repeating key) -> [IV(16)][AES-CBC ciphertext]
    combined = xor_decrypt(data["Result"], XOR_KEY)
    iv, ct = combined[:16], combined[16:]

    # Layer 2: AES-128-CBC, raw UTF-8 key (no KDF), PKCS7 padding
    cipher = AES.new(AES_KEY.encode("utf-8"), AES.MODE_CBC, iv)
    pt = cipher.decrypt(ct)
    pad_len = pt[-1]
    unpadded = pt[:-pad_len]

    # Layer 3: plaintext is itself base64 (starts with "%PDF" -> "JVBERi0x...")
    b64_clean = re.sub(r"[^A-Za-z0-9+/=]", "", unpadded.decode())
    pdf_bytes = base64.b64decode(b64_clean)

    with open("recovered.pdf", "wb") as f:
        f.write(pdf_bytes)

    print(f"Wrote recovered.pdf ({len(pdf_bytes)} bytes)")


if __name__ == "__main__":
    main()
