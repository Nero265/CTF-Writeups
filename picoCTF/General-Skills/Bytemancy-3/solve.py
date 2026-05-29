import socket
import time

adrese = {
    b'astral_spark': b'\xc1\x91\x04\x08',
    b'ember_sigil': b'\x76\x91\x04\x08',
    b'glyph_conflux': b'\x9a\x91\x04\x08',
    b'binding_word': b'\xe3\x91\x04\x08'
}

s = socket.socket()
s.connect(('green-hill.picoctf.net', 53702))

for runda in range(3):
    data = b''
    while b'==>' not in data:
        data += s.recv(1)
    print(data.decode('utf-8', errors='ignore'), end='')
    
    izabrana = None
    for ime in adrese:
        if ime in data:
            izabrana = ime
            break
            
    if izabrana:
        s.sendall(adrese[izabrana])
        print(f'[Skripta šalje bajtove za: {izabrana.decode()}]')

# Pauza od 1 sekunde da server stigne da ispiše flag
time.sleep(1)

# Čitamo sve preostale podatke iz bafera
ostatak = s.recv(4096)
print(ostatak.decode('utf-8', errors='ignore'))