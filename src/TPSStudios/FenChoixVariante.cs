using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TPSStudios;

/// <summary>
/// Choix de la version d'un mod au moment de jouer ou d'installer : la version normale ou une version
/// spéciale (ex. « V1 Alpha »). La version installée est mise en avant ; une version rangée sur ce PC
/// se remet en place instantanément, sans téléchargement.
/// </summary>
internal class FenChoixVariante : Form
{
	public Projet Choix;

	public FenChoixVariante(Projet p, Projet installee, string omsi, bool installer)
	{
		List<Projet> versions = (p.Principal ?? p).Versions();
		Text = p.Nom;
		base.FormBorderStyle = FormBorderStyle.None;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		int w = Zx.P(600);
		int y = Zx.P(22);
		Label titre = new Label
		{
			Text = (installer ? "Quelle version installer ?" : "Quelle version jouer ?"),
			Font = new Font("Segoe UI Semibold", Zx.P(17f)),
			ForeColor = C.Texte,
			BackColor = C.Fond,
			AutoSize = true,
			Location = new Point(Zx.P(28), y)
		};
		base.Controls.Add(titre);
		y += Zx.P(40);
		Label sous = new Label
		{
			Text = (p.Principal ?? p).Nom,
			Font = new Font("Segoe UI", Zx.P(9.5f)),
			ForeColor = C.Gris,
			BackColor = C.Fond,
			AutoSize = true,
			Location = new Point(Zx.P(30), y)
		};
		base.Controls.Add(sous);
		y += Zx.P(36);
		foreach (Projet v in versions)
		{
			Projet vv = v;
			bool estInstallee = installee != null && installee.VarianteId == v.VarianteId;
			string nom = ((v.VarianteId.Length == 0) ? ("Version normale" + ((v.Version.Length > 0) ? ("  ·  " + v.Version) : "")) : v.VarianteNom);
			Bouton b = new Bouton((estInstallee && !installer ? "▶  " : "") + nom);
			b.Plein = estInstallee;
			b.Couleur = ((v.VarianteId.Length == 0) ? C.Orange : Color.FromArgb(150, 90, 230));
			b.Size = new Size(w - Zx.P(56), Zx.P(48));
			b.Location = new Point(Zx.P(28), y);
			b.Click += delegate
			{
				Choix = vv;
				base.DialogResult = DialogResult.OK;
			};
			base.Controls.Add(b);
			y += Zx.P(54);
			string etat;
			Color ce;
			if (estInstallee)
			{
				etat = "✔  Installée sur ce PC";
				ce = C.Vert;
			}
			else if (Omsi.VersionRangee(p, omsi, v.VarianteId))
			{
				etat = "Déjà sur ce PC : changement instantané, sans téléchargement";
				ce = C.Jade;
			}
			else
			{
				etat = "À télécharger" + ((v.Taille > 0) ? ("  ·  " + Math.Max(1L, v.Taille / 1048576) + " Mo") : "");
				ce = C.Gris;
			}
			Label le = new Label
			{
				Text = etat,
				Font = new Font("Segoe UI", Zx.P(9f)),
				ForeColor = ce,
				BackColor = C.Fond,
				AutoSize = true,
				Location = new Point(Zx.P(30), y)
			};
			base.Controls.Add(le);
			y += Zx.P(22);
			string avert = ((v.Avertissement.Length > 0) ? v.Avertissement : "");
			if (avert.Length > 0)
			{
				Label la = new Label
				{
					Text = "⚠  " + avert,
					Font = new Font("Segoe UI", Zx.P(9f)),
					ForeColor = C.Orange,
					BackColor = C.Fond,
					AutoSize = false,
					Location = new Point(Zx.P(30), y),
					Size = new Size(w - Zx.P(60), Zx.P(40))
				};
				base.Controls.Add(la);
				y += Zx.P(42);
			}
			y += Zx.P(10);
		}
		Bouton annuler = new Bouton("Annuler");
		annuler.Plein = false;
		annuler.Size = new Size(Zx.P(120), Zx.P(36));
		annuler.Location = new Point(w - Zx.P(28) - Zx.P(120), y + Zx.P(4));
		annuler.Click += delegate
		{
			base.DialogResult = DialogResult.Cancel;
		};
		base.Controls.Add(annuler);
		base.ClientSize = new Size(w, y + Zx.P(56));
	}
}
