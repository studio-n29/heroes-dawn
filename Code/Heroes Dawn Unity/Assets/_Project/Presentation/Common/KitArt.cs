#nullable enable
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>
    /// Sprites du kit « Aube tactique » (boutons, surfaces…). Chargés par chemin au moment de la fabrication des prefabs (éditeur) :
    /// les prefabs en gardent la référence, le jeu n'a pas besoin de les chercher. Hors éditeur : null (repli sur les formes générées).
    /// </summary>
    public static class KitArt
    {
        public const string Root = "Assets/_Project/Art/UI/AubeTactiqueKit/";
        /// <summary>Faux : les tests du contrat « formes générées » désactivent l'habillage du kit.</summary>
        public static bool Enabled = true;

        public const string ExtensionRoot = "Assets/_Project/Resources/UI/AubeTactique/Ext/";

        /// <summary>Sprite du dossier d'extension (copie dans Resources, chemin relatif sans extension), ou null.</summary>
        public static Sprite? GetExt(string relative)
        {
#if UNITY_EDITOR
            return Enabled ? AssetDatabase.LoadAssetAtPath<Sprite>(ExtensionRoot + relative + ".png") : null;
#else
            return null;
#endif
        }

        /// <summary>Sprite du kit (chemin relatif au kit, sans extension), ou null.</summary>
        public static Sprite? Get(string relative)
        {
#if UNITY_EDITOR
            return Enabled ? AssetDatabase.LoadAssetAtPath<Sprite>(Root + relative + ".png") : null;
#else
            return null;
#endif
        }
    }
}
