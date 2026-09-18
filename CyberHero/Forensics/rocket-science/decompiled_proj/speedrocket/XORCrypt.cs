using System;
using System.Text;

namespace speedrocket;

public class XORCrypt
{
	public static string XOREncrypt(byte[] inputBytes, string key)
	{
		byte[] bytes = Encoding.get_UTF8().GetBytes(key);
		byte[] array = new byte[inputBytes.Length];
		for (int i = 0; i < inputBytes.Length; i++)
		{
			array[i] = (byte)(inputBytes[i] ^ bytes[i % bytes.Length]);
		}
		return Convert.ToBase64String(array);
	}

	public static byte[] XORDecrypt(string input, string key)
	{
		byte[] array = Convert.FromBase64String(input);
		byte[] bytes = Encoding.get_UTF8().GetBytes(key);
		byte[] array2 = new byte[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = (byte)(array[i] ^ bytes[i % bytes.Length]);
		}
		return array2;
	}
}
