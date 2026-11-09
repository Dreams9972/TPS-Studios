using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

internal class Option : Control, ILisse
{
	public string Titre = "";

	public string Aide = "";

	private bool on;

	private bool survol;

	private float pos;

	private static readonly Font fT = new Font("Segoe UI Semibold", Zx.P(10.5f));

	private static readonly Font fA = new Font("Segoe UI", Zx.P(9f));

	public bool Checked
	{
		get
		{
			return on;
		}
		set
		{
			if (on != value)
			{
				on = value;
				if (Fenetre.Fluide)
				{
					Lisse.Demande(this);
				}
				else
				{
					pos = (on ? 1f : 0f);
					Invalidate();
				}
				if (this.Changed != null)
				{
					this.Changed(this, EventArgs.Empty);
				}
			}
		}
	}

	public event EventHandler Changed;

	public bool Pas()
	{
		if (base.IsDisposed)
		{
			return false;
		}
		float num = (on ? 1f : 0f);
		pos += (num - pos) * 0.32f;
		bool flag = Math.Abs(num - pos) < 0.03f;
		if (flag)
		{
			pos = num;
		}
		Invalidate();
		return !flag;
	}

	public Option(string titre, string aide, bool val)
	{
		Titre = titre;
		Aide = aide;
		on = val;
		pos = (val ? 1f : 0f);
		base.Height = Zx.P(62);
		Cursor = Cursors.Hand;
		base.TabStop = true;
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
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

	protected override void OnClick(EventArgs e)
	{
		Sons.Clic();
		Checked = !on;
		Focus();
		base.OnClick(e);
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
		if (e.KeyCode == Keys.Space)
		{
			Checked = !on;
			e.Handled = true;
		}
		base.OnKeyDown(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.Clear(BackColor);
		Rectangle r = new Rectangle(0, Zx.P(2), base.Width - 1, base.Height - Zx.P(5));
		if (survol || (Focused && ShowFocusCues))
		{
			using GraphicsPath path = Bouton.Rond(r, Zx.P(8));
			using SolidBrush brush = new SolidBrush(Color.FromArgb(survol ? 40 : 22, 255, 255, 255));
			graphics.FillPath(brush, path);
		}
		Color c = (base.Enabled ? C.Texte : C.Gris);
		Lang.DrawText(graphics, Titre, fT, new Rectangle(Zx.P(14), Zx.P(9), base.Width - Zx.P(100), Zx.P(26)), c, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
		Lang.DrawText(graphics, Aide, fA, new Rectangle(Zx.P(14), Zx.P(36), base.Width - Zx.P(100), Zx.P(24)), C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
		Rectangle r2 = new Rectangle(base.Width - Zx.P(14) - Zx.P(46), (base.Height - Zx.P(24)) / 2, Zx.P(46), Zx.P(24));
		using (GraphicsPath path2 = Bouton.Rond(r2, Zx.P(12)))
		{
			using SolidBrush brush2 = new SolidBrush(Bouton.Mel(Color.FromArgb(52, 68, 96), C.Jade, pos));
			graphics.FillPath(brush2, path2);
		}
		int x = (int)((float)(r2.X + Zx.P(3)) + (float)(r2.Width - Zx.P(24)) * pos);
		using SolidBrush brush3 = new SolidBrush(Color.White);
		graphics.FillEllipse(brush3, x, r2.Y + Zx.P(3), Zx.P(18), Zx.P(18));
	}
}
