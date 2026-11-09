using System.Collections.Generic;

namespace TPSStudios;

internal class Compte
{
	public string Id = "";

	public string Pseudo = "";

	public string Cree = "";

	public string Derniere = "";

	public int Couleur;

	public int Banniere;

	public int CouleurPerso;

	public bool Admin;

	public bool AutoCouleurs;

	public string BanType = "";

	public string BanMotif = "";

	public int BanC1;

	public int BanC2;

	public int BanAngle;

	public int BanZoom;

	public int BanX = 50;

	public int BanY = 50;

	public string Bio = "";

	public string Statut = "";

	public string JeuFavori = "";

	public string Lieu = "";

	public string Deco = "";

	public string DiscordNom = "";

	public Dictionary<string, int> Lancements = new Dictionary<string, int>();

	public Dictionary<string, string> Dernier = new Dictionary<string, string>();

	public int Xp;

	public int Serie;

	public int SerieMax;

	public string DernierJour = "";

	public Dictionary<string, string> Succes = new Dictionary<string, string>();

	public Dictionary<string, int> Stats = new Dictionary<string, int>();
}
