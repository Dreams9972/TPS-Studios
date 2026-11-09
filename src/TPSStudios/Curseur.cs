using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

internal class Curseur : Control
{
	public int Min;

	public int Max = 100;

	private int val;

	private bool glisse;

	public int Valeur
	{
		get
		{
			return val;
		}
		set
		{
			int num = Math.Max(Min, Math.Min(Max, value));
			if (num != val)
			{
				val = num;
				Invalidate();
				if (this.Changed != null)
				{
					this.Changed(this, EventArgs.Empty);
				}
			}
		}
	}

	public event EventHandler Changed;

	public Curseur()
	{
		base.Height = Zx.P(28);
		Cursor = Cursors.Hand;
		base.TabStop = true;
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
	}

	private void Pos(int x)
	{
		Valeur = Min + (int)Math.Round((double)(Max - Min) * Math.Max(0.0, Math.Min(1.0, (double)(x - 10) / (double)Math.Max(1, base.Width - 20))));
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		glisse = true;
		Focus();
		Pos(e.X);
		base.OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (glisse)
		{
			Pos(e.X);
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		glisse = false;
		base.OnMouseUp(e);
	}

	protected override bool IsInputKey(Keys k)
	{
		if (k != Keys.Left && k != Keys.Right)
		{
			return base.IsInputKey(k);
		}
		return true;
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		int num = Math.Max(1, (Max - Min) / 50);
		if (e.KeyCode == Keys.Left)
		{
			Valeur -= num;
			e.Handled = true;
		}
		else if (e.KeyCode == Keys.Right)
		{
			Valeur += num;
			e.Handled = true;
		}
		base.OnKeyDown(e);
	}

	protected override void OnGotFocus(EventArgs e)
	{
		Invalidate();
		base.OnGotFocus(e);
	}

	protected override void OnLostFocus(EventArgs e)
	{
		Invalidate();
		base.OnLostFocus(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.Clear(BackColor);
		int num = base.Height / 2;
		int num2 = Zx.P(10) + (int)((double)((base.Width - Zx.P(20)) * (val - Min)) / (double)Math.Max(1, Max - Min));
		using (GraphicsPath path = Bouton.Rond(new Rectangle(Zx.P(10), num - Zx.P(3), base.Width - Zx.P(20), Zx.P(6)), Zx.P(3)))
		{
			using SolidBrush brush = new SolidBrush(C.Champ);
			graphics.FillPath(brush, path);
		}
		if (num2 > 12)
		{
			using GraphicsPath path2 = Bouton.Rond(new Rectangle(Zx.P(10), num - Zx.P(3), num2 - Zx.P(10), Zx.P(6)), Zx.P(3));
			using SolidBrush brush2 = new SolidBrush(C.Jade);
			graphics.FillPath(brush2, path2);
		}
		using (SolidBrush brush3 = new SolidBrush(Color.White))
		{
			graphics.FillEllipse(brush3, num2 - Zx.P(8), num - Zx.P(8), Zx.P(16), Zx.P(16));
		}
		if (Focused && ShowFocusCues)
		{
			using (Pen pen = new Pen(Color.FromArgb(160, C.Jade), Zx.P(2f)))
			{
				graphics.DrawEllipse(pen, num2 - Zx.P(10), num - Zx.P(10), Zx.P(20), Zx.P(20));
			}
		}
	}
}
