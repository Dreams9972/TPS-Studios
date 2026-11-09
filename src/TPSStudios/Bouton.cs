using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

internal class Bouton : Control, ILisse
{
	public Color Couleur = C.Orange;

	public bool Plein = true;

	public bool Danger;

	private bool survol;

	private bool enfonce;

	private float hv;

	private float cible;

	private Ressort ech = new Ressort
	{
		V = 1f
	};

	private Bitmap tampon;

	private float CibleEch
	{
		get
		{
			if (!enfonce || !base.Enabled)
			{
				return 1f;
			}
			if (base.Width >= 70)
			{
				return 0.95f;
			}
			return 0.9f;
		}
	}

	public static Color Mel(Color a, Color b, float t)
	{
		return Color.FromArgb((int)((float)(int)a.A + (float)(b.A - a.A) * t), (int)((float)(int)a.R + (float)(b.R - a.R) * t), (int)((float)(int)a.G + (float)(b.G - a.G) * t), (int)((float)(int)a.B + (float)(b.B - a.B) * t));
	}

	private void Vise(bool on)
	{
		cible = (on ? 1f : 0f);
		if (!Fenetre.Fluide)
		{
			hv = cible;
			Invalidate();
		}
		else
		{
			Lisse.Demande(this);
		}
	}

	private void Presse()
	{
		if (!Fenetre.Fluide)
		{
			ech.V = 1f;
			Invalidate();
		}
		else
		{
			Lisse.Demande(this);
			Invalidate();
		}
	}

	public bool Pas()
	{
		if (base.IsDisposed)
		{
			return false;
		}
		hv += (cible - hv) * 0.22f;
		bool flag = Math.Abs(cible - hv) < 0.02f;
		if (flag)
		{
			hv = cible;
		}
		bool result = ech.Pas(CibleEch, enfonce ? 0.32f : 0.2f, enfonce ? 0.55f : 0.68f);
		Invalidate();
		if (flag)
		{
			return result;
		}
		return true;
	}

	protected override void Dispose(bool d)
	{
		if (d && tampon != null)
		{
			tampon.Dispose();
			tampon = null;
		}
		base.Dispose(d);
	}

