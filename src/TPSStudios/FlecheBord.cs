using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

/// <summary>Petit bouton flèche translucide collé au bord (Bibliothèque ⇄ tous les mods).</summary>
internal class FlecheBord : Control
{
	public bool Droite = true;

	/// <summary>Texte affiché à côté de la flèche (vide : flèche seule).</summary>
	public string Libelle = "";

	/// <summary>Largeur de la pastille de la flèche ; le reste de la largeur sert au texte.</summary>
	public int LargeurFleche = Zx.P(30);

	public static readonly Font PoliceLibelle = new Font("Segoe UI Semibold", Zx.P(11f));

	private bool survol;

	public FlecheBord()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw, value: true);
		SetStyle(ControlStyles.StandardDoubleClick, value: false);
		BackColor = Color.Transparent;
		Cursor = Cursors.Hand;
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
		int lf = Math.Min(LargeurFleche, base.Width);
		Rectangle r = new Rectangle(Droite ? (base.Width - lf) : 0, 0, lf - 1, base.Height - 1);
		if (lf < base.Width && !string.IsNullOrEmpty(Libelle))
		{
			Rectangle rt = (Droite ? new Rectangle(0, 0, r.X - Zx.P(10), base.Height) : new Rectangle(r.Right + Zx.P(10), 0, base.Width - r.Right - Zx.P(10), base.Height));
			Lang.DrawText(graphics, Libelle, PoliceLibelle, rt, Color.FromArgb(survol ? 255 : 205, 255, 255, 255), TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | (Droite ? TextFormatFlags.Right : TextFormatFlags.Left));
		}
		using (GraphicsPath path = Bouton.Rond(r, Math.Min(lf / 2, Zx.P(14))))
		{
			using (SolidBrush brush = new SolidBrush(Color.FromArgb(survol ? 90 : 38, 255, 255, 255)))
			{
				graphics.FillPath(brush, path);
			}
			using Pen pen = new Pen(Color.FromArgb(survol ? 120 : 60, 255, 255, 255));
			graphics.DrawPath(pen, path);
		}
		int num = r.X + r.Width / 2;
		int num2 = base.Height / 2;
		int num3 = Zx.P(5);
		int num4 = Zx.P(9);
		Point[] points = (Droite ? new Point[3]
		{
			new Point(num - num3 / 2 - 1, num2 - num4),
			new Point(num + num3, num2),
			new Point(num - num3 / 2 - 1, num2 + num4)
		} : new Point[3]
		{
			new Point(num + num3 / 2 + 1, num2 - num4),
			new Point(num - num3, num2),
			new Point(num + num3 / 2 + 1, num2 + num4)
		});
		using Pen pen2 = new Pen(Color.FromArgb(survol ? 255 : 200, 255, 255, 255), Zx.P(2.4f));
		pen2.StartCap = LineCap.Round;
		pen2.EndCap = LineCap.Round;
		pen2.LineJoin = LineJoin.Round;
		graphics.DrawLines(pen2, points);
	}
}
