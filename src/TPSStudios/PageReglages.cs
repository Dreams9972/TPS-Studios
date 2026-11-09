using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TPSStudios;

internal class PageReglages : Panel
{
	private const int LargeurMax = 900;

	public Action<Dictionary<string, string>> Publie;

	public Action Annule;

	public Func<Dictionary<string, string>, string> Verif;

	private Dictionary<string, string> init;

	private Panel corps;

	private Panel pied;

	private Label lMsg;

	private Cadre cur;

	private int y;

	private int cy;

	private List<Cadre> cadres = new List<Cadre>();

	private List<KeyValuePair<string, Func<string>>> lecteurs = new List<KeyValuePair<string, Func<string>>>();

	private int Largeur => Math.Max(420, Math.Min(900, corps.ClientSize.Width - 48));

	private int X0 => Math.Max(24, (corps.ClientSize.Width - Largeur) / 2);

	public PageReglages(string titre, string aide, Dictionary<string, string> valeurs)
	{
		init = valeurs ?? new Dictionary<string, string>();
		BackColor = C.Fond;
		pied = new Panel();
		pied.Dock = DockStyle.Bottom;
		pied.Height = Zx.P(66);
		pied.BackColor = C.Cote;
		base.Controls.Add(pied);
		corps = new Panel();
		corps.Dock = DockStyle.Fill;
		corps.AutoScroll = true;
		corps.BackColor = C.Fond;
		base.Controls.Add(corps);
		corps.BringToFront();
		Theme.Sombre(corps);
		Label label = new Label
		{
			Text = titre,
			Font = new Font("Segoe UI Semibold", Zx.P(17f)),
			ForeColor = C.Texte,
			BackColor = C.Fond,
			AutoSize = true,
			Location = new Point(0, Zx.P(22))
		};
		corps.Controls.Add(label);
		Label label2 = new Label
		{
			Text = aide,
			Font = new Font("Segoe UI", Zx.P(9.5f)),
			ForeColor = C.Gris,
			BackColor = C.Fond,
			AutoSize = true,
			Location = new Point(0, Zx.P(60))
		};
		corps.Controls.Add(label2);
		y = Zx.P(96);
		Bouton bouton = new Bouton("Annuler les modifications");
		bouton.Plein = false;
		bouton.Size = new Size(Zx.P(210), Zx.P(38));
		bouton.Location = new Point(Zx.P(24), Zx.P(14));
		bouton.Click += delegate
		{
			if (Annule != null)
			{
				Annule();
			}
		};
		pied.Controls.Add(bouton);
		Bouton ok = new Bouton("Publier");
		ok.Size = new Size(Zx.P(150), Zx.P(38));
		ok.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		ok.Click += delegate
		{
			Valider();
		};
		pied.Controls.Add(ok);
		lMsg = new Label();
		lMsg.Font = new Font("Segoe UI", Zx.P(9.5f));
		lMsg.ForeColor = C.Gris;
		lMsg.BackColor = C.Cote;
		lMsg.TextAlign = ContentAlignment.MiddleLeft;
		lMsg.AutoEllipsis = true;
		lMsg.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		pied.Controls.Add(lMsg);
		pied.Resize += delegate
		{
			ok.Location = new Point(pied.Width - Zx.P(24) - Zx.P(150), Zx.P(14));
			lMsg.Bounds = new Rectangle(Zx.P(250), Zx.P(14), Math.Max(Zx.P(60), pied.Width - Zx.P(250) - Zx.P(24) - Zx.P(150) - Zx.P(16)), Zx.P(38));
		};
		corps.Resize += delegate
		{
			Disposer();
		};
		label.Tag = "titre";
		label2.Tag = "titre";
	}

	private void Disposer()
	{
		corps.SuspendLayout();
		int x = corps.AutoScrollPosition.X;
		_ = corps.AutoScrollPosition.Y;
		foreach (Control control in corps.Controls)
		{
			if (control.Tag as string == "titre")
			{
				control.Left = X0 + x;
			}
			else if (control is Cadre)
			{
				control.Left = X0 + x;
				control.Width = Largeur;
			}
		}
		corps.ResumeLayout();
	}

	public void Message(string t, Color c)
	{
		lMsg.ForeColor = c;
		lMsg.Text = t;
	}

	private string I(string k)
	{
		if (!init.TryGetValue(k, out var value))
		{
			return "";
		}
		return value ?? "";
	}

	private void FinSection()
	{
		if (cur != null)
		{
			cur.Height = cy + Zx.P(14);
			y += cur.Height + Zx.P(16);
		}
	}

	public void Section(string titre)
	{
		FinSection();
		cur = new Cadre();
		cur.Titre = titre;
		cur.Location = new Point(X0, y);
		cur.Size = new Size(Largeur, Zx.P(60));
		corps.Controls.Add(cur);
		cadres.Add(cur);
		cy = Zx.P(50);
		corps.AutoScrollMinSize = new Size(0, y + Zx.P(400));
	}

	private Label Etiq(string t)
	{
		Label label = new Label();
		label.Text = t;
		label.Font = new Font("Segoe UI", Zx.P(9f));
		label.ForeColor = C.Gris;
		label.BackColor = C.Carte;
		label.Location = new Point(Zx.P(24), cy);
		label.AutoSize = true;
		cur.Controls.Add(label);
		cy += Zx.P(22);
		return label;
	}

