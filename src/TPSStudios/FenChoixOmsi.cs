using System;
using System.Drawing;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenChoixOmsi : Form
{
	public string Choix = "";

	public FenChoixOmsi(bool openPresent)
	{
		Text = "Jouer à OMSI 2";
		base.ClientSize = new Size(Zx.P(560), Zx.P(300));
		base.FormBorderStyle = FormBorderStyle.None;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		Label value = new Label
		{
			Text = "Jouer à OMSI 2",
			Font = new Font("Segoe UI Semibold", Zx.P(17f)),
			ForeColor = C.Texte,
			BackColor = C.Fond,
			AutoSize = true,
			Location = new Point(Zx.P(28), Zx.P(22))
		};
		base.Controls.Add(value);
		Label value2 = new Label
		{
			Text = "Comment voulez-vous lancer le jeu ?",
			Font = new Font("Segoe UI", Zx.P(9.5f)),
			ForeColor = C.Gris,
			BackColor = C.Fond,
			AutoSize = true,
			Location = new Point(Zx.P(30), Zx.P(62))
		};
		base.Controls.Add(value2);
		Bouton bouton = new Bouton("openOMSI  ·  recommandé")
		{
			Size = new Size(Zx.P(504), Zx.P(48)),
			Location = new Point(Zx.P(28), Zx.P(100)),
			Enabled = openPresent
		};
		EventHandler value3 = delegate
		{
			Choix = "open";
			base.DialogResult = DialogResult.OK;
		};
		bouton.Click += value3;
		base.Controls.Add(bouton);
		Label value4 = new Label
		{
			Text = (openPresent ? "Plugins de cabine (écran, prise de service) et réglages du lanceur." : "openOMSI n'est pas installé ou est introuvable."),
			Font = new Font("Segoe UI", Zx.P(9f)),
			ForeColor = C.Gris,
			BackColor = C.Fond,
			AutoSize = true,
			Location = new Point(Zx.P(30), Zx.P(152))
		};
		base.Controls.Add(value4);
		Bouton bouton2 = new Bouton("OMSI 2 classique (Steam)");
		bouton2.Plein = false;
		bouton2.Size = new Size(Zx.P(504), Zx.P(44));
		bouton2.Location = new Point(Zx.P(28), Zx.P(184));
		bouton2.Click += delegate
		{
			Choix = "classique";
			base.DialogResult = DialogResult.OK;
		};
		base.Controls.Add(bouton2);
		Bouton bouton3 = new Bouton("Annuler");
		bouton3.Plein = false;
		bouton3.Size = new Size(Zx.P(120), Zx.P(36));
		bouton3.Location = new Point(Zx.P(412), Zx.P(246));
		bouton3.Click += delegate
		{
			base.DialogResult = DialogResult.Cancel;
		};
		base.Controls.Add(bouton3);
	}
}
