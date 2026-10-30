#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace N29.HeroesDawn.Presentation.Login
{
    public sealed class CinematicCaptionCue
    {
        public CinematicCaptionCue(float at, string text) { At = at; Text = text; }
        public float At { get; }
        public string Text { get; }
    }

    /// <summary>Un souvenir de Brie : durée éditoriale, images lentes et répliques minutées.</summary>
    public sealed class CinematicVideoStep
    {
        public CinematicVideoStep(string id, string video, float seconds, bool fadeToBlack,
            params CinematicCaptionCue[] cues)
        {
            Id = id; Video = video; Seconds = seconds; FadeToBlack = fadeToBlack;
            Cues = Array.AsReadOnly(cues);
        }
        public string Id { get; }
        public string Video { get; }
        public string? Voice => null; // Voix off volontairement désactivées.
        public bool IsBlack => false;
        public float Seconds { get; }
        public bool FadeToBlack { get; }
        public IReadOnlyList<CinematicCaptionCue> Cues { get; }
        public string Caption => Cues[0].Text;
        public string CaptionAt(float seconds)
        {
            string text = "";
            foreach (var cue in Cues)
            {
                if (seconds < cue.At) break;
                text = cue.Text;
            }
            return text;
        }
    }

    public static class CinematicVideoSequence
    {
        public const string VideoFolder = "Cinematic/Video";
        static CinematicCaptionCue C(float at, string text) => new CinematicCaptionCue(at, text);
        static CinematicVideoStep S(string id, string video, float seconds, bool black,
            params CinematicCaptionCue[] cues) => new CinematicVideoStep(id, video, seconds, black, cues);

        /// <summary>Étire toute la vidéo sur le plan ; une vidéo plus longue conserve sa durée native.</summary>
        public static float ShotDurationForVideo(double videoSeconds, float requestedSeconds)
            => (float)Math.Max(videoSeconds, requestedSeconds);

        public static float PlaybackSpeedForDuration(double videoSeconds, float shotSeconds)
            => videoSeconds <= 0 || shotSeconds <= 0 ? 1f : (float)Math.Min(1.0, videoSeconds / shotSeconds);

        public static IReadOnlyList<CinematicVideoStep> Steps { get; } = Array.AsReadOnly(new[]
        {
            S("S01", "s01", 8, false,
                C(0, "J'ai longtemps cru que nos pouvoirs feraient de nous des sauveurs."),
                C(4, "Il m'a fallu rencontrer Eda pour comprendre ce que cela voulait dire.")),
            S("S02", "s02", 9, false,
                C(0, "Dans les années soixante-dix, Frank Khost a appris à modifier nos gènes."),
                C(4.5f, "Il pouvait donner à des êtres humains des aptitudes extraordinaires.")),
            S("S07", "s07", 8, false,
                C(0, "Moi, Brie Rayner, je travaillais à la NASA quand on m'a choisie."),
                C(4, "Mon esprit a changé. J'ai appris à sentir et maîtriser l'électricité.")),
            S("S10", "s10", 9, false,
                C(0, "Avec mes camarades, nous sommes devenus l'Apex Unit."),
                C(4.5f, "En pleine Guerre froide, l'Amérique voulait une arme. Nous pensions servir la paix.")),
            S("S12", "s11", 8, true,
                C(0, "L'URSS avait son propre méta-humain : Viktor Volkov, maître de la matière."),
                C(4, "Quand son empire est tombé, Viktor a disparu. Nous sommes restés des symboles.")),
            S("S15", "s13", 8, false,
                C(0, "Puis, en 2026, il y a eu Eda. Un garçon qui souffrait, seul."),
                C(4, "Un jour, sa douleur est devenue trop lourde. Il a tenté d'en finir.")),
            S("S16", "s14", 6, false,
                C(0, "On l'a conduit à l'hôpital Salem. Les médecins n'ont pas pu le sauver.")),
            S("S17", "s15", 5, false,
                C(0, "Son cœur s'est arrêté. Mais quelque chose, en lui, refusait de mourir.")),
            S("S18", "s16", 9, false,
                C(0, "Eda a rouvert les yeux, traversé par une puissance aux flammes noires."),
                C(4.5f, "Nous avions été fabriqués. Lui était le premier méta-humain né naturellement.")),
            S("S20", "s18", 9, false,
                C(0, "Cette nouvelle a attiré Orik Kovvolt, le dirigeant de la X Society."),
                C(4.5f, "Il voulait recruter Eda et retourner sa puissance contre l'Apex Unit.")),
            S("S22", "s20", 8, false,
                C(0, "Eda a refusé. Orik a fait tuer ses proches. Eda l'a affronté..."),
                C(4, "... et découvert qui se cachait sous ce nom : Viktor Volkov.")),
            S("S24", "s24", 9, true,
                C(0, "Ce combat était un piège. Viktor avait besoin du sang d'Eda pour son rituel."),
                C(4.5f, "Il a réveillé Mephisto, un général des Enfers, qui a pris possession de son corps.")),
            S("S25", "s25", 9, false,
                C(0, "Eda s'est échappé. Il a prévenu tous ceux qui pouvaient l'entendre. En vain."),
                C(4.5f, "Moi, je l'ai cru. Nous sommes partis chercher Mephisto, traqués par le gouvernement.")),
            S("S26", "s26", 9, false,
                C(0, "Au fil de la route, j'ai découvert quelqu'un de bien plus fragile que ses pouvoirs."),
                C(4.5f, "Je voulais le protéger. Sans m'en apercevoir, je m'étais attachée à lui.")),
            S("S27", "s27", 8, false,
                C(0, "Ensemble, nous avons remonté la piste de Mephisto et du protocole Khost."),
                C(4, "Ces secrets m'ont forcée à regarder autrement notre propre histoire.")),
            S("S28", "s28", 8, false,
                C(0, "La piste nous a menés au Vatican. Mephisto était là. Nos poursuivants aussi."),
                C(4, "Ils ont voulu nous arrêter. Au milieu de la bataille, tout nous a échappé.")),
            S("S29", "s29", 7, false,
                C(0, "J'ai été gravement blessée. Eda m'a vue tomber..."),
                C(3.5f, "... et il a cru qu'il allait encore perdre quelqu'un qu'il aimait.")),
            S("S30", "s30", 8, false,
                C(0, "Sa peur s'est changée en colère. Ses flammes ont débordé de partout."),
                C(4, "Il n'arrivait plus à les retenir. Le Vatican a été détruit.")),
            S("S31", "s31", 8, false,
                C(0, "Mephisto a profité du chaos pour lui enfoncer sa lame dans le cœur."),
                C(4, "Il l'a tué. Mais Eda était un phénix. Sa mort n'était pas une fin.")),
            S("S32", "s32", 8, true,
                C(0, "Son corps s'est régénéré. Pourtant, je ne retrouvais plus le même garçon."),
                C(4, "Il se croyait responsable de tout. Il ne voulait plus se battre.")),
            S("S33", "s33", 9, false,
                C(0, "Pendant ce temps, Mephisto a gagné Paris et ouvert une brèche au-dessus de la tour Eiffel."),
                C(4.5f, "Les Enfers se déversaient dans notre monde. Il fallait l'arrêter.")),
            S("S34", "s34", 8, false,
                C(0, "Mes camarades de l'Apex Unit m'ont rejointe. Nous avons affronté les démons..."),
                C(4, "... aussi longtemps que nous l'avons pu. Mais nous n'étions pas assez forts.")),
            S("S35", "s35", 7, false,
                C(0, "La brèche grandissait. Le monde s'effondrait sous nos yeux."),
                C(3.5f, "Nous avions épuisé toutes nos forces. Je ne voyais plus d'issue.")),
            S("S36", "s36", 6, false,
                C(0, "Il restait Eda. Mais après ce qu'il avait subi..."),
                C(3, "... pouvais-je encore lui demander de nous sauver ?")),
            S("S37", "s37", 8, false,
                C(0, "Je n'ai pas eu à le faire. Eda est revenu de lui-même."),
                C(4, "Il avait peur, lui aussi. Cette fois, il a choisi de rester avec nous.")),
            S("S38", "s38", 7, false,
                C(0, "Il s'est jeté sur Mephisto et a arraché son âme au corps de Viktor."),
                C(3.5f, "Pour refermer la brèche, il devait l'emporter de l'autre côté.")),
            S("S39", "s39", 7, false,
                C(0, "Pour refermer la brèche, il devait l'emporter de l'autre côté."),
                C(3.5f, "Je l'ai vu traverser. Je n'ai pas eu le temps de lui dire adieu.")),
            S("S40", "s40", 9, false,
                C(0, "Puis il a libéré toute son énergie. Une Supernova a déchiré le ciel."),
                C(4.5f, "La faille s'est effondrée avec Mephisto. Eda n'est pas revenu.")),
            S("S41", "s41", 8, true,
                C(0, "Son onde d'énergie a traversé la Terre, transformant la nature et ses habitants."),
                C(4, "Partout, de nouveaux méta-humains se sont éveillés. Le monde avait changé.")),
            S("S44", "s44", 10, false,
                C(0, "Cinq ans plus tard, mes camarades et moi avons fondé The Dawn."),
                C(5, "Pour accueillir ces nouveaux venus, les protéger, et leur apprendre à vivre avec leurs dons.")),
            S("S46", "s46", 10, true,
                C(0, "Je ne peux pas leur promettre un monde sans peur. Mais ils ne seront plus seuls."),
                C(4, "C'est ce qu'Eda m'a appris. Et c'est la promesse que je vous fais."),
                C(7.5f, "Bienvenue dans l'Aube des Héros.")),
        });
        public static float DurationSeconds => Steps.Sum(s => s.Seconds);
    }
}
