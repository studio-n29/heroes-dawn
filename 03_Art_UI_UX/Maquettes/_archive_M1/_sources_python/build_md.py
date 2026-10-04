import sys
sys.path.insert(0, "/home/claude/mock")
import screens_a, screens_b  # noqa: F401
from screens_a import SCREENS

STYL = [
    ("S01-titre", "S01 — Titre", "L'illustration clé occupe tout l'écran ; le logo-titre domine, un seul appel à l'action (JOUER) dans la zone du pouce. Voile sombre en bas pour la lisibilité."),
    ("S02-qg", "S02 — QG", "Chaque pièce est une case de BD avec son récitatif jaune, son niveau et son état. MISSION est le bouton le plus fort de l'écran (rouge, en haut à droite) ; les badges rouges signalent ce qui attend le joueur."),
    ("S03-plateau", "S03 — Plateau, choix de destination", "Cases rondes encrées, couleur = type, destinations cerclées de jaune avec leur coût, chemin en pointillés. Le brouillard est une masse sombre et tramée ; un récitatif blanc annonce le nouveau secteur."),
    ("S04-combat", "S04 — Combat, ciblage et combo", "Les alliés sont présentés en cases inclinées façon planche splash ; les ennemis en silhouettes jusqu'à la production des illustrations. Les retours (dégâts, CRIT !, COMBO !) reprennent les onomatopées et éclats des comics."),
    ("S05-recrutement", "S05 — Recrutement", "Cinq cartes au format collection : portrait encadré, rareté en récitatif (cuivre, argent, or), bande de couleur de famille, coût en PR. La Légende est surélevée et entourée d'un halo doré."),
]

RULES = [
    "Format de référence : paysage 1920×1080 (mise à l'échelle 0,5 largeur/hauteur). Zone de sécurité mobile : 88 px de chaque côté (pointillés rouges sur les wireframes), aucun élément interactif dedans.",
    "Cibles tactiles : 96 px minimum en 1080p (environ 9 mm), 24 px d'écart entre deux boutons voisins.",
    "Placement constant : retour en haut à gauche, ressources et ⚙ en haut à droite, action principale en bas à droite (zone du pouce droit).",
    "Une seule action principale par écran (bouton jaune ou rouge) ; les actions secondaires sont sombres.",
    "Au plus une modale ouverte au-dessus d'un écran (confirmation comprise) ; le plateau et le combat se mettent en pause derrière une modale, sauf le temps actif du combat qui ne s'arrête que pour la retraite.",
    "Couleur jamais seule porteuse d'information : chaque famille a aussi son icône, chaque état a aussi un libellé (accessibilité daltonisme).",
    "Textes : Bangers pour les titres, boutons et chiffres ; Comic Neue pour tout texte de plus de 4 mots. Taille minimale 20 px en 1080p.",
    "PC : tout est cliquable à la souris ; raccourcis indiqués entre crochets sur les boutons ([Q], [W], [E], Espace…).",
]


