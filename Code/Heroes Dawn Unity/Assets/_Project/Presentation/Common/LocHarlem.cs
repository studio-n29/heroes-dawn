#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes du chapitre Harlem : français (<c>ExtendHarlem</c>) et anglais (<c>HarlemEnglish</c>, fusionné dans <c>En</c>).</summary>
    public static partial class Loc
    {
        static partial void ExtendHarlem(Dictionary<string, string> d)
        {
            // --- Ennemis ---
            d["enemies.hrl_gangster.name"] = "Homme de main du gang";
            d["enemies.hrl_torch.name"] = "Incendiaire";
            d["enemies.hrl_roofer.name"] = "Tireur de toit";
            d["enemies.hrl_lieutenant.name"] = "Lieutenant de Braise";
            d["enemies.hrl_braise.name"] = "BRAISE";
            d["enemies.hrl_braise.phase2"] = "Ça brûle";
            d["enemies.hrl_braise.phase3"] = "Tout le bloc";
            d["skills.hrl_gangster_a1"] = "Barre de fer";
            d["skills.hrl_gangster_a2"] = "Mur de blousons";
            d["skills.hrl_torch_a1"] = "Cocktail maison";
            d["skills.hrl_torch_a2"] = "Pluie d'étincelles";
            d["skills.hrl_roofer_a1"] = "Tir depuis le toit";
            d["skills.hrl_roofer_a2"] = "Visée sur la fenêtre";
            d["skills.hrl_lieutenant_a1"] = "Allumette";
            d["skills.hrl_lieutenant_a2"] = "Rue en flammes";
            d["skills.hrl_lieutenant_a3"] = "Veste ignifugée";
            d["skills.hrl_braise_a1"] = "Poing brûlant";
            d["skills.hrl_braise_a2"] = "Cercle de feu";
            d["skills.hrl_braise_a3"] = "Loyer impayé";
            d["skills.hrl_braise_a4"] = "Désigné coupable";
            d["skills.hrl_braise_p2_a2"] = "Flamme de rancune";
            d["archives.enemy.hrl_gangster"] = "Homme de main du gang : il barre l'escalier et la rue, et fait bloc avec ses camarades. Il vit du quartier qu'il rançonne.";
            d["archives.enemy.hrl_torch"] = "Incendiaire : il met le feu aux immeubles sur commande pour vider les logements. Ses flammes brûlent, sans jamais s'éterniser.";
            d["archives.enemy.hrl_roofer"] = "Tireur de toit : il surveille les ruelles depuis les terrasses et marque ceux qui s'attardent.";
            d["archives.enemy.hrl_lieutenant"] = "Lieutenant de Braise : il porte les ordres du chef de rue en rue. Son passage laisse des cendres sur toute l'escouade.";
            d["archives.enemy.hrl_braise"] = "BRAISE : Dante Reyes, chef du gang de Harlem. Il brûle des blocs entiers pour qu'on les rachète moins cher, et se dit simple intermédiaire.";

            // --- Zones ---
            d["zones.ny_3_1.name"] = "BLOC INCENDIÉ — LE FEU";
            d["zones.ny_3_1.short"] = "Bloc incendié";
            d["zones.ny_3_1.code"] = "Zone 3.1";
            d["zones.ny_3_1.desc"] = "Un immeuble de Harlem brûle, et les voisins disent que personne n'a laissé de cigarette. Des familles sont encore piégées dans les étages. Fais-les sortir avant que le bloc ne s'effondre, et regarde qui a barricadé l'escalier.";
            d["zones.ny_3_2.name"] = "CLINIQUE DE FORTUNE — TENIR LA PORTE";
            d["zones.ny_3_2.short"] = "Clinique de fortune";
            d["zones.ny_3_2.code"] = "Zone 3.2";
            d["zones.ny_3_2.desc"] = "Dans une ancienne église, le Dr Amina Colby soigne ceux que l'hôpital ne reçoit plus. Le gang veut la fermer, elle refuse de partir. Rassemble les soins et tiens la porte.";
            d["zones.ny_3_3.name"] = "MARCHÉ DE RUE — DEUX FEUX";
            d["zones.ny_3_3.short"] = "Marché de rue";
            d["zones.ny_3_3.code"] = "Zone 3.3";
            d["zones.ny_3_3.desc"] = "Les étals du marché et la clinique sont attaqués en même temps, et l'escouade ne peut pas être aux deux endroits. Choisis ce que tu défends en premier : l'autre site en paiera le prix. Il n'y a pas de bon choix, seulement un choix.";
            d["zones.ny_3_4.name"] = "RUE DE L'ATELIER — BRAISE";
            d["zones.ny_3_4.short"] = "Rue de l'atelier";
            d["zones.ny_3_4.code"] = "Zone 3.4";
            d["zones.ny_3_4.desc"] = "Le gang de Harlem se terre dans la rue de l'atelier, sur un trottoir que tout le quartier connaît et que personne ne traverse sans baisser les yeux. Brise sa garde et fais face à Braise.";
            d["qg.fil.zone_ny_3_1"] = "Le gang vide les blocs de Harlem. La clinique est la prochaine cible.";
            d["qg.fil.zone_ny_3_2"] = "La clinique tient. Le marché est le prochain point faible.";
            d["qg.fil.zone_ny_3_3"] = "Le chef du gang attend dans la rue de l'atelier.";
            d["qg.fil.zone_ny_3_4"] = "Direction Manhattan, où tout converge.";
            d["archives.zone.zone_ny_3_1"] = "À Harlem, un bloc incendié volontairement par un gang a été évacué à temps. Les habitants avaient refusé une offre de rachat.";
            d["archives.zone.zone_ny_3_2"] = "À Harlem, une clinique de fortune installée dans une ancienne église soigne tout le quartier. Le Dr Colby refuse d'évacuer, malgré l'attaque du gang.";
            d["archives.zone.zone_ny_3_3"] = "À Harlem, l'Aube défend la clinique ou le marché en premier, pendant que le gang attaque les deux. Chaque choix laisse une perte visible.";
            d["archives.zone.zone_ny_3_4"] = "À Harlem, l'Aube met fin au règne du gang de Braise dans la rue de l'atelier. Le quartier garde les traces du prix payé.";

            // --- Objectifs ---
            d["objectives.ny31.fire"] = "Atteindre l'immeuble en flammes";
            d["objectives.ny31.evacuate"] = "Évacuer les familles piégées";
            d["objectives.ny31.arsonist"] = "Vaincre le lieutenant incendiaire";
            d["objectives.ny31.roofs"] = "Ramener les derniers habitants des toits";
            d["objectives.ny32.clinic"] = "Entrer dans la clinique";
            d["objectives.ny32.supplies"] = "Rassembler les soins";
            d["objectives.ny32.assault"] = "Repousser l'assaut du gang";
            d["objectives.ny32.hold"] = "Tenir la porte jusqu'au bout";
            d["objectives.ny33.market"] = "Atteindre le marché";
            d["objectives.ny33.stalls"] = "Mettre les étals à l'abri";
            d["objectives.ny33.raiders"] = "Briser les pillards";
            d["objectives.ny33.priority"] = "Choisir quoi défendre en premier";
            d["objectives.ny34.chase"] = "Rattraper les guetteurs sur les toits";
            d["objectives.ny34.sidewalk"] = "Prendre le trottoir";
            d["objectives.ny34.workshop"] = "Forcer l'atelier";
            d["objectives.ny34.braise"] = "Vaincre Braise";

            // --- Dialogues ---
            d["speaker.narrateur"] = "Narrateur";
            d["speaker.habitant"] = "Un habitant";
            d["speaker.commercant"] = "Un commerçant";
            d["speaker.dr_colby"] = "Dr Amina Colby";
            d["speaker.braise"] = "Dante Reyes, « Braise »";
            d["narration.ny_3_1.intro.0"] = "Le blackout a laissé les immeubles de Harlem sans courant, sans pression, sans pompiers. Celui-ci brûle depuis une heure.";
            d["narration.ny_3_1.intro.1"] = "Il reste des gens à tous les étages. J'entends des enfants.";
            d["narration.ny_3_1.intro.2"] = "Mon voisin dit que c'était un court-circuit. Un court-circuit qui sent l'essence ?";
            d["narration.ny_3_1.intro.3"] = "On ouvre l'escalier de service. Je vous fais une passerelle. Tenez la rampe et ne regardez pas en bas.";
            d["narration.ny_3_1.intro.3.m"] = "Je vois quel étage cédera en premier. On évacue dans cet ordre.";
            d["narration.ny_3_1.intro.4"] = "Le plafond du troisième ne tiendra pas. On a quatre minutes.";
            d["narration.ny_3_1.elite.before.0"] = "Personne ne sort. Les locataires ont reçu une offre, ils vont l'accepter.";
            d["narration.ny_3_1.elite.before.1"] = "Il parle d'offre pendant que le bloc brûle. Il n'a même pas honte.";
            d["narration.ny_3_1.elite.after.0"] = "Des bidons, dans la cave. Quelqu'un a préparé ça.";
            d["narration.ny_3_1.exit.0"] = "Des hommes sont venus hier pour nous proposer de racheter l'immeuble. Nous avons dit non.";
            d["narration.ny_3_1.exit.1"] = "Ils ont changé la réponse eux-mêmes.";
            d["narration.ny_3_1.exit.2"] = "Je suis né à trois rues d'ici. J'ai dessiné sur ce mur quand j'avais quinze ans. Il n'en reste plus rien.";
            d["narration.ny_3_2.intro.0"] = "La clinique occupe une ancienne église. Un groupe électrogène tousse dans la cour, et une file de blessés attend à la porte.";
            d["narration.ny_3_2.intro.1"] = "Il n'y a ni badge ni formulaire. Ils entrent, c'est tout.";
            d["narration.ny_3_2.intro.2"] = "Évacuer ? Les hôpitaux du centre n'admettent personne de ce quartier. Ils ne soignent qu'ici. Si je pars, ils n'ont nulle part.";
            d["narration.ny_3_2.intro.3"] = "Elle ne cherche pas à être courageuse. Elle a fait ses comptes.";
            d["narration.ny_3_2.intro.4"] = "Alors on tient la clinique. Dites-nous seulement quelle porte ne doit jamais tomber.";
            d["narration.ny_3_2.intro.4.m"] = "Je vois par où ils vont entrer. Dites-moi seulement quelle porte vous voulez garder.";
            d["narration.ny_3_2.elite.before.0"] = "On vide le bâtiment. Ça sera plus simple pour tout le monde.";
            d["narration.ny_3_2.elite.before.1"] = "Plus simple pour qui ? Pas pour les gens couchés derrière moi.";
            d["narration.ny_3_2.elite.after.0"] = "L'entrée tient. Les patients sont à l'abri.";
            d["narration.ny_3_2.exit.0"] = "Je ne vais pas vous remercier. J'ai déjà trop remercié des gens qui n'ont pas pu rester.";
            d["narration.ny_3_2.exit.1"] = "Restez, si vous voulez. Le gang n'a pas fini.";
            d["narration.ny_3_3.intro.0"] = "Les vendeurs du marché montent leurs bâches avant le jour. À deux rues, la clinique s'allume.";
            d["narration.ny_3_3.intro.1"] = "Ici, tout le monde se connaît. Si le marché tombe, les gens n'ont plus rien à manger.";
            d["narration.ny_3_3.intro.2"] = "Deux alertes en même temps. Le gang attaque la clinique et le marché.";
            d["narration.ny_3_3.intro.3"] = "On ne peut pas couvrir les deux. Pas assez de monde.";
            d["narration.ny_3_3.choice.0"] = "C'est ton choix. Quoi que tu décides, l'autre côté va en payer le prix.";
            d["narration.ny_3_3.choice.1"] = "Choix : défendre la clinique en premier, ou le marché en premier.";
            d["narration.ny_3_3.branch_clinic.0"] = "La clinique d'abord. Des gens y sont couchés qui ne peuvent pas courir.";
            d["narration.ny_3_3.branch_clinic.0.m"] = "La clinique d'abord. Je vois les lits, les perfusions : ils ne pourraient pas fuir.";
            d["narration.ny_3_3.branch_clinic.1"] = "Le marché… Vous nous laissez ?";
            d["narration.ny_3_3.branch_clinic.2"] = "Je reviens. Cachez ce que vous pouvez.";
            d["narration.ny_3_3.branch_market.0"] = "Le marché d'abord. Des familles y vivent de ce qu'elles vendent aujourd'hui.";
            d["narration.ny_3_3.branch_market.0.m"] = "Le marché d'abord. Je vois les familles qui y comptent leurs derniers dollars.";
            d["narration.ny_3_3.branch_market.1"] = "La clinique ? Vous nous laissez ?";
            d["narration.ny_3_3.branch_market.2"] = "Tenez la porte. Je reviens.";
            d["narration.ny_3_3.exit.0"] = "On a tenu un côté. Pour l'autre, on arrive après.";
            d["narration.ny_3_3.exit.1"] = "On gagne rarement sans rien perdre. Retiens ce que ça coûte.";
            d["narration.ny_3_4.intro.0"] = "La rue de l'atelier est étroite, les volets fermés. Des tags couvrent les murs : la signature du gang.";
            d["narration.ny_3_4.intro.1"] = "Il n'y a personne dans cette rue, mais les fenêtres nous regardent.";
            d["narration.ny_3_4.intro.2"] = "Ils nous attendent. Chaque porte est tenue.";
            d["narration.ny_3_4.elite.before.0"] = "Braise a dit : personne ne passe. Il a aussi dit que vous essaieriez.";
            d["narration.ny_3_4.elite.after.0"] = "La rue est ouverte. Il est au bout, sur le trottoir.";
            d["narration.ny_3_4.boss.before.0"] = "Vous voilà. Je vous attendais plus tôt, mais le feu prend toujours son temps.";
            d["narration.ny_3_4.boss.before.1"] = "Le blackout a vidé la ville de ses règles. Moi, j'ai juste ramassé ce qui traînait.";
            d["narration.ny_3_4.boss.before.2"] = "Le Régisseur payait bien, avant que vous lui dérangiez son contrat. Un bloc libre vaut plus qu'un bloc habité.";
            d["narration.ny_3_4.boss.before.3"] = "Il ne s'excuse pas. Il explique, comme si c'était une évidence.";
            d["narration.ny_3_4.boss.before.4.d"] = "C'est ici. C'est ici que Théo est tombé. J'ai tracé la barrière à la craie, juste là. Elle a tenu une seconde.";
            d["narration.ny_3_4.boss.before.4.m"] = "C'est ici que mon frère a été abattu. Il vendait du pain. Il n'avait jamais rien fait.";
            d["narration.ny_3_4.boss.phase1.0"] = "Ça brûle. C'est le mot que j'ai appris.";
            d["narration.ny_3_4.boss.phase2.0"] = "Tout le bloc. Si je tombe, je ne tombe pas seul.";
            d["narration.ny_3_4.boss.after.0"] = "Vous ne comprenez pas. Si ce n'est pas moi, ce sera un autre.";
            d["narration.ny_3_4.boss.after.1"] = "Peut-être. Mais ce soir, c'est toi qui tombes.";
            d["narration.ny_3_4.boss.after.2.d"] = "Théo, je ne l'ai pas sauvé. Mais cette rue-là, je l'ai reprise.";
            d["narration.ny_3_4.boss.after.2.m"] = "Je vois ce que ce trottoir a vu. On n'oubliera pas son nom.";
            d["narration.ny_3_4.chapter_end.0"] = "Le chef du gang est tombé. Le quartier respire un peu, mais le prix est visible dans la rue.";
            d["narration.ny_3.epilogue.clinic.0"] = "Au matin, la clinique est intacte. Le marché, lui, n'est plus qu'une rangée de bâches noircies.";
            d["narration.ny_3.epilogue.clinic.1"] = "Nous avons sauvé nos vies. Pas nos étals. Je ne vous le reproche pas, mais je m'en souviendrai.";
            d["narration.ny_3.epilogue.clinic.2"] = "Les gens du marché viennent de se faire soigner chez moi. Ils n'ont jamais eu autant de raisons de nous dire merci, et jamais aussi peu envie de le faire.";
            d["narration.ny_3.epilogue.market.0"] = "Au matin, le marché est debout. La clinique, elle, a les vitraux brisés et les portes arrachées.";
            d["narration.ny_3.epilogue.market.1"] = "Je suis blessée, pas vaincue. Mais ma clinique fermera quelques semaines. Les malades iront où ils pourront.";
            d["narration.ny_3.epilogue.market.2"] = "On a gardé le marché. On a perdu la clinique. Je ne sais pas si c'était la bonne décision.";
            d["narration.ny_3.epilogue.common.0"] = "On n'a pas pu tout sauver. Cela ne fait pas de vous un mauvais héros. Cela fait de vous un héros qui regarde ce qu'il laisse derrière.";
            d["narration.ny_3.epilogue.common.1"] = "Manhattan, maintenant. Là où tout se décide.";
            d["chapter.ny_harlem.intro.agent_d"] = "C'est son quartier. Il en connaît chaque trottoir, chaque escalier de secours.";
            d["chapter.ny_harlem.intro.agent_m"] = "Ce n'est pas son quartier. Elle apprend ce que coûte une ville qui brûle.";

            // --- Fragments ---
            d["fragment.ny_harlem_fire.title"] = "Le bloc qui brûlait";
            d["fragment.ny_harlem_fire.text"] = "Les habitants avaient dit non à l'offre de rachat. Dans la nuit, le feu a donné une autre réponse. Tu as vu des familles descendre dans la fumée avec leurs photos sous le bras, comme si l'on pouvait sauver un quartier en emportant ses visages.";
            d["fragment.ny_harlem_clinic.title"] = "La clinique de l'église";
            d["fragment.ny_harlem_clinic.text"] = "Il n'y avait pas de formulaire, pas de badge, pas de questions. Seulement des draps blancs sous des vitraux fendus. Quand le docteur a dit qu'elle ne partirait pas, tu as compris que certains endroits n'existent que parce que quelqu'un refuse de les quitter.";
            d["fragment.ny_harlem_market.title"] = "Deux alertes";
            d["fragment.ny_harlem_market.text"] = "Deux alertes au même moment, deux endroits que tu aimais déjà. Tu savais qu'une seule main ne peut pas tenir deux portes. Ce que tu as choisi d'abord, le quartier le saura. Ce que tu as laissé, il s'en souviendra.";
            d["fragment.ny_harlem_chalk.title"] = "Le trottoir de craie";
            d["fragment.ny_harlem_chalk.text"] = "Il y avait encore un trait de craie sur le bord du trottoir. Tu as eu l'impression que la barrière tiendrait toujours, juste une seconde de plus. Tu as compris qu'on ne répare pas ce qui est arrivé ici : on fait en sorte que ça n'arrive plus.";
        }

        static Dictionary<string, string> HarlemEnglish(Dictionary<string, string> d)
        {
            // --- Enemies ---
            d["enemies.hrl_gangster.name"] = "Gang Muscle";
            d["enemies.hrl_torch.name"] = "Arsonist";
            d["enemies.hrl_roofer.name"] = "Rooftop Shooter";
            d["enemies.hrl_lieutenant.name"] = "Braise's Lieutenant";
            d["enemies.hrl_braise.name"] = "BRAISE";
            d["enemies.hrl_braise.phase2"] = "It Burns";
            d["enemies.hrl_braise.phase3"] = "The Whole Block";
            d["skills.hrl_gangster_a1"] = "Iron Bar";
            d["skills.hrl_gangster_a2"] = "Wall of Jackets";
            d["skills.hrl_torch_a1"] = "Homemade Cocktail";
            d["skills.hrl_torch_a2"] = "Rain of Sparks";
            d["skills.hrl_roofer_a1"] = "Rooftop Shot";
            d["skills.hrl_roofer_a2"] = "Window in the Crosshairs";
            d["skills.hrl_lieutenant_a1"] = "Lit Match";
            d["skills.hrl_lieutenant_a2"] = "Street on Fire";
            d["skills.hrl_lieutenant_a3"] = "Fireproof Jacket";
            d["skills.hrl_braise_a1"] = "Burning Fist";
            d["skills.hrl_braise_a2"] = "Ring of Fire";
            d["skills.hrl_braise_a3"] = "Unpaid Rent";
            d["skills.hrl_braise_a4"] = "Name the Culprit";
            d["skills.hrl_braise_p2_a2"] = "Grudge Flame";
            d["archives.enemy.hrl_gangster"] = "Gang Muscle: he blocks the stairwell and the street, and closes ranks with his mates. He lives off the neighborhood he shakes down.";
            d["archives.enemy.hrl_torch"] = "Arsonist: he sets buildings alight on command to empty the apartments. His flames bite, but they never linger.";
            d["archives.enemy.hrl_roofer"] = "Rooftop Shooter: he watches the alleys from the terraces and marks anyone who lingers.";
            d["archives.enemy.hrl_lieutenant"] = "Braise's Lieutenant: he carries the boss's orders from street to street. Where he passes, the whole squad is left with ashes.";
            d["archives.enemy.hrl_braise"] = "BRAISE: Dante Reyes, boss of the Harlem gang. He burns whole blocks so they can be bought cheaper, and calls himself a mere middleman.";

            // --- Zones ---
            d["zones.ny_3_1.name"] = "BURNED BLOCK — THE FIRE";
            d["zones.ny_3_1.short"] = "Burned Block";
            d["zones.ny_3_1.code"] = "Zone 3.1";
            d["zones.ny_3_1.desc"] = "A Harlem building is burning, and the neighbors swear nobody dropped a cigarette. Families are still trapped on the upper floors. Get them out before the block collapses, and look at who barricaded the stairs.";
            d["zones.ny_3_2.name"] = "MAKESHIFT CLINIC — HOLD THE DOOR";
            d["zones.ny_3_2.short"] = "Makeshift Clinic";
            d["zones.ny_3_2.code"] = "Zone 3.2";
            d["zones.ny_3_2.desc"] = "In a former church, Dr. Amina Colby treats the people the hospital no longer takes. The gang wants her shut down, and she refuses to leave. Gather the supplies and hold the door.";
            d["zones.ny_3_3.name"] = "STREET MARKET — TWO FIRES";
            d["zones.ny_3_3.short"] = "Street Market";
            d["zones.ny_3_3.code"] = "Zone 3.3";
            d["zones.ny_3_3.desc"] = "The market stalls and the clinic are under attack at the same time, and the squad cannot be in both places. Choose what you defend first: the other site will pay the price. There is no right choice, only a choice.";
            d["zones.ny_3_4.name"] = "WORKSHOP STREET — BRAISE";
            d["zones.ny_3_4.short"] = "Workshop Street";
            d["zones.ny_3_4.code"] = "Zone 3.4";
            d["zones.ny_3_4.desc"] = "The Harlem gang has dug in on Workshop Street, on a sidewalk the whole neighborhood knows and nobody crosses without lowering their eyes. Break its guard and face Braise.";
            d["qg.fil.zone_ny_3_1"] = "The gang is emptying Harlem's blocks. The clinic is next.";
            d["qg.fil.zone_ny_3_2"] = "The clinic holds. The market is the next weak point.";
            d["qg.fil.zone_ny_3_3"] = "The gang's leader is waiting on Workshop Street.";
            d["qg.fil.zone_ny_3_4"] = "On to Manhattan, where everything converges.";
            d["archives.zone.zone_ny_3_1"] = "In Harlem, a block set on fire by a gang is evacuated in time. The residents had refused a buyout offer.";
            d["archives.zone.zone_ny_3_2"] = "In Harlem, a makeshift clinic in a former church treats the whole neighborhood. Dr. Colby refuses to evacuate despite the gang's attack.";
            d["archives.zone.zone_ny_3_3"] = "In Harlem, The Dawn defends the clinic or the market first while the gang attacks both. Each choice leaves a visible loss.";
            d["archives.zone.zone_ny_3_4"] = "In Harlem, The Dawn ends the reign of Braise's gang on Workshop Street. The neighborhood bears the marks of the price paid.";

            // --- Objectives ---
            d["objectives.ny31.fire"] = "Reach the burning building";
            d["objectives.ny31.evacuate"] = "Evacuate the trapped families";
            d["objectives.ny31.arsonist"] = "Defeat the arsonist lieutenant";
            d["objectives.ny31.roofs"] = "Bring the last residents down from the roofs";
            d["objectives.ny32.clinic"] = "Enter the clinic";
            d["objectives.ny32.supplies"] = "Gather the medical supplies";
            d["objectives.ny32.assault"] = "Repel the gang's assault";
            d["objectives.ny32.hold"] = "Hold the door to the end";
            d["objectives.ny33.market"] = "Reach the market";
            d["objectives.ny33.stalls"] = "Move the stalls to safety";
            d["objectives.ny33.raiders"] = "Break the raiders";
            d["objectives.ny33.priority"] = "Choose what to defend first";
            d["objectives.ny34.chase"] = "Catch the lookouts on the rooftops";
            d["objectives.ny34.sidewalk"] = "Take the sidewalk";
            d["objectives.ny34.workshop"] = "Break into the workshop";
            d["objectives.ny34.braise"] = "Defeat Braise";

            // --- Dialogue ---
            d["speaker.narrateur"] = "Narrator";
            d["speaker.habitant"] = "A Resident";
            d["speaker.commercant"] = "A Vendor";
            d["speaker.dr_colby"] = "Dr. Amina Colby";
            d["speaker.braise"] = "Dante Reyes, \"Braise\"";
            d["narration.ny_3_1.intro.0"] = "The blackout left Harlem's buildings without power, without water pressure, without firefighters. This one has been burning for an hour.";
            d["narration.ny_3_1.intro.1"] = "There are people on every floor. I can hear children.";
            d["narration.ny_3_1.intro.2"] = "My neighbor says it was a short circuit. A short circuit that smells of gasoline?";
            d["narration.ny_3_1.intro.3"] = "We open the service stairs. I'll build you a walkway. Hold the rail and don't look down.";
            d["narration.ny_3_1.intro.3.m"] = "I see which floor will give way first. We evacuate in that order.";
            d["narration.ny_3_1.intro.4"] = "The third-floor ceiling won't hold. We have four minutes.";
            d["narration.ny_3_1.elite.before.0"] = "Nobody leaves. The tenants got an offer, and they're going to take it.";
            d["narration.ny_3_1.elite.before.1"] = "He talks about an offer while the block burns. He isn't even ashamed.";
            d["narration.ny_3_1.elite.after.0"] = "Gas cans, in the cellar. Someone set this up.";
            d["narration.ny_3_1.exit.0"] = "Men came yesterday offering to buy the building. We said no.";
            d["narration.ny_3_1.exit.1"] = "They changed the answer themselves.";
            d["narration.ny_3_1.exit.2"] = "I was born three streets from here. I drew on that wall when I was fifteen. Nothing's left of it.";
            d["narration.ny_3_2.intro.0"] = "The clinic occupies a former church. A generator coughs in the yard, and a line of wounded waits at the door.";
            d["narration.ny_3_2.intro.1"] = "No badge, no forms. They walk in, that's all.";
            d["narration.ny_3_2.intro.2"] = "Evacuate? The downtown hospitals admit nobody from this neighborhood. They only get treated here. If I leave, they have nowhere to go.";
            d["narration.ny_3_2.intro.3"] = "She isn't trying to be brave. She's done the math.";
            d["narration.ny_3_2.intro.4"] = "Then we hold the clinic. Just tell us which door must never fall.";
            d["narration.ny_3_2.intro.4.m"] = "I see where they'll come in. Just tell me which door you want to keep.";
            d["narration.ny_3_2.elite.before.0"] = "We're emptying the building. It'll be simpler for everyone.";
            d["narration.ny_3_2.elite.before.1"] = "Simpler for whom? Not for the people lying behind me.";
            d["narration.ny_3_2.elite.after.0"] = "The entrance holds. The patients are safe.";
            d["narration.ny_3_2.exit.0"] = "I won't thank you. I've thanked too many people who couldn't stay.";
            d["narration.ny_3_2.exit.1"] = "Stay, if you like. The gang isn't done.";
            d["narration.ny_3_3.intro.0"] = "The market vendors raise their tarps before daybreak. Two streets away, the clinic's lights come on.";
            d["narration.ny_3_3.intro.1"] = "Everybody knows everybody here. If the market falls, people have nothing to eat.";
            d["narration.ny_3_3.intro.2"] = "Two alerts at once. The gang is hitting the clinic and the market.";
            d["narration.ny_3_3.intro.3"] = "We can't cover both. Not enough people.";
            d["narration.ny_3_3.choice.0"] = "It's your call. Whatever you decide, the other side will pay for it.";
            d["narration.ny_3_3.choice.1"] = "Choice: defend the clinic first, or the market first.";
            d["narration.ny_3_3.branch_clinic.0"] = "The clinic first. People are lying there who can't run.";
            d["narration.ny_3_3.branch_clinic.0.m"] = "The clinic first. I see the beds, the drips: they couldn't flee.";
            d["narration.ny_3_3.branch_clinic.1"] = "The market... You're leaving us?";
            d["narration.ny_3_3.branch_clinic.2"] = "I'll be back. Hide what you can.";
            d["narration.ny_3_3.branch_market.0"] = "The market first. Families there live on what they sell today.";
            d["narration.ny_3_3.branch_market.0.m"] = "The market first. I see the families counting their last dollars there.";
            d["narration.ny_3_3.branch_market.1"] = "The clinic? You're leaving us?";
            d["narration.ny_3_3.branch_market.2"] = "Hold the door. I'll be back.";
            d["narration.ny_3_3.exit.0"] = "We held one side. For the other, we'll arrive after.";
            d["narration.ny_3_3.exit.1"] = "You rarely win without losing something. Remember what it costs.";
            d["narration.ny_3_4.intro.0"] = "Workshop Street is narrow, the shutters closed. Tags cover the walls: the gang's signature.";
            d["narration.ny_3_4.intro.1"] = "There's no one on this street, but the windows are watching us.";
            d["narration.ny_3_4.intro.2"] = "They're expecting us. Every door is held.";
            d["narration.ny_3_4.elite.before.0"] = "Braise said: nobody gets through. He also said you'd try.";
            d["narration.ny_3_4.elite.after.0"] = "The street is open. He's at the end, on the sidewalk.";
            d["narration.ny_3_4.boss.before.0"] = "There you are. I expected you sooner, but fire always takes its time.";
            d["narration.ny_3_4.boss.before.1"] = "The blackout emptied this city of its rules. I only picked up what was lying around.";
            d["narration.ny_3_4.boss.before.2"] = "The Steward paid well, before you disturbed his contract. A free block is worth more than an occupied one.";
            d["narration.ny_3_4.boss.before.3"] = "He doesn't apologize. He explains, as if it were obvious.";
            d["narration.ny_3_4.boss.before.4.d"] = "It's here. This is where Théo fell. I drew the barrier in chalk, right there. It held for one second.";
            d["narration.ny_3_4.boss.before.4.m"] = "This is where my brother was shot. He sold bread. He'd never done anything.";
            d["narration.ny_3_4.boss.phase1.0"] = "It burns. That's the word I learned.";
            d["narration.ny_3_4.boss.phase2.0"] = "The whole block. If I fall, I don't fall alone.";
            d["narration.ny_3_4.boss.after.0"] = "You don't understand. If it isn't me, it'll be someone else.";
            d["narration.ny_3_4.boss.after.1"] = "Maybe. But tonight, you're the one who falls.";
            d["narration.ny_3_4.boss.after.2.d"] = "I didn't save Théo. But this street, I took back.";
            d["narration.ny_3_4.boss.after.2.m"] = "I see what this sidewalk saw. We won't forget his name.";
            d["narration.ny_3_4.chapter_end.0"] = "The gang leader is down. The neighborhood breathes a little, but the price is visible in the street.";
            d["narration.ny_3.epilogue.clinic.0"] = "By morning, the clinic is intact. The market is nothing but a row of blackened tarps.";
            d["narration.ny_3.epilogue.clinic.1"] = "We saved our lives. Not our stalls. I don't blame you, but I'll remember.";
            d["narration.ny_3.epilogue.clinic.2"] = "The market people just got treated at my clinic. They've never had more reason to thank us, or less desire to.";
            d["narration.ny_3.epilogue.market.0"] = "By morning, the market stands. The clinic has shattered stained glass and torn-off doors.";
            d["narration.ny_3.epilogue.market.1"] = "I'm hurt, not beaten. But my clinic will close for a few weeks. The sick will go where they can.";
            d["narration.ny_3.epilogue.market.2"] = "We kept the market. We lost the clinic. I don't know if it was the right call.";
            d["narration.ny_3.epilogue.common.0"] = "We couldn't save everything. That doesn't make you a bad hero. It makes you a hero who looks at what he leaves behind.";
            d["narration.ny_3.epilogue.common.1"] = "Manhattan, now. Where everything gets decided.";
            d["chapter.ny_harlem.intro.agent_d"] = "It's his neighborhood. He knows every sidewalk, every fire escape.";
            d["chapter.ny_harlem.intro.agent_m"] = "It isn't her neighborhood. She is learning what a burning city costs.";

            // --- Fragments ---
            d["fragment.ny_harlem_fire.title"] = "The Block That Burned";
            d["fragment.ny_harlem_fire.text"] = "The residents had said no to the buyout offer. In the night, the fire gave another answer. You watched families come down through the smoke with photos under their arms, as if a neighborhood could be saved by carrying out its faces.";
            d["fragment.ny_harlem_clinic.title"] = "The Church Clinic";
            d["fragment.ny_harlem_clinic.text"] = "There were no forms, no badges, no questions. Only white sheets under cracked stained glass. When the doctor said she wouldn't leave, you understood that some places exist only because someone refuses to abandon them.";
            d["fragment.ny_harlem_market.title"] = "Two Alerts";
            d["fragment.ny_harlem_market.text"] = "Two alerts at the same moment, two places you already loved. You knew one hand can't hold two doors. What you chose first, the neighborhood will know. What you left behind, it will remember.";
            d["fragment.ny_harlem_chalk.title"] = "The Chalk Sidewalk";
            d["fragment.ny_harlem_chalk.text"] = "There was still a line of chalk on the edge of the sidewalk. You felt the barrier would hold forever, just one second longer. You understood that what happened here can't be repaired: you make sure it never happens again.";
            return d;
        }
    }
}
