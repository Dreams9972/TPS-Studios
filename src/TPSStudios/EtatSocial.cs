using System.Collections.Generic;

namespace TPSStudios;

internal class EtatSocial
{
	public int Likes;

	public bool Aime;

	/// <summary>Moyenne des étoiles données par les joueurs (0 : aucun vote), nombre de votes, ma note (0 : pas noté).</summary>
	public double NoteMoy;

	public int NoteNb;

	public int MaNote;

	public List<ComSocial> Coms = new List<ComSocial>();
}
