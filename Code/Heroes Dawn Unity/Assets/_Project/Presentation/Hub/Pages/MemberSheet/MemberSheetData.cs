#nullable enable

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Compétence sur la fiche : nom et description générée.</summary>
    public struct SkillLineData
    {
        public string Name;
        public string Description;
    }

    /// <summary>Tout ce qu'affiche la fiche d'un membre (M21).</summary>
    public struct MemberSheetData
    {
        public MemberCardData Card;
        public bool Owned;
        public string FatigueText;
        public float Fatigue01;
        public int Fatigue;
        public SkillLineData[] Skills;
        /// <summary>Combos possibles pour la famille du membre : « Point faible · avec Perception ».</summary>
        public string[] Combos;
        public string PrimaryLabel;
        public bool PrimaryEnabled;
        /// <summary>Pourquoi l'action principale est grisée (« Il manque 400 CR ») ; vide sinon.</summary>
        public string PrimaryReason;
        public bool ShowArrows;
        public bool ShowRooms;
        /// <summary>Bouton Equipement : libelle avec les emplacements remplis ; cache sur la fiche d un heros de la boutique.</summary>
        public string LoadoutLabel;
        public bool ShowLoadout;
    }
}
