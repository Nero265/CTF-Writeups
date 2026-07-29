# The Compressed Truth — Windows Registry Forensics    

**Category:** Forensics    
**Difficulty:** Easy    
**Artifacts provided:** Partial KAPE collection (`NTUSER.DAT` hives for 3 users, `SYSTEM32\config\DEFAULT` hive)    

---

## 📁 Folder Contents

- `README.md` — this writeup
- `scripts/decode_7zip_mru.py` — standalone script to decode 7-Zip MRU registry keys directly from a `NTUSER.DAT` hive, bypassing regipy-dump's 128-byte truncation
- `timeline.csv` — structured, machine-readable incident timeline reconstructed from the registry artifacts

---

## 📜 Scenario

CROWQUILL authenticated as `vmarr` using stolen credentials and accessed his workstation without triggering any alarms — no forced entry, no broken locks, just a stolen identity presented at the right threshold. Inside, they located the Shard Reference custody chain and went further, deploying a credential-extraction tool to dump the contents of a password manager directly from memory. The stolen data was staged, compressed into an archive, and exfiltrated. Files were deleted, tools removed, connection closed.

The machine has been imaged and handed over for investigation. No files remain — but the registry remembers.

---

## 🧰 Tools Used

- `regipy` (Python registry parser) — bulk hive dumping and targeted key extraction
- `RegRipper3.0` (`rip.pl`) — plugin-based parsing (UserAssist, RecentDocs, TypedPaths, RunMRU)
- Python (manual UTF-16LE / PIDL binary decoding)

---

## 🔍 Methodology

### 1. Identifying the target user

The KAPE collection contained three `NTUSER.DAT` hives: `cyberjunkie`, `Default`, and `vmarr`. Given the scenario, `vmarr` is the compromised identity CROWQUILL authenticated as.

```bash
find C/Users -iname "NTUSER.DAT"
```

### 2. Recovering 7-Zip activity (`Software\7-Zip\FM`)

`regipy-dump` truncates `REG_BINARY` values at 128 bytes when serializing to JSON — several MRU keys in this challenge (`ArcHistory`, `FolderHistory`, `CopyHistory`) exceed that limit and get silently cut off. Reading the hive directly through `regipy.registry.RegistryHive` bypasses the truncation and reveals the full MRU chain:

```python
from regipy.registry import RegistryHive

reg = RegistryHive('vmar_NTUSER.DAT')
key = reg.get_key('\\Software\\7-Zip\\FM')
for v in key.values:
    if isinstance(v.value, bytes):
        print(v.name, v.value.decode('utf-16-le', errors='ignore'))
```

This surfaced the full drill-down path the operative navigated, folder by folder, ending inside a nested archive:

```
C:\Users\vmarr\Documents\Registry\
C:\Users\vmarr\Documents\Registry\shard_storage\
C:\Users\vmarr\Documents\Registry\shard_storage\ShardKeepass_FirstMark\
C:\Users\vmarr\Documents\Registry\shard_ref\
C:\Users\vmarr\Documents\Registry\internal_reports\
C:\Users\vmarr\Documents\Registry\custody_chains\
C:\Users\vmarr\Documents\Registry\oath_records_cinderbound_vol2.zip\
C:\Users\vmarr\Documents\Registry\oath_records_cinderbound_vol2.zip\oath_records_cinderbound_vol2\
C:\Users\vmarr\Documents\Registry\oath_records_cinderbound_vol2.zip\oath_records_cinderbound_vol2\saltoaths_secretive\
```

> The reusable version of this decoding logic is in [`scripts/decode_7zip_mru.py`](./scripts/decode_7zip_mru.py) — run it against any `NTUSER.DAT` to get the full, untruncated 7-Zip MRU history.

### 3. Confirming the credential-theft tool (`Software\7-Zip\Extraction`)

The `PathHistory` value under the `Extraction` key recorded the last folder browsed during an archive extraction:

```
C:\Users\vmarr\AppData\Local\Temp\writ\KeeFarce\
```

**KeeFarce** is a real-world tool that injects a DLL into a running `KeePass.exe` process to dump all decrypted credential entries straight from memory — no lock-picking, no brute force, just lifting the key from a hand already holding it.

### 4. Building the timeline with UserAssist

```bash
perl rip.pl -r vmar_NTUSER.DAT -p userassist
```

| Time (UTC) | Event |
|---|---|
| 12:16:55 | `KeePass-2.61.1-Setup.exe` executed |
| 12:32:19 | `PowerShell_ISE.exe` executed — KeeFarce staging |
| 13:13:16 | `7zFM.exe` (7-Zip File Manager) executed |
| 13:15:15 | Extraction path recorded — `Temp\writ\KeeFarce\` |
| 13:18:23 | `shard_ref_011_ember_court_piece.txt`, `marr_working_notes_lowtide.txt` accessed |
| 13:20:32 | Path typed in Explorer — `C:\Users\Public\Music\saltwork` |
| 13:24:55 | `shardchain.tar` archive finalized |
| 13:42:15 | `gkape.exe` executed — post-incident acquisition |

### 5. RecentDocs — confirming what was touched

```bash
perl rip.pl -r vmar_NTUSER.DAT -p recentdocs
```

Confirmed access to `shard_ref_011_ember_court_piece.txt`, the `shard_references` folder, and the finalized archive `shardchain.tar` in the `.tar` extension sub-key.

---

## 🚩 Flags

| # | Question | Answer |
|---|---|---|
| 1 | Name of the credential-extraction tool | `KeeFarce` |
| 2 | Time the tool first touched the system | `2026-06-18 13:15:15` |
| 3 | Deepest folder enumerated inside the archive | `saltoaths_secretive` |
| 4 | Staging location for stolen records | `C:\Users\vmarr\Desktop\working\` |
| 5 | Name of the archive prepared for exfiltration | `C:\Users\Public\Pictures\shardchain.tar` |
| 6 | Location of the master credential vault | `C:\Users\vmarr\Documents\Registry\shard_storage\ShardKeepass_FirstMark\` |
| 7 | Folder where 7-Zip operations concluded | `C:\Users\Public\Music\saltwork` |

---

## 🧠 Key Takeaway

`regipy-dump`'s JSON output silently truncates `REG_BINARY` values over 128 bytes. Any MRU-style key (`ArcHistory`, `FolderHistory`, `CopyHistory`, `PathHistory`) that looks "too short" should be re-read directly via `RegistryHive.get_key().values` in Python before concluding a trail is a dead end — three of the seven flags in this challenge were hidden in the truncated tail of a single key.
