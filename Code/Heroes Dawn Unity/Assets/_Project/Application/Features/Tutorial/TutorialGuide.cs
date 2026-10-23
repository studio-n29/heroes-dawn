#nullable enable
using System;
using N29.HeroesDawn.Application.Features.Meta;

namespace N29.HeroesDawn.Application.Features.Tutorial
{
    /// <summary>Étapes bloquantes de la première partie guidée (spec « Maquettes UI et tutoriel » §6), dans l'ordre.</summary>
    public enum TutorialStep
    {
        /// <summary>QG : « Bienvenue à Red Hook » (Compris).</summary>
        Welcome,
        /// <summary>Toucher l'onglet Équipe.</summary>
        OpenTeam,
        /// <summary>Équipe : « Voici ta recrue » (Compris).</summary>
        Squad,
        /// <summary>Toucher la recrue : ouvre sa fiche.</summary>
        OpenSheet,
        /// <summary>Fiche : toucher Entraîner.</summary>
        Train,
        /// <summary>Fiche : toucher le premier emplacement d objet.</summary>
        EquipSlot,
        /// <summary>Inventaire : choisir l objet offert.</summary>
        EquipPick,
        /// <summary>Toucher l onglet Recruter.</summary>
        OpenRecruit,
        /// <summary>Recruter un héros (confirmation comprise).</summary>
        Recruit,
        /// <summary>Revenir sur l onglet Équipe.</summary>
        BackToTeam,
        /// <summary>Équipe : le duo et sa synergie (Compris).</summary>
        Duo,
        /// <summary>Toucher MISSION : ouvre la carte de la ville.</summary>
        StartMission,
        /// <summary>Carte : toucher Déployer.</summary>
        Deploy,
        /// <summary>Plateau : les 3 objectifs (Compris).</summary>
        Objectives,
        /// <summary>Toucher Lancer.</summary>
        Roll,
        /// <summary>Toucher une case à portée.</summary>
        Move,
        /// <summary>Combat : les jauges (Compris, combat figé).</summary>
        CombatAtb,
        /// <summary>Combat : choisir une compétence puis une cible (combat figé jusqu'à l'action).</summary>
        CombatSkill,
        /// <summary>Résultats : la fatigue ; toucher Continuer termine la partie guidée.</summary>
        Results,
        Finished,
    }

    /// <summary>Un geste ou un événement du jeu qui peut faire avancer le tutoriel.</summary>
    public enum TutorialSignal { TeamOpened, MapOpened, BoardOpened, Rolled, Moved, SkillUsed, ResultsClosed, SheetOpened, Trained, SlotOpened, ItemEquipped, RecruitOpened, Recruited }

    /// <summary>
    /// Tutoriel : machine d'étapes enregistrée dans le profil (reprise après fermeture). Les étapes « Compris » avancent par
    /// <see cref="Acknowledge"/>, les autres par le geste réel du joueur (<see cref="Signal"/>). Les astuces contextuelles
    /// (<see cref="Hint"/>) ne s'affichent qu'une fois la partie guidée terminée ou passée, chacune une seule fois.
    /// </summary>
    public sealed class TutorialGuide
    {
        public const int StepCount = (int)TutorialStep.Finished;

        public static readonly string[] HintIds =
        {
            "auto", "timeline", "bonus_die", "end_move", "event", "minigame", "rest", "defeat", "fatigue", "zone_done", "recruit", "infirmary",
        };

        readonly MetaContext _c;

        public TutorialGuide(MetaContext context) { _c = context; }

        Ports.TutorialSave Save => _c.Meta.tutorial;

        public TutorialStep Current => Save.done ? TutorialStep.Finished : (TutorialStep)Math.Min(Math.Max(0, Save.step), StepCount);
        public bool Active => !Save.done;
        public bool Skipped => Save.skipped;

        /// <summary>Rang affiché dans la bulle (« 3 / 19 »).</summary>
        public int Position => Math.Min((int)Current + 1, StepCount);

