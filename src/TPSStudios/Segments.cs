using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

internal class Segments : Control, ILisse
{
	public string[] Items = new string[0];

	private int idx;

	private int survolI = -1;

	public bool Etire;

	public int MinW = Zx.P(70);

	private Ressort px = new Ressort
	{
		V = -1f
	};

	private Ressort pw = default(Ressort);

	private static readonly Font fS = new Font("Segoe UI Semibold", Zx.P(9.5f));

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
				if (Fenetre.Fluide && px.V >= 0f && base.IsHandleCreated && base.Visible)
				{
					Lisse.Demande(this);
				}
				else
				{
					px.V = -1f;
				}
				Invalidate();
				if (this.Changed != null)
				{
					this.Changed(this, EventArgs.Empty);
				}
			}
		}
	}

	public event EventHandler Changed;

	private void Cible(out float x, out float w0)
	{
		int[] array = Largeurs();
		int num = 1;
		x = -1f;
		w0 = 0f;
		for (int i = 0; i < array.Length; i++)
		{
			if (i == idx)
			{
				x = num + Zx.P(2);
				w0 = array[i] - 4;
			}
			num += array[i];
		}
	}

	public bool Pas()
	{
		if (base.IsDisposed)
		{
			return false;
		}
		Cible(out var x, out var w);
		bool flag = px.Pas(x, 0.2f, 0.62f);
		bool result = pw.Pas(w, 0.2f, 0.62f);
		Invalidate();
		if (!flag)
		{
			return result;
		}
		return true;
	}

	public Segments()
	{
		base.Height = Zx.P(36);
		Cursor = Cursors.Hand;
		base.TabStop = true;
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
	}

	private int[] Largeurs()
	{
		int[] array = new int[Items.Length];
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Math.Max(MinW, Lang.MeasureText(Items[i], fS).Width + ((MinW < 50) ? 8 : 30));
			num += array[i];
		}
		if (Etire && num < base.Width - 2 && array.Length > 0)
		{
			int num2 = (base.Width - 2 - num) / array.Length;
			for (int j = 0; j < array.Length; j++)
			{
				array[j] += num2;
			}
		}
		return array;
	}

	private int Pos(int x)
	{
		int[] array = Largeurs();
		int num = 1;
		for (int i = 0; i < array.Length; i++)
		{
			if (x >= num && x < num + array[i])
			{
				return i;
			}
			num += array[i];
		}
		return -1;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		int num = Pos(e.X);
		if (num != survolI)
		{
			survolI = num;
			Invalidate();
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		survolI = -1;
		Invalidate();
		base.OnMouseLeave(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		int num = Pos(e.X);
		if (num >= 0)
		{
			if (num != Index)
			{
				Sons.Clic();
			}
			Index = num;
		}
		Focus();
		base.OnMouseDown(e);
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
		if (e.KeyCode == Keys.Left && idx > 0)
		{
			Index = idx - 1;
			e.Handled = true;
		}
		else if (e.KeyCode == Keys.Right && idx < Items.Length - 1)
		{
			Index = idx + 1;
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
		int[] array = Largeurs();
		int num = 2;
		int[] array2 = array;
		foreach (int num2 in array2)
		{
			num += num2;
		}
		Rectangle r = new Rectangle(0, 0, Math.Min(base.Width - 1, num), base.Height - 1);
		using (GraphicsPath path = Bouton.Rond(r, Zx.P(9)))
		{
			using (SolidBrush brush = new SolidBrush(C.Champ))
			{
				graphics.FillPath(brush, path);
			}
			using Pen pen = new Pen((Focused && ShowFocusCues) ? C.Jade : C.Ligne);
			graphics.DrawPath(pen, path);
		}
		int num3 = 1;
		Cible(out var x, out var w);
		if (px.V < 0f || x < 0f)
		{
			px.V = x;
			pw.V = w;
			px.Vit = 0f;
			pw.Vit = 0f;
		}
		if (idx >= 0 && idx < array.Length && pw.V > 4f)
		{
			RectangleF r2 = new RectangleF(px.V, Zx.P(3), pw.V, base.Height - Zx.P(7));
			using GraphicsPath path2 = Bouton.RondF(r2, 7f);
			using LinearGradientBrush brush2 = new LinearGradientBrush(new RectangleF(r2.X, r2.Y, r2.Width, r2.Height + 1f), ControlPaint.Light(C.JadeFonce, 0.1f), C.JadeFonce, 90f);
			graphics.FillPath(brush2, path2);
		}
		for (int j = 0; j < array.Length; j++)
		{
			Rectangle r3 = new Rectangle(num3 + Zx.P(2), Zx.P(3), array[j] - Zx.P(4), base.Height - Zx.P(7));
			if (j != idx && j == survolI)
			{
				using GraphicsPath path3 = Bouton.Rond(r3, Zx.P(7));
				using SolidBrush brush3 = new SolidBrush(Color.FromArgb(34, 255, 255, 255));
				graphics.FillPath(brush3, path3);
			}
			Lang.DrawText(graphics, Items[j], fS, r3, (j == idx) ? Color.White : ((j == survolI) ? C.Texte : Color.FromArgb(176, 190, 210)), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			num3 += array[j];
		}
	}
}
