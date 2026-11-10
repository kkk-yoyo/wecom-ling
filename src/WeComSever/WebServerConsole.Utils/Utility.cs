using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace WebServerConsole.Utils;

public static class Utility
{
	public static async Task<string> SendPostRequest(string url, string jsonData, int timeoutSeconds = 5)
	{
		using HttpClient client = new HttpClient
		{
			Timeout = TimeSpan.FromSeconds(timeoutSeconds)
		};
		StringContent content = new StringContent(jsonData, Encoding.UTF8, "text/html");
		try
		{
			HttpResponseMessage httpResponseMessage = await client.PostAsync(url, content);
			if (httpResponseMessage.IsSuccessStatusCode)
			{
				return await httpResponseMessage.Content.ReadAsStringAsync();
			}
		}
		catch (TaskCanceledException)
		{
		}
		return null;
	}

	public static string ReadFileToString(string filePath)
	{
		try
		{
			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException("File not found.", filePath);
			}
			return File.ReadAllText(filePath);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error reading file: " + ex.Message);
			return string.Empty;
		}
	}

	public static string DecryptStringFromBase64(string cipherText, string key, string iv)
	{
		byte[] buffer = Convert.FromBase64String(cipherText);
		using Aes aes = Aes.Create();
		aes.Key = Encoding.UTF8.GetBytes(key);
		aes.IV = Encoding.UTF8.GetBytes(iv);
		aes.Mode = CipherMode.CBC;
		aes.Padding = PaddingMode.PKCS7;
		ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
		using MemoryStream stream = new MemoryStream(buffer);
		using CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Read);
		using StreamReader streamReader = new StreamReader(stream2);
		return streamReader.ReadToEnd();
	}

	public static string ReverseString(string str)
	{
		char[] array = str.ToCharArray();
		Array.Reverse(array);
		return new string(array);
	}

	public static bool IsRunningAsAdministrator()
	{
		using WindowsIdentity ntIdentity = WindowsIdentity.GetCurrent();
		return new WindowsPrincipal(ntIdentity).IsInRole(WindowsBuiltInRole.Administrator);
	}

	public static bool IsValidUrl(string url)
	{
		if (Uri.TryCreate(url, UriKind.Absolute, out var result))
		{
			if (!(result.Scheme == Uri.UriSchemeHttp))
			{
				return result.Scheme == Uri.UriSchemeHttps;
			}
			return true;
		}
		return false;
	}
}
