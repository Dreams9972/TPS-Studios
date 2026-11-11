using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace TPSStudios;

internal class Tuile : Control, ILisse
{
	private Ressort ech = new Ressort
	{
		V = 1f
	};

	private Bitmap tampon;

	private bool appui;

	public string Titre = "";

	public string Sous = "";

	public string Badge = "";

	public string Glyphe = "";

	public string Etiquette = "";

	public string Prix = "";

	public Color BadgeCouleur = C.Gris;

	public Color PrixCouleur = C.Vert;

	public Image Img;

	public bool Compacte;

	public bool Choisie;

	public bool Inactive;

	private bool survol;

	public string Coin = "";

	private Rectangle zoneCoin;

	private bool survolCoin;

	private static readonly Font fGc = new Font("Segoe MDL2 Assets", Zx.P(10f));

	private Bitmap cacheImg;

	private Image cacheSrc;

	private int cacheW;

	private int cacheH;

	public float Neige;

	public int IndiceDecor = -1;

	public static ThemeBase Decor;

	private bool attente;

	public static Func<bool> NeigeEnChute;

	private PointF onde;

	private float ondeT = -1f;

	private Stopwatch ondeSw;

	private Timer ondeTm;

	private static readonly Font fG = new Font("Segoe MDL2 Assets", Zx.P(22f));

	private static readonly Font fGg = new Font("Segoe MDL2 Assets", Zx.P(40f));

	private static readonly Font fT = new Font("Segoe UI Semibold", Zx.P(12f));

	private float CibleEch
	{
		get
		{
			if (!Inactive)
			{
				if (!appui)
				{
					return 1f;
				}
				return 0.975f;
			}
			return 1f;
		}
	}

	public event EventHandler ClicCoin;

	public static event Action<Tuile> Deneigee;

	private void Anime()
	{
		if (!Fenetre.Fluide)
		{
			ech.V = 1f;
			Invalidate();
		}
		else
		{
			Lisse.Demande(this);
		}
	}

	public bool Pas()
	{
		if (base.IsDisposed)
		{
			return false;
		}
		bool result = ech.Pas(CibleEch, appui ? 0.3f : 0.16f, appui ? 0.55f : 0.7f);
		Invalidate();
		return result;
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (appui)
		{
			appui = false;
			Anime();
		}
		base.OnMouseUp(e);
	}

