using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

internal class Avis : Control
{
	public Color Couleur = C.Jade;

	private static readonly Font fa = new Font("Segoe UI Semibold", Zx.P(10.5f));

	public Avis()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		base.Visible = false;
		base.Height = Zx.P(46);
	}

	public void Poser(string t, Color c)
	{
		Text = t;
		Couleur = c;
		Size size = Lang.MeasureText(t, fa);
		base.Width = Math.Min(Zx.P(900), size.Width + Zx.P(72));
		base.Height = Zx.P(46);
		using (GraphicsPath path = Bouton.Rond(new Rectangle(0, 0, base.Width, base.Height), Zx.P(23)))
		{
			base.Region = new Region(path);
		}
		Invalidate();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.Clear(Color.FromArgb(18, 22, 46));
		using (GraphicsPath path = Bouton.Rond(new Rectangle(1, 1, base.Width - Zx.P(3), base.Height - Zx.P(3)), Zx.P(22)))
		{
			using Pen pen = new Pen(Couleur, Zx.P(2f));
			graphics.DrawPath(pen, path);
		}
		using (SolidBrush brush = new SolidBrush(Couleur))
		{
			graphics.FillEllipse(brush, Zx.P(20), base.Height / 2 - Zx.P(5), Zx.P(10), Zx.P(10));
		}
		Lang.DrawText(graphics, Text, fa, new Rectangle(Zx.P(40), 0, base.Width - Zx.P(52), base.Height), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
	}
}
