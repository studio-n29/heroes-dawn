#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Tutorial
{
    /// <summary>Éléments d'interface que le tutoriel peut désigner.</summary>
    public enum TutorialTarget
    {
        None, QgPage, TabTeam, TeamSquad, TeamMission, MapDeploy, BoardObjectives, BoardRoll, BoardDestination,
        TeamTile, SheetTrain, SheetSlot, SheetPick, TabRecruit, RecruitCard,
        CombatEnemies, CombatSkills, ResultsContinue,
        QgMission,
    }

    /// <summary>
    /// Pose un nom sur un élément d'interface (prefab) pour que le tutoriel puisse le trouver, le cerner et l'éclairer.
    /// Un marqueur n'est « vivant » que tant que son objet est actif et le composant activé : le tutoriel n'affiche
    /// une étape que si sa cible est visible à l'écran.
    /// </summary>
    public sealed class TutorialTargetMarker : MonoBehaviour
    {
        static readonly List<TutorialTargetMarker> LiveList = new List<TutorialTargetMarker>();

        [SerializeField] TutorialTarget id;

        public TutorialTarget Id { get => id; set => id = value; }
        public RectTransform Rect => (RectTransform)transform;

        void OnEnable() => LiveList.Add(this);
        void OnDisable() => LiveList.Remove(this);

        /// <summary>Premier marqueur vivant de cet identifiant (le plus récemment activé), ou null.</summary>
        public static TutorialTargetMarker? Find(TutorialTarget id)
        {
            for (int i = LiveList.Count - 1; i >= 0; i--)
                if (LiveList[i] != null && LiveList[i].id == id) return LiveList[i];
            return null;
        }

        /// <summary>Tous les marqueurs vivants d'un identifiant (cases à portée du plateau).</summary>
        public static void FindAll(TutorialTarget id, List<TutorialTargetMarker> into)
        {
            into.Clear();
            foreach (var m in LiveList) if (m != null && m.id == id) into.Add(m);
        }
    }
}