	private void Reconstruire(Rectangle zi)
	{
		if (cacheImg != null)
		{
			cacheImg.Dispose();
		}
		Bitmap image = new Bitmap(Math.Max(1, zi.Width), Math.Max(1, zi.Height), PixelFormat.Format32bppPArgb);
		using (Graphics graphics = Graphics.FromImage(image))
		{
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			// Liste des mods : l'image s'affiche en entier (fond flou si ses proportions diffèrent de la case).
			Fenetre.DessinerAjuste(graphics, Img, new Rectangle(0, 0, zi.Width, zi.Height), 1.04, 0);
			using LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, zi.Bottom - Zx.P(60), base.Width, Zx.P(61)), Color.FromArgb(0, C.Carte), C.Carte, 90f);
			graphics.FillRectangle(brush, 0, zi.Bottom - Zx.P(60), base.Width, Zx.P(61));
		}
		cacheImg = image;
		cacheSrc = Img;
		cacheW = zi.Width;
		cacheH = zi.Height;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && cacheImg != null)
		{
			cacheImg.Dispose();
			cacheImg = null;
		}
		if (disposing && tampon != null)
		{
			tampon.Dispose();
			tampon = null;
		}
		base.Dispose(disposing);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		bool flag = Coin.Length > 0 && zoneCoin.Contains(e.Location);
		if (flag != survolCoin)
		{
			survolCoin = flag;
			Invalidate();
		}
		base.OnMouseMove(e);
	}

	protected override void OnClick(EventArgs e)
	{
		Sons.Clic();
		if (Coin.Length > 0 && zoneCoin.Contains(PointToClient(Cursor.Position)))
		{
			if (this.ClicCoin != null)
			{
				this.ClicCoin(this, e);
			}
		}
		else
		{
			if (attente)
			{
				return;
			}
			bool neige = Neige > 0.25f && Tuile.Deneigee != null;
			if (neige)
			{
				Neige = 0f;
				Invalidate();
				Tuile.Deneigee(this);
			}
			attente = true;
			Stopwatch sw = Stopwatch.StartNew();
			Timer tm = new Timer();
			tm.Interval = 16;
			tm.Tick += delegate
			{
				long elapsedMilliseconds = sw.ElapsedMilliseconds;
				if (elapsedMilliseconds >= (neige ? 160 : 40) && (!neige || elapsedMilliseconds >= 300 || NeigeEnChute == null || !NeigeEnChute()))
				{
					tm.Stop();
					tm.Dispose();
					attente = false;
					if (!base.IsDisposed)
					{
						Cliquer(e);
					}
				}
			};
			tm.Start();
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left && !Inactive)
		{
			appui = true;
			Anime();
			onde = e.Location;
			ondeT = 0f;
			ondeSw = Stopwatch.StartNew();
			if (ondeTm == null)
			{
				ondeTm = new Timer();
				ondeTm.Interval = 15;
				ondeTm.Tick += delegate
				{
					ondeT = (float)((double)ondeSw.ElapsedMilliseconds / 420.0);
					if (ondeT >= 1f)
					{
						ondeT = -1f;
						ondeTm.Stop();
					}
					Invalidate();
				};
				base.Disposed += delegate
				{
					ondeTm.Dispose();
				};
			}
			ondeTm.Start();
			Invalidate();
		}
		base.OnMouseDown(e);
	}

	private void DessinerOnde(Graphics g, GraphicsPath p)
	{
		if (ondeT < 0f)
		{
			return;
		}
		float num = 1f - (1f - ondeT) * (1f - ondeT) * (1f - ondeT);
		float num2 = (float)Math.Sqrt(base.Width * base.Width + base.Height * base.Height) * 1.05f * num;
		if (num2 < 3f)
		{
			return;
		}
		GraphicsState gstate = g.Save();
		g.SetClip(p, CombineMode.Intersect);
		using (GraphicsPath graphicsPath = new GraphicsPath())
		{
			graphicsPath.AddEllipse(onde.X - num2, onde.Y - num2, num2 * 2f, num2 * 2f);
			using PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath);
			pathGradientBrush.CenterColor = Color.FromArgb((int)(70f * (1f - ondeT)), 160, 175, 255);
			pathGradientBrush.SurroundColors = new Color[1] { Color.FromArgb((int)(20f * (1f - ondeT)), 255, 255, 255) };
			g.FillPath(pathGradientBrush, graphicsPath);
		}
		using (Pen pen = new Pen(Color.FromArgb((int)(150f * (1f - ondeT)), 200, 210, 255), Zx.P(2f)))
		{
			g.DrawEllipse(pen, onde.X - num2, onde.Y - num2, num2 * 2f, num2 * 2f);
		}
		g.Restore(gstate);
	}

	private void Cliquer(EventArgs e)
	{
		base.OnClick(e);
	}

	private void DessinerNeige(Graphics g)
	{
		float num = Math.Min(1f, Neige);
		if (num < 0.03f)
		{
			return;
		}
		g.SmoothingMode = SmoothingMode.AntiAlias;
		int width = base.Width;
		using (SolidBrush brush = new SolidBrush(Color.FromArgb((int)(230f * num), 250, 252, 255)))
		{
			g.FillRectangle(brush, Zx.P(11), 0f, width - Zx.P(22), Zx.P(2.4f));
		}
		for (int i = 0; i < 6; i++)
		{
			float num2 = 18f + (float)(width - 36) * ((float)i / 5f) + 9f * (float)Math.Sin(i * 3 + base.Top);
			float num3 = (4f + 9f * BanAnim.H(i, base.Top)) * num;
			using SolidBrush brush2 = new SolidBrush(Color.FromArgb((int)(220f * num), 210, 232, 255));
			g.FillPolygon(brush2, new PointF[3]
			{
				new PointF(num2 - Zx.P(2.4f), 1.5f),
				new PointF(num2 + Zx.P(2.4f), 1.5f),
				new PointF(num2, 1.5f + num3)
			});
		}
	}

	public Tuile()
	{
		Cursor = Cursors.Hand;
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		SetStyle(ControlStyles.StandardDoubleClick, value: false);
		BackColor = Color.Transparent;
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		survol = true;
		Anime();
		Invalidate();
		base.OnMouseEnter(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		survol = false;
		survolCoin = false;
		appui = false;
		Anime();
		Invalidate();
		base.OnMouseLeave(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Echelle.Peindre(e.Graphics, this, ech.V, Peint, ref tampon);
	}

	private void Peint(Graphics g)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.InterpolationMode = InterpolationMode.HighQualityBicubic;
		Rectangle r = new Rectangle(0, 0, base.Width - 1, base.Height - 1);
		if (!Compacte && Coin.Length > 0)
		{
			int num = Lang.MeasureText(Coin, F.Etat).Width + Zx.P(44);
			zoneCoin = new Rectangle(base.Width - Zx.P(16) - num, base.Height - Zx.P(38), num, Zx.P(28));
		}
		bool flag = survol && !Inactive;
		using (GraphicsPath graphicsPath = Bouton.Rond(r, Zx.P(10)))
		{
			using (SolidBrush brush = new SolidBrush(flag ? C.CarteSurvol : C.Carte))
			{
				g.FillPath(brush, graphicsPath);
			}
			if (Compacte)
			{
				Rectangle r2 = new Rectangle(Zx.P(16), (base.Height - Zx.P(52)) / 2, Zx.P(52), Zx.P(52));
				using (GraphicsPath path = Bouton.Rond(r2, Zx.P(10)))
				{
					using SolidBrush brush2 = new SolidBrush(Color.FromArgb(Inactive ? 30 : 60, C.Jade));
					g.FillPath(brush2, path);
				}
				Lang.DrawText(g, Glyphe, fG, r2, C.Jade, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
				Lang.DrawText(g, Titre, fT, new Rectangle(Zx.P(84), base.Height / 2 - Zx.P(26), base.Width - Zx.P(96), Zx.P(24)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
				Lang.DrawText(g, Sous, F.Etat, new Rectangle(Zx.P(84), base.Height / 2 + Zx.P(2), base.Width - Zx.P(96), Zx.P(20)), C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
			}
			else
			{
				int num2 = Math.Max(Zx.P(40), base.Height - Zx.P(116));
				Rectangle rectangle = new Rectangle(0, 0, base.Width, num2);
				g.SetClip(graphicsPath);
				if (Img != null)
				{
					if (cacheImg == null || cacheSrc != Img || cacheW != rectangle.Width || cacheH != rectangle.Height)
					{
						Reconstruire(rectangle);
					}
					g.SetClip(rectangle, CombineMode.Intersect);
					g.DrawImageUnscaled(cacheImg, 0, 0);
				}
				else
				{
					using (LinearGradientBrush brush3 = new LinearGradientBrush(rectangle, C.Ligne, C.Carte, 60f))
					{
						g.FillRectangle(brush3, rectangle);
					}
					Lang.DrawText(g, Glyphe, fGg, rectangle, Color.FromArgb(140, C.Gris), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
				}
				g.ResetClip();
				if (Etiquette.Length > 0)
				{
					Size size = Lang.MeasureText(Etiquette, F.Etat);
					Rectangle r3 = new Rectangle(Zx.P(12), Zx.P(12), size.Width + Zx.P(18), Zx.P(24));
					using (GraphicsPath path2 = Bouton.Rond(r3, Zx.P(10)))
					{
						using SolidBrush brush4 = new SolidBrush(Color.FromArgb(200, C.Fond));
						g.FillPath(brush4, path2);
					}
					Lang.DrawText(g, Etiquette, F.Etat, r3, C.Texte, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
				}
				if (Prix.Length > 0)
				{
					Size size2 = Lang.MeasureText(Prix, F.Etat);
					Rectangle r4 = new Rectangle(base.Width - Zx.P(12) - size2.Width - Zx.P(20), Zx.P(12), size2.Width + Zx.P(20), Zx.P(24));
					using (GraphicsPath path3 = Bouton.Rond(r4, Zx.P(10)))
					{
						using SolidBrush brush5 = new SolidBrush(Color.FromArgb(235, PrixCouleur));
						g.FillPath(brush5, path3);
					}
					Lang.DrawText(g, Prix, F.Etat, r4, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
				}
				Lang.DrawText(g, Titre, fT, new Rectangle(Zx.P(16), num2 + Zx.P(10), base.Width - Zx.P(32), Zx.P(24)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
				Lang.DrawText(g, Sous, F.Etat, new Rectangle(Zx.P(16), num2 + Zx.P(36), base.Width - Zx.P(32), Zx.P(34)), C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
				if (Badge.Length > 0)
				{
					Size size3 = Lang.MeasureText(Badge, F.Etat);
					Rectangle r5 = new Rectangle(Zx.P(16), base.Height - Zx.P(34), size3.Width + Zx.P(20), Zx.P(22));
					using (GraphicsPath path4 = Bouton.Rond(r5, Zx.P(10)))
					{
						using SolidBrush brush6 = new SolidBrush(Color.FromArgb(46, BadgeCouleur));
						g.FillPath(brush6, path4);
					}
					Lang.DrawText(g, Badge, F.Etat, r5, BadgeCouleur, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
				}
			}
			if (!Compacte && Coin.Length > 0)
			{
				using (GraphicsPath path5 = Bouton.Rond(zoneCoin, Zx.P(8)))
				{
					using (SolidBrush brush7 = new SolidBrush(survolCoin ? C.JadeFonce : C.Fond))
					{
						g.FillPath(brush7, path5);
					}
					using Pen pen = new Pen(survolCoin ? C.Jade : C.Ligne);
					g.DrawPath(pen, path5);
				}
				Lang.DrawText(g, "\ue713", fGc, new Rectangle(zoneCoin.X + Zx.P(6), zoneCoin.Y, Zx.P(24), zoneCoin.Height), survolCoin ? Color.White : C.Jade, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
				Lang.DrawText(g, Coin, F.Etat, new Rectangle(zoneCoin.X + Zx.P(30), zoneCoin.Y, zoneCoin.Width - Zx.P(34), zoneCoin.Height), survolCoin ? Color.White : C.Texte, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			}
			if (Inactive)
			{
				using SolidBrush brush8 = new SolidBrush(Color.FromArgb(90, C.Fond));
				g.FillPath(brush8, graphicsPath);
			}
			if (flag || Choisie)
			{
				using GraphicsPath path6 = Bouton.Rond(new Rectangle(Zx.P(2), Zx.P(2), base.Width - Zx.P(5), base.Height - Zx.P(5)), Zx.P(9));
				using Pen pen2 = new Pen(Color.FromArgb(flag ? 70 : 45, C.Jade), Zx.P(3f));
				g.DrawPath(pen2, path6);
			}
			using (Pen pen3 = new Pen(Choisie ? C.Jade : (flag ? C.Jade : C.Ligne), Choisie ? Zx.P(2f) : 1f))
			{
				g.DrawPath(pen3, graphicsPath);
			}
			if (IndiceDecor >= 0 && Decor != null)
			{
				try
				{
					Decor.DecorTuile(g, IndiceDecor, base.Width, base.Height);
				}
				catch (Exception e)
				{
					Journal.Une("decor", e);
				}
			}
			try
			{
				DessinerOnde(g, graphicsPath);
			}
			catch (Exception e2)
			{
				Journal.Une("onde", e2);
			}
		}
		if (Neige > 0.03f)
		{
			try
			{
				DessinerNeige(g);
			}
			catch (Exception e3)
			{
				Journal.Une("neige", e3);
			}
		}
	}
}