	public void Note(string t)
	{
		Label label = new Label();
		label.Text = t;
		label.Font = new Font("Segoe UI", Zx.P(9f));
		label.ForeColor = C.Gris;
		label.BackColor = C.Carte;
		label.Location = new Point(Zx.P(24), cy);
		label.Size = new Size(Largeur - Zx.P(48), Zx.P(36));
		label.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		cur.Controls.Add(label);
		cy += Zx.P(40);
		Fin();
	}

	private void Fin()
	{
		cur.Height = cy + Zx.P(14);
		corps.AutoScrollMinSize = new Size(0, y + cur.Height + Zx.P(40));
	}

	public Champ TexteLibre(string etiquette, string indice, int max)
	{
		return Champ0(null, etiquette, indice, max, multi: false, Zx.P(36));
	}

	public Champ Texte(string cle, string etiquette, string indice, int max)
	{
		return Champ0(cle, etiquette, indice, max, multi: false, Zx.P(36));
	}

	public Champ Texte(string cle, string etiquette, string indice, int max, bool multi, int h)
	{
		return Champ0(cle, etiquette, indice, max, multi, h);
	}

	private Champ Champ0(string cle, string etiquette, string indice, int max, bool multi, int h)
	{
		Etiq(etiquette);
		Champ champ = new Champ(multi, h);
		champ.Location = new Point(Zx.P(24), cy);
		champ.Width = Largeur - Zx.P(48);
		champ.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		champ.MaxLength = max;
		champ.Indice = indice;
		if (cle != null)
		{
			string text = I(cle);
			champ.Text = (multi ? text.Replace("\r\n", "\n").Replace("\n", "\r\n") : text);
			Champ tt = champ;
			lecteurs.Add(new KeyValuePair<string, Func<string>>(cle, () => (!multi) ? tt.Text.Trim() : tt.Text.Replace("\r\n", "\n").Trim()));
		}
		cur.Controls.Add(champ);
		cy += h + Zx.P(16);
		Fin();
		return champ;
	}

	public Segments Choix(string cle, string etiquette, string[] noms, string[] valeurs)
	{
		Etiq(etiquette);
		Segments segments = new Segments();
		segments.Items = noms;
		segments.Etire = true;
		segments.Location = new Point(Zx.P(24), cy);
		segments.Width = Largeur - Zx.P(48);
		segments.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		segments.Index = Math.Max(0, Array.IndexOf(valeurs, I(cle)));
		Segments s2 = segments;
		lecteurs.Add(new KeyValuePair<string, Func<string>>(cle, () => valeurs[Math.Max(0, s2.Index)]));
		cur.Controls.Add(segments);
		cy += Zx.P(50);
		Fin();
		return segments;
	}

	public Option Bascule(string cle, string titre, string aide, string valOui, string valNon, bool defaut)
	{
		string text = I(cle);
		Option option = new Option(titre, aide, init.ContainsKey(cle) ? (text == valOui) : defaut);
		option.Location = new Point(Zx.P(14), cy - Zx.P(4));
		option.Width = Largeur - Zx.P(28);
		option.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		Option o2 = option;
		lecteurs.Add(new KeyValuePair<string, Func<string>>(cle, () => (!o2.Checked) ? valNon : valOui));
		cur.Controls.Add(option);
		cy += Zx.P(66);
		Fin();
		return option;
	}

	public Bouton Action(string texte, Color couleur, out Label etat)
	{
		Bouton bouton = new Bouton(texte);
		bouton.Plein = false;
		bouton.Couleur = couleur;
		bouton.Size = new Size(Math.Max(Zx.P(150), Lang.MeasureText(texte, new Font("Segoe UI Semibold", Zx.P(10f))).Width + Zx.P(40)), Zx.P(36));
		bouton.Location = new Point(Zx.P(24), cy);
		cur.Controls.Add(bouton);
		Label label = new Label();
		label.Font = new Font("Segoe UI", Zx.P(9f));
		label.ForeColor = C.Gris;
		label.BackColor = C.Carte;
		label.Location = new Point(bouton.Right + Zx.P(14), cy);
		label.Size = new Size(Math.Max(Zx.P(80), Largeur - bouton.Right - Zx.P(38)), Zx.P(36));
		label.TextAlign = ContentAlignment.MiddleLeft;
		label.AutoEllipsis = true;
		label.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		cur.Controls.Add(label);
		etat = label;
		cy += Zx.P(50);
		Fin();
		return bouton;
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		FinSection();
		cur = null;
		corps.AutoScrollMinSize = new Size(0, y + Zx.P(20));
		Disposer();
		base.OnHandleCreated(e);
	}

	private void Valider()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (KeyValuePair<string, Func<string>> lecteur in lecteurs)
		{
			dictionary[lecteur.Key] = lecteur.Value();
		}
		string text = ((Verif != null) ? Verif(dictionary) : null);
		if (text != null)
		{
			Message("✖  " + text, C.Rouge);
			return;
		}
		Message("Publication…", C.Gris);
		Application.DoEvents();
		if (Publie != null)
		{
			Publie(dictionary);
		}
	}
}
