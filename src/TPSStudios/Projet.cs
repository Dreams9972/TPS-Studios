using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace TPSStudios;

internal class Projet
{
	public string Id;

	public string Nom;

	public string SousTitre;

	public string Description;

	public string Version;

	public string Date;

	public string Md5;

	public string Zip;

	public string Sha256;

	public string Image;

	public string Notes;

	public string Jeu;

	public string FichierVersion;

	public string Liste;

	public string Marque;

	public string Carte;

	public string Meteo;

	public string BusDefaut;

	public string HofDefaut;

	public string Consigne;

	public string Categorie = "cartes";

	public string Statut = "";

	public string Sortie = "";

	public int Ordre;

	public bool ALaUne;

	public List<string> Galerie = new List<string>();

	public Dictionary<string, object> Raw;

	public long Taille;

	public List<string[]> Parties = new List<string[]>();

	public List<string[]> Dlc = new List<string[]>();

	public decimal Prix;

	public string LienAchat = "";

	public string AccesSha = "";

	public string Vip = "";

	public string PublicLe = "";

	public int PrixPoints;

	public List<string> Lies = new List<string>();

	public List<string> Tags = new List<string>();

	public string Editeur = "";

	public string Video = "";

	public string Prerequis = "";

	public string Ruban = "";

	public decimal AncienPrix;

	public bool Commentaires = true;

	public bool Nouveautes;

	public List<string[]> Liens = new List<string[]>();

	public static readonly string[] RubansId = new string[6] { "", "nouveau", "exclusif", "promo", "maj", "populaire" };

	public static readonly string[] RubansNom = new string[6] { "Aucun", "Nouveau", "Exclusif", "Promo", "Mise à jour", "Populaire" };

	public bool APaquet
	{
		get
		{
			if (Zip.Length <= 0)
			{
				return Parties.Count > 0;
			}
			return true;
		}
	}

	public bool Payant => Prix > 0m;

	public string TexteRecherche => (Nom + " " + SousTitre + " " + string.Join(" ", Tags.ToArray())).ToLowerInvariant();

