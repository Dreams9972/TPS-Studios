using System.Drawing;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenJeton : Form
{
	private Champ tJeton;

	private Label lEtat;

	private Bouton bOk;

	public FenJeton()
	{
		Text = "Mode administrateur";
		base.ClientSize = new Size(Zx.P(660), Zx.P(520));
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
		Lb("Activer le mode administrateur", Zx.P(28), Zx.P(22), Zx.P(600), Zx.P(34), new Font("Segoe UI Semibold", Zx.P(17f)), C.Texte);
		Cadre cadre = new Cadre
		{
			Titre = "Comment ça marche",
			Location = new Point(Zx.P(28), Zx.P(74)),
			Size = new Size(Zx.P(604), Zx.P(190))
		};
		base.Controls.Add(cadre);
		Label value = new Label
		{
			BackColor = C.Carte,
			ForeColor = Color.FromArgb(205, 214, 228),
			Font = new Font("Segoe UI", Zx.P(9.5f)),
			Location = new Point(Zx.P(20), Zx.P(44)),
			Size = new Size(Zx.P(564), Zx.P(138)),
			Text = "Ce mode permet de modifier les pages du catalogue TPS Groupe, publiées ou à venir. Il demande un jeton d'accès GitHub autorisé à écrire sur le dépôt Dreams9972/TPS-Studios (permission « Contents : Read and write »).\n\nLe jeton est vérifié auprès de GitHub puis chiffré pour votre session Windows : il n'est jamais affiché ni envoyé ailleurs."
		};
		cadre.Controls.Add(value);
		Lb("Jeton d'accès GitHub", Zx.P(30), Zx.P(284), Zx.P(600), Zx.P(22), new Font("Segoe UI Semibold", Zx.P(10.5f)), C.Texte);
		tJeton = new Champ(multi: false, Zx.P(40));
		tJeton.Location = new Point(Zx.P(28), Zx.P(312));
		tJeton.Width = Zx.P(604);
		tJeton.Mdp = true;
		tJeton.Indice = "ghp_… ou github_pat_…";
		base.Controls.Add(tJeton);
		lEtat = Lb("", Zx.P(30), Zx.P(366), Zx.P(600), Zx.P(50), new Font("Segoe UI", Zx.P(9.5f)), C.Gris);
		Panel panel = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = Zx.P(70),
			BackColor = C.Cote
		};
		base.Controls.Add(panel);
		Bouton bouton = new Bouton("Utiliser le GitHub CLI de ce PC");
		bouton.Plein = false;
		bouton.Size = new Size(Zx.P(260), Zx.P(38));
		bouton.Location = new Point(Zx.P(150), Zx.P(16));
		bouton.Click += delegate
		{
			lEtat.ForeColor = C.Gris;
			lEtat.Text = "Lecture du compte GitHub CLI…";
			Application.DoEvents();
			string text = GitHub.JetonGhCli();
			if (text == null)
			{
				lEtat.ForeColor = C.Rouge;
				lEtat.Text = "GitHub CLI introuvable ou non connecté sur ce PC. Saisissez un jeton à la place.";
			}
			else
			{
				Activer(text);
			}
		};
		Bouton bouton2 = new Bouton("Annuler");
		bouton2.Plein = false;
		bouton2.Size = new Size(Zx.P(110), Zx.P(38));
		bouton2.Location = new Point(Zx.P(28), Zx.P(16));
		bouton2.Click += delegate
		{
			base.DialogResult = DialogResult.Cancel;
		};
		bOk = new Bouton("Activer");
		bOk.Size = new Size(Zx.P(120), Zx.P(38));
		bOk.Location = new Point(base.ClientSize.Width - Zx.P(28) - Zx.P(120), Zx.P(16));
		bOk.Click += delegate
		{
			if (tJeton.Text.Trim().Length < 20)
			{
				lEtat.ForeColor = C.Rouge;
				lEtat.Text = "Saisissez un jeton GitHub valide.";
			}
			else
			{
				Activer(tJeton.Text.Trim());
			}
		};
		panel.Controls.Add(bouton);
		panel.Controls.Add(bouton2);
		panel.Controls.Add(bOk);
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

	private void Activer(string jeton)
	{
		lEtat.ForeColor = C.Gris;
		lEtat.Text = "Vérification auprès de GitHub…";
		Application.DoEvents();
		string text = GitHub.Verifier(jeton);
		if (text != null)
		{
			lEtat.ForeColor = C.Rouge;
			lEtat.Text = text;
		}
		else
		{
			GitHub.EnregistrerJeton(jeton);
			tJeton.Text = "";
			base.DialogResult = DialogResult.OK;
		}
	}
}
