using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

internal class PaletteCouleurs : Control
{
	public int Index;

	public int Cell = 40;

	public event EventHandler Changed;

	public PaletteCouleurs()
	{
		base.Size = new Size(Zx.P(8) * Zx.P(40), Zx.P(38));
		Cursor = Cursors.Hand;
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		SetStyle(ControlStyles.StandardDoubleClick, value: false);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.Clear((base.Parent != null) ? base.Parent.BackColor : C.Fond);
		for (int i = 0; i < Comptes.Palette.Length; i++)
		{
			int num = Math.Min(28, Cell - 5);
			Rectangle rect = new Rectangle(i * Cell + (Cell - num) / 2, (base.Height - num) / 2, num, num);
			using (SolidBrush brush = new SolidBrush(Comptes.Palette[i]))
			{
				graphics.FillEllipse(brush, rect);
			}
			if (i == Index)
			{
				using Pen pen = new Pen(Color.White, Zx.P(2f));
				rect.Inflate(3, 3);
				graphics.DrawEllipse(pen, rect);
			}
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		int num = e.X / Cell;
		if (num >= 0 && num < Comptes.Palette.Length)
		{
			Index = num;
			Invalidate();
			if (this.Changed != null)
			{
				this.Changed(this, EventArgs.Empty);
			}
		}
		base.OnMouseDown(e);
	}
}
