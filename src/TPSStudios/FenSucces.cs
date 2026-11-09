using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenSucces : Form, IPage
{
	private Compte c;

	private ZoneDessin haut;

	private ZoneDessin cartes;

	private Panel defil;

	private Timer anim;

	private int CW;

	private int X0;

	private int cols = 3;

	private static readonly Font fTitreS = new Font("Segoe UI Semibold", Zx.P(22f));

	private static readonly Font fNomS = new Font("Segoe UI Semibold", Zx.P(11f));

	private static readonly Font fDescS = new Font("Segoe UI", Zx.P(9f));

	private static readonly Font fGlyS = new Font("Segoe MDL2 Assets", Zx.P(17f));

	private static readonly Font fPetitS = new Font("Segoe UI", Zx.P(8.5f));

	private static readonly Font fNivS = new Font("Segoe UI Semibold", Zx.P(13f));

	public FenSucces(Compte compte)
	{
		c = compte;
		Progression.Init(c);
		Progression.Verifier(c);
		base.FormBorderStyle = FormBorderStyle.None;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		Size size = (base.ClientSize = Integre.Hote);
		CW = Math.Min(size.Width, Zx.P(1240));
		X0 = (size.Width - CW) / 2;
		haut = new ZoneDessin();
		haut.Dock = DockStyle.Top;
		haut.Height = Zx.P(214);
		haut.Dessiner = DessinerHaut;
		base.Controls.Add(haut);
		Control.ControlCollection controls = haut.Controls;
		EventHandler h = delegate
		{
			base.DialogResult = DialogResult.OK;
		};
		controls.Add(Integre.Retour(h, X0 + Zx.P(24), Zx.P(20)));
		defil = new Panel();
		defil.Dock = DockStyle.Fill;
		defil.AutoScroll = true;
		defil.BackColor = C.Fond;
		base.Controls.Add(defil);
		defil.BringToFront();
		Theme.Sombre(defil);
		cartes = new ZoneDessin();
		cartes.Location = new Point(X0, 0);
		cartes.Dessiner = DessinerCartes;
		defil.Controls.Add(cartes);
		Dispose_Layout();
		defil.Resize += delegate
		{
			Dispose_Layout();
		};
		anim = new Timer();
		anim.Interval = 40;
		anim.Tick += delegate
		{
			haut.Invalidate();
		};
		anim.Start();
		base.Disposed += delegate
		{
			anim.Stop();
			anim.Dispose();
		};
	}

	private void Dispose_Layout()
	{
		cols = ((CW >= 960) ? 3 : 2);
		int num = (Progression.Defs.Count + cols - 1) / cols;
		cartes.Width = CW - SystemInformation.VerticalScrollBarWidth;
		cartes.Height = Zx.P(16) + num * Zx.P(124) + Zx.P(24);
	}

	private void DessinerHaut(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.Clear(C.Fond);
		using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, w, h + 1), Color.FromArgb(24, 28, 62), C.Fond, 90f))
		{
			g.FillRectangle(brush, 0, 0, w, h);
		}
		Progression.Niveau(c.Xp, out var niv, out var dans, out var besoin);
		double temps = BanAnim.Temps;
		Progression.Palier(niv);
		Lang.DrawText(g, "Succès", fTitreS, new Point(X0 + Zx.P(140), Zx.P(22)), C.Texte);
		Lang.DrawText(g, c.Succes.Count + " sur " + Progression.Defs.Count + " débloqués", fDescS, new Point(X0 + Zx.P(142), Zx.P(62)), C.Gris);
		Badge.Dessiner(g, new Rectangle(X0 + Zx.P(28), Zx.P(84), Zx.P(112), Zx.P(112)), niv, temps);
		int num = X0 + Zx.P(160);
		Lang.DrawText(g, "Niveau " + niv + "  ·  " + Progression.Titre(niv), fNivS, new Point(num, Zx.P(92)), C.Texte);
		Rectangle r = new Rectangle(num, Zx.P(124), Math.Min(Zx.P(460), CW - Zx.P(560)), Zx.P(12));
		double num2 = ((niv >= 100) ? 1.0 : ((double)dans / (double)besoin));
		using (GraphicsPath path = Bouton.Rond(r, Zx.P(6)))
		{
			using SolidBrush brush2 = new SolidBrush(Color.FromArgb(60, 255, 255, 255));
			g.FillPath(brush2, path);
		}
		int num3 = (int)((double)r.Width * num2);
		if (num3 > 8)
		{
			using GraphicsPath path2 = Bouton.Rond(new Rectangle(r.X, r.Y, num3, r.Height), Zx.P(6));
			using LinearGradientBrush brush3 = new LinearGradientBrush(new Rectangle(r.X, r.Y, Math.Max(Zx.P(2), num3), r.Height), C.Jade, C.Orange, 0f);
			g.FillPath(brush3, path2);
		}
		Lang.DrawText(g, (niv >= 100) ? ("Niveau maximum atteint · " + c.Xp + " XP") : (dans + " / " + besoin + " XP pour le niveau " + (niv + 1)), fDescS, new Point(num, Zx.P(142)), C.Gris);
		Lang.DrawText(g, "Série : " + c.Serie + " jour" + ((c.Serie > 1) ? "s" : "") + "  ·  record " + c.SerieMax, fDescS, new Point(num, Zx.P(162)), C.Gris);
		int num4 = 46;
		int num5 = Zx.P(10);
		int num6 = 10 * num4 + 9 * num5;
		int num7 = X0 + CW - Zx.P(24) - num6;
		if (num7 < num + 300)
		{
			num7 = num + Zx.P(300);
		}
		for (int i = 0; i < 10; i++)
		{
			Rectangle rectangle = new Rectangle(num7 + i * (num4 + num5), Zx.P(100), num4, num4);
			bool flag = niv >= i * 10 + 1;
			if (flag)
			{
				Badge.Dessiner(g, rectangle, i * 10 + Math.Min(10, niv - i * 10), temps);
			}
			else
			{
				Badge.Verrou(g, rectangle);
			}
			Lang.DrawText(g, "Niv. " + (i * 10 + 1), fPetitS, new Rectangle(rectangle.X - Zx.P(8), rectangle.Bottom + Zx.P(2), rectangle.Width + Zx.P(16), Zx.P(16)), flag ? C.Texte : C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
		}
	}

	private void DessinerCartes(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
		g.Clear(C.Fond);
		int num = (CW - Zx.P(48) - SystemInformation.VerticalScrollBarWidth - (cols - 1) * Zx.P(16)) / cols;
		List<DefSucces> list = Progression.Vitrine(c);
		for (int i = 0; i < list.Count; i++)
		{
			DefSucces defSucces = list[i];
			bool flag = c.Succes.ContainsKey(defSucces.Id);
			int num2 = Math.Min(defSucces.Objectif, Math.Max(0, defSucces.Valeur(c)));
			Rectangle r = new Rectangle(Zx.P(24) + i % cols * (num + Zx.P(16)), Zx.P(16) + i / cols * Zx.P(124), num, Zx.P(108));
			using (GraphicsPath path = Bouton.Rond(r, Zx.P(14)))
			{
				using (SolidBrush brush = new SolidBrush(flag ? Color.FromArgb(34, 38, 78) : C.Carte))
				{
					g.FillPath(brush, path);
				}
				using Pen pen = new Pen(flag ? Color.FromArgb(200, C.Orange) : C.Ligne, flag ? 1.6f : 1f);
				g.DrawPath(pen, path);
			}
			Rectangle rectangle = new Rectangle(r.X + Zx.P(16), r.Y + Zx.P(18), Zx.P(54), Zx.P(54));
			using (LinearGradientBrush brush2 = new LinearGradientBrush(rectangle, flag ? ControlPaint.Light(C.Orange, 0.1f) : Color.FromArgb(46, 52, 90), flag ? ControlPaint.Dark(C.Orange, 0.2f) : Color.FromArgb(28, 32, 62), 60f))
			{
				g.FillEllipse(brush2, rectangle);
			}
			Lang.DrawText(g, flag ? "\ue73e" : defSucces.Glyphe, fGlyS, rectangle, flag ? Color.White : C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			int num3 = rectangle.Right + Zx.P(14);
			int num4 = r.Right - num3 - Zx.P(14);
			Lang.DrawText(g, defSucces.Nom, fNomS, new Rectangle(num3, r.Y + Zx.P(14), num4 - Zx.P(60), Zx.P(22)), flag ? C.Texte : Color.FromArgb(200, 206, 226), TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
			string t = "+" + defSucces.Xp + " XP";
			Size size = Lang.MeasureText(t, fPetitS);
			Rectangle r2 = new Rectangle(r.Right - Zx.P(14) - size.Width - Zx.P(16), r.Y + Zx.P(14), size.Width + Zx.P(16), Zx.P(20));
			using (GraphicsPath path2 = Bouton.Rond(r2, Zx.P(10)))
			{
				using SolidBrush brush3 = new SolidBrush(Color.FromArgb(flag ? 70 : 40, flag ? C.Vert : C.Jade));
				g.FillPath(brush3, path2);
			}
			Lang.DrawText(g, t, fPetitS, r2, flag ? C.Vert : Color.FromArgb(170, 184, 255), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			Lang.DrawText(g, defSucces.Desc, fDescS, new Rectangle(num3, r.Y + Zx.P(40), num4, Zx.P(18)), C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
			if (flag)
			{
				Lang.DrawText(g, "Débloqué le " + c.Succes[defSucces.Id], fPetitS, new Point(num3, r.Y + Zx.P(78)), C.Vert);
				continue;
			}
			Rectangle r3 = new Rectangle(num3, r.Y + Zx.P(70), num4 - Zx.P(70), Zx.P(8));
			using (GraphicsPath path3 = Bouton.Rond(r3, Zx.P(4)))
			{
				using SolidBrush brush4 = new SolidBrush(Color.FromArgb(60, 255, 255, 255));
				g.FillPath(brush4, path3);
			}
			int num5 = (int)((double)r3.Width * ((double)num2 / (double)defSucces.Objectif));
			if (num5 > 4)
			{
				using GraphicsPath path4 = Bouton.Rond(new Rectangle(r3.X, r3.Y, num5, r3.Height), Zx.P(4));
				using SolidBrush brush5 = new SolidBrush(C.Jade);
				g.FillPath(brush5, path4);
			}
			Lang.DrawText(g, num2 + " / " + defSucces.Objectif, fPetitS, new Rectangle(r3.Right + Zx.P(8), r.Y + Zx.P(64), Zx.P(60), Zx.P(18)), C.Gris, TextFormatFlags.NoPadding);
		}
	}
}
