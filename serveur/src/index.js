// TPS Studios : serveur social (likes, commentaires, temps réel) — Cloudflare Worker + D1 + Durable Object
// Sécurité : HTTPS uniquement (Cloudflare), jeton secret par joueur (seul son hachage SHA-256 est stocké), limites de débit par joueur et par IP,
// validation stricte de toutes les entrées, texte nettoyé (caractères de contrôle et d'inversion de sens retirés), requêtes SQL préparées,
// modération par clé administrateur (secret Cloudflare ADMIN_KEY, jamais dans le code), aucune donnée personnelle (pas d'e-mail, pas d'adresse IP stockée en clair).

const MOD = /^[a-z0-9][a-z0-9_-]{1,39}$/;
const PSEUDO = /^[\p{L}\p{N} _().\-]{2,20}$/u;
const RESERVE = /(admin|modo|staff|\btps\b)/i;      // pseudos réservés à l'équipe (inscription avec la clé administrateur)
const COMMENTAIRE_MAX = 500, CORPS_MAX = 4096, HISTORIQUE = 100;
const ENTETES = { 'content-type': 'application/json; charset=utf-8', 'cache-control': 'no-store', 'x-content-type-options': 'nosniff', 'referrer-policy': 'no-referrer' };

const rep = (o, statut = 200) => new Response(JSON.stringify(o), { status: statut, headers: ENTETES });
const erreur = (statut, msg) => rep({ erreur: msg }, statut);
const b64url = (octets) => btoa(String.fromCharCode(...octets)).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
const alea = (n) => { const b = new Uint8Array(n); crypto.getRandomValues(b); return b; };
const hex = (octets) => [...octets].map((x) => x.toString(16).padStart(2, '0')).join('');
async function sha256(t) { return hex(new Uint8Array(await crypto.subtle.digest('SHA-256', new TextEncoder().encode(t)))); }
function egal(a, b) { if (a.length !== b.length) return false; let r = 0; for (let i = 0; i < a.length; i++) r |= a.charCodeAt(i) ^ b.charCodeAt(i); return r === 0; }   // comparaison en temps constant
const propre = (t, max) => String(t ?? '').replace(/[\u0000-\u001f\u007f-\u009f​-‏‪-‮⁠-⁩﻿]/g, ' ').replace(/\s+/g, ' ').trim().slice(0, max);
const maintenant = () => Math.floor(Date.now() / 1000);

async function corps(request) {
  const longueur = parseInt(request.headers.get('content-length') || '0', 10);
  if (longueur > CORPS_MAX) throw { statut: 413, msg: 'requête trop grande' };
  const t = await request.text(); if (t.length > CORPS_MAX) throw { statut: 413, msg: 'requête trop grande' };
  try { const o = JSON.parse(t || '{}'); return o && typeof o === 'object' ? o : {}; } catch { throw { statut: 400, msg: 'JSON invalide' }; }
}

// limite de débit : au plus `max` actions par fenêtre de `fen` secondes pour la clé donnée
async function limite(env, cle, max, fen) {
  const now = maintenant(), l = await env.DB.prepare('SELECT n, debut FROM limites WHERE cle = ?').bind(cle).first();
  if (!l || now - l.debut >= fen) { await env.DB.prepare('INSERT OR REPLACE INTO limites (cle, n, debut) VALUES (?, 1, ?)').bind(cle, now).run(); return true; }
  if (l.n >= max) return false;
  await env.DB.prepare('UPDATE limites SET n = n + 1 WHERE cle = ?').bind(cle).run(); return true;
}
async function ip(request) { return (await sha256((request.headers.get('cf-connecting-ip') || 'inconnue') + '|tps')).slice(0, 24); }

async function joueur(request, env) {
  const h = request.headers.get('authorization') || ''; if (!h.startsWith('Bearer ')) return null;
  const jeton = h.slice(7).trim(), p = jeton.indexOf('.'); if (p < 8 || p > 40 || jeton.length > 120) return null;
  const id = jeton.slice(0, p), secret = jeton.slice(p + 1);
  const j = await env.DB.prepare('SELECT j.id, j.secret_hash, j.pseudo, j.niveau, j.admin, j.banni, (SELECT depuis FROM vip v WHERE v.joueur_id = j.id) AS vip_depuis FROM joueurs j WHERE j.id = ?').bind(id).first();
  if (!j || j.banni || !egal(j.secret_hash, await sha256(secret))) return null;
  return j;
}
const estAdminCle = (request, env) => !!env.ADMIN_KEY && egal(request.headers.get('x-admin-key') || '', env.ADMIN_KEY);
// clé administrateur : après 10 essais faux en une heure depuis une même connexion, plus aucun essai n'est accepté (même juste) jusqu'à la fin de l'heure
async function adminOk(request, env) {
  if (!request.headers.get('x-admin-key')) return false;
  const k = 'admf:' + (await ip(request)), b = await env.DB.prepare('SELECT n, debut FROM limites WHERE cle = ?').bind(k).first();
  if (b && maintenant() - b.debut < 3600 && b.n >= 10) return false;
  if (estAdminCle(request, env)) return true;
  await limite(env, k, 10, 3600); return false;
}

