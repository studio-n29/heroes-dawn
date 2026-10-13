#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Design
{
    /// <summary>
    /// Dessine une fiche de contenu (classe sérialisable) en champs d'édition à partir de ses champs publics : nombres, textes, listes
    /// déroulantes pour les énumérations, listes de sous-fiches (effets) avec ajout, suppression et déplacement.
    /// </summary>
    public static class RecordDrawer
    {
        static readonly Dictionary<string, string> Labels = new Dictionary<string, string>
        {
            ["id"] = "Identifiant", ["name"] = "Nom", ["rarity"] = "Rareté", ["family"] = "Famille", ["role"] = "Rôle",
            ["hp"] = "PV", ["atk"] = "Attaque", ["def"] = "Défense", ["spd"] = "Vitesse", ["perk"] = "Effet spécial", ["perkValue"] = "Valeur de l'effet",
            ["price"] = "Prix (Crédits)", ["slot"] = "Emplacement", ["cooldown"] = "Recharge (tours)", ["target"] = "Cible", ["effects"] = "Effets",
            ["type"] = "Type", ["value"] = "Valeur", ["status"] = "Statut", ["duration"] = "Durée (tours)", ["chance"] = "Chance (%)",
            ["unlockKind"] = "Déblocage", ["unlockZone"] = "Zone", ["feat"] = "Prouesse", ["skills"] = "Compétences",
        };

        /// <summary>Choix proposés pour un champ texte (liste déroulante) ; null : saisie libre.</summary>
        public delegate string[]? ChoiceProvider(object owner, FieldInfo field);

        /// <summary>Dessine tous les champs de <paramref name="record"/> sauf <paramref name="hidden"/> ; renvoie vrai si l'un a changé.</summary>
        public static bool Draw(object record, ISet<string>? hidden = null, ChoiceProvider? choices = null)
        {
            bool changed = false;
            foreach (var f in record.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (hidden != null && hidden.Contains(f.Name)) continue;
                EditorGUI.BeginChangeCheck();
                var label = Label(f);
                var value = f.GetValue(record);
                if (f.FieldType == typeof(int)) f.SetValue(record, EditorGUILayout.IntField(label, (int)value));
                else if (f.FieldType == typeof(bool)) f.SetValue(record, EditorGUILayout.Toggle(label, (bool)value));
                else if (f.FieldType == typeof(string)) f.SetValue(record, DrawString(record, f, label, (string)value, choices));
                else if (f.FieldType.IsEnum) f.SetValue(record, EditorGUILayout.EnumPopup(label, (Enum)value));
                else if (value is IList list && f.FieldType.IsGenericType && !f.FieldType.GetGenericArguments()[0].IsPrimitive && f.FieldType.GetGenericArguments()[0] != typeof(string))
                    changed |= DrawList(label, list, f.FieldType.GetGenericArguments()[0], choices);
                if (EditorGUI.EndChangeCheck()) changed = true;
            }
            return changed;
        }

        static GUIContent Label(FieldInfo f)
        {
            var tip = f.GetCustomAttribute<TooltipAttribute>()?.tooltip ?? "";
            return new GUIContent(Labels.TryGetValue(f.Name, out var l) ? l : ObjectNames.NicifyVariableName(f.Name), tip);
        }

        static string DrawString(object owner, FieldInfo f, GUIContent label, string value, ChoiceProvider? choices)
        {
            var options = choices?.Invoke(owner, f);
            if (options == null) return EditorGUILayout.TextField(label, value);
            var all = new[] { "" }.Concat(options).ToArray();
            int index = Math.Max(0, Array.IndexOf(all, value));
            var shown = all.Select(s => s.Length == 0 ? "(aucun)" : s).ToArray();
            return all[EditorGUILayout.Popup(label, index, shown)];
        }

        static bool DrawList(GUIContent label, IList list, Type elementType, ChoiceProvider? choices)
        {
            bool changed = false;
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            int remove = -1, up = -1, down = -1;
            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField((i + 1).ToString(), EditorStyles.miniBoldLabel, GUILayout.Width(24));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("▲", GUILayout.Width(26))) up = i;
                if (GUILayout.Button("▼", GUILayout.Width(26))) down = i;
                if (GUILayout.Button("✕", GUILayout.Width(26))) remove = i;
                EditorGUILayout.EndHorizontal();
                changed |= Draw(list[i], null, choices);
                EditorGUILayout.EndVertical();
            }
            if (GUILayout.Button("+ Ajouter")) { list.Add(Activator.CreateInstance(elementType)); changed = true; }
            if (remove >= 0) { list.RemoveAt(remove); changed = true; }
            else if (up > 0) { Swap(list, up, up - 1); changed = true; }
            else if (down >= 0 && down < list.Count - 1) { Swap(list, down, down + 1); changed = true; }
            return changed;
        }

        static void Swap(IList list, int a, int b) { var t = list[a]; list[a] = list[b]; list[b] = t; }
    }
}
