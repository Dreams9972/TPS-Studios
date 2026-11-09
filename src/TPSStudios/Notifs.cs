using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace TPSStudios;

internal static class Notifs
{
	public static List<Notif> Liste = new List<Notif>();

	private static bool charge;

	public static readonly string[] Types = new string[5] { "lanceur", "maj", "sortie", "nouveau", "vip" };

	public static readonly string[] TypesNoms = new string[5] { "Mises à jour du lanceur", "Mises à jour de mes mods", "Sorties des mods suivis", "Nouveaux mods TPS Studios", "Accès anticipé VIP" };

	public static readonly string[] TypesAide = new string[5] { "Une nouvelle version du lanceur est prête.", "Un mod installé a une nouvelle version.", "Un mod suivi devient disponible (et rappel 24 h avant une sortie datée).", "Un nouveau contenu apparaît dans le catalogue.", "Un mod en accès anticipé est disponible pour les VIP." };

	private static string Fichier => Path.Combine(Config.Dossier, "notifications.json");

	public static List<Notif> Toutes
	{
		get
		{
			Charger();
			return Liste;
		}
	}

	public static int NonLues
	{
		get
		{
			Charger();
			int num = 0;
			foreach (Notif item in Liste)
			{
				if (!item.Lu)
				{
					num++;
				}
			}
			return num;
		}
	}

	public static event Action<Notif> Nouvelle;

	private static void Charger()
	{
		if (charge)
		{
			return;
		}
		charge = true;
		try
		{
			if (File.Exists(Fichier))
			{
				List<Notif> list = new JavaScriptSerializer().Deserialize<List<Notif>>(File.ReadAllText(Fichier, Encoding.UTF8));
				if (list != null)
				{
					Liste = list;
				}
			}
		}
		catch
		{
		}
	}

	private static void Sauver()
	{
		try
		{
			Disque.EcrireTexte(Fichier, new JavaScriptSerializer().Serialize(Liste));
		}
		catch (Exception e)
		{
			Journal.Une("notifications", e);
		}
	}

	public static bool TypeActif(string t)
	{
		return Config.Oui("notif_" + t, defaut: true);
	}

	public static bool Suivi(Projet p, string omsi)
	{
		string text = Config.Lire("suivre_" + p.Id);
		if (text == "1")
		{
			return true;
		}
		if (text == "0")
		{
			return false;
		}
		return Omsi.Installee(p, omsi) != null;
	}

	public static void Suivre(string id, bool on)
	{
		Config.Ecrire("suivre_" + id, on ? "1" : "0");
	}

	public static void ToutLu()
	{
		Charger();
		foreach (Notif item in Liste)
		{
			item.Lu = true;
		}
		Sauver();
	}

	public static void Vider()
	{
		Liste.Clear();
		Sauver();
	}

	public static void Ajouter(string cle, string type, string titre, string texte, string mod)
	{
		Charger();
		foreach (Notif item in Liste)
		{
			if (item.Cle == cle)
			{
				return;
			}
		}
		Notif notif = new Notif();
		notif.Cle = cle;
		notif.Type = type;
		notif.Titre = titre;
		notif.Texte = texte;
		notif.Mod = mod ?? "";
		notif.Date = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
		Liste.Insert(0, notif);
		while (Liste.Count > 60)
		{
			Liste.RemoveAt(Liste.Count - 1);
		}
		Sauver();
		if (Notifs.Nouvelle != null)
		{
			try
			{
				Notifs.Nouvelle(notif);
			}
			catch
			{
			}
		}
	}

