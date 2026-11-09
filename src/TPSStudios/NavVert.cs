using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

internal class NavVert : Control
{
	private const int HL = 44;

	public string[] Items = new string[0];

	public string[] Glyphes = new string[0];

	private int idx;

	private int survol = -1;

	private static readonly Font fN = new Font("Segoe UI Semibold", Zx.P(10f));

	private static readonly Font fG = new Font("Segoe MDL2 Assets", Zx.P(12f));

	public int Index
	{
		get
		{
			return idx;
		}
		set
		{
			if (idx != value)
			{
				idx = value;
				Invalidate();
				if (this.Changed != null)
				{
					this.Changed(this, EventArgs.Empty);
				}
			}
		}
	}

	public event EventHandler Changed;

	public NavVert()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		Cursor = Cursors.Hand;
		BackColor = C.Fond;
	}

	private int Sous(int y)
	{
		int num = y / 48;
		if (num < 0 || num >= Items.Length || y % 48 >= 44)
		{
			return -1;
		}
		return num;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		int num = Sous(e.Y);
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
		int num = Sous(e.Y);
		if (num >= 0)
		{
			if (num != Index)
			{
				Sons.Clic();
			}
			Index = num;
		}
		base.OnMouseDown(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.Clear(BackColor);
		for (int i = 0; i < Items.Length; i++)
		{
			Rectangle r = new Rectangle(0, i * (44 + Zx.P(4)), base.Width - 1, 44);
			bool flag = i == idx;
			if (flag || i == survol)
			{
				using GraphicsPath path = Bouton.Rond(r, Zx.P(10));
				using SolidBrush brush = new SolidBrush(flag ? C.CarteSurvol : Color.FromArgb(20, 255, 255, 255));
				graphics.FillPath(brush, path);
			}
			if (flag)
			{
				using GraphicsPath path2 = Bouton.Rond(new Rectangle(0, r.Y + Zx.P(11), Zx.P(4), 44 - Zx.P(22)), Zx.P(2));
				using SolidBrush brush2 = new SolidBrush(C.Orange);
				graphics.FillPath(brush2, path2);
			}
			if (i < Glyphes.Length)
			{
				Lang.DrawText(graphics, Glyphes[i], fG, new Rectangle(Zx.P(14), r.Y, Zx.P(26), 44), flag ? C.Texte : C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			}
			Lang.DrawText(graphics, Items[i], fN, new Rectangle(Zx.P(50), r.Y, base.Width - Zx.P(58), 44), flag ? C.Texte : Color.FromArgb(176, 190, 210), TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		}
	}
}
