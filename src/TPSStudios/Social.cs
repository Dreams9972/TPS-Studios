using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace TPSStudios;

internal static class Social
{
	public static string Base = "";

	public static string Pseudo = "";

	public static bool EstAdmin;

	private static string jeton = "";

	private static string cleAdmin = "";

	private static string pseudoSouhaite = "";

	public static List<Ami> Amis = new List<Ami>();

	public static List<Ami> Recues = new List<Ami>();

	public static List<Ami> Envoyees = new List<Ami>();

	public static List<string> CadeauxVip = new List<string>();

	public static bool Vip;

	public static bool BonusPris;

	public static int Points;

	public static int PointsTotal;

	private static readonly List<string> achats = new List<string>();

	public static bool Actif => Base.Length > 0;

	public static bool Connecte => jeton.Length > 0;

	private static string Fichier => Path.Combine(Config.Dossier, "social.dat");

	public static bool CleAdminPresente => cleAdmin.Length > 0;

	public static string MonId
	{
		get
		{
			int num = jeton.IndexOf('.');
			if (num <= 0)
			{
				return "";
			}
			return jeton.Substring(0, num);
		}
	}

	public static void Configurer(string adresse)
	{
		string environmentVariable = Environment.GetEnvironmentVariable("TPS_SERVEUR");
		if (!string.IsNullOrEmpty(environmentVariable))
		{
			adresse = environmentVariable;
		}
		adresse = (adresse ?? "").Trim().TrimEnd('/');
		Uri result;
		bool flag = !string.IsNullOrEmpty(environmentVariable) && Uri.TryCreate(adresse, UriKind.Absolute, out result) && result.IsLoopback;
		if (!Uri.TryCreate(adresse, UriKind.Absolute, out result) || (!(result.Scheme == Uri.UriSchemeHttps) && !flag))
		{
			Base = "";
		}
		else if (Base != adresse)
		{
			Base = adresse;
			jeton = "";
			Pseudo = "";
			EstAdmin = false;
			Lire();
		}
	}

	private static string[] LireFichier()
	{
		try
		{
			return File.Exists(Fichier) ? Encoding.UTF8.GetString(ProtectedData.Unprotect(File.ReadAllBytes(Fichier), null, DataProtectionScope.CurrentUser)).Split('\n') : new string[0];
		}
		catch
		{
			return new string[0];
		}
	}

	private static void Lire()
	{
		string[] array = LireFichier();
		if (array.Length == 0)
		{
			return;
		}
		if (array[0] == "v2")
		{
			if (array.Length > 1 && array[1].Length > 0)
			{
				cleAdmin = array[1];
			}
			for (int i = 2; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('\t');
				if (array2.Length >= 4 && array2[0] == Base && Base.Length > 0)
				{
					jeton = array2[1];
					Pseudo = array2[2];
					EstAdmin = array2[3] == "1";
				}
			}
		}
		else
		{
			if (array.Length >= 4 && array[0] == Base && Base.Length > 0)
			{
				jeton = array[1];
				Pseudo = array[2];
				EstAdmin = array[3] == "1";
			}
			if (array.Length >= 5 && array[4].Length > 0)
			{
				cleAdmin = array[4];
			}
		}
	}

