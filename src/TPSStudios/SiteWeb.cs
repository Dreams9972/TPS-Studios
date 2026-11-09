using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;

namespace TPSStudios;

/// <summary>Une actualité du site web TPS Groupe.</summary>
internal class ArticleSite
{
	public string Titre = "";

	public string Resume = "";

	public string Lien = "";

	public string ImageUrl = "";

	public DateTime? Date;

	public Image Image;
}

/// <summary>
/// Actualités du site web TPS Groupe affichées dans la grande case de l'accueil. Le flux se
/// règle dans l'admin (Accueil › Site web) : RSS, Atom ou JSON. Il est relu toutes les
/// 10 minutes, avec une copie locale pour les démarrages hors ligne.
/// </summary>
internal static class SiteWeb
{
	private static List<ArticleSite> articles = new List<ArticleSite>();

	private static string fluxCharge = "";

	private static DateTime lu = DateTime.MinValue;

	private static bool enCours;

	public static List<ArticleSite> Articles
	{
		get
		{
			lock (typeof(SiteWeb))
			{
				return new List<ArticleSite>(articles);
			}
		}
	}

	/// <summary>Relit le flux s'il a changé ou date de plus de 10 minutes ; « pret » est appelé (hors fil d'interface) quand la liste change.</summary>
	public static void Rafraichir(string flux, string site, Action pret)
	{
		flux = (flux ?? "").Trim();
		if (flux.Length == 0)
		{
			lock (typeof(SiteWeb))
			{
				if (articles.Count > 0)
				{
					articles = new List<ArticleSite>();
					fluxCharge = "";
					pret?.Invoke();
				}
			}
			return;
		}
		lock (typeof(SiteWeb))
		{
			if (enCours || (flux == fluxCharge && (DateTime.Now - lu).TotalMinutes < 10.0))
			{
				return;
			}
			enCours = true;
		}
		Thread thread = new Thread((ThreadStart)delegate
		{
			List<ArticleSite> list = null;
			string cache = Path.Combine(Config.Cache, "site_flux.txt");
			try
			{
				string texte = null;
				try
				{
					using WebClient webClient = Web.Client();
					webClient.Encoding = Encoding.UTF8;
					texte = webClient.DownloadString(flux);
					File.WriteAllText(cache, flux + "\n" + texte, Encoding.UTF8);
				}
				catch (Exception e)
				{
					Journal.Une("site-web", e);
					if (File.Exists(cache))
					{
						string s = File.ReadAllText(cache, Encoding.UTF8);
						int n = s.IndexOf('\n');
						if (n > 0 && s.Substring(0, n) == flux)
						{
							texte = s.Substring(n + 1);
						}
					}
				}
				if (texte != null)
				{
					list = Lire(texte, flux, site);
					foreach (ArticleSite a in list)
					{
						a.Image = ChargerImage(a.ImageUrl);
					}
				}
			}
			catch (Exception e2)
			{
				Journal.Une("site-web", e2);
			}
			lock (typeof(SiteWeb))
			{
				enCours = false;
				lu = DateTime.Now;
				fluxCharge = flux;
				if (list != null)
				{
					articles = list;
				}
			}
			if (list != null)
			{
				pret?.Invoke();
			}
		});
		thread.IsBackground = true;
		thread.Start();
	}

	private static Image ChargerImage(string url)
	{
		if (!Uri.TryCreate(url ?? "", UriKind.Absolute, out var u) || (u.Scheme != Uri.UriSchemeHttps && u.Scheme != Uri.UriSchemeHttp))
		{
			return null;
		}
		try
		{
			using WebClient webClient = Web.Client();
			byte[] buffer = webClient.DownloadData(u);
			using MemoryStream stream = new MemoryStream(buffer);
			using Image image = Image.FromStream(stream);
			return new Bitmap(image);
		}
		catch (Exception e)
		{
			Journal.Une("site-web-image", e);
			return null;
		}
	}

