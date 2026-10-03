#nullable enable
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs
{
    /// <summary>
    /// Recette d'un prefab : reconstruit l'élément d'interface (helpers <c>Ui.*</c>, clés de localisation uniquement)
    /// et branche les références de sa vue. Découverte par réflexion par <see cref="PrefabGenerator"/>.
    /// </summary>
    public interface IPrefabRecipe
    {
        /// <summary>Chemin relatif à la racine des prefabs, ex. « Widgets/Portrait.prefab ».</summary>
        string RelativePath { get; }

        /// <summary>Ordre de génération : 0 widgets, 5 widgets composés, 10 communs et popups, 20 écrans.</summary>
        int Order { get; }

        /// <summary>Construit la hiérarchie (objet racine non enregistré) ; le générateur l'enregistre puis la détruit.</summary>
        GameObject Build(PrefabContext ctx);
    }
}
