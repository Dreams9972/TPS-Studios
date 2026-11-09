using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace TPSStudios;

/// <summary>
/// Bandes-annonces de la chaîne YouTube TPS Studios : le lanceur lit le flux public de la
/// chaîne (dernières vidéos) et associe une vidéo à un mod quand son titre contient le nom
/// du mod (de préférence « trailer », « bande-annonce » ou « teaser »). Cache d'une journée.
/// </summary>
internal static class YouTube
{
	public const string ChaineParDefaut = "https://www.youtube.com/@TPS_Studios_";

	private static List<string[]> videos;

	private static bool enCours;

	/// <summary>Identifiant YouTube (11 caractères) d'une adresse, ou null.</summary>
	public static string Id(string url)
	{
		if (string.IsNullOrEmpty(url))
		{
			return null;
		}
		Match match = Regex.Match(url, "^https?://(?:www\\.|m\\.)?(?:youtube\\.com/(?:watch\\?(?:.*&)?v=|embed/|shorts/|live/)|youtube-nocookie\\.com/embed/|youtu\\.be/)([A-Za-z0-9_-]{11})");
		if (!match.Success)
		{
			return null;
		}
		return match.Groups[1].Value;
	}

	/// <summary>Bande-annonce trouvée sur la chaîne pour ce mod (null si rien, ou pas encore chargé).</summary>
	public static string TrailerPour(Projet p, string chaine, Action pret)
	{
		List<string[]> list = Videos(chaine, pret);
		if (list == null || p == null)
		{
			return null;
		}
		string text = Norm(p.Nom);
		if (text.Length < 3)
		{
			return null;
		}
		string[] array = null;
		int num = -1;
		foreach (string[] item in list)
		{
			string text2 = Norm(item[1]);
			if (text2.IndexOf(text, StringComparison.Ordinal) >= 0)
			{
				int num2 = ((text2.Contains("trailer") || text2.Contains("bandeannonce") || text2.Contains("teaser")) ? 2 : 1);
				if (num2 > num)
				{
					num = num2;
					array = item;
				}
			}
		}
		if (array == null)
		{
			return null;
		}
		return "https://www.youtube.com/watch?v=" + array[0];
	}

	private static List<string[]> Videos(string chaine, Action pret)
	{
		lock (typeof(YouTube))
		{
			if (videos != null)
			{
				return videos;
			}
			if (enCours)
			{
				return null;
			}
			enCours = true;
		}
		Thread thread = new Thread((ThreadStart)delegate
		{
			List<string[]> list = new List<string[]>();
			try
			{
				list = Charger(string.IsNullOrEmpty(chaine) ? ChaineParDefaut : chaine);
			}
			catch (Exception e)
			{
				Journal.Une("youtube", e);
			}
			lock (typeof(YouTube))
			{
				videos = list;
				enCours = false;
			}
			if (list.Count > 0)
			{
				pret?.Invoke();
			}
		});
		thread.IsBackground = true;
		thread.Start();
		return null;
	}

	private static List<string[]> Charger(string chaine)
	{
		string path = Path.Combine(Config.Cache, "youtube_flux.xml");
		string text = null;
		if (File.Exists(path) && (DateTime.Now - File.GetLastWriteTime(path)).TotalHours < 24.0)
		{
			text = File.ReadAllText(path, Encoding.UTF8);
		}
		else
		{
			try
			{
				string text2 = IdChaine(chaine);
				if (text2 != null)
				{
					using WebClient webClient = Web.Client();
					webClient.Encoding = Encoding.UTF8;
					text = webClient.DownloadString("https://www.youtube.com/feeds/videos.xml?channel_id=" + text2);
					File.WriteAllText(path, text, Encoding.UTF8);
				}
			}
			catch (Exception e)
			{
				Journal.Une("youtube", e);
			}
			if (text == null && File.Exists(path))
			{
				text = File.ReadAllText(path, Encoding.UTF8);
			}
		}
		List<string[]> list = new List<string[]>();
		if (text == null)
		{
			return list;
		}
		foreach (Match item in Regex.Matches(text, "<entry>(.*?)</entry>", RegexOptions.Singleline))
		{
			Match match = Regex.Match(item.Groups[1].Value, "<yt:videoId>([A-Za-z0-9_-]{11})</yt:videoId>");
			Match match2 = Regex.Match(item.Groups[1].Value, "<title>(.*?)</title>", RegexOptions.Singleline);
			if (match.Success && match2.Success)
			{
				list.Add(new string[2]
				{
					match.Groups[1].Value,
					WebUtility.HtmlDecode(match2.Groups[1].Value)
				});
			}
		}
		return list;
	}

	/// <summary>Identifiant « UC… » de la chaîne (adresse /channel/UC… ou page @nom).</summary>
	private static string IdChaine(string chaine)
	{
		Match match = Regex.Match(chaine, "/channel/(UC[A-Za-z0-9_-]{22})");
		if (match.Success)
		{
			return match.Groups[1].Value;
		}
		using WebClient webClient = Web.Client();
		webClient.Encoding = Encoding.UTF8;
		webClient.Headers[HttpRequestHeader.Cookie] = "SOCS=CAI; CONSENT=YES+cb";
		webClient.Headers[HttpRequestHeader.AcceptLanguage] = "fr-FR,fr;q=0.9";
		string input = webClient.DownloadString(chaine);
		Match match2 = Regex.Match(input, "\"(?:channelId|externalId)\"\\s*:\\s*\"(UC[A-Za-z0-9_-]{22})\"");
		if (!match2.Success)
		{
			match2 = Regex.Match(input, "/channel/(UC[A-Za-z0-9_-]{22})");
		}
		if (!match2.Success)
		{
			return null;
		}
		return match2.Groups[1].Value;
	}

	/// <summary>Minuscules, sans accents ni espaces : « Région Parisienne » → « regionparisienne ».</summary>
	public static string Norm(string s)
	{
		string text = (s ?? "").Normalize(NormalizationForm.FormD).ToLowerInvariant();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in text)
		{
			if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark && ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')))
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}
}