	public Bouton(string t)
	{
		Text = t;
		Cursor = Cursors.Hand;
		Font = new Font("Segoe UI Semibold", Zx.P(10f));
		base.TabStop = true;
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		BackColor = Color.Transparent;
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		survol = true;
		Vise(on: true);
		base.OnMouseEnter(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		survol = false;
		if (enfonce)
		{
			enfonce = false;
			Presse();
		}
		Vise(on: false);
		base.OnMouseLeave(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			enfonce = true;
			Presse();
			if (base.Enabled)
			{
				Sons.Clic();
			}
		}
		base.OnMouseDown(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		enfonce = false;
		Presse();
		base.OnMouseUp(e);
	}

	protected override void OnEnabledChanged(EventArgs e)
	{
		Invalidate();
		base.OnEnabledChanged(e);
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

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (base.Enabled && (e.KeyCode == Keys.Space || e.KeyCode == Keys.Return))
		{
			OnClick(EventArgs.Empty);
			e.Handled = true;
		}
		base.OnKeyDown(e);
	}

	protected override bool IsInputKey(Keys k)
	{
		if (k != Keys.Return)
		{
			return base.IsInputKey(k);
		}
		return true;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (base.Width >= 6 && base.Height >= 6)
		{
			Echelle.Peindre(e.Graphics, this, ech.V, Peint, ref tampon);
		}
	}

	private void Peint(Graphics g)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.PixelOffsetMode = PixelOffsetMode.HighQuality;
		int num = Math.Max(Zx.P(4), Math.Min(Zx.P(10), base.Height / 3));
		RectangleF r = new RectangleF(0.5f, 0.5f, (float)base.Width - 1.5f, (float)base.Height - 1.5f);
		Color c = Color.White;
		int y = 0;
		using (GraphicsPath path = RondF(r, num))
		{
			if (!base.Enabled)
			{
				using (SolidBrush brush = new SolidBrush(Color.FromArgb(30, 36, 60)))
				{
					g.FillPath(brush, path);
				}
				using (Pen pen = new Pen(Color.FromArgb(48, 56, 90)))
				{
					g.DrawPath(pen, path);
				}
				c = Color.FromArgb(118, 126, 158);
			}
			else if (Plein)
			{
				Color color = (Danger ? C.Rouge : Couleur);
				Color color2 = (enfonce ? ControlPaint.Dark(color, 0.14f) : ControlPaint.Light(color, 0.16f + 0.16f * hv));
				Color color3 = (enfonce ? ControlPaint.Dark(color, 0.32f) : Mel(ControlPaint.Dark(color, 0.16f), color, hv));
				using (LinearGradientBrush brush2 = new LinearGradientBrush(new RectangleF(0f, 0f, base.Width, base.Height + 1), color2, color3, 90f))
				{
					g.FillPath(brush2, path);
				}
				GraphicsState gs = g.Save();
				g.SetClip(path, CombineMode.Intersect);
				using (LinearGradientBrush brush3 = new LinearGradientBrush(new RectangleF(0f, 0f, base.Width, (float)base.Height * 0.55f + 1f), Color.FromArgb(enfonce ? 10 : ((int)(46f + 30f * hv)), 255, 255, 255), Color.FromArgb(0, 255, 255, 255), 90f))
				{
					g.FillRectangle(brush3, 0f, 0f, base.Width, (float)base.Height * 0.55f);
				}
				g.Restore(gs);
				using (Pen pen2 = new Pen(Color.FromArgb(enfonce ? 70 : ((int)(110f + 70f * hv)), ControlPaint.Light(color, 0.55f)), 1f))
				{
					g.DrawPath(pen2, path);
				}
			}
			else
			{
				Color color4 = (enfonce ? Color.FromArgb(16, 20, 44) : Mel(Color.FromArgb(32, 37, 72), Color.FromArgb(46, 54, 102), hv));
				Color color5 = (enfonce ? Color.FromArgb(13, 16, 36) : Mel(Color.FromArgb(20, 24, 50), Color.FromArgb(32, 38, 78), hv));
				using (LinearGradientBrush brush4 = new LinearGradientBrush(new RectangleF(0f, 0f, base.Width, base.Height + 1), color4, color5, 90f))
				{
					g.FillPath(brush4, path);
				}
				Color color6 = (Danger ? Color.FromArgb((int)(140f + 115f * hv), C.Rouge) : Mel(Color.FromArgb(64, 74, 124), Color.FromArgb(150, 166, 255), hv));
				using (Pen pen4 = new Pen(color6, 1f))
				{
					g.DrawPath(pen4, path);
				}
				if (!enfonce)
				{
					GraphicsState gs2 = g.Save();
					g.SetClip(path, CombineMode.Intersect);
					using (LinearGradientBrush brush5 = new LinearGradientBrush(new RectangleF(0f, 0f, base.Width, (float)base.Height * 0.5f + 1f), Color.FromArgb((int)(14f + 18f * hv), 255, 255, 255), Color.FromArgb(0, 255, 255, 255), 90f))
					{
						g.FillRectangle(brush5, 0f, 0f, base.Width, (float)base.Height * 0.5f);
					}
					g.Restore(gs2);
				}
				c = (Danger ? Color.FromArgb(245, 128, 118) : Mel(C.Texte, Color.White, hv));
			}
		}
		if (Focused && ShowFocusCues && base.Enabled)
		{
			using GraphicsPath path3 = RondF(new RectangleF(Zx.P(2.5f), Zx.P(2.5f), (float)base.Width - 1f - Zx.P(5f), (float)base.Height - 1f - Zx.P(5f)), Math.Max(2, num - Zx.P(2)));
			using Pen pen6 = new Pen(Color.FromArgb(220, 175, 190, 255), 1.6f);
			g.DrawPath(pen6, path3);
		}
		Lang.DrawText(g, Text, Font, new Rectangle(0, y, base.Width, base.Height), c, TextFormatFlags.EndEllipsis | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
	}

	public static GraphicsPath RondF(RectangleF r, float rr)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		float num = Math.Max(2f, Math.Min(rr * 2f, Math.Min(r.Width, r.Height)));
		graphicsPath.AddArc(r.X, r.Y, num, num, 180f, 90f);
		graphicsPath.AddArc(r.Right - num, r.Y, num, num, 270f, 90f);
		graphicsPath.AddArc(r.Right - num, r.Bottom - num, num, num, 0f, 90f);
		graphicsPath.AddArc(r.X, r.Bottom - num, num, num, 90f, 90f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	public static GraphicsPath Rond(Rectangle r, int rr)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		int num = Math.Max(2, Math.Min(rr * 2, Math.Min(r.Width, r.Height)));
		graphicsPath.AddArc(r.X, r.Y, num, num, 180f, 90f);
		graphicsPath.AddArc(r.Right - num, r.Y, num, num, 270f, 90f);
		graphicsPath.AddArc(r.Right - num, r.Bottom - num, num, num, 0f, 90f);
		graphicsPath.AddArc(r.X, r.Bottom - num, num, num, 90f, 90f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}
}
