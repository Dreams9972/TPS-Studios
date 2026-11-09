-- J'aime sur les commentaires (lanceur v2.9.2). Le serveur la crée aussi tout seul au besoin.
CREATE TABLE IF NOT EXISTS likes_commentaires (
  commentaire_id INTEGER NOT NULL,
  joueur_id TEXT NOT NULL,
  cree INTEGER NOT NULL,
  PRIMARY KEY (commentaire_id, joueur_id)
);