def main():
    out = []
    out.append("# Maquettes & screen flow\n")
    out.append("> Rendu cible du **jeu complet** (19 écrans du document `01_UX_Ecrans.md`), en paysage 1920×1080.\n"
               "> **Wireframes** = structure et contenu (niveaux de gris, pastilles rouges renvoyant aux légendes).\n"
               "> **Maquettes stylisées** = intention visuelle, DA comics de référence *Secret Wars* (2015).\n"
               "> Les portraits des maquettes sont provisoires (extraits de l'illustration clé). Images dans `Maquettes/`, scripts de génération dans `Maquettes/_sources/`.\n")
    out.append("## 1. Screen flow\n")
    out.append("![Screen flow](Maquettes/flow/F01-screen-flow.png)\n")
    out.append("- Le **QG** est le hub : 5 pièces (salon, infirmerie, opérations, labo, archives) et 5 menus permanents (recrutement, équipe, pass, boutique, paramètres).\n"
               "- La **boucle de mission** : carte → préparation → plateau → case (combat, mini-jeu, événement, repos) → résultats → plateau.\n"
               "- Sorties de la boucle : zone sécurisée ou escouade épuisée → QG ; « Retour QG » depuis le plateau referme le brouillard.\n"
               "- Le **prologue** ne se joue qu'au premier lancement, puis mène au QG.\n")
    out.append("## 2. Parcours joueur\n")
    out.append("![Parcours joueur](Maquettes/flow/F02-parcours.png)\n")
    out.append("- **Session type (5-10 min)** : collecter au QG, reprendre la zone, 2 à 4 lancers, un combat, résultats.\n"
               "- **Premier lancement (~5 min)** : scène d'introduction, mini-plateau scripté de 8 cases, premier combat avec combo guidé, arrivée au QG, recrutement gratuit.\n"
               "- **Boucle de retour (2 min)** : labo, expéditions (récupérer puis relancer), infirmerie, pool de recrutement.\n")
    out.append("## 3. Règles de conception\n")
    out += [f"- {r}" for r in RULES]
    out.append("")
    out.append("## 4. Wireframes annotés\n")
    out.append("| Réf. | Écran |\n|---|---|")
    for s in SCREENS:
        out.append(f"| {s['id']} | {s['name']} |")
    out.append("")
    for s in SCREENS:
        out.append(f"### {s['id']} — {s['name']}\n")
        out.append(f"![{s['id']}](Maquettes/wireframes/{s['id']}-{s['slug']}.png)\n")
        for i, line in enumerate(s["legend"], 1):
            out.append(f"{i}. {line}")
        out.append("")
    out.append("## 5. Maquettes stylisées\n")
    for f, title, desc in STYL:
        out.append(f"### {title}\n")
        out.append(f"![{title}](Maquettes/stylisees/{f}.png)\n")
        out.append(desc + "\n")
    out.append("## 6. Kit UI\n")
    out.append("![Kit UI](Maquettes/stylisees/S06-kit-ui.png)\n")
    out.append("| Composant | Règle |\n|---|---|\n"
               "| Bouton principal | Jaune #FFD23F, texte Bangers encre, contour noir 5 px, ombre décalée 7 px ; un seul par écran |\n"
               "| Bouton danger | Rouge #E5484D (retraite, suppression, MISSION) |\n"
               "| Bouton secondaire | Gris foncé #2B3140, texte blanc |\n"
               "| Récitatif | Bandeau jaune (ou blanc pour un retour d'info), lettrage Bangers, ombre 5 px |\n"
               "| Case de BD | Fond #1E222B, contour noir 5 px, rayon 14 px, ombre décalée 9 px, trame de points discrète |\n"
               "| Jauges | PV allié vert, PV ennemi rouge, ATB jaune, fatigue verte puis orange au-delà de 60 %, bouclier bleu clair |\n"
               "| Familles | Matière ambre, Élémentaire cyan, Physique/Bio rouge, Perception violet, toujours avec leur icône |\n"
               "| Retours de combat | Chiffres Bangers encrés (dégâts blancs, critiques jaunes, soins verts), éclats « CRIT ! » et « COMBO ! » |\n")
    out.append("## 7. Écarts avec le prototype M1\n")
    out.append("Le prototype jouable ne couvre que : titre, QG simplifié (onglets Équipe et Recrutement + salle des opérations), plateau de la zone 1.1, combat, résultats, et les fenêtres de mini-jeu « Démonstration », d'événement et de repos. "
               "Les pièces du QG, la carte de la ville, la préparation, le pass, la boutique et les paramètres arrivent aux jalons suivants (voir `05_Production/Roadmap.md`).\n")
    open("/home/claude/mock/out/03_Maquettes_Screen_Flow.md", "w", encoding="utf-8").write("\n".join(out))
    print("ok")


main()
