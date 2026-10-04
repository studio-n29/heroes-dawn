#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes du pass, des missions du jour, de la boutique, des opérations, du labo et des archives.</summary>
    public static partial class Loc
    {
        static partial void ExtendMeta(Dictionary<string, string> d)
        {
            d["ui.gems_short"] = "Gemmes";
            d["ui.xp_short"] = "pts";
            d["errors.not_enough_gems"] = "Pas assez de Gemmes.";
            d["errors.meta_unknown"] = "Élément introuvable.";
            d["errors.meta_not_ready"] = "Pas encore prêt.";
            d["errors.meta_claimed"] = "Déjà récupéré.";
            d["errors.meta_premium"] = "Il faut le pass premium.";
            d["errors.meta_level_locked"] = "Niveau pas encore atteint.";
            d["errors.meta_busy"] = "Déjà occupé.";
            d["errors.meta_no_members"] = "Pas assez de membres libres hors de l'escouade.";
            d["errors.meta_payment"] = "Paiement refusé (simulé). Réessaie.";
            d["errors.meta_done"] = "Déjà fait.";
            d["errors.on_expedition"] = "Ce membre est en expédition.";
            d["wip.soon"] = "BIENTÔT";

            // --- Pass
            d["page.pass"] = "Pass · Saison 1";
            d["pass.level"] = "NIVEAU {0} / {1}";
            d["pass.season"] = "Fin de la saison dans {0}";
            d["pass.xp"] = "{0} / {1} pts";
            d["pass.max"] = "NIVEAU MAX";
            d["pass.buy_premium"] = "PASS PREMIUM · {0} GEMMES";
            d["pass.premium_on"] = "PASS PREMIUM ACTIF";
            d["pass.premium_bought"] = "Pass premium débloqué !";
            d["pass.premium_needed"] = "Le pass premium débloque cette piste.";
            d["pass.claim_all"] = "TOUT RÉCUPÉRER ({0})";
            d["pass.nothing"] = "RIEN À RÉCUPÉRER";
            d["pass.nothing_reason"] = "Gagne des points avec les missions du jour et les combats.";
            d["pass.free"] = "Gratuit : {0}";
            d["pass.prem"] = "Premium : {0}";
            d["pass.take_free"] = "PRENDRE";
            d["pass.take_premium"] = "PREMIUM";
            d["pass.taken"] = "PRIS";
            d["pass.locked_level"] = "Atteins le niveau {0}.";
            d["pass.claimed"] = "Pass : {0}";

            // --- Missions du jour
            d["daily.win2"] = "Gagner {0} combats";
            d["daily.win3"] = "Gagner {0} combats";
            d["daily.win5"] = "Gagner {0} combats";
            d["daily.train1"] = "Entraîner {0} membre";
            d["daily.train2"] = "Entraîner {0} fois";
            d["daily.recruit1"] = "Recruter {0} héros";
            d["daily.zone1"] = "Valider {0} zone";
            d["daily.earn1000"] = "Gagner {0} Crédits";
            d["daily.claim"] = "RÉCUPÉRER";
            d["daily.not_done"] = "Mission pas terminée.";
            d["daily.claimed"] = "Mission accomplie : {0}";
            d["daily.reset"] = "Nouvelles missions dans {0}";
            d["qg.daily.body"] = "Trois missions par jour pour faire avancer le pass.";
            d["qg.daily.line"] = "{0} · {1} / {2}";
            d["qg.daily.all_done"] = "Missions terminées. À demain !";
            d["qg.daily.claimable"] = "{0} à récupérer";

            d["qg.ops.ready"] = "{0} expédition(s) de retour !";
            d["qg.ops.running"] = "{0} expédition(s) en cours";
            d["qg.ops.idle"] = "Envoie des membres en expédition";
            d["qg.lab.ready"] = "Recherche terminée !";
            d["qg.lab.running"] = "Recherche en cours · {0}";
            d["qg.lab.idle"] = "Lance une recherche";
            d["qg.archives.new"] = "{0} nouvelle(s) fiche(s)";
            d["qg.archives.none"] = "Aucune nouveauté";

            // --- Objectifs secondaires
            d["qg.secondary"] = "Objectifs secondaires";
            d["qg.secondary.none"] = "Aucun objectif pour l'instant. Valide une zone pour en débloquer.";
            d["qg.secondary.line"] = "{0} : les pillards sont revenus · expire dans {1}";
            d["meta.src.objective"] = "Objectif secondaire rempli : {0}";
            d["meta.src.first_clear"] = "Première victoire : {0}";

            // --- Boutique
            d["page.shop"] = "Boutique";
            d["shop.gems"] = "GEMMES : {0}";
            d["shop.simulated"] = "Les achats sont simulés dans cette version : rien n'est débité.";
            d["shop.gift"] = "CADEAU DU JOUR · {0}";
            d["shop.gift_done"] = "CADEAU PRIS";
            d["shop.gift_done_reason"] = "Reviens demain pour un nouveau cadeau.";
            d["shop.gift_taken"] = "Cadeau du jour récupéré !";
            d["shop.pack"] = "{0} Gemmes";
            d["shop.pack_price"] = "{0} · achat simulé";
            d["shop.ex_credits"] = "1 000 Crédits";
            d["shop.ex_pr"] = "60 PR";
            d["shop.ex_rest"] = "Repos complet de l'équipe";
            d["shop.cost_gems"] = "Coût : {0} Gemmes";
            d["shop.exchange"] = "{0} GEMMES";
            d["shop.need_gems"] = "Il te manque {0} Gemmes.";
            d["shop.exchanged"] = "Échange effectué !";
            d["shop.confirm.title"] = "Acheter des Gemmes ?";
            d["shop.confirm.body"] = "{0} Gemmes pour {1}. Achat simulé : aucun paiement réel n'est effectué.";
            d["shop.confirm.ok"] = "ACHETER";
            d["shop.bought"] = "+{0} Gemmes !";

            // --- Opérations
            d["page.operations"] = "Salle des opérations";
            d["ops.title"] = "EXPÉDITIONS";
            d["ops.free"] = "{0} membre(s) libre(s) hors de l'escouade.";
            d["ops.none_free"] = "Aucun membre libre : recrute des héros et garde-les hors de l'escouade.";
            d["ops.slot_free"] = "Emplacement {0} · libre";
            d["ops.templates"] = "Patrouille 15 min (1 membre) · Reconnaissance 1 h (2) · Escorte 4 h (3)";
            d["ops.go"] = "{0} · {1}";
            d["ops.need"] = "Il faut {0} membre(s) libre(s) hors de l'escouade.";
            d["ops.patrol"] = "Patrouille de quartier";
            d["ops.recon"] = "Reconnaissance";
            d["ops.convoy"] = "Escorte de convoi";
            d["ops.running"] = "{0} · gains : {1}";
            d["ops.back"] = "DE RETOUR";
            d["ops.claim"] = "RÉCUPÉRER";
            d["ops.speed"] = "ACCÉLÉRER · {0}";
            d["ops.started"] = "Expédition lancée !";
            d["ops.claimed"] = "Expédition terminée : {0}";

            // --- Labo
            d["page.lab"] = "Labo de recherche";
            d["lab.title"] = "RECHERCHES";
            d["lab.idle"] = "Aucune recherche en cours.";
            d["lab.running"] = "En cours : {0} · {1}";
            d["lab.level"] = "{0} · niv. {1}/{2}";
            d["lab.training"] = "Méthodes d'entraînement";
            d["lab.training.desc"] = "Entraînement moins cher : -4 % de Crédits par niveau.";
            d["lab.loot"] = "Récupération de butin";
            d["lab.loot.desc"] = "Combats plus rentables : +5 % de Crédits par niveau.";
            d["lab.recovery"] = "Médecine de terrain";
            d["lab.recovery.desc"] = "Repos plus rapide hors mission : +1 fatigue récupérée par minute et par niveau.";
            d["lab.logistics"] = "Logistique";
            d["lab.logistics.desc"] = "Zones plus généreuses : +5 % de récompense de validation par niveau.";
            d["lab.start"] = "RECHERCHER · {0} CR";
            d["lab.time"] = "durée {0}";
            d["lab.max"] = "NIVEAU MAX";
            d["lab.busy"] = "Une recherche est déjà en cours.";
            d["lab.speed"] = "ACCÉLÉRER · {0} GEMMES";
            d["lab.started"] = "Recherche lancée !";
            d["lab.sped"] = "Recherche accélérée !";
            d["lab.done"] = "Recherche terminée : {0}";

            // --- Archives
            d["page.archives"] = "Archives";
            d["archives.title"] = "DOSSIERS DE LA GUILDE";
            d["archives.count"] = "{0} fiches sur {1} débloquées.";
            d["archives.locked"] = "Fiche verrouillée";
            d["archives.locked_hint"] = "Avance dans l'histoire pour débloquer cette fiche : recrute des héros, valide des zones et bats leurs ennemis.";
            d["archives.new"] = "NOUVEAU";
            d["archives.cat.Lore"] = "Histoire";
            d["archives.cat.Hero"] = "Héros";
            d["archives.cat.Zone"] = "Zone";
            d["archives.cat.Enemy"] = "Ennemi";
            d["archives.lore.fracture.title"] = "La Fracture";
            d["archives.lore.fracture.body"] = "Après la Supernova de Paris, le monde a connu un blackout. Les radiations ont éveillé des milliers de méta-humains naturels. New York, fragile, est devenue l'une des villes les plus touchées : les gangs, l'AMIF et les civils pris entre deux feux s'y disputent la nuit.";
            d["archives.lore.section.title"] = "La section New York";
            d["archives.lore.section.body"] = "La Guilde ouvre des sections locales dans les grandes villes. Celle de New York s'installe à Red Hook, dans un entrepôt reconverti. Tu la diriges : recrute, entraîne, soigne, et envoie tes escouades sur le terrain.";
            d["archives.lore.rayner.title"] = "Brie Rayner";
            d["archives.lore.rayner.body"] = "Directrice stratégique et scientifique de la Guilde. Hyper-intelligente et électro-kinésiste, elle suit chaque section à distance et te fait parvenir ses ordres par le fil du QG.";
            d["archives.lore.apex.title"] = "Les Apex";
            d["archives.lore.apex.body"] = "Les premiers méta-humains, créés dans les années 80 par le protocole Khost, ont fondé la Guilde. Adam West entraîne ta première équipe à Red Hook. Les Légendes que tu recrutes sont, elles, des méta-humains naturels.";
            d["archives.hero.ancre"] = "ANCRE tient la ligne. Né au pied d'une grue de Red Hook, il a retenu un conteneur en chute avec ses seules mains pendant la Supernova. Depuis, rien ne le déplace.";
            d["archives.hero.verre"] = "VERRE plie le verre et la matière transparente. Elle a emprisonné trois pillards dans une cage de vitrine, puis a demandé poliment qu'on la laisse tranquille.";
            d["archives.hero.givre"] = "GIVRE fige l'air autour d'elle. Ses morsures de froid ralentissent tout ce qu'elle touche, y compris ses propres pensées quand elle est fatiguée.";
            d["archives.hero.echo"] = "ÉCHO entend ce que les murs ont retenu. Ses ondes révèlent les failles de l'adversaire : une marque, une faiblesse, et l'escouade frappe là où ça fait mal.";
            d["archives.hero.arc"] = "ARC canalise la foudre. Impulsif, brillant, il décharge d'un coup toute l'énergie qu'il a accumulée en marchant dans les couloirs du métro.";
            d["archives.hero.rempart"] = "REMPART est une forteresse sur deux jambes. Il protège les autres avant lui-même, et c'est un peu son problème.";
            d["archives.hero.seve"] = "SÈVE fait pousser ce qui guérit. Les ronces de ses mains referment les plaies comme elles ouvrent les gardes.";
            d["archives.hero.etincelle"] = "ÉTINCELLE vient de découvrir son pouvoir : de petites décharges qu'elle manie encore avec prudence. On dit qu'elle ira loin.";
            d["archives.zone.zone_ny_1_1"] = "Red Hook, les quais : la bande de Brick tenait les docks et le terminal. Le générateur est coupé, la grue est à toi. Le quartier respire.";
            d["archives.zone.zone_ny_1_2"] = "Gowanus, le canal : la crue a noyé les entrepôts, le gang du canal en a profité. Une famille a pu être évacuée du deuxième étage.";
            d["archives.zone.zone_ny_1_3"] = "Dumbo, le pont : le Titan du pont ne laissait plus rien passer. Maintenant que les câbles sont rétablis, la ville est de nouveau reliée.";
            d["archives.enemy.thug"] = "Voyou : le fantassin des gangs. Poings, barres de fer et beaucoup de bruit. Dangereux en nombre.";
            d["archives.enemy.brute"] = "Cogneur : lent mais solide, il provoque ses adversaires et encaisse. Il faut passer à travers sa garde.";
            d["archives.enemy.street_pyro"] = "Pyro de rue : cocktails et jets de flammes. Ses brûlures rongent l'équipe si on le laisse faire.";
            d["archives.enemy.brick"] = "BRICK : chef de la bande de Red Hook, un mur de briques vivant. Son onde de choc secoue toute l'escouade ; il tombe pourtant comme les autres.";
            d["archives.enemy.sniper"] = "Tireur d'élite : un fusil, du sang-froid. Il marque une cible avant de la viser à nouveau.";
            d["archives.enemy.canal_chief"] = "Chef du canal : il dirige le gang de Gowanus. Sa vague d'acide ronge l'armure de toute l'escouade.";
            d["archives.enemy.bridge_titan"] = "TITAN DU PONT : un colosse qui a avalé la structure du pont. Il casse les câbles et frappe toute l'équipe.";
        }
    }
}
