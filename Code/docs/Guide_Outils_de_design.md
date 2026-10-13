# Outils de design (Unity)

Quatre fenêtres sous **Heroes Dawn ▸ Outils**. Elles éditent des fichiers JSON du projet (`Assets/_Project/Resources/Content/`) que le jeu lit au démarrage à la place du contenu écrit dans le code.

| Fenêtre | Fichier | Ce qu'on y fait |
|---|---|---|
| **Agents** | `agents.json` | Ajouter, dupliquer, modifier, supprimer un agent complet : nom, famille, rôle, rareté, statistiques, déblocage (boutique, arc, week-end, prouesse + zone), trois compétences. |
| **Compétences** | `skills.json` | Une compétence : nom, emplacement, recharge, cible, liste d'effets (dégâts, soin, bouclier, statut, recul, élan, jauge). La description du jeu est calculée à partir des effets ; l'aperçu la montre. |
| **Objets** | `items.json` | Un objet : nom, rareté, bonus en % (PV, attaque, défense, vitesse), effet spécial, prix. |
| **Narration** | zones (`Resources/Zones`) et `texts.json` | Onglet *Dialogues* : répliques jouées avant et après une case de chaque zone (créer, modifier, réordonner, supprimer). Onglet *Textes* : modifier un texte du jeu (tutoriel, astuces, première connexion, quêtes, QG, combat…), en ajouter, revenir à l'original. |

## Règles communes
- **Enregistrer** est refusé tant que la validation signale une erreur (identifiant invalide ou en double, compétence introuvable ou partagée, effet sans durée, texte sans le `{0}` de l'original…). Les avertissements (équipe de départ incomplète) n'empêchent pas d'enregistrer.
- Un fichier présent **remplace la liste du code en entier** : supprimer une ligne supprime l'élément du jeu. Le menu *Réinitialiser les données depuis le code* refait les trois fichiers agents / compétences / objets depuis le code.
- Le nom d'un agent saisi dans l'outil prime sur son nom de code d'origine.
- Un agent supprimé ou renommé n'efface pas les sauvegardes des joueurs : l'agent absent est simplement ignoré à leur chargement.
- Les ennemis, les cartes du grimoire, les synergies et le tutoriel (ses étapes sont codées) ne se créent pas dans ces outils ; leurs textes se modifient dans l'onglet *Textes*.

## Travailler avec les feuilles Google
Les feuilles « Heroes Dawn - Agents / Compétences / Quêtes / Objets » sont une vue de lecture. Menu *Heroes Dawn ▸ Équilibrage ▸ Exporter le contenu en CSV* pour les régénérer depuis le jeu.
