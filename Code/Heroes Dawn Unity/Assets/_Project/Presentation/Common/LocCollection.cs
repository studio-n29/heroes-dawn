#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes des objets, du grimoire (cartes, boosters) et de l'équipement.</summary>
    public static partial class Loc
    {
        static partial void ExtendCollection(Dictionary<string, string> d)
        {
            // Objets
            d["items.kevlar_vest.name"] = "Gilet kevlar";
            d["items.steel_knuckles.name"] = "Poings d'acier";
            d["items.running_shoes.name"] = "Chaussures de course";
            d["items.riot_shield.name"] = "Bouclier anti-émeute";
            d["items.adrenaline_patch.name"] = "Patch d'adrénaline";
            d["items.titan_gauntlets.name"] = "Gantelets Titan";
            d["items.reactive_plating.name"] = "Plaques réactives";
            d["items.phoenix_core.name"] = "Cœur de Phénix";
            d["items.time_shard.name"] = "Éclat temporel";
            d["items.apex_module.name"] = "Module Apex";
            d["items.power_gauntlet.name"] = "Gantelet de puissance";
            d["items.arcane_focus.name"] = "Focalisateur arcanique";
            // Cartes (noms de compétence)
            d["skills.card_glass_wall"] = "Mur de verre";
            d["skills.card_shackle"] = "Entrave";
            d["skills.card_prism_burst"] = "Éclat prismatique";
            d["skills.card_ember"] = "Braise";
            d["skills.card_storm"] = "Tempête";
            d["skills.card_inferno"] = "Brasier";
            d["skills.card_second_wind"] = "Second souffle";
            d["skills.card_bulwark_charge"] = "Charge du rempart";
            d["skills.card_regen_field"] = "Champ régénérant";
            d["skills.card_mark"] = "Marque";
            d["skills.card_precog"] = "Précognition";
            d["skills.card_time_rift"] = "Faille temporelle";
            d["boosters.booster_standard.name"] = "Booster de cartes";

            // Raretés, effets
            d["item.rarity.Common"] = "Commun";
            d["item.rarity.Rare"] = "Rare";
            d["item.rarity.Epic"] = "Épique";
            d["item.rarity.Legendary"] = "Légendaire";
            d["item.fx.hp"] = "PV +{0} %";
            d["item.fx.atk"] = "ATQ +{0} %";
            d["item.fx.def"] = "DEF +{0} %";
            d["item.fx.spd"] = "VIT +{0} %";
            d["item.fx.start_gauge"] = "Départ +{0} % sur la frise";
            d["item.fx.opening_shield"] = "Bouclier {0} % au début";
            d["item.fx.crit"] = "Critique +{0} %";
            d["item.fx.skill_power"] = "Compétences +{0} %";

            // Fiche d'agent et page Équipement
            d["sheet.loadout"] = "ÉQUIPEMENT";
            d["sheet.loadout_counts"] = "ÉQUIPEMENT · objets {0}/{1} · cartes {2}/{3}";
            d["hub.avatar_role"] = "CHEF D’UNITÉ";
            d["loadout.badge_equipped"] = "ÉQUIPÉ";
            d["sheet.level_bar"] = "NIV. {0} / {1}";
            d["sheet.tiers"] = "PALIERS PASSIFS";
            d["sheet.tier_passive"] = "+{0} % {1}";
            d["sheet.tier_level"] = "NIV. {0}";
            d["sheet.tier_stats"] = "PV {0} · ATQ {1} · DÉF {2}";
            d["sheet.affinity"] = "AGENTS À FORTE AFFINITÉ";
            d["affinity.title"] = "Affinités de {0}";
            d["affinity.subtitle"] = "Chaque duo donne un seul bonus quand les deux agents sont dans l’escouade.";
            d["affinity.empty"] = "Aucun autre agent dans ton équipe pour l’instant.";
            d["affinity.link"] = "Lien niv. {0} · {1} victoire(s)";
            d["affinity.none"] = "Aucune synergie";
            d["sheet.skill_fixed"] = "COMPÉTENCE {0}";
            d["sheet.skill_card"] = "CARTE {0}";
            d["sheet.item_slot"] = "OBJET {0}";
            d["sheet.slot_empty"] = "VIDE";
            d["sheet.slot_locked"] = "VERROUILLÉ";
            d["sheet.item_locked_toast"] = "Un troisième emplacement d'objet arrivera bientôt.";
            d["sheet.picker_items"] = "OBJETS DISPONIBLES · EMPLACEMENT {0}";
            d["sheet.picker_cards"] = "CARTES DISPONIBLES · EMPLACEMENT {0}";
            d["sheet.picker_hint"] = "Touche un élément pour l'équiper.";
            d["sheet.picker_remove"] = "RETIRER";
            d["sheet.picker_close"] = "FERMER";
            d["page.loadout"] = "Équipement";
            d["page.grimoire"] = "Grimoire";
            d["nav.profile"] = "PROFIL";
            d["page.salon"] = "Salon des agents";
            d["page.infirmary"] = "Salon des agents";
            d["lounge.header"] = "SALON DES AGENTS";
            d["lounge.line"] = "{0} agent(s) · {1} fatigué(s) — entraîne-les ou soigne toute l’équipe.";
            d["lounge.row"] = "Niv. {0} · {1}";
            d["page.inventory"] = "Inventaire";
            d["inventory.header"] = "TES OBJETS";
            d["inventory.line"] = "{0} objet(s) · {1} équipé(s)";
            d["inventory.team"] = "ÉQUIPE";
            d["inventory.worn"] = "Équipé sur : {0}";
            d["inventory.count"] = "×{0}";
            d["inventory.empty"] = "Aucun objet";
            d["inventory.empty_sub"] = "Gagne des combats : les objets se trouvent sur les ennemis vaincus.";
            d["qg.grimoire.info"] = "{0} / {1} cartes";
            d["qg.inventory.info"] = "{0} objet(s)";
            d["qg.agents.info"] = "{0} / {1} agents";
            d["loadout.header"] = "Équipement de {0}";
            d["loadout.line"] = "Objets {0}/{1} · Cartes {2}/{3} · {4}";
            d["loadout.grimoire"] = "GRIMOIRE";
            d["loadout.items_title"] = "OBJETS ÉQUIPÉS";
            d["loadout.cards_title"] = "CARTES ÉQUIPÉES (compétences 3 et 4)";
            d["loadout.stock_items"] = "OBJETS EN RÉSERVE";
            d["loadout.stock_cards"] = "CARTES EN RÉSERVE";
            d["loadout.free_item"] = "Emplacement libre";
            d["loadout.free_item_sub"] = "Équipe un objet de ta réserve.";
            d["loadout.free_card"] = "Carte {0} : libre";
            d["loadout.free_card_sub"] = "Compétence {0} ; équipe une carte de la catégorie {1}.";
            d["loadout.locked_card_sub"] = "Équipe d'abord la carte 3.";
            d["loadout.card_slot"] = "Carte {0} · {1}";
            d["loadout.remove"] = "RETIRER";
            d["loadout.equip"] = "ÉQUIPER";
            d["loadout.to_slot"] = "→ {0}";
            d["loadout.none_items"] = "Aucun objet en réserve. Cherche-en en boutique ou en événement.";
            d["loadout.none_cards"] = "Aucune carte compatible. Ouvre des boosters dans le Grimoire.";
            d["loadout.copies"] = "×{0}";
            d["loadout.no_slot_reason"] = "Emplacements pleins : retire un objet.";
            d["loadout.slot_reason"] = "Carte 3 d'abord.";
            d["loadout.equipped"] = "{0} équipé.";
            d["loadout.removed"] = "{0} retiré.";
            d["loadout.stats_line"] = "Bonus : {0}";

            // Grimoire
            d["grimoire.header"] = "GRIMOIRE";
            d["grimoire.line"] = "{0} carte(s) possédée(s) sur {1} · {2} booster(s) en réserve · pitié {3}/{4}";
            d["grimoire.open"] = "OUVRIR UN BOOSTER";
            d["grimoire.open_none"] = "Aucun booster";
            d["grimoire.buy"] = "ACHETER · {0} PR";
            d["grimoire.buy_reason"] = "Il manque {0} PR.";
            d["grimoire.owned"] = "×{0}";
            d["grimoire.missing"] = "—";
            d["grimoire.card_line"] = "{0} · {1} · {2}";
            d["grimoire.owned_level"] = "×{0} · Niv. {1}/{2}";
            d["grimoire.upgrade"] = "AMÉLIORER ({0} doublon(s) + {1} Cr)";
            d["grimoire.max_level"] = "NIVEAU MAX";
            d["grimoire.upgrade_copies"] = "Il faut {0} exemplaire(s) en doublon.";
            d["grimoire.upgrade_credits"] = "Il manque {0} Crédits.";
            d["grimoire.upgraded"] = "Carte améliorée !";
            d["grimoire.opened_title"] = "BOOSTER OUVERT !";
            d["grimoire.opened_body"] = "Tu obtiens :\n{0}";
            d["grimoire.opened_ok"] = "SUPER";
            d["grimoire.bought"] = "Booster acheté.";
            d["grimoire.new_card"] = "NOUVEAU";

            // Boutique d'objets, sources
            d["shop.items_title"] = "OBJETS DU JOUR";
            d["shop.item_price"] = "{0} CR";
            d["shop.item_sold"] = "VENDU";
            d["shop.item_bought"] = "{0} acheté.";
            d["shop.item_missing"] = "Il manque {0} CR.";
            d["shop.booster_row"] = "Acheter un booster";
            d["shop.booster_cards"] = "{0} cartes de compétences, dont une rare garantie régulièrement.";
            d["shop.booster_stock"] = "Boosters en réserve : {0}";
            d["shop.booster_open"] = "OUVRIR";
            d["shop.booster_none"] = "Aucun booster en réserve : achètes-en un ou gagne-en en jouant.";
            d["shop.gift_title"] = "Cadeau du jour";
            d["shop.gift_claim"] = "RÉCUPÉRER";
            d["shop.tab.heroes"] = "HÉROS";
            d["shop.tab.boosters"] = "BOOSTERS";
            d["shop.tab.items"] = "OBJETS";
            d["shop.tab.gems"] = "GEMMES ET CRÉDITS";
            d["shop.tab.gift"] = "CADEAU";
            d["booster.title"] = "OUVERTURE DU BOOSTER";
            d["booster.tap"] = "Touche pour ouvrir";
            d["booster.skip"] = "PASSER";
            d["shop.booster_sub"] = "3 cartes de compétences pour ton grimoire.";
            d["shop.grimoire"] = "GRIMOIRE";
            d["board.item_found"] = "Objet trouvé : {0}";
            d["board.booster_earned"] = "Booster de cartes gagné !";
            d["board.zone_booster"] = " · +{0} booster";

            // Erreurs
            d["errors.collection_member"] = "Agent introuvable.";
            d["errors.collection_unknown"] = "Objet ou carte inconnu.";
            d["errors.collection_no_slot"] = "Aucun emplacement libre.";
            d["errors.collection_same_type"] = "Cet agent porte déjà un objet de ce type.";
            d["equip.type_taken"] = "Type déjà porté";
            d["filters.all"] = "Toutes";
            d["grimoire.none_filtered"] = "Aucune carte pour ces filtres.";
            d["inventory.none_filtered"] = "Aucun objet pour ce filtre.";
            d["card.nature.offensive"] = "Offensive";
            d["card.nature.defensive"] = "Défensive";
            d["card.nature.support"] = "Soutien";
            d["item.type.weapon"] = "Arme";
            d["item.type.armor"] = "Armure";
            d["item.type.tool"] = "Outil";
            d["item.type.implant"] = "Implant";
            d["errors.collection_max_level"] = "Cette carte est déjà au niveau maximum.";
            d["errors.collection_copies"] = "Il manque des exemplaires en doublon.";
            d["errors.preset_empty"] = "Aucune composition enregistrée ici.";
            d["playtest.title"] = "TON AVIS COMPTE";
            d["playtest.intro"] = "Bravo, la zone 1.1 est terminée ! Note de 1 (pas du tout) à 5 (excellent), puis dis-nous tout.";
            d["playtest.q.fun"] = "C’était amusant ?";
            d["playtest.q.clarity"] = "Les écrans étaient clairs ?";
            d["playtest.q.difficulty"] = "La difficulté était juste ?";
            d["playtest.q.tutorial"] = "Le tutoriel t’a aidé ?";
            d["playtest.comment"] = "Ce qui t’a plu, gêné, ou les bugs vus :";
            d["playtest.comment_hint"] = "Écris ici…";
            d["playtest.send"] = "ENVOYER";
            d["playtest.later"] = "PLUS TARD";
            d["playtest.sent"] = "Merci ! Ton retour a bien été envoyé à l’équipe.";
            d["playtest.need_all"] = "Note les quatre questions.";
            d["playtest.thanks"] = "Merci ! Ton retour est copié : colle-le dans un message à l’équipe.";
            d["team.preset.filled"] = "{0} · {1} agents";
            d["team.preset.empty"] = "{0} · vide";
            d["team.preset.save"] = "SAUVER";
            d["team.preset.saving"] = "CHOISIS";
            d["team.preset.saved"] = "Composition {0} enregistrée.";
            d["team.preset.loaded"] = "Composition {0} chargée.";
            d["errors.preset_unavailable"] = "Aucun agent de cette composition n’est disponible.";
            d["errors.collection_not_owned"] = "Tu n'en as plus de libre en réserve.";
            d["errors.collection_family"] = "Cette carte ne va pas à la catégorie de cet agent.";
            d["errors.collection_first_card"] = "Équipe d'abord la carte de l'emplacement 3.";
            d["errors.collection_nothing"] = "Rien à retirer.";
            d["errors.collection_sold_out"] = "Offre déjà achetée aujourd'hui.";
            d["errors.collection_no_booster"] = "Aucun booster à ouvrir.";
            d["errors.collection_not_for_sale"] = "Non vendu.";
        }
    }
}
