#nullable enable
using System;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface IMemberSheetView : IView
    {
        event Action PreviousClicked;
        event Action NextClicked;
        /// <summary>Action principale : Entraîner (membre possédé), Recruter (héros de la boutique, tâche 4).</summary>
        event Action PrimaryClicked;
        /// <summary>Bouton Soigner touché : soigne cet agent.</summary>
        event Action HealClicked;
        /// <summary>Bouton Dossier touché : ouvre le panneau de lore de l'agent.</summary>
        event Action DossierClicked;
        /// <summary>Bouton Fermer du dossier touché.</summary>
        event Action DossierClosed;
        /// <summary>Equipement : objets et cartes de l agent.</summary>
        event Action LoadoutClicked;
        /// <summary>Zone des affinités touchée : ouvre le pop-up avec tous les agents.</summary>
        event Action AffinityClicked;
        /// <summary>Case de compétence touchée (0 à 3).</summary>
        event Action<int> SkillTileClicked;
        /// <summary>Case d'objet touchée (0 à 2).</summary>
        event Action<int> ItemTileClicked;
        /// <summary>Option choisie dans le panneau d'inventaire.</summary>
        event Action<int> PickerChosen;
        event Action PickerRemoved;
        event Action PickerClosed;
        void Show(MemberSheetData data);
        void ShowDossier(DossierData data);
        void HideDossier();
        void ShowPicker(PickerData data);
        void HidePicker();
    }
}
