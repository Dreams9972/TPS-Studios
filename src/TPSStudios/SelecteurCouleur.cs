using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TPSStudios;

internal class SelecteurCouleur : Control
{
	private double h;

	private double s = 1.0;

	private double v = 1.0;

	private Bitmap sv;

	private int svH = -1;

	public readonly Champ Hex;

	private int glisse;

	private bool maj;

	private Rectangle rSV;

	private Rectangle rHue;

	private Rectangle rSw;

	private Rectangle rPre;

	public static readonly Color[] Presets = new Color[16]
	{
		Color.FromArgb(86, 106, 255),
		Color.FromArgb(38, 120, 230),
		Color.FromArgb(235, 150, 30),
		Color.FromArgb(215, 70, 60),
		Color.FromArgb(150, 90, 220),
		Color.FromArgb(230, 90, 160),
		Color.FromArgb(90, 170, 60),
		Color.FromArgb(70, 190, 220),
		Color.FromArgb(255, 255, 255),
		Color.FromArgb(200, 205, 215),
		Color.FromArgb(120, 130, 150),
		Color.FromArgb(60, 70, 90),
		Color.FromArgb(20, 25, 40),
		Color.FromArgb(240, 200, 60),
		Color.FromArgb(255, 120, 80),
		Color.FromArgb(120, 220, 160)
	};

	public Color Couleur
	{
		get
		{
			return FromHsv(h, s, v);
		}
		set
		{
			ToHsv(value, out var num, out var num2, out var num3);
			h = num;
			s = num2;
			v = num3;
			MajHex();
			Invalidate();
		}
	}

	public event EventHandler Changed;

