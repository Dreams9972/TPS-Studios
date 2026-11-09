using System;
using System.Drawing;
using System.Windows.Forms;

namespace TPSStudios;

internal class NavItem : Control
{
	public string Glyphe;

	public string Texte;

	public bool Actif;

	private bool survol;

	private static readonly Font fG = new Font("Segoe MDL2 Assets", Zx.P(13f));

	private static readonly Font fT = new Font("Segoe UI Semibold", Zx.P(10.5f));

	public NavItem(string glyphe, string texte)
	{
		Glyphe = glyphe;
		Texte = texte;
		base.Size = new Size(Zx.P(260), Zx.P(42));
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
		graphics.Clear(Actif ? C.Carte : (survol ? Color.FromArgb(14, 25, 45) : C.Cote));
		if (Actif)
		{
			using SolidBrush brush = new SolidBrush(C.Jade);
			graphics.FillRectangle(brush, 0, 0, Zx.P(4), base.Height);
		}
		Lang.DrawText(graphics, Glyphe, fG, new Rectangle(Zx.P(14), 0, Zx.P(34), base.Height), Actif ? C.Jade : C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		Lang.DrawText(graphics, Texte, fT, new Rectangle(Zx.P(56), 0, base.Width - Zx.P(60), base.Height), Actif ? C.Texte : Color.FromArgb(190, 202, 220), TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
	}
}