	private static void Ecrire()
	{
		try
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string text = cleAdmin;
			string[] array = LireFichier();
			if (array.Length > 0 && array[0] == "v2")
			{
				if (text.Length == 0 && array.Length > 1)
				{
					text = array[1];
				}
				for (int i = 2; i < array.Length; i++)
				{
					string[] array2 = array[i].Split('\t');
					if (array2.Length >= 4 && array2[0].Length > 0 && array2[1].Length > 0)
					{
						dictionary[array2[0]] = array[i];
					}
				}
			}
			else if (array.Length >= 4)
			{
				if (array[0].Length > 0 && array[1].Length > 0)
				{
					dictionary[array[0]] = array[0] + "\t" + array[1] + "\t" + array[2] + "\t" + array[3];
				}
				if (text.Length == 0 && array.Length >= 5)
				{
					text = array[4];
				}
			}
			if (Base.Length > 0 && jeton.Length > 0)
			{
				dictionary[Base] = Base + "\t" + jeton + "\t" + Pseudo.Replace("\t", " ") + "\t" + (EstAdmin ? "1" : "0");
			}
			List<string> list = new List<string>();
			list.Add("v2");
			list.Add(text);
			list.AddRange(dictionary.Values);
			Disque.Ecrire(Fichier, ProtectedData.Protect(Encoding.UTF8.GetBytes(string.Join("\n", list.ToArray())), null, DataProtectionScope.CurrentUser));
		}
		catch (Exception e)
		{
			Journal.Une("social", e);
		}
	}

	public static string VerifierCleAdmin()
	{
		if (!Actif)
		{
			return "adresse du serveur inconnue";
		}
		if (!CleAdminPresente)
		{
			return "aucune clé administrateur sur ce PC";
		}
		try
		{
			Appel("GET", "/v1/admin/reglages", null, auth: false, out var statut);
			return statut switch
			{
				403 => "clé administrateur refusée par le serveur", 
				200 => null, 
				_ => "le serveur a répondu " + statut, 
			};
		}
		catch (Exception ex)
		{
			return ex.Message;
		}
	}

	public static string CleDepuisFichier(string chemin)
	{
		string text = File.ReadAllText(chemin, Encoding.UTF8);
		Match match = Regex.Match(text, "Cl[eé] administrateur\\s*:\\s*(\\S+)");
		if (match.Success)
		{
			return match.Groups[1].Value.Trim();
		}
		text = text.Trim();
		if (text.Length < 16 || text.Length > 120 || text.Contains(" ") || text.Contains("\n"))
		{
			return null;
		}
		return text;
	}

	public static void DefinirCleAdmin(string cle)
	{
		cleAdmin = (cle ?? "").Trim();
		Ecrire();
	}

	public static void Presence(string statut)
	{
		AppelAuth("PUT", "/v1/presence", new Dictionary<string, object> { 
		{
			"statut",
			statut ?? ""
		} }, out var _);
	}

	public static bool StatsPresence(out int enLigne, out int enJeu)
	{
		enLigne = 0;
		enJeu = 0;
		int statut;
		Dictionary<string, object> dictionary = Appel("GET", "/v1/presence", null, auth: false, out statut);
		if (statut != 200 || dictionary == null)
		{
			return false;
		}
		enLigne = Convert.ToInt32(dictionary["en_ligne"]);
		enJeu = Convert.ToInt32(dictionary["en_jeu"]);
		return true;
	}

	private static List<Ami> LireAmis(Dictionary<string, object> r, string k)
	{
		List<Ami> list = new List<Ami>();
		if (r == null || !r.TryGetValue(k, out var value) || value == null || value is string)
		{
			return list;
		}
		if (!(value is IEnumerable enumerable))
		{
			return list;
		}
		foreach (object item in enumerable)
		{
			if (item is Dictionary<string, object> dictionary)
			{
				Ami ami = new Ami();
				if (dictionary.TryGetValue("id", out var value2))
				{
					ami.Id = Convert.ToString(value2);
				}
				if (dictionary.TryGetValue("pseudo", out value2))
				{
					ami.Pseudo = Convert.ToString(value2);
				}
				if (dictionary.TryGetValue("niveau", out value2))
				{
					ami.Niveau = Convert.ToInt32(value2);
				}
				if (dictionary.TryGetValue("vip", out value2))
				{
					ami.Vip = Convert.ToBoolean(value2);
				}
				if (dictionary.TryGetValue("statut", out value2))
				{
					ami.Statut = Convert.ToString(value2);
				}
				if (dictionary.TryGetValue("en_ligne", out value2))
				{
					ami.EnLigne = Convert.ToBoolean(value2);
				}
				if (dictionary.TryGetValue("lien", out value2))
				{
					ami.Lien = Convert.ToString(value2);
				}
				if (dictionary.TryGetValue("communs", out value2) && value2 != null && !(value2 is IEnumerable))
				{
					ami.Communs = Convert.ToInt32(value2);
				}
				if (ami.Id.Length > 0)
				{
					list.Add(ami);
				}
			}
		}
		return list;
	}

	public static bool ChargerAmis()
	{
		if (!Actif || !Connecte)
		{
			return false;
		}
		int st;
		Dictionary<string, object> dictionary = AppelAuth("GET", "/v1/amis", null, out st);
		if (st != 200 || dictionary == null)
		{
			return false;
		}
		List<Ami> amis = LireAmis(dictionary, "amis");
		List<Ami> recues = LireAmis(dictionary, "recues");
		List<Ami> envoyees = LireAmis(dictionary, "envoyees");
		Amis = amis;
		Recues = recues;
		Envoyees = envoyees;
		List<string> list = new List<string>();
		if (dictionary.TryGetValue("cadeaux", out var value) && value is IEnumerable && !(value is string))
		{
			foreach (object item in (IEnumerable)value)
			{
				if (item is Dictionary<string, object> dictionary2 && dictionary2.TryGetValue("de", out var value2))
				{
					list.Add(Convert.ToString(value2));
				}
			}
		}
		CadeauxVip = list;
		return true;
	}

	public static void CadeauxVus()
	{
		AppelAuth("POST", "/v1/cadeaux/vu", new Dictionary<string, object>(), out var _);
		CadeauxVip = new List<string>();
	}

	public static string CodeAmi(string id)
	{
		id = (id ?? "").ToUpperInvariant();
		if (id.Length != 16)
		{
			return id;
		}
		return id.Substring(0, 4) + "-" + id.Substring(4, 4) + "-" + id.Substring(8, 4) + "-" + id.Substring(12, 4);
	}

	public static string IdDepuisCode(string code)
	{
		string text = Regex.Replace((code ?? "").ToLowerInvariant(), "[^0-9a-f]", "");
		if (text.Length != 16)
		{
			return null;
		}
		return text;
	}

	public static ProfilAmi ProfilJoueur(string id)
	{
		int st;
		Dictionary<string, object> dictionary = AppelAuth("GET", "/v1/joueurs/" + id, null, out st);
		if (st != 200 || dictionary == null)
		{
			throw new Exception(Msg(dictionary, "profil indisponible"));
		}
		ProfilAmi profilAmi = new ProfilAmi();
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		dictionary2.Add("x", new object[1] { dictionary });
		List<Ami> list = LireAmis(dictionary2, "x");
		if (list.Count > 0)
		{
			profilAmi.A = list[0];
		}
		profilAmi.EstAmi = dictionary.TryGetValue("ami", out var value) && Convert.ToBoolean(value);
		profilAmi.Communs = LireAmis(dictionary, "communs");
		profilAmi.A.Communs = profilAmi.Communs.Count;
		if (dictionary.TryGetValue("mods", out value) && value is IEnumerable && !(value is string))
		{
			foreach (object item in (IEnumerable)value)
			{
				if (item is Dictionary<string, object> dictionary3)
				{
					dictionary3.TryGetValue("id", out var value2);
					dictionary3.TryGetValue("version", out var value3);
					dictionary3.TryGetValue("actif", out var value4);
					profilAmi.Mods.Add(new string[3]
					{
						Convert.ToString(value2),
						Convert.ToString(value3 ?? ""),
						(value4 is bool && !(bool)value4) ? "0" : "1"
					});
				}
			}
		}
		return profilAmi;
	}

	public static bool EnvoyerMods(List<string[]> l)
	{
		if (!Actif || !Connecte)
		{
			return false;
		}
		ArrayList arrayList = new ArrayList();
		foreach (string[] item in l)
		{
			arrayList.Add(new Dictionary<string, object>
			{
				{
					"id",
					item[0]
				},
				{
					"version",
					item[1]
				},
				{
					"actif",
					item[2] != "0"
				}
			});
		}
		AppelAuth("PUT", "/v1/mods", new Dictionary<string, object> { { "mods", arrayList } }, out var st);
		return st == 200;
	}

	public static string EnvoyerSupport(Dictionary<string, object> t)
	{
		if (!Actif)
		{
			return "le serveur TPS Groupe n'est pas disponible";
		}
		int statut;
		Dictionary<string, object> r = Appel("POST", "/v1/support", t, Connecte, out statut);
		if (statut != 201 && statut != 200)
		{
			return Msg(r, "le ticket n'a pas pu être envoyé (" + statut + ")");
		}
		return null;
	}

	public static string ReglerWebhook(string url)
	{
		if (!Actif)
		{
			return "indiquez d'abord l'adresse du serveur";
		}
		if (!CleAdminPresente)
		{
			return "clé administrateur absente sur ce PC : cliquez sur « Importer la clé »";
		}
		int statut;
		Dictionary<string, object> r = Appel("PUT", "/v1/admin/reglages", new Dictionary<string, object> { { "webhook_support", url } }, auth: false, out statut);
		return statut switch
		{
			403 => "clé administrateur refusée : cliquez sur « Importer la clé » et choisissez le fichier cle-admin.txt du serveur", 
			200 => null, 
			_ => Msg(r, "refusé par le serveur (" + statut + ")"), 
		};
	}

	public static bool? WebhookRegle()
	{
		try
		{
			if (!Actif || !CleAdminPresente)
			{
				return null;
			}
			int statut;
			Dictionary<string, object> dictionary = Appel("GET", "/v1/admin/reglages", null, auth: false, out statut);
			object value;
			return (statut == 200 && dictionary != null && dictionary.TryGetValue("webhook_support", out value)) ? new bool?(Convert.ToBoolean(value)) : null;
		}
		catch
		{
			return null;
		}
	}

	public static string OffrirVip(string id, string code)
	{
		int st;
		Dictionary<string, object> r = AppelAuth("POST", "/v1/vip/offrir", new Dictionary<string, object>
		{
			{ "id", id },
			{ "code", code }
		}, out st);
		if (st != 200)
		{
			return Msg(r, "le VIP n'a pas pu être offert");
		}
		return null;
	}

	public static List<Ami> Chercher(string q)
	{
		int st;
		Dictionary<string, object> r = AppelAuth("GET", "/v1/joueurs/recherche?q=" + Uri.EscapeDataString(q ?? ""), null, out st);
		if (st != 200)
		{
			throw new Exception(Msg(r, "recherche impossible"));
		}
		return LireAmis(r, "joueurs");
	}

	public static string DemanderAmi(string id)
	{
		int st;
		Dictionary<string, object> r = AppelAuth("POST", "/v1/amis/demande", new Dictionary<string, object> { { "id", id } }, out st);
		if (st != 200 && st != 201)
		{
			return Msg(r, "demande refusée");
		}
		return null;
	}

	public static string AccepterAmi(string id)
	{
		int st;
		Dictionary<string, object> r = AppelAuth("POST", "/v1/amis/" + id + "/accepter", new Dictionary<string, object>(), out st);
		if (st != 200)
		{
			return Msg(r, "acceptation impossible");
		}
		return null;
	}

	public static string RetirerAmi(string id)
	{
		int st;
		Dictionary<string, object> r = AppelAuth("DELETE", "/v1/amis/" + id, null, out st);
		if (st != 200)
		{
			return Msg(r, "action impossible");
		}
		return null;
	}

	public static string VerifierVip(string id)
	{
		int statut;
		Dictionary<string, object> dictionary = Appel("GET", "/v1/admin/vip/verifier?id=" + Uri.EscapeDataString(id ?? ""), null, auth: false, out statut);
		if (statut != 200 || dictionary == null)
		{
			return "✖  " + Msg(dictionary, "vérification impossible (" + statut + ")");
		}
		bool flag = Convert.ToBoolean(dictionary["vip"]);
		long num = Convert.ToInt64(dictionary["depuis"]);
		return string.Concat(flag ? "✔  VIP : " : "✖  pas VIP : ", dictionary["pseudo"], flag ? (" (depuis le " + new DateTime(1970, 1, 1).AddSeconds(num).ToLocalTime().ToString("dd/MM/yyyy") + ")") : "");
	}

	public static bool Possede(string article)
	{
		lock (achats)
		{
			return achats.Contains(article);
		}
	}

	private static void LirePoints(Dictionary<string, object> r)
	{
		if (r == null)
		{
			return;
		}
		if (r.ContainsKey("vip"))
		{
			Vip = Convert.ToBoolean(r["vip"]);
		}
		if (r.ContainsKey("solde"))
		{
			Points = Convert.ToInt32(r["solde"]);
		}
		if (r.ContainsKey("total"))
		{
			PointsTotal = Convert.ToInt32(r["total"]);
		}
		if (r.ContainsKey("bonus_pris"))
		{
			BonusPris = Convert.ToBoolean(r["bonus_pris"]);
		}
		IEnumerable enumerable = (r.ContainsKey("achats") ? (r["achats"] as IEnumerable) : null);
		if (enumerable == null)
		{
			return;
		}
		lock (achats)
		{
			achats.Clear();
			foreach (object item in enumerable)
			{
				achats.Add(Convert.ToString(item));
			}
		}
	}

	public static string ActiverVip(string code)
	{
		int st;
		Dictionary<string, object> dictionary = AppelAuth("POST", "/v1/vip/activer", new Dictionary<string, object> { { "code", code } }, out st);
		if (st == 200 && dictionary != null)
		{
			Vip = true;
			return null;
		}
		return Msg(dictionary, "activation impossible");
	}

	public static void EtatPoints()
	{
		int st;
		Dictionary<string, object> r = AppelAuth("GET", "/v1/vip/points", null, out st);
		if (st == 200)
		{
			LirePoints(r);
		}
	}

	public static int Pointage()
	{
		int st;
		Dictionary<string, object> dictionary = AppelAuth("POST", "/v1/vip/pointage", new Dictionary<string, object>(), out st);
		if (st == 200 && dictionary != null)
		{
			LirePoints(dictionary);
			BonusPris = true;
			if (!dictionary.ContainsKey("gagne"))
			{
				return 0;
			}
			return Convert.ToInt32(dictionary["gagne"]);
		}
		return 0;
	}

	public static string Acheter(string article)
	{
		int st;
		Dictionary<string, object> dictionary = AppelAuth("POST", "/v1/boutique/acheter", new Dictionary<string, object> { { "article", article } }, out st);
		if (st == 200 && dictionary != null)
		{
			LirePoints(dictionary);
			return null;
		}
		return Msg(dictionary, "achat impossible");
	}

	/// <summary>Crée n codes VIP (1 à 500) : le serveur en accepte 50 par demande, on enchaîne les demandes.</summary>
	public static List<string> CodesVip(int n, string note, out string err)
	{
		err = null;
		n = Math.Max(1, Math.Min(500, n));
		List<string> tous = new List<string>();
		while (tous.Count < n)
		{
			List<string> lot = CodesVipLot(Math.Min(50, n - tous.Count), note, out err);
			if (lot == null || lot.Count == 0)
			{
				if (tous.Count == 0)
				{
					return null;
				}
				err = null;
				break;
			}
			tous.AddRange(lot);
		}
		return tous;
	}

	private static List<string> CodesVipLot(int n, string note, out string err)
	{
		err = null;
		int statut;
		Dictionary<string, object> dictionary = Appel("POST", "/v1/admin/vip/codes", new Dictionary<string, object>
		{
			{ "n", n },
			{ "note", note }
		}, auth: false, out statut);
		if (statut != 201 || dictionary == null)
		{
			err = Msg(dictionary, "refusé par le serveur (" + statut + ")");
			return null;
		}
		List<string> list = new List<string>();
		if (dictionary["codes"] is IEnumerable enumerable)
		{
			foreach (object item in enumerable)
			{
				list.Add(Convert.ToString(item));
			}
		}
		return list;
	}

	public static Dictionary<string, object> Appel(string methode, string chemin, object corps, bool auth, out int statut)
	{
		statut = 0;
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
		HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Base + chemin);
		httpWebRequest.Method = methode;
		httpWebRequest.Timeout = 9000;
		httpWebRequest.ReadWriteTimeout = 9000;
		httpWebRequest.UserAgent = "TPS-Studios-Lanceur/" + Config.VERSION;
		httpWebRequest.ContentType = "application/json";
		httpWebRequest.Accept = "application/json";
		if (auth && jeton.Length > 0)
		{
			httpWebRequest.Headers["Authorization"] = "Bearer " + jeton;
		}
		if ((chemin == "/v1/inscription" || chemin.StartsWith("/v1/admin/")) && cleAdmin.Length > 0)
		{
			httpWebRequest.Headers["X-Admin-Key"] = cleAdmin;
		}
		if (corps != null)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize(corps));
			httpWebRequest.ContentLength = bytes.Length;
			using Stream stream = httpWebRequest.GetRequestStream();
			stream.Write(bytes, 0, bytes.Length);
		}
		else if (methode != "GET")
		{
			httpWebRequest.ContentLength = 0L;
		}
		string input;
		try
		{
			using HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			statut = (int)httpWebResponse.StatusCode;
			input = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8).ReadToEnd();
		}
		catch (WebException ex)
		{
			if (!(ex.Response is HttpWebResponse httpWebResponse2))
			{
				throw new Exception("serveur injoignable");
			}
			statut = (int)httpWebResponse2.StatusCode;
			input = new StreamReader(httpWebResponse2.GetResponseStream(), Encoding.UTF8).ReadToEnd();
		}
		try
		{
			return new JavaScriptSerializer().DeserializeObject(input) as Dictionary<string, object>;
		}
		catch
		{
			return null;
		}
	}

	private static string Msg(Dictionary<string, object> r, string defaut)
	{
		if (r == null || !r.ContainsKey("erreur"))
		{
			return defaut;
		}
		return Convert.ToString(r["erreur"]);
	}

	private static Dictionary<string, object> AppelAuth(string m, string chemin, object corps, out int st)
	{
		Dictionary<string, object> result = Appel(m, chemin, corps, auth: true, out st);
		if (st == 401 && pseudoSouhaite.Length > 0)
		{
			jeton = "";
			string text = Inscrire(pseudoSouhaite);
			if (text != null)
			{
				throw new Exception(text);
			}
			result = Appel(m, chemin, corps, auth: true, out st);
		}
		return result;
	}

	public static string Inscrire(string pseudo)
	{
		Random random = new Random();
		for (int i = 0; i < 3; i++)
		{
			string text = ((i == 0) ? pseudo : (pseudo + random.Next(10, 99)));
			if (text.Length > 20)
			{
				text = text.Substring(0, 20);
			}
			int statut;
			Dictionary<string, object> dictionary = Appel("POST", "/v1/inscription", new Dictionary<string, object> { { "pseudo", text } }, auth: false, out statut);
			if (statut == 201 && dictionary != null)
			{
				jeton = Convert.ToString(dictionary["jeton"]);
				Pseudo = Convert.ToString(dictionary["pseudo"]);
				EstAdmin = dictionary.ContainsKey("admin") && Convert.ToBoolean(dictionary["admin"]);
				Ecrire();
				return null;
			}
			if (statut == 409 && i == 0 && cleAdmin.Length > 0)
			{
				int statut2;
				Dictionary<string, object> dictionary2 = Appel("POST", "/v1/admin/reprendre", new Dictionary<string, object> { { "pseudo", text } }, auth: false, out statut2);
				if (statut2 == 200 && dictionary2 != null)
				{
					jeton = Convert.ToString(dictionary2["jeton"]);
					Pseudo = Convert.ToString(dictionary2["pseudo"]);
					EstAdmin = dictionary2.ContainsKey("admin") && Convert.ToBoolean(dictionary2["admin"]);
					Ecrire();
					return null;
				}
			}
			if (statut != 409)
			{
				return Msg(dictionary, "inscription impossible");
			}
		}
		return "ce pseudo est déjà pris";
	}

	public static string Assurer(Compte c)
	{
		pseudoSouhaite = ((c != null) ? c.Pseudo : "Joueur");
		if (!Connecte)
		{
			return Inscrire(pseudoSouhaite);
		}
		return null;
	}

	public static ComSocial Com(Dictionary<string, object> d)
	{
		if (d == null || !d.ContainsKey("id"))
		{
			return null;
		}
		ComSocial comSocial = new ComSocial();
		comSocial.Id = Convert.ToInt32(d["id"]);
		comSocial.Auteur = Convert.ToString(d.ContainsKey("auteur") ? d["auteur"] : "");
		comSocial.Texte = Convert.ToString(d.ContainsKey("texte") ? d["texte"] : "");
		comSocial.Niveau = ((!d.ContainsKey("niveau")) ? 1 : Convert.ToInt32(d["niveau"]));
		comSocial.Admin = d.ContainsKey("admin") && Convert.ToBoolean(d["admin"]);
		comSocial.Vip = d.ContainsKey("vip") && Convert.ToBoolean(d["vip"]);
		comSocial.Statut = (d.ContainsKey("statut") ? Convert.ToString(d["statut"]) : "");
		comSocial.Cree = (d.ContainsKey("cree") ? Convert.ToInt64(d["cree"]) : 0);
		comSocial.Likes = ((d.ContainsKey("likes") && d["likes"] != null) ? Convert.ToInt32(d["likes"]) : 0);
		comSocial.Aime = d.ContainsKey("aime") && d["aime"] != null && Convert.ToBoolean(d["aime"]);
		comSocial.Moi = (d.ContainsKey("moi") && Convert.ToBoolean(d["moi"])) || (Pseudo.Length > 0 && comSocial.Auteur == Pseudo);
		return comSocial;
	}

	public static EtatSocial Etat(string mod)
	{
		int statut;
		Dictionary<string, object> dictionary = Appel("GET", "/v1/mod/" + mod, null, auth: true, out statut);
		if (statut != 200 || dictionary == null)
		{
			throw new Exception(Msg(dictionary, "réponse inattendue du serveur"));
		}
		EtatSocial etatSocial = new EtatSocial();
		etatSocial.Likes = Convert.ToInt32(dictionary["likes"]);
		etatSocial.Aime = Convert.ToBoolean(dictionary["aime"]);
		if (dictionary["commentaires"] is IEnumerable enumerable)
		{
			foreach (object item in enumerable)
			{
				ComSocial comSocial = Com(item as Dictionary<string, object>);
				if (comSocial != null)
				{
					etatSocial.Coms.Add(comSocial);
				}
			}
		}
		return etatSocial;
	}

	public static int Aimer(string mod, out bool aime)
	{
		int st;
		Dictionary<string, object> dictionary = AppelAuth("POST", "/v1/mod/" + mod + "/aimer", new Dictionary<string, object>(), out st);
		if (st != 200 || dictionary == null)
		{
			throw new Exception(Msg(dictionary, "échec"));
		}
		aime = Convert.ToBoolean(dictionary["aime"]);
		return Convert.ToInt32(dictionary["likes"]);
	}

	public static ComSocial Commenter(string mod, string texte)
	{
		int st;
		Dictionary<string, object> dictionary = AppelAuth("POST", "/v1/mod/" + mod + "/commentaires", new Dictionary<string, object> { { "texte", texte } }, out st);
		if (st != 201 || dictionary == null)
		{
			throw new Exception(Msg(dictionary, "échec"));
		}
		ComSocial comSocial = Com(dictionary);
		if (comSocial != null)
		{
			comSocial.Moi = true;
		}
		return comSocial;
	}

	/// <summary>Aimer / ne plus aimer un commentaire (renvoie le nouveau nombre de J'aime).</summary>
	public static int AimerCom(int id, out bool aime)
	{
		int st;
		Dictionary<string, object> dictionary = AppelAuth("POST", "/v1/commentaires/" + id + "/aimer", new Dictionary<string, object>(), out st);
		if (st == 404 && (dictionary == null || !dictionary.ContainsKey("erreur") || Convert.ToString(dictionary["erreur"]).IndexOf("commentaire") < 0))
		{
			throw new Exception("les J'aime sur les commentaires ne sont pas encore activés sur le serveur");
		}
		if (st != 200 || dictionary == null)
		{
			throw new Exception(Msg(dictionary, "échec"));
		}
		aime = Convert.ToBoolean(dictionary["aime"]);
		return Convert.ToInt32(dictionary["likes"]);
	}

	public static void Supprimer(int id)
	{
		int st;
		Dictionary<string, object> r = AppelAuth("DELETE", "/v1/commentaires/" + id, null, out st);
		if (st != 200)
		{
			throw new Exception(Msg(r, "suppression refusée"));
		}
	}

	public static Dictionary<string, object> Moi()
	{
		int st;
		Dictionary<string, object> dictionary = AppelAuth("GET", "/v1/moi", null, out st);
		if (st == 200 && dictionary != null && dictionary.ContainsKey("admin"))
		{
			EstAdmin = Convert.ToBoolean(dictionary["admin"]);
		}
		if (st == 200 && dictionary != null && dictionary.ContainsKey("vip"))
		{
			Vip = Convert.ToBoolean(dictionary["vip"]);
		}
		if (st != 200)
		{
			return null;
		}
		return dictionary;
	}

	public static void MajNiveau(int niv)
	{
		AppelAuth("PUT", "/v1/moi", new Dictionary<string, object> { { "niveau", niv } }, out var _);
	}
}
