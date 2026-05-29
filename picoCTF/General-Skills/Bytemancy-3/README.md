# picoCTF - Bytemancy 3

## Description
Can you conjure the right bytes? The program's source code can be downloaded here and the compiled spellbook binary can be downloaded here. Connect to the program with netcat:  
`nc green-hill.picoctf.net port`

When connecting to the server, it provides the following instruction:
> I will name four procedures hidden inside spellbook. Each round, send me their *raw* 4-byte addresses in little-endian form. 3 correct answers unlock the flag.

---

## Methodology

This challenge requires us to interact with a remote server that asks for the raw 4-byte memory addresses of specific functions (procedures) hidden inside a compiled binary named `spellbook`. 

The difficulty comes from three things:
1. We need to find the correct memory addresses of the functions.
2. We must convert these addresses into **Little-Endian** format (bytes reversed).
3. The server selects 3 random functions in a random order each time we connect, and we must send non-printable raw bytes. This makes manual entry impossible, so we need to **automate** the process using a Python script.

### Step 1: Analyzing the Binary
First, we use the `nm` tool in Linux to list all symbols and extract the memory addresses of the magic procedures from the provided `spellbook` binary:

`nm spellbook`

From the output, we locate the four relevant procedures and their corresponding hexadecimal addresses:
* astral_spark -> 080491c1
* ember_sigil   -> 08049176
* glyph_conflux -> 0804919a
* binding_word -> 080491e3

### Step 2: Little-Endian Conversion
Since x86 architecture uses Little-Endian representation, the bytes must be sent in reverse order (from least significant to most significant byte). 

We map the addresses to their raw byte representation for our script:
* astral_spark (080491c1) -> \xc1\x91\x04\x08
* ember_sigil (08049176) -> \x76\x91\x04\x08
* glyph_conflux (0804919a) -> \x9a\x91\x04\x08
* binding_word (080491e3) -> \xe3\x91\x04\x08

---

## Solution Script

To handle the interactive nature of the server and dynamically respond with the correct bytes based on what the server asks, we use the companion Python socket script (`solve.py`) located in this directory.

The script listens to the server's output, parses the required function name, maps it to the respective Little-Endian byte sequence, and automatically submits the solution for all 3 rounds to retrieve the flag.

---

## Flag
Running the script successfully completes all three rounds and prints out the final flag.
