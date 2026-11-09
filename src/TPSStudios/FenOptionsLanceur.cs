using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenOptionsLanceur : Form
{
	public Dictionary<string, string> Valeurs = new Dictionary<string, string>();

	public static readonly string[] Cles = new string[7] { "alerte", "alerte_type", "maintenance", "maintenance_msg", "themes", "boutique", "version_min" };

	private Champ tAlerte;

	private Champ tMaint;

	private Champ tVmin;

	private Segments sgType;

	private Segments sgThemes;

	private Option oMaint;

	private Option oBoutique;

	private Label lErr;

	public FenOptionsLanceur(Dictionary<string, string> init)
	{
		FenOptionsLanceur fenOptionsLanceur = this;
		Text = "Options du lanceur";
		base.ClientSize = new Size(Zx.P(640), Zx.P(736));
		base.FormBorderStyle = FormBorderStyle.None;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		string value2;
		Func<string, string> func = (string k) => (init == null || !init.TryGetValue(k, out value2)) ? "" : (value2 ?? "");
		Func<string, string> func2 = func;
		Label value = new Label
		{
			Text = "Options du lanceur",
			Font = new Font("Segoe UI Semibold", Zx.P(17f)),
			ForeColor = C.Texte,
			BackColor = C.Fond,
			Location = new Point(Zx.P(28), Zx.P(18)),
			AutoSize = true
		};
		base.Controls.Add(value);
		Func<string, int, Label> func3 = delegate(string t, int y)
		{
			Label label = new Label
			{
				Text = t,
				Font = new Font("Segoe UI", Zx.P(9f)),
				ForeColor = C.Gris,
				BackColor = C.Fond,
				Location = new Point(Zx.P(28), y),
				AutoSize = true
			};
			base.Controls.Add(label);
			return label;
		};
		func3("Bandeau affiché en haut de l'accueil de tous les joueurs (vide = aucun)", 66);
		tAlerte = new Champ(multi: false, Zx.P(36));
		tAlerte.Location = new Point(Zx.P(28), Zx.P(88));
		tAlerte.Width = Zx.P(584);
		tAlerte.MaxLength = 140;
		tAlerte.Text = func2("alerte");
		tAlerte.Indice = "ex. Maintenance du serveur ce soir de 22 h à 23 h";
		base.Controls.Add(tAlerte);
		sgType = new Segments();
		sgType.Items = new string[3] { "Information", "Avertissement", "Important" };
		sgType.Etire = true;
		sgType.Location = new Point(Zx.P(28), Zx.P(132));
		sgType.Width = Zx.P(584);
		string text = func2("alerte_type");
		sgType.Index = ((text == "avertissement") ? 1 : ((text == "important") ? 2 : 0));
		base.Controls.Add(sgType);
		oMaint = new Option("Mode maintenance", "Suspend les installations et mises à jour des mods pour tous les joueurs (le jeu reste jouable).", func2("maintenance") == "1");
		oMaint.Location = new Point(Zx.P(20), Zx.P(182));
		oMaint.Width = Zx.P(600);
		base.Controls.Add(oMaint);
		func3("Message affiché pendant la maintenance", 250);
		tMaint = new Champ(multi: false, Zx.P(36));
		tMaint.Location = new Point(Zx.P(28), Zx.P(272));
		tMaint.Width = Zx.P(584);
		tMaint.MaxLength = 160;
		tMaint.Text = func2("maintenance_msg");
		tMaint.Indice = "ex. Mise à jour du catalogue en cours, revenez dans une heure";
		base.Controls.Add(tMaint);
		func3("Thèmes des fêtes", 322);
		sgThemes = new Segments();
		sgThemes.Items = new string[2] { "Selon le calendrier (et le choix des joueurs)", "Désactivés pour tous" };
		sgThemes.Etire = true;
		sgThemes.Location = new Point(Zx.P(28), Zx.P(344));
		sgThemes.Width = Zx.P(584);
		sgThemes.Index = ((func2("themes") == "off") ? 1 : 0);
		base.Controls.Add(sgThemes);
		oBoutique = new Option("Boutique visible", "Affiche la tuile « Boutique » sur l'accueil de tous les joueurs.", func2("boutique") != "0");
		oBoutique.Location = new Point(Zx.P(20), Zx.P(394));
		oBoutique.Width = Zx.P(600);
		base.Controls.Add(oBoutique);
		func3("Version minimale du lanceur : les versions plus anciennes se mettent à jour d'office (ex. 2.6.0)", 462);
		tVmin = new Champ(multi: false, Zx.P(36));
		tVmin.Location = new Point(Zx.P(28), Zx.P(484));
		tVmin.Width = Zx.P(200);
		tVmin.MaxLength = 12;
		tVmin.Text = func2("version_min");
		tVmin.Indice = "vide = aucune";
		base.Controls.Add(tVmin);
		func3("Salon Discord des tickets de support : Paramètres du salon › Intégrations › Webhooks › Copier l'URL du webhook", 532);
		Champ tWh = new Champ(multi: false, Zx.P(36));
		tWh.Location = new Point(Zx.P(28), Zx.P(554));
		tWh.Width = Zx.P(440);
		tWh.MaxLength = 200;
		tWh.Indice = "https://discord.com/api/webhooks/…  (gardé sur le serveur, jamais publié)";
		base.Controls.Add(tWh);
		Bouton bouton = new Bouton("Enregistrer")
		{
			Size = new Size(Zx.P(136), Zx.P(36)),
			Location = new Point(Zx.P(476), Zx.P(554))
		};
		base.Controls.Add(bouton);
		Label lWh = func3("", 598);
		lWh.AutoSize = false;
		lWh.Size = new Size(Zx.P(430), Zx.P(20));
		Bouton bouton2 = new Bouton("Importer la clé…")
		{
			Plein = false,
			Size = new Size(Zx.P(136), Zx.P(30)),
			Location = new Point(Zx.P(476), Zx.P(594))
		};
		base.Controls.Add(bouton2);
		bouton2.Click += delegate
		{
			ImportCle.Lancer(fenOptionsLanceur, lWh);
		};
		Action etatWh = delegate
		{
			Thread thread2 = new Thread((ThreadStart)delegate
			{
				bool? b = Social.WebhookRegle();
				try
				{
					fenOptionsLanceur.BeginInvoke((MethodInvoker)delegate
					{
						lWh.ForeColor = ((b == true) ? C.Vert : C.Gris);
						lWh.Text = ((b == true) ? "✔  Tickets actifs : ils arrivent dans ton salon Discord." : ((b == false) ? "Aucun salon réglé : le bouton Support affiche « pas encore configuré »." : "État inconnu (serveur ou clé administrateur absents)."));
					});
				}
				catch
				{
				}
			});
			thread2.IsBackground = true;
			thread2.Start();
		};
		bouton.Click += delegate
		{
			string u = tWh.Text.Trim();
			lWh.ForeColor = C.Gris;
			lWh.Text = "Enregistrement…";
			Thread thread = new Thread((ThreadStart)delegate
			{
				string e = Social.ReglerWebhook(u);
				try
				{
					fenOptionsLanceur.BeginInvoke((MethodInvoker)delegate
					{
						if (e != null)
						{
							lWh.ForeColor = C.Rouge;
							lWh.Text = "✖  " + e;
						}
						else
						{
							tWh.Text = "";
							etatWh();
						}
					});
				}
				catch
				{
				}
			});
			thread.IsBackground = true;
			thread.Start();
		};
		base.HandleCreated += delegate
		{
			etatWh();
		};
		lErr = func3("", 626);
		lErr.ForeColor = C.Rouge;
		Panel panel = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = Zx.P(70),
			BackColor = C.Cote
		};
		base.Controls.Add(panel);
		Bouton bouton3 = new Bouton("Annuler");
		bouton3.Plein = false;
		bouton3.Size = new Size(Zx.P(120), Zx.P(38));
		bouton3.Location = new Point(Zx.P(28), Zx.P(16));
		bouton3.Click += delegate
		{
			base.DialogResult = DialogResult.Cancel;
		};
		panel.Controls.Add(bouton3);
		Bouton bouton4 = new Bouton("Valider")
		{
			Size = new Size(Zx.P(140), Zx.P(38)),
			Location = new Point(base.ClientSize.Width - Zx.P(28) - Zx.P(140), Zx.P(16))
		};
		panel.Controls.Add(bouton4);
		bouton4.Click += delegate
		{
			string text2 = tVmin.Text.Trim();
			if (text2.Length > 0 && !Regex.IsMatch(text2, "^\\d+\\.\\d+\\.\\d+$"))
			{
				lErr.Text = "Version minimale invalide (format 2.6.0).";
			}
			else
			{
				string text3 = tAlerte.Text.Trim();
				Valeurs["alerte"] = text3;
				Valeurs["alerte_type"] = ((text3.Length > 0) ? (new string[3] { "information", "avertissement", "important" })[Math.Max(0, sgType.Index)] : "");
				Valeurs["maintenance"] = (oMaint.Checked ? "1" : "");
				Valeurs["maintenance_msg"] = (oMaint.Checked ? tMaint.Text.Trim() : "");
				Valeurs["themes"] = ((sgThemes.Index == 1) ? "off" : "");
				Valeurs["boutique"] = (oBoutique.Checked ? "" : "0");
				Valeurs["version_min"] = text2;
				base.DialogResult = DialogResult.OK;
			}
		};
	}
}
