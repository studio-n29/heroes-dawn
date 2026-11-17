#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes du chapitre Kapotnya : français (<c>ExtendKapotnya</c>) et anglais (<c>KapotnyaEnglish</c>, fusionné dans <c>En</c>).</summary>
    public static partial class Loc
    {
        static partial void ExtendKapotnya(Dictionary<string, string> d)
        {
            // --- Ennemis ---
            d["enemies.kap_garde.name"] = "Garde de raffinerie";
            d["enemies.kap_convoyeur.name"] = "Convoyeur";
            d["enemies.kap_technicien.name"] = "Technicien du laboratoire";
            d["enemies.kap_surveillant.name"] = "Surveillant de site";
            d["enemies.kap_contremaitre.name"] = "LE CONTREMAÎTRE";
            d["enemies.kap_contremaitre.phase2"] = "Dernier chargement";
            d["enemies.kap_contremaitre.phase3"] = "Tout est signé";
            d["skills.kap_garde_a1"] = "Matraque de service";
            d["skills.kap_garde_a2"] = "Périmètre de sécurité";
            d["skills.kap_convoyeur_a1"] = "Tir de sommation";
            d["skills.kap_convoyeur_a2"] = "Bordereau signé";
            d["skills.kap_technicien_a1"] = "Décharge de paillasse";
            d["skills.kap_technicien_a2"] = "Réactif instable";
            d["skills.kap_surveillant_a1"] = "Inspection";
            d["skills.kap_surveillant_a2"] = "Évacuation d'office";
            d["skills.kap_surveillant_a3"] = "Combinaison ignifugée";
            d["skills.kap_contremaitre_a1"] = "Pointage";
            d["skills.kap_contremaitre_a2"] = "Cadenas de quai";
            d["skills.kap_contremaitre_a3"] = "Chargement groupé";
            d["skills.kap_contremaitre_a4"] = "Avis de licenciement";
            d["skills.kap_contremaitre_p2_a2"] = "Retenue sur salaire";
            d["archives.enemy.kap_garde"] = "Garde de raffinerie : il tient le périmètre au nom d'un employeur qu'il ne nomme jamais. Il se couvre d'un bouclier et attire les coups.";
            d["archives.enemy.kap_convoyeur"] = "Convoyeur : tireur posté sur les camions. Chaque tir est accompagné d'un bordereau en règle.";
            d["archives.enemy.kap_technicien"] = "Technicien du laboratoire : il manipule des prélèvements sans poser de questions. Ses réactifs rongent tout ce qu'ils touchent.";
            d["archives.enemy.kap_surveillant"] = "Surveillant de site : l'élite de Kapotnya. Il évacue les importuns avec la même courtoisie qu'il évacue les ouvriers.";
            d["archives.enemy.kap_contremaitre"] = "LE CONTREMAÎTRE : Fiodor Gromov, chef de quai. Il ne pose pas de questions et signe tous les bordereaux. Un homme de confiance, payé pour ne rien savoir.";

            // --- Zones (chapitre Kapotnya) ---
            d["zones.mos_2_1.name"] = "RAFFINERIE — LA FUITE";
            d["zones.mos_2_1.short"] = "Raffinerie";
            d["zones.mos_2_1.code"] = "Zone M2.1";
            d["zones.mos_2_1.desc"] = "Une fuite de gaz vient d'éclater à la raffinerie et les ouvriers sont bloqués. Colmate la fuite, brise la garde et ouvre les passages. Une vanne a été tournée à la main avant l'alerte : ce n'est pas un accident.";
            d["zones.mos_2_2.name"] = "ROUTE DU CONVOI — LES CAMIONS";
            d["zones.mos_2_2.short"] = "Route du convoi";
            d["zones.mos_2_2.code"] = "Zone M2.2";
            d["zones.mos_2_2.desc"] = "Les camions roulent sans phares dans la brume, sur la route industrielle. Rattrape-les, brise l'escorte et vois ce que contiennent ces caisses lumineuses : des résidus de la brèche de la Supernova.";
            d["zones.mos_2_3.name"] = "LABORATOIRE CLANDESTIN — LES DOSSIERS";
            d["zones.mos_2_3.short"] = "Laboratoire";
            d["zones.mos_2_3.code"] = "Zone M2.3";
            d["zones.mos_2_3.desc"] = "Le laboratoire est caché dans un ancien entrepôt frigorifique : couloirs blancs, tubes, Apex prélevés, attachés, affaiblis. Fouille les dossiers avant que le chef du laboratoire ne les détruise, et libère les captifs.";
            d["zones.mos_2_4.name"] = "QUAI DE CHARGEMENT — LE CONTREMAÎTRE";
            d["zones.mos_2_4.short"] = "Quai de chargement";
            d["zones.mos_2_4.code"] = "Zone M2.4";
            d["zones.mos_2_4.desc"] = "Le quai de Kapotnya charge le dernier camion avant l'aube. Franchis les grues, brise la garde du quai et affronte Gromov, qui vérifie, charge et signe sans jamais poser de question.";
            d["qg.fil.zone_mos_2_1"] = "Les camions du transfert prennent la route du convoi.";
            d["qg.fil.zone_mos_2_2"] = "La route mène à un laboratoire clandestin.";
            d["qg.fil.zone_mos_2_3"] = "Les dossiers désignent le quai de chargement.";
            d["qg.fil.zone_mos_2_4"] = "Direction Sadovoye Koltso et les tours d'Orlan.";
            d["archives.zone.zone_mos_2_1"] = "À la raffinerie de Kapotnya, l'Aube colmate une fuite volontaire qui servait à couvrir un transfert de marchandises.";
            d["archives.zone.zone_mos_2_2"] = "Sur la route de Kapotnya, l'Aube intercepte des camions chargés de résidus de la brèche. Les caisses portent un code soviétique.";
            d["archives.zone.zone_mos_2_3"] = "Dans le laboratoire clandestin de Kapotnya, l'Aube libère des Apex prélevés et saisit les dossiers montrant l'AMIF comme client : seconde partie de la preuve du marché noir.";
            d["archives.zone.zone_mos_2_4"] = "Au quai de chargement de Kapotnya, l'Aube vainc le Contremaître. Il lâche un nom avant de tomber : « Orlan paie. »";

            // --- Fragments ---
            d["fragment.mos_refinery_leak.title"] = "La fuite qui servait d'écran";
            d["fragment.mos_refinery_leak.text"] = "Les torchères brûlaient comme des cierges sur un autel de tuyaux. La fuite n'était pas un accident : on avait ouvert la vanne pour que tous regardent les flammes pendant qu'on chargeait autre chose dans l'ombre.";
            d["fragment.mos_convoy_residues.title"] = "Du charbon qui brille";
            d["fragment.mos_convoy_residues.text"] = "Les résidus de la brèche voyageaient dans des caisses de charbon, sous une bâche. Ils brillaient d'une lumière qui n'appartenait à aucune lampe. Tu as pensé à Paris, et à ceux qui s'étaient demandé pourquoi tout le monde voulait cette lueur.";
            d["fragment.mos_lab_files.title"] = "Les dossiers du laboratoire";
            d["fragment.mos_lab_files.text"] = "Tout était rangé, daté, signé : des Apex, des prélèvements, des clients. Un client revenait à chaque page. Dans les marges, une écriture ancienne annotait chaque ligne, comme si les morts continuaient à tenir les comptes.";
            d["fragment.mos_quay_gromov.title"] = "Un chef de quai";
            d["fragment.mos_quay_gromov.text"] = "Il ne posait pas de questions. Il pesait, comptait, signait. Chaque bordereau portait le même nom, et tu as compris que le mal le plus poli est celui qui se contente de signer.";

            // --- Objectifs ---
            d["objectives.mos21.gate"] = "Franchir le portail de la raffinerie";
            d["objectives.mos21.leak"] = "Colmater la fuite";
            d["objectives.mos21.guard"] = "Briser la garde de la raffinerie";
            d["objectives.mos21.workers"] = "Sortir les ouvriers";
            d["objectives.mos22.chase"] = "Rattraper les camions";
            d["objectives.mos22.escort"] = "Briser l'escorte du convoi";
            d["objectives.mos22.residue"] = "Inspecter les caisses de résidus";
            d["objectives.mos22.roadend"] = "Suivre la route jusqu'au bout";
            d["objectives.mos23.files"] = "Fouiller les dossiers";
            d["objectives.mos23.techs"] = "Vaincre les techniciens";
            d["objectives.mos23.free"] = "Libérer les Apex";
            d["objectives.mos23.leave"] = "Quitter le laboratoire";
            d["objectives.mos24.cranes"] = "Franchir les grues";
            d["objectives.mos24.overseer"] = "Briser la garde du quai";
            d["objectives.mos24.dockers"] = "Faire parler les dockers";
            d["objectives.mos24.foreman"] = "Vaincre le Contremaître";

            // --- Dialogues ---
            d["speaker.ouvrier"] = "Un ouvrier";
            d["speaker.technicien"] = "Un technicien";
            d["speaker.apex_libere"] = "Un Apex libéré";
            d["speaker.contremaitre"] = "Fiodor Gromov, le Contremaître";
            d["speaker.narrateur"] = "Narrateur";
            d["narration.mos_2_1.intro.0"] = "Kapotnya respire un brouillard jaune. Les torchères éclairent les tuyaux comme des cierges.";
            d["narration.mos_2_1.intro.1"] = "Alarme de gaz sur le bloc C. Il reste des gens à l'intérieur.";
            d["narration.mos_2_1.intro.2"] = "La vanne était ouverte avant l'alarme. Quelqu'un l'a tournée à la main.";
            d["narration.mos_2_1.intro.3"] = "Pourquoi provoquer une fuite sur sa propre raffinerie ?";
            d["narration.mos_2_1.elite.before.0"] = "L'évacuation est suspendue. Le transfert ne s'arrête pas pour un accident.";
            d["narration.mos_2_1.elite.before.1"] = "Il parle de transfert avant de parler des ouvriers. Il vient de se trahir.";
            d["narration.mos_2_1.elite.after.0"] = "On nous a dit de quitter le site pour la nuit. Quelques-uns ont refusé.";
            d["narration.mos_2_1.elite.after.1"] = "La fuite sert d'écran : pendant qu'on regarde les flammes, on charge autre chose.";
            d["narration.mos_2_1.exit.0"] = "Une fuite volontaire pour couvrir un transfert. Les camions partent vers la route du convoi.";
            d["narration.mos_2_1.exit.1"] = "Alors on les suit.";
            d["narration.mos_2_2.intro.0"] = "La route du convoi traverse la brume. Les camions roulent sans phares, comme s'ils connaissaient chaque virage.";
            d["narration.mos_2_2.intro.1"] = "Quatre camions, une escorte, et pas un seul feu allumé.";
            d["narration.mos_2_2.intro.2"] = "Les camions sont chargés de ce que nous avons vu sur la voie 9. Mais plus lourd, plus dangereux.";
            d["narration.mos_2_2.elite.before.0"] = "Ce convoi est autorisé. Vous avez la permission de reculer.";
            d["narration.mos_2_2.elite.before.1"] = "Autorisé par qui ? Il ne répond pas.";
            d["narration.mos_2_2.elite.after.0"] = "Les caisses brillent. Ce sont des résidus de la Supernova.";
            d["narration.mos_2_2.elite.after.1"] = "Des fragments de la brèche, transportés comme du charbon.";
            d["narration.mos_2_2.exit.0"] = "Même code soviétique que sur les caisses de Sergueiv. Quelqu'un relie les deux.";
            d["narration.mos_2_2.exit.1"] = "Alors le laboratoire est au bout de cette route.";
            d["narration.mos_2_3.intro.0"] = "Sous l'entrepôt frigorifique, un laboratoire. Les couloirs sont blancs, les tubes pleins d'une lumière qui n'est pas de l'électricité.";
            d["narration.mos_2_3.intro.1"] = "Je n'aime pas cette lumière. Elle a la couleur de Paris.";
            d["narration.mos_2_3.intro.2"] = "Ne… ne me touchez pas. Ils m'ont déjà pris ce qui brillait en moi.";
            d["narration.mos_2_3.intro.3"] = "Tu es en sécurité. Respire. On te sort.";
            d["narration.mos_2_3.elite.before.0"] = "Les dossiers ne sortent pas. Je les détruirai plutôt que de vous les laisser.";
            d["narration.mos_2_3.elite.before.1"] = "Je vois ses mains avant qu'il ne bouge. Il va jeter les dossiers dans l'incinérateur.";
            d["narration.mos_2_3.elite.after.0"] = "Les dossiers sont intacts. Toutes les livraisons, tous les noms de clients.";
            d["narration.mos_2_3.elite.after.1"] = "Un client revient à chaque page : l'AMIF.";
            d["narration.mos_2_3.elite.after.2"] = "Et dans les marges, des annotations signées « Volkov ». Des chiffres d'avant l'Union.";
            d["narration.mos_2_3.exit.0"] = "Merci. Je croyais que personne ne viendrait.";
            d["narration.mos_2_3.exit.1"] = "Les dossiers du laboratoire, avec le registre du monastère : le marché noir est prouvé.";
            d["narration.mos_2_3.exit.2"] = "Il manque encore celui qui les paie.";
            d["narration.mos_2_4.intro.0"] = "Le quai de chargement s'étend sous les grues. Un dernier camion attend, les phares éteints.";
            d["narration.mos_2_4.intro.1"] = "Tous les bordereaux portent la même signature, ligne après ligne.";
            d["narration.mos_2_4.elite.before.0"] = "Ce quai est fermé. Le chef de quai vous recevra, mais pas ce soir.";
            d["narration.mos_2_4.elite.after.0"] = "La garde est tombée. Il attend sur la passerelle.";
            d["narration.mos_2_4.boss.before.0"] = "Gromov, chef de quai. Vous êtes en retard : le dernier camion est déjà parti.";
            d["narration.mos_2_4.boss.before.1"] = "Je ne pose pas de questions. Je vérifie les bordereaux et je signe. C'est pour cela qu'on me paie.";
            d["narration.mos_2_4.boss.before.2"] = "Par qui ?";
            d["narration.mos_2_4.boss.before.3"] = "Un chef de quai ne répond pas à cette question. Un chef de quai pèse, compte, expédie.";
            d["narration.mos_2_4.boss.after.0"] = "Assez. Je ne suis qu'un chef de quai : je compte, je charge, je signe.";
            d["narration.mos_2_4.boss.after.1"] = "Orlan paie.";
            d["narration.mos_2_4.boss.after.2"] = "Orlan.";
            d["narration.mos_2_4.chapter_end.0"] = "L'Aube sait désormais pour qui travaillent les camions. Le nom d'Orlan mène aux tours de Sadovoye Koltso.";

            // --- Variantes par parcours, indices de la cathédrale, phases de boss, ouvertures ---
            d["narration.mos_2_1.intro.2.d"] = "Je vois la trace d'une main sur cette vanne. Quelqu'un l'a ouverte exprès.";
            d["narration.mos_2_1.intro.4"] = "Les torchères de la raffinerie… de loin, on dirait des cierges. Chez moi, tout brûlait comme ça.";
            d["narration.mos_2_2.intro.2.d"] = "Ces caisses ont été emballées par des mains qui savent ce qu'elles portent.";
            d["narration.mos_2_3.intro.4"] = "L'odeur… cette cire brûlée, ici ? Sous un laboratoire ?";
            d["narration.mos_2_3.elite.before.1.d"] = "Il n'a pas l'air d'un chimiste. Il a l'air d'un homme qui a peur de ceux qui lisent ces papiers.";
            d["narration.mos_2_4.boss.before.2.d"] = "Qui signe vos chèques ?";
            d["narration.mos_2_4.boss.after.3"] = "La cire sur les caisses ? Une habitude des hommes qui nous les confient. Une habitude d'église.";
            d["narration.mos_2_4.boss.phase1.0"] = "Dernier chargement. Je regrette de vous faire attendre.";
            d["narration.mos_2_4.boss.phase2.0"] = "Tout est signé. Vous n'avez plus rien à contester.";
            d["chapter.mos_kapotnya.intro.agent_m"] = "Elle connaît ce brouillard : sa ville le respire depuis toujours. Elle n'a jamais su ce qu'il couvrait.";
            d["chapter.mos_kapotnya.intro.agent_d"] = "Il a déjà vu un gang et une agence marcher ensemble. Ici, c'est un État qui regarde ailleurs.";
        }

        static Dictionary<string, string> KapotnyaEnglish(Dictionary<string, string> d)
        {
            // --- Enemies ---
            d["skills.pyro_a1"] = "Flame Jet";   // ennemi réutilisé (street_pyro) : texte EN absent de la base
            d["skills.pyro_a2"] = "Molotov";
            d["enemies.kap_garde.name"] = "Refinery Guard";
            d["enemies.kap_convoyeur.name"] = "Convoy Gunman";
            d["enemies.kap_technicien.name"] = "Lab Technician";
            d["enemies.kap_surveillant.name"] = "Site Supervisor";
            d["enemies.kap_contremaitre.name"] = "THE FOREMAN";
            d["enemies.kap_contremaitre.phase2"] = "Last Load";
            d["enemies.kap_contremaitre.phase3"] = "Everything Is Signed";
            d["skills.kap_garde_a1"] = "Service Baton";
            d["skills.kap_garde_a2"] = "Safety Perimeter";
            d["skills.kap_convoyeur_a1"] = "Warning Round";
            d["skills.kap_convoyeur_a2"] = "Signed Waybill";
            d["skills.kap_technicien_a1"] = "Bench Discharge";
            d["skills.kap_technicien_a2"] = "Unstable Reagent";
            d["skills.kap_surveillant_a1"] = "Inspection";
            d["skills.kap_surveillant_a2"] = "Compulsory Evacuation";
            d["skills.kap_surveillant_a3"] = "Fireproof Suit";
            d["skills.kap_contremaitre_a1"] = "Clocking In";
            d["skills.kap_contremaitre_a2"] = "Dock Padlock";
            d["skills.kap_contremaitre_a3"] = "Bulk Loading";
            d["skills.kap_contremaitre_a4"] = "Dismissal Notice";
            d["skills.kap_contremaitre_p2_a2"] = "Wage Deduction";
            d["archives.enemy.kap_garde"] = "Refinery guard: he holds the perimeter on behalf of an employer he never names. He raises a shield and draws the blows.";
            d["archives.enemy.kap_convoyeur"] = "Convoy gunman: a marksman posted on the trucks. Every shot comes with a waybill in perfect order.";
            d["archives.enemy.kap_technicien"] = "Lab technician: he handles samples without asking questions. His reagents eat through whatever they touch.";
            d["archives.enemy.kap_surveillant"] = "Site supervisor: the elite of Kapotnya. He escorts intruders out with the same courtesy he shows the workers.";
            d["archives.enemy.kap_contremaitre"] = "THE FOREMAN: Fiodor Gromov, dock foreman. He asks no questions and signs every waybill. A trusted man, paid to know nothing.";

            // --- Zones (Kapotnya chapter) ---
            d["zones.mos_2_1.name"] = "REFINERY — THE LEAK";
            d["zones.mos_2_1.short"] = "Refinery";
            d["zones.mos_2_1.code"] = "Zone M2.1";
            d["zones.mos_2_1.desc"] = "A gas leak has just broken out at the refinery and the workers are stuck. Plug the leak, break the guard and open the passages. A valve was turned by hand before the alarm: this is no accident.";
            d["zones.mos_2_2.name"] = "CONVOY ROAD — THE TRUCKS";
            d["zones.mos_2_2.short"] = "Convoy Road";
            d["zones.mos_2_2.code"] = "Zone M2.2";
            d["zones.mos_2_2.desc"] = "The trucks drive without headlights through the fog, along the industrial road. Catch them, break the escort and see what is inside those glowing crates: residues from the Supernova breach.";
            d["zones.mos_2_3.name"] = "UNDERGROUND LAB — THE FILES";
            d["zones.mos_2_3.short"] = "Lab";
            d["zones.mos_2_3.code"] = "Zone M2.3";
            d["zones.mos_2_3.desc"] = "The lab is hidden in a former cold-storage warehouse: white corridors, tubes, Apex extracted, bound, weakened. Search the files before the lab chief destroys them, and free the captives.";
            d["zones.mos_2_4.name"] = "LOADING DOCK — THE FOREMAN";
            d["zones.mos_2_4.short"] = "Loading Dock";
            d["zones.mos_2_4.code"] = "Zone M2.4";
            d["zones.mos_2_4.desc"] = "The Kapotnya dock loads the last truck before dawn. Get past the cranes, break the dock guard and face Gromov, who checks, loads and signs without ever asking a question.";
            d["qg.fil.zone_mos_2_1"] = "The transfer trucks take the convoy road.";
            d["qg.fil.zone_mos_2_2"] = "The road leads to a clandestine laboratory.";
            d["qg.fil.zone_mos_2_3"] = "The files point to the loading dock.";
            d["qg.fil.zone_mos_2_4"] = "On to Sadovoye Koltso and Orlan's towers.";
            d["archives.zone.zone_mos_2_1"] = "At the Kapotnya refinery, The Dawn plugs a deliberate leak that was covering a transfer of goods.";
            d["archives.zone.zone_mos_2_2"] = "On the Kapotnya road, The Dawn intercepts trucks loaded with residues from the breach. The crates carry a Soviet code.";
            d["archives.zone.zone_mos_2_3"] = "In Kapotnya's clandestine laboratory, The Dawn frees extracted Apex and seizes the files showing the AMIF as a client: the second part of the proof of the black market.";
            d["archives.zone.zone_mos_2_4"] = "At Kapotnya's loading dock, The Dawn defeats the Foreman. Before falling he lets a name slip: \"Orlan pays.\"";

            // --- Fragments ---
            d["fragment.mos_refinery_leak.title"] = "The Leak That Was a Screen";
            d["fragment.mos_refinery_leak.text"] = "The flares burned like candles on an altar of pipes. The leak was no accident: the valve was opened so everyone would watch the flames while something else was loaded in the dark.";
            d["fragment.mos_convoy_residues.title"] = "Coal That Glows";
            d["fragment.mos_convoy_residues.text"] = "The residues of the breach traveled in coal crates, under a tarp. They glowed with a light that belonged to no lamp. You thought of Paris, and of those who had wondered why everyone wanted that glow.";
            d["fragment.mos_lab_files.title"] = "The Laboratory Files";
            d["fragment.mos_lab_files.text"] = "Everything was filed, dated, signed: Apex, extractions, clients. One client came up on every page. In the margins, an old handwriting annotated every line, as if the dead were still keeping the books.";
            d["fragment.mos_quay_gromov.title"] = "A Dock Foreman";
            d["fragment.mos_quay_gromov.text"] = "He asked no questions. He weighed, counted, signed. Every waybill bore the same name, and you understood that the politest evil is the kind that merely signs.";

            // --- Objectives ---
            d["objectives.mos21.gate"] = "Get through the refinery gate";
            d["objectives.mos21.leak"] = "Plug the leak";
            d["objectives.mos21.guard"] = "Break the refinery guard";
            d["objectives.mos21.workers"] = "Bring the workers out";
            d["objectives.mos22.chase"] = "Catch up with the trucks";
            d["objectives.mos22.escort"] = "Break the convoy escort";
            d["objectives.mos22.residue"] = "Inspect the residue crates";
            d["objectives.mos22.roadend"] = "Follow the road to its end";
            d["objectives.mos23.files"] = "Search the files";
            d["objectives.mos23.techs"] = "Defeat the technicians";
            d["objectives.mos23.free"] = "Free the Apex";
            d["objectives.mos23.leave"] = "Leave the lab";
            d["objectives.mos24.cranes"] = "Get past the cranes";
            d["objectives.mos24.overseer"] = "Break the dock guard";
            d["objectives.mos24.dockers"] = "Make the dockers talk";
            d["objectives.mos24.foreman"] = "Defeat the Foreman";

            // --- Dialogue ---
            d["speaker.ouvrier"] = "Worker";
            d["speaker.apex_libere"] = "Freed Apex";
            d["speaker.contremaitre"] = "Fiodor Gromov, the Foreman";
            d["speaker.technicien"] = "Technician";
            d["speaker.narrateur"] = "Narrator";
            d["narration.mos_2_1.intro.0"] = "Kapotnya breathes a yellow fog. The flare stacks light the pipes like candles.";
            d["narration.mos_2_1.intro.1"] = "Gas alarm on block C. There are still people inside.";
            d["narration.mos_2_1.intro.2"] = "The valve was open before the alarm. Someone turned it by hand.";
            d["narration.mos_2_1.intro.3"] = "Why cause a leak at your own refinery?";
            d["narration.mos_2_1.elite.before.0"] = "The evacuation is suspended. The transfer doesn't stop for an accident.";
            d["narration.mos_2_1.elite.before.1"] = "He mentions the transfer before the workers. He just gave himself away.";
            d["narration.mos_2_1.elite.after.0"] = "We were told to leave the site for the night. A few of us refused.";
            d["narration.mos_2_1.elite.after.1"] = "The leak is a screen: while we watch the flames, they load something else.";
            d["narration.mos_2_1.exit.0"] = "A deliberate leak to cover a transfer. The trucks are heading for the convoy road.";
            d["narration.mos_2_1.exit.1"] = "Then we follow them.";
            d["narration.mos_2_2.intro.0"] = "The convoy road crosses the fog. The trucks drive without headlights, as if they knew every bend.";
            d["narration.mos_2_2.intro.1"] = "Four trucks, an escort, and not a single light on.";
            d["narration.mos_2_2.intro.2"] = "The trucks are loaded with what we saw on track 9. But heavier, more dangerous.";
            d["narration.mos_2_2.elite.before.0"] = "This convoy is authorized. You have permission to back off.";
            d["narration.mos_2_2.elite.before.1"] = "Authorized by whom? He doesn't answer.";
            d["narration.mos_2_2.elite.after.0"] = "The crates are glowing. These are Supernova residues.";
            d["narration.mos_2_2.elite.after.1"] = "Fragments of the breach, shipped like coal.";
            d["narration.mos_2_2.exit.0"] = "The same Soviet code as on the Sergueiv crates. Someone is linking the two.";
            d["narration.mos_2_2.exit.1"] = "Then the laboratory is at the end of this road.";
            d["narration.mos_2_3.intro.0"] = "Beneath the cold-storage warehouse, a laboratory. The corridors are white, the tubes full of a light that isn't electricity.";
            d["narration.mos_2_3.intro.1"] = "I don't like that light. It's the color of Paris.";
            d["narration.mos_2_3.intro.2"] = "Don't... don't touch me. They already took what shone inside me.";
            d["narration.mos_2_3.intro.3"] = "You're safe. Breathe. We're getting you out.";
            d["narration.mos_2_3.elite.before.0"] = "The files don't leave. I'll destroy them before I let you have them.";
            d["narration.mos_2_3.elite.before.1"] = "I see his hands before he moves. He's going to throw the files into the incinerator.";
            d["narration.mos_2_3.elite.after.0"] = "The files are intact. Every delivery, every client's name.";
            d["narration.mos_2_3.elite.after.1"] = "One client comes up on every page: the AMIF.";
            d["narration.mos_2_3.elite.after.2"] = "And in the margins, annotations signed \"Volkov.\" Figures from before the Union.";
            d["narration.mos_2_3.exit.0"] = "Thank you. I thought no one would come.";
            d["narration.mos_2_3.exit.1"] = "The laboratory files, together with the monastery ledger: the black market is proven.";
            d["narration.mos_2_3.exit.2"] = "We still need the one who pays them.";
            d["narration.mos_2_4.intro.0"] = "The loading dock stretches beneath the cranes. One last truck waits, headlights off.";
            d["narration.mos_2_4.intro.1"] = "Every waybill carries the same signature, line after line.";
            d["narration.mos_2_4.elite.before.0"] = "This dock is closed. The dock foreman will see you, but not tonight.";
            d["narration.mos_2_4.elite.after.0"] = "The guard has fallen. He's waiting on the gangway.";
            d["narration.mos_2_4.boss.before.0"] = "Gromov, dock foreman. You're late: the last truck has already left.";
            d["narration.mos_2_4.boss.before.1"] = "I don't ask questions. I check the waybills and I sign. That's what I'm paid for.";
            d["narration.mos_2_4.boss.before.2"] = "By whom?";
            d["narration.mos_2_4.boss.before.3"] = "A dock foreman doesn't answer that question. A dock foreman weighs, counts, ships.";
            d["narration.mos_2_4.boss.after.0"] = "Enough. I'm only a dock foreman: I count, I load, I sign.";
            d["narration.mos_2_4.boss.after.1"] = "Orlan pays.";
            d["narration.mos_2_4.boss.after.2"] = "Orlan.";
            d["narration.mos_2_4.chapter_end.0"] = "The Dawn now knows who the trucks work for. Orlan's name leads to the towers of Sadovoye Koltso.";

            // --- Route variants, cathedral hints, boss phases, openings ---
            d["narration.mos_2_1.intro.2.d"] = "I see the trace of a hand on this valve. Someone opened it on purpose.";
            d["narration.mos_2_1.intro.4"] = "The refinery flares... from afar, they look like candles. Back home, everything burned like that.";
            d["narration.mos_2_2.intro.2.d"] = "These crates were packed by hands that know what they're carrying.";
            d["narration.mos_2_3.intro.4"] = "That smell... burnt wax, here? Under a laboratory?";
            d["narration.mos_2_3.elite.before.1.d"] = "He doesn't look like a chemist. He looks like a man afraid of whoever reads these papers.";
            d["narration.mos_2_4.boss.before.2.d"] = "Who signs your paychecks?";
            d["narration.mos_2_4.boss.after.3"] = "The wax on the crates? A habit of the men who entrust them to us. A church habit.";
            d["narration.mos_2_4.boss.phase1.0"] = "Last load. I'm sorry to keep you waiting.";
            d["narration.mos_2_4.boss.phase2.0"] = "Everything is signed. You have nothing left to contest.";
            d["chapter.mos_kapotnya.intro.agent_m"] = "She knows this fog: her city has breathed it forever. She never knew what it covered.";
            d["chapter.mos_kapotnya.intro.agent_d"] = "He has already seen a gang and an agency work hand in hand. Here, it is a state looking the other way.";
            return d;
        }
    }
}
