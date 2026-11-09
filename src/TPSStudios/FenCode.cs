using System;
using System.Drawing;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenCode : Form
{
	private Champ tCode;

	private Label lErr;

	private Projet p;

	public FenCode(Projet projet)
	{
		p = projet;
		base.FormBorderStyle = FormBorderStyle.None;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		base.StartPosition = FormStartPosition.Manual;
		base.ShowInTaskbar = false;
		base.ClientSize = new Size(Zx.P(560), Zx.P(330));
		Panel value = new Panel
		{
			Dock = DockStyle.Top,
			Height = Zx.P(4),
			BackColor = C.Orange
		};
		base.Controls.Add(value);
		Label value2 = new Label
		{
			Text = "Code d'accès",
			Font = new Font("Segoe UI Semibold", Zx.P(15f)),
			ForeColor = C.Texte,
			BackColor = C.Fond,
			Location = new Point(Zx.P(32), Zx.P(30)),
			Size = new Size(Zx.P(500), Zx.P(34))
		};
		base.Controls.Add(value2);
		Label value3 = new Label
		{
			Text = "Entrez le code reçu après l'achat de « " + p.Nom + " » pour débloquer son installation. Il vous a été remis avec votre achat.",
			Font = Font,
			ForeColor = Color.FromArgb(205, 214, 228),
			BackColor = C.Fond,
			Location = new Point(Zx.P(32), Zx.P(76)),
			Size = new Size(Zx.P(496), Zx.P(52))
		};
		base.Controls.Add(value3);
		tCode = new Champ(multi: false, Zx.P(40));
		tCode.Location = new Point(Zx.P(32), Zx.P(140));
		tCode.Width = Zx.P(496);
		tCode.Indice = "Votre code d'accès";
		base.Controls.Add(tCode);
		lErr = new Label();
		lErr.Font = new Font("Segoe UI", Zx.P(9.5f));
		lErr.ForeColor = C.Rouge;
		lErr.BackColor = C.Fond;
		lErr.Location = new Point(Zx.P(32), Zx.P(192));
		lErr.Size = new Size(Zx.P(496), Zx.P(40));
		base.Controls.Add(lErr);
		Panel panel = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = Zx.P(70),
			BackColor = C.Cote
		};
		base.Controls.Add(panel);
		Bouton bouton = new Bouton("Annuler")
		{
			Plein = false,
			Size = new Size(Zx.P(120), Zx.P(38)),
			Location = new Point(Zx.P(28), Zx.P(16))
		};
		EventHandler value4 = delegate
		{
			base.DialogResult = DialogResult.Cancel;
		};
		bouton.Click += value4;
		Bouton bouton2 = new Bouton("Valider");
		bouton2.Size = new Size(Zx.P(140), Zx.P(38));
		bouton2.Location = new Point(Zx.P(560) - Zx.P(28) - Zx.P(140), Zx.P(16));
		bouton2.Click += delegate
		{
			Valider();
		};
		panel.Controls.Add(bouton);
		panel.Controls.Add(bouton2);
	}

	private void Valider()
	{
		if (!p.CodeValide(tCode.Text))
		{
			lErr.Text = ((p.AccesSha.Length == 0) ? "Aucun code n'est défini pour ce contenu : contactez TPS Groupe." : "Code incorrect.");
			return;
		}
		Config.Ecrire("acces_" + p.Id, "1");
		base.DialogResult = DialogResult.OK;
	}
}
