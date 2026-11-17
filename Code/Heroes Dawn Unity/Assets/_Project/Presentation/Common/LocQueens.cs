#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes du chapitre Queens : français (<c>ExtendQueens</c>) et anglais (<c>QueensEnglish</c>, fusionné dans <c>En</c>).</summary>
    public static partial class Loc
    {
        static partial void ExtendQueens(Dictionary<string, string> d)
        {
            // --- Ennemis (le Syndicat) ---
            d["enemies.syn_racketeer.name"] = "Racketteur du Syndicat";
            d["enemies.syn_hacker.name"] = "Pirate des caméras";
            d["enemies.syn_gunman.name"] = "Exécuteur";
            d["enemies.syn_collector.name"] = "Collecteur";
            d["enemies.syn_regisseur.name"] = "LE RÉGISSEUR";
            d["enemies.syn_regisseur.phase2"] = "Le contrat est signé";
            d["enemies.syn_regisseur.phase3"] = "Résiliation";
            d["skills.syn_racketeer_a1"] = "Droit de passage";
            d["skills.syn_racketeer_a2"] = "Protection rapprochée";
            d["skills.syn_hacker_a1"] = "Tir de surveillance";
            d["skills.syn_hacker_a2"] = "Coupure de flux";
            d["skills.syn_gunman_a1"] = "Paiement comptant";
            d["skills.syn_gunman_a2"] = "Clause d'exécution";
            d["skills.syn_collector_a1"] = "Recouvrement";
            d["skills.syn_collector_a2"] = "Tournée des comptes";
            d["skills.syn_collector_a3"] = "Acompte versé";
            d["skills.syn_regisseur_a1"] = "Clause pénale";
            d["skills.syn_regisseur_a2"] = "Garantie bancaire";
            d["skills.syn_regisseur_a3"] = "Audit surprise";
            d["skills.syn_regisseur_a4"] = "Avenant";
            d["skills.syn_regisseur_p2_a2"] = "Pénalité de retard";
            d["archives.enemy.syn_racketeer"] = "Racketteur du Syndicat : il tient les étals et fait payer le droit d'être regardé. Il se couvre d'un bouclier et attire les coups.";
            d["archives.enemy.syn_hacker"] = "Pirate des caméras : il tient les flux du marché et les revend plus haut. Il aveugle ses cibles en coupant leurs images.";
            d["archives.enemy.syn_gunman"] = "Exécuteur : l'encaisseur du Syndicat. Chaque coup est une ligne de contrat, et il expose sa cible avant de conclure.";
            d["archives.enemy.syn_collector"] = "Collecteur : l'élite du recouvrement. Sa tournée des comptes marque toute l'escouade.";
            d["archives.enemy.syn_regisseur"] = "LE RÉGISSEUR : Elias Voss, intendant du Syndicat. Il ne parle ni de proies ni d'ennemis, seulement de contrats, de dates de livraison et de pénalités.";

            // --- Zones (chapitre Queens) ---
            d["zones.ny_2_1.name"] = "FLUSHING — LE MARCHÉ SOUS CAMÉRAS";
            d["zones.ny_2_1.short"] = "Flushing";
            d["zones.ny_2_1.code"] = "Zone 2.1";
            d["zones.ny_2_1.desc"] = "Flushing grouille : chaque étal a sa caméra au-dessus de la caisse, et personne ne lève les yeux. Coupe-les étal par étal sans effrayer les commerçants, et regarde de près le boîtier : un sigle y est gravé.";
            d["zones.ny_2_2.name"] = "ASTORIA — LES INDICS";
            d["zones.ny_2_2.short"] = "Astoria";
            d["zones.ny_2_2.code"] = "Zone 2.2";
            d["zones.ny_2_2.desc"] = "À Astoria, les indics du Syndicat se retrouvent dans un café de nuit. L'un d'eux a peur pour sa fille : neutralise la surveillance du Syndicat sans l'exposer, et laisse-le parler avant de le mettre à l'abri.";
            d["zones.ny_2_3.name"] = "TRIAGE DE JAMAICA — LE CONVOI";
            d["zones.ny_2_3.short"] = "Jamaica";
            d["zones.ny_2_3.code"] = "Zone 2.3";
            d["zones.ny_2_3.desc"] = "Un convoi sans marque quitte la gare de triage de Jamaica à l'aube, sous escorte du Syndicat. Fouille les wagons, brise l'escorte et libère les Apex éveillés, drogués et menottés, qui y voyagent.";
            d["zones.ny_2_4.name"] = "TERMINAL DE FRET — LE RÉGISSEUR";
            d["zones.ny_2_4.short"] = "Terminal de fret";
            d["zones.ny_2_4.code"] = "Zone 2.4";
            d["zones.ny_2_4.desc"] = "Le terminal de fret est le centre logistique du Syndicat : conteneurs, grues, et une salle vitrée au sommet de l'une d'elles. Ouvre les cages, brise la garde du quai et fais face au Régisseur : son registre de transport dit tout.";
            d["qg.fil.zone_ny_2_1"] = "Le Syndicat loue ses yeux à l'AMIF. Direction Astoria.";
            d["qg.fil.zone_ny_2_2"] = "Les listes du jeudi mènent à Jamaica.";
            d["qg.fil.zone_ny_2_3"] = "Le registre de ce convoi est au terminal de fret.";
            d["qg.fil.zone_ny_2_4"] = "Preuve n°1 en main. Cap sur Harlem.";
            d["archives.zone.zone_ny_2_1"] = "À Flushing, les caméras du marché appartiennent au Syndicat, mais leurs boîtiers portent le sigle d'un prestataire de l'AMIF.";
            d["archives.zone.zone_ny_2_2"] = "À Astoria, un indic du Syndicat avoue que les listes d'Apex sont remises chaque semaine à un contact de l'AMIF.";
            d["archives.zone.zone_ny_2_3"] = "Le convoi de Jamaica transporte des Apex éveillés, drogués et menottés, destinés à l'AMIF. L'Aube les libère.";
            d["archives.zone.zone_ny_2_4"] = "Au terminal de fret, l'Aube saisit le registre de transport : la preuve que le Syndicat livre des Apex captifs à l'AMIF, qui lui fournit la surveillance.";

            // --- Objectifs ---
            d["objectives.ny21.market"] = "Entrer dans le marché";
            d["objectives.ny21.cameras"] = "Couper les caméras en silence";
            d["objectives.ny21.collector"] = "Vaincre le Collecteur";
            d["objectives.ny21.logo"] = "Relever le sigle des caméras";
            d["objectives.ny22.meeting"] = "Trouver le point de rendez-vous";
            d["objectives.ny22.chase"] = "Rattraper l'indic";
            d["objectives.ny22.escort"] = "Briser l'escorte de l'indic";
            d["objectives.ny22.shelter"] = "Mettre l'indic à l'abri";
            d["objectives.ny23.wagons"] = "Fouiller les wagons";
            d["objectives.ny23.escort"] = "Briser l'escorte du convoi";
            d["objectives.ny23.captives"] = "Identifier les captifs";
            d["objectives.ny23.free"] = "Libérer le convoi";
            d["objectives.ny24.terminal"] = "Entrer dans le terminal";
            d["objectives.ny24.cages"] = "Ouvrir les cages";
            d["objectives.ny24.guard"] = "Briser la garde du quai";
            d["objectives.ny24.regisseur"] = "Vaincre le Régisseur";

            // --- Dialogues ---
            d["speaker.regisseur"] = "Elias Voss, le Régisseur";
            d["speaker.indic"] = "Un indic";
            d["speaker.commercant"] = "Un commerçant";
            d["speaker.captif"] = "Un captif";
            d["speaker.narrateur"] = "Narrateur";
            d["narration.ny_2_1.intro.0"] = "Flushing grouille. Chaque étal a sa caméra au-dessus de la caisse, et personne ne lève les yeux.";
            d["narration.ny_2_1.intro.1"] = "Dix caméras sur un marché d'oignons. Le Syndicat ne rate pas une transaction.";
            d["narration.ny_2_1.intro.2"] = "Ou quelqu'un lui prête le matériel.";
            d["narration.ny_2_1.intro.3"] = "Elles tournent depuis l'été. Elles nous voient. Elles ne nous aident jamais.";
            d["narration.ny_2_1.intro.4"] = "Si on les coupe d'un coup, tout le marché le saura. On y va étal par étal.";
            d["narration.ny_2_1.intro.5"] = "Vous êtes l'Aube. Faites vite, avant que quelqu'un le raconte.";
            d["narration.ny_2_1.elite.before.0"] = "On ne coupe pas ce qui protège le quartier. Surtout pas pour des visiteurs.";
            d["narration.ny_2_1.elite.before.1"] = "« Protéger » : tu es sûr que c'est le mot qu'ils t'ont donné ?";
            d["narration.ny_2_1.elite.after.0"] = "Regardez le boîtier. Ce n'est pas un modèle de rue. Il y a un sigle gravé dessous.";
            d["narration.ny_2_1.elite.after.1"] = "Un prestataire de l'AMIF. Les caméras du Syndicat sont louées à l'agence.";
            d["narration.ny_2_1.exit.0"] = "Un gang qui loue ses yeux à une agence d'État. On remonte la piste jusqu'à Astoria.";
            d["narration.ny_2_1.exit.1"] = "Ou on suit les caméras. Elles vont toutes dans la même direction.";
            d["narration.ny_2_2.intro.0"] = "Astoria, une heure du matin. Le café est à moitié fermé, la salle à moitié pleine. Tous parlent à voix basse.";
            d["narration.ny_2_2.intro.1"] = "Celui du fond n'a pas touché son café. Il regarde la porte toutes les trente secondes.";
            d["narration.ny_2_2.intro.2"] = "Je ne suis pas une balance. Je suis un type qui a une fille.";
            d["narration.ny_2_2.intro.3"] = "On ne vous demande pas de trahir. On vous demande de dire ce qu'on ne peut plus ignorer.";
            d["narration.ny_2_2.intro.4"] = "Il ne ment pas. Il a peur pour quelqu'un, pas pour lui.";
            d["narration.ny_2_2.elite.before.0"] = "Tout le monde reste assis. Il y a des visiteurs qui n'ont pas été invités.";
            d["narration.ny_2_2.elite.after.0"] = "Dehors, vite. Avant que les autres comprennent.";
            d["narration.ny_2_2.exit.0"] = "Chaque jeudi, le Syndicat apporte une liste. Des noms, des quartiers, des pouvoirs. Un type en costume la prend, sans jamais donner son nom.";
            d["narration.ny_2_2.exit.1"] = "Un costume de l'AMIF ?";
            d["narration.ny_2_2.exit.2"] = "Un costume qui ne se salit jamais. Les nouveaux Apex apparaissent sur la liste avant même d'avoir peur.";
            d["narration.ny_2_2.exit.3"] = "On t'emmène dans un refuge. Ta fille aussi.";
            d["narration.ny_2_3.intro.0"] = "La gare de triage de Jamaica dort sous les lampes orange. Un convoi sans marque attend à la voie 14.";
            d["narration.ny_2_3.intro.1"] = "Pas d'étiquette, pas de numéro, et une escorte armée. Ce n'est pas du fret.";
            d["narration.ny_2_3.intro.2"] = "Dans les wagons, il y a des gens. Je les entends respirer.";
            d["narration.ny_2_3.intro.3"] = "Alors on ne ralentit pas. On ouvre le dernier wagon.";
            d["narration.ny_2_3.elite.before.0"] = "Ce convoi est sous contrat. Reculez, ou le contrat vous inclura.";
            d["narration.ny_2_3.elite.before.1"] = "Il récite. Il ne sait même pas pour qui il travaille.";
            d["narration.ny_2_3.elite.after.0"] = "Vous êtes… de l'Aube ? On nous avait dit qu'on partait vers un endroit sûr.";
            d["narration.ny_2_3.elite.after.1"] = "Qui vous l'a dit ?";
            d["narration.ny_2_3.elite.after.2"] = "Un homme avec un badge. Il disait que l'agence se chargerait de nous.";
            d["narration.ny_2_3.exit.0"] = "Des Apex éveillés, livrés comme des colis. À l'agence.";
            d["narration.ny_2_3.exit.1"] = "Le registre de ce convoi sera au terminal de fret. C'est là qu'on le trouvera.";
            d["narration.ny_2_4.intro.0"] = "Le terminal de fret domine la baie. Au sommet d'une grue, une salle vitrée surveille chaque conteneur.";
            d["narration.ny_2_4.intro.1"] = "De là-haut, on voit tout. C'est exactement ce que veut un gestionnaire.";
            d["narration.ny_2_4.intro.2"] = "Des livraisons, des horaires, des numéros… et à côté, le même sigle que sur les caméras.";
            d["narration.ny_2_4.intro.3"] = "On ne prend pas des extraits. On prend le registre entier.";
            d["narration.ny_2_4.elite.before.0"] = "La direction ne reçoit pas sans rendez-vous.";
            d["narration.ny_2_4.elite.after.0"] = "Il n'y a plus que lui là-haut. Et il nous attend.";
            d["narration.ny_2_4.boss.before.0"] = "Vous êtes en retard. Le dernier convoi est parti sans vous, et pourtant vous êtes là. Quelle manière de livrer.";
            d["narration.ny_2_4.boss.before.1"] = "Je ne dirige pas un gang. Je gère un contrat. Les contrats ne s'arrêtent pas parce que quelqu'un le regrette.";
            d["narration.ny_2_4.boss.before.2"] = "Des gens ont été enfermés dans vos wagons.";
            d["narration.ny_2_4.boss.before.3"] = "Des gens ont été transportés. Il y a une différence : l'une rapporte et l'autre se plaint.";
            d["narration.ny_2_4.boss.after.0"] = "Le registre ! Chaque livraison porte le même signataire : l'agence.";
            d["narration.ny_2_4.boss.after.1"] = "Le Syndicat remet les captifs. L'AMIF fournit la surveillance, et revend ce qu'elle ne garde pas. Tout est consigné.";
            d["narration.ny_2_4.boss.after.2"] = "C'est la preuve que Brooklyn nous laissait deviner : ils ne se combattent pas, ils s'associent.";
            d["narration.ny_2_4.boss.after.3"] = "Vous croyez abattre un gang. Vous venez de déranger un contrat.";
            d["narration.ny_2_4.chapter_end.0"] = "Les captifs sont libérés. Le registre de transport est en sûreté : première preuve de la collusion entre le Syndicat et l'AMIF.";
            d["narration.ny_2_4.chapter_end.1"] = "Garde-le. Nous en aurons besoin à Salem.";

            // --- Fragments de souvenirs ---
            d["fragment.ny_flushing_cameras.title"] = "Des yeux qui louent";
            d["fragment.ny_flushing_cameras.text"] = "Dix caméras sur un marché d'oignons, et personne ne levait les yeux. Elles voyaient chaque pièce, chaque visage, chaque Apex qui hésitait devant un étal. Sous l'une d'elles, un sigle d'agence : ce n'est pas le Syndicat qui t'observe, c'est celui qui lui loue la vue.";
            d["fragment.ny_astoria_informant.title"] = "La liste du jeudi";
            d["fragment.ny_astoria_informant.text"] = "Il n'était pas une balance, il était un père. Chaque jeudi, une liste changeait de mains : des noms, des quartiers, des pouvoirs encore endormis. Quelqu'un dans un costume impeccable la rangeait sans lever les yeux.";
            d["fragment.ny_jamaica_convoy.title"] = "Voie 14";
            d["fragment.ny_jamaica_convoy.text"] = "Les wagons n'avaient ni nom ni numéro, et pourtant tout y était attendu. Tu as entendu des respirations derrière l'acier, et une voix qui répétait qu'on partait vers un endroit sûr. Elle n'avait jamais eu de raison de se méfier d'un badge.";
            d["fragment.ny_terminal_register.title"] = "Le registre de transport";
            d["fragment.ny_terminal_register.text"] = "Tout était consigné : les dates, les wagons, les noms de ceux qu'on avait attrapés. À côté de chaque ligne, un seul signataire, et ce n'était pas le Syndicat. Les captifs n'étaient pas livrés à un gang, mais à une agence.";

            // --- Variantes de parcours, phases de boss, ouverture ---
            d["narration.ny_2_1.intro.4.m"] = "Je vois ce que chaque caméra a filmé hier. Coupons-les dans l'ordre : celles qui regardent la caisse en dernier.";
            d["narration.ny_2_1.exit.2"] = "À Moscou, c'étaient des hologrammes. Ici, des caméras. Le même sourire poli derrière.";
            d["narration.ny_2_2.intro.3.m"] = "Je vois ce qui t'est arrivé ce matin. On ne te demande pas de parler : on te demande de nous laisser écouter.";
            d["narration.ny_2_2.exit.4"] = "À Moscou, on parle de clients. Ici, de listes. C'est le même mot.";
            d["narration.ny_2_3.intro.3.m"] = "Je vois qui est monté dans ces wagons, et ce qu'on leur a promis. On ne ralentit pas.";
            d["narration.ny_2_3.exit.2"] = "À Moscou, on les appelle des trophées. Ici, des captifs.";
            d["narration.ny_2_4.boss.before.2.m"] = "Je vois ces wagons pleins, et ce que vous touchez pour chacun.";
            d["narration.ny_2_4.boss.after.4"] = "Moscou avait les oligarques. New York a un intendant. La logique est identique : les personnes en marchandise.";
            d["narration.ny_2_4.boss.phase1.0"] = "Le contrat est signé. Vous n'avez plus qu'à l'exécuter.";
            d["narration.ny_2_4.boss.phase2.0"] = "Résiliation. Je n'aime pas perdre un client.";
            d["chapter.ny_queens.intro.agent_d"] = "Il connaît le bruit de ces rues. Il découvre qui les écoute.";
            d["chapter.ny_queens.intro.agent_m"] = "Elle a déjà vu ce schéma à Moscou : un gang, une agence, un contrat.";
        }

        static Dictionary<string, string> QueensEnglish(Dictionary<string, string> d)
        {
            // --- Enemies (the Syndicate) ---
            d["enemies.syn_racketeer.name"] = "Syndicate Racketeer";
            d["enemies.syn_hacker.name"] = "Camera Hacker";
            d["enemies.syn_gunman.name"] = "Executioner";
            d["enemies.syn_collector.name"] = "Collector";
            d["enemies.syn_regisseur.name"] = "THE STEWARD";
            d["enemies.syn_regisseur.phase2"] = "The Contract Is Signed";
            d["enemies.syn_regisseur.phase3"] = "Termination";
            d["skills.syn_racketeer_a1"] = "Right of Way";
            d["skills.syn_racketeer_a2"] = "Close Protection";
            d["skills.syn_hacker_a1"] = "Surveillance Shot";
            d["skills.syn_hacker_a2"] = "Feed Cut";
            d["skills.syn_gunman_a1"] = "Cash Payment";
            d["skills.syn_gunman_a2"] = "Execution Clause";
            d["skills.syn_collector_a1"] = "Debt Recovery";
            d["skills.syn_collector_a2"] = "Collection Round";
            d["skills.syn_collector_a3"] = "Deposit Paid";
            d["skills.syn_regisseur_a1"] = "Penalty Clause";
            d["skills.syn_regisseur_a2"] = "Bank Guarantee";
            d["skills.syn_regisseur_a3"] = "Surprise Audit";
            d["skills.syn_regisseur_a4"] = "Amendment";
            d["skills.syn_regisseur_p2_a2"] = "Late Penalty";
            d["archives.enemy.syn_racketeer"] = "Syndicate racketeer: he runs the stalls and charges for the right to be watched. He raises a shield and draws the blows.";
            d["archives.enemy.syn_hacker"] = "Camera hacker: he holds the market feeds and resells them higher up. He blinds his targets by cutting their picture.";
            d["archives.enemy.syn_gunman"] = "Executioner: the Syndicate's enforcer. Every blow is a line in a contract, and he exposes his target before closing the deal.";
            d["archives.enemy.syn_collector"] = "Collector: the elite of debt recovery. His collection round marks the whole squad.";
            d["archives.enemy.syn_regisseur"] = "THE STEWARD: Elias Voss, steward of the Syndicate. He never speaks of prey or enemies, only of contracts, delivery dates and penalties.";

            // --- Zones (Queens chapter) ---
            d["zones.ny_2_1.name"] = "FLUSHING — THE MARKET UNDER CAMERAS";
            d["zones.ny_2_1.short"] = "Flushing";
            d["zones.ny_2_1.code"] = "Zone 2.1";
            d["zones.ny_2_1.desc"] = "Flushing teems: every stall has a camera above the register, and no one looks up. Cut them stall by stall without scaring the shopkeepers, and take a close look at the housing: a logo is engraved on it.";
            d["zones.ny_2_2.name"] = "ASTORIA — THE INFORMANTS";
            d["zones.ny_2_2.short"] = "Astoria";
            d["zones.ny_2_2.code"] = "Zone 2.2";
            d["zones.ny_2_2.desc"] = "In Astoria, the Syndicate's informants meet in an all-night café. One of them is afraid for his daughter: neutralize the Syndicate's watchers without exposing him, and let him talk before getting him to safety.";
            d["zones.ny_2_3.name"] = "JAMAICA RAIL YARD — THE CONVOY";
            d["zones.ny_2_3.short"] = "Jamaica";
            d["zones.ny_2_3.code"] = "Zone 2.3";
            d["zones.ny_2_3.desc"] = "An unmarked convoy leaves the Jamaica rail yard at dawn under Syndicate escort. Search the cars, break the escort and free the awakened Apex, drugged and handcuffed, riding inside.";
            d["zones.ny_2_4.name"] = "FREIGHT TERMINAL — THE STEWARD";
            d["zones.ny_2_4.short"] = "Freight Terminal";
            d["zones.ny_2_4.code"] = "Zone 2.4";
            d["zones.ny_2_4.desc"] = "The freight terminal is the Syndicate's logistics hub: containers, cranes, and a glass room at the top of one of them. Open the cages, break the dock guard and face the Steward: his transport ledger says it all.";
            d["qg.fil.zone_ny_2_1"] = "The Syndicate rents its eyes to the AMIF. On to Astoria.";
            d["qg.fil.zone_ny_2_2"] = "Thursday's lists lead to Jamaica.";
            d["qg.fil.zone_ny_2_3"] = "This convoy's ledger is at the freight terminal.";
            d["qg.fil.zone_ny_2_4"] = "Proof no. 1 in hand. Heading to Harlem.";
            d["archives.zone.zone_ny_2_1"] = "In Flushing, the market's cameras belong to the Syndicate, but their housings carry the logo of an AMIF contractor.";
            d["archives.zone.zone_ny_2_2"] = "In Astoria, a Syndicate informant admits that lists of Apex are handed every week to an AMIF contact.";
            d["archives.zone.zone_ny_2_3"] = "The Jamaica convoy carries awakened Apex, drugged and handcuffed, bound for the AMIF. The Dawn frees them.";
            d["archives.zone.zone_ny_2_4"] = "At the freight terminal, The Dawn seizes the transport ledger: proof that the Syndicate delivers captive Apex to the AMIF, which supplies it with surveillance.";

            // --- Objectives ---
            d["objectives.ny21.market"] = "Enter the market";
            d["objectives.ny21.cameras"] = "Cut the cameras quietly";
            d["objectives.ny21.collector"] = "Defeat the Collector";
            d["objectives.ny21.logo"] = "Read the logo on the cameras";
            d["objectives.ny22.meeting"] = "Find the meeting point";
            d["objectives.ny22.chase"] = "Catch the informant";
            d["objectives.ny22.escort"] = "Break the informant's escort";
            d["objectives.ny22.shelter"] = "Get the informant to safety";
            d["objectives.ny23.wagons"] = "Search the wagons";
            d["objectives.ny23.escort"] = "Break the convoy escort";
            d["objectives.ny23.captives"] = "Identify the captives";
            d["objectives.ny23.free"] = "Free the convoy";
            d["objectives.ny24.terminal"] = "Enter the terminal";
            d["objectives.ny24.cages"] = "Open the cages";
            d["objectives.ny24.guard"] = "Break the dock guard";
            d["objectives.ny24.regisseur"] = "Defeat the Steward";

            // --- Dialogues ---
            d["speaker.regisseur"] = "Elias Voss, the Steward";
            d["speaker.indic"] = "An Informant";
            d["speaker.commercant"] = "A Vendor";
            d["speaker.captif"] = "A Captive";
            d["speaker.narrateur"] = "Narrator";
            d["narration.ny_2_1.intro.0"] = "Flushing teems. Every stall has a camera above the register, and no one looks up.";
            d["narration.ny_2_1.intro.1"] = "Ten cameras over a market of onions. The Syndicate doesn't miss a transaction.";
            d["narration.ny_2_1.intro.2"] = "Or someone lends it the gear.";
            d["narration.ny_2_1.intro.3"] = "They've been running since summer. They see us. They never help us.";
            d["narration.ny_2_1.intro.4"] = "If we cut them all at once, the whole market will know. We go stall by stall.";
            d["narration.ny_2_1.intro.5"] = "You're The Dawn. Be quick, before someone talks.";
            d["narration.ny_2_1.elite.before.0"] = "You don't cut what protects the neighborhood. Least of all for visitors.";
            d["narration.ny_2_1.elite.before.1"] = "\"Protect\": are you sure that's the word they gave you?";
            d["narration.ny_2_1.elite.after.0"] = "Look at the housing. It's not a street model. There's a logo engraved underneath.";
            d["narration.ny_2_1.elite.after.1"] = "An AMIF contractor. The Syndicate's cameras are leased from the agency.";
            d["narration.ny_2_1.exit.0"] = "A gang leasing its eyes from a state agency. We follow the trail to Astoria.";
            d["narration.ny_2_1.exit.1"] = "Or we follow the cameras. They all point the same way.";
            d["narration.ny_2_2.intro.0"] = "Astoria, one in the morning. The café is half closed, the room half full. Everyone speaks in low voices.";
            d["narration.ny_2_2.intro.1"] = "The man in the back hasn't touched his coffee. He checks the door every thirty seconds.";
            d["narration.ny_2_2.intro.2"] = "I'm not a snitch. I'm a guy with a daughter.";
            d["narration.ny_2_2.intro.3"] = "We're not asking you to betray anyone. We're asking you to say what we can no longer ignore.";
            d["narration.ny_2_2.intro.4"] = "He isn't lying. He's afraid for someone, not for himself.";
            d["narration.ny_2_2.elite.before.0"] = "Everybody stay seated. We have visitors who weren't invited.";
            d["narration.ny_2_2.elite.after.0"] = "Out, quick. Before the others figure it out.";
            d["narration.ny_2_2.exit.0"] = "Every Thursday, the Syndicate brings a list. Names, neighborhoods, powers. A guy in a suit takes it, and never gives his name.";
            d["narration.ny_2_2.exit.1"] = "A suit from the AMIF?";
            d["narration.ny_2_2.exit.2"] = "A suit that never gets dirty. New Apex show up on the list before they've even been afraid.";
            d["narration.ny_2_2.exit.3"] = "We're taking you to a safe house. Your daughter too.";
            d["narration.ny_2_3.intro.0"] = "The Jamaica rail yard sleeps under orange lamps. An unmarked convoy waits on track 14.";
            d["narration.ny_2_3.intro.1"] = "No label, no number, and an armed escort. That's not freight.";
            d["narration.ny_2_3.intro.2"] = "There are people in those cars. I can hear them breathing.";
            d["narration.ny_2_3.intro.3"] = "Then we don't slow down. We open the last car.";
            d["narration.ny_2_3.elite.before.0"] = "This convoy is under contract. Back off, or the contract will include you.";
            d["narration.ny_2_3.elite.before.1"] = "He's reciting. He doesn't even know who he works for.";
            d["narration.ny_2_3.elite.after.0"] = "You're... from The Dawn? They told us we were going somewhere safe.";
            d["narration.ny_2_3.elite.after.1"] = "Who told you that?";
            d["narration.ny_2_3.elite.after.2"] = "A man with a badge. He said the agency would take care of us.";
            d["narration.ny_2_3.exit.0"] = "Awakened Apex, delivered like parcels. To the agency.";
            d["narration.ny_2_3.exit.1"] = "This convoy's ledger will be at the freight terminal. That's where we'll find it.";
            d["narration.ny_2_4.intro.0"] = "The freight terminal overlooks the bay. At the top of a crane, a glass room watches every container.";
            d["narration.ny_2_4.intro.1"] = "From up there, you see everything. Exactly what a manager wants.";
            d["narration.ny_2_4.intro.2"] = "Deliveries, schedules, numbers... and beside them, the same logo as on the cameras.";
            d["narration.ny_2_4.intro.3"] = "We don't take excerpts. We take the whole ledger.";
            d["narration.ny_2_4.elite.before.0"] = "Management doesn't receive without an appointment.";
            d["narration.ny_2_4.elite.after.0"] = "There's only him up there now. And he's expecting us.";
            d["narration.ny_2_4.boss.before.0"] = "You're late. The last convoy left without you, and yet here you are. What a way to deliver.";
            d["narration.ny_2_4.boss.before.1"] = "I don't run a gang. I manage a contract. Contracts don't stop because someone regrets them.";
            d["narration.ny_2_4.boss.before.2"] = "People were locked in your cars.";
            d["narration.ny_2_4.boss.before.3"] = "People were transported. There's a difference: one pays, and the other complains.";
            d["narration.ny_2_4.boss.after.0"] = "The ledger! Every delivery carries the same signatory: the agency.";
            d["narration.ny_2_4.boss.after.1"] = "The Syndicate hands over the captives. The AMIF provides the surveillance, and resells what it doesn't keep. Everything is recorded.";
            d["narration.ny_2_4.boss.after.2"] = "It's the proof Brooklyn only let us guess at: they don't fight each other, they partner.";
            d["narration.ny_2_4.boss.after.3"] = "You think you're taking down a gang. You've just disrupted a contract.";
            d["narration.ny_2_4.chapter_end.0"] = "The captives are freed. The transport ledger is safe: the first proof of collusion between the Syndicate and the AMIF.";
            d["narration.ny_2_4.chapter_end.1"] = "Keep it. We'll need it in Salem.";

            // --- Memory fragments ---
            d["fragment.ny_flushing_cameras.title"] = "Eyes for Rent";
            d["fragment.ny_flushing_cameras.text"] = "Ten cameras over a market of onions, and no one looking up. They saw every coin, every face, every Apex who hesitated at a stall. Under one of them, an agency logo: it isn't the Syndicate watching you, it's whoever rents it the view.";
            d["fragment.ny_astoria_informant.title"] = "Thursday's List";
            d["fragment.ny_astoria_informant.text"] = "He wasn't a snitch, he was a father. Every Thursday a list changed hands: names, neighborhoods, powers still asleep. Someone in an immaculate suit filed it away without looking up.";
            d["fragment.ny_jamaica_convoy.title"] = "Track 14";
            d["fragment.ny_jamaica_convoy.text"] = "The cars had no name or number, and yet everything about them was expected. You heard breathing behind the steel, and a voice repeating that they were going somewhere safe. It had never had a reason to distrust a badge.";
            d["fragment.ny_terminal_register.title"] = "The Transport Ledger";
            d["fragment.ny_terminal_register.text"] = "Everything was recorded: dates, cars, the names of those who'd been taken. Beside every line, a single signatory, and it wasn't the Syndicate. The captives weren't being delivered to a gang, but to an agency.";

            // --- Route variants, boss phases, opening ---
            d["narration.ny_2_1.intro.4.m"] = "I see what each camera filmed yesterday. We cut them in order: the ones watching the register last.";
            d["narration.ny_2_1.exit.2"] = "In Moscow it was holograms. Here, cameras. The same polite smile behind them.";
            d["narration.ny_2_2.intro.3.m"] = "I see what happened to you this morning. We're not asking you to talk: we're asking you to let us listen.";
            d["narration.ny_2_2.exit.4"] = "In Moscow they say clients. Here, lists. It's the same word.";
            d["narration.ny_2_3.intro.3.m"] = "I see who climbed into those cars, and what they were promised. We don't slow down.";
            d["narration.ny_2_3.exit.2"] = "In Moscow they call them trophies. Here, captives.";
            d["narration.ny_2_4.boss.before.2.m"] = "I see those cars full, and what you collect for each one.";
            d["narration.ny_2_4.boss.after.4"] = "Moscow had its oligarchs. New York has a steward. The logic is identical: people as merchandise.";
            d["narration.ny_2_4.boss.phase1.0"] = "The contract is signed. All that's left is for you to carry it out.";
            d["narration.ny_2_4.boss.phase2.0"] = "Termination. I don't like losing a client.";
            d["chapter.ny_queens.intro.agent_d"] = "He knows the sound of these streets. He is about to learn who listens to them.";
            d["chapter.ny_queens.intro.agent_m"] = "She has seen this pattern before in Moscow: a gang, an agency, a contract.";
            return d;
        }
    }
}
