#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Login
{
    public struct IntroCinematicTexts
    {
        public string Skip;
    }

    /// <summary>Création du personnage joueur : titre, texte du bouton, indication du champ de nom, longueur maximale et galerie d'avatars.</summary>
    public struct ChooseCharacterData
    {
        public string Title;
        public string Confirm;
        public string NameHint;
        public int MaxNameLength;
        public IReadOnlyList<string> Avatars;
    }

    /// <summary>Le personnage choisi : nom saisi (non vide, rognage fait) et identifiant de l'avatar de la galerie.</summary>
    public readonly struct CharacterChoice
    {
        public readonly string Name;
        public readonly string Avatar;

        public CharacterChoice(string name, string avatar) { Name = name; Avatar = avatar; }
    }
}
