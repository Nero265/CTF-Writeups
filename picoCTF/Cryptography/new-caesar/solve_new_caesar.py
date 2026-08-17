import string

LOWERCASE_OFFSET = ord("a")
ALPHABET = string.ascii_lowercase[:16]

def unshift(c, k):
	t1 = ord(c) - LOWERCASE_OFFSET
	t2 = ord(k) - LOWERCASE_OFFSET
	return ALPHABET[(t1 - t2) % len(ALPHABET)]
	
def b16_decode(enc):
	plain = ""
	for i in range (0, len(enc),2):
		high = ALPHABET.index(enc[i])
		low = ALPHABET.index(enc[i+1])
		byte = (high << 4) + low
		plain += chr(byte)
	return plain
	
enc_flag = "fegdeogdgecoeocgcgchcfcffccfca"

for key in ALPHABET:
	b16 = "".join(unshift(c, key) for c in enc_flag)
	flag = b16_decode(b16)
	print(f"Key = {key} -> picoCTF{flag}")