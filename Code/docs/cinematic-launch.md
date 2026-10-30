# Cinématique de lancement — récit développé de Brie

Version actuelle : 31 plans, 250 secondes (4 min 10 s) de montage hors chargements. La limite globale de trois minutes est supprimée pour laisser le récit se terminer. Cette version remplace le montage court de 22 plans.

Le lancement reste : splashscreen Unity → cinématique à chaque lancement → connexion. Voix off désactivées, musique dédiée Four-Note Awakening, police Barlow Condensed SemiBold et ombrage des sous-titres conservés.

## Récit et montage

Brie raconte la création des méta-humains et de l'Apex Unit, le rival soviétique Viktor, la mort et le réveil d'Eda, la X Society, le refus du recrutement et le massacre de ses proches, le piège de Viktor, le rituel de Mephisto, leur fuite et leur rapprochement. Le Vatican est rétabli comme une véritable séquence : enquête, confrontation, blessure de Brie, perte de contrôle, destruction, mort puis régénération d'Eda. Paris montre ensuite la bataille perdue, la brèche qui grandit, les doutes de Brie, le retour d'Eda, l'extraction de Mephisto, le sacrifice, l'onde mondiale et la fondation de The Dawn.

Les plans demandés S15, S28, S29, S30, S31, S35 et S36 sont tous présents. La vidéo s15 est également incluse comme S17 : le plan narratif S15 correspond à s13 dans le montage source. Les sous-titres expliquent les liens de cause à effet et restent à moins de 20 caractères par seconde. Le découpage complet est dans `02_Lore_Narration/07_Cinematique_Brie_Montage.md`.

## Cadence et lisibilité

La vitesse est calculée pour chaque vidéo : durée native divisée par la durée du plan, fondus compris. La vidéo couvre ainsi toute la lecture des sous-titres et arrive à sa fin au changement de plan. Si une source est plus longue que le plan prévu, le plan est prolongé pour la lire entièrement à vitesse native, sans accélération.

Le plan conserve son discret mouvement de cadrage et les fondus entre images. La dernière image reste utilisée pendant la préparation de la vidéo suivante. Les cinq fins de chapitre au noir et le découpage des répliques sont conservés.

## Mobile et validation

L'accès aux vidéos Android/iOS, l'attente de la première image, le format 16:9, la zone sûre du bouton Passer et la texture du plan précédent sont conservés.

Validation : compilation Presentation et Tests.Presentation réussie ; contrôle autonome des 31 vidéos, de la durée de 250 secondes, du minutage et du débit des sous-titres. Tests ajoutés pour les plans demandés et les vitesses selon la cadence source. La fluidité visuelle reste à contrôler dans Unity et sur appareils ; ces contrôles de compilation ne la prouvent pas.
## Musique importée

Four-Note Awakening — Studio N29, source fournie : https://suno.com/s/oqWLA354qdxEv2aC.

Le fichier local fourni `Four-Note Awakening - usesuno.com.wav` est copié et renommé `Assets/_Project/Audio/Music/Cinematic/four-note-awakening.wav`. Le fichier de Downloads reste intact. WAV PCM 16 bits stéréo, 44 100 Hz, durée 250,006984 s. Import Unity en streaming, compression Vorbis qualité 0,7 ; aucune dépendance à Suno à l'exécution.

Entrée MusicId.Cinematic = 6 dans AudioCatalog. Lecture unique sans boucle ni coupe de la réverbération finale. La fin ou le bouton Passer déclenche un fondu vers MusicId.MainTheme. Les préférences de volume du joueur restent appliquées. Les délais de préparation des vidéos peuvent décaler les points musicaux par rapport au tableau éditorial.

Contrôles : WAV lu, copie vérifiée par SHA-256, référence GUID du catalogue vérifiée ; compilation Presentation, Composition et Tests.PlayMode vérifiée. Lecture sonore dans Unity et sur appareils à contrôler.