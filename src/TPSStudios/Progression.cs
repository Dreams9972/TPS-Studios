using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace TPSStudios;

internal static class Progression
{
	public const int NivMax = 100;

	public static readonly string[] Paliers = new string[10] { "Passager", "Voyageur", "Conducteur", "Pilote", "Chef de ligne", "Capitaine", "Commandant", "Maître de ligne", "Légende", "Légende TPS" };

	private static bool enVerif;

	public static readonly List<DefSucces> Defs = Base();

	private static readonly Dictionary<string, DefSucces> complets = new Dictionary<string, DefSucces>();

	public static event Action<string, string, Color> Notification;

	public static int Cout(int n)
	{
		return 60 + 12 * n;
	}

	public static void Niveau(int xp, out int niv, out int dans, out int besoin)
	{
		niv = 1;
		int num = Math.Max(0, xp);
		while (niv < 100 && num >= Cout(niv))
		{
			num -= Cout(niv);
			niv++;
		}
		besoin = Cout(Math.Min(niv, 99));
		dans = ((niv >= 100) ? besoin : num);
	}

	public static int NiveauDe(Compte c)
	{
		Niveau(c?.Xp ?? 0, out var niv, out var _, out var _);
		return niv;
	}

	public static int Palier(int niv)
	{
		return Math.Max(0, Math.Min(9, (niv - 1) / 10));
	}

	public static string Titre(int niv)
	{
		return Paliers[Palier(niv)];
	}

	private static void Avis(string t, string d, Color c)
	{
		if (Progression.Notification != null)
		{
			try
			{
				Progression.Notification(t, d, c);
			}
			catch
			{
			}
		}
	}

	public static int S(Compte c, string cle)
	{
		if (c == null || !c.Stats.TryGetValue(cle, out var value))
		{
			return 0;
		}
		return value;
	}

	public static void Compter(Compte c, string cle, int delta)
	{
		if (c != null)
		{
			c.Stats.TryGetValue(cle, out var value);
			c.Stats[cle] = value + delta;
		}
	}

	private static int Lancements(Compte c)
	{
		int num = 0;
		foreach (int value in c.Lancements.Values)
		{
			num += value;
		}
		return num;
	}

	public static void Init(Compte c)
	{
		if (c != null && c.Xp == 0 && c.Lancements.Count > 0)
		{
			c.Xp = Lancements(c) * 100;
		}
	}

	public static void Gagner(Compte c, int xp, string raison)
	{
		if (c != null && xp > 0)
		{
			Init(c);
			int num = NiveauDe(c);
			c.Xp += xp;
			int num2 = NiveauDe(c);
			if (num2 > num)
			{
				Avis("Niveau " + num2 + " atteint !", ((Palier(num2) > Palier(num)) ? ("Nouveau badge : " + Titre(num2) + "  ·  ") : (Titre(num2) + "  ·  ")) + "+" + xp + " XP (" + raison + ")", C.Orange);
			}
			else
			{
				Avis("+" + xp + " XP", raison, C.Jade);
			}
			Verifier(c);
			Comptes.Sauvegarder();
		}
	}

	public static void Connexion(Compte c)
	{
		if (c == null)
		{
			return;
		}
		Init(c);
		string text = DateTime.Now.ToString("yyyy-MM-dd");
		string value = DateTime.Now.ToString("yyyyMMdd");
		List<string> list = new List<string>();
		foreach (string key in c.Stats.Keys)
		{
			if ((key.StartsWith("xp_lancer_") || key.StartsWith("xp_jeu_") || key.StartsWith("xp_comm_") || key.StartsWith("jlj_") || key.StartsWith("secj_") || (key.StartsWith("g_") && key.Contains("_jour_"))) && !key.EndsWith(value))
			{
				list.Add(key);
			}
		}
		foreach (string item in list)
		{
			c.Stats.Remove(item);
		}
		if (c.DernierJour == text)
		{
			Verifier(c);
			return;
		}
		DateTime result;
		bool flag = DateTime.TryParseExact(c.DernierJour, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result) && result.Date == DateTime.Now.Date.AddDays(-1.0);
		c.Serie = ((!flag) ? 1 : (c.Serie + 1));
		c.SerieMax = Math.Max(c.SerieMax, c.Serie);
		c.DernierJour = text;
		Compter(c, "jours", 1);
		Gagner(c, 40 + Math.Min(c.Serie, 30) * 5, "connexion du jour · série de " + c.Serie + " jour" + ((c.Serie > 1) ? "s" : ""));
	}

