using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TPSStudios;

/// <summary>
/// Barre de titre intégrée : la barre Windows classique disparaît (le contenu monte jusqu'en haut de la fenêtre),
/// les boutons réduire / agrandir / fermer sont dessinés dans le style du lanceur, et on déplace la fenêtre en
/// tirant le haut de n'importe quelle page. Les bords gardent le redimensionnement et l'ancrage de Windows (Aero Snap).
/// </summary>
internal partial class Fenetre : IMessageFilter
{
	private const int WM_NCCALCSIZE = 0x83;
	private const int WM_NCLBUTTONDOWN = 0xA1;
	private const int WM_LBUTTONDOWN = 0x201;
	private const int WM_LBUTTONDBLCLK = 0x203;
	private const int HTCAPTION = 2;
	private const int HTTOP = 12;

	[StructLayout(LayoutKind.Sequential)]
	private struct RECT
	{
		public int left, top, right, bottom;
	}

	[DllImport("user32.dll")]
	private static extern bool ReleaseCapture();

	[DllImport("user32.dll", EntryPoint = "SendMessageW")]
	private static extern IntPtr EnvoyerMsg(IntPtr h, int msg, IntPtr w, IntPtr l);

	[DllImport("user32.dll")]
	private static extern int GetSystemMetrics(int i);

	[DllImport("user32.dll")]
	private static extern bool SetWindowPos(IntPtr h, IntPtr apres, int x, int y, int cx, int cy, uint f);

	/// <summary>
	/// Barre Windows retirée (sauf sous Mono pour les tests, ou si le joueur a choisi la barre classique dans
	/// Réglages › Affichage : solution de repli si l'intégration pose problème sur un PC).
	/// </summary>
	private static readonly bool sansBarreWindows = Type.GetType("Mono.Runtime") == null && Config.Lire("barre_classique") != "1";

	private BarreTitre barreTitre;

	/// <summary>Hauteur de la bande du haut qui sert à déplacer la fenêtre.</summary>
	private static int HautDeplacement => Zx.P(22);

	private void InstallerBarreTitre()
	{
		barreTitre = new BarreTitre(this);
		base.Controls.Add(barreTitre);
		PlacerBarreTitre();
		base.Resize += delegate
		{
			PlacerBarreTitre();
		};
		Application.AddMessageFilter(this);
		base.FormClosed += delegate
		{
			Application.RemoveMessageFilter(this);
		};
	}

	internal void PlacerBarreTitre()
	{
		if (barreTitre == null)
		{
			return;
		}
		barreTitre.Visible = !plein && (sansBarreWindows || Type.GetType("Mono.Runtime") != null);
		barreTitre.Location = new Point(base.ClientSize.Width - barreTitre.Width, 0);
		barreTitre.Invalidate();
		barreTitre.BringToFront();
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		if (sansBarreWindows)
		{
			// recalcul du cadre : la zone de la barre Windows devient du contenu
			SetWindowPos(base.Handle, IntPtr.Zero, 0, 0, 0, 0, 0x0027u);
		}
	}

