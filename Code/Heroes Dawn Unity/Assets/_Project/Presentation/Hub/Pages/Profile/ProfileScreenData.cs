#nullable enable
using System;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Identifiants des six tuiles de la grille « Mes stats » (planche 12.2), dans l'ordre d'affichage.</summary>
    public static class ProfileTiles
    {
        public const string Level = "level", Agents = "agents", Operations = "operations", Missions = "missions", Achievements = "achievements", Lair = "lair";
        public static readonly string[] All = { Level, Agents, Operations, Missions, Achievements, Lair };
    }

    /// <summary>Une tuile de statistique : libellé, valeur, et si un appui ouvre quelque chose.</summary>
    public readonly struct ProfileTileData
    {
        public readonly string Id, Label, Value;
        public readonly bool Tappable;
        public ProfileTileData(string id, string label, string value, bool tappable) { Id = id; Label = label; Value = value; Tappable = tappable; }
    }

    /// <summary>Un ami de la liste : nom, tag, état de connexion (simulé en local : toujours hors ligne).</summary>
    public readonly struct ProfileFriendData
    {
        public readonly string Name, Tag, Status;
        public readonly bool Online;
        public ProfileFriendData(string name, string tag, string status, bool online) { Name = name; Tag = tag; Status = status; Online = online; }
    }

    /// <summary>Un succès de la liste complète.</summary>
    public readonly struct ProfileAchievementData
    {
        public readonly string Title, Description, Progress;
        public readonly bool Unlocked;
        public readonly float Fraction;
        public ProfileAchievementData(string title, string description, string progress, bool unlocked, float fraction)
        { Title = title; Description = description; Progress = progress; Unlocked = unlocked; Fraction = fraction; }
    }

    /// <summary>Tout ce que l'écran Profil affiche (planche 12.2) : bandeau, bloc Compte, statistiques, amis, liens ; ou la liste des succès.</summary>
    public struct ProfileScreenData
    {
        public string HeroId;
        public string Pseudo;
        /// <summary>Tag personnel (« Pseudo#1234 ») ou invitation à créer un compte.</summary>
        public string TagLine;
        public bool TagCopyable;
        public string RankLine;
        /// <summary>« Compte : Invité » ou « Compte : adresse ».</summary>
        public string AccountLabel;
        public string AccountA, AccountB;
        public bool AccountVisible, AccountBVisible;
        public ProfileTileData[] Tiles;
        public string FriendsTitle, FriendsEmpty, AddLabel;
        public ProfileFriendData[] Friends;
        public bool CanAddFriend;
        public string AddReason;
        public string[] LinkLabels;
        public string AchievementsTitle, AchievementsLine, BackLabel;
        /// <summary>Non nul : l'écran montre la liste complète des succès à la place des statistiques et des amis.</summary>
        public ProfileAchievementData[]? Achievements;
    }

    public interface IProfilePageView : IView
    {
        /// <summary>Tag touché : copie.</summary>
        event Action TagClicked;
        /// <summary>Avatar touché : passe au suivant de la galerie.</summary>
        event Action AvatarClicked;
        /// <summary>0 = bouton principal du bloc Compte, 1 = secondaire.</summary>
        event Action<int> AccountClicked;
        event Action<int> TileClicked;
        event Action AddFriendClicked;
        event Action<int> FriendRemoveClicked;
        event Action<int> FriendCopyClicked;
        /// <summary>Lien du bas : 0 registre des agents, 1 archives, 2 tournoi.</summary>
        event Action<int> LinkClicked;
        /// <summary>Retour de la liste des succès.</summary>
        event Action BackClicked;
        void Show(ProfileScreenData data);
    }
}
