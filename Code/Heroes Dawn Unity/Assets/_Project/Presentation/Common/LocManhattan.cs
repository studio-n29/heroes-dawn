#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes du chapitre Manhattan : français (<c>ExtendManhattan</c>) et anglais (<c>ManhattanEnglish</c>, fusionné dans <c>En</c>).</summary>
    public static partial class Loc
    {
        static partial void ExtendManhattan(Dictionary<string, string> d)
        {
            // --- Ennemis ---
            d["enemies.mnh_guard.name"] = "Garde de tour";
            d["enemies.mnh_agent.name"] = "Agent de sécurité privée";
            d["enemies.mnh_drone.name"] = "Drone de propagande";
            d["enemies.mnh_chief.name"] = "Chef de la sécurité";
            d["enemies.mnh_stroke.name"] = "LE GÉNÉRAL STROKE";
            d["enemies.mnh_stroke.phase2"] = "Procédure d'urgence";
            d["enemies.mnh_stroke.phase3"] = "Protocole de rétention";
            d["skills.mnh_guard_a1"] = "Matraque réglementaire";
            d["skills.mnh_guard_a2"] = "Cordon privé";
            d["skills.mnh_agent_a1"] = "Tir de sommation";
            d["skills.mnh_agent_a2"] = "Cible validée";
            d["skills.mnh_drone_a1"] = "Rafale de slogans";
            d["skills.mnh_drone_a2"] = "Flash publicitaire";
            d["skills.mnh_chief_a1"] = "Rapport d'incident";
            d["skills.mnh_chief_a2"] = "Évacuation forcée";
            d["skills.mnh_chief_a3"] = "Périmètre fermé";
            d["skills.mnh_stroke_a1"] = "Tir de précision";
            d["skills.mnh_stroke_a2"] = "Position retranchée";
            d["skills.mnh_stroke_a3"] = "Tir de barrage";
            d["skills.mnh_stroke_a4"] = "Ordre de reddition";
            d["skills.mnh_stroke_p2_a2"] = "Lance-grenades";
            d["skills.mnh_stroke_p3_a3"] = "Barrage final";
            d["archives.enemy.mnh_guard"] = "Garde de tour : le portier des immeubles de Midtown. Il se couvre d'un bouclier et attire les coups sur lui.";
            d["archives.enemy.mnh_agent"] = "Agent de sécurité privée : tireur payé par les financeurs de l'AMIF et du Syndicat. Il valide sa cible avant de tirer.";
            d["archives.enemy.mnh_drone"] = "Drone de propagande : il diffuse le message de l'AMIF sur les écrans et aveugle quiconque lève les yeux.";
            d["archives.enemy.mnh_chief"] = "Chef de la sécurité : l'élite des cercles de pouvoir. Ses évacuations forcées secouent toute l'escouade.";
            d["archives.enemy.mnh_stroke"] = "LE GÉNÉRAL STROKE : un homme, pas un méta-humain. Il gagne par l'équipement et le dispositif : à chaque phase, il change de matériel.";
            d["archives.enemy.amif_trooper"] = "Soldat de l'AMIF : fantassin de l'agence, qui frappe l'escouade entière quand il se déploie.";
            d["archives.enemy.amif_marksman"] = "Tireur de l'AMIF : posté en retrait, il marque sa cible avant le tir décisif.";
            d["archives.enemy.amif_captain"] = "Capitaine de l'AMIF : officier de première ligne. Il se couvre d'un bouclier et secoue l'escouade d'un coup d'ensemble.";

            // --- Zones (chapitre Manhattan) ---
            d["zones.ny_4_1.name"] = "TIMES SQUARE — LES ÉCRANS";
            d["zones.ny_4_1.short"] = "Times Square";
            d["zones.ny_4_1.code"] = "Zone 4.1";
            d["zones.ny_4_1.desc"] = "Sur chaque écran de Times Square, l'AMIF présente l'Aube comme une menace pour la ville. Cette fois, il n'y a plus de gang entre vous et lui. Reprends la régie et laisse New York voir ce que tu vois.";
            d["zones.ny_4_2.name"] = "TOURS DE MIDTOWN — LES CERCLES";
            d["zones.ny_4_2.short"] = "Tours de Midtown";
            d["zones.ny_4_2.code"] = "Zone 4.2";
            d["zones.ny_4_2.desc"] = "Les étages les plus hauts de Midtown abritent ceux qui paient. Infiltre leurs cercles, passe leurs gardes et lis la liste de leurs donateurs : certains noms n'ont rien à y faire deux fois.";
            d["zones.ny_4_3.name"] = "SIÈGE DE L'ONU — L'ANNONCE";
            d["zones.ny_4_3.short"] = "Siège de l'ONU";
            d["zones.ny_4_3.code"] = "Zone 4.3";
            d["zones.ny_4_3.desc"] = "Les Accords de Salem vont être annoncés devant les délégués du monde entier. Rayner est dans la salle, Stroke aussi, et ils ne sont pas d'accord. Protège l'assemblée jusqu'à la lecture.";
            d["zones.ny_4_4.name"] = "ESPLANADE DE L'ONU — LE GÉNÉRAL";
            d["zones.ny_4_4.short"] = "Esplanade";
            d["zones.ny_4_4.code"] = "Zone 4.4";
            d["zones.ny_4_4.desc"] = "Les Accords sont sortis, mais Stroke n'a pas dit son dernier mot. Il t'attend seul sur l'esplanade, sous les caméras de la foule, calme et convaincu. Franchis sa garde et fais-lui face.";
            d["qg.fil.zone_ny_4_1"] = "Les cercles de pouvoir attendent à Midtown.";
            d["qg.fil.zone_ny_4_2"] = "L'annonce des Accords se prépare à l'ONU.";
            d["qg.fil.zone_ny_4_3"] = "Stroke attend sur l'esplanade.";
            d["qg.fil.zone_ny_4_4"] = "Direction Salem, où se joueront les Accords.";
            d["archives.zone.zone_ny_4_1"] = "À Times Square, l'Aube prend la régie des écrans et diffuse l'évacuation de Harlem à la place de la propagande de l'AMIF.";
            d["archives.zone.zone_ny_4_2"] = "Dans une tour de Midtown, l'Aube découvre que plusieurs financeurs de l'AMIF et du Syndicat se recoupent. Un nom renvoie à des archives des années 80.";
            d["archives.zone.zone_ny_4_3"] = "À l'ONU, Rayner et Stroke s'opposent publiquement devant les négociateurs, pendant que l'Aube protège la retransmission de l'annonce des Accords.";
            d["archives.zone.zone_ny_4_4"] = "Sur l'esplanade de l'ONU, l'Aube affronte le Général Stroke. Il se retire en promettant que Salem décidera. Les Accords sont désormais publics.";

            // --- Fragments ---
            d["fragment.ny_times_square.title"] = "Des écrans à l'envers";
            d["fragment.ny_times_square.text"] = "Chaque écran racontait ton histoire, mais à l'envers. Tu y voyais l'incendie sans les mains qui avaient porté les enfants. Pour une nuit, tu as repris la parole : tu n'as rien dit, tu as seulement montré.";
            d["fragment.ny_midtown_list.title"] = "Des noms sur deux listes";
            d["fragment.ny_midtown_list.text"] = "Tu pensais que le Syndicat et l'AMIF étaient deux ennemis qui s'arrangeaient. Tu as compris qu'ils partageaient une comptabilité. Cinq noms revenaient sur les deux listes, comme une signature qu'on ne cherche même plus à cacher.";
            d["fragment.ny_un_stage.title"] = "Deux voix";
            d["fragment.ny_un_stage.text"] = "Une voix parlait de protection, l'autre de liberté. Elles disaient presque les mêmes mots, mais pas pour les mêmes gens. Le monde écoutait. Pour la première fois, personne ne pouvait faire semblant de ne pas entendre.";
            d["fragment.ny_un_esplanade.title"] = "Salem décidera";
            d["fragment.ny_un_esplanade.text"] = "Il ne t'a pas menacé. Il a parlé de comptabilité, de chiffres, d'ordre. Il était seul sur l'esplanade, calme, convaincu. Tu as compris qu'on ne bat pas un homme comme lui d'un seul coup : on lui retire sa scène.";

            // --- Objectifs ---
            d["objectives.ny41.square"] = "Entrer dans Times Square";
            d["objectives.ny41.screens"] = "Pirater les écrans";
            d["objectives.ny41.control"] = "Vaincre le chef de la régie";
            d["objectives.ny41.broadcast"] = "Reprendre la diffusion";
            d["objectives.ny42.badges"] = "Infiltrer les cercles de pouvoir";
            d["objectives.ny42.security"] = "Vaincre la sécurité privée";
            d["objectives.ny42.roofs"] = "Franchir les toits des tours";
            d["objectives.ny42.ledger"] = "Récupérer la liste des financeurs";
            d["objectives.ny43.delegates"] = "Protéger les délégués";
            d["objectives.ny43.captain"] = "Vaincre le capitaine de l'AMIF";
            d["objectives.ny43.hall"] = "Tenir le hall de l'assemblée";
            d["objectives.ny43.accords"] = "Assister à l'annonce des Accords";
            d["objectives.ny44.esplanade"] = "Gagner l'esplanade";
            d["objectives.ny44.evacuate"] = "Évacuer la foule";
            d["objectives.ny44.line"] = "Briser la ligne de l'AMIF";
            d["objectives.ny44.stroke"] = "Vaincre le Général Stroke";

            // --- Dialogues ---
            d["speaker.agent_d"] = "Agent D";
            d["speaker.darius_brooks"] = "Darius Brooks";
            d["speaker.agent_amif"] = "Un agent de l'AMIF";
            d["speaker.voix"] = "Voix de propagande";
            d["speaker.narrateur"] = "Narrateur";
            // Scènes du script (13_Script_New_York.md, chapitre 4)
            d["narration.ny_4_1.intro.0"] = "Times Square ne dort jamais. Ce soir, chaque écran répète le même message : l'Aube est une menace.";
            d["narration.ny_4_1.intro.1"] = "« Les méta-humains non encadrés mettent en danger les civils. L'AMIF vous protège. »";
            d["narration.ny_4_1.intro.2"] = "Ils ont repris les images de Brooklyn. À l'envers.";
            d["narration.ny_4_1.intro.3"] = "La régie est au sous-sol. Si on y arrive, on peut remplacer le flux.";
            d["narration.ny_4_1.intro.4"] = "On ne leur répond pas avec des mots. On leur montre.";
            d["narration.ny_4_1.intro.4.m"] = "Je vois ce que ces écrans montreront dans une minute, si on les laisse faire. Remplaçons-le avant.";
            d["narration.ny_4_1.exit.3"] = "À Moscou, c'étaient des hologrammes. Ici, des panneaux. La mise en scène est la même.";
            d["narration.ny_4_2.intro.3.m"] = "Je vois qui a signé ces noms, et dans quel ordre. On ne vole rien : on lit.";
            d["narration.ny_4_2.exit.3"] = "Orlan, le Passeur, ces financeurs… Ce sont les mêmes personnes, en plus polies.";
            d["narration.ny_4_3.intro.3.m"] = "Je vois les mains qui couperont les câbles dans dix secondes. Je les arrête avant.";
            d["narration.ny_4_4.boss.before.2.m"] = "À Paris, j'ai vu ce que vous prélevez. Des milliers de morts, vous dites ? Vous avez aussi gardé les restes.";
            d["narration.ny_4_4.boss.phase1.0"] = "Procédure d'urgence. Je regrette qu'on en arrive là.";
            d["narration.ny_4_4.boss.phase2.0"] = "Protocole de rétention. Rien de personnel, rien d'humain : seulement de l'ordre.";
            d["narration.ny_4_4.boss.after.3.d"] = "Je ne sais pas ce qu'il prépare à Salem. Mais il sait qu'on y sera.";
            d["narration.ny_4_4.boss.after.3.m"] = "Salem. Là où Moscou et Paris se rejoignent. Il sait que j'y serai aussi.";
            d["chapter.ny_manhattan.intro.agent_d"] = "Il voit l'AMIF à visage découvert pour la première fois : jusque-là, elle n'avait qu'un Titan et des listes.";
            d["chapter.ny_manhattan.intro.agent_m"] = "Elle a vu l'AMIF prélever à Paris et ses clients sourire à Moscou. Ici, elle les voit réunis.";
            d["narration.ny_4_1.elite.before.0"] = "Cette zone est sous contrôle. Merci de reculer. Vous êtes filmés.";
            d["narration.ny_4_1.elite.before.1"] = "Il ne dit pas « interdit ». Il dit « filmés ». Il veut que ça se voie.";
            d["narration.ny_4_1.elite.after.0"] = "Régie atteinte. Le flux est à nous.";
            d["narration.ny_4_1.exit.0"] = "Sur tous les écrans, l'évacuation du bloc de Harlem : un enfant, des bras, une porte qui tient.";
            d["narration.ny_4_1.exit.1"] = "Pas le registre, pas encore. Une preuve diffusée trop tôt sera démentie. Montrons d'abord ce qu'on est.";
            d["narration.ny_4_1.exit.2"] = "Ça suffit pour ce soir. Les gens ont regardé.";
            d["narration.ny_4_2.intro.0"] = "La tour de verre domine Midtown. Au quarante-deuxième étage, un gala réunit des fondations, des conseils, des sourires bien payés.";
            d["narration.ny_4_2.intro.1"] = "On est les seuls à ne pas porter de badge doré.";
            d["narration.ny_4_2.intro.2"] = "Une salle de « partenaires ». Les noms sont affichés comme des trophées.";
            d["narration.ny_4_2.intro.3"] = "On ne vole rien. On compare.";
            d["narration.ny_4_2.elite.before.0"] = "Cette salle est réservée. Veuillez rejoindre le salon.";
            d["narration.ny_4_2.elite.before.1"] = "Il a l'air poli. Il est très inquiet.";
            d["narration.ny_4_2.elite.after.0"] = "J'ai la liste de l'AMIF. Toi, celle du registre de Queens.";
            d["narration.ny_4_2.elite.after.1"] = "Trois noms. Quatre. Cinq. Ils sont sur les deux.";
            d["narration.ny_4_2.exit.0"] = "Ceux qui paient l'AMIF paient aussi le Syndicat. Ce n'est pas une collusion, c'est un budget commun.";
            d["narration.ny_4_2.exit.1"] = "Et là : une fondation dont le nom me dit quelque chose. Des archives des années 80.";
            d["narration.ny_4_2.exit.2"] = "Pas ce soir. Mais on garde le nom.";
            d["narration.ny_4_3.intro.0"] = "Le hall de l'ONU est plein de drapeaux, de micros et de regards. Dans une heure, les Accords de Salem seront annoncés.";
            d["narration.ny_4_3.intro.1"] = "Quelqu'un coupe déjà les câbles de la retransmission.";
            d["narration.ny_4_3.intro.2"] = "Sabotage à l'étage technique. Si l'annonce ne passe pas, tout ça n'a servi à rien.";
            d["narration.ny_4_3.intro.3"] = "On tient la salle. Personne ne coupe cette retransmission.";
            d["narration.ny_4_3.elite.before.0"] = "Pour la sécurité des délégués, la salle est évacuée. Merci de coopérer.";
            d["narration.ny_4_3.elite.before.1"] = "Évacuer la salle, c'est étouffer l'annonce. Il le sait.";
            d["narration.ny_4_3.elite.after.0"] = "La salle est tenue. Rayner peut parler.";
            d["narration.ny_4_3.exit.0"] = "La salle retient son souffle. Pour la première fois, la voix de l'AMIF et celle de The Dawn s'opposent devant tous.";
            d["narration.ny_4_3.exit.1"] = "Il a gardé son calme. Il en gardera encore. Il nous attend dehors.";
            d["narration.ny_4_3.stage.0"] = "Les Apex ne demandent pas la permission d'exister. Ils demandent d'être reconnus, pas contrôlés.";
            d["narration.ny_4_3.stage.1"] = "Madame Rayner parle de reconnaissance. Je parle de responsabilité. Un pouvoir qu'on ne mesure pas est un pouvoir qu'on ne protège pas.";
            d["narration.ny_4_3.stage.2"] = "Mesurer, prélever, enfermer : ce n'est pas protéger. C'est posséder.";
            d["narration.ny_4_3.stage.3"] = "Les chiffres de Paris, madame, ne mentent pas. Je ne possède personne. Je compte.";
            d["narration.ny_4_3.stage.4"] = "Alors comptez aussi ceux que vous avez enfermés à Queens. Il y en avait beaucoup.";
            d["narration.ny_4_3.stage.5.d"] = "C'est lui. Il n'a pas de visage méchant. C'est ce qui est le plus effrayant.";
            d["narration.ny_4_3.stage.5.m"] = "Les chiffres de Paris. J'y étais. Ils n'ont pas dit tout ce qu'ils ont compté.";
            d["speaker.agent_m"] = "Agent M";
            d["narration.ny_4_4.intro.0"] = "L'esplanade de l'ONU est inondée de lumière. Des caméras de presse bordent le périmètre. Au centre, un homme seul en uniforme gris.";
            d["narration.ny_4_4.intro.1"] = "Il n'a pas ses gardes de tout à l'heure. Il a voulu qu'on le voie seul.";
            d["narration.ny_4_4.elite.before.0"] = "Le Général a demandé à vous parler. Seul à seul.";
            d["narration.ny_4_4.elite.before.1"] = "« Seul à seul » devant mille caméras.";
            d["narration.ny_4_4.elite.after.0"] = "La garde est tombée. Plus rien entre lui et nous.";
            d["narration.ny_4_4.boss.before.0"] = "Je vous félicite. Vous avez tenu votre salle, repris vos écrans, retourné quelques indics. Il ne manque que la suite logique.";
            d["narration.ny_4_4.boss.before.1"] = "Inscrivez-vous. Volontairement. Je vous garantis votre rang, votre immunité, votre autonomie. L'Aube deviendrait une section de l'agence.";
            d["narration.ny_4_4.boss.before.2"] = "Votre agence prélève ce qu'elle appelle protéger. On a vu les wagons.";
            d["narration.ny_4_4.boss.before.3"] = "Vous avez vu des wagons. J'ai vu des milliers de morts à Paris. Nous ne sommes pas d'accord sur la comptabilité.";
            d["narration.ny_4_4.boss.after.0"] = "Vous croyez avoir gagné. L'esplanade vous regarde, mais c'est à Salem que tout se règle.";
            d["narration.ny_4_4.boss.after.1"] = "Il se retire. Il n'a pas l'air vaincu.";
            d["narration.ny_4_4.boss.after.2"] = "Salem décidera.";
            d["narration.ny_4_4.chapter_end.0"] = "Les Accords de Salem sortent au grand jour. Stroke s'est retiré, mais sa promesse pèse sur la ville.";
            d["narration.ny_4_4.chapter_end.1"] = "Les négociations commencent à Salem. C'est là que tout se jouera.";
        }

        static Dictionary<string, string> ManhattanEnglish(Dictionary<string, string> d)
        {
            // --- Enemies ---
            d["enemies.mnh_guard.name"] = "Tower Guard";
            d["enemies.mnh_agent.name"] = "Private Security Agent";
            d["enemies.mnh_drone.name"] = "Propaganda Drone";
            d["enemies.mnh_chief.name"] = "Security Chief";
            d["enemies.mnh_stroke.name"] = "GENERAL STROKE";
            d["enemies.mnh_stroke.phase2"] = "Emergency Procedure";
            d["enemies.mnh_stroke.phase3"] = "Retention Protocol";
            d["skills.mnh_guard_a1"] = "Regulation Baton";
            d["skills.mnh_guard_a2"] = "Private Cordon";
            d["skills.mnh_agent_a1"] = "Warning Shot";
            d["skills.mnh_agent_a2"] = "Target Confirmed";
            d["skills.mnh_drone_a1"] = "Slogan Burst";
            d["skills.mnh_drone_a2"] = "Ad Flash";
            d["skills.mnh_chief_a1"] = "Incident Report";
            d["skills.mnh_chief_a2"] = "Forced Evacuation";
            d["skills.mnh_chief_a3"] = "Locked Perimeter";
            d["skills.mnh_stroke_a1"] = "Precision Shot";
            d["skills.mnh_stroke_a2"] = "Dug-In Position";
            d["skills.mnh_stroke_a3"] = "Suppressing Fire";
            d["skills.mnh_stroke_a4"] = "Surrender Order";
            d["skills.mnh_stroke_p2_a2"] = "Grenade Launcher";
            d["skills.mnh_stroke_p3_a3"] = "Final Barrage";
            d["archives.enemy.mnh_guard"] = "Tower Guard: the doorman of Midtown's buildings. He raises a shield and draws every blow to himself.";
            d["archives.enemy.mnh_agent"] = "Private Security Agent: a gunman on the payroll of the AMIF's and the Syndicate's backers. He confirms his target before he fires.";
            d["archives.enemy.mnh_drone"] = "Propaganda Drone: it broadcasts the AMIF's message on the screens and dazzles anyone who looks up.";
            d["archives.enemy.mnh_chief"] = "Security Chief: the elite of the circles of power. His forced evacuations rattle the entire squad.";
            d["archives.enemy.mnh_stroke"] = "GENERAL STROKE: a man, not a meta-human. He wins through gear and tactics, and at every phase he changes his equipment.";
            d["archives.enemy.amif_trooper"] = "AMIF Trooper: the agency's infantryman, who hits the whole squad when he deploys.";
            d["archives.enemy.amif_marksman"] = "AMIF Marksman: posted at the back, he marks his target before the decisive shot.";
            d["archives.enemy.amif_captain"] = "AMIF Captain: a front-line officer. He raises a shield and rattles the squad with a sweeping strike.";

            // --- Zones (Manhattan chapter) ---
            d["zones.ny_4_1.name"] = "TIMES SQUARE — THE SCREENS";
            d["zones.ny_4_1.short"] = "Times Square";
            d["zones.ny_4_1.code"] = "Zone 4.1";
            d["zones.ny_4_1.desc"] = "On every screen in Times Square, the AMIF presents the Dawn as a threat to the city. This time, no gang stands between you and them. Take back the control room and let New York see what you see.";
            d["zones.ny_4_2.name"] = "MIDTOWN TOWERS — THE CIRCLES";
            d["zones.ny_4_2.short"] = "Midtown Towers";
            d["zones.ny_4_2.code"] = "Zone 4.2";
            d["zones.ny_4_2.desc"] = "The highest floors of Midtown house the people who pay. Infiltrate their circles, get past their guards and read their donor list: some names have no business appearing twice.";
            d["zones.ny_4_3.name"] = "UN HEADQUARTERS — THE ANNOUNCEMENT";
            d["zones.ny_4_3.short"] = "UN Headquarters";
            d["zones.ny_4_3.code"] = "Zone 4.3";
            d["zones.ny_4_3.desc"] = "The Salem Accords are about to be announced before delegates from around the world. Rayner is in the hall, and so is Stroke, and they do not agree. Protect the assembly until the reading is done.";
            d["zones.ny_4_4.name"] = "UN PLAZA — THE GENERAL";
            d["zones.ny_4_4.short"] = "UN Plaza";
            d["zones.ny_4_4.code"] = "Zone 4.4";
            d["zones.ny_4_4.desc"] = "The Accords are out, but Stroke is not done. He waits alone on the plaza under the crowd's cameras, calm and convinced. Get past his guard and face him.";
            d["qg.fil.zone_ny_4_1"] = "The circles of power await in Midtown.";
            d["qg.fil.zone_ny_4_2"] = "The announcement of the Accords is being prepared at the UN.";
            d["qg.fil.zone_ny_4_3"] = "Stroke waits on the plaza.";
            d["qg.fil.zone_ny_4_4"] = "On to Salem, where the Accords will be played out.";
            d["archives.zone.zone_ny_4_1"] = "At Times Square, The Dawn takes the screen control room and broadcasts the Harlem evacuation in place of the AMIF's propaganda.";
            d["archives.zone.zone_ny_4_2"] = "In a Midtown tower, The Dawn discovers that several funders of the AMIF and the Syndicate overlap. One name points to archives from the eighties.";
            d["archives.zone.zone_ny_4_3"] = "At the UN, Rayner and Stroke clash publicly before the negotiators while The Dawn protects the broadcast of the Accords announcement.";
            d["archives.zone.zone_ny_4_4"] = "On the UN plaza, The Dawn faces General Stroke. He withdraws, promising that Salem will decide. The Accords are now public.";

            // --- Fragments ---
            d["fragment.ny_times_square.title"] = "Screens Played Backwards";
            d["fragment.ny_times_square.text"] = "Every screen told your story, but backwards. You saw the fire without the hands that carried the children. For one night, you took back the floor: you said nothing, you only showed.";
            d["fragment.ny_midtown_list.title"] = "Names on Two Lists";
            d["fragment.ny_midtown_list.text"] = "You thought the Syndicate and the AMIF were two enemies who made deals. You understood they shared a ledger. Five names came up on both lists, like a signature nobody bothers to hide anymore.";
            d["fragment.ny_un_stage.title"] = "Two Voices";
            d["fragment.ny_un_stage.text"] = "One voice spoke of protection, the other of freedom. They used almost the same words, but not for the same people. The world was listening. For the first time, no one could pretend not to hear.";
            d["fragment.ny_un_esplanade.title"] = "Salem Will Decide";
            d["fragment.ny_un_esplanade.text"] = "He didn't threaten you. He spoke of accounting, of numbers, of order. He stood alone on the plaza, calm, convinced. You understood you don't beat a man like him in one blow: you take away his stage.";

            // --- Objectives ---
            d["objectives.ny41.square"] = "Enter Times Square";
            d["objectives.ny41.screens"] = "Hack the screens";
            d["objectives.ny41.control"] = "Defeat the control room chief";
            d["objectives.ny41.broadcast"] = "Retake the broadcast";
            d["objectives.ny42.badges"] = "Infiltrate the circles of power";
            d["objectives.ny42.security"] = "Defeat the private security";
            d["objectives.ny42.roofs"] = "Cross the tower rooftops";
            d["objectives.ny42.ledger"] = "Recover the donor list";
            d["objectives.ny43.delegates"] = "Protect the delegates";
            d["objectives.ny43.captain"] = "Defeat the AMIF captain";
            d["objectives.ny43.hall"] = "Hold the assembly hall";
            d["objectives.ny43.accords"] = "Witness the Accords announcement";
            d["objectives.ny44.esplanade"] = "Reach the esplanade";
            d["objectives.ny44.evacuate"] = "Evacuate the crowd";
            d["objectives.ny44.line"] = "Break the AMIF line";
            d["objectives.ny44.stroke"] = "Defeat General Stroke";

            // --- Dialogue ---
            d["speaker.agent_d"] = "Agent D";
            d["speaker.darius_brooks"] = "Darius Brooks";
            d["speaker.agent_amif"] = "An AMIF agent";
            d["speaker.voix"] = "Propaganda Voice";
            d["speaker.narrateur"] = "Narrator";
            d["narration.ny_4_1.intro.0"] = "Times Square never sleeps. Tonight, every screen repeats the same message: The Dawn is a threat.";
            d["narration.ny_4_1.intro.1"] = "\"Unsupervised meta-humans endanger civilians. The AMIF protects you.\"";
            d["narration.ny_4_1.intro.2"] = "They've reused the Brooklyn footage. Backwards.";
            d["narration.ny_4_1.intro.3"] = "The control room is in the basement. If we reach it, we can replace the feed.";
            d["narration.ny_4_1.intro.4"] = "We don't answer them with words. We show them.";
            d["narration.ny_4_1.intro.4.m"] = "I see what these screens will show in a minute, if we let them. Let's replace it before that.";
            d["narration.ny_4_1.exit.3"] = "In Moscow, it was holograms. Here, billboards. The staging is the same.";
            d["narration.ny_4_2.intro.3.m"] = "I see who signed these names, and in what order. We're not stealing: we're reading.";
            d["narration.ny_4_2.exit.3"] = "Orlan, the Smuggler, these financiers... They're the same people, only more polite.";
            d["narration.ny_4_3.intro.3.m"] = "I see the hands that will cut the cables in ten seconds. I stop them first.";
            d["narration.ny_4_4.boss.before.2.m"] = "In Paris I saw what you harvest. Thousands dead, you say? You also kept the remains.";
            d["narration.ny_4_4.boss.phase1.0"] = "Emergency procedure. I regret it has come to this.";
            d["narration.ny_4_4.boss.phase2.0"] = "Retention protocol. Nothing personal, nothing human: only order.";
            d["narration.ny_4_4.boss.after.3.d"] = "I don't know what he's preparing in Salem. But he knows we'll be there.";
            d["narration.ny_4_4.boss.after.3.m"] = "Salem. Where Moscow and Paris meet. He knows I'll be there too.";
            d["chapter.ny_manhattan.intro.agent_d"] = "He sees the AMIF with its face uncovered for the first time: until now, it was only a Titan and lists.";
            d["chapter.ny_manhattan.intro.agent_m"] = "She has seen the AMIF harvest in Paris and its clients smile in Moscow. Here, she sees them together.";
            d["narration.ny_4_1.elite.before.0"] = "This area is under control. Please step back. You are being filmed.";
            d["narration.ny_4_1.elite.before.1"] = "He doesn't say \"forbidden.\" He says \"filmed.\" He wants it seen.";
            d["narration.ny_4_1.elite.after.0"] = "Control room reached. The feed is ours.";
            d["narration.ny_4_1.exit.0"] = "On every screen, the evacuation of the Harlem block: a child, arms, a door that holds.";
            d["narration.ny_4_1.exit.1"] = "Not the ledger, not yet. Proof released too early will be denied. Let's first show who we are.";
            d["narration.ny_4_1.exit.2"] = "That's enough for tonight. People watched.";
            d["narration.ny_4_2.intro.0"] = "The glass tower dominates Midtown. On the forty-second floor, a gala gathers foundations, boards, well-paid smiles.";
            d["narration.ny_4_2.intro.1"] = "We're the only ones without a gold badge.";
            d["narration.ny_4_2.intro.2"] = "A room of \"partners.\" The names are displayed like trophies.";
            d["narration.ny_4_2.intro.3"] = "We're not stealing anything. We're comparing.";
            d["narration.ny_4_2.elite.before.0"] = "This room is reserved. Please rejoin the lounge.";
            d["narration.ny_4_2.elite.before.1"] = "He looks polite. He's very worried.";
            d["narration.ny_4_2.elite.after.0"] = "I have the AMIF list. You have the one from the Queens ledger.";
            d["narration.ny_4_2.elite.after.1"] = "Three names. Four. Five. They're on both.";
            d["narration.ny_4_2.exit.0"] = "The people who pay the AMIF also pay the Syndicate. It isn't collusion, it's a shared budget.";
            d["narration.ny_4_2.exit.1"] = "And there: a foundation whose name rings a bell. Archives from the eighties.";
            d["narration.ny_4_2.exit.2"] = "Not tonight. But we keep the name.";
            d["narration.ny_4_3.intro.0"] = "The UN hall is full of flags, microphones and eyes. In an hour, the Salem Accords will be announced.";
            d["narration.ny_4_3.intro.1"] = "Someone is already cutting the broadcast cables.";
            d["narration.ny_4_3.intro.2"] = "Sabotage on the technical floor. If the announcement doesn't air, all of this was for nothing.";
            d["narration.ny_4_3.intro.3"] = "We hold the room. Nobody cuts this broadcast.";
            d["narration.ny_4_3.elite.before.0"] = "For the delegates' safety, the room is being evacuated. Please cooperate.";
            d["narration.ny_4_3.elite.before.1"] = "Evacuating the room is smothering the announcement. He knows it.";
            d["narration.ny_4_3.elite.after.0"] = "The room is secured. Rayner can speak.";
            d["narration.ny_4_3.exit.0"] = "The room holds its breath. For the first time, the voice of the AMIF and the voice of The Dawn clash before everyone.";
            d["narration.ny_4_3.exit.1"] = "He kept his composure. He'll keep it a while longer. He's waiting outside.";
            d["narration.ny_4_3.stage.0"] = "Apex don't ask permission to exist. They ask to be recognized, not controlled.";
            d["narration.ny_4_3.stage.1"] = "Ms. Rayner speaks of recognition. I speak of responsibility. A power that isn't measured is a power that isn't protected.";
            d["narration.ny_4_3.stage.2"] = "Measuring, harvesting, locking away: that isn't protecting. It's owning.";
            d["narration.ny_4_3.stage.3"] = "The numbers from Paris, madam, don't lie. I don't own anyone. I count.";
            d["narration.ny_4_3.stage.4"] = "Then count the ones you locked up in Queens too. There were many.";
            d["narration.ny_4_3.stage.5.d"] = "That's him. He doesn't have a villain's face. That's the scariest part.";
            d["narration.ny_4_3.stage.5.m"] = "The numbers from Paris. I was there. They didn't say everything they counted.";
            d["speaker.agent_m"] = "Agent M";
            d["narration.ny_4_4.intro.0"] = "The UN plaza is flooded with light. Press cameras line the perimeter. At the center, a lone man in a gray uniform.";
            d["narration.ny_4_4.intro.1"] = "He doesn't have the guards from earlier. He wanted to be seen alone.";
            d["narration.ny_4_4.elite.before.0"] = "The General asked to speak with you. Alone.";
            d["narration.ny_4_4.elite.before.1"] = "\"Alone,\" in front of a thousand cameras.";
            d["narration.ny_4_4.elite.after.0"] = "The guard is down. Nothing left between him and us.";
            d["narration.ny_4_4.boss.before.0"] = "I commend you. You held your room, took back your screens, turned a few informants. Only the logical next step is missing.";
            d["narration.ny_4_4.boss.before.1"] = "Register. Voluntarily. I guarantee your rank, your immunity, your autonomy. The Dawn would become a section of the agency.";
            d["narration.ny_4_4.boss.before.2"] = "Your agency harvests what it calls protecting. We saw the cars.";
            d["narration.ny_4_4.boss.before.3"] = "You saw cars. I saw thousands dead in Paris. We disagree about the accounting.";
            d["narration.ny_4_4.boss.after.0"] = "You think you've won. The plaza is watching you, but it's in Salem that everything gets settled.";
            d["narration.ny_4_4.boss.after.1"] = "He's withdrawing. He doesn't look beaten.";
            d["narration.ny_4_4.boss.after.2"] = "Salem will decide.";
            d["narration.ny_4_4.chapter_end.0"] = "The Salem Accords come out into the open. Stroke has withdrawn, but his promise weighs on the city.";
            d["narration.ny_4_4.chapter_end.1"] = "The negotiations begin in Salem. That's where everything will be played out.";
            return d;
        }
    }
}
