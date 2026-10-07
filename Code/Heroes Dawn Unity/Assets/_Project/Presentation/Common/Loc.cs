#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>
    /// Textes FR du prototype (le M2 passera par Unity Localization / fr.json).
    /// Une clé absente est affichée telle quelle entre crochets pour être repérée facilement.
    /// </summary>
    public static partial class Loc
    {
        static readonly Dictionary<string, string> Fr = Extended(new Dictionary<string, string>
        {
            // --- Général ---
            ["game.title"] = "HEROES DAWN",
            ["game.studio"] = "STUDIO N29",
            ["game.tagline"] = "New York tient encore. Pour combien de temps ?",
            ["game.build"] = "Prototype M1 · réseau simulé",
            ["ui.ok"] = "OK", ["ui.cancel"] = "Annuler", ["ui.confirm"] = "Confirmer", ["ui.continue"] = "Continuer",
            ["ui.close"] = "Fermer", ["ui.back"] = "Retour", ["ui.credits"] = "Crédits", ["ui.pr"] = "PR",
            ["ui.level"] = "Niv. {0}", ["ui.fatigue"] = "Fatigue {0} %", ["ui.saving"] = "Sauvegarde…",

            // --- Connexion ---
            ["login.play"] = "JOUER EN INVITÉ",
            ["login.connecting"] = "Connexion aux serveurs N29…",
            ["login.loading"] = "Chargement du profil…",
            ["login.reset"] = "Effacer la sauvegarde locale",
            ["login.reset.title"] = "Effacer la sauvegarde ?",
            ["login.reset.body"] = "Ta progression locale sera supprimée. Le profil invité repartira de zéro.",
            ["login.welcome_new"] = "Bienvenue, {0} ! Ton escouade t'attend au QG.",
            ["login.welcome_back"] = "Bon retour, {0} !",
            ["login.recovered"] = "Ton équipe s'est reposée : -{0} points de fatigue au total.",
            ["login.fail_network"] = "Connexion impossible (réseau simulé)",

            // --- QG ---
            ["hub.title"] = "QG — RED HOOK",
            ["hub.tab.team"] = "ÉQUIPE",
            ["hub.tab.recruit"] = "BOUTIQUE",
            ["hub.ops"] = "SALLE DES OPÉRATIONS",
            ["hub.squad"] = "Escouade ({0}/4)",
            ["hub.squad.in"] = "DANS L'ESCOUADE",
            ["hub.squad.add"] = "AJOUTER",
            ["hub.train"] = "ENTRAÎNER · {0}",
            ["hub.train.max"] = "NIVEAU MAX",
            ["hub.recruit"] = "RECRUTER · {0} PR",
            ["hub.heal"] = "INFIRMERIE · -30 % · {0}",
            ["hub.deploy"] = "DÉPLOYER",
            ["hub.deploy.exhausted"] = "Escouade épuisée : passe par l'infirmerie.",
            ["hub.zone.done"] = "Zone sécurisée — rejouable (récompense réduite)",
            ["hub.zone.new"] = "3 objectifs · brouillard de guerre · Élite : Brick",
            ["hub.logout"] = "Déconnexion",
            ["hub.recruited"] = "{0} rejoint l'équipe !",
            ["shop.title"] = "Boutique éphémère · 3 héros par jour",
            ["shop.countdown.h"] = "Nouveaux héros dans {0} h {1:00}",
            ["shop.countdown.m"] = "Nouveaux héros dans {0} min",
            ["shop.reroll.free"] = "RENOUVELER · GRATUIT",
            ["shop.reroll.cost"] = "RENOUVELER · {0} CR",
            ["shop.reroll.confirm.title"] = "Renouveler la boutique ?",
            ["shop.reroll.confirm.body"] = "3 nouveaux héros pour {0} Crédits.",
            ["shop.recruited"] = "RECRUTÉ",
            ["shop.empty"] = "Collection de New York complète !",
            // --- Coque du hub ---
            ["nav.qg"] = "QG", ["nav.team"] = "ÉQUIPE", ["nav.recruit"] = "RECRUTER", ["nav.pass"] = "PASS", ["nav.shop"] = "BOUTIQUE",
            ["nav.back"] = "RETOUR", ["nav.settings"] = "RÉGLAGES",
            ["hub.quit.title"] = "Quitter le jeu ?", ["hub.quit.body"] = "Ta progression est sauvegardée.", ["hub.quit.ok"] = "Quitter",
            ["page.qg"] = "QG · Section New York", ["page.team"] = "Équipe · {0} membres", ["page.recruit"] = "Recrutement · Boutique du jour",
            ["page.pass"] = "Pass · Saison 1", ["page.shop"] = "Boutique", ["page.salon"] = "Salon d'entraînement", ["page.infirmary"] = "Infirmerie",
            ["page.operations"] = "Salle des opérations", ["page.lab"] = "Labo de recherche", ["page.archives"] = "Archives",
            ["page.settings"] = "Paramètres", ["page.member_sheet"] = "Fiche de membre",
            // --- QG (M10) ---
            ["qg.rooms"] = "LES PIÈCES DU QG", ["qg.daily"] = "MISSIONS DU JOUR", ["qg.daily.body"] = "Trois missions par jour pour faire avancer le pass.",
            ["qg.alerts"] = "ALERTES", ["qg.secondary"] = "Objectifs secondaires en temps réel", ["qg.fil"] = "FIL DU QG",
            ["qg.salon.one"] = "1 membre à entraîner", ["qg.salon.many"] = "{0} membres à entraîner", ["qg.salon.none"] = "Toute l'équipe est au niveau max",
            ["qg.infirmary.one"] = "1 membre fatigué", ["qg.infirmary.many"] = "{0} membres fatigués", ["qg.infirmary.none"] = "Toute l'équipe est en forme",
            ["qg.fil.speaker"] = "RAYNER",
            ["qg.fil.zone_ny_1_1"] = "La bande de Brick tient les quais de Red Hook. Coupe le générateur, atteins la grue du terminal, puis fais tomber Brick à l'entrepôt.",
            ["qg.fil.done"] = "Red Hook est sécurisé. Entraîne l'équipe et repose-la : la fracture ne dort jamais.",
            ["qg.mission"] = "MISSION",
            // --- Équipe (M20) et fiche (M21) ---
            ["team.filter.all"] = "TOUS", ["team.sort"] = "TRI", ["team.sort.level"] = "NIVEAU", ["team.sort.rarity"] = "RARETÉ", ["team.sort.fatigue"] = "FATIGUE",
            ["team.exhausted"] = "ÉPUISÉ", ["team.squad.remove"] = "RETIRER",
            ["team.combos"] = "COMBOS DE L'ESCOUADE", ["team.combos.none"] = "Aucun combo : mélange les familles.", ["team.combo_line"] = "{0} · {1} + {2}",
            ["team.mission"] = "PRÉPARER UNE MISSION",
            ["sheet.skills"] = "COMPÉTENCES", ["sheet.combos"] = "COMBOS DE LA FAMILLE", ["sheet.combo_line"] = "{0} · avec {1}",
            ["sheet.previous"] = "PRÉC.", ["sheet.next"] = "SUIV.", ["sheet.salon"] = "SALON", ["sheet.infirmary"] = "INFIRMERIE",
            ["hub.train.missing"] = "Il manque {0} CR",
            // --- Salon, Infirmerie, Paramètres ---
            ["salon.simple"] = "Version simple : l'entraînement est immédiat. Les séances longues arriveront avec le Salon complet.",
            ["infirmary.simple"] = "Version simple : un soin pour toute l'équipe, -30 % de fatigue chacun.",
            ["infirmary.band.fit"] = "EN FORME", ["infirmary.band.tired"] = "FATIGUÉ", ["infirmary.band.exhausted"] = "ÉPUISÉ",
            ["settings.account"] = "COMPTE", ["settings.audio"] = "AUDIO", ["settings.graphics"] = "GRAPHISMES",
            ["settings.accessibility"] = "ACCESSIBILITÉ", ["settings.notifications"] = "NOTIFICATIONS", ["settings.controls"] = "CONTRÔLES",
            ["settings.player"] = "{0} · {1}", ["settings.logout"] = "SE DÉCONNECTER", ["settings.reset"] = "EFFACER LA SAUVEGARDE",
            ["settings.reset.hint"] = "Efface ta progression et recommence une partie neuve. Irréversible.",
            ["settings.reset.done"] = "Sauvegarde effacée : nouvelle partie.",
            // --- Recruter (M30 adapté) et confirmation (M31) ---
            ["recruit.rule"] = "3 héros chaque jour à 9h00. Les Légendes sont rares : ne les laisse pas filer.",
            ["recruit.in_team"] = "DANS L'ÉQUIPE", ["recruit.progress"] = "Collection : {0} / {1}",
            ["recruit.confirm.title"] = "Recruter {0} ?", ["recruit.confirm.ok"] = "RECRUTER",
            ["recruit.confirm.before"] = "Solde actuel", ["recruit.confirm.cost"] = "Coût", ["recruit.confirm.after"] = "Après l'achat",
            ["recruit.missing"] = "Il manque {0} PR",
            ["recruit.how_to_earn"] = "Gagne des PR en sécurisant des zones et en remplissant leurs objectifs.",
            ["recruit.combo"] = "Combo phare : {0} avec {1}", ["recruit.combo.family"] = "Combo phare : {0}",
            // --- Fonctions à venir (WIP) ---
            ["wip.soon"] = "BIENTÔT",
            ["wip.toast"] = "{0} arrive bientôt",
            ["wip.pass.title"] = "Le pass saisonnier", ["wip.pass.planned"] = "Saisons de 6 semaines, 50 paliers, piste gratuite et piste premium, missions du jour.",
            ["wip.shop.title"] = "La boutique", ["wip.shop.planned"] = "Offres, Gemmes, PR, accélérateurs et cosmétiques (tenues, décor du QG, dés et pions). Pas de pay-to-win, pas de loot box.",
            ["wip.operations.title"] = "La salle des opérations", ["wip.operations.planned"] = "Expéditions de 1 h à 12 h : destination, membres disponibles, gains et fatigue annoncés avant l'envoi.",
            ["wip.lab.title"] = "Le labo de recherche", ["wip.lab.planned"] = "Production horaire de PR, stock à collecter, amélioration de la capacité.",
            ["wip.archives.title"] = "Les archives", ["wip.archives.planned"] = "Collection des recrues, lore, scènes débloquées, musiques et succès.",
            ["wip.daily_missions.title"] = "Les missions du jour", ["wip.daily_missions.planned"] = "Trois missions par jour, réclamables d'un geste, qui font progresser le pass.",
            ["wip.secondary_objectives.title"] = "Les objectifs secondaires", ["wip.secondary_objectives.planned"] = "Objectifs en temps réel dans les zones déjà validées, avec compte à rebours.",
            ["wip.gems.title"] = "Les Gemmes", ["wip.gems.planned"] = "Monnaie premium : elle ne fera gagner que du temps et des cosmétiques.",
            ["wip.member_art.title"] = "L'illustration en pied", ["wip.member_art.planned"] = "Chaque membre aura son illustration complète sur sa fiche.",
            ["wip.settings_audio.title"] = "Les réglages audio", ["wip.settings_audio.planned"] = "Volume général, musique, effets et voix, avec aperçu sonore immédiat.",
            ["wip.settings_graphics.title"] = "Les réglages graphiques", ["wip.settings_graphics.planned"] = "Qualité en 4 niveaux, 30 ou 60 images par seconde, économie de batterie.",
            ["wip.settings_accessibility.title"] = "L'accessibilité", ["wip.settings_accessibility.planned"] = "Taille du texte, mode daltonien, réduction des flashs et des secousses, temps étendu des mini-jeux, sous-titres.",
            ["wip.settings_notifications.title"] = "Les notifications", ["wip.settings_notifications.planned"] = "Un interrupteur par type de notification et des heures calmes.",
            ["wip.settings_controls.title"] = "Les contrôles", ["wip.settings_controls.planned"] = "Raccourcis clavier modifiables et réglages de la caméra du plateau (PC).",
            ["hub.trained"] = "{0} passe niveau {1} !",
            ["hub.healed"] = "L'infirmerie a remis l'équipe sur pied.",
            ["hub.stats"] = "PV {0} · ATQ {1} · DÉF {2} · VIT {3}",

            // --- Plateau ---
            ["board.roll"] = "LANCER !",
            ["board.bonus"] = "DÉ BONUS ({0})",
            ["board.end_move"] = "TERMINER",
            ["board.pm"] = "{0} PM",
            ["board.reserve"] = "Réserve : {0}/{1} PM",
            ["board.back_hq"] = "Retour QG",
            ["board.back_hq.title"] = "Quitter la zone ?",
            ["board.back_hq.body"] = "Le brouillard se refermera et la partie sera perdue. Les gains déjà obtenus sont conservés.",
            ["board.objectives"] = "OBJECTIFS",
            ["board.hint.roll"] = "Lance le dé pour avancer.",
            ["board.hint.choose"] = "Choisis une case en surbrillance (ou termine le déplacement).",
            ["board.hint.done"] = "Zone sécurisée !",
            ["board.resource"] = "+{0} Crédits · +{1} PR",
            ["board.fog"] = "Nouveau secteur en vue !",
            ["board.banked"] = "{0} PM mis en réserve",
            ["board.bonus_ready"] = "Dé bonus disponible !",
            ["board.objective_done"] = "Objectif rempli : {0}",
            ["board.stepped_back"] = "L'équipe recule d'une case.",
            ["board.zone_complete"] = "ZONE SÉCURISÉE !",
            ["board.zone_rewards"] = "+{0} Crédits · +{1} PR",
            ["board.zone_rewards.replay"] = "Zone déjà sécurisée : +{0} Crédits",
            ["board.to_hq"] = "Retour au QG",
            ["board.legend"] = "VS combat · MJ mini-jeu · ! événement · $ ressource · Zz repos",

            // --- Combat ---
            ["combat.auto"] = "AUTO", ["combat.speed"] = "x{0}", ["combat.retreat"] = "RETRAITE",
            ["combat.retreat.title"] = "Battre en retraite ?",
            ["combat.retreat.body"] = "Le combat compte comme une défaite : l'équipe recule d'une case et prend de la fatigue.",
            ["combat.combo"] = "COMBO !",
            ["combat.choose_target"] = "Choisis une cible",
            ["combat.choose_ally"] = "Choisis un allié",
            ["combat.waiting"] = "Les jauges se remplissent…",
            ["combat.ready"] = "{0} est prêt !",
            ["combat.victory"] = "VICTOIRE !", ["combat.defeat"] = "DÉFAITE…",
            ["combat.reward"] = "+{0} Crédits",
            ["combat.fatigue"] = "Fatigue de l'escouade",
            ["combat.to_hq"] = "Escouade épuisée : retour instantané au QG.",
            ["combat.verify.pending"] = "Vérification serveur (simulée)…",
            ["combat.verify.ok"] = "Combat vérifié par le serveur (rejeu identique)",
            ["combat.verify.ko"] = "Rejeu divergent : résultat refusé en ligne !",
            ["combat.verify.skip"] = "Vérification ignorée (debug)",
            ["combat.debug_win"] = "DEBUG : GAGNER",
            ["combat.miss"] = "RATÉ", ["combat.crit"] = "CRITIQUE !", ["combat.skip"] = "SONNÉ",
            ["combat.rank"] = "Combat {0}",
            ["combat.cooldown"] = "{0} t",

            // --- Cases ---
            ["case.Entry"] = "Entrée", ["case.Empty"] = "Rue", ["case.Combat"] = "Combat", ["case.MiniGame"] = "Mini-jeu",
            ["case.Narrative"] = "Événement", ["case.Resource"] = "Ressource", ["case.Rest"] = "Repos",
            ["rank.Easy"] = "Facile", ["rank.Normal"] = "Normal", ["rank.Elite"] = "Élite", ["rank.Boss"] = "Boss",

            ["rest.title"] = "PLANQUE",
            ["rest.body"] = "Un entrepôt abandonné, au calme. Ton escouade peut souffler : -30 % de fatigue.",
            ["rest.take"] = "SE REPOSER", ["rest.skip"] = "Passer",

            ["mg.stop"] = "STOP !",
            ["mg.rules"] = "Arrête le curseur dans la zone verte. 2 réussites sur 3 essais.",
            ["mg.score"] = "Réussites : {0}/2 · Essais restants : {1}",
            ["mg.success"] = "RÉUSSI ! +{0} Crédits · +{1} PR",
            ["mg.fail"] = "ÉCHEC… Reviens sur cette case pour réessayer.",
            ["mg.n08.title"] = "GÉNÉRATEUR DU QUAI",
            ["mg.n08.body"] = "Le générateur alimente les projecteurs des contrebandiers. Coupe les circuits au bon moment.",
            ["mg.n15.title"] = "TERMINAL DE SÉCURITÉ",
            ["mg.n15.body"] = "Un terminal de la capitainerie. Force les pare-feu un par un.",
            ["mg.n21.title"] = "GRUE BLOQUÉE",
            ["mg.n21.body"] = "Une grue bloque le passage. Relance son moteur en synchronisant les pistons.",
            ["mg.default.title"] = "ÉPREUVE", ["mg.default.body"] = "Montre ce que ton escouade sait faire.",

            ["ev.choice.credits"] = "Fouiller les lieux (+{0} Crédits)",
            ["ev.choice.research"] = "Récupérer les données (+{0} PR)",
            ["ev.choice.morale"] = "Aider les civils (-{0} % fatigue)",
            ["ev.n05.title"] = "LE DOCKER INQUIET",
            ["ev.n05.body"] = "« Ils déchargent des caisses la nuit, sans manifeste. Depuis, plus personne ne traîne sur le quai 9. » Le vieux docker te fixe, à moitié rassuré de voir des capes dans le coin.",
            ["ev.n13.title"] = "RADIO PIRATE",
            ["ev.n13.body"] = "Un émetteur bricolé crache des coordonnées en boucle. Quelqu'un coordonne les pillages du port depuis ce toit.",
            ["ev.n20.title"] = "CONTENEUR PIÉGÉ",
            ["ev.n20.body"] = "Des coups sourds résonnent dans un conteneur scellé. Des civils y ont été enfermés par la bande de Brick.",
            ["ev.default.title"] = "ÉVÉNEMENT", ["ev.default.body"] = "Quelque chose se passe ici.",

            // --- Zones & objectifs ---
            ["zones.ny_1_1.name"] = "RED HOOK — LES QUAIS",
            ["zones.ny_1_1.code"] = "Zone 1.1",
            ["zones.ny_1_1.desc"] = "La bande de Brick a pris le contrôle des docks. Sécurise le quartier avant que la fracture ne s'étende.",
            ["objectives.ny11.warehouse"] = "Vaincre Brick à l'entrepôt",
            ["objectives.ny11.generator"] = "Couper le générateur du quai",
            ["objectives.ny11.crane"] = "Atteindre la grue du terminal",

            // --- Familles, rôles, raretés ---
            ["family.Matter"] = "Matière", ["family.Elemental"] = "Élémentaire", ["family.PhysicalBio"] = "Physique / Bio", ["family.Perception"] = "Perception",
            ["role.Tank"] = "Tank", ["role.Damage"] = "Dégâts", ["role.Control"] = "Contrôle", ["role.Support"] = "Soutien", ["role.Precision"] = "Précision",
            ["rarity.Agent"] = "Agent", ["rarity.Elite"] = "Élite", ["rarity.Legend"] = "Légende",

            // --- Membres ---
            ["members.ancre.name"] = "Ancre", ["members.verre.name"] = "Verre", ["members.givre.name"] = "Givre", ["members.echo.name"] = "Écho",
            ["members.arc.name"] = "Arc", ["members.rempart.name"] = "Rempart", ["members.seve.name"] = "Sève", ["members.etincelle.name"] = "Étincelle",

            // --- Ennemis ---
            ["enemies.thug.name"] = "Voyou", ["enemies.brute.name"] = "Cogneur", ["enemies.street_pyro.name"] = "Pyro de rue", ["enemies.brick.name"] = "BRICK",

            // --- Compétences ---
            ["skills.ancre_a1"] = "Coup d'amarre", ["skills.ancre_a2"] = "Ancrage", ["skills.ancre_a3"] = "Mur vivant",
            ["skills.verre_a1"] = "Éclat", ["skills.verre_a2"] = "Cage de verre", ["skills.verre_a3"] = "Prisme protecteur",
            ["skills.givre_a1"] = "Morsure", ["skills.givre_a2"] = "Souffle polaire", ["skills.givre_a3"] = "Zéro absolu",
            ["skills.echo_a1"] = "Balise sonar", ["skills.echo_a2"] = "Onde révélatrice", ["skills.echo_a3"] = "Résonance brisée",
            ["skills.arc_a1"] = "Arc électrique", ["skills.arc_a2"] = "Surcharge", ["skills.arc_a3"] = "Tempête d'arcs",
            ["skills.rempart_a1"] = "Charge", ["skills.rempart_a2"] = "Garde", ["skills.rempart_a3"] = "Bastion",
            ["skills.seve_a1"] = "Épine", ["skills.seve_a2"] = "Greffe", ["skills.seve_a3"] = "Floraison",
            ["skills.etincelle_a1"] = "Décharge", ["skills.etincelle_a2"] = "Étincelle vitale", ["skills.etincelle_a3"] = "Court-circuit",
            ["skills.thug_a1"] = "Coup de poing", ["skills.thug_a2"] = "Barre de fer",
            ["skills.brute_a1"] = "Écrasement", ["skills.brute_a2"] = "Provocation",
            ["skills.pyro_a1"] = "Jet de flammes", ["skills.pyro_a2"] = "Cocktail",
            ["skills.brick_a1"] = "Direct du mur", ["skills.brick_a2"] = "Mur de briques", ["skills.brick_a3"] = "Onde de choc",

            // --- Combos ---
            ["combos.charged_projectiles.name"] = "Projectiles chargés", ["combos.forged_armor.name"] = "Armure forgée",
            ["combos.trap.name"] = "Piège", ["combos.elemental_body.name"] = "Corps élémentaire", ["combos.weak_point.name"] = "Point faible",
            ["combos.triage.name"] = "Triage", ["combos.resonance_matter.name"] = "Résonance de la matière",
            ["combos.resonance_elemental.name"] = "Résonance élémentaire", ["combos.resonance_physical.name"] = "Résonance vitale",
            ["combos.resonance_perception.name"] = "Résonance perceptive",

            // --- Statuts ---
            ["status.Burn"] = "Brûlure", ["status.Freeze"] = "Gel", ["status.Shock"] = "Choc", ["status.Corrosion"] = "Corrosion",
            ["status.Blind"] = "Aveuglé", ["status.Vulnerable"] = "Vulnérable", ["status.Bind"] = "Entrave", ["status.Mark"] = "Marque",
            ["status.Taunt"] = "Provocation", ["status.Shield"] = "Bouclier", ["status.Regen"] = "Régén.", ["status.Haste"] = "Hâte", ["status.Evade"] = "Esquive",

            // --- Cibles & effets (descriptions générées) ---
            ["target.SingleEnemy"] = "1 ennemi", ["target.AllEnemies"] = "Tous les ennemis", ["target.SingleAlly"] = "1 allié",
            ["target.AllAllies"] = "Toute l'équipe", ["target.Self"] = "Soi-même",
            ["fx.damage"] = "Dégâts {0} %", ["fx.heal"] = "Soin {0} % PV", ["fx.shield"] = "Bouclier {0} % PV",
            ["fx.gauge_up"] = "Jauge +{0} %", ["fx.gauge_down"] = "Jauge -{0} %",
            ["fx.status"] = "{0} {1} t", ["fx.status_chance"] = "{0} {1} t ({2} %)",

            // --- Erreurs ---
            ["errors.network"] = "Serveur injoignable. Réessaie.",
            ["errors.navigation"] = "Navigation impossible, réessaie.",
            ["errors.profile_load"] = "Impossible de charger le profil.",
            ["errors.not_enough_credits"] = "Pas assez de Crédits.",
            ["errors.not_enough_pr"] = "Pas assez de PR.",
            ["errors.already_owned"] = "Déjà dans l'équipe.",
            ["errors.unknown_member"] = "Membre inconnu.",
            ["errors.shop.expired"] = "La boutique s'est renouvelée.",
            ["errors.shop.slot"] = "Offre introuvable.",
            ["errors.shop.recruited"] = "Héros déjà recruté.",
            ["errors.max_level"] = "Niveau maximum atteint.",
            ["errors.squad_full"] = "L'escouade compte déjà 4 membres.",
            ["errors.squad_empty"] = "L'escouade doit garder au moins 1 membre.",
            ["errors.nothing_to_heal"] = "Personne n'a besoin de soins.",
            ["errors.in_mission"] = "Impossible pendant une mission.",
            ["errors.no_run"] = "Aucune zone en cours.",
            ["errors.board.not_allowed"] = "Action impossible pour le moment.",
            ["errors.squad_exhausted"] = "Toute l'escouade est épuisée.",
            ["errors.unknown_zone"] = "Zone inconnue.",
            ["errors.no_combat"] = "Aucun combat en attente.",
        });

        /// <summary>Les textes des fonctions ajoutées (un fichier partiel par fonction) complètent le dictionnaire de base.</summary>
        static Dictionary<string, string> Extended(Dictionary<string, string> d)
        {
            ExtendMap(d);
            ExtendCombat(d);
            ExtendMeta(d);
            ExtendTutorial(d);
            ExtendSettings(d);
            ExtendBoard(d);
            return d;
        }

        static partial void ExtendBoard(Dictionary<string, string> d);
        static partial void ExtendMap(Dictionary<string, string> d);
        static partial void ExtendCombat(Dictionary<string, string> d);
        static partial void ExtendMeta(Dictionary<string, string> d);
        static partial void ExtendTutorial(Dictionary<string, string> d);
        static partial void ExtendSettings(Dictionary<string, string> d);

        public static string T(string key) => Fr.TryGetValue(key, out var v) ? v : "[" + key + "]";
        public static bool Has(string key) => Fr.ContainsKey(key);
        public static string F(string key, params object[] args) => string.Format(T(key), args);
        public static string Or(string key, string fallbackKey) => Has(key) ? T(key) : T(fallbackKey);

        public static string Error(DomainError? e) => e == null ? "" : T(e.LocKey);
        public static string MemberName(MemberDef d) => T(d.NameKey);
        public static string EnemyName(EnemyDef d) => T(d.NameKey);
        public static string SkillName(SkillDef s) => Or("skills." + s.Id.Value, "skills." + s.Id.Value);
        public static string ComboName(ComboDef c) => T(c.NameKey);
        public static string StatusName(StatusType s) => T("status." + s);
        public static string FamilyName(Family f) => T("family." + f);
        public static string RoleName(Role r) => T("role." + r);
        public static string RarityName(Rarity r) => T("rarity." + r);
        public static string RankName(CombatRank r) => T("rank." + r);
        public static string CaseName(CaseType t) => T("case." + t);
    }
}
