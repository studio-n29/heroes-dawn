#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>
    /// Réécriture « définitive » des textes narratifs de la boucle New York (acte I) : un ton épique et lyrique de récit de super-héros,
    /// dans l'esprit des grandes sagas (Secret Wars) : des phrases courtes, des serments, des enjeux qui dépassent les personnages.
    /// Ces textes remplacent ceux des autres fichiers de localisation (chargés avant).
    /// </summary>
    public static partial class Loc
    {
        static partial void ExtendEpic(Dictionary<string, string> d)
        {
            // ---- Rayner, le fil du QG (quête principale)
            d["qg.fil.zone_ny_1_1"] = "Sous les grues de Red Hook, l'AMIF fait glisser dans la nuit des méta-humains enchaînés. Franchis les grilles, plonge le port dans le noir, arrache les registres et fais taire l'échange. Ramène la vérité.";
            d["qg.fil.zone_ny_1_2"] = "Les preuves mènent à Crocodile, seigneur du crime qui rachète les captifs de l'AMIF. À Brownsville, abats son lieutenant et brise les cellules : chaque chaîne rompue est une promesse tenue.";
            d["qg.fil.zone_ny_1_3"] = "Crocodile tient Coney Island en otage et t'attend au bout de son piège. Sauve les innocents, déjoue ses hommes, puis affronte-le : ce soir, une légende se joue.";
            d["qg.fil.zone_ny_1_4"] = "L'AMIF a verrouillé Brooklyn. Perce son cordon, aveugle ses yeux électroniques et tiens le pont : Rayner garde un dernier atout.";
            d["qg.fil.done"] = "Red Hook est à nous. L'Aube se lève sur New York : entraîne les tiens, soigne-les, car la fracture ne dort jamais.";

            // ---- Zones : présentation
            d["zones.ny_1_1.desc"] = "Une rumeur court sur les docks de Red Hook : l'AMIF y enchaîne des méta-humains pour les faire disparaître dans la nuit. Frapper de front condamnerait l'Aube. Il faut des preuves, et assez de courage pour les prendre.";
            d["zones.ny_1_2.desc"] = "Plus sombre qu'on ne le craignait : l'AMIF vend ses captifs à Antonio Kribelchi, dit « Crocodile », roi du crime et méta-humain lui-même, pour payer ses armes. À Brownsville, votre équipe descend dans les cellules pour rendre la lumière aux otages.";
            d["zones.ny_1_3.desc"] = "Crocodile défie l'Aube : il a pris Coney Island en otage pour attirer vos héros dans son piège. Les innocents d'abord. Le tyran ensuite.";
            d["zones.ny_1_4.desc"] = "Après Coney Island, l'AMIF quadrille Brooklyn pour écraser d'un seul coup l'Aube et le gang de Crocodile. Percer le cordon, tenir le pont : ce soir, un pont devient la frontière entre deux mondes.";

            // ---- Objectifs
            d["objectives.ny11.infiltrate"] = "Franchir les grilles du quai";
            d["objectives.ny11.generator"] = "Plonger le port dans le noir (générateur)";
            d["objectives.ny11.documents"] = "Arracher les registres de transport";
            d["objectives.ny11.exchange"] = "Briser l'échange en cours";
            d["objectives.ny11.escape"] = "Disparaître avec la vérité";
            d["objectives.ny12.infiltrate"] = "Pénétrer dans Brownsville";
            d["objectives.ny12.lieutenant"] = "Abattre le lieutenant de Crocodile";
            d["objectives.ny12.hostages"] = "Briser les cellules (crocheter)";
            d["objectives.ny12.evacuate"] = "Conduire les otages à l'abri";
            d["objectives.ny13.civilians"] = "Sauver les civils du piège";
            d["objectives.ny13.trap"] = "Déjouer les hommes de Crocodile";
            d["objectives.ny13.crocodile"] = "Faire tomber Crocodile";
            d["objectives.ny14.cordon"] = "Percer le cordon de l'AMIF";
            d["objectives.ny14.network"] = "Aveugler le réseau de surveillance";
            d["objectives.ny14.bridge"] = "Tenir le pont face au Titan";

            // ---- Archives : ce que l'histoire retient
            d["archives.zone.zone_ny_1_1"] = "Red Hook, une nuit sans lune. Le courant est coupé, les registres arrachés, l'échange brisé : l'Aube tient la preuve que l'AMIF enchaîne et vend des méta-humains. Le plan d'intervention peut commencer.";
            d["archives.zone.zone_ny_1_2"] = "Brownsville. Derrière les cellules, le nom de Crocodile : c'est lui qui rachetait les captifs de l'AMIF. Les otages respirent enfin, mais le seigneur du crime a juré de faire payer l'Aube.";
            d["archives.zone.zone_ny_1_3"] = "Coney Island. Le piège était tendu, l'Aube y est entrée les yeux ouverts. Crocodile est tombé, mais l'île porte les cicatrices de la bataille : certaines victoires coûtent un morceau de ville.";
            d["archives.zone.zone_ny_1_4"] = "Le pont de Brooklyn. Rayner a arrêté la guerre d'un seul secret : Stroke a reculé. Crocodile portera le crime, et le Général promet que l'Aube devra répondre de tout une fois les Accords de Salem signés.";

            // ---- Dialogues des chefs de zone
            d["narration.ny_1_1.n23.before.0"] = "« Vous avez traversé mon royaume de rouille et de sel pour mourir sur ce quai. Admirable. Inutile. »";
            d["narration.ny_1_1.n23.before.1"] = "Anchor, tu es le mur. Frost, Echo : couvrez-le. Cette nuit nous appartient, ou personne ne la verra finir.";
            d["narration.ny_1_1.n23.after.0"] = "Brick s'écroule parmi les conteneurs, et le vacarme du port se tait. Les cages sont ouvertes, les registres parlent : la vérité est entre vos mains.";
            d["narration.ny_1_1.n23.after.1"] = "Partez avant l'aube. L'AMIF reviendra, et quand elle reviendra, elle trouvera des héros.";
            d["narration.ny_1_2.n14.before.0"] = "« Crocodile m'a confié les clés de l'enfer. Entrez : il vous garde une place. »";
            d["narration.ny_1_2.n14.before.1"] = "Derrière lui, des innocents. Devant nous, une porte. On ne s'arrête pas, pas ce soir.";
            d["narration.ny_1_2.n14.after.0"] = "« Vous croyez avoir gagné ? Vous venez d'allumer la mèche. Crocodile fera pleuvoir le feu sur vos têtes. »";
            d["narration.ny_1_2.n19.after.0"] = "Les chaînes tombent. Les otages respirent l'air libre et, pour la première fois depuis longtemps, lèvent les yeux vers un ciel qui ne leur ment pas.";
            d["narration.ny_1_2.n19.after.1"] = "Mais les ombres s'allongent. Crocodile n'oublie jamais, et ce qu'il jure, il l'écrit dans le sang.";
            d["narration.ny_1_3.n18.before.0"] = "« Toute l'Aube sur mon île… Je n'aurais pas osé rêver pareil cadeau. Soyez les bienvenus à vos propres funérailles. »";
            d["narration.ny_1_3.n18.before.1"] = "Nous savions que c'était un piège. Qu'importe : quand le monde appelle, on répond. Les civils d'abord, puis lui.";
            d["narration.ny_1_3.n18.after.0"] = "« Impossible… Je suis… un roi… »";
            d["narration.ny_1_3.n18.after.1"] = "Crocodile s'effondre, et les manèges éteints de Coney Island se taisent. La victoire a un goût de cendre : certaines blessures ne guérissent qu'en légende.";
            d["narration.ny_1_4.n18.before.0"] = "« Vous êtes cernés. Déposez vos pouvoirs aux pieds de l'Histoire, et peut-être l'Histoire vous épargnera-t-elle. »";
            d["narration.ny_1_4.n18.before.1"] = "Tenez la ligne. Un pont n'est qu'un serment de pierre : aujourd'hui, c'est nous qui le tenons.";
            d["narration.ny_1_4.n18.after.0"] = "« Assez ! Stroke, j'ai de quoi mettre la ville entière face à vos petits secrets. Faut-il les dire ici, devant vos hommes ? »";
            d["narration.ny_1_4.n18.after.1"] = "« … Repli. Tout le monde. »";
            d["narration.ny_1_4.n18.after.2"] = "« Crocodile portera le crime. Une ombre de plus au tableau : il fera un coupable admirable. »";
            d["narration.ny_1_4.n18.after.3"] = "« Souvenez-vous de ce jour, Rayner. Quand les Accords de Salem seront signés, l'Aube devra répondre de tout. »";

            // ---- Tutoriel : la voix de Rayner
            d["tutorial.step.Welcome.title"] = "BIENVENUE AU QG";
            d["tutorial.step.Welcome.body"] = "Je suis Rayner. Ici, dans la section de Red Hook, brûle le dernier phare d'un monde qui vacille. Avant la vraie bataille, un petit entraînement.";
            d["tutorial.step.Results.body"] = "Le premier combat est gagné. Touche CONTINUER : un nouvel allié rejoint l'Aube.";
            d["tutorial.step.Squad.body"] = "Un héros ne combat jamais seul. Un nouvel agent te rejoint, et leurs pouvoirs s'éveillent ensemble : deux familles différentes forment une synergie.";
        }
    }
}
