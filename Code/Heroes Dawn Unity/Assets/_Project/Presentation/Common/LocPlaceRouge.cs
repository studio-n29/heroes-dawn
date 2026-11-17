#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes du chapitre Place Rouge (Moscou, chapitre 4) : français (<c>ExtendPlaceRouge</c>) et anglais (<c>PlaceRougeEnglish</c>, fusionné dans <c>En</c>).</summary>
    public static partial class Loc
    {
        static partial void ExtendPlaceRouge(Dictionary<string, string> d)
        {
            // --- Ennemis ---
            d["enemies.pr_kremlin_guard.name"] = "Garde du Kremlin";
            d["enemies.pr_technician.name"] = "Technicien des salles de recherche";
            d["enemies.pr_operator.name"] = "Opérateur des forces spéciales";
            d["enemies.pr_commandant.name"] = "Commandant de la garde";
            d["enemies.pr_conseiller.name"] = "LE CONSEILLER";
            d["enemies.pr_conseiller.phase2"] = "Dernière offre";
            d["enemies.pr_conseiller.phase3"] = "Réalité nécessaire";
            d["skills.pr_kremlin_guard_a1"] = "Garde à vous";
            d["skills.pr_kremlin_guard_a2"] = "Haie d'honneur";
            d["skills.pr_technician_a1"] = "Relevé de mesures";
            d["skills.pr_technician_a2"] = "Décharge d'inventaire";
            d["skills.pr_operator_a1"] = "Contrôle d'identité";
            d["skills.pr_operator_a2"] = "Mise à l'écart";
            d["skills.pr_commandant_a1"] = "Salut réglementaire";
            d["skills.pr_commandant_a2"] = "Extinction des feux";
            d["skills.pr_commandant_a3"] = "Capote d'apparat";
            d["skills.pr_conseiller_a1"] = "Formule de politesse";
            d["skills.pr_conseiller_a2"] = "Immunité diplomatique";
            d["skills.pr_conseiller_a3"] = "Huis clos";
            d["skills.pr_conseiller_a4"] = "Avis défavorable";
            d["skills.pr_conseiller_p2_a2"] = "Proposition ferme";
            d["archives.enemy.pr_kremlin_guard"] = "Garde du Kremlin : une haie de parade devenue un mur. Il se couvre d'un bouclier et attire les coups sans jamais élever la voix.";
            d["archives.enemy.pr_technician"] = "Technicien des salles de recherche : il entretient d'anciens appareils soviétiques et sait très bien où viser. Il fragilise sa cible avant de la mesurer.";
            d["archives.enemy.pr_operator"] = "Opérateur des forces spéciales : il ferme les rues sans ordre écrit. Un contrôle d'identité courtois, puis plus rien ne bouge.";
            d["archives.enemy.pr_commandant"] = "Commandant de la garde : l'élite des couloirs du Kremlin. Quand il éteint les lampes, toute l'escouade perd ses repères.";
            d["archives.enemy.pr_conseiller"] = "LE CONSEILLER : Arkadi Zorin, haut responsable de la faction interne. Il ne hausse jamais le ton ; il propose, et son offre a toujours un prix.";

            // --- Zones (chapitre Place Rouge) ---
            d["zones.mos_4_1.name"] = "PLACE ROUGE — LE COUVRE-FEU";
            d["zones.mos_4_1.short"] = "Rues sous couvre-feu";
            d["zones.mos_4_1.code"] = "Zone M4.1";
            d["zones.mos_4_1.desc"] = "Le couvre-feu tombe sans annonce, sur un ordre que personne n'a signé. Des forces spéciales sans insigne ferment les rues autour de la place, et des civils restent bloqués dehors, derrière les barrages. Protège-les, passe le cordon et gagne la grille des tunnels.";
            d["zones.mos_4_2.name"] = "SOUS LE KREMLIN — LES TUNNELS";
            d["zones.mos_4_2.short"] = "Tunnels sous le Kremlin";
            d["zones.mos_4_2.code"] = "Zone M4.2";
            d["zones.mos_4_2.desc"] = "Sous les pavés, des tunnels de pierre humide et de câbles anciens conduisent droit au conseil. Ils traversent d'anciennes salles de recherche soviétiques, encore entretenues : planches d'anatomie, cages, dossiers « Khost ». Avance sans te faire annoncer et vois ce que l'on garde si bien.";
            d["zones.mos_4_3.name"] = "KREMLIN — L'OFFRE";
            d["zones.mos_4_3.short"] = "Salle du conseil";
            d["zones.mos_4_3.code"] = "Zone M4.3";
            d["zones.mos_4_3.desc"] = "La salle du conseil est ronde, boisée, à la lumière basse : une grande table, des chaises vides et un seul homme debout. Le Conseiller y propose à l'Aube une alliance contre l'AMIF, en échange de sa neutralité, et un envoyé de l'agence, tapi dans l'ombre, y est démasqué. Passe l'antichambre, tiens-toi à la table et écoute l'offre.";
            d["zones.mos_4_4.name"] = "KREMLIN — LE CONSEILLER";
            d["zones.mos_4_4.short"] = "Salle du conseil, le Conseiller";
            d["zones.mos_4_4.code"] = "Zone M4.4";
            d["zones.mos_4_4.desc"] = "L'Aube a refusé la neutralité, et le Conseiller regrette, sans colère. Consulte le dossier du conseil, brise sa garde personnelle et fais face à Arkadi Zorin. Dans cette salle, la politesse est une arme, et l'issue se paie d'un choix : l'arrêter ou l'écouter.";
            d["qg.fil.zone_mos_4_1"] = "Les tunnels sous le Kremlin mènent au conseil.";
            d["qg.fil.zone_mos_4_2"] = "Le conseil attend dans la salle du fond.";
            d["qg.fil.zone_mos_4_3"] = "Le Conseiller attend la réponse.";
            d["qg.fil.zone_mos_4_4"] = "Direction Salem.";
            d["archives.zone.zone_mos_4_1"] = "Sous le couvre-feu de la Place Rouge, l'Aube protège des civils bloqués et repère un envoyé de l'AMIF en civil.";
            d["archives.zone.zone_mos_4_2"] = "Sous le Kremlin, l'Aube traverse d'anciennes salles de recherche soviétiques. Les dossiers portent un nom : « Khost ». La piste mène à Salem.";
            d["archives.zone.zone_mos_4_3"] = "À la salle du conseil, le Conseiller offre à l'Aube une alliance contre l'AMIF, en échange de sa neutralité. Un envoyé de l'AMIF est démasqué.";
            d["archives.zone.zone_mos_4_4"] = "Dans la salle du conseil, l'Aube vainc le Conseiller. Le choix de l'arrêter ou de l'écouter façonne l'épilogue. Les salles soviétiques mènent à Salem.";

            // --- Objectifs ---
            d["objectives.mos41.civilians"] = "Dégager les civils bloqués";
            d["objectives.mos41.cordon"] = "Briser le cordon des forces spéciales";
            d["objectives.mos41.barrage"] = "Tenir le barrage";
            d["objectives.mos41.grille"] = "Atteindre la grille des tunnels";
            d["objectives.mos42.descent"] = "Descendre sous le Kremlin";
            d["objectives.mos42.halls"] = "Traverser les salles de recherche";
            d["objectives.mos42.warden"] = "Vaincre le gardien des tunnels";
            d["objectives.mos42.council"] = "Atteindre le conseil";
            d["objectives.mos43.antechamber"] = "Traverser l'antichambre";
            d["objectives.mos43.hearing"] = "Tenir sa place à la table";
            d["objectives.mos43.amif"] = "Repousser l'équipe de l'AMIF";
            d["objectives.mos43.offer"] = "Entendre l'offre du Conseiller";
            d["objectives.mos44.file"] = "Consulter le dossier du conseil";
            d["objectives.mos44.guard"] = "Briser la garde personnelle";
            d["objectives.mos44.conseiller"] = "Vaincre le Conseiller";

            // --- Dialogues ---
            d["speaker.conseiller"] = "Arkadi Zorin, le Conseiller";
            d["speaker.civil_moscou"] = "Un civil";
            d["speaker.agent_amif"] = "Un agent de l'AMIF";
            d["speaker.narrateur"] = "Narrateur";
            d["speaker.ilia"] = "Ilia (voix off)";
            d["chapter.mos_place_rouge.intro.agent_m"] = "Elle revient à la ville qu'elle croyait connaître, sur la place où tout a commencé pour elle.";
            d["chapter.mos_place_rouge.intro.agent_d"] = "Il arrive sur la place où un État décide de ce qu'on voit. Il a déjà vu ce que cache une vitrine.";
            d["narration.mos_4_1.intro.0"] = "Le couvre-feu tombe sans annonce. Les volets se ferment, les rues se vident, et des civils restent dehors.";
            d["narration.mos_4_1.intro.1"] = "Aucune sirène, aucun avis. Juste des patrouilles qui ne portent pas d'insigne.";
            d["narration.mos_4_1.intro.2"] = "Ils nous ont dit de rentrer. Mais la porte est de l'autre côté de leurs barrages.";
            d["narration.mos_4_1.intro.3"] = "On vous ouvre un passage. Suivez-nous, ne courez pas.";
            d["narration.mos_4_1.intro.3.d"] = "Suivez-moi. Je trace le chemin. Ne regardez pas les barrages.";
            d["narration.mos_4_1.elite.before.0"] = "Ordre de fermeture. Pas d'ordre écrit, mais un ordre.";
            d["narration.mos_4_1.elite.before.1"] = "Il ne sait pas qui l'a donné. Il obéit quand même.";
            d["narration.mos_4_1.elite.after.0"] = "Les civils sont à l'abri. Les barrages sont levés.";
            d["narration.mos_4_1.exit.0"] = "Quelqu'un nous regarde, là-bas, sous le porche. Pas de costume, pas d'insigne.";
            d["narration.mos_4_1.exit.1"] = "Il a un badge dissimulé sous son manteau. Je connais ce sigle.";
            d["narration.mos_4_1.exit.2"] = "L'AMIF. Elle était à Moscou depuis le début : elle ne s'est montrée qu'ici.";
            d["narration.mos_4_1.exit.3"] = "New York avait Stroke. Ici, l'AMIF reste dans l'ombre d'un État. C'est plus patient.";
            d["narration.mos_4_2.intro.0"] = "Sous le Kremlin, les tunnels sentent la pierre mouillée et la cire. Des câbles anciens courent le long des murs.";
            d["narration.mos_4_2.intro.1"] = "Ces tunnels datent d'avant l'Union. Quelqu'un les entretient.";
            d["narration.mos_4_2.intro.2"] = "Des salles de recherche. Des cages. Des planches d'anatomie… Des Apex, avant même que le mot existe.";
            d["narration.mos_4_2.intro.3"] = "Je vois les mains qui ont travaillé ici. Elles écrivent comme dans les marges du laboratoire.";
            d["narration.mos_4_2.intro.3.d"] = "Ces cages ont la taille d'un homme. Je préfère ne pas dessiner ce qu'il y avait dedans.";
            d["narration.mos_4_2.intro.4"] = "Cette odeur… cire, encens, pierre froide. C'est celle de la cathédrale, avant l'explosion. Je ne comprends pas.";
            d["narration.mos_4_2.elite.before.0"] = "Le conseil vous attend. Personne n'a dit qu'il vous attendait entier.";
            d["narration.mos_4_2.elite.after.0"] = "Les accès sont libres. Le conseil est au bout.";
            d["narration.mos_4_2.exit.0"] = "Un mot revient sur les dossiers : « Khost ».";
            d["narration.mos_4_2.exit.1"] = "Je ne sais pas ce que c'est, mais ces dossiers parlent d'un endroit loin d'ici. Salem.";
            d["narration.mos_4_2.exit.2"] = "Alors tout ça, c'est le début d'autre chose.";
            d["narration.mos_4_3.intro.0"] = "La salle du conseil est ronde, boisée, silencieuse. Une seule chaise est occupée.";
            d["narration.mos_4_3.intro.1"] = "Soyez les bienvenus. J'ai longtemps attendu une visite aussi précise.";
            d["narration.mos_4_3.intro.1.m"] = "Mademoiselle, je vous attendais surtout vous.";
            d["narration.mos_4_3.intro.2"] = "L'AMIF vous pourchasse. Elle prélève et revend vos semblables. Je peux la chasser de cette ville.";
            d["narration.mos_4_3.intro.3"] = "En échange de votre neutralité. Laissez Moscou régler ses affaires, et l'Aube sera libre de partir.";
            d["narration.mos_4_3.intro.4"] = "Il parle de neutralité. Il parle de liberté. Il ne parle ni de ses trophées ni de ses laboratoires.";
            d["narration.mos_4_3.intro.5"] = "La cire sur ses mains. Le même parfum. C'est lui.";
            d["narration.mos_4_3.elite.before.0"] = "Il est temps de vous retirer. Ce dossier n'est pas le vôtre.";
            d["narration.mos_4_3.elite.before.1"] = "Vous n'avez pas été invité, monsieur. Mes hommes vont vous raccompagner.";
            d["narration.mos_4_3.elite.after.0"] = "L'envoyé est tombé. L'AMIF s'est montrée, et elle a perdu.";
            d["narration.mos_4_3.exit.0"] = "Vous voyez : je tiens mes promesses. Reste la vôtre.";
            d["narration.mos_4_3.exit.1"] = "Une promesse à un homme qui se tient derrière tout ça.";
            d["narration.mos_4_4.intro.0"] = "La salle du conseil se vide. Les gardes reculent. Le Conseiller reste seul à la table.";
            d["narration.mos_4_4.elite.before.0"] = "Le Conseiller ne vous ferait pas de mal. Pas lui. Il délègue.";
            d["narration.mos_4_4.elite.after.0"] = "Ses derniers hommes sont tombés. Il n'a plus que ses mots.";
            d["narration.mos_4_4.boss.before.0"] = "Vous refusez. C'est un choix, et je le respecte. Je ne suis pas un homme qui insiste.";
            d["narration.mos_4_4.boss.before.1"] = "Moscou n'a pas besoin de héros. Elle a besoin d'ordre. Vous êtes un accident qui ne sait pas qu'il en est un.";
            d["narration.mos_4_4.boss.before.2"] = "Un accident ? Les gens que vous avez vendus n'étaient pas un accident.";
            d["narration.mos_4_4.boss.before.3"] = "Ils étaient une ressource. La ville avait faim.";
            d["narration.mos_4_4.boss.phase1.0"] = "Dernière offre. Elle n'est plus négociable.";
            d["narration.mos_4_4.boss.phase2.0"] = "Réalité nécessaire. Je regrette qu'on en arrive là.";
            d["narration.mos_4_4.boss.after.0"] = "Vous avez gagné. Je demande seulement quelques mots avant que vous décidiez de moi.";
            d["narration.mos_4_4.boss.after.1.m"] = "Vous voulez savoir pourquoi la cathédrale ? Il fallait un choc. Un vrai. J'avais besoin d'un Apex qui apprendrait à voir. Votre frère devait être le prix.";
            d["narration.mos_4_4.boss.after.1.d"] = "Vous êtes venu de loin pour comprendre un système. J'aurais pu vous expliquer, mais personne ne veut jamais entendre.";
            d["narration.mos_4_4.boss.after.2"] = "Ilia… Vous l'avez blessé pour m'éveiller.";
            d["narration.mos_4_4.boss.after.3"] = "Alors cette nuit-là n'était pas un hasard. Vous l'aviez prévue.";
            d["narration.mos_4_4.boss.after.4"] = "Mara… je suis là. Je suis toujours là.";
            d["narration.mos_4_4.boss.after.5"] = "Mes hommes marquent leurs caisses à la cire des cierges. Une vieille habitude d'église. Elle m'a trahi plus d'une fois.";
            d["narration.mos_4_4.choice.0"] = "Tu peux l'arrêter, ou l'écouter. Quoi que tu choisisses, Moscou en gardera le souvenir.";
            d["narration.mos_4_4.choice.1"] = "Choix : arrêter le Conseiller, ou l'écouter.";
            d["narration.mos_4_4.branch_arrest.0"] = "Assez. Vous répondrez de vos actes devant ceux que vous avez trahis.";
            d["narration.mos_4_4.branch_arrest.1"] = "Je répondrai. Mais ils ne sauront jamais tout.";
            d["narration.mos_4_4.branch_listen.0"] = "Dites. Je vous écoute.";
            d["narration.mos_4_4.branch_listen.1"] = "L'AMIF a un projet, mademoiselle. À Salem, quelque chose se prépare. Mes recherches y mènent. Je n'en suis pas l'auteur.";
            d["narration.mos_4_4.chapter_end.0"] = "Le Conseiller est vaincu. Les salles soviétiques s'ouvrent vers un passage ancien : il mène à Salem.";
            d["narration.mos_4.epilogue.arrest.0"] = "Au matin, le Conseiller est remis aux autorités. Le cercle du pouvoir se fige, puis se retourne contre l'Aube.";
            d["narration.mos_4.epilogue.arrest.1"] = "Nous avons fait ce qui devait l'être. Moscou ne nous le pardonnera pas de sitôt.";
            d["narration.mos_4.epilogue.listen.0"] = "Au matin, le Conseiller disparaît, laissant derrière lui des noms et une piste. L'Aube est libre, mais elle lui doit quelque chose.";
            d["narration.mos_4.epilogue.listen.1"] = "Nous savons enfin ce qui se joue à Salem. Reste à savoir ce que ça nous coûtera.";
            d["narration.mos_4.epilogue.common.0"] = "Un passage sous les salles soviétiques. Il mène plus loin que Moscou.";
            d["narration.mos_4.epilogue.common.1"] = "Salem. C'est là que tout se rejoint.";

            // --- Fragments ---
            d["fragment.mos_curfew.title"] = "Sans ordre officiel";
            d["fragment.mos_curfew.text"] = "Les rues se fermaient sans ordre, et personne n'osait demander qui les fermait. Tu as vu un homme sous un porche, un badge caché sous un manteau. Moscou avait appris à obéir à des ordres qui n'existent pas.";
            d["fragment.mos_kremlin_tunnels.title"] = "Sous le Kremlin";
            d["fragment.mos_kremlin_tunnels.text"] = "Les tunnels sentaient la cire, comme une église enterrée. Dans les salles de recherche, les mots en marge étaient les mêmes que ceux du laboratoire. Tu as compris que l'histoire de Moscou ne finissait pas à ses murs.";
            d["fragment.mos_council_offer.title"] = "L'offre";
            d["fragment.mos_council_offer.text"] = "Il t'a offert la fin de l'AMIF en échange de ton silence. Il n'a pas élevé la voix. Tu as compris qu'on ne ment pas toujours : parfois, on propose seulement ce qui arrange.";
            d["fragment.mos_place_rouge_choice.title"] = "L'homme de l'ordre";
            d["fragment.mos_place_rouge_choice.text"] = "Il ne criait pas. Il parlait d'ordre, de ressources, de faim. Quand tu as compris que ta vie avait été un moyen, tu as eu le choix : le livrer ou l'entendre. Les deux se paient.";
        }

        static Dictionary<string, string> PlaceRougeEnglish(Dictionary<string, string> d)
        {
            // --- Enemies ---
            d["enemies.pr_kremlin_guard.name"] = "Kremlin Guard";
            d["enemies.pr_technician.name"] = "Research Hall Technician";
            d["enemies.pr_operator.name"] = "Special Forces Operator";
            d["enemies.pr_commandant.name"] = "Guard Commander";
            d["enemies.pr_conseiller.name"] = "THE COUNCILLOR";
            d["enemies.pr_conseiller.phase2"] = "Final Offer";
            d["enemies.pr_conseiller.phase3"] = "Necessary Reality";
            d["skills.pr_kremlin_guard_a1"] = "Attention";
            d["skills.pr_kremlin_guard_a2"] = "Guard of Honor";
            d["skills.pr_technician_a1"] = "Readout";
            d["skills.pr_technician_a2"] = "Inventory Discharge";
            d["skills.pr_operator_a1"] = "ID Check";
            d["skills.pr_operator_a2"] = "Sidelined";
            d["skills.pr_commandant_a1"] = "Regulation Salute";
            d["skills.pr_commandant_a2"] = "Lights Out";
            d["skills.pr_commandant_a3"] = "Dress Greatcoat";
            d["skills.pr_conseiller_a1"] = "Courteous Closing";
            d["skills.pr_conseiller_a2"] = "Diplomatic Immunity";
            d["skills.pr_conseiller_a3"] = "Closed Session";
            d["skills.pr_conseiller_a4"] = "Unfavorable Opinion";
            d["skills.pr_conseiller_p2_a2"] = "Firm Proposal";
            d["archives.enemy.pr_kremlin_guard"] = "Kremlin guard: a parade line turned into a wall. He raises a shield and draws the blows without ever raising his voice.";
            d["archives.enemy.pr_technician"] = "Research hall technician: he maintains old Soviet equipment and knows exactly where to aim. He weakens his target before taking its measure.";
            d["archives.enemy.pr_operator"] = "Special forces operator: he closes streets without a written order. A courteous ID check, then nothing moves.";
            d["archives.enemy.pr_commandant"] = "Guard commander: the elite of the Kremlin corridors. When he puts out the lamps, the whole squad loses its bearings.";
            d["archives.enemy.pr_conseiller"] = "THE COUNCILLOR: Arkadi Zorin, senior official of the inner faction. He never raises his voice; he proposes, and his offer always has a price.";

            // --- Zones (Red Square chapter) ---
            d["zones.mos_4_1.name"] = "RED SQUARE — THE CURFEW";
            d["zones.mos_4_1.short"] = "Streets under curfew";
            d["zones.mos_4_1.code"] = "Zone M4.1";
            d["zones.mos_4_1.desc"] = "The curfew falls without warning, on an order nobody signed. Special forces wearing no insignia are closing the streets around the square, and civilians are stuck outside, behind the roadblocks. Protect them, break through the cordon and reach the tunnel gate.";
            d["zones.mos_4_2.name"] = "BENEATH THE KREMLIN — THE TUNNELS";
            d["zones.mos_4_2.short"] = "Tunnels beneath the Kremlin";
            d["zones.mos_4_2.code"] = "Zone M4.2";
            d["zones.mos_4_2.desc"] = "Beneath the cobblestones, tunnels of wet stone and old cables lead straight to the council. They cross old Soviet research halls, still maintained: anatomy charts, cages, \"Khost\" files. Move without being announced and see what is so carefully kept.";
            d["zones.mos_4_3.name"] = "KREMLIN — THE OFFER";
            d["zones.mos_4_3.short"] = "Council Chamber";
            d["zones.mos_4_3.code"] = "Zone M4.3";
            d["zones.mos_4_3.desc"] = "The council chamber is round, paneled, dimly lit: a great table, empty chairs and a single man standing. There the Councillor offers The Dawn an alliance against the AMIF in exchange for its neutrality, and an agency envoy, lurking in the shadows, is unmasked. Cross the antechamber, hold your seat at the table and hear the offer.";
            d["zones.mos_4_4.name"] = "KREMLIN — THE COUNCILLOR";
            d["zones.mos_4_4.short"] = "Council chamber, the Councillor";
            d["zones.mos_4_4.code"] = "Zone M4.4";
            d["zones.mos_4_4.desc"] = "The Dawn has refused neutrality, and the Councillor regrets it, without anger. Read the council's file, break his personal guard and face Arkadi Zorin. In this room, politeness is a weapon, and the outcome costs a choice: arrest him, or hear him out.";
            d["qg.fil.zone_mos_4_1"] = "The tunnels beneath the Kremlin lead to the council.";
            d["qg.fil.zone_mos_4_2"] = "The council waits in the back chamber.";
            d["qg.fil.zone_mos_4_3"] = "The Councillor awaits an answer.";
            d["qg.fil.zone_mos_4_4"] = "On to Salem.";
            d["archives.zone.zone_mos_4_1"] = "Under the Red Square curfew, The Dawn protects stranded civilians and spots an AMIF envoy in plain clothes.";
            d["archives.zone.zone_mos_4_2"] = "Beneath the Kremlin, The Dawn passes through old Soviet research rooms. The files carry one name: \"Khost.\" The trail leads to Salem.";
            d["archives.zone.zone_mos_4_3"] = "In the council chamber, the Councillor offers The Dawn an alliance against the AMIF in exchange for its neutrality. An AMIF envoy is unmasked.";
            d["archives.zone.zone_mos_4_4"] = "In the council chamber, The Dawn defeats the Councillor. The choice to arrest or hear him shapes the epilogue. The Soviet chambers lead to Salem.";

            // --- Objectives ---
            d["objectives.mos41.civilians"] = "Free the trapped civilians";
            d["objectives.mos41.cordon"] = "Break the special forces cordon";
            d["objectives.mos41.barrage"] = "Hold the barricade";
            d["objectives.mos41.grille"] = "Reach the tunnel gate";
            d["objectives.mos42.descent"] = "Go down beneath the Kremlin";
            d["objectives.mos42.halls"] = "Cross the research halls";
            d["objectives.mos42.warden"] = "Defeat the tunnel warden";
            d["objectives.mos42.council"] = "Reach the council";
            d["objectives.mos43.antechamber"] = "Cross the antechamber";
            d["objectives.mos43.hearing"] = "Hold your seat at the table";
            d["objectives.mos43.amif"] = "Repel the AMIF team";
            d["objectives.mos43.offer"] = "Hear the Councillor's offer";
            d["objectives.mos44.file"] = "Read the council's file";
            d["objectives.mos44.guard"] = "Break the personal guard";
            d["objectives.mos44.conseiller"] = "Defeat the Councillor";

            // --- Dialogue ---
            d["speaker.conseiller"] = "Arkadi Zorin, the Councillor";
            d["speaker.civil_moscou"] = "A Civilian";
            d["speaker.agent_amif"] = "An AMIF Agent";
            d["speaker.ilia"] = "Ilia (voice-over)";
            d["chapter.mos_place_rouge.intro.agent_m"] = "She returns to the city she thought she knew, on the square where it all began for her.";
            d["chapter.mos_place_rouge.intro.agent_d"] = "He arrives on the square where a state decides what you get to see. He has already seen what a shop window hides.";
            d["narration.mos_4_1.intro.0"] = "The curfew falls without warning. Shutters close, streets empty, and civilians are left outside.";
            d["narration.mos_4_1.intro.1"] = "No siren, no notice. Just patrols wearing no insignia.";
            d["narration.mos_4_1.intro.2"] = "They told us to go home. But our door is on the other side of their roadblocks.";
            d["narration.mos_4_1.intro.3"] = "We'll open a path for you. Follow us, don't run.";
            d["narration.mos_4_1.intro.3.d"] = "Follow me. I'll draw the way. Don't look at the roadblocks.";
            d["narration.mos_4_1.elite.before.0"] = "Closure order. No written order, but an order.";
            d["narration.mos_4_1.elite.before.1"] = "He doesn't know who gave it. He obeys anyway.";
            d["narration.mos_4_1.elite.after.0"] = "The civilians are safe. The roadblocks are lifted.";
            d["narration.mos_4_1.exit.0"] = "Someone's watching us, over there, under the archway. No suit, no insignia.";
            d["narration.mos_4_1.exit.1"] = "He has a badge hidden under his coat. I know that logo.";
            d["narration.mos_4_1.exit.2"] = "The AMIF. It's been in Moscow from the start: it only showed itself here.";
            d["narration.mos_4_1.exit.3"] = "New York had Stroke. Here, the AMIF stays in the shadow of a state. It's more patient.";
            d["narration.mos_4_2.intro.0"] = "Beneath the Kremlin, the tunnels smell of wet stone and wax. Old cables run along the walls.";
            d["narration.mos_4_2.intro.1"] = "These tunnels date from before the Union. Someone maintains them.";
            d["narration.mos_4_2.intro.2"] = "Research rooms. Cages. Anatomy charts... Apex, before the word even existed.";
            d["narration.mos_4_2.intro.3"] = "I see the hands that worked here. They write the way they do in the laboratory margins.";
            d["narration.mos_4_2.intro.3.d"] = "These cages are man-sized. I'd rather not draw what was inside.";
            d["narration.mos_4_2.intro.4"] = "That smell... wax, incense, cold stone. It's the cathedral's, before the explosion. I don't understand.";
            d["narration.mos_4_2.elite.before.0"] = "The council is expecting you. Nobody said it was expecting you in one piece.";
            d["narration.mos_4_2.elite.after.0"] = "The passages are clear. The council is at the end.";
            d["narration.mos_4_2.exit.0"] = "One word keeps coming up in the files: \"Khost.\"";
            d["narration.mos_4_2.exit.1"] = "I don't know what it is, but these files talk about a place far from here. Salem.";
            d["narration.mos_4_2.exit.2"] = "Then all of this is the beginning of something else.";
            d["narration.mos_4_3.intro.0"] = "The council chamber is round, paneled, silent. Only one chair is occupied.";
            d["narration.mos_4_3.intro.1"] = "Welcome. I have long awaited a visit this precise.";
            d["narration.mos_4_3.intro.1.m"] = "Miss, it was mostly you I was waiting for.";
            d["narration.mos_4_3.intro.2"] = "The AMIF hunts you. It harvests and resells your kind. I can drive it out of this city.";
            d["narration.mos_4_3.intro.3"] = "In exchange for your neutrality. Let Moscow settle its own affairs, and The Dawn will be free to leave.";
            d["narration.mos_4_3.intro.4"] = "He talks of neutrality. He talks of freedom. He says nothing of his trophies or his laboratories.";
            d["narration.mos_4_3.intro.5"] = "Wax on his hands. The same scent. It's him.";
            d["narration.mos_4_3.elite.before.0"] = "It's time you withdrew. This file is not yours.";
            d["narration.mos_4_3.elite.before.1"] = "You weren't invited, sir. My men will see you out.";
            d["narration.mos_4_3.elite.after.0"] = "The envoy has fallen. The AMIF showed itself, and it lost.";
            d["narration.mos_4_3.exit.0"] = "You see: I keep my promises. Yours remains.";
            d["narration.mos_4_3.exit.1"] = "A promise to a man who stands behind all of this.";
            d["narration.mos_4_4.intro.0"] = "The council chamber empties. The guards fall back. The Councillor remains alone at the table.";
            d["narration.mos_4_4.elite.before.0"] = "The Councillor wouldn't harm you. Not him. He delegates.";
            d["narration.mos_4_4.elite.after.0"] = "His last men have fallen. He has only his words left.";
            d["narration.mos_4_4.boss.before.0"] = "You refuse. That is a choice, and I respect it. I am not a man who insists.";
            d["narration.mos_4_4.boss.before.1"] = "Moscow doesn't need heroes. It needs order. You are an accident that doesn't know it is one.";
            d["narration.mos_4_4.boss.before.2"] = "An accident? The people you sold were not an accident.";
            d["narration.mos_4_4.boss.before.3"] = "They were a resource. The city was hungry.";
            d["narration.mos_4_4.boss.phase1.0"] = "Final offer. It is no longer negotiable.";
            d["narration.mos_4_4.boss.phase2.0"] = "Necessary reality. I regret it has come to this.";
            d["narration.mos_4_4.boss.after.0"] = "You have won. I ask only a few words before you decide about me.";
            d["narration.mos_4_4.boss.after.1.m"] = "You want to know why the cathedral? A shock was needed. A real one. I needed an Apex who would learn to see. Your brother was to be the price.";
            d["narration.mos_4_4.boss.after.1.d"] = "You came a long way to understand a system. I could have explained it, but nobody ever wants to hear.";
            d["narration.mos_4_4.boss.after.2"] = "Ilia... You hurt him to awaken me.";
            d["narration.mos_4_4.boss.after.3"] = "So that night was no accident. You had planned it.";
            d["narration.mos_4_4.boss.after.4"] = "Mara... I'm here. I'm still here.";
            d["narration.mos_4_4.boss.after.5"] = "My men mark their crates with candle wax. An old church habit. It has betrayed me more than once.";
            d["narration.mos_4_4.choice.0"] = "You can arrest him, or hear him out. Whatever you choose, Moscow will remember it.";
            d["narration.mos_4_4.choice.1"] = "Choice: arrest the Councillor, or hear him out.";
            d["narration.mos_4_4.branch_arrest.0"] = "Enough. You will answer for your acts before those you betrayed.";
            d["narration.mos_4_4.branch_arrest.1"] = "I will answer. But they will never know everything.";
            d["narration.mos_4_4.branch_listen.0"] = "Speak. I'm listening.";
            d["narration.mos_4_4.branch_listen.1"] = "The AMIF has a project, miss. Something is being prepared in Salem. My research leads there. I am not its author.";
            d["narration.mos_4_4.chapter_end.0"] = "The Councillor is defeated. The Soviet chambers open onto an ancient passage: it leads to Salem.";
            d["narration.mos_4.epilogue.arrest.0"] = "By morning, the Councillor is handed to the authorities. The circle of power freezes, then turns against The Dawn.";
            d["narration.mos_4.epilogue.arrest.1"] = "We did what had to be done. Moscow won't forgive us any time soon.";
            d["narration.mos_4.epilogue.listen.0"] = "By morning, the Councillor disappears, leaving behind names and a trail. The Dawn is free, but it owes him something.";
            d["narration.mos_4.epilogue.listen.1"] = "We finally know what's at stake in Salem. We just don't know what it will cost us.";
            d["narration.mos_4.epilogue.common.0"] = "A passage beneath the Soviet chambers. It leads farther than Moscow.";
            d["narration.mos_4.epilogue.common.1"] = "Salem. That's where everything comes together.";

            // --- Fragments ---
            d["fragment.mos_curfew.title"] = "No Official Order";
            d["fragment.mos_curfew.text"] = "The streets were closing without any order, and no one dared ask who was closing them. You saw a man under an archway, a badge hidden under his coat. Moscow had learned to obey orders that don't exist.";
            d["fragment.mos_kremlin_tunnels.title"] = "Beneath the Kremlin";
            d["fragment.mos_kremlin_tunnels.text"] = "The tunnels smelled of wax, like a buried church. In the research rooms, the margin notes were the same as in the laboratory. You understood that Moscow's story didn't end at its walls.";
            d["fragment.mos_council_offer.title"] = "The Offer";
            d["fragment.mos_council_offer.text"] = "He offered you the end of the AMIF in exchange for your silence. He didn't raise his voice. You understood that lies aren't always told: sometimes one merely proposes what is convenient.";
            d["fragment.mos_place_rouge_choice.title"] = "The Man of Order";
            d["fragment.mos_place_rouge_choice.text"] = "He never shouted. He spoke of order, of resources, of hunger. When you understood that your life had been a means, you were given a choice: hand him over, or hear him out. Both have a price.";
            return d;
        }
    }
}
