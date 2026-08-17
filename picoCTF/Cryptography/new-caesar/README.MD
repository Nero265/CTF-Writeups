# New Caesar

**Category:** Cryptography
**Platform:** picoCTF
**Tools:** Python

## Recon

The challenge provides `new_caesar.py`, which generates the encrypted flag in two steps:

1. **Custom Base16 encoding** — instead of the standard hex alphabet (`0-9a-f`), it uses `string.ascii_lowercase[:16]`, i.e. letters `a-p`. Each byte of the plaintext is split into two nibbles (4 bits each), and each nibble is mapped to a corresponding letter from that alphabet.
2. **Caesar shift** — every character of the resulting "b16" string is further shifted by the value of a secret `key`, where `key` is a single letter from the same alphabet (`a-p`).

The shift function operates modulo 16 (the length of the alphabet), which is the key signal for the next step.

## Vulnerability

Since `key` must be exactly one letter from a 16-character alphabet (`a` to `p`), the entire keyspace has only **16 possible values**. This makes the shift trivially brute-forceable — no advanced cryptanalysis is needed. It's enough to try all 16 keys and identify which one produces readable (printable) output in the form `picoCTF{...}`.

## Exploit

```python
import string

LOWERCASE_OFFSET = ord("a")
ALPHABET = string.ascii_lowercase[:16]

def unshift(c, k):
    t1 = ord(c) - LOWERCASE_OFFSET
    t2 = ord(k) - LOWERCASE_OFFSET
    return ALPHABET[(t1 - t2) % len(ALPHABET)]

def b16_decode(enc):
    plain = ""
    for i in range(0, len(enc), 2):
        high = ALPHABET.index(enc[i])
        low = ALPHABET.index(enc[i + 1])
        byte = (high << 4) + low
        plain += chr(byte)
    return plain

enc_flag = "fegdeogdgecoeocgcgchcfcffccfca"

for key in ALPHABET:
    b16 = "".join(unshift(c, key) for c in enc_flag)
    flag = b16_decode(b16)
    print(f"Key = {key} -> picoCTF{{{flag}}}")
```

Running the script through all 16 possible keys, the only readable result is produced with `key = p`:

```
Key = p -> picoCTF{et_tu?_77866c61}
```

**Flag:** `picoCTF{et_tu?_77866c61}`

## Lessons Learned

- **Small keyspace beats cryptanalysis.** Whenever the number of possible keys is small enough (here, 16), there's no need to "break" the scheme mathematically — just try them all and filter by printable ASCII output.
- **Custom alphabets look intimidating but are just substitution.** Swapping the standard hex alphabet (`0-9a-f`) for `a-p` doesn't change the underlying math — it's still 4-bit nibble mapping, just with different labels.
- **Modulo alphabet length is a tell.** When a shift/unshift function uses `% len(ALPHABET)`, the alphabet's length directly defines the keyspace size — worth spotting immediately during code analysis.
- **"Printable check" as a brute-force filter.** Instead of manually checking all 16 outputs, it's convenient to automate a check for whether the result contains only printable characters and/or starts with `picoCTF{`.
