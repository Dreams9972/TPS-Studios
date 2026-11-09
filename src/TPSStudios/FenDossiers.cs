using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenDossiers : Form
{
	private Champ tO;

	private Champ tQ;

	private Label eO;

	private Label eQ;

	private Bouton bOk;

	public FenDossiers(string pourquoi)
	{
		Theme.Fenetre(this);
		Text = "Dossiers du jeu";
		base.ClientSize = new Size(Zx.P(640), (pourquoi != null) ? Zx.P(392) : Zx.P(348));
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
		int num = Zx.P(18);
		if (pourquoi != null)
		{
			Lb(pourquoi, Zx.P(24), num, Zx.P(592), Zx.P(40), new Font("Segoe UI Semibold", Zx.P(10f)), C.Orange);
			num += Zx.P(44);
		}
		Lb("Le lanceur a besoin de deux dossiers. Ils sont souvent l'un dans l'autre : openOMSI est en général un sous-dossier d'OMSI 2.", Zx.P(24), num, Zx.P(592), Zx.P(42), Font, C.Gris);
		num += Zx.P(50);
		tO = Ligne("1.  Dossier d'OMSI 2", "celui où se trouve Omsi.exe : la carte, les rames et les objets y vont", num, Omsi.DetecteOmsi(), out eO);
		num += Zx.P(114);
		tQ = Ligne("2.  Dossier d'openOMSI", "celui où se trouve openomsi.exe : plugins de cabine et lancement du jeu", num, Omsi.DetecteOpen(Omsi.DetecteOmsi()), out eQ);
		Bouton bouton = new Bouton("Détecter automatiquement");
		bouton.Plein = false;
		bouton.Size = new Size(Zx.P(210), Zx.P(36));
		bouton.Location = new Point(Zx.P(156), base.ClientSize.Height - Zx.P(54));
		bouton.Click += delegate
		{
			Config.Ecrire("omsi", "");
			Config.Ecrire("openomsi", "");
			Omsi.Oublier();
			tO.Text = Omsi.DetecteOmsi() ?? "";
			tQ.Text = Omsi.DetecteOpen(tO.Text) ?? "";
		};
		Bouton bouton2 = new Bouton("Annuler");
		bouton2.Plein = false;
		bouton2.Size = new Size(Zx.P(120), Zx.P(36));
		bouton2.Location = new Point(Zx.P(24), base.ClientSize.Height - Zx.P(54));
		bouton2.Click += delegate
		{
			base.DialogResult = DialogResult.Cancel;
		};
		bOk = new Bouton("Enregistrer");
		bOk.Size = new Size(Zx.P(120), Zx.P(36));
		bOk.Location = new Point(Zx.P(496), base.ClientSize.Height - Zx.P(54));
		bOk.Click += delegate
		{
			Config.Ecrire("omsi", tO.Text.Trim());
			Config.Ecrire("openomsi", tQ.Text.Trim());
			Omsi.Oublier();
			base.DialogResult = DialogResult.OK;
		};
		base.Controls.Add(bouton);
		base.Controls.Add(bouton2);
		base.Controls.Add(bOk);
		tO.TextChanged += delegate
		{
			if (tQ.Text.Trim().Length == 0)
			{
				tQ.Text = Omsi.DetecteOpen(tO.Text.Trim()) ?? "";
			}
			Valider();
		};
		tQ.TextChanged += delegate
		{
			Valider();
		};
		Valider();
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

	private Champ Ligne(string titre, string aide, int y, string val, out Label etat)
	{
		Lb(titre, Zx.P(24), y, Zx.P(592), Zx.P(22), new Font("Segoe UI Semibold", Zx.P(10.5f)), C.Texte);
		Lb(aide, Zx.P(24), y + Zx.P(22), Zx.P(592), Zx.P(20), new Font("Segoe UI", Zx.P(9f)), C.Gris);
		Champ t = new Champ(multi: false, Zx.P(36));
		t.Location = new Point(Zx.P(24), y + Zx.P(44));
		t.Width = Zx.P(470);
		t.Text = val ?? "";
		base.Controls.Add(t);
		Bouton bouton = new Bouton("Parcourir…");
		bouton.Plein = false;
		bouton.Size = new Size(Zx.P(112), Zx.P(36));
		bouton.Location = new Point(Zx.P(504), y + Zx.P(44));
		string ti = titre;
		string ai = aide;
		bouton.Click += delegate
		{
			using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			folderBrowserDialog.Description = ti + " : " + ai;
			folderBrowserDialog.SelectedPath = (Directory.Exists(t.Text) ? t.Text : "");
			folderBrowserDialog.ShowNewFolderButton = false;
			if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
			{
				t.Text = folderBrowserDialog.SelectedPath;
			}
		};
		base.Controls.Add(bouton);
		etat = Lb("", Zx.P(24), y + Zx.P(84), Zx.P(592), Zx.P(20), new Font("Segoe UI", Zx.P(9f)), C.Gris);
		return t;
	}

	private void Valider()
	{
		bool flag = File.Exists(Path.Combine(tO.Text.Trim(), "Omsi.exe"));
		bool flag2 = File.Exists(Path.Combine(tQ.Text.Trim(), "openomsi.exe"));
		eO.Text = (flag ? "✔  Omsi.exe trouvé" : "✖  Omsi.exe n'est pas dans ce dossier");
		eO.ForeColor = (flag ? C.Vert : C.Rouge);
		eQ.Text = (flag2 ? "✔  openomsi.exe trouvé" : ((tQ.Text.Trim().Length == 0) ? "✖  à indiquer (sinon : pas de plugins de cabine ni de bouton JOUER)" : "✖  openomsi.exe n'est pas dans ce dossier"));
		eQ.ForeColor = (flag2 ? C.Vert : C.Orange);
		bOk.Enabled = flag;
	}
}
