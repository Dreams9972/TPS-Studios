using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenCommunaute : Form
{
	public Action Publie;

	public Action Annule;

	private Champ tInvite;

	private Champ tApp;

	private Champ tServeur;

	private Champ tCle;

	private Champ tVipLien;

	private Champ tVerif;

	private string annoncesTxt = "";

	private Dictionary<string, string> opts = new Dictionary<string, string>();

	private Label lErr;

	public Dictionary<string, string> Valeurs = new Dictionary<string, string>();

	public FenCommunaute(Dictionary<string, string> init)
	{
		Text = "Communauté et VIP";
		base.ClientSize = new Size(Zx.P(620), Zx.P(712));
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
		Lb("Communauté et VIP", Zx.P(28), Zx.P(20), Zx.P(560), Zx.P(34), new Font("Segoe UI Semibold", Zx.P(17f)), C.Texte);
		Lb("Ces valeurs sont publiées dans le catalogue : les joueurs qui activent « Afficher mon activité sur Discord » n'ont rien à configurer, et leur activité affiche un bouton « Rejoindre TPS Groupe ».", Zx.P(28), Zx.P(60), Zx.P(564), Zx.P(56), new Font("Segoe UI", Zx.P(9.5f)), C.Gris);
		tApp = ChampL("Identifiant d'application (discord.com/developers/applications → Application ID)", Zx.P(28), Zx.P(128), Zx.P(564), init.TryGetValue("discord_app", out var value) ? value : "", 22);
		tInvite = ChampL("Lien d'invitation permanent (https://discord.gg/…), facultatif", Zx.P(28), Zx.P(210), Zx.P(564), init.TryGetValue("discord_invite", out value) ? value : "", 80);
		tServeur = ChampL("Serveur social : adresse https://… (likes et commentaires en temps réel), facultatif", Zx.P(28), Zx.P(292), Zx.P(564), init.TryGetValue("serveur", out value) ? value : "", 120);
		tCle = ChampL("Clé administrateur (gardée sur ce PC) — ou importez cle-admin.txt", Zx.P(28), Zx.P(374), Zx.P(400), "", 80);
		Bouton bouton = new Bouton("Importer la clé…")
		{
			Plein = false,
			Size = new Size(Zx.P(156), Zx.P(36)),
			Location = new Point(Zx.P(436), Zx.P(396))
		};
		base.Controls.Add(bouton);
		tVipLien = ChampL("Lien d'achat du VIP à 5 € (https://… : PayPal, Ko-fi, Stripe…), facultatif", Zx.P(28), Zx.P(456), Zx.P(564), init.TryGetValue("vip_lien", out value) ? value : "", 300);
		tVerif = ChampL("Vérifier l'identifiant VIP d'un joueur (rôle Discord)", Zx.P(28), Zx.P(538), Zx.P(420), "", 40);
		Bouton bouton2 = new Bouton("Vérifier")
		{
			Plein = false,
			Couleur = C.Or,
			Size = new Size(Zx.P(130), Zx.P(36)),
			Location = new Point(Zx.P(462), Zx.P(560))
		};
		base.Controls.Add(bouton2);
		bouton2.Click += delegate
		{
			if (tCle.Text.Trim().Length > 0)
			{
				Social.DefinirCleAdmin(tCle.Text);
			}
			if (tServeur.Text.Trim().Length > 0)
			{
				Social.Configurer(tServeur.Text.Trim());
			}
			if (!Social.Actif || !Social.CleAdminPresente)
			{
				lErr.ForeColor = C.Rouge;
				lErr.Text = "Indiquez l'adresse du serveur et la clé administrateur.";
			}
			else
			{
				string id = tVerif.Text.Trim();
				lErr.ForeColor = C.Gris;
				lErr.Text = "Vérification…";
				Thread thread = new Thread((ThreadStart)delegate
				{
					string r;
					try
					{
						r = Social.VerifierVip(id);
					}
					catch (Exception ex)
					{
						r = "✖  " + ex.Message;
					}
					try
					{
						BeginInvoke((MethodInvoker)delegate
						{
							lErr.ForeColor = (r.StartsWith("✔") ? C.Vert : C.Rouge);
							lErr.Text = r;
						});
					}
					catch
					{
					}
				});
				thread.IsBackground = true;
				thread.Start();
			}
		};
		lErr = Lb("", Zx.P(28), Zx.P(604), Zx.P(564), Zx.P(40), new Font("Segoe UI", Zx.P(9.5f)), C.Rouge);
		bouton.Click += delegate
		{
			if (tServeur.Text.Trim().Length > 0)
			{
				Social.Configurer(tServeur.Text.Trim());
			}
			ImportCle.Lancer(this, lErr);
		};
		Panel panel = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = Zx.P(70),
			BackColor = C.Cote
		};
		base.Controls.Add(panel);
		Bouton bouton3 = new Bouton("Annuler");
		bouton3.Plein = false;
		bouton3.Size = new Size(Zx.P(100), Zx.P(38));
		bouton3.Location = new Point(Zx.P(28), Zx.P(16));
		bouton3.Click += delegate
		{
			if (Annule != null)
			{
				Annule();
			}
			else
			{
				base.DialogResult = DialogResult.Cancel;
			}
		};
		Bouton bouton4 = new Bouton("Publier");
		bouton4.Size = new Size(Zx.P(160), Zx.P(38));
		bouton4.Location = new Point(base.ClientSize.Width - Zx.P(28) - Zx.P(160), Zx.P(16));
		bouton4.Click += delegate
		{
			Valider();
		};
		Bouton bouton5 = new Bouton("Codes VIP");
		bouton5.Plein = false;
		bouton5.Couleur = C.Or;
		bouton5.Size = new Size(Zx.P(110), Zx.P(38));
		bouton5.Location = new Point(Zx.P(136), Zx.P(16));
		bouton5.Click += delegate
		{
			CodesVip();
		};
		annoncesTxt = (init.TryGetValue("annonces", out value) ? value : "");
		Bouton bouton6 = new Bouton("Annonces…")
		{
			Plein = false,
			Size = new Size(Zx.P(112), Zx.P(38)),
			Location = new Point(Zx.P(254), Zx.P(16))
		};
		string[] cles = FenOptionsLanceur.Cles;
		foreach (string key in cles)
		{
			opts[key] = (init.TryGetValue(key, out value) ? value : "");
		}
		Bouton bouton7 = new Bouton("Options…");
		bouton7.Plein = false;
		bouton7.Size = new Size(Zx.P(104), Zx.P(38));
		bouton7.Location = new Point(Zx.P(374), Zx.P(16));
		bouton7.Click += delegate
		{
			using FenOptionsLanceur fenOptionsLanceur = new FenOptionsLanceur(opts);
			if (fenOptionsLanceur.Afficher(this) == DialogResult.OK)
			{
				opts = fenOptionsLanceur.Valeurs;
				lErr.ForeColor = C.Vert;
				lErr.Text = "✔  Options prêtes : cliquez sur « Publier ».";
			}
		};
		bouton6.Click += delegate
		{
			using FenAnnonces fenAnnonces = new FenAnnonces(annoncesTxt);
			if (fenAnnonces.Afficher(this) == DialogResult.OK)
			{
				annoncesTxt = fenAnnonces.Texte;
				lErr.ForeColor = C.Vert;
				lErr.Text = "✔  Annonces prêtes : cliquez sur « Publier ».";
			}
		};
		bouton4.Size = new Size(Zx.P(106), Zx.P(38));
		bouton4.Location = new Point(base.ClientSize.Width - Zx.P(28) - Zx.P(106), Zx.P(16));
		panel.Controls.Add(bouton3);
		panel.Controls.Add(bouton4);
		panel.Controls.Add(bouton5);
		panel.Controls.Add(bouton6);
		panel.Controls.Add(bouton7);
	}

	private void CodesVip()
	{
		if (tCle.Text.Trim().Length > 0)
		{
			Social.DefinirCleAdmin(tCle.Text);
		}
		if (tServeur.Text.Trim().Length > 0)
		{
			Social.Configurer(tServeur.Text.Trim());
		}
		if (!Social.Actif)
		{
			lErr.ForeColor = C.Rouge;
			lErr.Text = "Indiquez d'abord l'adresse du serveur social.";
			return;
		}
		if (!Social.CleAdminPresente)
		{
			lErr.ForeColor = C.Rouge;
			lErr.Text = "Indiquez d'abord la clé administrateur du serveur (fichier cle-admin.txt).";
			return;
		}
		lErr.ForeColor = C.Gris;
		lErr.Text = "Création de 5 codes VIP…";
		Thread thread = new Thread((ThreadStart)delegate
		{
			string err = null;
			List<string> codes = null;
			try
			{
				codes = Social.CodesVip(5, "lanceur", out err);
			}
			catch (Exception ex)
			{
				err = ex.Message;
			}
			try
			{
				BeginInvoke((MethodInvoker)delegate
				{
					if (codes == null)
					{
						lErr.ForeColor = C.Rouge;
						lErr.Text = "Codes impossibles : " + err;
					}
					else
					{
						string text = string.Join("\r\n", codes.ToArray());
						try
						{
							Clipboard.SetText(text);
						}
						catch
						{
						}
						lErr.ForeColor = C.Vert;
						lErr.Text = "✔  5 codes copiés dans le presse-papiers (ils ne seront plus affichés).";
						MessageBox.Show(this, "Codes VIP à usage unique :\n\n" + text + "\n\nIls sont copiés dans le presse-papiers. Donnez-en un à chaque acheteur.", "Codes VIP");
					}
				});
			}
			catch
			{
			}
		});
		thread.IsBackground = true;
		thread.Start();
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

	private Champ ChampL(string etiquette, int x, int y, int w, string val, int max)
	{
		Lb(etiquette, x, y, w, Zx.P(20), new Font("Segoe UI", Zx.P(9f)), C.Gris);
		Champ champ = new Champ(multi: false, Zx.P(36));
		champ.Location = new Point(x, y + Zx.P(22));
		champ.Width = w;
		champ.MaxLength = max;
		champ.Text = val;
		base.Controls.Add(champ);
		return champ;
	}

	private void Valider()
	{
		string text = tInvite.Text.Trim();
		string text2 = tApp.Text.Trim();
		if (text2.Length > 0 && !Regex.IsMatch(text2, "^\\d{15,22}$"))
		{
			lErr.Text = "Identifiant d'application invalide (15 à 22 chiffres).";
			return;
		}
		if (text.Length > 0 && !Regex.IsMatch(text, "^https://(discord\\.gg|discord\\.com/invite)/[A-Za-z0-9-]{2,40}$"))
		{
			lErr.Text = "Lien d'invitation invalide : https://discord.gg/… ou https://discord.com/invite/…";
			return;
		}
		string text3 = tServeur.Text.Trim().TrimEnd('/');
		if (text3.Length > 0 && !Regex.IsMatch(text3, "^https://[A-Za-z0-9.-]+(:\\d{2,5})?$"))
		{
			lErr.Text = "Adresse du serveur invalide : https://nom-du-serveur.workers.dev";
			return;
		}
		string text4 = tVipLien.Text.Trim();
		if (text4.Length > 0 && (!Uri.TryCreate(text4, UriKind.Absolute, out var result) || !(result.Scheme == Uri.UriSchemeHttps)))
		{
			lErr.Text = "Lien d'achat VIP invalide : il doit commencer par https://";
			return;
		}
		foreach (KeyValuePair<string, string> opt in opts)
		{
			Valeurs[opt.Key] = opt.Value ?? "";
		}
		Valeurs["vip_lien"] = text4;
		Valeurs["annonces"] = annoncesTxt;
		Valeurs["discord_app"] = text2;
		Valeurs["discord_invite"] = text;
		Valeurs["serveur"] = text3;
		if (tCle.Text.Trim().Length > 0)
		{
			Social.DefinirCleAdmin(tCle.Text);
		}
		if (Publie != null)
		{
			Publie();
		}
		else
		{
			base.DialogResult = DialogResult.OK;
		}
	}
}
