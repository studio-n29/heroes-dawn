#nullable enable
using System;
using System.Collections.Generic;

namespace N29.HeroesDawn.Application.Ports
{
    // Données de méta-progression sauvegardées dans le profil (schéma v4). Classes simples et listes seulement : JsonUtility.

    [Serializable] public sealed class CounterSave { public string id = ""; public int value; }

    [Serializable]
    public sealed class DailySave
    {
        public string id = "";
        public int progress;
        public bool claimed;
    }

    [Serializable]
    public sealed class QuestSave
    {
        public string id = "";
        public int progress;
        public bool claimed;
    }

    [Serializable]
    public sealed class SecondarySave
    {
        public string zoneId = "";
        public long expiresUtcTicks;
    }

    [Serializable]
    public sealed class ExpeditionSave
    {
        public int slot;
        public string templateId = "";
        public List<string> members = new List<string>();
        public long endUtcTicks;
    }

    [Serializable]
    public sealed class SettingsSave
    {
        public int master = 100, music = 80, sfx = 100, voice = 100;
        public bool muteInBackground = true;
        /// <summary>0 = auto, 1 = bas, 2 = moyen, 3 = haut.</summary>
        public int quality;
        public bool fps60 = true;
        public bool batterySaver;
        public bool fullCombatEffects = true;
        public bool reduceFlashes;
        public bool reduceShakes;
        public bool extendedMiniGameTime;
        public bool subtitles = true;
        /// <summary>0 = normal, 1 = grand, 2 = très grand.</summary>
        public int textSize;
        public string language = "fr";
        public bool notifications = true;
        public bool notifTraining = true, notifExpedition = true, notifLab = true, notifRecovered = true, notifShop = true, notifObjectives = true, notifSeason = true;
        public bool quietHours = true;
        public bool showKeys = true;
        public bool consentStats = true;
    }

    [Serializable]
    public sealed class TutorialSave
    {
        /// <summary>Index de l'étape atteinte (0 = pas commencé).</summary>
        public int step;
        public bool done;
        public bool skipped;
        public List<string> hintsSeen = new List<string>();
    }

    /// <summary>Première connexion : cinématique d introduction vue, pseudo choisi, campagne choisie.</summary>
    [Serializable]
    public sealed class OnboardingSave
    {
        public bool done;
        public string pseudo = "";
        public string campaign = "";
        /// <summary>Parcours choisi : Agent D ou Agent M (vide : Agent D par défaut).</summary>
        public string hero = "";
    }

    /// <summary>Tout ce qui dépasse le noyau (équipe, ressources, boutique de recrutement) : pass, missions, opérations, labo, archives…</summary>
    [Serializable]
    public sealed class MetaSave
    {
        public int gems = 50;

        // Pass saisonnier
        public string seasonStart = "";
        public int passXp;
        public bool passPremium;
        public List<int> passFreeClaimed = new List<int>();
        public List<int> passPremiumClaimed = new List<int>();

        // Missions du jour, objectifs secondaires
        public string dailyDay = "";
        public List<DailySave> daily = new List<DailySave>();
        public List<SecondarySave> secondary = new List<SecondarySave>();
        /// <summary>Quêtes secondaires de la campagne (progression et récompense réclamée).</summary>
        public List<QuestSave> quests = new List<QuestSave>();
        public long secondaryNextUtcTicks;

        // Opérations, labo
        public List<ExpeditionSave> expeditions = new List<ExpeditionSave>();
        public List<CounterSave> labLevels = new List<CounterSave>();
        public string labActive = "";
        public long labEndUtcTicks;

        // Archives, boutique
        public List<string> archivesRead = new List<string>();
        public string dailyGiftDay = "";
        public List<CounterSave> purchases = new List<CounterSave>();

        // Réglages et tutoriel
        public SettingsSave settings = new SettingsSave();
        public TutorialSave tutorial = new TutorialSave();
        public OnboardingSave onboarding = new OnboardingSave();

        // Objets, grimoire, boosters
        public CollectionSave collection = new CollectionSave();

        public int GetCounter(List<CounterSave> list, string id)
        {
            foreach (var c in list) if (c.id == id) return c.value;
            return 0;
        }

        public void SetCounter(List<CounterSave> list, string id, int value)
        {
            foreach (var c in list) if (c.id == id) { c.value = value; return; }
            list.Add(new CounterSave { id = id, value = value });
        }
    }
}
