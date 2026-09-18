using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace speedrocket;

public class AESCrypt
{
	public static byte[] Encrypt(string plainText, string key)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		byte[] bytes = Encoding.get_UTF8().GetBytes(key);
		Aes val = Aes.Create();
		try
		{
			((SymmetricAlgorithm)val).set_Key(bytes);
			((SymmetricAlgorithm)val).set_Mode((CipherMode)1);
			((SymmetricAlgorithm)val).GenerateIV();
			byte[] iV = ((SymmetricAlgorithm)val).get_IV();
			MemoryStream val2 = new MemoryStream();
			try
			{
				CryptoStream val3 = new CryptoStream((Stream)(object)val2, ((SymmetricAlgorithm)val).CreateEncryptor(), (CryptoStreamMode)1);
				try
				{
					byte[] bytes2 = Encoding.get_UTF8().GetBytes(plainText);
					int num = ((SymmetricAlgorithm)val).get_BlockSize() / 8;
					int num2 = num - bytes2.Length % num;
					byte b = (byte)num2;
					byte[] array = new byte[bytes2.Length + num2];
					global::System.Array.Copy((global::System.Array)bytes2, 0, (global::System.Array)array, 0, bytes2.Length);
					for (int i = bytes2.Length; i < array.Length; i++)
					{
						array[i] = b;
					}
					((Stream)val3).Write(array, 0, array.Length);
				}
				finally
				{
					((global::System.IDisposable)val3)?.Dispose();
				}
				return CombineArrays(iV, val2.ToArray());
			}
			finally
			{
				((global::System.IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((global::System.IDisposable)val)?.Dispose();
		}
	}

	public static string Decrypt(byte[] combined, string key)
	{
		byte[] cipherText = ExtractCipherText(combined);
		byte[] iv = ExtractIv(combined);
		return DecryptStringFromBytes_Aes(cipherText, key, iv);
	}

	public static string DecryptStringFromBytes_Aes(byte[] cipherText, string key, byte[] iv)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		string text = null;
		Aes val = Aes.Create();
		try
		{
			((SymmetricAlgorithm)val).set_Key(Encoding.get_UTF8().GetBytes(key));
			((SymmetricAlgorithm)val).set_IV(iv);
			((SymmetricAlgorithm)val).set_Mode((CipherMode)1);
			_ = ((SymmetricAlgorithm)val).get_BlockSize() / 8;
			ICryptoTransform val2 = ((SymmetricAlgorithm)val).CreateDecryptor(((SymmetricAlgorithm)val).get_Key(), ((SymmetricAlgorithm)val).get_IV());
			MemoryStream val3 = new MemoryStream(cipherText);
			try
			{
				CryptoStream val4 = new CryptoStream((Stream)(object)val3, val2, (CryptoStreamMode)0);
				try
				{
					StreamReader val5 = new StreamReader((Stream)(object)val4);
					try
					{
						return ((TextReader)val5).ReadToEnd();
					}
					finally
					{
						((global::System.IDisposable)val5)?.Dispose();
					}
				}
				finally
				{
					((global::System.IDisposable)val4)?.Dispose();
				}
			}
			finally
			{
				((global::System.IDisposable)val3)?.Dispose();
			}
		}
		finally
		{
			((global::System.IDisposable)val)?.Dispose();
		}
	}

	private static byte[] CombineArrays(byte[] array1, byte[] array2)
	{
		byte[] array3 = new byte[array1.Length + array2.Length];
		Buffer.BlockCopy((global::System.Array)array1, 0, (global::System.Array)array3, 0, array1.Length);
		Buffer.BlockCopy((global::System.Array)array2, 0, (global::System.Array)array3, array1.Length, array2.Length);
		return array3;
	}

	private static byte[] ExtractIv(byte[] combined)
	{
		byte[] array = new byte[16];
		Buffer.BlockCopy((global::System.Array)combined, 0, (global::System.Array)array, 0, array.Length);
		return array;
	}

	private static byte[] ExtractCipherText(byte[] combined)
	{
		byte[] array = new byte[combined.Length - 16];
		Buffer.BlockCopy((global::System.Array)combined, 16, (global::System.Array)array, 0, array.Length);
		return array;
	}
}
