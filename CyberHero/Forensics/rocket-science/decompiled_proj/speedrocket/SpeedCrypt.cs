namespace speedrocket;

public class SpeedCrypt
{
	public static string Encrypt(string plainText, string key, string xorKey)
	{
		return XORCrypt.XOREncrypt(AESCrypt.Encrypt(plainText, key), xorKey);
	}

	public static string Decrypt(string b64encrpyted, string key, string xorKey)
	{
		return AESCrypt.Decrypt(XORCrypt.XORDecrypt(b64encrpyted, xorKey), key);
	}
}
