using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

internal class Cadre : Panel
{
	public string Titre = "";

	public Cadre()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		SetStyle(ControlStyles.StandardDoubleClick, value: false);
		BackColor = C.Carte;
	}

	protected override void OnPaintBackground(PaintEventArgs e)
	{
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.Clear((base.Parent != null) ? base.Parent.BackColor : C.Fond);
		Rectangle r = new Rectangle(0, 0, base.Width - 1, base.Height - 1);
		if (r.Width < 4 || r.Height < 4)
		{
			return;
		}
		using (GraphicsPath path = Bouton.Rond(r, Zx.P(12)))
		{
			using (SolidBrush brush = new SolidBrush(C.Carte))
			{
				graphics.FillPath(brush, path);
			}
			using Pen pen = new Pen(C.Ligne);
			graphics.DrawPath(pen, path);
		}
		if (Titre.Length > 0)
		{
			using (SolidBrush brush2 = new SolidBrush(C.Jade))
			{
				graphics.FillRectangle(brush2, Zx.P(18), Zx.P(19), Zx.P(3), Zx.P(14));
			}
			Lang.DrawText(graphics, Lang.T(Titre).ToUpperInvariant(), F.Rubrique, new Point(Zx.P(28), Zx.P(17)), C.Gris);
		}
	}
}