	/// <summary>WM_NCCALCSIZE : on garde les bords (redimensionnement, ombre, ancrage) mais plus la barre de titre.</summary>
	private bool TraiterCadre(ref Message m)
	{
		if (!sansBarreWindows || m.Msg != WM_NCCALCSIZE || m.WParam == IntPtr.Zero || base.FormBorderStyle == FormBorderStyle.None)
		{
			return false;
		}
		RECT r0 = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));
		int haut = r0.top;
		base.WndProc(ref m);
		RECT r = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));
		r.top = haut;
		if (base.WindowState == FormWindowState.Maximized)
		{
			// agrandie, la fenêtre déborde de l'écran de l'épaisseur du cadre : on la compense en haut
			r.top += GetSystemMetrics(33) + GetSystemMetrics(92);
		}
		Marshal.StructureToPtr(r, m.LParam, fDeleteOld: false);
		m.Result = IntPtr.Zero;
		return true;
	}

	private Bitmap fondBarre;

	private string cleFondBarre = "";

	/// <summary>Fond sous les boutons de la barre : le même que la page visible (image gardée en cache).</summary>
	internal void PeindreSousBarre(Graphics g, Rectangle r)
	{
		PanelFond p = ((couches.Count > 0) ? null : ((page != null && page.Visible) ? ((PanelFond)page) : PanelVisible()));
		if (p == null)
		{
			using SolidBrush b = new SolidBrush(C.Fond);
			g.FillRectangle(b, 0, 0, r.Width, r.Height);
			return;
		}
		ThemeBase tb = ((p == pAccueil) ? noel : ((p == pJeux) ? themeJeux : null));
		string cle = p.Name + "|" + p.GetHashCode() + "|" + p.ClientSize + "|" + r + "|" + ((tb != null && tb.ATeinte) ? tb.Id : "") + "|" + fond.Pret;
		if (fondBarre == null || cle != cleFondBarre)
		{
			fondBarre?.Dispose();
			fondBarre = new Bitmap(Math.Max(1, r.Width), Math.Max(1, r.Height));
			using (Graphics gb = Graphics.FromImage(fondBarre))
			{
				Rectangle dest = new Rectangle(-r.X, -r.Y, p.ClientSize.Width, p.ClientSize.Height);
				if (tb != null && tb.ATeinte)
				{
					fond.PeindreTeinte(gb, dest, tb.Id, tb.Teinter);
				}
				else
				{
					fond.Peindre(gb, dest);
				}
			}
			cleFondBarre = cle;
		}
		g.DrawImageUnscaled(fondBarre, 0, 0);
	}

	public void BasculerAgrandi()
	{
		base.WindowState = ((base.WindowState == FormWindowState.Maximized) ? FormWindowState.Normal : FormWindowState.Maximized);
	}

	/// <summary>Clic dans la bande du haut d'une page (hors boutons, champs…) : déplace la fenêtre ; double-clic : agrandit.</summary>
	public bool PreFilterMessage(ref Message m)
	{
		if (!sansBarreWindows || (m.Msg != WM_LBUTTONDOWN && m.Msg != WM_LBUTTONDBLCLK) || plein || !base.Visible || base.IsDisposed)
		{
			return false;
		}
		Control c = Control.FromHandle(m.HWnd);
		if (c == null || !(c is PanelFond || c is Form || c.GetType() == typeof(Panel) || c is Label) || c.TopLevelControl != this)
		{
			return false;
		}
		Point p = PointToClient(Cursor.Position);
		if (p.Y < 0 || p.Y >= HautDeplacement || p.X < 0 || p.X >= base.ClientSize.Width)
		{
			return false;
		}
		if (m.Msg == WM_LBUTTONDBLCLK)
		{
			BasculerAgrandi();
			return true;
		}
		ReleaseCapture();
		int ht = ((p.Y < Zx.P(4) && base.WindowState == FormWindowState.Normal && sansBarreWindows) ? HTTOP : HTCAPTION);
		EnvoyerMsg(base.Handle, WM_NCLBUTTONDOWN, (IntPtr)ht, IntPtr.Zero);
		return true;
	}
}

/// <summary>Boutons réduire / agrandir / fermer, dessinés dans le style du lanceur.</summary>
internal class BarreTitre : Control
{
	private readonly Fenetre f;

	private int survol = -1;

	private int appui = -1;

	private static int LB => Zx.P(46);

	public BarreTitre(Fenetre f)
	{
		this.f = f;
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		BackColor = C.Fond;
		base.Size = new Size(LB * 3, Zx.P(28));
		base.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		base.TabStop = false;
	}

