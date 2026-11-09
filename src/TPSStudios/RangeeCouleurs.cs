using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

internal class RangeeCouleurs : Control
{
	public Color[] Couleurs = new Color[0];

	private int survol = -1;

	public event Action<Color> Choisie;

	public RangeeCouleurs()
	{
		base.Height = Zx.P(40);
		Cursor = Cursors.Hand;
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
	}

	private int Pos(int x)
	{
		int num = (x - 4) / 42;
		if (num < 0 || num >= Couleurs.Length || x < 4)
		{
			return -1;
		}
		return num;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		int num = Pos(e.X);
		if (num != survol)
		{
			survol = num;
			Invalidate();
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		survol = -1;
		Invalidate();
		base.OnMouseLeave(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		int num = Pos(e.X);
		if (num >= 0 && this.Choisie != null)
		{
			this.Choisie(Couleurs[num]);
		}
		base.OnMouseDown(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.Clear(BackColor);
		if (Couleurs.Length == 0)
		{
			Lang.DrawText(graphics, "Importez une photo de profil pour voir ses couleurs.", new Font("Segoe UI", Zx.P(9f)), new Point(Zx.P(4), Zx.P(10)), C.Gris);
			return;
		}
		for (int i = 0; i < Couleurs.Length; i++)
		{
			Rectangle rectangle = new Rectangle(Zx.P(4) + i * Zx.P(42), Zx.P(4), Zx.P(32), Zx.P(32));
			using (SolidBrush brush = new SolidBrush(Couleurs[i]))
			{
				graphics.FillEllipse(brush, rectangle);
			}
			using Pen pen = new Pen((i == survol) ? Color.White : Color.FromArgb(80, 255, 255, 255), (i == survol) ? Zx.P(2f) : 1f);
			graphics.DrawEllipse(pen, (i == survol) ? Rectangle.Inflate(rectangle, Zx.P(2), Zx.P(2)) : rectangle);
		}
	}
}
