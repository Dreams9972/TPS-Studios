using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TPSStudios;

/// <summary>
/// Mode administrateur : publie le paquet d'un mod (dossier → zips → GitHub Releases
/// → catalogue). Rien n'est gardé sur le PC : chaque partie est effacée dès qu'elle
/// est envoyée et vérifiée.
/// </summary>
internal class FenPaquet : Form
{
	private readonly string id;

	private readonly string nom;

	private Champ tDossier;

	private Champ tVersion;

	private Champ tNotes;

	private Option cSpeciale;

	private Champ tAvert;

	/// <summary>Version spéciale (ex. « V1 Alpha ») : publiée à côté de la version normale, au choix du joueur.</summary>
	private bool speciale;

	private string avertissement = "";

	private Label lResume;

	private Label lEtat;

	private Barre barre;

	private Bouton bPublier;

	private Bouton bAnnuler;

	private Bouton bParcourir;

	private List<Paquet.Fichier> fichiers;

	private string structure;

	private volatile bool annule;

	private bool enCours;

	public FenPaquet(string id, string nom, string version, string notes)
	{
		this.id = id;
		this.nom = nom;
		Text = "Publier le paquet de " + nom;
		base.ClientSize = new Size(Zx.P(700), Zx.P(770));
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterParent;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		try
		{
			base.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		}
		catch
		{
		}
		Theme.Fenetre(this);
		Lb("Publier le paquet de " + nom, Zx.P(28), Zx.P(20), Zx.P(640), Zx.P(34), new Font("Segoe UI Semibold", Zx.P(16f)), C.Texte);
		Lb("Le dossier est découpé en parties de 250 Mo, envoyées sur GitHub (gratuit) puis effacées de ce PC une par une. Le catalogue est mis à jour à la fin : les joueurs verront « METTRE À JOUR ».", Zx.P(30), Zx.P(58), Zx.P(640), Zx.P(48), new Font("Segoe UI", Zx.P(9.5f)), C.Gris);
		Lb("Dossier du mod", Zx.P(30), Zx.P(118), Zx.P(640), Zx.P(22), new Font("Segoe UI Semibold", Zx.P(10.5f)), C.Texte);
		tDossier = new Champ(multi: false, Zx.P(38));
		tDossier.Location = new Point(Zx.P(28), Zx.P(144));
		tDossier.Width = Zx.P(500);
		tDossier.ReadOnly = true;
		tDossier.Indice = "Contient « jeu\\ » (et « plugins\\ », « fonts\\ ») ou directement maps\\, Vehicles\\…";
		base.Controls.Add(tDossier);
		bParcourir = new Bouton("Parcourir…");
		bParcourir.Plein = false;
		bParcourir.Size = new Size(Zx.P(136), Zx.P(38));
		bParcourir.Location = new Point(Zx.P(536), Zx.P(144));
		bParcourir.Click += delegate
		{
			Choisir();
		};
		base.Controls.Add(bParcourir);
		lResume = Lb("", Zx.P(30), Zx.P(188), Zx.P(640), Zx.P(44), new Font("Segoe UI", Zx.P(9.5f)), C.Gris);
		Lb("Nouvelle version", Zx.P(30), Zx.P(238), Zx.P(640), Zx.P(22), new Font("Segoe UI Semibold", Zx.P(10.5f)), C.Texte);
		tVersion = new Champ(multi: false, Zx.P(38));
		tVersion.Location = new Point(Zx.P(28), Zx.P(264));
		tVersion.Width = Zx.P(240);
		tVersion.Text = version;
		tVersion.Indice = "ex. V1.2";
		base.Controls.Add(tVersion);
		Lb((version.Length > 0) ? ("Version actuelle : " + version) : "Premier paquet de cette page", Zx.P(284), Zx.P(272), Zx.P(380), Zx.P(22), new Font("Segoe UI", Zx.P(9.5f)), C.Gris);
		cSpeciale = new Option("Version spéciale", "Publiée à côté de la version normale : le joueur choisit laquelle jouer (ex. « V1 Alpha »).", val: false);
		cSpeciale.Location = new Point(Zx.P(18), Zx.P(312));
		cSpeciale.Width = Zx.P(664);
		base.Controls.Add(cSpeciale);
		tAvert = new Champ(multi: false, Zx.P(38));
		tAvert.Location = new Point(Zx.P(28), Zx.P(380));
		tAvert.Width = Zx.P(644);
		tAvert.MaxLength = 200;
		tAvert.Indice = "Avertissement montré au moment du choix (ex. version non maintenue, bugs possibles)";
		tAvert.Enabled = false;
		base.Controls.Add(tAvert);
		cSpeciale.Changed += delegate
		{
			tAvert.Enabled = cSpeciale.Checked;
		};
		Lb("Notes de version (affichées aux joueurs)", Zx.P(30), Zx.P(432), Zx.P(640), Zx.P(22), new Font("Segoe UI Semibold", Zx.P(10.5f)), C.Texte);
		tNotes = new Champ(multi: true, Zx.P(130));
		tNotes.Location = new Point(Zx.P(28), Zx.P(458));
		tNotes.Width = Zx.P(644);
		tNotes.Text = notes.Replace("\r\n", "\n").Replace("\n", "\r\n");
		base.Controls.Add(tNotes);
		barre = new Barre();
		barre.Location = new Point(Zx.P(28), Zx.P(606));
		barre.Size = new Size(Zx.P(644), Zx.P(8));
		barre.Visible = false;
		base.Controls.Add(barre);
		lEtat = Lb("", Zx.P(30), Zx.P(622), Zx.P(640), Zx.P(54), new Font("Segoe UI", Zx.P(9.5f)), C.Gris);
		Panel panel = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = Zx.P(70),
			BackColor = C.Cote
		};
		base.Controls.Add(panel);
		bAnnuler = new Bouton("Annuler");
		bAnnuler.Plein = false;
		bAnnuler.Size = new Size(Zx.P(120), Zx.P(38));
		bAnnuler.Location = new Point(Zx.P(28), Zx.P(16));
		bAnnuler.Click += delegate
		{
			Close();
		};
		bPublier = new Bouton("Publier");
		bPublier.Size = new Size(Zx.P(160), Zx.P(38));
		bPublier.Location = new Point(base.ClientSize.Width - Zx.P(28) - Zx.P(160), Zx.P(16));
		bPublier.Enabled = false;
		bPublier.Click += delegate
		{
			Lancer();
		};
		panel.Controls.Add(bAnnuler);
		panel.Controls.Add(bPublier);
		base.FormClosing += delegate(object o, FormClosingEventArgs e)
		{
			if (enCours)
			{
				e.Cancel = true;
				if (!annule && MessageBox.Show(this, "Arrêter la publication ? Le catalogue ne sera pas modifié : les joueurs garderont la version actuelle.", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
				{
					annule = true;
					lEtat.ForeColor = C.Gris;
					lEtat.Text = "Arrêt en cours…";
				}
			}
		};
	}

	private Label Lb(string t, int x, int y, int w, int h, Font f, Color c)
	{
		Label label = new Label();
		label.Text = t;
		label.Location = new Point(x, y);
		label.Size = new Size(w, h);
		label.Font = f;
		label.ForeColor = c;
		label.BackColor = C.Fond;
		base.Controls.Add(label);
		return label;
	}

	private void Sur(Action a)
	{
		try
		{
			if (base.IsHandleCreated && !base.IsDisposed)
			{
				BeginInvoke(a);
			}
		}
		catch
		{
		}
	}

	private static string Taille(long o)
	{
		if (o >= 1073741824)
		{
			return ((double)o / 1073741824.0).ToString("0.0") + " Go";
		}
		return Math.Max(1L, o / 1048576) + " Mo";
	}

	private void Choisir()
	{
		using FolderBrowserDialog dlg = new FolderBrowserDialog();
		dlg.Description = "Dossier du mod " + nom;
		dlg.ShowNewFolderButton = false;
		if (dlg.ShowDialog(this) != DialogResult.OK)
		{
			return;
		}
		tDossier.Text = dlg.SelectedPath;
		fichiers = null;
		bPublier.Enabled = false;
		lResume.ForeColor = C.Gris;
		lResume.Text = "Lecture du dossier…";
		UseWaitCursor = true;
		Application.DoEvents();
		try
		{
			fichiers = Paquet.Lister(dlg.SelectedPath, new string[0], out structure, out var ignores);
			long total = 0L;
			foreach (Paquet.Fichier f in fichiers)
			{
				total += f.Taille;
			}
			if (fichiers.Count == 0)
			{
				lResume.ForeColor = C.Rouge;
				lResume.Text = "Aucun fichier à publier dans ce dossier.";
				fichiers = null;
				return;
			}
			lResume.Text = fichiers.Count.ToString("N0") + " fichiers  ·  " + Taille(total) + " avant compression  ·  " + structure + ((ignores.Count > 0) ? ("\n" + ignores.Count + " fichier(s) hors de jeu\\ plugins\\ fonts\\ seront ignorés (ex. " + ignores[0] + ")") : "");
			bPublier.Enabled = true;
		}
		catch (Exception ex)
		{
			lResume.ForeColor = C.Rouge;
			lResume.Text = "Dossier illisible : " + ex.Message;
			fichiers = null;
		}
		finally
		{
			UseWaitCursor = false;
		}
	}

	private void Lancer()
	{
		string version = tVersion.Text.Trim();
		string notes = tNotes.Text.Trim().Replace("\r\n", "\n");
		speciale = cSpeciale.Checked;
		avertissement = tAvert.Text.Trim();
		if (fichiers == null)
		{
			return;
		}
		if (version.Length == 0 || version.Length > 40)
		{
			lEtat.ForeColor = C.Rouge;
			lEtat.Text = "Indiquez le numéro de la nouvelle version.";
			return;
		}
		try
		{
			string cache = Config.Cache;
			if (new DriveInfo(Path.GetPathRoot(cache)).AvailableFreeSpace < 2 * Paquet.TAILLE_PARTIE + 104857600)
			{
				lEtat.ForeColor = C.Rouge;
				lEtat.Text = "Il faut au moins 600 Mo libres sur le disque du lanceur pour préparer une partie.";
				return;
			}
		}
		catch
		{
		}
		if (MessageBox.Show(this, "Publier " + nom + " " + version + (speciale ? " comme version spéciale (au choix, à côté de la version normale)" : "") + " pour tous les joueurs ?\n\nL'envoi peut être long selon votre connexion. Ne fermez pas le lanceur pendant l'envoi.", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
		{
			return;
		}
		enCours = true;
		annule = false;
		bPublier.Enabled = false;
		bParcourir.Enabled = false;
		tVersion.ReadOnly = true;
		tNotes.ReadOnly = true;
		bAnnuler.Text = "Arrêter";
		barre.Visible = true;
		barre.Valeur = 0.0;
		lEtat.ForeColor = C.Gris;
		lEtat.Text = "Préparation…";
		List<Paquet.Fichier> liste = fichiers;
		Thread thread = new Thread((ThreadStart)delegate
		{
			string err = null;
			string fin = null;
			try
			{
				fin = Travail(liste, version, notes);
			}
			catch (OperationCanceledException)
			{
				err = "Publication arrêtée. Le catalogue n'a pas été modifié.";
			}
			catch (Exception ex)
			{
				err = "La publication a échoué : " + ex.Message + "\nLe catalogue n'a pas été modifié : relancez « Publier » avec le même numéro pour reprendre.";
				Journal.Ecrire("admin-paquet", ex);
			}
			Sur(delegate
			{
				enCours = false;
				if (err == null)
				{
					barre.Valeur = 1.0;
					barre.Invalidate();
					MessageBox.Show(this, fin, Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
					base.DialogResult = DialogResult.OK;
					return;
				}
				lEtat.ForeColor = annule ? C.Gris : C.Rouge;
				lEtat.Text = err;
				bAnnuler.Text = "Fermer";
				bPublier.Enabled = true;
				bParcourir.Enabled = true;
				tVersion.ReadOnly = false;
				tNotes.ReadOnly = false;
			});
		});
		thread.IsBackground = true;
		thread.Start();
	}

	private DateTime derEtat = DateTime.MinValue;

	private double valeur;

	private void Etat(string t, double v)
	{
		valeur = v;
		DateTime now = DateTime.Now;
		if ((now - derEtat).TotalMilliseconds < 120.0 && !t.EndsWith("…"))
		{
			return;
		}
		derEtat = now;
		Sur(delegate
		{
			lEtat.Text = t;
			barre.Valeur = v;
			barre.Invalidate();
		});
	}

	private string Travail(List<Paquet.Fichier> liste, string version, string notes)
	{
		Etat("Lecture du catalogue sur GitHub…", 0.0);
		Dictionary<string, object> page = Page(GitHub.LireCatalogue(out var _));
		if (page == null)
		{
			throw new Exception("la page « " + id + " » n'est pas dans le catalogue publié : publiez-la d'abord");
		}
		if (!speciale && string.Equals(Str(page, "version").Trim(), version, StringComparison.OrdinalIgnoreCase))
		{
			throw new Exception("la version " + version + " est déjà celle du catalogue : changez le numéro");
		}
		string baseNom = Paquet.NomFichier(id);
		string fVersion = Str(page, "fichier_version");
		if (fVersion.Length == 0)
		{
			fVersion = baseNom + "_version.txt";
		}
		string fListe = Str(page, "liste");
		if (fListe.Length == 0)
		{
			fListe = baseNom + "_fichiers_installes.txt";
		}
		HashSet<string> exclus = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"jeu/" + fVersion.Replace('\\', '/'),
			"jeu/" + fListe.Replace('\\', '/')
		};
		liste = liste.FindAll((Paquet.Fichier f) => !exclus.Contains(f.Entree));
		long total = 0L;
		foreach (Paquet.Fichier f2 in liste)
		{
			total += f2.Taille;
		}
		string date = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
		string md5;
		using (MD5 m = MD5.Create())
		{
			StringBuilder sb = new StringBuilder();
			byte[] h = m.ComputeHash(Encoding.UTF8.GetBytes(id + "|" + version + "|" + DateTime.UtcNow.Ticks + "|" + total));
			foreach (byte b in h)
			{
				sb.Append(b.ToString("x2"));
			}
			md5 = sb.ToString();
		}
		string tag = Paquet.Tag(id, version);
		if (UtiliseRelease(page, tag))
		{
			throw new Exception("le paquet en ligne utilise déjà la release « " + tag + " » : choisissez un autre numéro de version");
		}
		Etat("Création de la release " + tag + " sur GitHub…", 0.0);
		Dictionary<string, object> release = GitHub.ReleaseParTag(tag) ?? GitHub.CreerRelease(tag, nom + " " + version, (notes.Length > 0) ? notes : ("Paquet " + nom + " " + version + ", publié depuis le lanceur TPS Groupe."));
		bool unSeul = total <= Paquet.TAILLE_PARTIE;
		string tmp = Path.Combine(Config.Cache, "publication", baseNom);
		List<Dictionary<string, object>> parties = new List<Dictionary<string, object>>();
		long tailleTotale = 0L;
		int numCourant = 1;
		try
		{
			Paquet.Construire(liste, "jeu/" + fVersion.Replace('\\', '/'), version + "\r\n" + date + "\r\n" + md5 + "\r\nvariante:" + (speciale ? IdVariante(version) : "") + "\r\n", tmp, baseNom, unSeul, delegate(string chemin, int num)
			{
				string nomFichier = Path.GetFileName(chemin);
				long taille = new FileInfo(chemin).Length;
				Etat("Partie " + num + " : calcul de l'empreinte…", valeur);
				string sha = Web.Sha256(chemin);
				string url = null;
				for (int essai = 1; essai <= 3; essai++)
				{
					try
					{
						GitHub.SupprimerAssetNomme(release, nomFichier);
						url = GitHub.EnvoyerAsset(release, chemin, nomFichier, sha, delegate(long e, long t)
						{
							Etat("Partie " + num + " : envoi sur GitHub " + e * 100 / Math.Max(1L, t) + " %  (" + Taille(e) + " / " + Taille(t) + ")" + ((essai > 1) ? ("  ·  essai " + essai) : ""), valeur);
						}, () => annule);
						break;
					}
					catch (OperationCanceledException)
					{
						throw;
					}
					catch (Exception)
					{
						if (essai == 3 || annule)
						{
							throw;
						}
						Etat("Partie " + num + " : envoi interrompu, nouvel essai dans 10 s…", valeur);
						Thread.Sleep(10000);
					}
				}
				Dictionary<string, object> p = new Dictionary<string, object>();
				p["url"] = url;
				p["sha256"] = sha;
				p["taille"] = taille;
				parties.Add(p);
				tailleTotale += taille;
				numCourant = num + 1;
			}, delegate(long fait)
			{
				Etat("Partie " + numCourant + " : compression " + fait * 100 / Math.Max(1L, total) + " % du mod", (double)fait / (double)Math.Max(1L, total) * 0.98);
			}, () => annule);
		}
		finally
		{
			try
			{
				Directory.Delete(tmp, recursive: true);
			}
			catch
			{
			}
		}
		if (annule)
		{
			throw new OperationCanceledException();
		}
		Etat("Mise à jour du catalogue…", 0.99);
		string sha2;
		Dictionary<string, object> cat = GitHub.LireCatalogue(out sha2);
		page = Page(cat);
		if (page == null)
		{
			throw new Exception("la page « " + id + " » a disparu du catalogue pendant l'envoi");
		}
		Dictionary<string, object> pagePrincipale = page;
		if (speciale)
		{
			ArrayList vars = ((page.TryGetValue("variantes", out var vv) && vv is ArrayList) ? ((ArrayList)vv) : new ArrayList());
			string vid = IdVariante(version);
			Dictionary<string, object> dv = null;
			foreach (object o in vars)
			{
				if (o is Dictionary<string, object> d0 && Str(d0, "id") == vid)
				{
					dv = d0;
				}
			}
			if (dv == null)
			{
				dv = new Dictionary<string, object>();
				vars.Add(dv);
			}
			dv["id"] = vid;
			dv["nom"] = version;
			dv["avertissement"] = avertissement;
			page["variantes"] = vars;
			page = dv;
		}
		page["version"] = version;
		page["date"] = date;
		page["md5"] = md5;
		page["taille"] = tailleTotale;
		pagePrincipale["fichier_version"] = fVersion;
		pagePrincipale["liste"] = fListe;
		if (notes.Length > 0)
		{
			page["notes"] = notes;
		}
		if (parties.Count == 1 && unSeul)
		{
			page["zip"] = parties[0]["url"];
			page["sha256"] = parties[0]["sha256"];
			page.Remove("parties");
		}
		else
		{
			page["zip"] = "";
			page["sha256"] = "";
			ArrayList arrayList = new ArrayList();
			foreach (Dictionary<string, object> item in parties)
			{
				arrayList.Add(item);
			}
			page["parties"] = arrayList;
		}
		GitHub.EcrireCatalogue(cat, sha2, "Paquet " + nom + " " + version + " publié depuis le lanceur par " + ((Comptes.Actif != null) ? Comptes.Actif.Pseudo : "admin") + " (" + parties.Count + " partie(s), " + Taille(tailleTotale) + ")");
		return "✔  " + nom + " " + version + " est en ligne (" + parties.Count + " partie(s), " + Taille(tailleTotale) + ").\n\nLes joueurs verront « METTRE À JOUR ». Aucun fichier du paquet n'est resté sur ce PC : vous pouvez supprimer vos anciennes archives ZIP de " + nom + ".";
	}

	/// <summary>« V1 Alpha » → « v1_alpha » (identifiant d'une version spéciale).</summary>
	private static string IdVariante(string version)
	{
		string s = Categories.Slug(version);
		return (s.Length > 0) ? s : "speciale";
	}

	/// <summary>Vrai si le paquet actuellement en ligne télécharge depuis cette release (on ne doit pas y toucher).</summary>
	private static bool UtiliseRelease(Dictionary<string, object> page, string tag)
	{
		string marque = "/releases/download/" + tag + "/";
		if (Str(page, "zip").IndexOf(marque, StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return true;
		}
		if (page.TryGetValue("parties", out var value) && value is ArrayList)
		{
			foreach (object item in (ArrayList)value)
			{
				if (item is Dictionary<string, object> d && Str(d, "url").IndexOf(marque, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	private Dictionary<string, object> Page(Dictionary<string, object> cat)
	{
		if (cat != null && cat.TryGetValue("projets", out var value) && value is ArrayList)
		{
			foreach (object item in (ArrayList)value)
			{
				if (item is Dictionary<string, object> d && Str(d, "id") == id)
				{
					return d;
				}
			}
		}
		return null;
	}

	private static string Str(Dictionary<string, object> d, string k)
	{
		if (d == null || !d.TryGetValue(k, out var value) || value == null)
		{
			return "";
		}
		return value.ToString();
	}
}