async function avis(env, mod, msg) { await env.SALLE.get(env.SALLE.idFromName(mod)).fetch('https://salle/emettre', { method: 'POST', body: JSON.stringify(msg) }); }
async function compteLikes(env, mod) { return (await env.DB.prepare('SELECT COUNT(*) AS n FROM likes WHERE mod_id = ?').bind(mod).first()).n; }
const commentaireJson = (r, moi) => ({ id: r.id, auteur: r.pseudo, niveau: r.niveau, admin: !!r.admin, vip: !!r.vip, statut: r.statut || '', texte: r.texte, cree: r.cree, moi: !!moi && r.joueur_id === moi.id, likes: r.likes || 0, aime: !!r.aime });
// J'aime sur les commentaires (v2.9) : table créée au besoin (voir migrations/0002_likes_commentaires.sql)
let tableLikesCom = false;
async function assureLikesCom(env) {
  if (tableLikesCom) return;
  await env.DB.prepare('CREATE TABLE IF NOT EXISTS likes_commentaires (commentaire_id INTEGER NOT NULL, joueur_id TEXT NOT NULL, cree INTEGER NOT NULL, PRIMARY KEY (commentaire_id, joueur_id))').run();
  tableLikesCom = true;
}
const PRESENCE = 150;   // secondes : au-delà, le joueur est considéré hors ligne
const AMIS_PRESENCE = PRESENCE, AMIS_MAX = 200;
const MES_AMIS = "WITH mes(id) AS (SELECT CASE WHEN a = ? THEN b ELSE a END FROM amis WHERE etat = 'ami' AND (a = ? OR b = ?)) ";   // mes amis (3 paramètres : mon id)
async function estAmi(env, x, y) { const [a, b] = x < y ? [x, y] : [y, x]; return !!(await env.DB.prepare("SELECT 1 FROM amis WHERE a = ? AND b = ? AND etat = 'ami'").bind(a, b).first()); }
const normaliserCode = (t) => String(t ?? '').toUpperCase().replace(/[\s_]/g, '').replace(/^TPSVIP/, 'TPS-VIP-').replace(/^(TPS-VIP-)([A-Z0-9]{4})-?([A-Z0-9]{4})-?([A-Z0-9]{4})$/, '$1$2-$3-$4');
// codes VIP : TPS-VIP-XXXX-XXXX-XXXX (alphabet sans caractères ambigus) ; seule l'empreinte salée est stockée
const ALPHA = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789', CODE_VIP = /^TPS-VIP-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}$/;
function nouveauCode() { const b = alea(12); let s = ''; for (let i = 0; i < 12; i++) { s += ALPHA[b[i] % ALPHA.length]; if (i % 4 === 3 && i < 11) s += '-'; } return 'TPS-VIP-' + s; }
const empreinteVip = (code) => sha256(code + '|vip-tps');

// points VIP : gagnés côté serveur uniquement (bonus du jour, premier « J'aime » d'un mod, commentaires), dépensés dans la boutique
const BONUS_JOUR = 50, BONUS_LIKE = 10, BONUS_COMM = 5, BONUS_BIENVENUE = 200;
const COSMETIQUES = { 'ban:diamant': 300, 'ban:neonvip': 500, 'deco:couronne': 400, 'deco:flammes': 600 };   // prix en points (mêmes identifiants dans le lanceur)
const ARTICLE = /^(ban|deco):[a-z0-9-]{2,20}$|^mod:[a-z0-9][a-z0-9_-]{1,39}$/;
async function crediter(env, id, n) { await env.DB.prepare('INSERT INTO points (joueur_id, solde, total) VALUES (?, ?, ?) ON CONFLICT(joueur_id) DO UPDATE SET solde = solde + excluded.solde, total = total + excluded.total').bind(id, n, n).run(); }
async function premiereFois(env, id, cle) { return (await env.DB.prepare('INSERT OR IGNORE INTO points_faits (joueur_id, cle) VALUES (?, ?)').bind(id, cle).run()).meta.changes === 1; }
async function etatPoints(env, id) {
  const p = await env.DB.prepare('SELECT solde, total, dernier_jour FROM points WHERE joueur_id = ?').bind(id).first(), a = await env.DB.prepare('SELECT article FROM achats WHERE joueur_id = ? ORDER BY cree').bind(id).all();
  return { solde: p ? p.solde : 0, total: p ? p.total : 0, bonus_pris: !!p && p.dernier_jour === new Date().toISOString().slice(0, 10), achats: a.results.map((x) => x.article) };
}
// prix en points d'un mod : lu dans le catalogue public (champ « prix_points »), jamais dans la requête du joueur
async function prixMod(env, id) {
  const r = await fetch(env.CATALOGUE_URL || 'https://raw.githubusercontent.com/Dreams9972/TPS-Studios/main/catalogue.json', { cf: { cacheTtl: 60 } });
  if (!r.ok) return 0; const cat = await r.json(); const p = (cat.projets || []).find((x) => x && x.id === id);
  const n = p ? parseInt(p.prix_points, 10) : 0; return n > 0 && n <= 1000000 ? n : 0;
}

