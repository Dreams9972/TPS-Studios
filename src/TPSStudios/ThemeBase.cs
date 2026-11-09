using System.Drawing;

namespace TPSStudios;

internal abstract class ThemeBase
{
	public bool Fige;

	public readonly int Page;

	public string Id = "";

	public static Rectangle[] Tuiles = new Rectangle[0];

	public static Rectangle[] Cartes = new Rectangle[0];

	public static Rectangle ZoneLibre;

	public virtual bool NeigeTuiles => false;

	public virtual bool EnChute => false;

	public virtual bool ATeinte => false;

	public virtual bool Secoue => false;

	protected ThemeBase(int page)
	{
		Page = page;
	}

	public virtual void Deneiger(Rectangle tuile)
	{
	}

	public virtual void Teinter(Graphics g, int w, int h)
	{
	}

	public virtual void SurTuiles(Graphics g, int w, int h)
	{
	}

	public virtual void DecorTuile(Graphics g, int i, int w, int h)
	{
	}

	public virtual int EtatTuile(int i)
	{
		return 0;
	}

	public virtual PointF Decalage(int i)
	{
		return PointF.Empty;
	}

	public abstract void Avancer(double dt, int w, int h);

	public abstract void PeindreFond(Graphics g, int w, int h);

	public abstract void PeindreNeige(Graphics g, int w, int h, double densite);

	public void PeindreDevant(Graphics g, int w, int h)
	{
		if (!Fige)
		{
			PeindreNeige(g, w, h, 1.0);
		}
	}
}
