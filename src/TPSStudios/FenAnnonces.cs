using System.Drawing;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenAnnonces : Form
{
	private Champ t;

	public string Texte = "";

	public FenAnnonces(string actuel)
	{
		Text = "Annonces";
		base.ClientSize = new Size(Zx.P(620), Zx.P(470));
		base.FormBorderStyle = FormBorderStyle.None;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		Label value = new Label
		{
			Text = "Annonces de l'équipe",
			Font = new Font("Segoe UI Semibold", Zx.P(17f)),
			ForeColor = C.Texte,
			BackColor = C.Fond,
			Location = new Point(Zx.P(28), Zx.P(20)),
			AutoSize = true
		};
		base.Controls.Add(value);
		Label value2 = new Label
		{
			Text = "Une annonce par ligne : « Titre | texte ». Elles s'affichent en premier dans la case « Informations » de l'accueil de tous les joueurs.\nExemple :  Tournoi ParisView samedi | Rendez-vous sur le Discord à 20 h",
			Font = new Font("Segoe UI", Zx.P(9.5f)),
			ForeColor = C.Gris,
			BackColor = C.Fond,
			Location = new Point(Zx.P(28), Zx.P(62)),
			Size = new Size(Zx.P(564), Zx.P(56))
		};
		base.Controls.Add(value2);
		t = new Champ(multi: true, Zx.P(250));
		t.Location = new Point(Zx.P(28), Zx.P(126));
		t.Width = Zx.P(564);
		t.MaxLength = 1500;
		t.Text = (actuel ?? "").Replace("\r\n", "\n").Replace("\n", "\r\n");
		base.Controls.Add(t);
		Panel panel = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = Zx.P(70),
			BackColor = C.Cote
		};
		base.Controls.Add(panel);
		Bouton bouton = new Bouton("Annuler");
		bouton.Plein = false;
		bouton.Size = new Size(Zx.P(120), Zx.P(38));
		bouton.Location = new Point(Zx.P(28), Zx.P(16));
		bouton.Click += delegate
		{
			base.DialogResult = DialogResult.Cancel;
		};
		panel.Controls.Add(bouton);
		Bouton bouton2 = new Bouton("Valider");
		bouton2.Size = new Size(Zx.P(140), Zx.P(38));
		bouton2.Location = new Point(base.ClientSize.Width - Zx.P(28) - Zx.P(140), Zx.P(16));
		bouton2.Click += delegate
		{
			Texte = t.Text.Replace("\r\n", "\n").Trim();
			base.DialogResult = DialogResult.OK;
		};
		panel.Controls.Add(bouton2);
	}
}
