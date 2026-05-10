# CTF: WeaponizedKey — PowerShell Obfuscation Analysis

**Category:** Reverse Engineering / Malware Analysis  
**Platform:** CyberHero Playground  
**Tools:** Manual analysis, CyberChef

---

## Challenge Description

A suspicious PowerShell script (`SecretWeapon.ps1`) is provided.
The script is heavily obfuscated — all variable names are replaced
with random strings of 100+ characters. The goal is to deobfuscate
the script, reconstruct a secret API key, and retrieve the flag
from a remote endpoint.

**Provided files:** `SecretWeapon.ps1`

---

## Obfuscation Analysis

Opening `SecretWeapon.ps1` reveals the obfuscation pattern immediately:

```powershell
$dthBwETvJ1Ti4vmrceDaIJ78ZFqti26UFU...incredibly_long = "X1Vu"
$i6y8lV32Pc99tgLvlCuclafrQIKHYUkda...incredibly_long = "NWgz"
$ouyH5KaEzveqq4rDgefH91xzqY4rsnlYh...incredibly_long = "bDM0"
```

All variable names are randomized 100+ character strings, but their
**values** are short Base64 fragments. This is a classic PowerShell
obfuscation technique — the logic is intact, only the names are hidden.

### Step 1 — Extract all Base64 fragments

Map each long variable name to its value by scrolling right in the editor:

| Readable name | Value  |
|---------------|--------|
| $Symb0l       | X1Vu   |
| $M1sc         | NWgz   |
| $D1g1t        | bDM0   |
| $C0d3         | TjFu   |
| $R4nd0m       | Awd    |
| $Sp3c14l      | zNy    |
| $End          | RH0=   |
| $L3tt3r       | akE1   |
| $Num3r1c      | De1    |
| $T3xt         | TExf   |
| $Alph4        | U0N    |
| $Extr4        | c2gz   |

### Step 2 — Trace the joining logic

The obfuscated script contains lines like:

```powershell
$longVar1 = $longVar2 + $longVar3 + $longVar4 -join ''
```

Replacing long names with readable ones reveals 4 groups (Parts):

```powershell
$Part1 = $D1g1t  + $Extr4 + $End    -join ''  → "bDM0c2gzRH0="
$Part2 = $C0d3   + $L3tt3r + $Symb0l -join ''  → "TjFuakE1X1Vu"
$Part3 = $Alph4  + $Num3r1c + $R4nd0m -join '' → "U0NDe1Awd"
$Part4 = $Sp3c14l + $M1sc + $T3xt    -join ''  → "zNyNWgzTExf"
```

### Step 3 — Reconstruct the final key

The final assembly line uses comma-separated parts (not `+`),
which defines the exact order:

```powershell
$K3yV4lu3 = ($Part3, $Part4, $Part2, $Part1) -join ''
```

Concatenated result:

```
U0NDe1AwdzNyNWgzTExfTjFuakE1X1VubDM0c2gzRH0=
```

### Step 4 — Decode Base64

The `==` padding confirms Base64 encoding. Decode using CyberChef
(From Base64) or PowerShell:

```powershell
[System.Text.Encoding]::UTF8.GetString(
    [System.Convert]::FromBase64String(
        "U0NDe1AwdzNyNWgzTExfTjFuakE1X1VubDM0c2gzRH0="
    )
)
```

This produces the API key sent to the endpoint:

```
https://cyberhero.rs/secret_weapon?key=
```

The server response contained the flag (server no longer active).

---

## Deobfuscated Script

See `SecretWeaponRaw.ps1` for the fully readable version with
all variable names replaced by descriptive identifiers.

---

## Key Analysis Techniques

| Tehnika | Svrha |
|---------|-------|
| Scroll right in editor | Reveal hidden variable values |
| Search for `-join ''` | Find places where strings are concatenateda |
| Search for `FromBase64String` | Identify encoding routines |
| Search for `DownloadString` | Detect network calls / C2 URLs |
| CyberChef → From Base64 | Decode concatenated string |
---

## Lessons Learned

- PowerShell obfuscation rarely changes program logic —
  only names and formatting are altered
- Long random variable names are a red flag in malware analysis;
  always map them to their values first
- `-join ''` and `,` operators in PowerShell control string
  concatenation order — critical for reassembly
- `FromBase64String` is a common indicator of hidden payloads
  in obfuscated scripts
- Real-world malware uses identical techniques to hide C2 URLs,
  download payloads, and exfiltrate data via DNS or HTTP
