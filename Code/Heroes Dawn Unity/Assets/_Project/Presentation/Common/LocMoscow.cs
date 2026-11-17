#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>
    /// Textes du chapitre Sergueiv (Moscou, parcours d'Agent M) : ennemis, zones, objectifs, dialogues, fragments, archives, carte.
    /// Le français complète le dictionnaire de base (<c>ExtendMoscow</c>) ; l'anglais est fusionné dans <c>En</c> (<see cref="WithMoscowEnglish"/>).
    /// Source des répliques et des fragments : spec 2026-11-13, annexes A et B.
    /// </summary>
    public static partial class Loc
    {
        static partial void ExtendMoscow(Dictionary<string, string> d)
        {
            // --- Ennemis ---
            d["enemies.mos_guard.name"] = "Garde de la mafia";
            d["enemies.mos_smuggler.name"] = "Contrebandier";
            d["enemies.mos_enforcer.name"] = "Homme de main";
            d["enemies.mos_passeur.name"] = "LE PASSEUR";
            d["enemies.mos_passeur.phase2"] = "Les clients arrivent";
            d["enemies.mos_passeur.phase3"] = "Tout doit partir";
            d["skills.mos_guard_a1"] = "Coup de crosse";
            d["skills.mos_guard_a2"] = "Cordon de sécurité";
            d["skills.mos_smuggler_a1"] = "Tir d'avertissement";
            d["skills.mos_smuggler_a2"] = "Livraison express";
            d["skills.mos_enforcer_a1"] = "Poing ganté";
            d["skills.mos_enforcer_a2"] = "Visite de courtoisie";
            d["skills.mos_enforcer_a3"] = "Manteau doublé";
            d["skills.mos_passeur_a1"] = "Marchandage";
            d["skills.mos_passeur_a2"] = "Cargaison scellée";
            d["skills.mos_passeur_a3"] = "Livraison";
            d["skills.mos_passeur_a4"] = "Mise en demeure";
            d["skills.mos_passeur_p2_a2"] = "Clients pressés";
            d["archives.enemy.mos_guard"] = "Garde de la mafia : un cordon humain posté aux entrées. Il se couvre d'un bouclier et attire les coups.";
            d["archives.enemy.mos_smuggler"] = "Contrebandier : tireur de la filière du maglev. Il marque sa cible avant de la livrer au plomb.";
            d["archives.enemy.mos_enforcer"] = "Homme de main : l'élite du comité d'accueil. Ses visites de courtoisie secouent toute l'escouade.";
            d["archives.enemy.mos_passeur"] = "LE PASSEUR : il ne parle jamais de victimes ni d'ennemis, seulement de clients. Tout, chez lui, a un prix.";

            // --- Zones (chapitre Sergueiv) ---
            d["zones.mos_1_1.name"] = "GARE DU MAGLEV — L'ACCUEIL";
            d["zones.mos_1_1.short"] = "Gare du maglev";
            d["zones.mos_1_1.code"] = "Zone M1.1";
            d["zones.mos_1_1.desc"] = "Moscou t'ouvre grand les bras : hologrammes à ton nom, quais balayés, sourires parfaits. Trop parfaits. Un cargo scellé dort pourtant sur la voie 9, sans manifeste. Traverse l'accueil officiel et vois ce qu'on cherche à te cacher.";
            d["zones.mos_1_2.name"] = "MONASTÈRE — LES ARCHIVES";
            d["zones.mos_1_2.short"] = "Monastère";
            d["zones.mos_1_2.code"] = "Zone M1.2";
            d["zones.mos_1_2.desc"] = "Le cargo ne figure sur aucun manifeste, mais quelqu'un a gardé la trace des livraisons : les archives du monastère, fermées au public. Fais-toi passer pour la restauratrice de la fresque et fouille ce qu'elle cache.";
            d["zones.mos_1_3.name"] = "VOIE DU MAGLEV — LE CONVOI";
            d["zones.mos_1_3.short"] = "Voie du maglev";
            d["zones.mos_1_3.code"] = "Zone M1.3";
            d["zones.mos_1_3.desc"] = "Le registre mène à un convoi qui n'existe pas, éclairé comme une fête sous la ville. Rattrape-le, brise son escorte et ouvre les caisses : elles portent un code que personne n'utilise plus.";
            d["zones.mos_1_4.name"] = "DÉPÔT DU MAGLEV — LE PASSEUR";
            d["zones.mos_1_4.short"] = "Dépôt";
            d["zones.mos_1_4.code"] = "Zone M1.4";
            d["zones.mos_1_4.desc"] = "Le convoi finit ici, dans un dépôt où tout a un prix. Son maître ne dit jamais victime ni ennemi, seulement client. Fouille le dépôt, brise sa garde et fais face au Passeur.";
            d["qg.fil.zone_mos_1_1"] = "Qui a déposé le cargo de la voie 9 ?";
            d["qg.fil.zone_mos_1_2"] = "Le registre mentionne Kapotnya.";
            d["qg.fil.zone_mos_1_3"] = "Que contiennent les caisses au code soviétique ?";
            d["qg.fil.zone_mos_1_4"] = "Direction Kapotnya, ses raffineries et son convoi.";

            // --- Objectifs ---
            d["objectives.mos11.welcome"] = "Franchir l'accueil officiel";
            d["objectives.mos11.platforms"] = "Se glisser derrière les quais";
            d["objectives.mos11.escort"] = "Neutraliser l'escorte du cargo";
            d["objectives.mos11.leave"] = "Quitter la gare";
            d["objectives.mos12.archives"] = "Entrer dans les archives";
            d["objectives.mos12.fresco"] = "Faire pivoter la fresque";
            d["objectives.mos12.guardian"] = "Vaincre le gardien des archives";
            d["objectives.mos12.register"] = "Emporter le registre";
            d["objectives.mos13.convoy"] = "Rattraper le convoi";
            d["objectives.mos13.escort"] = "Briser l'escorte du convoi";
            d["objectives.mos13.crates"] = "Ouvrir les caisses";
            d["objectives.mos14.depot"] = "Fouiller le dépôt";
            d["objectives.mos14.guard"] = "Briser la garde du dépôt";
            d["objectives.mos14.passeur"] = "Vaincre le Passeur";

            // --- Dialogues (annexe A de la spec) ---
            d["speaker.agent_m"] = "Agent M";
            d["chapter.mos_sergueiv.intro.agent_m"] = "Elle connaît ces rues, ces codes, ces sourires. Elle n'a jamais regardé ce qu'ils couvraient.";
            d["chapter.mos_sergueiv.intro.agent_d"] = "Il a déjà vu un gang et une agence marcher ensemble. Ici, personne ne s'en cache : tout est poli.";
            d["speaker.agent_d"] = "Agent D";
            d["speaker.passeur"] = "Le Passeur";
            d["speaker.enforcer"] = "Homme de main";
            d["speaker.narrateur"] = "Narrateur";
            d["narration.mos_1_1.intro.0"] = "Les hologrammes saluent l'Aube par son nom. Les quais sont balayés, les sourires parfaits.";
            d["narration.mos_1_1.intro.1"] = "On dirait qu'ils attendaient un chef d'État.";
            d["narration.mos_1_1.intro.2"] = "Ils attendaient quelqu'un. Je ne sais pas encore qui.";
            d["narration.mos_1_1.intro.3"] = "Ces quais ont été lavés cette nuit. Je vois les balais, les seaux… et personne n'a dormi.";
            d["narration.mos_1_1.intro.3.d"] = "Ils ont lavé ces quais pour nous. À New York, on cache les caméras, pas les balais.";
            d["narration.mos_1_1.intro.4"] = "Un accueil de cette qualité, ça se prépare des jours à l'avance.";
            d["narration.mos_1_1.intro.5"] = "Ou ça sert à nous garder les yeux sur la façade.";
            d["narration.mos_1_1.intro.6"] = "Cette odeur de cire et d'encens sur un quai de gare… je la connais.";
            d["narration.mos_1_1.elite.before.0"] = "Bienvenue à Moscou. Le comité d'accueil vous prie de rebrousser chemin.";
            d["narration.mos_1_1.elite.before.1"] = "Un accueil trop parfait cache toujours quelque chose.";
            d["narration.mos_1_1.elite.after.0"] = "Ce cargo scellé n'est sur aucun manifeste. Je vois les camions qui l'ont déposé… trois jours avant nous.";
            d["narration.mos_1_1.elite.after.1"] = "Trois jours ? Avant même qu'on décide de venir ?";
            d["narration.mos_1_1.elite.after.2"] = "Avant même qu'on décide de venir.";
            d["narration.mos_1_1.exit.0"] = "On sort par les archives du monastère. Si quelqu'un nous y attend, ce n'est pas pour nous souhaiter la bienvenue.";
            d["narration.mos_1_2.intro.0"] = "Le monastère est silencieux. Les archives de Sergueiv dorment sous les voûtes, au bout d'un couloir de cierges.";
            d["narration.mos_1_2.intro.1"] = "Les cierges sont électriques. Même les saints ont une facture d'énergie.";
            d["narration.mos_1_2.intro.2"] = "Cette fresque a été repeinte. Moins de dix jours. Le pigment ne ment pas.";
            d["narration.mos_1_2.intro.2.d"] = "Je dessine des murs, pas des fresques. Mais celui-ci a été retouché, j'en suis sûr.";
            d["narration.mos_1_2.intro.3"] = "Quelqu'un a pris la peine de peindre par-dessus un mur. Pour cacher quoi ?";
            d["narration.mos_1_2.intro.4"] = "Je le vois… et je ne vois pas la couleur, je vois la porte.";
            d["narration.mos_1_2.intro.4.d"] = "Je vois le trait qui ne va pas. Je ne vois pas pourquoi.";
            d["narration.mos_1_2.intro.5"] = "Même couloir de cierges que chez moi… avant que tout bascule.";
            d["narration.mos_1_2.elite.before.0"] = "Les archives sont fermées au public.";
            d["narration.mos_1_2.elite.before.1"] = "Pas pour la restauratrice de la fresque.";
            d["narration.mos_1_2.elite.after.0"] = "Derrière la fresque, un registre. Je l'avais vu avant de le toucher.";
            d["narration.mos_1_2.elite.after.1"] = "Des noms… des dates… des livraisons. Ce sont des ventes.";
            d["narration.mos_1_2.exit.0"] = "Un registre, la moitié d'une preuve. L'autre moitié est du côté des raffineries.";
            d["narration.mos_1_2.exit.1"] = "Alors on part pour Kapotnya.";
            d["narration.mos_1_3.intro.0"] = "Le maglev file sous la ville, éclairé comme une fête. Un convoi roule en parallèle, sans lumière.";
            d["narration.mos_1_3.intro.1"] = "Un convoi qui ne figure sur aucun horaire, sur une ligne qui affiche tout.";
            d["narration.mos_1_3.intro.2"] = "Je vois les caisses… avant qu'on les charge. Ce sont les mêmes que sur la voie 9.";
            d["narration.mos_1_3.intro.2.d"] = "Je ne vois pas leur passé, mais je vois qu'on a emballé ces caisses à la main. Quelqu'un tient à elles.";
            d["narration.mos_1_3.intro.3"] = "Alors c'est la même filière.";
            d["narration.mos_1_3.elite.before.0"] = "Ce convoi n'existe pas. Vous n'avez rien vu.";
            d["narration.mos_1_3.elite.before.1"] = "Il le pense. C'est ça qui me gêne : il a peur pour lui, pas pour la cargaison.";
            d["narration.mos_1_3.elite.after.0"] = "Ces caisses portent un code soviétique. Quelqu'un déterre de vieilles recherches.";
            d["narration.mos_1_3.elite.after.1"] = "Soviétique ? Cinq ans après la Supernova, qui exhume ça ?";
            d["narration.mos_1_3.exit.0"] = "Un code d'avant l'Union sur des caisses de cette année. Retiens-le.";
            d["narration.mos_1_3.exit.1"] = "Je le retiens. Il ne s'efface pas.";
            d["narration.mos_1_4.intro.0"] = "Le dépôt est rangé comme une boutique. Chaque caisse porte une étiquette, chaque étiquette un prix.";
            d["narration.mos_1_4.intro.1"] = "Un entrepôt de contrebande avec un système de rangement plus propre que le mien.";
            d["narration.mos_1_4.intro.2"] = "Ces caisses partent ce soir. Je vois le camion, le chemin, l'adresse : une raffinerie.";
            d["narration.mos_1_4.intro.3"] = "Donc tout converge vers Kapotnya.";
            d["narration.mos_1_4.elite.before.0"] = "Le dépôt n'est pas ouvert. Vous avez dû vous tromper de guichet.";
            d["narration.mos_1_4.elite.after.0"] = "Il n'avait pas peur de nous. Il avait peur de celui qui est derrière.";
            d["narration.mos_1_4.boss.before.0"] = "Vous êtes en avance. D'ordinaire mes clients prennent rendez-vous.";
            d["narration.mos_1_4.boss.before.1"] = "Je ne dis pas victimes. Je ne dis pas adversaires. Je dis : clients.";
            d["narration.mos_1_4.boss.before.2"] = "N'écoute pas. Il compte déjà ce qu'il peut vendre de nous.";
            d["narration.mos_1_4.boss.before.3"] = "Les pouvoirs ont un prix, mademoiselle Varga. Les vôtres aussi.";
            d["narration.mos_1_4.boss.after.0"] = "Vous croyez avoir trouvé quelque chose ? On vous a laissé trouver. L'accueil, c'est moi qui l'ai organisé.";
            d["narration.mos_1_4.boss.after.1"] = "Alors c'était pour nous tenir loin de Kapotnya.";
            d["narration.mos_1_4.boss.after.2"] = "Je n'ai jamais dit « loin ». J'ai dit « accueillis ».";
            d["narration.mos_1_4.boss.after.3"] = "Alors c'est là que nous irons.";
            d["narration.mos_1_4.boss.before.3.m"] = "Votre vue sur le passé vaut cher, mademoiselle. Les enchères sont ouvertes.";
            d["narration.mos_1_4.boss.before.3.d"] = "Une imagination qui devient matière. Voilà un produit que je n'ai jamais eu en rayon.";
            d["narration.mos_1_4.boss.phase1.0"] = "Les clients arrivent. Je regrette : la boutique ferme tôt ce soir.";
            d["narration.mos_1_4.boss.phase2.0"] = "Tout doit partir. Vous compris.";
            d["narration.mos_1_4.boss.after.4"] = "La cire, l'encens, les cierges… c'est un parfum qui revient souvent chez mes clients.";
            d["narration.mos_1_4.chapter_end.0"] = "La Dawn comprend que l'accueil visait à la tenir loin de Kapotnya. Le convoi suivant part à l'aube.";

            // --- Fragments de souvenirs (annexe B de la spec) et archives de zone ---
            d["fragment.mos_maglev_welcome.title"] = "L'accueil parfait";
            d["fragment.mos_maglev_welcome.text"] = "Les hologrammes saluaient l'Aube par son nom, les quais étaient balayés, les sourires, parfaits. Un cargo scellé dormait pourtant sur la voie 9, sans nom et sans manifeste. Moscou t'ouvrait grand les bras, et regardait ailleurs.";
            d["fragment.mos_monastery_register.title"] = "Le registre sous la fresque";
            d["fragment.mos_monastery_register.text"] = "Tu as restauré mille fresques sans jamais chercher ce qu'elles cachaient. Celle-ci couvrait un registre : des noms, des dates, des livraisons. Quelqu'un avait peint par-dessus une vérité, et l'avait confiée aux saints.";
            d["fragment.mos_convoy_crates.title"] = "Des caisses marquées";
            d["fragment.mos_convoy_crates.text"] = "Le convoi filait sous la ville, éclairé comme une fête. Les caisses portaient un code que personne n'utilise plus. Il y avait des mains neuves sur de très vieilles recherches.";
            d["fragment.mos_depot_passeur.title"] = "Le Passeur";
            d["fragment.mos_depot_passeur.text"] = "Il ne disait jamais victime, jamais ennemi : seulement client. Dans son dépôt, tout avait un prix, même l'accueil qu'on t'avait réservé. Ce soir, tu as compris que la ville entière avait été invitée à regarder ailleurs.";
            d["archives.zone.zone_mos_1_1"] = "À la gare du maglev, l'Aube est accueillie comme une délégation d'État. Un cargo scellé, absent de tous les manifestes, y a été déposé trois jours plus tôt.";
            d["archives.zone.zone_mos_1_2"] = "Dans les archives du monastère, une fresque récemment repeinte cachait un registre de livraisons. C'est la première moitié de la preuve du marché noir.";
            d["archives.zone.zone_mos_1_3"] = "Le convoi du maglev transporte des caisses marquées d'un code soviétique. Quelqu'un exhume d'anciennes recherches.";
            d["archives.zone.zone_mos_1_4"] = "Le Passeur dirigeait le dépôt du maglev comme une boutique. L'accueil de l'Aube était sa mise en scène, pour l'éloigner de Kapotnya.";

            // --- Carte de Moscou ---
            d["map.borough.sergueiv"] = "SERGUEIV";
            d["map.borough.kapotnya"] = "KAPOTNYA";
            d["map.borough.sadovoye"] = "SADOVOYE KOLTSO";
            d["map.borough.place_rouge"] = "PLACE ROUGE";
            d["map.chapter"] = "CHAPITRE {0}";
            d["map.chapter_soon"] = "CHAPITRE {0} · BIENTÔT";
        }

        /// <summary>Ajoute les textes anglais de Moscou au dictionnaire anglais (méthode statique sans état : sûre dans un initialiseur de champ).</summary>
        static Dictionary<string, string> WithMoscowEnglish(Dictionary<string, string> d)
        {
            // --- Enemies ---
            d["enemies.mos_guard.name"] = "Mob Guard";
            d["enemies.mos_smuggler.name"] = "Trafficker";
            d["enemies.mos_enforcer.name"] = "Enforcer";
            d["enemies.mos_passeur.name"] = "THE SMUGGLER";
            d["enemies.mos_passeur.phase2"] = "The Customers Arrive";
            d["enemies.mos_passeur.phase3"] = "Everything Must Go";
            d["skills.mos_guard_a1"] = "Rifle Butt";
            d["skills.mos_guard_a2"] = "Security Cordon";
            d["skills.mos_smuggler_a1"] = "Warning Shot";
            d["skills.mos_smuggler_a2"] = "Express Delivery";
            d["skills.mos_enforcer_a1"] = "Gloved Fist";
            d["skills.mos_enforcer_a2"] = "Courtesy Call";
            d["skills.mos_enforcer_a3"] = "Padded Coat";
            d["skills.mos_passeur_a1"] = "Haggling";
            d["skills.mos_passeur_a2"] = "Sealed Cargo";
            d["skills.mos_passeur_a3"] = "Delivery";
            d["skills.mos_passeur_a4"] = "Formal Notice";
            d["skills.mos_passeur_p2_a2"] = "Impatient Customers";
            d["archives.enemy.mos_guard"] = "Mob guard: a human cordon posted at the entrances. He raises a shield and draws the blows.";
            d["archives.enemy.mos_smuggler"] = "Smuggler: a gunman of the maglev route. He marks his target before delivering lead.";
            d["archives.enemy.mos_enforcer"] = "Enforcer: the elite of the welcoming committee. His courtesy calls rattle the whole squad.";
            d["archives.enemy.mos_passeur"] = "THE SMUGGLER: he never says victims or enemies, only customers. Everything he owns has a price.";

            // --- Zones (Sergueiv chapter) ---
            d["zones.mos_1_1.name"] = "MAGLEV STATION — THE WELCOME";
            d["zones.mos_1_1.short"] = "Maglev Station";
            d["zones.mos_1_1.code"] = "Zone M1.1";
            d["zones.mos_1_1.desc"] = "Moscow opens its arms wide: holograms greeting you by name, swept platforms, flawless smiles. Too flawless. Yet a sealed cargo sits on track 9, with no manifest. Make it through the official welcome and see what they are trying to hide.";
            d["zones.mos_1_2.name"] = "MONASTERY — THE ARCHIVES";
            d["zones.mos_1_2.short"] = "Monastery";
            d["zones.mos_1_2.code"] = "Zone M1.2";
            d["zones.mos_1_2.desc"] = "The cargo is on no manifest, but someone kept track of the deliveries: the monastery archives, closed to the public. Pass yourself off as the woman restoring the fresco and search what it hides.";
            d["zones.mos_1_3.name"] = "MAGLEV LINE — THE CONVOY";
            d["zones.mos_1_3.short"] = "Maglev Line";
            d["zones.mos_1_3.code"] = "Zone M1.3";
            d["zones.mos_1_3.desc"] = "The ledger leads to a convoy that does not exist, lit like a festival beneath the city. Catch up with it, break its escort and open the crates: they carry a code nobody uses anymore.";
            d["zones.mos_1_4.name"] = "MAGLEV DEPOT — THE SMUGGLER";
            d["zones.mos_1_4.short"] = "Depot";
            d["zones.mos_1_4.code"] = "Zone M1.4";
            d["zones.mos_1_4.desc"] = "The convoy ends here, in a depot where everything has a price. Its master never says victim or enemy, only customer. Search the depot, break its guard and face the Smuggler.";
            d["qg.fil.zone_mos_1_1"] = "Who left the cargo on track 9?";
            d["qg.fil.zone_mos_1_2"] = "The ledger mentions Kapotnya.";
            d["qg.fil.zone_mos_1_3"] = "What do the crates with the Soviet code contain?";
            d["qg.fil.zone_mos_1_4"] = "On to Kapotnya, its refineries and its convoy.";

            // --- Objectives ---
            d["objectives.mos11.welcome"] = "Get through the official welcome";
            d["objectives.mos11.platforms"] = "Slip behind the platforms";
            d["objectives.mos11.escort"] = "Take down the cargo escort";
            d["objectives.mos11.leave"] = "Leave the station";
            d["objectives.mos12.archives"] = "Enter the archives";
            d["objectives.mos12.fresco"] = "Swivel the fresco";
            d["objectives.mos12.guardian"] = "Defeat the archives guardian";
            d["objectives.mos12.register"] = "Take the ledger";
            d["objectives.mos13.convoy"] = "Catch up with the convoy";
            d["objectives.mos13.escort"] = "Break the convoy escort";
            d["objectives.mos13.crates"] = "Open the crates";
            d["objectives.mos14.depot"] = "Search the depot";
            d["objectives.mos14.guard"] = "Break the depot guard";
            d["objectives.mos14.passeur"] = "Defeat the Smuggler";

            // --- Dialogue (spec appendix A) ---
            d["speaker.agent_m"] = "Agent M";
            d["chapter.mos_sergueiv.intro.agent_m"] = "She knows these streets, these codes, these smiles. She never looked at what they covered.";
            d["chapter.mos_sergueiv.intro.agent_d"] = "He has already seen a gang and an agency work hand in hand. Here, no one hides it: everything is polite.";
            d["speaker.agent_d"] = "Agent D";
            d["speaker.passeur"] = "The Smuggler";
            d["speaker.enforcer"] = "Enforcer";
            d["speaker.narrateur"] = "Narrator";
            d["narration.mos_1_1.intro.0"] = "Holograms greet The Dawn by name. The platforms are swept, the smiles flawless.";
            d["narration.mos_1_1.intro.1"] = "You'd think they were expecting a head of state.";
            d["narration.mos_1_1.intro.2"] = "They were expecting someone. I don't know yet who.";
            d["narration.mos_1_1.intro.3"] = "These platforms were washed tonight. I can see the brooms, the buckets... and nobody slept.";
            d["narration.mos_1_1.intro.3.d"] = "They washed these platforms for us. In New York, they hide the cameras, not the brooms.";
            d["narration.mos_1_1.intro.4"] = "A welcome of this quality takes days to prepare.";
            d["narration.mos_1_1.intro.5"] = "Or it's there to keep our eyes on the façade.";
            d["narration.mos_1_1.intro.6"] = "That smell of wax and incense on a train platform... I know it.";
            d["narration.mos_1_1.elite.before.0"] = "Welcome to Moscow. The welcoming committee kindly asks you to turn back.";
            d["narration.mos_1_1.elite.before.1"] = "A welcome that perfect is always hiding something.";
            d["narration.mos_1_1.elite.after.0"] = "This sealed cargo is on no manifest. I can see the trucks that left it here... three days before we arrived.";
            d["narration.mos_1_1.elite.after.1"] = "Three days? Before we even decided to come?";
            d["narration.mos_1_1.elite.after.2"] = "Before we even decided to come.";
            d["narration.mos_1_1.exit.0"] = "We leave through the monastery archives. If someone's waiting there, it isn't to welcome us.";
            d["narration.mos_1_2.intro.0"] = "The monastery is silent. The Sergueiv archives sleep under the vaults, at the end of a corridor of candles.";
            d["narration.mos_1_2.intro.1"] = "The candles are electric. Even the saints have an energy bill.";
            d["narration.mos_1_2.intro.2"] = "This fresco was repainted. Less than ten days ago. The pigment doesn't lie.";
            d["narration.mos_1_2.intro.2.d"] = "I draw walls, not frescoes. But this one's been touched up, I'm sure of it.";
            d["narration.mos_1_2.intro.3"] = "Someone went to the trouble of painting over a wall. To hide what?";
            d["narration.mos_1_2.intro.4"] = "I see it... and it isn't the color I see, it's the door.";
            d["narration.mos_1_2.intro.4.d"] = "I see the line that's off. I don't see why.";
            d["narration.mos_1_2.intro.5"] = "The same corridor of candles as back home... before everything fell apart.";
            d["narration.mos_1_2.elite.before.0"] = "The archives are closed to the public.";
            d["narration.mos_1_2.elite.before.1"] = "Not for the woman restoring the fresco.";
            d["narration.mos_1_2.elite.after.0"] = "Behind the fresco, a ledger. I saw it before I touched it.";
            d["narration.mos_1_2.elite.after.1"] = "Names... dates... deliveries. These are sales.";
            d["narration.mos_1_2.exit.0"] = "A ledger: half a proof. The other half is out by the refineries.";
            d["narration.mos_1_2.exit.1"] = "Then we head for Kapotnya.";
            d["narration.mos_1_3.intro.0"] = "The maglev races beneath the city, lit like a festival. A convoy runs alongside it, without lights.";
            d["narration.mos_1_3.intro.1"] = "A convoy that's on no timetable, on a line that displays everything.";
            d["narration.mos_1_3.intro.2"] = "I see the crates... before they're loaded. They're the same as on track 9.";
            d["narration.mos_1_3.intro.2.d"] = "I can't see their past, but I can see these crates were packed by hand. Someone cares about them.";
            d["narration.mos_1_3.intro.3"] = "Then it's the same pipeline.";
            d["narration.mos_1_3.elite.before.0"] = "This convoy doesn't exist. You saw nothing.";
            d["narration.mos_1_3.elite.before.1"] = "He means it. That's what bothers me: he's afraid for himself, not for the cargo.";
            d["narration.mos_1_3.elite.after.0"] = "These crates carry a Soviet code. Someone is digging up old research.";
            d["narration.mos_1_3.elite.after.1"] = "Soviet? Five years after the Supernova, who's exhuming that?";
            d["narration.mos_1_3.exit.0"] = "A pre-Union code on crates from this year. Remember it.";
            d["narration.mos_1_3.exit.1"] = "I'll remember it. It doesn't fade.";
            d["narration.mos_1_4.intro.0"] = "The depot is arranged like a shop. Every crate carries a label, every label a price.";
            d["narration.mos_1_4.intro.1"] = "A smuggling warehouse with a tidier filing system than mine.";
            d["narration.mos_1_4.intro.2"] = "These crates leave tonight. I see the truck, the route, the address: a refinery.";
            d["narration.mos_1_4.intro.3"] = "So everything converges on Kapotnya.";
            d["narration.mos_1_4.elite.before.0"] = "The depot isn't open. You must have the wrong counter.";
            d["narration.mos_1_4.elite.after.0"] = "He wasn't afraid of us. He was afraid of whoever's behind him.";
            d["narration.mos_1_4.boss.before.0"] = "You're early. My clients usually make an appointment.";
            d["narration.mos_1_4.boss.before.1"] = "I don't say victims. I don't say adversaries. I say: customers.";
            d["narration.mos_1_4.boss.before.2"] = "Don't listen. He's already counting what he can sell of us.";
            d["narration.mos_1_4.boss.before.3"] = "Powers have a price, Miss Varga. Yours too.";
            d["narration.mos_1_4.boss.after.0"] = "You think you found something? We let you find it. I arranged the welcome.";
            d["narration.mos_1_4.boss.after.1"] = "So that was to keep us away from Kapotnya.";
            d["narration.mos_1_4.boss.after.2"] = "I never said \"away.\" I said \"welcomed.\"";
            d["narration.mos_1_4.boss.after.3"] = "Then that is where we go.";
            d["narration.mos_1_4.boss.before.3.m"] = "Your sight into the past is worth a great deal, miss. The bidding is open.";
            d["narration.mos_1_4.boss.before.3.d"] = "An imagination that becomes matter. Now there's a product I've never had on my shelves.";
            d["narration.mos_1_4.boss.phase1.0"] = "The customers are arriving. My apologies: the shop closes early tonight.";
            d["narration.mos_1_4.boss.phase2.0"] = "Everything must go. You included.";
            d["narration.mos_1_4.boss.after.4"] = "Wax, incense, candles... that's a scent that comes up often among my clients.";
            d["narration.mos_1_4.chapter_end.0"] = "The Dawn understands that the welcome was meant to keep it away from Kapotnya. The next convoy leaves at dawn.";

            // --- Memory fragments (spec appendix B) and zone archives ---
            d["fragment.mos_maglev_welcome.title"] = "The Perfect Welcome";
            d["fragment.mos_maglev_welcome.text"] = "Holograms greeted The Dawn by name, the platforms were swept, the smiles flawless. Yet a sealed cargo sat on track 9, with no name and no manifest. Moscow opened its arms wide, and looked the other way.";
            d["fragment.mos_monastery_register.title"] = "The Ledger Under the Fresco";
            d["fragment.mos_monastery_register.text"] = "You have restored a thousand frescoes without ever looking for what they hid. This one covered a ledger: names, dates, deliveries. Someone had painted over the truth and entrusted it to the saints.";
            d["fragment.mos_convoy_crates.title"] = "Marked Crates";
            d["fragment.mos_convoy_crates.text"] = "The convoy raced beneath the city, lit like a festival. The crates bore a code nobody uses anymore. New hands on very old research.";
            d["fragment.mos_depot_passeur.title"] = "The Smuggler";
            d["fragment.mos_depot_passeur.text"] = "He never said victim, never enemy: only customer. In his depot everything had a price, even the welcome prepared for you. Tonight you understood the whole city had been invited to look elsewhere.";
            d["archives.zone.zone_mos_1_1"] = "At the maglev station, The Dawn is welcomed like a state delegation. A sealed cargo, absent from every manifest, was left there three days earlier.";
            d["archives.zone.zone_mos_1_2"] = "In the monastery archives, a recently repainted fresco hid a ledger of deliveries. It is the first half of the proof of the black market.";
            d["archives.zone.zone_mos_1_3"] = "The maglev convoy carries crates marked with a Soviet code. Someone is unearthing old research.";
            d["archives.zone.zone_mos_1_4"] = "The Smuggler ran the maglev depot like a shop. The welcome given to The Dawn was his staging, to keep it away from Kapotnya.";

            // --- Moscow map ---
            d["map.borough.sergueiv"] = "SERGUEIV";
            d["map.borough.kapotnya"] = "KAPOTNYA";
            d["map.borough.sadovoye"] = "SADOVOYE KOLTSO";
            d["map.borough.place_rouge"] = "RED SQUARE";
            d["map.chapter"] = "CHAPTER {0}";
            d["map.chapter_soon"] = "CHAPTER {0} · COMING SOON";
            return d;
        }
    }
}
