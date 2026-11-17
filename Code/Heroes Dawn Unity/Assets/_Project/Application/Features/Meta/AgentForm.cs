#nullable enable

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>La fiche d'identification de The Dawn remplie par le joueur à la première connexion : nom, prénom, âge, avatar, pouvoir.</summary>
    public readonly struct AgentForm
    {
        public const int MinAge = 12;
        public const int MaxAge = 99;

        public readonly string LastName;
        public readonly string FirstName;
        public readonly int Age;
        public readonly string Avatar;
        public readonly string Power;

        public AgentForm(string lastName, string firstName, int age, string avatar, string power)
        {
            LastName = lastName; FirstName = firstName; Age = age; Avatar = avatar; Power = power;
        }
    }
}
