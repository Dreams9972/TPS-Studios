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
		SetStyle(ControlStyles.StandardDoubleClick, value: false);
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

	/// <summary>
	/// Style moderne : boutons principaux en couleur pleine et unie (sans reflet ni contour), boutons secondaires
	/// en verre translucide avec un liseré fin ; coins très arrondis (pastille pour les petits boutons).
	/// </summary>
	private void Peint(Graphics g)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.PixelOffsetMode = PixelOffsetMode.HighQuality;
		float num = ((base.Height <= Zx.P(44)) ? ((float)base.Height / 2f - 1f) : (float)Zx.P(14));
		RectangleF r = new RectangleF(0.5f, 0.5f, (float)base.Width - 1.5f, (float)base.Height - 1.5f);
		Color c = Color.White;
		using (GraphicsPath path = RondF(r, num))
		{
			if (!base.Enabled)
			{
				using (SolidBrush brush = new SolidBrush(Color.FromArgb(14, 255, 255, 255)))
				{
					g.FillPath(brush, path);
				}
				c = Color.FromArgb(110, 120, 150);
			}
			else if (Plein)
			{
				Color color = (Danger ? C.Rouge : Couleur);
				Color fond = (enfonce ? ControlPaint.Dark(color, 0.12f) : Mel(color, ControlPaint.Light(color, 0.18f), hv));
				using (SolidBrush brush2 = new SolidBrush(fond))
				{
					g.FillPath(brush2, path);
				}
				if (!enfonce && hv > 0.01f)
				{
					// légère lueur au survol
					using Pen lueur = new Pen(Color.FromArgb((int)(70f * hv), ControlPaint.Light(color, 0.6f)), 1f);
					g.DrawPath(lueur, path);
				}
			}
			else
			{
				int a = (enfonce ? 10 : ((int)(16f + 18f * hv)));
				using (SolidBrush brush4 = new SolidBrush(Danger ? Color.FromArgb(a + 6, C.Rouge) : Color.FromArgb(a, 255, 255, 255)))
				{
					g.FillPath(brush4, path);
				}
				Color liseré = (Danger ? Color.FromArgb((int)(90f + 120f * hv), C.Rouge) : Color.FromArgb((int)(34f + 40f * hv), 255, 255, 255));
				using (Pen pen4 = new Pen(liseré, 1f))
				{
					g.DrawPath(pen4, path);
				}
				c = (Danger ? Color.FromArgb(250, 140, 130) : Mel(Color.FromArgb(222, 228, 245), Color.White, hv));
			}
		}
		if (Focused && ShowFocusCues && base.Enabled)
		{
			using GraphicsPath path3 = RondF(new RectangleF(Zx.P(2.5f), Zx.P(2.5f), (float)base.Width - 1f - Zx.P(5f), (float)base.Height - 1f - Zx.P(5f)), Math.Max(2f, num - (float)Zx.P(2)));
			using Pen pen6 = new Pen(Color.FromArgb(200, 175, 190, 255), 1.4f);
			g.DrawPath(pen6, path3);
		}
		Lang.DrawText(g, Text, Font, new Rectangle(Zx.P(6), 0, base.Width - Zx.P(12), base.Height), c, TextFormatFlags.EndEllipsis | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
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
