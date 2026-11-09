using System;

namespace TPSStudios;

internal struct Ressort
{
	public float V;

	public float Vit;

	public bool Pas(float cible, float raideur, float amorti)
	{
		Vit = Vit * amorti + (cible - V) * raideur;
		V += Vit;
		bool flag = Math.Abs(cible - V) < 0.0015f && Math.Abs(Vit) < 0.0015f;
		if (flag)
		{
			V = cible;
			Vit = 0f;
		}
		return !flag;
	}
}
