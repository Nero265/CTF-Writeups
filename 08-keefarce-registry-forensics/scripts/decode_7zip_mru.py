#!/usr/bin/env python3
"""
decode_7zip_mru.py

Reads 7-Zip's MRU registry keys (ArcHistory, FolderHistory, CopyHistory,
PanelPath0-9, Extraction\\PathHistory) directly from a NTUSER.DAT hive
using regipy's RegistryHive object, bypassing regipy-dump's JSON
serialization, which silently truncates REG_BINARY values over 128 bytes.

Each MRU value is a REG_BINARY blob containing one or more UTF-16LE,
null-terminated strings concatenated together (double-null terminated
overall) - this script splits them out into a clean list of paths.

Usage:
    python3 decode_7zip_mru.py /path/to/NTUSER.DAT
"""

import sys

from regipy.registry import RegistryHive

TARGET_KEYS = [
    r"\Software\7-Zip\FM",
    r"\Software\7-Zip\Extraction",
]


def split_utf16_mru(raw: bytes) -> list[str]:
    """Split a REG_BINARY MRU blob into individual UTF-16LE strings."""
    if not raw:
        return []
    decoded = raw.decode("utf-16-le", errors="ignore")
    # Individual entries are null-terminated; blob is double-null terminated.
    parts = [p for p in decoded.split("\x00") if p]
    return parts


def dump_key(hive: RegistryHive, path: str) -> None:
    try:
        key = hive.get_key(path)
    except Exception as exc:
        print(f"[!] Could not open {path}: {exc}")
        return

    print(f"\n=== {path} ===")
    print(f"LastWrite: {key.timestamp}")

    if not key.values:
        print("  (no values)")
        return

    for v in key.values:
        if isinstance(v.value, (bytes, bytearray)):
            entries = split_utf16_mru(bytes(v.value))
            if entries:
                print(f"  [{v.name}] ({len(v.value)} bytes raw)")
                for e in entries:
                    print(f"      -> {e}")
            else:
                print(f"  [{v.name}] (empty / non-string binary, {len(v.value)} bytes)")
        else:
            print(f"  [{v.name}] = {v.value}")


def main() -> None:
    if len(sys.argv) != 2:
        print(f"Usage: {sys.argv[0]} <path-to-NTUSER.DAT>")
        sys.exit(1)

    hive_path = sys.argv[1]
    hive = RegistryHive(hive_path)

    for key_path in TARGET_KEYS:
        dump_key(hive, key_path)


if __name__ == "__main__":
    main()
