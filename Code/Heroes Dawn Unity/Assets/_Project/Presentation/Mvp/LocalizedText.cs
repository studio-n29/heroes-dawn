#nullable enable
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>
    /// Texte statique d'un prefab : porte une clé de localisation et affiche sa traduction à l'activation
    /// (aussi en éditeur, pour l'aperçu). Les prefabs ne contiennent ainsi aucun texte en dur.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public sealed class LocalizedText : MonoBehaviour
    {
        [SerializeField] string key = "";

        public string Key
        {
            get => key;
            set { key = value; Apply(); }
        }

        void OnEnable() => Apply();

        void Apply()
        {
            if (string.IsNullOrEmpty(key)) return;
            var t = GetComponent<TMP_Text>();
            if (t != null) t.text = Loc.T(key);
        }
    }
}