	/// <summary>Lit un flux RSS, Atom ou JSON (8 articles au maximum, les plus récents d'abord).</summary>
	public static List<ArticleSite> Lire(string texte, string flux, string site)
	{
		List<ArticleSite> list = new List<ArticleSite>();
		string t = (texte ?? "").TrimStart('\ufeff', ' ', '\r', '\n', '\t');
		if (t.StartsWith("[") || t.StartsWith("{"))
		{
			LireJson(t, list);
		}
		else
		{
			foreach (Match item in Regex.Matches(t, "<(item|entry)\\b[^>]*>(.*?)</\\1>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
			{
				string x = item.Groups[2].Value;
				ArticleSite a = new ArticleSite();
				a.Titre = Texte(Balise(x, "title"));
				string lien = Balise(x, "link");
				if (lien.Length == 0)
				{
					Match m = Regex.Match(x, "<link\\b[^>]*href=\"([^\"]+)\"", RegexOptions.IgnoreCase);
					lien = (m.Success ? WebUtility.HtmlDecode(m.Groups[1].Value) : "");
				}
				a.Lien = lien.Trim();
				string resume = Balise(x, "description");
				if (resume.Length == 0)
				{
					resume = Balise(x, "summary");
				}
				if (resume.Length == 0)
				{
					resume = Balise(x, "content");
				}
				string brut = Cdata(resume);
				a.Resume = Texte(resume);
				Match img = Regex.Match(x, "<(?:enclosure|media:content|media:thumbnail)\\b[^>]*url=\"([^\"]+)\"", RegexOptions.IgnoreCase);
				if (!img.Success)
				{
					img = Regex.Match(WebUtility.HtmlDecode(brut), "<img\\b[^>]*src=[\"']([^\"']+)[\"']", RegexOptions.IgnoreCase);
				}
				a.ImageUrl = (img.Success ? WebUtility.HtmlDecode(img.Groups[1].Value) : "");
				string d = Balise(x, "pubDate");
				if (d.Length == 0)
				{
					d = Balise(x, "updated");
				}
				if (d.Length == 0)
				{
					d = Balise(x, "published");
				}
				a.Date = LireDate(Texte(d));
				if (a.Titre.Length > 0)
				{
					list.Add(a);
				}
			}
		}
		foreach (ArticleSite a2 in list)
		{
			a2.Lien = Absolue(a2.Lien, flux, site);
			a2.ImageUrl = Absolue(a2.ImageUrl, flux, site);
			if (a2.Resume.Length > 220)
			{
				a2.Resume = a2.Resume.Substring(0, 217).TrimEnd() + "…";
			}
		}
		list.Sort((ArticleSite x, ArticleSite y) => (y.Date ?? DateTime.MinValue).CompareTo(x.Date ?? DateTime.MinValue));
		if (list.Count > 8)
		{
			list.RemoveRange(8, list.Count - 8);
		}
		return list;
	}

	private static void LireJson(string t, List<ArticleSite> list)
	{
		JavaScriptSerializer js = new JavaScriptSerializer();
		js.MaxJsonLength = 20000000;
		object o = js.DeserializeObject(t);
		IEnumerable items = o as IEnumerable;
		if (o is Dictionary<string, object> d0)
		{
			items = null;
			foreach (string k in new string[4] { "articles", "actualites", "items", "posts" })
			{
				if (d0.TryGetValue(k, out var v) && v is IEnumerable && !(v is string))
				{
					items = (IEnumerable)v;
					break;
				}
			}
		}
		if (items == null || o is string)
		{
			return;
		}
		foreach (object item in items)
		{
			if (item is Dictionary<string, object> d)
			{
				ArticleSite a = new ArticleSite();
				a.Titre = Texte(Val(d, "titre", "title", "nom"));
				a.Resume = Texte(Val(d, "resume", "texte", "description", "summary", "excerpt"));
				a.Lien = Val(d, "lien", "url", "link");
				a.ImageUrl = Val(d, "image", "image_url", "cover", "thumbnail");
				a.Date = LireDate(Val(d, "date", "publie", "published", "created_at"));
				if (a.Titre.Length > 0)
				{
					list.Add(a);
				}
			}
		}
	}

	private static string Val(Dictionary<string, object> d, params string[] cles)
	{
		foreach (string k in cles)
		{
			if (d.TryGetValue(k, out var v) && v != null && !(v is IEnumerable && !(v is string)))
			{
				string s = Convert.ToString(v, CultureInfo.InvariantCulture).Trim();
				if (s.Length > 0)
				{
					return s;
				}
			}
		}
		return "";
	}

	private static string Balise(string x, string nom)
	{
		Match m = Regex.Match(x, "<" + Regex.Escape(nom) + "\\b[^>]*>(.*?)</" + Regex.Escape(nom) + ">", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		return m.Success ? m.Groups[1].Value : "";
	}

	private static string Cdata(string s)
	{
		Match m = Regex.Match(s ?? "", "<!\\[CDATA\\[(.*?)\\]\\]>", RegexOptions.Singleline);
		return m.Success ? m.Groups[1].Value : (s ?? "");
	}

	/// <summary>Texte brut : sans CDATA, balises HTML ni entités, espaces resserrés.</summary>
	private static string Texte(string s)
	{
		s = Cdata(s);
		s = WebUtility.HtmlDecode(s);
		s = Regex.Replace(s, "<[^>]+>", " ");
		s = WebUtility.HtmlDecode(s);
		return Regex.Replace(s, "\\s+", " ").Trim();
	}

	private static DateTime? LireDate(string s)
	{
		s = (s ?? "").Trim();
		if (s.Length == 0)
		{
			return null;
		}
		if (DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var r))
		{
			return r.LocalDateTime;
		}
		Match m = Regex.Match(s, "^\\w{3}, (\\d{1,2} \\w{3} \\d{4} \\d{2}:\\d{2}(?::\\d{2})?)");
		if (m.Success && DateTime.TryParse(m.Groups[1].Value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var r2))
		{
			return r2;
		}
		return null;
	}

	private static string Absolue(string u, string flux, string site)
	{
		u = (u ?? "").Trim();
		if (u.Length == 0)
		{
			return "";
		}
		if (Uri.TryCreate(u, UriKind.Absolute, out var a) && (a.Scheme == Uri.UriSchemeHttps || a.Scheme == Uri.UriSchemeHttp))
		{
			return a.AbsoluteUri;
		}
		foreach (string b in new string[2] { site, flux })
		{
			if (Uri.TryCreate(b ?? "", UriKind.Absolute, out var bu) && Uri.TryCreate(bu, u, out var r) && (r.Scheme == Uri.UriSchemeHttps || r.Scheme == Uri.UriSchemeHttp))
			{
				return r.AbsoluteUri;
			}
		}
		return "";
	}
}
