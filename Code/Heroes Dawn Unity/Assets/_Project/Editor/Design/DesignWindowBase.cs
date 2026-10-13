#nullable enable
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Design
{
    /// <summary>
    /// Fenêtre d'édition d'une liste de fiches : recherche et liste à gauche, formulaire et aperçu à droite, boutons Ajouter, Dupliquer,
    /// Supprimer, Enregistrer et Recharger. Enregistrer est refusé tant que des erreurs de validation subsistent.
    /// </summary>
    public abstract class DesignWindowBase<TRecord> : EditorWindow where TRecord : class
    {
        protected abstract string Noun { get; }
        protected abstract List<TRecord> Records { get; }
        protected abstract string IdOf(TRecord r);
        protected abstract string LabelOf(TRecord r);
        protected abstract TRecord NewRecord();
        protected abstract void SetId(TRecord r, string id);
        protected abstract bool Dirty { get; set; }
        protected abstract void SaveFile();
        protected abstract IReadOnlyList<string> Problems();
        /// <summary>Dessine les champs de la fiche ; renvoie vrai si l'une a changé.</summary>
        protected abstract bool DrawForm(TRecord r);
        /// <summary>Avertissements qui n empêchent pas d enregistrer.</summary>
        protected virtual IReadOnlyList<string> Warnings() => System.Array.Empty<string>();
        protected virtual void DrawPreview(TRecord r) { }
        protected virtual void OnDeleting(TRecord r) { }
        protected virtual void OnDuplicated(TRecord source, TRecord copy) { }

        Vector2 _listScroll, _formScroll;
        string _search = "";
        int _selected = -1;

        protected TRecord? Current => _selected >= 0 && _selected < Records.Count ? Records[_selected] : null;

        protected virtual void OnEnable() { DesignStore.EnsureLoaded(); }

        void OnGUI()
        {
            DesignStore.EnsureLoaded();
            var problems = Problems();
            DrawToolbar(problems.Count == 0);
            EditorGUILayout.BeginHorizontal();
            DrawList();
            DrawDetail();
            EditorGUILayout.EndHorizontal();
            DrawProblems(problems);
            var warnings = Warnings();
            if (warnings.Count > 0) EditorGUILayout.HelpBox(string.Join(System.Environment.NewLine, warnings), MessageType.Warning);
        }

        void DrawToolbar(bool canSave)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("+ Ajouter", EditorStyles.toolbarButton, GUILayout.Width(80))) Add();
            using (new EditorGUI.DisabledScope(Current == null))
            {
                if (GUILayout.Button("Dupliquer", EditorStyles.toolbarButton, GUILayout.Width(80))) Duplicate();
                if (GUILayout.Button("Supprimer", EditorStyles.toolbarButton, GUILayout.Width(80))) Delete();
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label(Dirty ? "● modifié" : "enregistré", EditorStyles.miniLabel);
            using (new EditorGUI.DisabledScope(!Dirty || !canSave))
                if (GUILayout.Button("Enregistrer", EditorStyles.toolbarButton, GUILayout.Width(90))) SaveFile();
            if (GUILayout.Button("Recharger", EditorStyles.toolbarButton, GUILayout.Width(80)) && (!Dirty || EditorUtility.DisplayDialog("Recharger", "Les modifications non enregistrées seront perdues.", "Recharger", "Annuler")))
            {
                DesignStore.Reload();
                _selected = -1;
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(280));
            _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField);
            GUILayout.Label(Records.Count + " " + Noun + "(s)", EditorStyles.miniLabel);
            _listScroll = EditorGUILayout.BeginScrollView(_listScroll, "box");
            for (int i = 0; i < Records.Count; i++)
            {
                var r = Records[i];
                var label = LabelOf(r);
                if (_search.Length > 0 && label.IndexOf(_search, System.StringComparison.OrdinalIgnoreCase) < 0 && IdOf(r).IndexOf(_search, System.StringComparison.OrdinalIgnoreCase) < 0) continue;
                var style = i == _selected ? EditorStyles.whiteLabel : EditorStyles.label;
                var rect = EditorGUILayout.GetControlRect(false, 20);
                if (i == _selected) EditorGUI.DrawRect(rect, new Color(0.24f, 0.48f, 0.9f, 0.9f));
                if (GUI.Button(rect, label, i == _selected ? EditorStyles.whiteLabel : EditorStyles.label)) { _selected = i; GUI.FocusControl(null); }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        void DrawDetail()
        {
            EditorGUILayout.BeginVertical();
            _formScroll = EditorGUILayout.BeginScrollView(_formScroll);
            var r = Current;
            if (r == null) EditorGUILayout.HelpBox("Choisis un(e) " + Noun + " dans la liste, ou ajoutes-en un(e).", MessageType.Info);
            else
            {
                if (DrawForm(r)) Dirty = true;
                EditorGUILayout.Space(8);
                DrawPreview(r);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        void DrawProblems(IReadOnlyList<string> problems)
        {
            if (problems.Count == 0) return;
            EditorGUILayout.HelpBox(string.Join("\n", problems.Take(8)) + (problems.Count > 8 ? "\n… et " + (problems.Count - 8) + " autre(s)" : ""), MessageType.Error);
        }

        void Add()
        {
            var r = NewRecord();
            Records.Add(r);
            _selected = Records.Count - 1;
            Dirty = true;
        }

        void Duplicate()
        {
            var source = Current;
            if (source == null) return;
            var copy = JsonUtility.FromJson<TRecord>(JsonUtility.ToJson(source));
            string baseId = IdOf(source) + "_copie", id = baseId;
            for (int n = 2; Records.Any(x => IdOf(x) == id); n++) id = baseId + n;
            SetId(copy, id);
            Records.Insert(_selected + 1, copy);
            _selected++;
            OnDuplicated(source, copy);
            Dirty = true;
        }

        void Delete()
        {
            var r = Current;
            if (r == null || !EditorUtility.DisplayDialog("Supprimer", "Supprimer « " + LabelOf(r) + " » ?", "Supprimer", "Annuler")) return;
            OnDeleting(r);
            Records.Remove(r);
            _selected = Mathf.Min(_selected, Records.Count - 1);
            Dirty = true;
        }
    }
}
