# Serveur social TPS Groupe (Cloudflare Worker)

Code du serveur des J'aime, commentaires, amis et du temps réel, déployé sur Cloudflare
sous le nom `tps-studios-social`.

- `src/index.origine.js` : le code tel qu'il est déployé aujourd'hui (récupéré depuis Cloudflare).
- `src/index.js` : la nouvelle version, avec les **J'aime sur les commentaires** (lanceur v2.9.2) :
  - chaque commentaire renvoie `likes` et `aime` ;
  - nouvelle adresse `POST /v1/commentaires/{id}/aimer` (aimer / ne plus aimer) ;
  - mise à jour en direct pour tout le monde (message `likecom`) ;
  - la table `likes_commentaires` est créée toute seule au premier usage
    (voir aussi `migrations/0002_likes_commentaires.sql`).

## Mettre en ligne

Avec Node.js installé, dans ce dossier :

```
npx wrangler login
npx wrangler deploy
```

Les secrets et variables déjà réglés sur le Worker (`ADMIN_KEY`, `DISCORD_WEBHOOK`, `CATALOGUE_URL`) sont conservés.

Tant que cette version n'est pas déployée, le lanceur affiche les compteurs à 0 et
indique « les J'aime sur les commentaires ne sont pas encore activés sur le serveur » au clic.
