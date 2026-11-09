using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace TPSStudios;

internal class Catalogue
{
	public List<Projet> Projets = new List<Projet>();

	public List<Dictionary<string, string>> JeuxWeb = new List<Dictionary<string, string>>();

	public Dictionary<string, string> JeuOmsi;

	public string LanceurVersion;

	public string LanceurUrl;

	public string LanceurSha;

	public Dictionary<string, string> Communaute = new Dictionary<string, string>();

	public static Catalogue Charger(out string erreur)
	{
		erreur = null;
		string text = null;
		string path = Path.Combine(Config.Cache, "catalogue.json");
		try
		{
			if (File.Exists(Config.Catalogue))
			{
				text = File.ReadAllText(Config.Catalogue, Encoding.UTF8);
			}
			else if (Config.Catalogue == "https://raw.githubusercontent.com/Dreams9972/TPS-Studios/main/catalogue.json")
			{
				try
				{
					using WebClient webClient = Web.Client();
					webClient.Encoding = Encoding.UTF8;
					webClient.Headers[HttpRequestHeader.Accept] = "application/vnd.github.raw";
					text = webClient.DownloadString("https://api.github.com/repos/Dreams9972/TPS-Studios/contents/catalogue.json?ref=main");
				}
				catch
				{
					text = null;
				}
				if (text == null || !text.TrimStart().StartsWith("{"))
				{
					using WebClient webClient2 = Web.Client();
					webClient2.Encoding = Encoding.UTF8;
					text = webClient2.DownloadString(Config.Catalogue + "?t=" + DateTime.UtcNow.Ticks);
				}
			}
			else
			{
				using WebClient webClient3 = Web.Client();
				webClient3.Encoding = Encoding.UTF8;
				text = webClient3.DownloadString(Config.Catalogue + (Config.Catalogue.StartsWith("http") ? ("?t=" + DateTime.UtcNow.Ticks) : ""));
			}
			File.WriteAllText(path, text, Encoding.UTF8);
		}
		catch (Exception ex)
		{
			erreur = ex.Message;
			if (File.Exists(path))
			{
				text = File.ReadAllText(path, Encoding.UTF8);
			}
		}
		Catalogue catalogue = new Catalogue();
		if (text == null)
		{
			return catalogue;
		}
		JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
		javaScriptSerializer.MaxJsonLength = 16777216;
		Dictionary<string, object> dictionary;
		try
		{
			dictionary = javaScriptSerializer.Deserialize<Dictionary<string, object>>(text);
		}
		catch (Exception ex2)
		{
			erreur = "catalogue illisible (" + ex2.Message + ")";
			Journal.Ecrire("catalogue", ex2);
			return catalogue;
		}
		if (dictionary == null)
		{
			return catalogue;
		}
		if (dictionary.TryGetValue("lanceur", out var value) && value is Dictionary<string, object>)
		{
			Dictionary<string, object> dictionary2 = (Dictionary<string, object>)value;
			if (dictionary2.TryGetValue("version", out var value2))
			{
				catalogue.LanceurVersion = string.Concat(value2);
			}
			if (dictionary2.TryGetValue("url", out value2))
			{
				catalogue.LanceurUrl = string.Concat(value2);
			}
			if (dictionary2.TryGetValue("sha256", out value2))
			{
				catalogue.LanceurSha = string.Concat(value2);
			}
		}
		if (dictionary.TryGetValue("communaute", out value) && value is Dictionary<string, object>)
		{
			foreach (KeyValuePair<string, object> item in (Dictionary<string, object>)value)
			{
				catalogue.Communaute[item.Key] = string.Concat(item.Value);
			}
		}
		if ((dictionary.TryGetValue("jeux_liste", out value) || dictionary.TryGetValue("jeux", out value)) && value is ArrayList)
		{
			foreach (object item2 in (ArrayList)value)
			{
				if (!(item2 is Dictionary<string, object> dictionary3))
				{
					continue;
				}
				Dictionary<string, string> dictionary4 = new Dictionary<string, string>();
				foreach (KeyValuePair<string, object> item3 in dictionary3)
				{
					dictionary4[item3.Key] = string.Concat(item3.Value);
				}
				string value4;
				Uri result;
				if (dictionary4.TryGetValue("id", out var value0))
				{
					dictionary4.TryGetValue("categories", out var value5);
					Categories.Definir(value0, value5);
				}
				if (dictionary4.TryGetValue("id", out var value3) && value3 == "omsi2")
				{
					catalogue.JeuOmsi = dictionary4;
				}
				else if (dictionary4.TryGetValue("id", out value3) && Regex.IsMatch(value3, "^[a-z0-9_-]{2,40}$") && dictionary4.TryGetValue("url", out value4) && Uri.TryCreate(value4, UriKind.Absolute, out result) && (result.Scheme == Uri.UriSchemeHttps || result.Scheme == Uri.UriSchemeHttp))
				{
					catalogue.JeuxWeb.Add(dictionary4);
				}
			}
		}
		if (dictionary.TryGetValue("projets", out value) && value is ArrayList)
		{
			foreach (object item4 in (ArrayList)value)
			{
				if (item4 is Dictionary<string, object>)
				{
					catalogue.Projets.Add(Projet.De((Dictionary<string, object>)item4));
				}
			}
		}
		if (!Comptes.EstAdmin)
		{
			catalogue.Projets.RemoveAll((Projet p) => p.StatutEffectif == "masque");
		}
		List<KeyValuePair<int, Projet>> list = new List<KeyValuePair<int, Projet>>();
		for (int i = 0; i < catalogue.Projets.Count; i++)
		{
			list.Add(new KeyValuePair<int, Projet>(i, catalogue.Projets[i]));
		}
		list.Sort(delegate(KeyValuePair<int, Projet> a, KeyValuePair<int, Projet> b)
		{
			if (a.Value.ALaUne != b.Value.ALaUne)
			{
				if (!a.Value.ALaUne)
				{
					return 1;
				}
				return -1;
			}
			return (a.Value.Ordre != b.Value.Ordre) ? a.Value.Ordre.CompareTo(b.Value.Ordre) : a.Key.CompareTo(b.Key);
		});
		catalogue.Projets.Clear();
		foreach (KeyValuePair<int, Projet> item5 in list)
		{
			catalogue.Projets.Add(item5.Value);
		}
		return catalogue;
	}
}
