using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace TPSStudios;

internal static class Web
{
	private static readonly string[] hotesSurs = new string[5] { "github.com", "raw.githubusercontent.com", "objects.githubusercontent.com", "release-assets.githubusercontent.com", "api.github.com" };

	public static WebClient Client()
	{
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
		WebClient webClient = new WebClientDelai();
		webClient.Headers[HttpRequestHeader.UserAgent] = "TPS-Studios-Lanceur/" + Config.VERSION;
		return webClient;
	}

	public static string Secours(string url)
	{
		Match match = Regex.Match(url ?? "", "^https://github\\.com/([^/]+)/([^/]+)/releases/download/([^/]+)/([^/?#]+)$");
		if (!match.Success)
		{
			return null;
		}
		string input;
		using (WebClient webClient = Client())
		{
			webClient.Headers[HttpRequestHeader.Accept] = "application/vnd.github+json";
			input = webClient.DownloadString("https://api.github.com/repos/" + match.Groups[1].Value + "/" + match.Groups[2].Value + "/releases/tags/" + match.Groups[3].Value);
		}
		string text = Uri.UnescapeDataString(match.Groups[4].Value);
		foreach (Match item in Regex.Matches(input, "\"url\"\\s*:\\s*\"(https://api\\.github\\.com/repos/[^\"]+/releases/assets/\\d+)\"[^{}]*?\"name\"\\s*:\\s*\"([^\"]+)\""))
		{
			if (item.Groups[2].Value == text)
			{
				return item.Groups[1].Value;
			}
		}
		return null;
	}

	public static WebClient ClientSecours()
	{
		WebClient webClient = Client();
		webClient.Headers[HttpRequestHeader.Accept] = "application/octet-stream";
		return webClient;
	}

	public static void Telecharger(string url, string dest)
	{
		try
		{
			using WebClient webClient = Client();
			webClient.DownloadFile(url, dest);
		}
		catch (Exception e)
		{
			string text = null;
			try
			{
				text = Secours(url);
			}
			catch
			{
			}
			if (text == null)
			{
				throw;
			}
			Journal.Une("secours", e);
			using WebClient webClient2 = ClientSecours();
			webClient2.DownloadFile(text, dest);
		}
	}

	public static bool UrlSure(string u)
	{
		if (string.IsNullOrEmpty(u))
		{
			return false;
		}
		if (File.Exists(u))
		{
			return Config.Catalogue != "https://raw.githubusercontent.com/Dreams9972/TPS-Studios/main/catalogue.json";
		}
		if (!Uri.TryCreate(u, UriKind.Absolute, out var result) || result.Scheme != Uri.UriSchemeHttps)
		{
			return false;
		}
		string[] array = hotesSurs;
		foreach (string value in array)
		{
			if (result.Host.Equals(value, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		if (result.Host.EndsWith(".r2.dev", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return false;
	}

	public static string Sha256(string f)
	{
		using SHA256 sHA = SHA256.Create();
		using FileStream inputStream = File.OpenRead(f);
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array = sHA.ComputeHash(inputStream);
		foreach (byte b in array)
		{
			stringBuilder.Append(b.ToString("x2"));
		}
		return stringBuilder.ToString();
	}
}