	public DateTime? DatePublic
	{
		get
		{
			if (!DateTime.TryParseExact(PublicLe, new string[2] { "yyyy-MM-dd HH:mm", "yyyy-MM-dd" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
			{
				return null;
			}
			return result;
		}
	}

	public bool RestreintVip
	{
		get
		{
			if (Vip == "test")
			{
				return true;
			}
			if (Vip == "avance")
			{
				DateTime? datePublic = DatePublic;
				if (datePublic.HasValue)
				{
					return datePublic.Value > DateTime.Now;
				}
				return true;
			}
			return false;
		}
	}

	public bool AccesVip
	{
		get
		{
			if (RestreintVip && !Social.Vip)
			{
				return Comptes.EstAdmin;
			}
			return true;
		}
	}

	public string PrixTexte
	{
		get
		{
			if (!(Prix > 0m))
			{
				return "Gratuit";
			}
			return Prix.ToString("0.00", CultureInfo.GetCultureInfo("fr-FR")) + " €";
		}
	}

	public bool Debloque
	{
		get
		{
			if (!Config.Oui("acces_" + Id, defaut: false))
			{
				return Social.Possede("mod:" + Id);
			}
			return true;
		}
	}

	public DateTime? DateSortie
	{
		get
		{
			if (!DateTime.TryParseExact(Sortie, new string[2] { "yyyy-MM-dd HH:mm", "yyyy-MM-dd" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
			{
				return null;
			}
			return result;
		}
	}

	public string StatutEffectif
	{
		get
		{
			string text = (Statut ?? "").ToLowerInvariant();
			switch (text)
			{
			case "masque":
			case "bientot":
				return text;
			case "programme":
			{
				DateTime? dateSortie = DateSortie;
				if (!dateSortie.HasValue || !(dateSortie.Value <= DateTime.Now) || !APaquet)
				{
					return "bientot";
				}
				return "publie";
			}
			default:
				return "publie";
			}
		}
	}

	public bool Installable
	{
		get
		{
			if (StatutEffectif == "publie" && APaquet)
			{
				return AccesVip;
			}
			return false;
		}
	}

	public static string NomRuban(string id)
	{
		int num = Array.IndexOf(RubansId, id ?? "");
		if (num <= 0)
		{
			return "";
		}
		return RubansNom[num];
	}

	public static List<string[]> LireLiens(string t)
	{
		List<string[]> list = new List<string[]>();
		string[] array = (t ?? "").Replace("\r", "").Split('\n');
		foreach (string text in array)
		{
			int num = text.IndexOf('|');
			if (num > 0)
			{
				string text2 = text.Substring(0, num).Trim();
				string uriString = text.Substring(num + 1).Trim();
				if (text2.Length > 0 && Uri.TryCreate(uriString, UriKind.Absolute, out var result) && result.Scheme == Uri.UriSchemeHttps && list.Count < 3)
				{
					list.Add(new string[2]
					{
						(text2.Length > 40) ? text2.Substring(0, 40) : text2,
						result.AbsoluteUri
					});
				}
			}
		}
		return list;
	}

	public static bool ParsePrix(string t, out decimal d)
	{
		d = 0m;
		if (string.IsNullOrEmpty(t))
		{
			return false;
		}
		if (decimal.TryParse(t.Trim().Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out d) && d >= 0.5m)
		{
			return d <= 999m;
		}
		return false;
	}

	public static string Sha256Hex(string t)
	{
		using SHA256 sHA = SHA256.Create();
		return BitConverter.ToString(sHA.ComputeHash(Encoding.UTF8.GetBytes(t))).Replace("-", "").ToLowerInvariant();
	}

	public bool CodeValide(string code)
	{
		if (AccesSha.Length > 0)
		{
			return Sha256Hex((code ?? "").Trim().ToLowerInvariant()) == AccesSha.ToLowerInvariant();
		}
		return false;
	}

	private static string S(Dictionary<string, object> d, string k)
	{
		if (!d.TryGetValue(k, out var value) || value == null)
		{
			return "";
		}
		return value.ToString();
	}

	public static Projet De(Dictionary<string, object> d)
	{
		Projet projet = new Projet();
		projet.Id = S(d, "id");
		projet.Nom = S(d, "nom");
		projet.SousTitre = S(d, "sous_titre");
		projet.Description = S(d, "description");
		projet.Version = S(d, "version");
		projet.Date = S(d, "date");
		projet.Md5 = S(d, "md5");
		projet.Zip = S(d, "zip");
		projet.Sha256 = S(d, "sha256");
		projet.Image = S(d, "image");
		projet.Notes = S(d, "notes");
		projet.Jeu = S(d, "jeu");
		projet.FichierVersion = S(d, "fichier_version");
		projet.Liste = S(d, "liste");
		projet.Marque = S(d, "marque");
		projet.Carte = S(d, "carte");
		projet.Meteo = S(d, "meteo");
		projet.BusDefaut = S(d, "bus_defaut");
		projet.HofDefaut = S(d, "hof_defaut");
		projet.Consigne = S(d, "consigne");
		long.TryParse(S(d, "taille"), out var result);
		projet.Taille = result;
		if (ParsePrix(S(d, "prix"), out var d2))
		{
			projet.Prix = d2;
		}
		projet.LienAchat = S(d, "lien_achat");
		projet.AccesSha = S(d, "acces_sha256");
		projet.Categorie = Categories.Valide(S(d, "categorie"), projet.Jeu);
		projet.Statut = S(d, "statut");
		projet.Sortie = S(d, "sortie");
		projet.Vip = S(d, "vip").ToLowerInvariant();
		projet.PublicLe = S(d, "public_le");
		int.TryParse(S(d, "prix_points"), out var result2);
		projet.PrixPoints = ((result2 > 0 && result2 <= 1000000) ? result2 : 0);
		int.TryParse(S(d, "ordre"), out var result3);
		projet.Ordre = result3;
		projet.ALaUne = S(d, "a_la_une").ToLowerInvariant() == "true";
		projet.Editeur = S(d, "editeur").Trim();
		projet.Prerequis = S(d, "prerequis").Trim();
		projet.Ruban = ((NomRuban(S(d, "ruban")).Length > 0) ? S(d, "ruban") : "");
		projet.Liens = LireLiens(S(d, "liens"));
		string[] array = S(d, "tags").Split(new char[2] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			string text2 = text.Trim().TrimStart('#');
			if (text2.Length > 0 && text2.Length <= 24 && projet.Tags.Count < 8 && !projet.Tags.Contains(text2))
			{
				projet.Tags.Add(text2);
			}
		}
		string[] array2 = S(d, "lies").ToLowerInvariant().Split(new char[3] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text3 in array2)
		{
			if (!projet.Lies.Contains(text3.Trim()) && projet.Lies.Count < 12)
			{
				projet.Lies.Add(text3.Trim());
			}
		}
		string text4 = S(d, "video");
		projet.Video = ((Uri.TryCreate(text4, UriKind.Absolute, out var result4) && result4.Scheme == Uri.UriSchemeHttps) ? text4 : "");
		if (ParsePrix(S(d, "ancien_prix"), out var d3))
		{
			projet.AncienPrix = d3;
		}
		projet.Commentaires = S(d, "commentaires").ToLowerInvariant() != "off";
		projet.Nouveautes = S(d, "nouveautes").ToLowerInvariant() == "true";
		if (d.TryGetValue("galerie", out var value) && value is ArrayList)
		{
			foreach (object item in (ArrayList)value)
			{
				if (item != null && item.ToString().Length > 0 && projet.Galerie.Count < 10)
				{
					projet.Galerie.Add(item.ToString());
				}
			}
		}
		if (d.TryGetValue("parties", out var value2) && value2 is ArrayList)
		{
			foreach (object item2 in (ArrayList)value2)
			{
				if (item2 is Dictionary<string, object> d4)
				{
					string text5 = S(d4, "url");
					string text6 = S(d4, "sha256");
					if (text5.Length > 0 && text6.Length == 64 && projet.Parties.Count < 40)
					{
						projet.Parties.Add(new string[4]
						{
							text5,
							text6,
							S(d4, "taille"),
							S(d4, "miroir")
						});
					}
				}
			}
		}
		string[] array3 = S(d, "dlc").Replace("\r", "").Split('\n');
		foreach (string text7 in array3)
		{
			string[] array4 = text7.Split('|');
			string text8 = array4[0].Trim();
			string text9 = ((array4.Length > 1) ? array4[1].Trim() : "");
			string text10 = ((array4.Length > 2 && array4[2].Trim().ToLowerInvariant().StartsWith("fac")) ? "1" : "");
			if (Regex.IsMatch(text8, "^\\d{3,10}$") && projet.Dlc.Count < 30)
			{
				projet.Dlc.Add(new string[3]
				{
					text8,
					(text9.Length > 0) ? text9 : ("DLC " + text8),
					text10
				});
			}
		}
		projet.Raw = d;
		return projet;
	}
}