	private int Index(Point p)
	{
		if (p.Y < 0 || p.Y >= base.Height || p.X < 0)
		{
			return -1;
		}
		int i = p.X / LB;
		return (i >= 0 && i < 3) ? i : (-1);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		int i = Index(e.Location);
		if (i != survol)
		{
			survol = i;
			Invalidate();
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		survol = -1;
		appui = -1;
		Invalidate();
		base.OnMouseLeave(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			appui = Index(e.Location);
			Invalidate();
		}
		base.OnMouseDown(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		int i = Index(e.Location);
		bool ok = e.Button == MouseButtons.Left && i >= 0 && i == appui;
		appui = -1;
		Invalidate();
		base.OnMouseUp(e);
		if (!ok)
		{
			return;
		}
		switch (i)
		{
		case 0:
			f.WindowState = FormWindowState.Minimized;
			break;
		case 1:
			f.BasculerAgrandi();
			break;
		default:
			f.Close();
			break;
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics g = e.Graphics;
		try
		{
			f.PeindreSousBarre(g, base.Bounds);
		}
		catch
		{
			g.Clear(C.Fond);
		}
		g.SmoothingMode = SmoothingMode.AntiAlias;
		// petite plaque discrète : voile sombre, coin arrondi en bas à gauche, liseré fin
		int rr = Zx.P(10);
		using (GraphicsPath plaque = new GraphicsPath())
		{
			plaque.AddLine(0, 0, base.Width, 0);
			plaque.AddLine(base.Width, 0, base.Width, base.Height);
			plaque.AddLine(base.Width, base.Height, rr, base.Height);
			plaque.AddArc(0, base.Height - 2 * rr - 1, 2 * rr, 2 * rr, 90f, 90f);
			plaque.CloseFigure();
			using (SolidBrush voile = new SolidBrush(Color.FromArgb(90, 6, 8, 20)))
			{
				g.FillPath(voile, plaque);
			}
			using Pen lis = new Pen(Color.FromArgb(26, 255, 255, 255), 1f);
			g.DrawLine(lis, 0, 0, 0, base.Height - rr);
			g.DrawArc(lis, 0, base.Height - 2 * rr - 1, 2 * rr, 2 * rr, 90f, 90f);
			g.DrawLine(lis, rr, base.Height - 1, base.Width, base.Height - 1);
		}
		for (int i = 0; i < 3; i++)
		{
			Rectangle r = new Rectangle(i * LB, 0, LB, base.Height);
			if (i == survol || i == appui)
			{
				Color fond = ((i == 2) ? Color.FromArgb((i == appui) ? 200 : 232, 232, 17, 35) : Color.FromArgb((i == appui) ? 34 : 24, 255, 255, 255));
				using SolidBrush b = new SolidBrush(fond);
				g.FillRectangle(b, r);
			}
			int cx = r.X + r.Width / 2;
			int cy = r.Y + r.Height / 2;
			int k = Zx.P(5);
			Color ci = ((i == 2 && i == survol) ? Color.White : Color.FromArgb((i == survol) ? 255 : 205, 225, 230, 245));
			using Pen p = new Pen(ci, Math.Max(1f, Zx.P(1.1f)));
			switch (i)
			{
			case 0:
				g.DrawLine(p, cx - k, cy, cx + k, cy);
				break;
			case 1:
				if (f.WindowState == FormWindowState.Maximized)
				{
					int d = Zx.P(2);
					g.DrawRectangle(p, cx - k, cy - k + d, 2 * k - d, 2 * k - d);
					g.DrawLines(p, new Point[3]
					{
						new Point(cx - k + d, cy - k + d),
						new Point(cx - k + d, cy - k),
						new Point(cx + k, cy - k)
					});
					g.DrawLine(p, cx + k, cy - k, cx + k, cy + k - d);
				}
				else
				{
					g.DrawRectangle(p, cx - k, cy - k, 2 * k, 2 * k);
				}
				break;
			default:
				g.DrawLine(p, cx - k, cy - k, cx + k, cy + k);
				g.DrawLine(p, cx + k, cy - k, cx - k, cy + k);
				break;
			}
		}
	}
}