	public static void Verifier(Catalogue cat, string omsi, string lanceurDispo)
	{
		if (cat == null)
		{
			return;
		}
		Charger();
		bool flag = false;
		for (int num = Liste.Count - 1; num >= 0; num--)
		{
			Notif notif = Liste[num];
			bool flag2 = false;
			if (notif.Type == "lanceur")
			{
				string a = (notif.Cle.StartsWith("lanceur:") ? notif.Cle.Substring(8) : "");
				flag2 = !Fenetre.PlusRecent(a, "2.7.9");
			}
			else if (notif.Type == "maj")
			{
				string[] q = notif.Cle.Split(':');
				Projet projet = ((q.Length > 1) ? cat.Projets.Find((Projet z) => z.Id == q[1]) : null);
				if (projet == null)
				{
					flag2 = true;
				}
				else
				{
					string[] array = Omsi.Installee(projet, omsi);
					flag2 = array == null || !Omsi.Perimee(projet, array) || (q.Length > 2 && q[2] != projet.Version);
				}
			}
			if (flag2)
			{
				Liste.RemoveAt(num);
				flag = true;
			}
		}
		if (flag)
		{
			Sauver();
		}
		if (lanceurDispo != null && TypeActif("lanceur"))
		{
			Ajouter("lanceur:" + lanceurDispo, "lanceur", "Lanceur v" + lanceurDispo + " disponible", "Il s'installe au prochain démarrage (ou Paramètres › Mises à jour).", "");
		}
		string text = Config.Lire("notif_connus") ?? "";
		bool flag3 = text.Length == 0;
		HashSet<string> hashSet = new HashSet<string>(text.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries));
		foreach (Projet projet2 in cat.Projets)
		{
			bool flag4 = Suivi(projet2, omsi);
			bool flag5 = Config.Lire("suivre_" + projet2.Id) == "0";
			string[] array2 = Omsi.Installee(projet2, omsi);
			if (!hashSet.Contains(projet2.Id))
			{
				if (!flag3 && TypeActif("nouveau") && projet2.StatutEffectif != "masque")
				{
					Ajouter("nouveau:" + projet2.Id, "nouveau", "Nouveau : " + projet2.Nom, (projet2.SousTitre ?? "") + (projet2.Installable ? "" : "  ·  bientôt disponible"), projet2.Id);
				}
				hashSet.Add(projet2.Id);
			}
			if (array2 != null && !flag5 && TypeActif("maj") && Omsi.Perimee(projet2, array2))
			{
				Ajouter("maj:" + projet2.Id + ":" + projet2.Version, "maj", "Mise à jour : " + projet2.Nom + " " + projet2.Version, "La nouvelle version est prête à installer.", projet2.Id);
			}
			string text2 = Config.Lire("etat_" + projet2.Id) ?? "";
			string text3 = (projet2.Installable ? "publie" : projet2.StatutEffectif);
			if (flag4 && TypeActif("sortie") && text2 == "bientot" && text3 == "publie")
			{
				Ajouter("sortie:" + projet2.Id + ":" + projet2.Version, "sortie", projet2.Nom + " est disponible !", "Le contenu que vous suivez peut être installé.", projet2.Id);
			}
			if (flag4 && TypeActif("sortie") && projet2.StatutEffectif == "bientot")
			{
				DateTime? dateSortie = projet2.DateSortie;
				if (dateSortie.HasValue && dateSortie.Value > DateTime.Now && (dateSortie.Value - DateTime.Now).TotalHours <= 24.0)
				{
					Ajouter("rappel:" + projet2.Id + ":" + dateSortie.Value.ToString("yyyyMMddHHmm"), "sortie", projet2.Nom + " sort bientôt", "Disponible le " + dateSortie.Value.ToString("dd/MM à HH:mm") + ".", projet2.Id);
				}
			}
			if (projet2.Vip == "avance" && projet2.RestreintVip && Social.Vip && TypeActif("vip"))
			{
				Ajouter("vip:" + projet2.Id + ":" + projet2.Version, "vip", "Accès anticipé VIP : " + projet2.Nom, "Installez-le avant tout le monde.", projet2.Id);
			}
			if (text2 != text3)
			{
				Config.Ecrire("etat_" + projet2.Id, text3);
			}
		}
		string text4 = string.Join(",", hashSet);
		if (text4 != text)
		{
			Config.Ecrire("notif_connus", text4);
		}
	}
}
