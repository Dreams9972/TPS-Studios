using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TPSStudios;

internal class Champ : Control
{
	public readonly TextBox Box = new TextBox();

	private bool focus;

	public override string Text
	{
		get
		{
			return Box.Text;
		}
		set
		{
			Box.Text = value;
		}
	}

	public int MaxLength
	{
		get
		{
			return Box.MaxLength;
		}
		set
		{
			Box.MaxLength = value;
		}
	}

	public bool ReadOnly
	{
		get
		{
			return Box.ReadOnly;
		}
		set
		{
			Box.ReadOnly = value;
			Box.BackColor = (value ? Color.FromArgb(16, 28, 50) : C.Champ);
			Box.ForeColor = (value ? C.Gris : C.Texte);
			Invalidate();
		}
	}

	public bool Mdp
	{
		set
		{
			Box.UseSystemPasswordChar = value;
		}
	}

	public string Indice
	{
		set
		{
			string v = Lang.T(value);
			Box.HandleCreated += delegate
			{
				if (!Box.Multiline)
				{
					SendMessage(Box.Handle, 5377, (IntPtr)1, v);
				}
			};
		}
	}

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern IntPtr SendMessage(IntPtr h, int msg, IntPtr w, string l);

	public Champ(bool multi, int hauteur)
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		Box.BorderStyle = BorderStyle.None;
		Box.BackColor = C.Champ;
		Box.ForeColor = C.Texte;
		Box.Font = new Font("Segoe UI", Zx.P(10f));
		Box.Multiline = multi;
		if (multi)
		{
			Box.ScrollBars = ScrollBars.Vertical;
			Box.AcceptsReturn = true;
			Theme.Sombre(Box);
		}
		Box.GotFocus += delegate
		{
			focus = true;
			Invalidate();
		};
		Box.LostFocus += delegate
		{
			focus = false;
			Invalidate();
		};
		Box.TextChanged += delegate
		{
			OnTextChanged(EventArgs.Empty);
		};
		base.Controls.Add(Box);
		base.Height = hauteur;
		Cursor = Cursors.IBeam;
	}

	public new bool Focus()
	{
		return Box.Focus();
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		Box.Focus();
		base.OnMouseDown(e);
	}

	protected override void OnResize(EventArgs e)
	{
		if (Box.Multiline)
		{
			Box.SetBounds(Zx.P(12), Zx.P(10), Math.Max(Zx.P(10), base.Width - Zx.P(24)), Math.Max(Zx.P(10), base.Height - Zx.P(20)));
		}
		else
		{
			Box.SetBounds(Zx.P(12), Math.Max(0, (base.Height - Box.Height) / 2), Math.Max(Zx.P(10), base.Width - Zx.P(24)), Box.Height);
		}
		base.OnResize(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.Clear(BackColor);
		Rectangle r = new Rectangle(0, 0, base.Width - 1, base.Height - 1);
		if (r.Width < 4 || r.Height < 4)
		{
			return;
		}
		using GraphicsPath path = Bouton.Rond(r, Zx.P(8));
		using (SolidBrush brush = new SolidBrush(ReadOnly ? Color.FromArgb(16, 28, 50) : C.Champ))
		{
			graphics.FillPath(brush, path);
		}
		using Pen pen = new Pen(focus ? C.Jade : C.Ligne, focus ? 1.5f : 1f);
		graphics.DrawPath(pen, path);
	}
}
