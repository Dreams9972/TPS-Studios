using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;

namespace TPSStudios;

/// <summary>Infos de la fiche Steam d'un jeu (comme le bloc « À propos » de Steam).</summary>
internal class FicheSteam
{
	public string Description = "";

	public string Developpeurs = "";

	public string Editeurs = "";

	public string Sortie = "";

	public string Genres = "";

	public string Fonctionnalites = "";

	public string Langues = "";

	public List<string> ConfigMin = new List<string>();

	/// <summary>Captures : { miniature, pleine taille }.</summary>
	public List<string[]> Captures = new List<string[]>();

	public string Video = "";

	public string Page = "";
}

/// <summary>
/// Lit la fiche d'un jeu sur l'API publique du magasin Steam (appdetails), en français,
/// avec un cache de 3 jours dans le dossier du lanceur. Tout se fait en arrière-plan.
/// </summary>
internal static class Steam
{
	private static readonly Dictionary<string, FicheSteam> fiches = new Dictionary<string, FicheSteam>();

	private static readonly HashSet<string> enCours = new HashSet<string>();

	/// <summary>Renvoie la fiche si elle est déjà connue ; sinon lance son chargement et appelle « pret » à la fin (autre thread).</summary>
	public static FicheSteam Fiche(string appid, Action pret)
	{
		if (string.IsNullOrEmpty(appid))
		{
			return null;
		}
		lock (fiches)
		{
			if (fiches.TryGetValue(appid, out var value))
			{
				return value;
			}
			if (!enCours.Add(appid))
			{
				return null;
			}
		}
		Thread thread = new Thread((ThreadStart)delegate
		{
			FicheSteam ficheSteam = null;
			try
			{
				ficheSteam = Charger(appid);
			}
			catch (Exception e)
			{
				Journal.Une("steam", e);
			}
			lock (fiches)
			{
				fiches[appid] = ficheSteam;
				enCours.Remove(appid);
			}
			if (ficheSteam != null)
			{
				pret?.Invoke();
			}
		});
		thread.IsBackground = true;
		thread.Start();
		return null;
	}

	private static FicheSteam Charger(string appid)
	{
		string path = Path.Combine(Config.Cache, "steam_" + appid + ".json");
		string text = null;
		bool flag = File.Exists(path) && (DateTime.Now - File.GetLastWriteTime(path)).TotalDays < 3.0;
		if (flag)
		{
			text = File.ReadAllText(path, Encoding.UTF8);
		}
		else
		{
			try
			{
				using WebClient webClient = Web.Client();
				webClient.Encoding = Encoding.UTF8;
				text = webClient.DownloadString("https://store.steampowered.com/api/appdetails?appids=" + appid + "&l=french&cc=fr");
				if (Lire(text, appid) != null)
				{
					File.WriteAllText(path, text, Encoding.UTF8);
				}
			}
			catch
			{
				if (File.Exists(path))
				{
					text = File.ReadAllText(path, Encoding.UTF8);
				}
			}
		}
		return Lire(text, appid);
	}

