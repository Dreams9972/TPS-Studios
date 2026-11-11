using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenTray : Form
{
	private ZoneDessin z;

	private Timer tm;

	private Action ouvrir;

	private List<KeyValuePair<Rectangle, Action>> clics = new List<KeyValuePair<Rectangle, Action>>();

	private Point souris = new Point(-1, -1);

	private static readonly Font fT = new Font("Segoe UI Semibold", Zx.P(10.5f));

	private static readonly Font fS = new Font("Segoe UI", Zx.P(9f));

	private static readonly Font fG = new Font("Segoe MDL2 Assets", Zx.P(13f));

	private static readonly Font fG2 = new Font("Segoe MDL2 Assets", Zx.P(16f));

	private static readonly Font fB = new Font("Segoe UI Semibold", Zx.P(9f));

	private Rectangle rBar;

	private bool glisse;

	private bool volOuvert;

	private Action quitter;

	protected override bool ShowWithoutActivation => false;

	private void Volume(bool ouvert)
	{
		volOuvert = ouvert;
		int num = (ouvert ? 226 : 196);
		int bottom = base.Bottom;
		base.Size = new Size(base.Width, num);
		base.Top = bottom - num;
		using (GraphicsPath path = Bouton.Rond(new Rectangle(0, 0, base.Width, base.Height), Zx.P(14)))
		{
			Region region = base.Region;
			base.Region = new Region(path);
			region?.Dispose();
		}
		z.Invalidate();
	}

	private void Regle(int x)
	{
		Rectangle rectangle = Rectangle.Inflate(rBar, -Zx.P(6), -Zx.P(10));
		VolumeWin.Ecrire((float)(x - rectangle.X) / (float)Math.Max(1, rectangle.Width));
		z.Invalidate();
	}

	public FenTray(Action ouvrirLanceur)
		: this(ouvrirLanceur, null)
	{
	}

	public FenTray(Action ouvrirLanceur, Action quitterLanceur)
	{
		ouvrir = ouvrirLanceur;
		quitter = quitterLanceur;
		base.FormBorderStyle = FormBorderStyle.None;
		base.ShowInTaskbar = false;
		base.TopMost = true;
		base.StartPosition = FormStartPosition.Manual;
		BackColor = C.Fond;
		base.Size = new Size(Zx.P(340), Zx.P(196));
		Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
		base.Location = new Point(workingArea.Right - base.Width - Zx.P(12), workingArea.Bottom - base.Height - Zx.P(12));
		using (GraphicsPath path = Bouton.Rond(new Rectangle(0, 0, base.Width, base.Height), Zx.P(14)))
		{
			base.Region = new Region(path);
		}
		z = new ZoneDessin();
		z.Dock = DockStyle.Fill;
		z.Dessiner = Dessiner;
		base.Controls.Add(z);
		z.MouseMove += delegate(object o, MouseEventArgs e)
		{
			souris = e.Location;
			bool flag = false;
			foreach (KeyValuePair<Rectangle, Action> clic in clics)
			{
				if (clic.Key.Contains(e.Location))
				{
					flag = true;
				}
			}
			z.Cursor = (flag ? Cursors.Hand : Cursors.Default);
			z.Invalidate();
		};
		z.MouseLeave += delegate
		{
			souris = new Point(-1, -1);
			z.Invalidate();
		};
		z.MouseClick += delegate(object o, MouseEventArgs e)
		{
			foreach (KeyValuePair<Rectangle, Action> item in new List<KeyValuePair<Rectangle, Action>>(clics))
			{
				if (item.Key.Contains(e.Location))
				{
					Sons.Clic();
					item.Value();
					z.Invalidate();
					break;
				}
			}
		};
		z.MouseDown += delegate(object o, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && !rBar.IsEmpty && rBar.Contains(e.Location))
			{
				glisse = true;
				Regle(e.X);
			}
		};
		z.MouseMove += delegate(object o, MouseEventArgs e)
		{
			if (glisse)
			{
				Regle(e.X);
			}
		};
		z.MouseUp += delegate
		{
			glisse = false;
		};
		base.Deactivate += delegate
		{
			Close();
		};
		base.KeyPreview = true;
		base.KeyDown += delegate(object o, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				Close();
			}
		};
		tm = new Timer();
		tm.Interval = 500;
		tm.Tick += delegate
		{
			z.Invalidate();
		};
		tm.Start();
		base.FormClosed += delegate
		{
			tm.Stop();
		};
	}

	private void Dessiner(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		clics.Clear();
		using (Pen pen = new Pen(C.Ligne))
		{
			using GraphicsPath path = Bouton.Rond(new Rectangle(0, 0, w - 1, h - 1), Zx.P(14));
			g.DrawPath(pen, path);
		}
		Lang.DrawText(g, "TPS Groupe", fT, new Point(Zx.P(16), Zx.P(13)), C.Texte);
		int num = Lang.MeasureText("Ouvrir le lanceur", fB).Width + Zx.P(24);
		Rectangle rectangle = new Rectangle(w - Zx.P(14) - num, Zx.P(9), num, Zx.P(28));
		bool flag = rectangle.Contains(souris);
		using (GraphicsPath path2 = Bouton.Rond(rectangle, Zx.P(14)))
		{
			using SolidBrush brush = new SolidBrush(flag ? C.Jade : Color.FromArgb(60, C.Jade));
			g.FillPath(brush, path2);
		}
		Lang.DrawText(g, "Ouvrir le lanceur", fB, rectangle, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		clics.Add(new KeyValuePair<Rectangle, Action>(rectangle, delegate
		{
			Close();
			if (ouvrir != null)
			{
				ouvrir();
			}
		}));
		if (quitter != null)
		{
			Rectangle rectangle2 = new Rectangle(rectangle.X - Zx.P(8) - Zx.P(30), Zx.P(9), Zx.P(30), Zx.P(28));
			bool flag2 = rectangle2.Contains(souris);
			using (GraphicsPath path3 = Bouton.Rond(rectangle2, Zx.P(14)))
			{
				using SolidBrush brush2 = new SolidBrush(flag2 ? C.Rouge : Color.FromArgb(50, C.Rouge));
				g.FillPath(brush2, path3);
			}
			Lang.DrawText(g, "\ue7e8", fG, rectangle2, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			clics.Add(new KeyValuePair<Rectangle, Action>(rectangle2, delegate
			{
				Close();
				quitter();
			}));
		}
		using (Pen pen2 = new Pen(Color.FromArgb(60, C.Ligne)))
		{
			g.DrawLine(pen2, Zx.P(14), Zx.P(46), w - Zx.P(14), Zx.P(46));
		}
		string text = Musique.EnCours();
		string lecteur = Musique.Lecteur;
		string mode = Musique.Mode;
		bool flag3 = text.Length > 0;
		Color color = ((lecteur == "Spotify" || (lecteur.Length == 0 && mode != "apple")) ? Color.FromArgb(30, 185, 84) : Color.FromArgb(250, 45, 85));
		Rectangle rectangle3 = new Rectangle(Zx.P(16), Zx.P(58), Zx.P(40), Zx.P(40));
		using (GraphicsPath path4 = Bouton.Rond(rectangle3, Zx.P(9)))
		{
			using LinearGradientBrush brush3 = new LinearGradientBrush(rectangle3, color, ControlPaint.Dark(color, 0.3f), 45f);
			g.FillPath(brush3, path4);
		}
		Lang.DrawText(g, "\uec4f", fG, rectangle3, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		string t = text;
		string t2 = "";
		int num2 = text.IndexOf(" - ");
		if (num2 > 0)
		{
			t2 = text.Substring(0, num2);
			t = text.Substring(num2 + 3);
		}
		if (!flag3)
		{
			t = ((lecteur.Length > 0) ? (lecteur + " est en pause") : "Aucune musique en cours");
			t2 = ((lecteur.Length > 0) ? "Appuyez sur lecture" : ("Ouvrez " + Musique.NomMode));
		}
		Lang.DrawText(g, t, fT, new Rectangle(Zx.P(66), Zx.P(60), w - Zx.P(82), Zx.P(20)), flag3 ? C.Texte : C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		Lang.DrawText(g, t2, fS, new Rectangle(Zx.P(66), Zx.P(80), w - Zx.P(82), Zx.P(18)), flag3 ? color : C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		int num3 = Zx.P(116);
		int num4 = w / 2;
		Action<Rectangle, string, Font, Color, bool, Action> action = delegate(Rectangle rb, string gl, Font f, Color c, bool plein, Action a)
		{
			bool flag4 = rb.Contains(souris);
			using (SolidBrush brush7 = new SolidBrush((!plein) ? Color.FromArgb(flag4 ? 80 : 36, c) : (flag4 ? ControlPaint.Light(c, 0.15f) : c)))
			{
				g.FillEllipse(brush7, rb);
			}
			using (Pen pen3 = new Pen(Color.FromArgb(150, c)))
			{
				g.DrawEllipse(pen3, rb);
			}
			Lang.DrawText(g, gl, f, rb, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			clics.Add(new KeyValuePair<Rectangle, Action>(rb, a));
		};
		action(new Rectangle(num4 - Zx.P(25), num3, Zx.P(50), Zx.P(50)), flag3 ? "\ue769" : "\ue768", fG2, C.Orange, arg5: true, delegate
		{
			Musique.Basculer();
		});
		action(new Rectangle(num4 - Zx.P(25) - Zx.P(12) - Zx.P(40), num3 + Zx.P(5), Zx.P(40), Zx.P(40)), "\ue892", fG, color, arg5: false, delegate
		{
			Musique.Commande("precedent");
		});
		action(new Rectangle(num4 + Zx.P(25) + Zx.P(12), num3 + Zx.P(5), Zx.P(40), Zx.P(40)), "\ue893", fG, color, arg5: false, delegate
		{
			Musique.Commande("suivant");
		});
		action(new Rectangle(num4 - Zx.P(25) - Zx.P(12) - Zx.P(40) - Zx.P(14) - Zx.P(34), num3 + Zx.P(8), Zx.P(34), Zx.P(34)), "\ue8a7", fG, C.Gris, arg5: false, delegate
		{
			Close();
			Musique.AfficherLecteur();
		});
		action(new Rectangle(num4 + Zx.P(25) + Zx.P(12) + Zx.P(40) + Zx.P(14), num3 + Zx.P(8), Zx.P(34), Zx.P(34)), "\ue767", fG, volOuvert ? C.Jade : C.Gris, volOuvert, delegate
		{
			Volume(!volOuvert);
		});
		rBar = Rectangle.Empty;
		if (!volOuvert)
		{
			return;
		}
		float num5 = VolumeWin.Lire();
		Rectangle rectangle4 = new Rectangle(Zx.P(52), Zx.P(190), w - Zx.P(52) - Zx.P(66), Zx.P(8));
		rBar = Rectangle.Inflate(rectangle4, Zx.P(6), Zx.P(10));
		Lang.DrawText(g, (num5 <= 0.001f) ? "\ue74f" : "\ue767", fG, new Rectangle(Zx.P(16), Zx.P(180), Zx.P(28), Zx.P(28)), C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		using (GraphicsPath path5 = Bouton.Rond(rectangle4, Zx.P(4)))
		{
			using SolidBrush brush4 = new SolidBrush(Color.FromArgb(60, 255, 255, 255));
			g.FillPath(brush4, path5);
		}
		int num6 = (int)((float)rectangle4.Width * Math.Max(0f, num5));
		if (num6 > 4)
		{
			using GraphicsPath path6 = Bouton.Rond(new Rectangle(rectangle4.X, rectangle4.Y, num6, rectangle4.Height), Zx.P(4));
			using LinearGradientBrush brush5 = new LinearGradientBrush(new Rectangle(rectangle4.X, rectangle4.Y, Math.Max(Zx.P(2), num6), rectangle4.Height), color, C.Orange, 0f);
			g.FillPath(brush5, path6);
		}
		if (num5 >= 0f)
		{
			using (SolidBrush brush6 = new SolidBrush(Color.White))
			{
				g.FillEllipse(brush6, rectangle4.X + num6 - Zx.P(7), rectangle4.Y - Zx.P(3), Zx.P(14), Zx.P(14));
			}
			Lang.DrawText(g, (int)Math.Round(num5 * 100f) + " %", fS, new Rectangle(rectangle4.Right + Zx.P(10), Zx.P(182), Zx.P(50), Zx.P(24)), C.Texte, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		}
	}
}