	public SelecteurCouleur()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		Hex = new Champ(multi: false, Zx.P(34));
		Hex.Width = Zx.P(116);
		Hex.MaxLength = 7;
		base.Controls.Add(Hex);
		base.Height = Zx.P(236);
		Champ hex = Hex;
		EventHandler value = delegate
		{
			if (!maj)
			{
				Match match = Regex.Match(Hex.Text.Trim(), "^#?([0-9a-fA-F]{6})$");
				if (match.Success)
				{
					int num = Convert.ToInt32(match.Groups[1].Value, 16);
					Color c = Color.FromArgb((num >> 16) & 0xFF, (num >> 8) & 0xFF, num & 0xFF);
					ToHsv(c, out var num2, out var num3, out var num4);
					h = num2;
					s = num3;
					v = num4;
					Invalidate();
					if (this.Changed != null)
					{
						this.Changed(this, EventArgs.Empty);
					}
				}
			}
		};
		hex.TextChanged += value;
	}

	public static Color FromHsv(double h, double s, double v)
	{
		h = (h % 360.0 + (double)Zx.P(360)) % 360.0;
		int num = (int)(h / 60.0) % 6;
		double num2 = h / 60.0 - Math.Floor(h / 60.0);
		double num3 = v * 255.0;
		int num4 = (int)Math.Round(num3);
		int num5 = (int)Math.Round(num3 * (1.0 - s));
		int num6 = (int)Math.Round(num3 * (1.0 - num2 * s));
		int num7 = (int)Math.Round(num3 * (1.0 - (1.0 - num2) * s));
		return num switch
		{
			0 => Color.FromArgb(num4, num7, num5), 
			1 => Color.FromArgb(num6, num4, num5), 
			2 => Color.FromArgb(num5, num4, num7), 
			3 => Color.FromArgb(num5, num6, num4), 
			4 => Color.FromArgb(num7, num5, num4), 
			_ => Color.FromArgb(num4, num5, num6), 
		};
	}

	public static void ToHsv(Color c, out double h, out double s, out double v)
	{
		double num = (double)(int)c.R / 255.0;
		double num2 = (double)(int)c.G / 255.0;
		double num3 = (double)(int)c.B / 255.0;
		double num4 = Math.Max(num, Math.Max(num2, num3));
		double num5 = Math.Min(num, Math.Min(num2, num3));
		double num6 = num4 - num5;
		v = num4;
		s = ((num4 == 0.0) ? 0.0 : (num6 / num4));
		if (num6 == 0.0)
		{
			h = 0.0;
		}
		else if (num4 == num)
		{
			h = (double)Zx.P(60) * ((num2 - num3) / num6 % 6.0);
		}
		else if (num4 == num2)
		{
			h = (double)Zx.P(60) * ((num3 - num) / num6 + (double)Zx.P(2));
		}
		else
		{
			h = (double)Zx.P(60) * ((num - num2) / num6 + (double)Zx.P(4));
		}
		if (h < 0.0)
		{
			h += Zx.P(360);
		}
	}

	private void MajHex()
	{
		maj = true;
		Color couleur = Couleur;
		Hex.Text = "#" + couleur.R.ToString("X2") + couleur.G.ToString("X2") + couleur.B.ToString("X2");
		maj = false;
	}

	protected override void OnResize(EventArgs e)
	{
		rSV = new Rectangle(0, 0, Math.Max(Zx.P(40), base.Width), Zx.P(128));
		rHue = new Rectangle(0, Zx.P(140), Math.Max(Zx.P(40), base.Width), Zx.P(14));
		rSw = new Rectangle(0, Zx.P(166), Zx.P(34), Zx.P(34));
		rPre = new Rectangle(0, Zx.P(208), Math.Max(Zx.P(40), base.Width), Zx.P(24));
		if (Hex != null)
		{
			Hex.Location = new Point(Zx.P(44), Zx.P(166));
		}
		base.OnResize(e);
	}

	private void Regenerer()
	{
		if (sv == null)
		{
			sv = new Bitmap(96, 64);
		}
		for (int i = 0; i < 64; i++)
		{
			for (int j = 0; j < 96; j++)
			{
				sv.SetPixel(j, i, FromHsv(h, (double)j / 95.0, 1.0 - (double)i / 63.0));
			}
		}
		svH = (int)h;
	}

	private void Evt()
	{
		MajHex();
		Invalidate();
		if (this.Changed != null)
		{
			this.Changed(this, EventArgs.Empty);
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Left)
		{
			base.OnMouseDown(e);
			return;
		}
		Rectangle rectangle = rHue;
		rectangle.Inflate(0, 6);
		if (rSV.Contains(e.Location))
		{
			glisse = 1;
		}
		else
		{
			if (!rectangle.Contains(e.Location))
			{
				if (rPre.Contains(e.Location))
				{
					int num = e.X * Presets.Length / Math.Max(1, base.Width);
					if (num >= 0 && num < Presets.Length)
					{
						Couleur = Presets[num];
						if (this.Changed != null)
						{
							this.Changed(this, EventArgs.Empty);
						}
					}
				}
				else
				{
					base.OnMouseDown(e);
				}
				return;
			}
			glisse = 2;
		}
		Deplacer(e.Location);
		base.OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (glisse > 0)
		{
			Deplacer(e.Location);
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		glisse = 0;
		base.OnMouseUp(e);
	}

	private void Deplacer(Point p)
	{
		if (glisse == 1)
		{
			s = Math.Max(0.0, Math.Min(1.0, (double)(p.X - rSV.X) / (double)rSV.Width));
			v = 1.0 - Math.Max(0.0, Math.Min(1.0, (double)(p.Y - rSV.Y) / (double)rSV.Height));
		}
		else if (glisse == 2)
		{
			h = Math.Max(0.0, Math.Min(1.0, (double)p.X / (double)base.Width)) * 359.9;
		}
		Evt();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.Clear(BackColor);
		if (sv == null || svH != (int)h)
		{
			Regenerer();
		}
		using (GraphicsPath path = Bouton.Rond(rSV, Zx.P(8)))
		{
			GraphicsState gstate = graphics.Save();
			graphics.SetClip(path, CombineMode.Intersect);
			graphics.InterpolationMode = InterpolationMode.Bilinear;
			graphics.PixelOffsetMode = PixelOffsetMode.Half;
			graphics.DrawImage(sv, rSV);
			graphics.Restore(gstate);
			using Pen pen = new Pen(C.Ligne);
			graphics.DrawPath(pen, path);
		}
		int num = rSV.X + (int)(s * (double)rSV.Width);
		int num2 = rSV.Y + (int)((1.0 - v) * (double)rSV.Height);
		using (Pen pen2 = new Pen(Color.Black, Zx.P(3f)))
		{
			graphics.DrawEllipse(pen2, num - Zx.P(7), num2 - Zx.P(7), Zx.P(14), Zx.P(14));
		}
		using (Pen pen3 = new Pen(Color.White, Zx.P(2f)))
		{
			graphics.DrawEllipse(pen3, num - Zx.P(7), num2 - Zx.P(7), Zx.P(14), Zx.P(14));
		}
		using (GraphicsPath path2 = Bouton.Rond(rHue, Zx.P(7)))
		{
			ColorBlend colorBlend = new ColorBlend(7);
			colorBlend.Positions = new float[7]
			{
				0f,
				1f / 6f,
				1f / 3f,
				0.5f,
				2f / 3f,
				5f / 6f,
				1f
			};
			colorBlend.Colors = new Color[7]
			{
				Color.Red,
				Color.Yellow,
				Color.Lime,
				Color.Cyan,
				Color.Blue,
				Color.Magenta,
				Color.Red
			};
			using LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new Rectangle(rHue.X, rHue.Y, Math.Max(1, rHue.Width), rHue.Height), Color.Red, Color.Red, 0f);
			linearGradientBrush.InterpolationColors = colorBlend;
			graphics.FillPath(linearGradientBrush, path2);
		}
		int num3 = (int)(h / 359.9 * (double)base.Width);
		using (SolidBrush brush = new SolidBrush(Couleur))
		{
			graphics.FillEllipse(brush, num3 - Zx.P(8), rHue.Y - Zx.P(3), Zx.P(16), Zx.P(20));
		}
		using (Pen pen4 = new Pen(Color.White, Zx.P(2f)))
		{
			graphics.DrawEllipse(pen4, num3 - Zx.P(8), rHue.Y - Zx.P(3), Zx.P(16), Zx.P(20));
		}
		using (GraphicsPath path3 = Bouton.Rond(rSw, Zx.P(8)))
		{
			using (SolidBrush brush2 = new SolidBrush(Couleur))
			{
				graphics.FillPath(brush2, path3);
			}
			using Pen pen5 = new Pen(C.Ligne);
			graphics.DrawPath(pen5, path3);
		}
		int num4 = base.Width / Presets.Length;
		for (int i = 0; i < Presets.Length; i++)
		{
			Rectangle rect = new Rectangle(i * num4 + Zx.P(2), rPre.Y + Zx.P(2), num4 - Zx.P(4), num4 - Zx.P(4));
			if (rect.Width < 6)
			{
				continue;
			}
			using (SolidBrush brush3 = new SolidBrush(Presets[i]))
			{
				graphics.FillEllipse(brush3, rect);
			}
			if (Presets[i].ToArgb() == Couleur.ToArgb())
			{
				using Pen pen6 = new Pen(Color.White, Zx.P(2f));
				graphics.DrawEllipse(pen6, Rectangle.Inflate(rect, Zx.P(2), Zx.P(2)));
			}
			else
			{
				using Pen pen7 = new Pen(Color.FromArgb(70, 255, 255, 255));
				graphics.DrawEllipse(pen7, rect);
			}
		}
	}
}