	public static FicheSteam Lire(string json, string appid)
	{
		if (string.IsNullOrEmpty(json))
		{
			return null;
		}
		JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
		javaScriptSerializer.MaxJsonLength = 16777216;
		Dictionary<string, object> dictionary = javaScriptSerializer.Deserialize<Dictionary<string, object>>(json);
		if (dictionary == null || !dictionary.TryGetValue(appid, out var value) || !(value is Dictionary<string, object> dictionary2) || !dictionary2.TryGetValue("data", out var value2) || !(value2 is Dictionary<string, object> d))
		{
			return null;
		}
		FicheSteam ficheSteam = new FicheSteam();
		ficheSteam.Page = "https://store.steampowered.com/app/" + appid;
		ficheSteam.Description = Texte(S(d, "short_description"));
		if (ficheSteam.Description.Length == 0)
		{
			ficheSteam.Description = Texte(S(d, "about_the_game"));
		}
		ficheSteam.Developpeurs = Liste(d, "developers", null);
		ficheSteam.Editeurs = Liste(d, "publishers", null);
		if (d.TryGetValue("release_date", out var value3) && value3 is Dictionary<string, object> d2)
		{
			ficheSteam.Sortie = S(d2, "date");
		}
		ficheSteam.Genres = Liste(d, "genres", "description");
		ficheSteam.Fonctionnalites = Liste(d, "categories", "description", 6);
		string text = Texte(Regex.Replace(S(d, "supported_languages"), "<br>.*$", "", RegexOptions.Singleline)).Replace("*", "");
		ficheSteam.Langues = text;
		if (d.TryGetValue("pc_requirements", out var value4) && value4 is Dictionary<string, object> d3)
		{
			string input = S(d3, "minimum");
			foreach (Match item in Regex.Matches(input, "<li>(.*?)</li>", RegexOptions.Singleline | RegexOptions.IgnoreCase))
			{
				string text2 = Texte(item.Groups[1].Value);
				if (text2.Length > 0 && ficheSteam.ConfigMin.Count < 8)
				{
					ficheSteam.ConfigMin.Add(text2);
				}
			}
		}
		if (d.TryGetValue("screenshots", out var value5) && value5 is ArrayList)
		{
			foreach (object item2 in (ArrayList)value5)
			{
				if (item2 is Dictionary<string, object> d4 && S(d4, "path_full").Length > 0 && ficheSteam.Captures.Count < 12)
				{
					ficheSteam.Captures.Add(new string[2]
					{
						(S(d4, "path_thumbnail").Length > 0) ? S(d4, "path_thumbnail") : S(d4, "path_full"),
						S(d4, "path_full")
					});
				}
			}
		}
		if (d.TryGetValue("movies", out var value6) && value6 is ArrayList)
		{
			foreach (object item3 in (ArrayList)value6)
			{
				if (!(item3 is Dictionary<string, object> dictionary3))
				{
					continue;
				}
				if (dictionary3.TryGetValue("mp4", out var value7) && value7 is Dictionary<string, object> d5)
				{
					ficheSteam.Video = ((S(d5, "max").Length > 0) ? S(d5, "max") : S(d5, "480"));
				}
				if (ficheSteam.Video.Length == 0)
				{
					foreach (object value8 in dictionary3.Values)
					{
						string text3 = (value8 as string) ?? "";
						if (text3.IndexOf(".mp4", StringComparison.OrdinalIgnoreCase) > 0)
						{
							ficheSteam.Video = text3;
							break;
						}
					}
				}
				if (ficheSteam.Video.Length > 0)
				{
					break;
				}
			}
		}
		if (ficheSteam.Video.StartsWith("http://"))
		{
			ficheSteam.Video = "https://" + ficheSteam.Video.Substring(7);
		}
		return ficheSteam;
	}

	private static string S(Dictionary<string, object> d, string k)
	{
		if (d == null || !d.TryGetValue(k, out var value) || value == null)
		{
			return "";
		}
		return value.ToString();
	}

	private static string Liste(Dictionary<string, object> d, string k, string champ, int max = 99)
	{
		List<string> list = new List<string>();
		if (d.TryGetValue(k, out var value) && value is ArrayList)
		{
			foreach (object item in (ArrayList)value)
			{
				string text = ((champ == null) ? (item as string) : ((item is Dictionary<string, object> d2) ? S(d2, champ) : null));
				if (!string.IsNullOrEmpty(text) && list.Count < max)
				{
					list.Add(text.Trim());
				}
			}
		}
		return string.Join(", ", list.ToArray());
	}

	/// <summary>HTML de Steam → texte simple.</summary>
	public static string Texte(string html)
	{
		if (string.IsNullOrEmpty(html))
		{
			return "";
		}
		string input = Regex.Replace(html, "<br\\s*/?>", "\n", RegexOptions.IgnoreCase);
		input = Regex.Replace(input, "<[^>]+>", "");
		input = WebUtility.HtmlDecode(input);
		input = Regex.Replace(input, "[ \\t]+", " ");
		input = Regex.Replace(input, "\\n\\s*\\n+", "\n");
		return input.Trim();
	}
}
