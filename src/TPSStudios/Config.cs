using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TPSStudios;

internal static class Config
{
	public const string VERSION = "3.0.4";

	public const string CATALOGUE = "https://raw.githubusercontent.com/Dreams9972/TPS-Studios/main/catalogue.json";

	public const string CATALOGUE_API = "https://api.github.com/repos/Dreams9972/TPS-Studios/contents/catalogue.json?ref=main";

	public static string Catalogue = "https://raw.githubusercontent.com/Dreams9972/TPS-Studios/main/catalogue.json";

	private static Dictionary<string, string> r;

	private static readonly object verrouConfig = new object();

	public static string Dossier
	{
		get
		{
			string environmentVariable = Environment.GetEnvironmentVariable("TPS_DOSSIER");
			if (string.IsNullOrEmpty(environmentVariable))
			{
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TPS Studios");
			}
			return environmentVariable;
		}
	}

	public static string Cache
	{
		get
		{
			string text = Path.Combine(Dossier, "cache");
			Directory.CreateDirectory(text);
			return text;
		}
	}

	private static string Fichier => Path.Combine(Dossier, "reglages.txt");

	public static string Lire(string k)
	{
		if (r == null)
		{
			r = new Dictionary<string, string>();
			try
			{
				string[] array = File.ReadAllLines(Fichier, Encoding.UTF8);
				foreach (string text in array)
				{
					int num = text.IndexOf('=');
					if (num > 0)
					{
						r[text.Substring(0, num)] = text.Substring(num + 1);
					}
				}
			}
			catch
			{
			}
		}
		if (!r.TryGetValue(k, out var value))
		{
			return null;
		}
		return value;
	}

	public static void Reinitialiser(params string[] garder)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (string text in garder)
		{
			string value = Lire(text);
			if (!string.IsNullOrEmpty(value))
			{
				dictionary[text] = value;
			}
		}
		r = new Dictionary<string, string>();
		try
		{
			if (File.Exists(Fichier))
			{
				File.Delete(Fichier);
			}
		}
		catch
		{
		}
		foreach (KeyValuePair<string, string> item in dictionary)
		{
			Ecrire(item.Key, item.Value);
		}
	}

	public static bool Oui(string k, bool defaut)
	{
		string text = Lire(k);
		if (!string.IsNullOrEmpty(text))
		{
			return text == "1";
		}
		return defaut;
	}

	public static void Ecrire(string k, string v)
	{
		lock (verrouConfig)
		{
			Lire(k);
			string text = v ?? "";
			if (r.TryGetValue(k, out var value) && value == text && File.Exists(Fichier))
			{
				return;
			}
			r[k] = text;
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, string> item in r)
			{
				list.Add(item.Key + "=" + item.Value);
			}
			try
			{
				Disque.EcrireTexte(Fichier, string.Join("\r\n", list.ToArray()) + "\r\n");
			}
			catch (Exception e)
			{
				Journal.Une("reglages", e);
			}
		}
	}
}
