using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

internal class Entree : Control
{
	public Projet P;

	public bool Choisi;

	public string Etat = "";

	public Color Pastille = C.Gris;

	public Image Vignette;

	private bool survol;

	public Entree()
	{
		base.Height = Zx.P(64);
		Cursor = Cursors.Hand;
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		SetStyle(ControlStyles.StandardDoubleClick, value: false);
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		survol = true;
		Invalidate();
		base.OnMouseEnter(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		survol = false;
		Invalidate();
		base.OnMouseLeave(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		graphics.Clear(Choisi ? C.Carte : (survol ? Color.FromArgb(14, 25, 45) : C.Cote));
		if (Choisi)
		{
			using SolidBrush brush = new SolidBrush(C.Jade);
			graphics.FillRectangle(brush, 0, 0, Zx.P(4), base.Height);
		}
		Rectangle rectangle = new Rectangle(Zx.P(16), Zx.P(10), Zx.P(44), Zx.P(44));
		using (GraphicsPath graphicsPath = Bouton.Rond(rectangle, Zx.P(6)))
		{
			if (Vignette != null)
			{
				graphics.SetClip(graphicsPath);
				graphics.DrawImage(Vignette, rectangle);
				graphics.ResetClip();
			}
			else
			{
				using SolidBrush brush2 = new SolidBrush(C.Ligne);
				graphics.FillPath(brush2, graphicsPath);
			}
		}
		Lang.DrawText(graphics, P.Nom, F.Nom, new Point(Zx.P(70), Zx.P(12)), C.Texte);
		using (SolidBrush brush3 = new SolidBrush(Pastille))
		{
			graphics.FillEllipse(brush3, Zx.P(71), Zx.P(39), Zx.P(8), Zx.P(8));
		}
		Lang.DrawText(graphics, Etat, F.Etat, new Point(Zx.P(82), Zx.P(34)), C.Gris);
	}
}
