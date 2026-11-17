#nullable enable
using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Application.Features.World
{
    /// <summary>Choix d'histoire du profil (Harlem 3.3, Place Rouge 4.4) : définitifs, un seul par choix ; la scène de la branche retenue se rejoue au rejeu de la zone.</summary>
    public sealed class NarrativeChoices
    {
        readonly MetaContext _c;

        public NarrativeChoices(MetaContext context) { _c = context; }

        /// <summary>Option retenue pour ce choix ; nul si le joueur n'a pas encore choisi.</summary>
        public string? Chosen(string choiceId) => _c.Meta.storyChoices.FirstOrDefault(s => s.id == choiceId)?.option;

        /// <summary>Retient une option ; sans effet si le choix est déjà fait ou si l'option n'existe pas. Rend l'option réellement retenue.</summary>
        public string? Choose(string choiceId, string option)
        {
            var existing = Chosen(choiceId);
            if (existing != null) return existing;
            var def = StoryChoices.Find(choiceId);
            if (def == null || def.Option(option) == null) return null;
            _c.Meta.storyChoices.Add(new StoryChoiceSave { id = choiceId, option = option });
            _c.Touch(MetaArea.Profile);
            return option;
        }
    }
}