	public static void Lancement(Compte c, string projetId, string jeu)
	{
		if (c != null)
		{
			string text = DateTime.Now.ToString("yyyyMMdd");
			string key = "xp_lancer_" + projetId + "_" + text;
			string text2 = "g_" + (string.IsNullOrEmpty(jeu) ? "omsi2" : jeu) + "_";
			Compter(c, "lancements", 1);
			if (!c.Stats.ContainsKey("joue_" + projetId))
			{
				c.Stats["joue_" + projetId] = 1;
				Compter(c, text2 + "contenus", 1);
			}
			if (!c.Stats.ContainsKey("jlj_" + text))
			{
				c.Stats["jlj_" + text] = 1;
				Compter(c, "lanc_jours", 1);
			}
			if (!c.Stats.ContainsKey("g_mod_" + projetId + "_jour_" + text))
			{
				c.Stats["g_mod_" + projetId + "_jour_" + text] = 1;
				Compter(c, "mod_" + projetId + "_jours", 1);
			}
			if (!c.Stats.ContainsKey(text2 + "jour_" + text))
			{
				c.Stats[text2 + "jour_" + text] = 1;
				Compter(c, text2 + "jours", 1);
			}
			if (c.Stats.ContainsKey(key))
			{
				Verifier(c);
				Comptes.Sauvegarder();
			}
			else
			{
				c.Stats[key] = 1;
				Gagner(c, 30, "lancement de la carte");
			}
		}
	}

	public static void Jeu(Compte c, int secondes, string jeu)
	{
		if (c == null)
		{
			return;
		}
		int num = S(c, "secondes");
		Compter(c, "secondes", secondes);
		string text = "secj_" + DateTime.Now.ToString("yyyyMMdd");
		int num2 = S(c, text);
		if (num2 < 14400)
		{
			int num3 = Math.Min(secondes, 14400 - num2);
			c.Stats[text] = num2 + num3;
			Compter(c, "sec_compte", num3);
			Compter(c, "g_" + (string.IsNullOrEmpty(jeu) ? "omsi2" : jeu) + "_sec", num3);
		}
		if (num / 600 != (num + secondes) / 600)
		{
			string text2 = "xp_jeu_" + DateTime.Now.ToString("yyyyMMdd");
			int num4 = S(c, text2);
			if (num4 < 6)
			{
				c.Stats[text2] = num4 + 1;
				Gagner(c, 15, "10 minutes de jeu");
			}
			else
			{
				Verifier(c);
				Comptes.Sauvegarder();
			}
		}
	}

	public static void Installation(Compte c, string id, string jeu, string categorie)
	{
		if (c != null)
		{
			Compter(c, "installs", 1);
			if (c.Stats.ContainsKey("inst_" + id))
			{
				Verifier(c);
				Comptes.Sauvegarder();
				return;
			}
			c.Stats["inst_" + id] = 1;
			Compter(c, "inst_uniques", 1);
			string text = "g_" + (string.IsNullOrEmpty(jeu) ? "omsi2" : jeu) + "_";
			Compter(c, text + "inst", 1);
			Compter(c, text + "cat_" + categorie, 1);
			Gagner(c, 40, "installation d'un nouveau mod");
		}
	}