async function route(request, env) {
  const url = new URL(request.url), chemin = url.pathname.replace(/\/+$/, ''), m = request.method;
  if (chemin === '/v1/etat') return rep({ ok: true, temps: maintenant() });

  // inscription : un joueur reçoit un jeton secret, affiché une seule fois
  if (m === 'POST' && chemin === '/v1/inscription') {
    if (!(await limite(env, 'ins:' + (await ip(request)), 10, 3600))) return erreur(429, 'trop d\'inscriptions depuis cette connexion, réessayez plus tard');
    const c = await corps(request), pseudo = propre(c.pseudo, 20), admin = await adminOk(request, env);
    if (!PSEUDO.test(pseudo)) return erreur(400, 'pseudo invalide (2 à 20 lettres, chiffres, espaces, _ ( ) . -)');
    if (RESERVE.test(pseudo) && !admin) return erreur(403, 'pseudo réservé à l\'équipe TPS Studios');
    if (await env.DB.prepare('SELECT 1 FROM joueurs WHERE pseudo_min = ?').bind(pseudo.toLowerCase()).first()) return erreur(409, 'ce pseudo est déjà pris');
    const id = hex(alea(8)), secret = b64url(alea(32));
    await env.DB.prepare('INSERT INTO joueurs (id, secret_hash, pseudo, pseudo_min, niveau, admin, banni, cree) VALUES (?, ?, ?, ?, 1, ?, 0, ?)').bind(id, await sha256(secret), pseudo, pseudo.toLowerCase(), admin ? 1 : 0, maintenant()).run();
    return rep({ id, secret, pseudo, jeton: id + '.' + secret, admin }, 201);
  }

  const moi = await joueur(request, env);

  if (chemin === '/v1/moi') {
    if (!moi) return erreur(401, 'jeton absent ou invalide');
    if (m === 'GET') {
      const v = await env.DB.prepare('SELECT COUNT(*) AS n FROM likes WHERE joueur_id = ?').bind(moi.id).first(), k = await env.DB.prepare('SELECT COUNT(*) AS n FROM commentaires WHERE joueur_id = ? AND supprime = 0').bind(moi.id).first();
      return rep({ id: moi.id, pseudo: moi.pseudo, niveau: moi.niveau, admin: !!moi.admin, vip: !!moi.vip_depuis, vip_depuis: moi.vip_depuis || 0, votes: v.n, commentaires: k.n });
    }
    if (m === 'PUT') {   // niveau affiché (informatif) et changement de pseudo
      if (!(await limite(env, 'moi:' + moi.id, 20, 3600))) return erreur(429, 'trop de modifications');
      const c = await corps(request), niveau = Math.max(1, Math.min(100, parseInt(c.niveau, 10) || moi.niveau));
      let pseudo = moi.pseudo;
      if (c.pseudo !== undefined && propre(c.pseudo, 20) !== moi.pseudo) {
        pseudo = propre(c.pseudo, 20);
        if (!PSEUDO.test(pseudo)) return erreur(400, 'pseudo invalide');
        if (RESERVE.test(pseudo) && !moi.admin) return erreur(403, 'pseudo réservé');
        if (await env.DB.prepare('SELECT 1 FROM joueurs WHERE pseudo_min = ? AND id <> ?').bind(pseudo.toLowerCase(), moi.id).first()) return erreur(409, 'ce pseudo est déjà pris');
      }
      await env.DB.prepare('UPDATE joueurs SET niveau = ?, pseudo = ?, pseudo_min = ? WHERE id = ?').bind(niveau, pseudo, pseudo.toLowerCase(), moi.id).run();
      return rep({ pseudo, niveau });
    }
  }

  // statut en direct : le lanceur l'envoie à chaque changement et toutes les minutes ; le nombre de joueurs en ligne est public
  if (chemin === '/v1/presence') {
    if (m === 'PUT') {
      if (!moi) return erreur(401, 'jeton absent ou invalide');
      if (!(await limite(env, 'pres:' + moi.id, 240, 3600))) return erreur(429, 'trop de mises à jour du statut');
      const c = await corps(request), statut = propre(c.statut, 60);
      if (statut) await env.DB.prepare('INSERT OR REPLACE INTO presence (joueur_id, statut, vu) VALUES (?, ?, ?)').bind(moi.id, statut, maintenant()).run();
      else await env.DB.prepare('DELETE FROM presence WHERE joueur_id = ?').bind(moi.id).run();   // fermeture du lanceur : hors ligne tout de suite
      return rep({ ok: true });
    }
    if (m === 'GET') {
      const r = await env.DB.prepare("SELECT COUNT(*) AS n, SUM(CASE WHEN statut LIKE 'En jeu%' THEN 1 ELSE 0 END) AS j FROM presence WHERE vu > ?").bind(maintenant() - PRESENCE).first();
      return rep({ en_ligne: r.n || 0, en_jeu: r.j || 0 });
    }
  }

  // temps réel : abonnement en lecture seule aux événements d'une page de mod
  let r = chemin.match(/^\/v1\/ws\/([a-z0-9_-]+)$/);
  if (r && m === 'GET') {
    if (!MOD.test(r[1])) return erreur(400, 'identifiant de mod invalide');
    if (request.headers.get('upgrade') !== 'websocket') return erreur(426, 'WebSocket attendu');
    return env.SALLE.get(env.SALLE.idFromName(r[1])).fetch(request);
  }

  r = chemin.match(/^\/v1\/mod\/([a-z0-9_-]+)$/);
  if (r && m === 'GET') {
    const mod = r[1]; if (!MOD.test(mod)) return erreur(400, 'identifiant de mod invalide');
    const likes = await compteLikes(env, mod);
    const aime = moi ? !!(await env.DB.prepare('SELECT 1 FROM likes WHERE mod_id = ? AND joueur_id = ?').bind(mod, moi.id).first()) : false;
    await assureLikesCom(env);
    const lignes = await env.DB.prepare('SELECT c.id, c.joueur_id, c.texte, c.cree, j.pseudo, j.niveau, j.admin, (SELECT 1 FROM vip v WHERE v.joueur_id = c.joueur_id) AS vip, (SELECT p.statut FROM presence p WHERE p.joueur_id = c.joueur_id AND p.vu > ?) AS statut, (SELECT COUNT(*) FROM likes_commentaires lc WHERE lc.commentaire_id = c.id) AS likes, (SELECT 1 FROM likes_commentaires lc WHERE lc.commentaire_id = c.id AND lc.joueur_id = ?) AS aime FROM commentaires c JOIN joueurs j ON j.id = c.joueur_id WHERE c.mod_id = ? AND c.supprime = 0 ORDER BY c.id DESC LIMIT ?').bind(maintenant() - PRESENCE, moi ? moi.id : '', mod, HISTORIQUE).all();
    return rep({ likes, aime, commentaires: lignes.results.reverse().map((x) => commentaireJson(x, moi)) });
  }

  r = chemin.match(/^\/v1\/mod\/([a-z0-9_-]+)\/aimer$/);
  if (r && m === 'POST') {
    const mod = r[1]; if (!MOD.test(mod)) return erreur(400, 'identifiant de mod invalide');
    if (!moi) return erreur(401, 'jeton absent ou invalide');
    if (!(await limite(env, 'like:' + moi.id, 30, 60))) return erreur(429, 'trop rapide');
    const deja = await env.DB.prepare('SELECT 1 FROM likes WHERE mod_id = ? AND joueur_id = ?').bind(mod, moi.id).first();
    if (deja) await env.DB.prepare('DELETE FROM likes WHERE mod_id = ? AND joueur_id = ?').bind(mod, moi.id).run();
    else {
      await env.DB.prepare('INSERT INTO likes (mod_id, joueur_id, cree) VALUES (?, ?, ?)').bind(mod, moi.id, maintenant()).run();
      if (moi.vip_depuis && await premiereFois(env, moi.id, 'like:' + mod)) await crediter(env, moi.id, BONUS_LIKE);   // une seule fois par mod
    }
    const likes = await compteLikes(env, mod); await avis(env, mod, { t: 'likes', likes });
    return rep({ likes, aime: !deja });
  }

  r = chemin.match(/^\/v1\/mod\/([a-z0-9_-]+)\/commentaires$/);
  if (r && m === 'POST') {
    const mod = r[1]; if (!MOD.test(mod)) return erreur(400, 'identifiant de mod invalide');
    if (!moi) return erreur(401, 'jeton absent ou invalide');
    const c = await corps(request), texte = propre(c.texte, COMMENTAIRE_MAX);
    if (texte.length < 2) return erreur(400, 'commentaire trop court');
    if (!(await limite(env, 'c15:' + moi.id, 1, 15))) return erreur(429, 'patientez quelques secondes entre deux commentaires');
    if (!(await limite(env, 'cj:' + moi.id, 40, 86400))) return erreur(429, 'limite de commentaires atteinte pour aujourd\'hui');
    if (await env.DB.prepare('SELECT 1 FROM commentaires WHERE joueur_id = ? AND mod_id = ? AND texte = ? AND cree > ?').bind(moi.id, mod, texte, maintenant() - 300).first()) return erreur(409, 'commentaire déjà publié');
    const cree = maintenant(), res = await env.DB.prepare('INSERT INTO commentaires (mod_id, joueur_id, texte, cree, supprime) VALUES (?, ?, ?, ?, 0)').bind(mod, moi.id, texte, cree).run();
    const com = { id: res.meta.last_row_id, auteur: moi.pseudo, niveau: moi.niveau, admin: !!moi.admin, vip: !!moi.vip_depuis, texte, cree };
    if (moi.vip_depuis && await limite(env, 'ptc:' + moi.id, 3, 86400)) await crediter(env, moi.id, BONUS_COMM);   // 3 commentaires récompensés par jour
    await avis(env, mod, { t: 'commentaire', c: com });
    return rep({ ...com, moi: true }, 201);
  }

  r = chemin.match(/^\/v1\/commentaires\/(\d{1,12})\/aimer$/);
  if (r && m === 'POST') {   // J'aime sur un commentaire (bascule)
    const id = parseInt(r[1], 10);
    if (!moi) return erreur(401, 'jeton absent ou invalide');
    if (!(await limite(env, 'likec:' + moi.id, 60, 60))) return erreur(429, 'trop rapide');
    const c = await env.DB.prepare('SELECT id, mod_id FROM commentaires WHERE id = ? AND supprime = 0').bind(id).first();
    if (!c) return erreur(404, 'commentaire introuvable');
    await assureLikesCom(env);
    const deja = await env.DB.prepare('SELECT 1 FROM likes_commentaires WHERE commentaire_id = ? AND joueur_id = ?').bind(id, moi.id).first();
    if (deja) await env.DB.prepare('DELETE FROM likes_commentaires WHERE commentaire_id = ? AND joueur_id = ?').bind(id, moi.id).run();
    else await env.DB.prepare('INSERT INTO likes_commentaires (commentaire_id, joueur_id, cree) VALUES (?, ?, ?)').bind(id, moi.id, maintenant()).run();
    const likes = (await env.DB.prepare('SELECT COUNT(*) AS n FROM likes_commentaires WHERE commentaire_id = ?').bind(id).first()).n;
    await avis(env, c.mod_id, { t: 'likecom', id, likes });
    return rep({ likes, aime: !deja });
  }
  r = chemin.match(/^\/v1\/commentaires\/(\d{1,12})$/);
  if (r && m === 'DELETE') {
    const id = parseInt(r[1], 10), admin = await adminOk(request, env);
    if (!moi && !admin) return erreur(401, 'jeton absent ou invalide');
    const c = await env.DB.prepare('SELECT id, mod_id, joueur_id FROM commentaires WHERE id = ? AND supprime = 0').bind(id).first();
    if (!c) return erreur(404, 'commentaire introuvable');
    if (!admin && !(moi && (c.joueur_id === moi.id || moi.admin))) return erreur(403, 'action refusée');
    await env.DB.prepare('UPDATE commentaires SET supprime = 1 WHERE id = ?').bind(id).run();
    await avis(env, c.mod_id, { t: 'supprime', id });
    return rep({ ok: true });
  }

  // amis : demandes, acceptation, retrait ; le statut en direct d'un joueur n'est visible que par ses amis
  // ma liste de mods installés (visible seulement par mes amis)
  if (chemin === '/v1/mods' && m === 'PUT') {
    if (!moi) return erreur(401, 'jeton absent ou invalide');
    if (!(await limite(env, 'mods:' + moi.id, 60, 3600))) return erreur(429, 'trop de mises à jour');
    const c = await corps(request), l = Array.isArray(c.mods) ? c.mods : [];
    const propres = l.slice(0, 150).filter((x) => x && MOD.test(String(x.id))).map((x) => ({ id: String(x.id), version: propre(x.version, 30), actif: x.actif !== false }));
    await env.DB.prepare('INSERT OR REPLACE INTO mods_joueur (joueur_id, liste, maj) VALUES (?, ?, ?)').bind(moi.id, JSON.stringify(propres), maintenant()).run();
    return rep({ ok: true, n: propres.length });
  }
  // profil d'un joueur vu par moi : amis en commun (toujours) ; statut et mods (seulement entre amis)
  r = chemin.match(/^\/v1\/joueurs\/([0-9a-f]{16})$/);
  if (r && m === 'GET') {
    if (!moi) return erreur(401, 'jeton absent ou invalide');
    if (!(await limite(env, 'prof:' + moi.id, 240, 3600))) return erreur(429, 'trop de demandes');
    const id = r[1], j = await env.DB.prepare('SELECT id, pseudo, niveau, cree, (SELECT 1 FROM vip v WHERE v.joueur_id = id) AS vip FROM joueurs WHERE id = ? AND banni = 0').bind(id).first();
    if (!j) return erreur(404, 'joueur introuvable');
    const ami = await estAmi(env, moi.id, id);
    const communs = await env.DB.prepare(MES_AMIS + "SELECT j.id, j.pseudo, j.niveau FROM amis a JOIN joueurs j ON j.id = (CASE WHEN a.a = ? THEN a.b ELSE a.a END) WHERE a.etat = 'ami' AND (a.a = ? OR a.b = ?) AND j.id IN (SELECT id FROM mes) AND j.banni = 0 ORDER BY j.pseudo_min LIMIT 50").bind(moi.id, moi.id, moi.id, id, id, id).all();
    const o = { id: j.id, pseudo: j.pseudo, niveau: j.niveau, vip: !!j.vip, cree: j.cree, ami, communs: communs.results.map((x) => ({ id: x.id, pseudo: x.pseudo, niveau: x.niveau })) };
    if (ami) {
      const p = await env.DB.prepare('SELECT statut FROM presence WHERE joueur_id = ? AND vu > ?').bind(id, maintenant() - PRESENCE).first(), md = await env.DB.prepare('SELECT liste, maj FROM mods_joueur WHERE joueur_id = ?').bind(id).first();
      o.statut = p ? p.statut : ''; o.en_ligne = !!p; o.mods = md ? JSON.parse(md.liste) : []; o.mods_maj = md ? md.maj : 0;
    }
    return rep(o);
  }
  // offrir le VIP à un ami avec un code VIP acheté : le code est activé sur SON compte
  if (m === 'POST' && chemin === '/v1/vip/offrir') {
    if (!moi) return erreur(401, 'jeton absent ou invalide');
    if (!(await limite(env, 'vipj:' + moi.id, 6, 3600)) || !(await limite(env, 'vipi:' + (await ip(request)), 12, 3600))) return erreur(429, 'trop d\'essais, réessayez dans une heure');
    const c = await corps(request), id = propre(c.id, 40).toLowerCase();
    if (!/^[0-9a-f]{16}$/.test(id) || id === moi.id) return erreur(400, 'ami invalide');
    if (!(await estAmi(env, moi.id, id))) return erreur(403, 'vous ne pouvez offrir le VIP qu\'à un ami');
    if (await env.DB.prepare('SELECT 1 FROM vip WHERE joueur_id = ?').bind(id).first()) return erreur(409, 'ce joueur est déjà VIP');
    const code = normaliserCode(c.code); if (!CODE_VIP.test(code)) return erreur(400, 'code VIP invalide (format TPS-VIP-XXXX-XXXX-XXXX)');
    const now = maintenant(), res = await env.DB.prepare('UPDATE vip_codes SET utilise_par = ?, utilise_le = ? WHERE code_hash = ? AND utilise_par IS NULL').bind(id, now, await empreinteVip(code)).run();
    if (!res.meta.changes) return erreur(404, 'code VIP inconnu ou déjà utilisé');
    await env.DB.prepare('INSERT OR IGNORE INTO vip (joueur_id, depuis) VALUES (?, ?)').bind(id, now).run();
    if (await premiereFois(env, id, 'bienvenue')) await crediter(env, id, BONUS_BIENVENUE);
    await env.DB.prepare("INSERT INTO cadeaux (de, a, quoi, cree, vu) VALUES (?, ?, 'vip', ?, 0)").bind(moi.id, id, now).run();
    return rep({ ok: true });
  }
  if (m === 'POST' && chemin === '/v1/cadeaux/vu') {
    if (!moi) return erreur(401, 'jeton absent ou invalide');
    await env.DB.prepare('UPDATE cadeaux SET vu = 1 WHERE a = ?').bind(moi.id).run(); return rep({ ok: true });
  }

  if (chemin === '/v1/amis' && m === 'GET') {
    if (!moi) return erreur(401, 'jeton absent ou invalide');
    const liens = await env.DB.prepare(MES_AMIS + "SELECT a, b FROM amis WHERE etat = 'ami' AND a IN (SELECT id FROM mes) AND b IN (SELECT id FROM mes)").bind(moi.id, moi.id, moi.id).all(), communs = {};
    for (const x of liens.results) { communs[x.a] = (communs[x.a] || 0) + 1; communs[x.b] = (communs[x.b] || 0) + 1; }
    const cad = await env.DB.prepare('SELECT c.quoi, c.cree, j.pseudo FROM cadeaux c JOIN joueurs j ON j.id = c.de WHERE c.a = ? AND c.vu = 0 ORDER BY c.id').bind(moi.id).all();
    const l = await env.DB.prepare('SELECT a.etat, a.de, j.id, j.pseudo, j.niveau, (SELECT 1 FROM vip v WHERE v.joueur_id = j.id) AS vip, (SELECT p.statut FROM presence p WHERE p.joueur_id = j.id AND p.vu > ?) AS statut FROM amis a JOIN joueurs j ON j.id = (CASE WHEN a.a = ? THEN a.b ELSE a.a END) WHERE (a.a = ? OR a.b = ?) AND j.banni = 0 ORDER BY j.pseudo_min').bind(maintenant() - AMIS_PRESENCE, moi.id, moi.id, moi.id).all();
    const amis = [], recues = [], envoyees = [];
    for (const x of l.results) {
      const o = { id: x.id, pseudo: x.pseudo, niveau: x.niveau, vip: !!x.vip, communs: communs[x.id] || 0 };
      if (x.etat === 'ami') amis.push({ ...o, statut: x.statut || '', en_ligne: !!x.statut });
      else (x.de === moi.id ? envoyees : recues).push(o);
    }
    return rep({ amis, recues, envoyees, cadeaux: cad.results.map((x) => ({ de: x.pseudo, quoi: x.quoi, cree: x.cree })) });
  }
  if (chemin === '/v1/joueurs/recherche' && m === 'GET') {
    if (!moi) return erreur(401, 'jeton absent ou invalide');
    if (!(await limite(env, 'rech:' + moi.id, 120, 3600))) return erreur(429, 'trop de recherches, patientez un peu');
    const q = propre(url.searchParams.get('q'), 20).toLowerCase(); if (q.length < 2) return rep({ joueurs: [] });
    const motif = q.replace(/[\\%_]/g, (c) => '\\' + c) + '%';
    const l = await env.DB.prepare(MES_AMIS + "SELECT j.id, j.pseudo, j.niveau, (SELECT 1 FROM vip v WHERE v.joueur_id = j.id) AS vip, (SELECT a.etat || ':' || a.de FROM amis a WHERE (a.a = j.id AND a.b = ?) OR (a.b = j.id AND a.a = ?)) AS lien, (SELECT COUNT(*) FROM amis k WHERE k.etat = 'ami' AND ((k.a = j.id AND k.b IN (SELECT id FROM mes)) OR (k.b = j.id AND k.a IN (SELECT id FROM mes)))) AS communs FROM joueurs j WHERE j.pseudo_min LIKE ? ESCAPE '\\' AND j.id <> ? AND j.banni = 0 ORDER BY length(j.pseudo_min), j.pseudo_min LIMIT 10").bind(moi.id, moi.id, moi.id, moi.id, moi.id, motif, moi.id).all();
    return rep({ joueurs: l.results.map((x) => ({ id: x.id, pseudo: x.pseudo, niveau: x.niveau, vip: !!x.vip, communs: x.communs || 0, lien: !x.lien ? '' : (x.lien.startsWith('ami') ? 'ami' : (x.lien.endsWith(':' + moi.id) ? 'envoyee' : 'recue')) })) });
  }
  if (chemin === '/v1/amis/demande' && m === 'POST') {
    if (!moi) return erreur(401, 'jeton absent ou invalide');
    if (!(await limite(env, 'ami:' + moi.id, 30, 3600))) return erreur(429, 'trop de demandes d\'ami, réessayez plus tard');
    const c = await corps(request), id = propre(c.id, 40).toLowerCase();
    if (!/^[0-9a-f]{16}$/.test(id) || id === moi.id) return erreur(400, 'joueur invalide');
    const cible = await env.DB.prepare('SELECT id, pseudo FROM joueurs WHERE id = ? AND banni = 0').bind(id).first(); if (!cible) return erreur(404, 'joueur introuvable');
    const [a, b2] = moi.id < id ? [moi.id, id] : [id, moi.id], lien = await env.DB.prepare('SELECT etat, de FROM amis WHERE a = ? AND b = ?').bind(a, b2).first();
    if (lien && lien.etat === 'ami') return rep({ etat: 'ami' });
    if (lien && lien.de !== moi.id) { await env.DB.prepare("UPDATE amis SET etat = 'ami', cree = ? WHERE a = ? AND b = ?").bind(maintenant(), a, b2).run(); return rep({ etat: 'ami' }); }   // il m'avait déjà demandé : nous voilà amis
    if (lien) return rep({ etat: 'envoyee' });
    const n = await env.DB.prepare('SELECT COUNT(*) AS n FROM amis WHERE a = ? OR b = ?').bind(moi.id, moi.id).first();
    if (n.n >= AMIS_MAX) return erreur(403, 'liste d\'amis pleine (' + AMIS_MAX + ' au plus, demandes comprises)');
    await env.DB.prepare("INSERT INTO amis (a, b, etat, de, cree) VALUES (?, ?, 'demande', ?, ?)").bind(a, b2, moi.id, maintenant()).run();
    return rep({ etat: 'envoyee' }, 201);
  }
  r = chemin.match(/^\/v1\/amis\/([0-9a-f]{16})\/accepter$/);
  if (r && m === 'POST') {
    if (!moi) return erreur(401, 'jeton absent ou invalide');
    const id = r[1], [a, b2] = moi.id < id ? [moi.id, id] : [id, moi.id];
    const res = await env.DB.prepare("UPDATE amis SET etat = 'ami', cree = ? WHERE a = ? AND b = ? AND etat = 'demande' AND de <> ?").bind(maintenant(), a, b2, moi.id).run();
    if (!res.meta.changes) return erreur(404, 'aucune demande de ce joueur');
    return rep({ etat: 'ami' });
  }
  r = chemin.match(/^\/v1\/amis\/([0-9a-f]{16})$/);
  if (r && m === 'DELETE') {   // refuser une demande reçue, annuler une demande envoyée ou retirer un ami
    if (!moi) return erreur(401, 'jeton absent ou invalide');
    const id = r[1], [a, b2] = moi.id < id ? [moi.id, id] : [id, moi.id];
    await env.DB.prepare('DELETE FROM amis WHERE a = ? AND b = ?').bind(a, b2).run();
    return rep({ ok: true });
  }

  // support : un joueur décrit son souci depuis la page d'un mod ; le ticket part dans le salon Discord de TPS Studios (webhook gardé sur le serveur)
  if (m === 'POST' && chemin === '/v1/support') {
    const cle = moi ? 'sup:' + moi.id : 'supi:' + (await ip(request));
    if (!(await limite(env, cle, 4, 3600))) return erreur(429, 'trop de tickets envoyés : réessayez dans une heure');
    const c = await corps(request), mod = propre(c.mod, 40), nomMod = propre(c.nom_mod, 60), sujet = propre(c.sujet, 40), texte = String(c.message ?? '').replace(/[\u0000-\u0008\u000b-\u001f\u007f]/g, ' ').trim().slice(0, 1000);
    const contact = propre(c.contact, 40), infos = propre(c.infos, 200);
    if (!MOD.test(mod)) return erreur(400, 'mod invalide');
    if (texte.length < 10) return erreur(400, 'décrivez le souci en quelques mots (10 caractères au moins)');
    const webhook = env.DISCORD_WEBHOOK || ((await env.DB.prepare("SELECT valeur FROM reglages WHERE cle = 'webhook_support'").first()) || {}).valeur || '';
    if (!/^https:\/\/(discord\.com|discordapp\.com)\/api\/webhooks\/\d+\/[\w-]+$/.test(webhook)) return erreur(503, 'le support n\'est pas encore configuré par TPS Studios');
    const sans = (t) => t.replace(/@/g, '@​').replace(/`/g, "'");   // aucune mention (@everyone…) ni mise en forme cassée
    const embed = {
      title: '🎫 Ticket · ' + sans(nomMod || mod), description: sans(texte), color: 0xf28c28,
      fields: [{ name: 'Problème', value: sans(sujet || 'Autre'), inline: true }, { name: 'Joueur', value: sans(moi ? moi.pseudo : 'non connecté'), inline: true }, { name: 'Discord', value: sans(contact || '—'), inline: true }, { name: 'Informations', value: sans(infos || '—'), inline: false }],
      footer: { text: 'Lanceur TPS Studios · ' + mod }, timestamp: new Date().toISOString(),
    };
    const r2 = await fetch(webhook + '?wait=true', { method: 'POST', headers: { 'content-type': 'application/json' }, body: JSON.stringify({ username: 'Support TPS Studios', embeds: [embed], allowed_mentions: { parse: [] } }) });
    if (!r2.ok) return erreur(502, 'Discord n\'a pas accepté le ticket (' + r2.status + ')');
    return rep({ ok: true }, 201);
  }
  // reprise d'un compte par l'administrateur (jeton perdu sur son PC) : nouveau secret pour ce pseudo, l'ancien ne marche plus
  if (m === 'POST' && chemin === '/v1/admin/reprendre') {
    if (!await adminOk(request, env)) return erreur(403, 'action refusée');
    const c = await corps(request), pseudo = propre(c.pseudo, 20);
    const j = await env.DB.prepare('SELECT id, pseudo, admin FROM joueurs WHERE pseudo_min = ? AND banni = 0').bind(pseudo.toLowerCase()).first();
    if (!j) return erreur(404, 'aucun joueur avec ce pseudo');
    const secret = b64url(alea(32));
    await env.DB.prepare('UPDATE joueurs SET secret_hash = ? WHERE id = ?').bind(await sha256(secret), j.id).run();
    return rep({ id: j.id, pseudo: j.pseudo, jeton: j.id + '.' + secret, admin: !!j.admin });
  }
  // réglages du serveur (clé administrateur) : adresse du webhook Discord des tickets, jamais renvoyée en clair
  if (chemin === '/v1/admin/reglages' && (m === 'PUT' || m === 'GET')) {
    if (!await adminOk(request, env)) return erreur(403, 'action refusée');
    if (m === 'GET') { const w = await env.DB.prepare("SELECT valeur FROM reglages WHERE cle = 'webhook_support'").first(); return rep({ webhook_support: !!(env.DISCORD_WEBHOOK || (w && w.valeur)) }); }
    const c = await corps(request), w = String(c.webhook_support ?? '').trim();
    if (w && !/^https:\/\/(discord\.com|discordapp\.com)\/api\/webhooks\/\d+\/[\w-]+$/.test(w)) return erreur(400, 'adresse de webhook Discord invalide (https://discord.com/api/webhooks/…)');
    if (w) await env.DB.prepare("INSERT OR REPLACE INTO reglages (cle, valeur) VALUES ('webhook_support', ?)").bind(w).run(); else await env.DB.prepare("DELETE FROM reglages WHERE cle = 'webhook_support'").run();
    return rep({ ok: true, webhook_support: !!w });
  }

  // VIP : un code acheté s'active une seule fois et devient lié au compte du joueur (il ne peut pas être réutilisé ni partagé)
  if (m === 'POST' && chemin === '/v1/vip/activer') {
    if (!moi) return erreur(401, 'jeton absent ou invalide');
    if (!(await limite(env, 'vipj:' + moi.id, 6, 3600)) || !(await limite(env, 'vipi:' + (await ip(request)), 12, 3600))) return erreur(429, 'trop d\'essais, réessayez dans une heure');
    if (moi.vip_depuis) return rep({ vip: true, deja: true });
    const c = await corps(request), code = String(c.code ?? '').toUpperCase().replace(/[\s_]/g, '').replace(/^TPSVIP/, 'TPS-VIP-').replace(/^(TPS-VIP-)([A-Z0-9]{4})-?([A-Z0-9]{4})-?([A-Z0-9]{4})$/, '$1$2-$3-$4');
    if (!CODE_VIP.test(code)) return erreur(400, 'code VIP invalide (format TPS-VIP-XXXX-XXXX-XXXX)');
    const h = await empreinteVip(code), now = maintenant();
    const res = await env.DB.prepare('UPDATE vip_codes SET utilise_par = ?, utilise_le = ? WHERE code_hash = ? AND utilise_par IS NULL').bind(moi.id, now, h).run();
    if (!res.meta.changes) return erreur(404, 'code VIP inconnu ou déjà utilisé');
    await env.DB.prepare('INSERT OR IGNORE INTO vip (joueur_id, depuis) VALUES (?, ?)').bind(moi.id, now).run();
    if (await premiereFois(env, moi.id, 'bienvenue')) await crediter(env, moi.id, BONUS_BIENVENUE);
    return rep({ vip: true, vip_depuis: now });
  }
  if (chemin === '/v1/vip/points' && m === 'GET') {
    if (!moi) return erreur(401, 'jeton absent ou invalide');
    return rep({ vip: !!moi.vip_depuis, ...(await etatPoints(env, moi.id)) });
  }
  if (chemin === '/v1/vip/pointage' && m === 'POST') {   // bonus du jour : une fois par jour (UTC) et par compte
    if (!moi) return erreur(401, 'jeton absent ou invalide');
    if (!moi.vip_depuis) return erreur(403, 'réservé aux membres VIP');
    const jour = new Date().toISOString().slice(0, 10);
    await env.DB.prepare('INSERT OR IGNORE INTO points (joueur_id) VALUES (?)').bind(moi.id).run();
    const r2 = await env.DB.prepare('UPDATE points SET solde = solde + ?, total = total + ?, dernier_jour = ? WHERE joueur_id = ? AND dernier_jour <> ?').bind(BONUS_JOUR, BONUS_JOUR, jour, moi.id, jour).run();
    return rep({ gagne: r2.meta.changes === 1 ? BONUS_JOUR : 0, ...(await etatPoints(env, moi.id)) });
  }
  if (chemin === '/v1/boutique/acheter' && m === 'POST') {
    if (!moi) return erreur(401, 'jeton absent ou invalide');
    if (!moi.vip_depuis) return erreur(403, 'les achats en points sont réservés aux membres VIP');
    if (!(await limite(env, 'achat:' + moi.id, 20, 3600))) return erreur(429, 'trop d\'achats, réessayez plus tard');
    const c = await corps(request), article = String(c.article ?? '');
    if (!ARTICLE.test(article)) return erreur(400, 'article invalide');
    const prix = article.startsWith('mod:') ? await prixMod(env, article.slice(4)) : (COSMETIQUES[article] || 0);
    if (!prix) return erreur(404, 'article non disponible en points');
    if (await env.DB.prepare('SELECT 1 FROM achats WHERE joueur_id = ? AND article = ?').bind(moi.id, article).first()) return erreur(409, 'article déjà acheté');
    const d = await env.DB.prepare('UPDATE points SET solde = solde - ? WHERE joueur_id = ? AND solde >= ?').bind(prix, moi.id, prix).run();
    if (d.meta.changes !== 1) return erreur(402, 'points insuffisants');
    const ins = await env.DB.prepare('INSERT OR IGNORE INTO achats (joueur_id, article, prix, cree) VALUES (?, ?, ?, ?)').bind(moi.id, article, prix, maintenant()).run();
    if (ins.meta.changes !== 1) { await crediter(env, moi.id, 0); await env.DB.prepare('UPDATE points SET solde = solde + ? WHERE joueur_id = ?').bind(prix, moi.id).run(); return erreur(409, 'article déjà acheté'); }   // achat en double simultané : remboursé
    return rep({ article, prix, ...(await etatPoints(env, moi.id)) });
  }
  if (m === 'POST' && chemin === '/v1/admin/points') {   // geste commercial : créditer un joueur
    if (!await adminOk(request, env)) return erreur(403, 'action refusée');
    const c = await corps(request), id = propre(c.id, 40), n = Math.max(-100000, Math.min(100000, parseInt(c.n, 10) || 0));
    if (n > 0) await crediter(env, id, n); else if (n < 0) await env.DB.prepare('UPDATE points SET solde = MAX(0, solde + ?) WHERE joueur_id = ?').bind(n, id).run();
    return rep({ ok: true });
  }
  if (m === 'POST' && chemin === '/v1/admin/vip/codes') {
    if (!await adminOk(request, env)) return erreur(403, 'action refusée');
    const c = await corps(request), n = Math.max(1, Math.min(50, parseInt(c.n, 10) || 1)), note = propre(c.note, 60), now = maintenant(), codes = [];
    for (let i = 0; i < n; i++) { const code = nouveauCode(); await env.DB.prepare('INSERT INTO vip_codes (code_hash, note, cree) VALUES (?, ?, ?)').bind(await empreinteVip(code), note, now).run(); codes.push(code); }
    return rep({ codes }, 201);   // affichés une seule fois : seule leur empreinte est conservée
  }
  if (m === 'GET' && chemin === '/v1/admin/vip') {
    if (!await adminOk(request, env)) return erreur(403, 'action refusée');
    const t = await env.DB.prepare('SELECT COUNT(*) AS n, SUM(CASE WHEN utilise_par IS NULL THEN 0 ELSE 1 END) AS u FROM vip_codes').first(), v = await env.DB.prepare('SELECT COUNT(*) AS n FROM vip').first();
    return rep({ codes: t.n || 0, utilises: t.u || 0, membres: v.n || 0 });
  }
  // rôle VIP sur Discord : l'équipe vérifie l'identifiant VIP qu'un joueur donne sur le serveur Discord
  if (m === 'GET' && chemin === '/v1/admin/vip/verifier') {
    if (!await adminOk(request, env)) return erreur(403, 'action refusée');
    const id = propre(url.searchParams.get('id'), 40).toLowerCase();
    if (!/^[0-9a-f]{16}$/.test(id)) return erreur(400, 'identifiant VIP invalide (16 caractères)');
    const j = await env.DB.prepare('SELECT j.pseudo, j.banni, (SELECT depuis FROM vip v WHERE v.joueur_id = j.id) AS depuis FROM joueurs j WHERE j.id = ?').bind(id).first();
    if (!j) return erreur(404, 'aucun joueur avec cet identifiant');
    return rep({ pseudo: j.pseudo, vip: !!j.depuis && !j.banni, depuis: j.depuis || 0 });
  }
  if (m === 'POST' && chemin === '/v1/admin/vip/retirer') {
    if (!await adminOk(request, env)) return erreur(403, 'action refusée');
    const c = await corps(request), id = propre(c.id, 40);
    await env.DB.prepare('DELETE FROM vip WHERE joueur_id = ?').bind(id).run();
    return rep({ ok: true });
  }

  if (m === 'POST' && chemin === '/v1/admin/bannir') {
    if (!await adminOk(request, env)) return erreur(403, 'action refusée');
    const c = await corps(request), id = propre(c.id, 40);
    await env.DB.prepare('UPDATE joueurs SET banni = 1 WHERE id = ?').bind(id).run();
    return rep({ ok: true });
  }

  return erreur(404, 'introuvable');
}

export default {
  async fetch(request, env) {
    try { return await route(request, env); }
    catch (e) {
      if (e && e.statut) return erreur(e.statut, e.msg);
      console.error('erreur serveur', e && e.message);
      return erreur(500, 'erreur du serveur');
    }
  },
};

// une « salle » par page de mod : diffuse aux navigateurs abonnés (WebSocket) les nouveaux likes, commentaires et suppressions ; les clients ne peuvent rien y écrire
export class Salle {
  constructor(state) { this.state = state; }
  async fetch(request) {
    const url = new URL(request.url);
    if (url.pathname === '/emettre' && request.method === 'POST') {
      const msg = await request.text();
      for (const ws of this.state.getWebSockets()) { try { ws.send(msg); } catch { /* connexion fermée */ } }
      return new Response('ok');
    }
    if (request.headers.get('upgrade') !== 'websocket') return new Response('WebSocket attendu', { status: 426 });
    if (this.state.getWebSockets().length >= 300) return new Response('salle pleine', { status: 503 });
    const paire = new WebSocketPair(); this.state.acceptWebSocket(paire[1]);
    return new Response(null, { status: 101, webSocket: paire[0] });
  }
  webSocketMessage(ws, message) { if (message === 'ping') ws.send('pong'); }   // seul le maintien de connexion est accepté
  webSocketClose(ws) { try { ws.close(); } catch { /* déjà fermée */ } }
}
