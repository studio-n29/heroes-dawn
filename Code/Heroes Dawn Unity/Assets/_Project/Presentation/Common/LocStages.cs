#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes des activités jouables : consignes, HUD, potion (livre, ingrédients), dialogue des quais.</summary>
    public static partial class Loc
    {
        static partial void ExtendStages(Dictionary<string, string> d)
        {
            d["stage.quit"] = "ABANDONNER";
            d["stage.start"] = "COMMENCER";
            d["stage.won"] = "RÉUSSI !";
            d["stage.lost"] = "ÉCHEC…";
            d["stage.terrain.Snow"] = "neige"; d["stage.terrain.TallGrass"] = "herbe haute"; d["stage.terrain.Sand"] = "sable"; d["stage.terrain.ground"] = "sol";

            d["stage.instr.survival"] = "Traverse la neige, l'herbe haute et le sable jusqu'à l'objet marqué par la balise jaune. Ta jauge de survie baisse (plus vite dans la neige) : recharge-la près d'un totem.\n\nZQSD / flèches ou joystick pour te déplacer.";
            d["stage.instr.arena"] = "Choisis ton agent, puis vaincs toutes les vagues. Les ennemis chargent, tirent ou foncent après un préavis jaune : esquive !\n\nZQSD / flèches ou joystick pour te déplacer · Espace : attaque · Maj : compétence spéciale.";
            d["stage.choose_agent"] = "Choisis ton agent";

            d["stage.survival.time"] = "Temps : {0} s";
            d["stage.survival.target"] = "Objet à {0} m · cap {1} · {2}";
            d["stage.survival.totem"] = "Près du totem : la jauge se recharge";
            d["stage.survival.gauge"] = "SURVIE";
            d["stage.survival.cold"] = "GELÉ…";


            d["stage.arena.wave"] = "Vague {0} / {1}";
            d["stage.arena.enemies"] = "Ennemis : {0} · vaincus : {1}";
            d["stage.arena.hp"] = "PV {0} / {1}";
            d["stage.arena.wave_banner"] = "VAGUE {0} !";
            d["stage.skill.matter_slam"] = "COUP DE MASSE"; d["stage.skill.matter_shockwave"] = "ONDE DE CHOC";
            d["stage.skill.elemental_bolt"] = "ÉCLAT"; d["stage.skill.elemental_nova"] = "NOVA";
            d["stage.skill.perception_shot"] = "TIR PRÉCIS"; d["stage.skill.perception_volley"] = "RAFALE";
            d["stage.skill.physical_strike"] = "FRAPPE"; d["stage.skill.physical_charge"] = "CHARGE";
            d["stage.skill.matter_slam.detail"] = "Coup de masse + onde de choc"; d["stage.skill.elemental_bolt.detail"] = "Éclat + nova";
            d["stage.skill.perception_shot.detail"] = "Tir précis + rafale"; d["stage.skill.physical_strike.detail"] = "Frappe + charge";

            d["stage.potion.book_title"] = "LIVRE DE POTIONS";
            d["stage.potion.undo"] = "RETIRER"; d["stage.potion.clear"] = "VIDER"; d["stage.potion.brew"] = "PRÉPARER";
            d["stage.potion.quantity"] = "Quantité : {0}";
            d["stage.potion.attempts"] = "Essais restants : {0}";
            d["stage.potion.incomplete"] = "Ajoute toutes les étapes avant de préparer.";
            d["stage.potion.success"] = "La potion fait effet !";
            d["stage.potion.fail"] = "La potion est ratée…";
            d["stage.potion.fb.correct"] = "juste"; d["stage.potion.fb.toomuch"] = "trop dosé"; d["stage.potion.fb.toolittle"] = "pas assez dosé";
            d["stage.potion.fb.wrongplace"] = "mauvaise place"; d["stage.potion.fb.wrong"] = "pas dans la recette";
            d["potion.intro"] = "{0} souffre de {1}. Prépare la bonne potion avec les indices du livre.";
            d["potion.intro_nobody"] = "Un membre de l'équipe souffre de {0}. Prépare la bonne potion avec les indices du livre.";
            d["potion.hint.effect"] = "• {0} : {1}.";
            d["potion.hint.order"] = "• Étape {1} : {0}.";
            d["potion.hint.quantity"] = "• Dose de {0} : {1}.";
            d["potion.book.effects"] = "Propriétés des ingrédients";
            d["potion.book.recipe"] = "Ce que dit le livre de la recette";
            d["potion.ingredient.Heat"] = "Piment de braise\n<size=65%>chaleur</size>"; d["potion.ingredient.Cold"] = "Menthe glacée\n<size=65%>froid</size>";
            d["potion.ingredient.Calm"] = "Fleur de lotus\n<size=65%>calme</size>"; d["potion.ingredient.Vigor"] = "Racine de ginseng\n<size=65%>vigueur</size>";
            d["potion.ingredient.Antidote"] = "Herbe de sève\n<size=65%>antidote</size>"; d["potion.ingredient.Binding"] = "Résine collante\n<size=65%>liant</size>";
            d["potion.ingredient.Light"] = "Poussière d'étoile\n<size=65%>lumière</size>"; d["potion.ingredient.Earth"] = "Champignon des profondeurs\n<size=65%>terre</size>";
            d["potion.name.Heat"] = "Piment de braise"; d["potion.name.Cold"] = "Menthe glacée"; d["potion.name.Calm"] = "Fleur de lotus"; d["potion.name.Vigor"] = "Racine de ginseng";
            d["potion.name.Antidote"] = "Herbe de sève"; d["potion.name.Binding"] = "Résine collante"; d["potion.name.Light"] = "Poussière d'étoile"; d["potion.name.Earth"] = "Champignon des profondeurs";
            d["potion.tag.Heat"] = "chaleur"; d["potion.tag.Cold"] = "froid"; d["potion.tag.Calm"] = "calme"; d["potion.tag.Vigor"] = "vigueur";
            d["potion.tag.Antidote"] = "antidote"; d["potion.tag.Binding"] = "liant"; d["potion.tag.Light"] = "lumière"; d["potion.tag.Earth"] = "terre";

            d["stage.dialogue.progress"] = "Réplique {0} / {1}";
            d["stage.dialogue.good"] = "Ça passe très bien."; d["stage.dialogue.ok"] = "Pas mal."; d["stage.dialogue.bad"] = "Mauvaise idée…";
            d["stage.dialogue.success"] = "Le contremaître se détend et te laisse passer."; d["stage.dialogue.failure"] = "Le contremaître appelle ses hommes : ça va se régler autrement.";

            // Dialogue des quais (dialogue_dockers) : une réplique, quatre réponses (bonne, deux acceptables, mauvaise).
            d["dialogue.dialogue_dockers.0"] = "Le contremaître te barre la route : « Les quais sont fermés. Qui t'envoie ? »";
            d["dialogue.dialogue_dockers.0.good"] = "« Personne. On cherche Brick, pas les ennuis avec vous. »";
            d["dialogue.dialogue_dockers.0.ok1"] = "« On passe, c'est tout. »";
            d["dialogue.dialogue_dockers.0.ok2"] = "« La ville a besoin de ce port ouvert. »";
            d["dialogue.dialogue_dockers.0.bad"] = "« Pousse-toi ou on te pousse. »";
            d["dialogue.dialogue_dockers.1"] = "Il croise les bras : « Brick paie bien. Pourquoi je te laisserais passer ? »";
            d["dialogue.dialogue_dockers.1.good"] = "« Parce qu'après lui, c'est vous qu'il fera payer. »";
            d["dialogue.dialogue_dockers.1.ok1"] = "« Parce que je suis plus aimable que lui. »";
            d["dialogue.dialogue_dockers.1.ok2"] = "« Je ne dirai rien à personne. »";
            d["dialogue.dialogue_dockers.1.bad"] = "« Tu veux vraiment le savoir ? »";
            d["dialogue.dialogue_dockers.2"] = "Un docker murmure : « Il a raison, tu sais. Brick a doublé les cadences. »";
            d["dialogue.dialogue_dockers.2.good"] = "« Alors aide-nous : dis-nous où est le générateur. »";
            d["dialogue.dialogue_dockers.2.ok1"] = "« Je comprends. Ça doit être dur. »";
            d["dialogue.dialogue_dockers.2.ok2"] = "« Et vous ne dites rien ? »";
            d["dialogue.dialogue_dockers.2.bad"] = "« Pas mon problème. »";
            d["dialogue.dialogue_dockers.3"] = "Le contremaître soupire : « Dernière question. Tu ne nous traîneras pas dans ta bagarre ? »";
            d["dialogue.dialogue_dockers.3.good"] = "« Promis : vous restez à l'abri, on s'occupe du reste. »";
            d["dialogue.dialogue_dockers.3.ok1"] = "« On fera au plus vite. »";
            d["dialogue.dialogue_dockers.3.ok2"] = "« Je ne peux rien promettre. »";
            d["dialogue.dialogue_dockers.3.bad"] = "« Il y aura des dégâts, c'est la vie. »";
        }
    }
}