	public static void LireStatsJeu(Compte c, string omsi)
	{
		if (c == null || omsi == null)
		{
			return;
		}
		string path = Path.Combine(omsi, "TPS_stats");
		if (!Directory.Exists(path))
		{
			return;
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		try
		{
			string[] files = Directory.GetFiles(path, "*.json");
			foreach (string text in files)
			{
				if (new FileInfo(text).Length > 65536)
				{
					continue;
				}
				Dictionary<string, object> dictionary2 = null;
				try
				{
					dictionary2 = new JavaScriptSerializer().DeserializeObject(File.ReadAllText(text, Encoding.UTF8)) as Dictionary<string, object>;
				}
				catch
				{
				}
				if (dictionary2 == null)
				{
					continue;
				}
				foreach (KeyValuePair<string, object> item in dictionary2)
				{
					if (item.Key.Length > 0 && item.Key.Length < 24 && int.TryParse(Convert.ToString(item.Value), out var result) && result >= 0 && result < 10000000)
					{
						dictionary.TryGetValue(item.Key, out var value);
						dictionary[item.Key] = value + result;
					}
				}
			}
		}
		catch
		{
		}
		bool flag = false;
		foreach (KeyValuePair<string, int> item2 in dictionary)
		{
			c.Stats.TryGetValue("jeu_" + item2.Key, out var value2);
			if (item2.Value > value2)
			{
				c.Stats["jeu_" + item2.Key] = item2.Value;
				flag = true;
			}
		}
		if (flag)
		{
			Verifier(c);
			Comptes.Sauvegarder();
		}
	}

	public static void Verifier(Compte c)
	{
		if (c == null || enVerif)
		{
			return;
		}
		enVerif = true;
		try
		{
			bool flag = true;
			while (flag)
			{
				flag = false;
				foreach (DefSucces def in Defs)
				{
					if (!c.Succes.ContainsKey(def.Id) && (def.Prec == null || c.Succes.ContainsKey(def.Prec)) && def.Valeur(c) >= def.Objectif)
					{
						c.Succes[def.Id] = DateTime.Now.ToString("dd/MM/yyyy");
						flag = true;
						int num = NiveauDe(c);
						c.Xp += def.Xp;
						int num2 = NiveauDe(c);
						Avis("Succès débloqué : " + def.Nom, def.Desc + ((def.Xp > 0) ? ("  ·  +" + def.Xp + " XP") : ""), C.Vert);
						if (num2 > num)
						{
							Avis("Niveau " + num2 + " atteint !", Titre(num2), C.Orange);
						}
					}
				}
			}
		}
		finally
		{
			enVerif = false;
		}
	}

	private static DefSucces D(string id, string nom, string desc, string glyphe, int xp, int obj, Func<Compte, int> val)
	{
		DefSucces defSucces = new DefSucces();
		defSucces.Id = id;
		defSucces.Nom = nom;
		defSucces.Desc = desc;
		defSucces.Glyphe = glyphe;
		defSucces.Xp = xp;
		defSucces.Objectif = obj;
		defSucces.Valeur = val;
		return defSucces;
	}

	private static List<DefSucces> P(string famille, string[] ids, string[] noms, string desc, string glyphe, int[] objectifs, int[] xps, Func<Compte, int> val)
	{
		List<DefSucces> list = new List<DefSucces>();
		for (int i = 0; i < ids.Length; i++)
		{
			DefSucces defSucces = D(ids[i], noms[i], string.Format(Lang.T(desc), objectifs[i], (objectifs[i] > 1) ? "s" : ""), glyphe, xps[i], objectifs[i], val);
			defSucces.Famille = famille;
			defSucces.Rang = i + 1;
			defSucces.NbRangs = ids.Length;
			defSucces.Prec = ((i > 0) ? ids[i - 1] : null);
			list.Add(defSucces);
		}
		return list;
	}

	private static List<DefSucces> Base()
	{
		List<DefSucces> list = new List<DefSucces>();
		list.Add(D("profil", "Premier pas", "Créer son profil", "\ue77b", 50, 1, (Compte c) => 1));
		list.Add(D("photo", "Une bonne bouille", "Choisir une photo de profil", "\ue722", 30, 1, (Compte c) => (Comptes.ImageAvatar(c) != null) ? 1 : 0));
		list.Add(D("style", "Style personnel", "Personnaliser sa bannière", "\ue790", 30, 1, (Compte c) => (!string.IsNullOrEmpty(c.BanType)) ? 1 : 0));
		list.Add(D("anime", "En mouvement", "Choisir une bannière animée", "\ue790", 60, 1, (Compte c) => (c.BanType == "anim") ? 1 : 0));
		list.Add(D("fete", "Esprit de fête", "Choisir une bannière des fêtes de l'année", "\ue7c1", 60, 1, (Compte c) => (c.BanType == "anim" && Array.IndexOf(BanAnim.Ids, c.BanMotif) >= 10) ? 1 : 0));
		list.Add(D("bio", "Présentation", "Écrire une bio d'au moins 10 caractères", "\ue8bd", 30, 1, (Compte c) => ((c.Bio ?? "").Trim().Length >= 10) ? 1 : 0));
		list.Add(D("discord", "Membre de la communauté", "Ouvrir l'invitation du Discord TPS Groupe", "\ue8f2", 30, 1, (Compte c) => S(c, "discord")));
		List<DefSucces> list2 = list;
		list2.AddRange(P("lanc", new string[5] { "l1", "l10", "l50", "l200", "l500" }, new string[5] { "Premier voyage", "Habitué", "Voyageur régulier", "Chauffeur de légende", "Pilier de la ligne" }, "Jouer {0} jour{1} à une carte lancée depuis le lanceur", "\ue768", new int[5] { 1, 10, 50, 200, 500 }, new int[5] { 50, 120, 300, 900, 2000 }, (Compte c) => S(c, "lanc_jours")));
		list2.AddRange(P("cont", new string[4] { "c2", "c5", "c10", "c20" }, new string[4] { "Curieux", "Explorateur", "Globe-trotteur", "Encyclopédie vivante" }, "Jouer à {0} mods différents", "\ue909", new int[4] { 2, 5, 10, 20 }, new int[4] { 80, 200, 500, 1200 }, (Compte c) => c.Lancements.Count));
		list2.AddRange(P("serie", new string[5] { "s3", "s7", "s30", "s100", "s365" }, new string[5] { "Fidèle", "Assidu", "Inarrêtable", "Gardien du lanceur", "Une année entière" }, "Ouvrir le lanceur {0} jours de suite", "\ue787", new int[5] { 3, 7, 30, 100, 365 }, new int[5] { 60, 150, 600, 2000, 5000 }, (Compte c) => c.SerieMax));
		list2.AddRange(P("jours", new string[4] { "j7", "j30", "j100", "j365" }, new string[4] { "De passage", "Habitué du lanceur", "Fidèle au poste", "Résident TPS" }, "Ouvrir le lanceur {0} jours au total", "\ue163", new int[4] { 7, 30, 100, 365 }, new int[4] { 60, 200, 700, 2000 }, (Compte c) => S(c, "jours")));
		list2.AddRange(P("temps", new string[4] { "m60", "m600", "m3000", "m6000" }, new string[4] { "Une heure de route", "Dix heures au volant", "Cinquante heures", "Cent heures de service" }, "Jouer {0} minutes depuis le lanceur", "\ue916", new int[4] { 60, 600, 3000, 6000 }, new int[4] { 100, 300, 900, 2000 }, (Compte c) => S(c, "sec_compte") / 60));
		list2.AddRange(P("inst", new string[4] { "i1", "i3", "i10", "i25" }, new string[4] { "Première installation", "Collectionneur", "Dépôt complet", "Grand dépôt" }, "Installer {0} mod{1} différent{1}", "\ue896", new int[4] { 1, 3, 10, 25 }, new int[4] { 40, 120, 400, 1000 }, (Compte c) => S(c, "inst_uniques")));
		list2.AddRange(P("traj", new string[4] { "t5", "t25", "t100", "t500" }, new string[4] { "Cinq trajets", "Navetteur", "Centenaire de la ligne", "Réseau entier" }, "Faire {0} trajets en jeu", "\ue806", new int[4] { 5, 25, 100, 500 }, new int[4] { 100, 300, 1000, 3000 }, (Compte c) => S(c, "jeu_trajets")));
		list2.AddRange(P("like", new string[5] { "v1", "v3", "v5", "v10", "v25" }, new string[5] { "Première voix", "Bon goût", "Critique", "Connaisseur", "Jury TPS" }, "Aimer {0} mod{1} différent{1}", "\ueb51", new int[5] { 1, 3, 5, 10, 25 }, new int[5] { 50, 100, 200, 400, 1000 }, (Compte c) => S(c, "votes")));
		list2.AddRange(P("comm", new string[5] { "k1", "k5", "k10", "k25", "k50" }, new string[5] { "Premier commentaire", "Bavard", "Plume de la communauté", "Chroniqueur", "Voix de la communauté" }, "Publier {0} commentaire{1} (3 comptés par jour)", "\ue8bd", new int[5] { 1, 5, 10, 25, 50 }, new int[5] { 50, 120, 250, 600, 1200 }, (Compte c) => S(c, "comm_comptes")));
		list2.AddRange(P("amis", new string[3] { "a1", "a5", "a10" }, new string[3] { "Premier ami", "Bande de copains", "Réseau d'amis" }, "Avoir {0} ami{1} TPS Groupe", "\ue716", new int[3] { 1, 5, 10 }, new int[3] { 50, 150, 400 }, (Compte c) => Math.Max(S(c, "amis_max"), Social.Amis.Count)));
		list2.AddRange(P("niv", new string[5] { "n10", "n25", "n50", "n75", "n100" }, new string[5] { "Voyageur", "Conducteur confirmé", "Capitaine", "Maître de ligne", "Légende TPS" }, "Atteindre le niveau {0}", "\ue735", new int[5] { 10, 25, 50, 75, 100 }, new int[5] { 100, 300, 800, 1400, 2000 }, (Compte c) => NiveauDe(c)));
		return list2;
	}

	public static void EnregistrerJeu(string id, string nom, int contenus)
	{
		if (string.IsNullOrEmpty(id))
		{
			return;
		}
		string g = "g_" + id + "_";
		if (complets.TryGetValue(id, out var value))
		{
			value.Objectif = Math.Max(1, contenus);
			value.Desc = "Jouer à chacun des " + Math.Max(1, contenus) + " mods " + nom;
			return;
		}
		Defs.Add(D(g + "depart", nom + " : premier départ", "Lancer " + nom + " depuis le lanceur", "\ue768", 60, 1, (Compte c) => (S(c, g + "jours") > 0) ? 1 : 0));
		Defs.AddRange(P(g + "temps", new string[3]
		{
			g + "h1",
			g + "h10",
			g + "h50"
		}, new string[3]
		{
			nom + " : rodage",
			nom + " : titulaire",
			nom + " : vétéran"
		}, "Jouer {0} heure{1} à " + nom, "\ue916", new int[3] { 1, 10, 50 }, new int[3] { 80, 300, 1000 }, (Compte c) => S(c, g + "sec") / 3600));
		Defs.AddRange(P(g + "inst", new string[3]
		{
			g + "i1",
			g + "i3",
			g + "i10"
		}, new string[3]
		{
			nom + " : première carte",
			nom + " : réseau en construction",
			nom + " : réseau complet"
		}, "Installer {0} mod{1} " + nom + " différent{1}", "\ue896", new int[3] { 1, 3, 10 }, new int[3] { 50, 150, 500 }, (Compte c) => S(c, g + "inst")));
		Defs.AddRange(P(g + "jours", new string[3]
		{
			g + "j5",
			g + "j30",
			g + "j100"
		}, new string[3]
		{
			nom + " : habitué",
			nom + " : passionné",
			nom + " : inconditionnel"
		}, "Jouer à " + nom + " {0} jour{1} différent{1}", "\ue787", new int[3] { 5, 30, 100 }, new int[3] { 100, 400, 1200 }, (Compte c) => S(c, g + "jours")));
		value = D(g + "tout", nom + " : complétionniste", "Jouer à chacun des " + Math.Max(1, contenus) + " mods " + nom, "\ue73e", 800, Math.Max(1, contenus), (Compte c) => S(c, g + "contenus"));
		Defs.Add(value);
		complets[id] = value;
		foreach (DefSucces dj in Defs)
		{
			if (dj.Id.StartsWith(g))
			{
				dj.Groupe = id;
			}
		}
		if (id == "omsi2")
		{
			Defs.Add(D(g + "bus", "OMSI 2 : nouveau véhicule", "Installer un bus OMSI 2", "\ue806", 80, 1, (Compte c) => S(c, g + "cat_bus")));
			Defs.Add(D(g + "livree", "OMSI 2 : coup de peinture", "Installer une livrée OMSI 2", "\ue790", 60, 1, (Compte c) => S(c, g + "cat_livrees")));
			Defs.Add(D(g + "girouette", "OMSI 2 : bonne destination", "Installer des girouettes OMSI 2", "\ue7c1", 60, 1, (Compte c) => S(c, g + "cat_girouettes")));
			Defs.Add(D(g + "ia", "OMSI 2 : trafic vivant", "Installer un pack IA OMSI 2", "\ue716", 60, 1, (Compte c) => S(c, g + "cat_packs_ia")));
			foreach (DefSucces dj2 in Defs)
			{
				if (dj2.Id.StartsWith(g))
				{
					dj2.Groupe = id;
				}
			}
		}
	}

	private static readonly HashSet<string> modsEnregistres = new HashSet<string>();

	/// <summary>
	/// Succès propres à un mod, créés dès qu'il apparaît dans le catalogue. Carte : l'installer, faire
	/// son premier service, y jouer 5 jours. Autres mods : l'installer.
	/// </summary>
	public static void EnregistrerMod(string id, string nom, string categorie)
	{
		if (string.IsNullOrEmpty(id) || !modsEnregistres.Add(id))
		{
			return;
		}
		string fam = "mod_" + id;
		if (categorie == "cartes")
		{
			DefSucces a = D("mod_" + id + "_inst", nom + " : bienvenue", "Installer la carte " + nom, "\ue896", 40, 1, (Compte c) => S(c, "inst_" + id));
			DefSucces b = D("mod_" + id + "_joue", nom + " : premier service", "Lancer une partie sur " + nom, "\ue768", 60, 1, (Compte c) => S(c, "joue_" + id));
			DefSucces d = D("mod_" + id + "_j5", nom + " : habitué de la ligne", "Jouer sur " + nom + " 5 jours différents", "\ue787", 150, 5, (Compte c) => S(c, "mod_" + id + "_jours"));
			DefSucces[] rangs = new DefSucces[3] { a, b, d };
			for (int i = 0; i < rangs.Length; i++)
			{
				rangs[i].Famille = fam;
				rangs[i].Rang = i + 1;
				rangs[i].NbRangs = rangs.Length;
				rangs[i].Prec = ((i > 0) ? rangs[i - 1].Id : null);
				rangs[i].Groupe = "mods";
				Defs.Add(rangs[i]);
			}
			return;
		}
		string glyphe;
		string verbe;
		switch (categorie)
		{
		case "bus":
			glyphe = "\ue806";
			verbe = "Installer le bus ";
			break;
		case "livrees":
			glyphe = "\ue790";
			verbe = "Installer la livrée ";
			break;
		case "girouettes":
			glyphe = "\ue7c1";
			verbe = "Installer les girouettes ";
			break;
		case "packs_ia":
			glyphe = "\ue716";
			verbe = "Installer le pack IA ";
			break;
		default:
			glyphe = "\ue896";
			verbe = "Installer ";
			break;
		}
		DefSucces e = D("mod_" + id + "_inst", nom + " : installé", verbe + nom, glyphe, 40, 1, (Compte c) => S(c, "inst_" + id));
		e.Groupe = "mods";
		Defs.Add(e);
	}

	public static List<DefSucces> Vitrine(Compte c)
	{
		List<DefSucces> list = new List<DefSucces>();
		HashSet<string> hashSet = new HashSet<string>();
		foreach (DefSucces def in Defs)
		{
			if (string.IsNullOrEmpty(def.Famille))
			{
				list.Add(def);
			}
			else
			{
				if (hashSet.Contains(def.Famille))
				{
					continue;
				}
				DefSucces item = def;
				foreach (DefSucces def2 in Defs)
				{
					if (def2.Famille == def.Famille)
					{
						item = def2;
						if (!c.Succes.ContainsKey(def2.Id))
						{
							break;
						}
					}
				}
				hashSet.Add(def.Famille);
				list.Add(item);
			}
		}
		return list;
	}
}
