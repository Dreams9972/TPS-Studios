using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

internal class PuceCompte : Control
{
	private Compte c;

	private bool survol;

	public Color Remplissage = Color.FromArgb(190, C.Cote);

	public PuceCompte()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		SetStyle(ControlStyles.StandardDoubleClick, value: false);
		Cursor = Cursors.Hand;
		BackColor = Color.Transparent;
	}

	public void Maj(Compte x)
	{
		c = x;
		Invalidate();
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
		using (GraphicsPath path = Bouton.Rond(new Rectangle(0, 0, base.Width - 1, base.Height - 1), Zx.P(10)))
		{
			using (SolidBrush brush = new SolidBrush(survol ? C.CarteSurvol : Remplissage))
			{
				graphics.FillPath(brush, path);
			}
			using Pen pen = new Pen(survol ? C.Jade : C.Ligne);
			graphics.DrawPath(pen, path);
		}
		Rectangle rectangle = new Rectangle(Zx.P(7), Zx.P(7), base.Height - Zx.P(15), base.Height - Zx.P(15));
		if (c != null)
		{
			Comptes.Avatar(graphics, c, rectangle, 11f);
			Lang.DrawText(graphics, c.Pseudo, F.Nom, new Rectangle(base.Height, Zx.P(5), base.Width - base.Height - Zx.P(8), Zx.P(22)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
			Lang.DrawText(graphics, "Mon profil", F.Etat, new Point(base.Height, Zx.P(27)), C.Gris);
			return;
		}
		using (SolidBrush brush2 = new SolidBrush(C.Ligne))
		{
			graphics.FillEllipse(brush2, rectangle);
		}
		Lang.DrawText(graphics, "+", new Font("Segoe UI Semibold", Zx.P(13f)), rectangle, C.Texte, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		Lang.DrawText(graphics, "Créer mon profil", F.Nom, new Point(base.Height, (base.Height - Zx.P(22)) / 2), C.Texte);
	}
}
