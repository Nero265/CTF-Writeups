"""
Decodes the hex-encoded HTTP response bodies pulled from tshark's
http.file_data field, and the base64-encoded PART 2 flag fragment
found embedded in the / page's input placeholder.

Usage:
    Extract the hex string from a given http.file_data column and
    pass it into decode_http_body(). Response bodies for "/" and
    "/home" both contained an HTML comment with a piece of the flag.
"""

import base64


def decode_http_body(hex_str: str) -> str:
    """Decode a tshark http.file_data hex string back to text (e.g. HTML)."""
    return bytes.fromhex(hex_str).decode()


def decode_base64_fragment(b64_str: str) -> str:
    """Decode a base64 string, used for the PART 2 flag fragment
    found in the input placeholder on the '/' page."""
    return base64.b64decode(b64_str).decode()


if __name__ == "__main__":
    # PART 2 was embedded as a base64 string in an <input placeholder="...">
    # on the / page:
    part2_b64 = "UEFSVCAyIDogYU5jMTNuN19hTDEzbjVf"
    print(decode_base64_fragment(part2_b64))
    # -> PART 2 : aNc13n7_aL13n5_

    # PART 1 and PART 3 were plain HTML comments inside the hex-encoded
    # response bodies for "/" and "/home" respectively — decode those
    # hex strings with decode_http_body() the same way, then grep for
    # "PART" in the resulting HTML.