        /// <summary>Vrai tant que le jeu doit attendre le joueur : le combat est figé pendant une bulle de combat.</summary>
        public bool Presenting { get; set; }
        public bool PausesCombat => Active && Presenting && (Current == TutorialStep.CombatAtb || Current == TutorialStep.CombatSkill);

        /// <summary>Une astuce est demandée : l'interface l'affiche (une fois chacune).</summary>
        public event Action<string>? HintRaised;

        public static bool NeedsAcknowledge(TutorialStep s)
            => s == TutorialStep.Welcome || s == TutorialStep.Squad || s == TutorialStep.Duo || s == TutorialStep.Objectives || s == TutorialStep.CombatAtb;

        /// <summary>À l'arrivée au QG : un joueur qui a déjà validé une zone n'a pas besoin de la partie guidée.</summary>
        public void BeginIfNeeded()
        {
            if (Save.done || Save.step > 0) return;
            if (_c.Session.CompletedZones.Count > 0) Finish(skipped: true);
        }

        public bool Acknowledge()
        {
            if (!Active || !NeedsAcknowledge(Current)) return false;
            Advance();
            return true;
        }

        public void Signal(TutorialSignal signal)
        {
            if (!Active) return;
            var cur = Current;
            // Fin des résultats : quoi qu'il reste, la partie guidée est terminée.
            if (signal == TutorialSignal.ResultsClosed)
            {
                if (cur >= TutorialStep.CombatAtb) Finish(skipped: false);
                return;
            }
            bool match = ((cur == TutorialStep.OpenTeam || cur == TutorialStep.BackToTeam) && signal == TutorialSignal.TeamOpened)
                      || (cur == TutorialStep.OpenSheet && signal == TutorialSignal.SheetOpened)
                      || (cur == TutorialStep.Train && signal == TutorialSignal.Trained)
                      || (cur == TutorialStep.EquipSlot && signal == TutorialSignal.SlotOpened)
                      || (cur == TutorialStep.EquipPick && signal == TutorialSignal.ItemEquipped)
                      || (cur == TutorialStep.OpenRecruit && signal == TutorialSignal.RecruitOpened)
                      || (cur == TutorialStep.Recruit && signal == TutorialSignal.Recruited)
                      || (cur == TutorialStep.StartMission && signal == TutorialSignal.MapOpened)
                      || (cur == TutorialStep.Deploy && signal == TutorialSignal.BoardOpened)
                      || (cur == TutorialStep.Roll && signal == TutorialSignal.Rolled)
                      || (cur == TutorialStep.Move && signal == TutorialSignal.Moved)
                      || (cur == TutorialStep.CombatSkill && signal == TutorialSignal.SkillUsed);
            if (match) Advance();
        }

        void Advance()
        {
            Save.step = Math.Min(StepCount, Save.step + 1);
            if (Save.step >= StepCount) { Finish(skipped: false); return; }
            _c.Touch(MetaArea.Tutorial);
        }

        void Finish(bool skipped)
        {
            Save.step = StepCount;
            Save.done = true;
            Save.skipped = skipped;
            Presenting = false;
            _c.Touch(MetaArea.Tutorial);
        }

        public void Skip() { if (Active) Finish(skipped: true); }

        /// <summary>Revoir le tutoriel (Paramètres ▸ Support) : repart de l'étape 1 ; les astuces déjà vues restent vues.</summary>
        public void Restart()
        {
            Save.step = 0;
            Save.done = false;
            Save.skipped = false;
            _c.Touch(MetaArea.Tutorial);
        }

        public bool HasSeenHint(string id) => Save.hintsSeen.Contains(id);

        /// <summary>Demande l'astuce <paramref name="id"/> : sans effet pendant la partie guidée ou si elle a déjà été vue.</summary>
        public bool Hint(string id)
        {
            if (Active || Save.hintsSeen.Contains(id)) return false;
            Save.hintsSeen.Add(id);
            _c.Touch(MetaArea.Tutorial);
            HintRaised?.Invoke(id);
            return true;
        }
    }
}
