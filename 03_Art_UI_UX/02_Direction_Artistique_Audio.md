# Direction artistique & audio

## Style : comics moderne
- Encrage marqué, aplats, trames ; palette nocturne urbaine relevée de couleurs vives pour les pouvoirs.
- **Couleur par famille** (toujours doublée d'une icône) : Matière = ambre, Élémentaire = cyan, Physique/Bio = rouge, Perception = violet.
- Le Héraut et le culte : rouge cendre / noir, en écho inversé au Phoenix.

## Assets
| Type | Production | Quantité NY |
|---|---|---|
| Portraits de recrues | Génération IA + retouche manuelle, charte stricte | 35 |
| Personnages en combat | Rig 2D partagé (Unity 2D Animation), quelques modèles 3D low poly | 35 + ~15 ennemis |
| Plateaux | Cartes illustrées vue de dessus, cases en icônes | 5 arrondissements |
| QG | Vue en coupe illustrée, 5 pièces × niveaux | 1 |
| Illustrations d'événements | IA + retouche | ~35 |
| UI | Kit comics (bulles, onomatopées, cadres) | — |

### Charte pour la génération IA
- Fiche de référence par personnage (turnaround, palette, signes distinctifs), prompts et réglages archivés.
- Retouche systématique : encrage, cohérence des mains/visages, logos et textes supprimés.
- Traçabilité : outil, date et licence de chaque asset (voir `05_Production/Obligations_Legales.md`).

## Référence visuelle : Secret Wars (2015)
- **Référence comics officielle du projet** : *Secret Wars* (Marvel, 2015). Rendu peint et détaillé, lumière dramatique, palettes crépusculaires (violet, ambre, rouge de la fracture), compositions épiques en plan large.
- **Traduction dans l'interface** : cadres de case à contour noir épais avec ombre portée décalée, récitatifs jaunes (#FFD23F), titres lettrés à la main, fonds illustrés voilés pour garder la lisibilité.

## Typographie
| Usage | Police | Licence |
|---|---|---|
| Titres, boutons, chiffres, bandeaux | **Bangers** (lettrage de couverture, capitales condensées) | SIL OFL 1.1 |
| Textes courants, descriptions, infobulles | **Comic Neue** Bold / Regular (lettrage de bulles, très lisible) | SIL OFL 1.1 |
- Couverture des accents français vérifiée (é, è, à, ç, œ, «, », …).
- Fichiers et licences : `Code/Heroes Dawn Unity/Assets/_Project/Resources/Fonts/`.

## Identité visuelle
| Élément | Fichier | Usage |
|---|---|---|
| Logo-titre « Heroes Dawn » | `Identite/HD_Title.png` (2464×1430, transparent) | Écran titre, store, marketing |
| Icône « H » | `Identite/HD_Logo_2048.png` (2048², transparent) | Icône d'application, barre du QG |
| Illustration clé paysage | `Identite/HD_Illustration_Landscape_4K.png` (3840×2160) | Écran titre, fonds voilés (QG, plateau, combat) |
| Illustration clé portrait | `Identite/HD_Illustration_Portrait_4K.png` (2160×3840) | Salle des opérations, store mobile, écran de chargement |
- Traitement : détourage, recentrage (icône H reconstruite sur son bord droit coupé), recadrage 16:9 / 9:16, agrandissement ×2 classique.
- **À faire avant la production** : repasser les originaux dans un upscaler IA pour les versions finales (store, marketing).

## Audio
- **Musique** : un thème par arrondissement (5), thème du QG, thème de combat, thème de boss. Banques libres de droits ou achetées.
- **Doublage partiel** : répliques courtes en combat (Légendes, PNJ principaux, Héraut). Dialogues en texte.
- **SFX** : banques sous licence ; signature sonore par famille.
- Voix : en anglais au lancement (sous-titres dans les 5 langues).
