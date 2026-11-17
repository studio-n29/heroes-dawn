#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes du chapitre Sadovoye : français (<c>ExtendSadovoye</c>) et anglais (<c>SadovoyeEnglish</c>, fusionné dans <c>En</c>).</summary>
    public static partial class Loc
    {
        static partial void ExtendSadovoye(Dictionary<string, string> d)
        {
            // Ennemis.
            d["enemies.sad_guard.name"] = "Garde privé";
            d["enemies.sad_agent.name"] = "Agent d'enchères";
            d["enemies.sad_operator.name"] = "Opérateur d'hologrammes";
            d["enemies.sad_majordome.name"] = "Majordome";
            d["enemies.sad_orlan.name"] = "DMITRI ORLAN";
            d["enemies.sad_orlan.phase2"] = "Dernière enchère";
            d["enemies.sad_orlan.phase3"] = "Liquidation";
            d["skills.sad_guard_a1"] = "Courtoisie armée";
            d["skills.sad_guard_a2"] = "Cordon de velours";
            d["skills.sad_agent_a1"] = "Tir discret";
            d["skills.sad_agent_a2"] = "Mise à prix";
            d["skills.sad_operator_a1"] = "Projection tranchante";
            d["skills.sad_operator_a2"] = "Mirage";
            d["skills.sad_majordome_a1"] = "Gant blanc";
            d["skills.sad_majordome_a2"] = "Service à la française";
            d["skills.sad_majordome_a3"] = "Livrée blindée";
            d["skills.sad_orlan_a1"] = "Prix d'ami";
            d["skills.sad_orlan_a2"] = "Verre sans tain";
            d["skills.sad_orlan_a3"] = "Fête de lumières";
            d["skills.sad_orlan_a4"] = "Clause de confidentialité";
            d["skills.sad_orlan_p2_a2"] = "Enchère montante";
            d["archives.enemy.sad_guard"] = "Garde privé : posté dans les halls des tours, il fait barrage avec la plus grande politesse. Il se couvre d'un bouclier et attire les coups.";
            d["archives.enemy.sad_agent"] = "Agent d'enchères : tireur discret des ventes privées. Il fixe un prix sur sa cible avant de tirer.";
            d["archives.enemy.sad_operator"] = "Opérateur d'hologrammes : il maquille la réalité avec la même aisance que le décor. Ses mirages aveuglent l'escouade.";
            d["archives.enemy.sad_majordome"] = "Majordome : l'élite de la maison. Il sert chaque invité comme il l'entend, et personne ne repart sans en porter la marque.";
            d["archives.enemy.sad_orlan"] = "DMITRI ORLAN : oligarque de l'anneau. Il fait de sa tour un théâtre et de chaque rencontre une vente. Il ne déteste personne : il évalue.";

            // Zones.
            d["zones.mos_3_1.name"] = "TOURS DE VERRE — LES TROPHÉES";
            d["zones.mos_3_1.short"] = "Tours de verre";
            d["zones.mos_3_1.code"] = "Zone M3.1";
            d["zones.mos_3_1.desc"] = "Sur le Jardin, les tours des oligarques brillent de mille hologrammes. Glisse-toi dans une tour grâce à des badges prêtés, infiltre les étages privés et traverse le salon : on y expose des trophées d'un genre particulier, des pouvoirs sous cloche.";
            d["zones.mos_3_2.name"] = "ENCHÈRES PRIVÉES — LE LOT";
            d["zones.mos_3_2.short"] = "Enchères privées";
            d["zones.mos_3_2.code"] = "Zone M3.2";
            d["zones.mos_3_2.desc"] = "Les trophées ne restent pas longtemps dans leurs bocaux : une vente se tient ce soir, sur invitation. Mêle-toi aux invités, trouble les enchères et libère l'Apex présenté comme une pièce rare.";
            d["zones.mos_3_3.name"] = "STUDIO D'HOLOGRAMMES — LE FAUX";
            d["zones.mos_3_3.short"] = "Studio d'hologrammes";
            d["zones.mos_3_3.code"] = "Zone M3.3";
            d["zones.mos_3_3.desc"] = "Un studio prépare une vidéo où l'on verrait l'Aube s'en prendre à des civils. Rien de tel n'a eu lieu. Arrive avant la diffusion, prends la régie, démonte le faux image par image et récupère les enregistrements d'origine.";
            d["zones.mos_3_4.name"] = "TOUR ORLAN — L'INTENDANT";
            d["zones.mos_3_4.short"] = "Tour Orlan";
            d["zones.mos_3_4.code"] = "Zone M3.4";
            d["zones.mos_3_4.desc"] = "Tout mène au sommet de la plus haute tour de l'anneau, où Dmitri Orlan reçoit comme on donne un spectacle. Force les coffres, brise sa sûreté et demande-lui pour qui il joue.";
            d["qg.fil.zone_mos_3_1"] = "Les enchères se tiennent à l'étage inférieur.";
            d["qg.fil.zone_mos_3_2"] = "Un studio d'hologrammes prépare une réponse.";
            d["qg.fil.zone_mos_3_3"] = "La tour Orlan domine l'anneau.";
            d["qg.fil.zone_mos_3_4"] = "Direction la Place Rouge.";
            d["archives.zone.zone_mos_3_1"] = "Dans les tours de verre de Sadovoye Koltso, l'Aube découvre un salon de trophées : des pouvoirs d'Apex conservés en bocaux et exposés à la vente.";
            d["archives.zone.zone_mos_3_2"] = "Aux enchères privées de Sadovoye, l'Aube perturbe la vente d'un Apex et libère le lot sous les yeux des enchérisseurs.";
            d["archives.zone.zone_mos_3_3"] = "Au studio d'hologrammes, l'Aube démonte une fausse vidéo où elle attaquerait des civils. Le démenti est publié, mais la rumeur circule.";
            d["archives.zone.zone_mos_3_4"] = "À la tour Orlan, l'Aube vainc l'oligarque. Il avoue travailler pour « quelqu'un au-dessus ». La fausse vidéo est démentie, mais la rumeur circule.";

            // Fragments de souvenirs (script, Moscou ch. 3).
            d["fragment.mos_towers_trophies.title"] = "Des trophées en bocaux";
            d["fragment.mos_towers_trophies.text"] = "Sous des cloches de verre, des lumières attendaient des acheteurs. Chaque bocal avait une étiquette, un nom, un prix. Tu as compris que le pouvoir de Moscou n'était pas celui des Apex, mais celui de ceux qui les collectionnent.";
            d["fragment.mos_auction.title"] = "Un lot";
            d["fragment.mos_auction.text"] = "Ils disaient « lot » comme on dit « tableau ». Personne dans la salle n'a élevé la voix. Tu as compris que la plus grande cruauté ne crie jamais, elle enchérit.";
            d["fragment.mos_holo_studio.title"] = "La fausse vidéo";
            d["fragment.mos_holo_studio.text"] = "Ils t'avaient filmée en train de faire ce que tu n'avais jamais fait. Chaque geste était parfait, chaque ombre exacte. Tu as compris que le mensonge le plus dangereux est celui qui te ressemble.";
            d["fragment.mos_orlan_tower.title"] = "Quelqu'un au-dessus";
            d["fragment.mos_orlan_tower.text"] = "Il t'a reçu comme un hôte parfait, dans une tour qui dominait la ville. Quand il a avoué ne pas être le dernier maillon, tu as compris que le mensonge de Moscou n'avait pas qu'un étage.";

            // Objectifs.
            d["objectives.mos31.floors"] = "Atteindre les étages privés";
            d["objectives.mos31.badge"] = "Se faire passer pour un invité";
            d["objectives.mos31.salon"] = "Vaincre le maître de salon";
            d["objectives.mos31.trophies"] = "Examiner les trophées";
            d["objectives.mos32.hall"] = "Entrer dans la salle des ventes";
            d["objectives.mos32.bids"] = "Perturber les enchères";
            d["objectives.mos32.lot"] = "Briser la garde du lot";
            d["objectives.mos32.apex"] = "Libérer l'Apex mis en vente";
            d["objectives.mos33.control"] = "Atteindre la régie";
            d["objectives.mos33.sync"] = "Désynchroniser les projecteurs";
            d["objectives.mos33.operator"] = "Neutraliser les opérateurs";
            d["objectives.mos33.fake"] = "Démonter la fausse vidéo";
            d["objectives.mos34.vault"] = "Ouvrir les coffres de la tour";
            d["objectives.mos34.guard"] = "Briser la sûreté de la tour";
            d["objectives.mos34.office"] = "Forcer le bureau d'Orlan";
            d["objectives.mos34.orlan"] = "Vaincre Dmitri Orlan";

            // Locuteurs (membres d'équipe, agent_m et enforcer : déjà définis ailleurs).
            d["speaker.narrateur"] = "Narrateur";
            d["speaker.commissaire"] = "Le commissaire";
            d["speaker.technicien_holo"] = "Technicien d'hologrammes";
            d["speaker.orlan"] = "Dmitri Orlan";

            // Phrases d'ouverture selon le parcours.
            d["chapter.mos_sadovoye.intro.agent_m"] = "Elle a grandi à l'ombre de ces tours. Elle n'en avait jamais vu l'intérieur.";
            d["chapter.mos_sadovoye.intro.agent_d"] = "Il a vu des gangs cacher leur pouvoir. Ici, il l'expose, comme une vitrine.";

            // Narration (script validé, Moscou ch. 3).
            d["narration.mos_3_1.intro.0"] = "Les tours de verre se reflètent dans le Jardin. À l'intérieur, des couloirs silencieux, des ascenseurs sans bruit.";
            d["narration.mos_3_1.intro.1"] = "Tout est trop propre. Même le silence a été nettoyé.";
            d["narration.mos_3_1.intro.2"] = "Ce salon… il y a des bocaux. Avec de la lumière dedans.";
            d["narration.mos_3_1.intro.3"] = "Ce ne sont pas des bocaux. Ce sont des trophées.";
            d["narration.mos_3_1.intro.3.d"] = "On expose des gens comme on expose des tableaux. Quelqu'un en tire de la fierté.";
            d["narration.mos_3_1.elite.before.0"] = "Les invités sont priés de rester au salon d'accueil.";
            d["narration.mos_3_1.elite.before.1"] = "Il sourit pendant qu'il bloque la porte. Le sourire fait partie du service.";
            d["narration.mos_3_1.elite.after.0"] = "Chaque bocal a une étiquette, un nom, un prix.";
            d["narration.mos_3_1.exit.0"] = "C'est la lumière qu'on a prise à l'Apex du laboratoire. Je la reconnais.";
            d["narration.mos_3_1.exit.1"] = "Ils l'exposent comme un tableau. On monte aux enchères.";
            d["narration.mos_3_1.exit.2"] = "Même parfum de cire dans cet ascenseur. Quelqu'un laisse sa signature partout.";
            d["narration.mos_3_2.intro.0"] = "La salle est tamisée. Des enchérisseurs masqués lèvent un doigt, jamais la voix.";
            d["narration.mos_3_2.intro.1"] = "Mesdames, messieurs, le lot suivant : un Apex éveillé, de rang cinq, en excellent état.";
            d["narration.mos_3_2.intro.2"] = "Un lot. Ils disent « lot ».";
            d["narration.mos_3_2.intro.3"] = "Je vois l'homme qu'ils présentent, sa vie avant la vente. On le vend avec son passé.";
            d["narration.mos_3_2.intro.3.d"] = "Je dessinerais bien un barreau devant chaque lot. Mais ce n'est pas encore le moment.";
            d["narration.mos_3_2.elite.before.0"] = "La vente est privée. Veuillez regagner vos places.";
            d["narration.mos_3_2.elite.before.1"] = "Il ne menace pas. Il demande poliment. C'est pire.";
            d["narration.mos_3_2.elite.after.0"] = "Les issues sont à nous. Le lot est libre.";
            d["narration.mos_3_2.exit.0"] = "La vente reprend, mesdames, messieurs. Un incident sans importance.";
            d["narration.mos_3_2.exit.1"] = "Un incident sans importance. Pour eux, nous sommes déjà oubliés.";
            d["narration.mos_3_3.intro.0"] = "Le studio est un plateau noir. Des projecteurs dessinent des visages qui n'existent pas.";
            d["narration.mos_3_3.intro.1"] = "Dans une heure, la ville verra l'Aube frapper des civils. Elle aura tout vu, et rien vécu.";
            d["narration.mos_3_3.intro.2"] = "Ils ont copié nos silhouettes, nos insignes, nos mouvements.";
            d["narration.mos_3_3.intro.3"] = "Je vois la vidéo avant qu'elle n'existe. Quelqu'un l'a répétée cent fois.";
            d["narration.mos_3_3.intro.3.d"] = "Ils ont copié mes lignes, mes couleurs, mes habitudes. Même mes erreurs. Quelqu'un étudie.";
            d["narration.mos_3_3.elite.before.0"] = "Les serveurs sont sous protection. Vous ne verrez que ce que nous diffusons.";
            d["narration.mos_3_3.elite.after.0"] = "Les enregistrements originaux sont là. Chaque plan truqué est daté.";
            d["narration.mos_3_3.exit.0"] = "On démonte la vidéo. Avec ses dates, elle prouve elle-même qu'elle est fausse.";
            d["narration.mos_3_3.exit.1"] = "Mais elle a déjà circulé. Quelque part, quelqu'un la croit.";
            d["narration.mos_3_4.intro.0"] = "L'étage d'Orlan est une vitre sur toute la ville. Les hologrammes de Moscou tournent à ses pieds comme des jouets.";
            d["narration.mos_3_4.intro.1"] = "Il nous regarde arriver depuis un étage. Il a tout vu avant nous.";
            d["narration.mos_3_4.elite.before.0"] = "Monsieur Orlan reçoit sur rendez-vous. Je regrette.";
            d["narration.mos_3_4.elite.after.0"] = "La porte est ouverte. Il nous attend.";
            d["narration.mos_3_4.boss.before.0"] = "Bienvenue. Vous avez vu mes trophées, mes enchères, mes caméras. Je suis honoré de votre curiosité.";
            d["narration.mos_3_4.boss.before.1"] = "Moscou vous a accueillis, mademoiselle. Je n'ai fait que vous offrir un spectacle à la hauteur.";
            d["narration.mos_3_4.boss.before.1.d"] = "Moscou vous a accueillis, monsieur.";
            d["narration.mos_3_4.boss.before.2"] = "Des gens ne sont pas un spectacle.";
            d["narration.mos_3_4.boss.before.2.d"] = "Des gens ne sont pas un spectacle.";
            d["narration.mos_3_4.boss.before.3"] = "Tout est un spectacle, mademoiselle. La seule question est de savoir qui tient la salle.";
            d["narration.mos_3_4.boss.phase1.0"] = "Dernière enchère. Je vous laisse la mise.";
            d["narration.mos_3_4.boss.phase2.0"] = "Liquidation. Tout doit partir, moi compris.";
            d["narration.mos_3_4.boss.after.0"] = "Vous croyez m'avoir démasqué. Je ne suis qu'un exécutant, comme tant d'autres.";
            d["narration.mos_3_4.boss.after.1"] = "Pour qui ?";
            d["narration.mos_3_4.boss.after.2"] = "Pour quelqu'un au-dessus. Quelqu'un qui n'a jamais eu besoin de signer.";
            d["narration.mos_3_4.boss.after.3"] = "Quelqu'un qui aime l'encens et la cire. Un homme d'ordre, mademoiselle.";
            d["narration.mos_3_4.chapter_end.0"] = "La fausse vidéo est démentie, mais la rumeur circule. Orlan a avoué : quelqu'un tire encore les ficelles, plus haut.";
        }

        static Dictionary<string, string> SadovoyeEnglish(Dictionary<string, string> d)
        {
            // Enemies.
            d["enemies.sad_guard.name"] = "Private Guard";
            d["enemies.sad_agent.name"] = "Auction Agent";
            d["enemies.sad_operator.name"] = "Hologram Operator";
            d["enemies.sad_majordome.name"] = "Majordomo";
            d["enemies.sad_orlan.name"] = "DMITRI ORLAN";
            d["enemies.sad_orlan.phase2"] = "Last Auction";
            d["enemies.sad_orlan.phase3"] = "Liquidation";
            d["skills.sad_guard_a1"] = "Armed Courtesy";
            d["skills.sad_guard_a2"] = "Velvet Rope";
            d["skills.sad_agent_a1"] = "Quiet Shot";
            d["skills.sad_agent_a2"] = "Reserve Price";
            d["skills.sad_operator_a1"] = "Sharp Projection";
            d["skills.sad_operator_a2"] = "Mirage";
            d["skills.sad_majordome_a1"] = "White Glove";
            d["skills.sad_majordome_a2"] = "Silver Service";
            d["skills.sad_majordome_a3"] = "Armored Livery";
            d["skills.sad_orlan_a1"] = "Friendly Price";
            d["skills.sad_orlan_a2"] = "One-Way Glass";
            d["skills.sad_orlan_a3"] = "Festival of Lights";
            d["skills.sad_orlan_a4"] = "Non-Disclosure Clause";
            d["skills.sad_orlan_p2_a2"] = "Rising Bid";
            d["archives.enemy.sad_guard"] = "Private guard: posted in the tower lobbies, he blocks the way with perfect politeness. He raises a shield and draws the blows.";
            d["archives.enemy.sad_agent"] = "Auction agent: the quiet gunman of private sales. He puts a price on his target before firing.";
            d["archives.enemy.sad_operator"] = "Hologram operator: he doctors reality as easily as scenery. His mirages blind the whole squad.";
            d["archives.enemy.sad_majordome"] = "Majordomo: the elite of the house. He serves every guest as he sees fit, and nobody leaves without his mark.";
            d["archives.enemy.sad_orlan"] = "DMITRI ORLAN: oligarch of the ring. He turns his tower into a theater and every meeting into a sale. He hates no one: he appraises.";

            // Zones.
            d["zones.mos_3_1.name"] = "GLASS TOWERS — THE TROPHIES";
            d["zones.mos_3_1.short"] = "Glass Towers";
            d["zones.mos_3_1.code"] = "Zone M3.1";
            d["zones.mos_3_1.desc"] = "On the Garden Ring, the oligarchs' towers glitter with a thousand holograms. Slip into a tower on borrowed badges, infiltrate the private floors and cross the lounge: they display trophies of a very particular kind, powers under glass.";
            d["zones.mos_3_2.name"] = "PRIVATE AUCTION — THE LOT";
            d["zones.mos_3_2.short"] = "Private Auction";
            d["zones.mos_3_2.code"] = "Zone M3.2";
            d["zones.mos_3_2.desc"] = "The trophies do not stay in their jars for long: a sale is held tonight, by invitation only. Mingle with the guests, disrupt the bidding and free the Apex presented as a rare piece.";
            d["zones.mos_3_3.name"] = "HOLOGRAM STUDIO — THE FAKE";
            d["zones.mos_3_3.short"] = "Hologram Studio";
            d["zones.mos_3_3.code"] = "Zone M3.3";
            d["zones.mos_3_3.desc"] = "A studio is preparing a video that would show The Dawn attacking civilians. Nothing of the kind happened. Get there before it airs, take the control room, take the fake apart frame by frame and recover the original recordings.";
            d["zones.mos_3_4.name"] = "ORLAN TOWER — THE STEWARD";
            d["zones.mos_3_4.short"] = "Orlan Tower";
            d["zones.mos_3_4.code"] = "Zone M3.4";
            d["zones.mos_3_4.desc"] = "Everything leads to the top of the ring's tallest tower, where Dmitri Orlan receives guests the way others stage a show. Break open the vaults, crush his security and ask him who he performs for.";
            d["qg.fil.zone_mos_3_1"] = "The auctions are held on the floor below.";
            d["qg.fil.zone_mos_3_2"] = "A hologram studio is preparing a response.";
            d["qg.fil.zone_mos_3_3"] = "Orlan's tower overlooks the ring.";
            d["qg.fil.zone_mos_3_4"] = "On to Red Square.";
            d["archives.zone.zone_mos_3_1"] = "In the glass towers of Sadovoye Koltso, The Dawn finds a trophy lounge: Apex powers preserved in jars and displayed for sale.";
            d["archives.zone.zone_mos_3_2"] = "At Sadovoye's private auctions, The Dawn disrupts the sale of an Apex and frees the lot before the bidders' eyes.";
            d["archives.zone.zone_mos_3_3"] = "At the hologram studio, The Dawn dismantles a fake video in which it attacks civilians. The denial is published, but the rumor spreads.";
            d["archives.zone.zone_mos_3_4"] = "At Orlan Tower, The Dawn defeats the oligarch. He admits to working for \"someone above.\" The fake video is denied, but the rumor circulates.";

            // Memory fragments (script, Moscow ch. 3).
            d["fragment.mos_towers_trophies.title"] = "Trophies in Jars";
            d["fragment.mos_towers_trophies.text"] = "Under glass bells, lights awaited buyers. Every jar had a label, a name, a price. You understood that Moscow's power wasn't that of the Apex, but of those who collect them.";
            d["fragment.mos_auction.title"] = "A Lot";
            d["fragment.mos_auction.text"] = "They said \"lot\" the way one says \"painting.\" No one in the room raised their voice. You understood that the greatest cruelty never shouts, it bids.";
            d["fragment.mos_holo_studio.title"] = "The Fake Video";
            d["fragment.mos_holo_studio.text"] = "They had filmed you doing what you had never done. Every gesture was perfect, every shadow exact. You understood that the most dangerous lie is the one that looks like you.";
            d["fragment.mos_orlan_tower.title"] = "Someone Above";
            d["fragment.mos_orlan_tower.text"] = "He received you like a perfect host, in a tower overlooking the city. When he admitted he was not the last link, you understood that Moscow's lie had more than one floor.";

            // Objectives.
            d["objectives.mos31.floors"] = "Reach the private floors";
            d["objectives.mos31.badge"] = "Pass yourself off as a guest";
            d["objectives.mos31.salon"] = "Defeat the lounge master";
            d["objectives.mos31.trophies"] = "Examine the trophies";
            d["objectives.mos32.hall"] = "Enter the sales room";
            d["objectives.mos32.bids"] = "Disrupt the bidding";
            d["objectives.mos32.lot"] = "Break the lot's guard";
            d["objectives.mos32.apex"] = "Free the Apex on sale";
            d["objectives.mos33.control"] = "Reach the control room";
            d["objectives.mos33.sync"] = "Desynchronize the projectors";
            d["objectives.mos33.operator"] = "Neutralize the operators";
            d["objectives.mos33.fake"] = "Take the fake video apart";
            d["objectives.mos34.vault"] = "Open the tower vaults";
            d["objectives.mos34.guard"] = "Break the tower's security";
            d["objectives.mos34.office"] = "Force Orlan's office";
            d["objectives.mos34.orlan"] = "Defeat Dmitri Orlan";

            // Speakers.
            d["speaker.narrateur"] = "Narrator";
            d["speaker.commissaire"] = "The Auctioneer";
            d["speaker.technicien_holo"] = "Hologram Technician";
            d["speaker.orlan"] = "Dmitri Orlan";

            // Route opening sentences.
            d["chapter.mos_sadovoye.intro.agent_m"] = "She grew up in the shadow of these towers. She had never seen the inside.";
            d["chapter.mos_sadovoye.intro.agent_d"] = "He has seen gangs hide their power. Here, it is displayed like a shop window.";

            // Narration (script, Moscow ch. 3).
            d["narration.mos_3_1.intro.0"] = "The glass towers are reflected in the Garden Ring. Inside, silent corridors, noiseless elevators.";
            d["narration.mos_3_1.intro.1"] = "Everything's too clean. Even the silence has been scrubbed.";
            d["narration.mos_3_1.intro.2"] = "This lounge... there are jars. With light inside.";
            d["narration.mos_3_1.intro.3"] = "They're not jars. They're trophies.";
            d["narration.mos_3_1.intro.3.d"] = "They exhibit people the way they exhibit paintings. Someone takes pride in it.";
            d["narration.mos_3_1.elite.before.0"] = "Guests are kindly asked to remain in the reception lounge.";
            d["narration.mos_3_1.elite.before.1"] = "He smiles while he blocks the door. The smile is part of the service.";
            d["narration.mos_3_1.elite.after.0"] = "Every jar has a label, a name, a price.";
            d["narration.mos_3_1.exit.0"] = "That's the light they took from the Apex in the laboratory. I recognize it.";
            d["narration.mos_3_1.exit.1"] = "They display it like a painting. Bidding starts soon.";
            d["narration.mos_3_1.exit.2"] = "The same scent of wax in this elevator. Someone leaves their signature everywhere.";
            d["narration.mos_3_2.intro.0"] = "The room is dimmed. Masked bidders raise a finger, never their voice.";
            d["narration.mos_3_2.intro.1"] = "Ladies and gentlemen, the next lot: an awakened Apex, rank five, in excellent condition.";
            d["narration.mos_3_2.intro.2"] = "A lot. They say \"lot.\"";
            d["narration.mos_3_2.intro.3"] = "I see the man they're presenting, his life before the sale. They're selling him with his past.";
            d["narration.mos_3_2.intro.3.d"] = "I'd gladly draw a bar in front of every lot. But it isn't time yet.";
            d["narration.mos_3_2.elite.before.0"] = "The sale is private. Please return to your seats.";
            d["narration.mos_3_2.elite.before.1"] = "He doesn't threaten. He asks politely. It's worse.";
            d["narration.mos_3_2.elite.after.0"] = "The exits are ours. The lot is free.";
            d["narration.mos_3_2.exit.0"] = "The sale resumes, ladies and gentlemen. An incident of no importance.";
            d["narration.mos_3_2.exit.1"] = "An incident of no importance. To them, we're already forgotten.";
            d["narration.mos_3_3.intro.0"] = "The studio is a black stage. Spotlights draw faces that don't exist.";
            d["narration.mos_3_3.intro.1"] = "In an hour, the city will see The Dawn strike civilians. It will have seen everything, and lived nothing.";
            d["narration.mos_3_3.intro.2"] = "They copied our silhouettes, our insignia, our movements.";
            d["narration.mos_3_3.intro.3"] = "I see the video before it exists. Someone rehearsed it a hundred times.";
            d["narration.mos_3_3.intro.3.d"] = "They copied my lines, my colors, my habits. Even my mistakes. Someone's been studying.";
            d["narration.mos_3_3.elite.before.0"] = "The servers are under protection. You will only see what we broadcast.";
            d["narration.mos_3_3.elite.after.0"] = "The original recordings are here. Every doctored shot is dated.";
            d["narration.mos_3_3.exit.0"] = "We take the video apart. With its dates, it proves by itself that it's fake.";
            d["narration.mos_3_3.exit.1"] = "But it's already circulated. Somewhere, someone believes it.";
            d["narration.mos_3_4.intro.0"] = "Orlan's floor is a pane of glass over the whole city. Moscow's holograms spin at his feet like toys.";
            d["narration.mos_3_4.intro.1"] = "He watches us arrive from a floor above. He saw everything before we did.";
            d["narration.mos_3_4.elite.before.0"] = "Mr. Orlan receives by appointment. My apologies.";
            d["narration.mos_3_4.elite.after.0"] = "The door is open. He's expecting us.";
            d["narration.mos_3_4.boss.before.0"] = "Welcome. You've seen my trophies, my auctions, my cameras. I'm honored by your curiosity.";
            d["narration.mos_3_4.boss.before.1"] = "Moscow welcomed you, miss. I merely offered you a show worthy of it.";
            d["narration.mos_3_4.boss.before.1.d"] = "Moscow welcomed you, sir.";
            d["narration.mos_3_4.boss.before.2"] = "People are not a show.";
            d["narration.mos_3_4.boss.before.2.d"] = "People are not a show.";
            d["narration.mos_3_4.boss.before.3"] = "Everything is a show, miss. The only question is who holds the room.";
            d["narration.mos_3_4.boss.phase1.0"] = "Last bid. I leave the stakes to you.";
            d["narration.mos_3_4.boss.phase2.0"] = "Liquidation. Everything must go, myself included.";
            d["narration.mos_3_4.boss.after.0"] = "You think you've exposed me. I'm only an executor, like so many others.";
            d["narration.mos_3_4.boss.after.1"] = "For whom?";
            d["narration.mos_3_4.boss.after.2"] = "For someone above. Someone who never needed to sign.";
            d["narration.mos_3_4.boss.after.3"] = "Someone who loves incense and wax. A man of order, miss.";
            d["narration.mos_3_4.chapter_end.0"] = "The fake video is denied, but the rumor circulates. Orlan has confessed: someone is still pulling the strings, higher up.";
            return d;
        }
    }
}
