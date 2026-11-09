using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

internal class Barre : Control
{
	public double Valeur;

	public Barre()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		SetStyle(ControlStyles.StandardDoubleClick, value: false);
		base.Height = 8;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.Clear(base.Parent.BackColor);
		using (GraphicsPath path = Bouton.Rond(new Rectangle(0, 0, base.Width - 1, base.Height - 1), Zx.P(3)))
		{
			using SolidBrush brush = new SolidBrush(C.Ligne);
			graphics.FillPath(brush, path);
		}
		if (Valeur <= 0.0)
		{
			return;
		}
		int num = (int)Math.Max(Zx.P(8), (double)(base.Width - 1) * Math.Min(1.0, Valeur));
		using GraphicsPath path2 = Bouton.Rond(new Rectangle(0, 0, num, base.Height - 1), Zx.P(3));
		using LinearGradientBrush brush2 = new LinearGradientBrush(new Rectangle(0, 0, num + 1, base.Height), C.Jade, Color.FromArgb(150, 168, 255), 0f);
		graphics.FillPath(brush2, path2);
	}
}
