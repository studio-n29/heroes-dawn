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
    public sealed class ActivitySave
    {
        public string zoneId = "";
        public int kind;
        public int target = 1;
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

    /// <summary>Composition d'équipe enregistrée : les agents de l'escouade (identifiants).</summary>
    [Serializable]
    public sealed class SquadPresetSave
    {
        public List<string> members = new List<string>();
    }

    /// <summary>Victoires remportées ensemble par deux agents (identifiants triés : a avant b).</summary>
    [Serializable]
    public sealed class LinkSave
    {
        public string a = "";
        public string b = "";
        public int wins;
    }

    /// <summary>Un objet fabriqué au labo : sa recette, sa qualité (<see cref="N29.HeroesDawn.Definitions.CraftQuality"/>) et les statistiques tirées (en %).</summary>
    [Serializable]
    public sealed class CraftedItemSave
    {
        public string id = "";
        public string recipeId = "";
        public int quality;
        public int hp, atk, def, spd;
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

        /// <summary>Liens entre agents : victoires remportées ensemble, par paire (voir <see cref="N29.HeroesDawn.Definitions.LinkRules"/>).</summary>
        public List<LinkSave> links = new List<LinkSave>();

        /// <summary>Compositions d'équipe enregistrées (trois emplacements, <see cref="N29.HeroesDawn.Application.Features.Hub.SquadPresets.Slots"/>).</summary>
        public List<SquadPresetSave> squadPresets = new List<SquadPresetSave>();

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
        /// <summary>Jour des activités du jour tirées, et ces activités (zone terminée, objectif, progression).</summary>
        public string activityDay = "";
        public List<ActivitySave> activities = new List<ActivitySave>();
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
        /// <summary>Vrai une fois le formulaire de retour de playtest proposé (après la zone 1.1).</summary>
        public bool playtestFormAsked;
        /// <summary>Rapports de retour de playtest envoyés (texte prêt à partager).</summary>
        public List<string> playtestReports = new List<string>();
        public OnboardingSave onboarding = new OnboardingSave();

        // Objets, grimoire, boosters
        public CollectionSave collection = new CollectionSave();

        // Fabrication au labo (lot E) : scraps, recettes apprises (hors recettes de départ), expérience par type d'objet, objets fabriqués
        /// <summary>Scraps possédés (nom de <see cref="N29.HeroesDawn.Definitions.ScrapKind"/>, nombre).</summary>
        public List<CounterSave> scraps = new List<CounterSave>();
        public List<string> recipes = new List<string>();
        /// <summary>Expérience de création (nom de <see cref="N29.HeroesDawn.Definitions.ItemType"/>, expérience totale).</summary>
        public List<CounterSave> craftXp = new List<CounterSave>();
        public List<CraftedItemSave> crafted = new List<CraftedItemSave>();
        /// <summary>Nombre d'objets fabriqués : numéro du prochain exemplaire et graine de chaque fabrication.</summary>
        public int craftedNext;
        /// <summary>Nombre de tirages de scraps après combat : fait varier la graine de chacun.</summary>
        public int scrapDrops;

        // Profil (lot F) : compteurs de statistiques (combatsWon, combatsLost, creditsEarned) et tags des amis (liste simulée locale)
        public List<CounterSave> stats = new List<CounterSave>();
        public List<string> friends = new List<string>();

        // Narration (lot G) : chapitres dont l'ouverture a été montrée
        public List<string> chaptersSeen = new List<string>();

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
