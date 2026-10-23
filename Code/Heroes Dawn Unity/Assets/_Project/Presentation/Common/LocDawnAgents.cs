#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Les 30 agents numérotés de The Dawn : nom, rôle, pouvoir et courte fiche du registre (source : 02_Lore_Narration/04_Agents_The_Dawn.md).</summary>
    public static partial class Loc
    {
        /// <summary>Agent (identifiant, nom, rôle de kit, pouvoir, fiche).</summary>
        static (string Id, string Name, string Role, string Power, string Bio)[] DawnAgents() => new[]
        {
            ("agent_d", "Agent D", "Control", "Matérialise ce que son esprit imagine", "Illustrateur de rue de Harlem, il a tracé à la craie une barrière qui s est matérialisée sous les balles."),
            ("agent_m", "Agent M", "Precision", "Perçoit le passé, le présent et le futur de ce qui l entoure", "Restauratrice de fresques à Moscou, elle a vu ce qui allait arriver au moment de l attentat de sa cathédrale."),
            ("darius_brooks", "Darius Brooks", "Tank", "Anime et contrôle les objets non organiques", "Mécanicien de Détroit, il a découvert son pouvoir en dégageant un enfant d'un échafaudage effondré à New York."),
            ("mei_lin_chen", "Mei-Lin Chen", "Damage", "Crée, dirige et étouffe les flammes", "Étudiante en art de Hong Kong, elle a senti le feu répondre à sa voix quand un gang a incendié son quartier."),
            ("freya_lindqvist", "Freya Lindqvist", "Support", "Lit et transmet les pensées", "Enfant muette de Stockholm, elle guide désormais l'évacuation d'une foule en entendant chaque pensée."),
            ("kwame_abara", "Kwame Abara", "Damage", "Se transforme en n'importe quel animal", "Soigneur au zoo du Bronx, il s'est transformé sous les tirs d'un trafiquant d'animaux."),
            ("giulia_rossi", "Giulia Rossi", "Damage", "Déplace, soulève et projette des objets par la pensée", "Employée d'un casino de Manhattan, elle a retourné les armes de ses agresseurs contre eux."),
            ("lucas_mendes", "Lucas Mendes", "Tank", "Rend son corps traversable par la matière", "Cascadeur brésilien, il a traversé un camion et des murs pour sauver une otage."),
            ("rin_takeda", "Rin Takeda", "Control", "Émet, capte et brouille les ondes électromagnétiques", "Hackeuse de Tokyo, elle a rétabli le courant d'un hôpital par la seule force de son esprit."),
            ("amara_hughes", "Amara Hughes", "Support", "Perçoit et influence les émotions", "Chanteuse de gospel d'Atlanta, elle a apaisé une foule en colère sur une seule note."),
            ("ilona_varga", "Ilona Varga", "Control", "Donne une forme physique aux peurs de ses cibles", "Fille de psychiatre de Budapest, elle a fait surgir la peur d'un preneur d'otages à Paris."),
            ("fatou_diallo", "Fatou Diallo", "Tank", "Transforme son énergie cinétique en boucliers et projectiles", "Sprinteuse de Dakar, elle a protégé les voyageurs d'un attentat du métro parisien."),
            ("clement_lefevre", "Clément Lefèvre", "Precision", "Crée des illusions visuelles et sonores", "Magicien de rue de Nantes, il a compris que ses illusions pouvaient devenir la réalité des autres."),
            ("youssef_haddad", "Youssef Haddad", "Precision", "Décuple tous ses sens", "Ancien enquêteur de Beyrouth devenu aveugle, il localise chaque otage à l'oreille."),
            ("severine_morel", "Séverine Morel", "Support", "Absorbe l'énergie vitale de ses cibles", "Infirmière en soins palliatifs, elle apaise ses patients en prenant leur douleur."),
            ("sam_okafor", "Sam Okafor", "Tank", "Absorbe les coups puis les redistribue en onde de choc", "Ancien boxeur de Lagos, il a survécu à une fusillade moscovite pour protéger sa petite sœur."),
            ("astrid_hallstrom", "Astrid Hallström", "Support", "Manipule l'eau sous toutes ses formes", "Pêcheuse norvégienne, la vague a répondu à sa peur lors d'un naufrage sur la Moskova."),
            ("celeste_moreau", "Céleste Moreau", "Control", "Produit des gaz toxiques, anesthésiants ou euphorisants", "Chimiste marseillaise, elle a endormi les flammes et les survivants d'un métro moscovite."),
            ("haruto_nakamura", "Haruto Nakamura", "Control", "Sépare son esprit de son corps", "Étudiant en médecine d'Osaka, il explore un hôpital en feu sans son corps pour trouver les survivants."),
            ("yelena_ivanova", "Yelena Ivanova", "Control", "Crée et contrôle la glace et le froid intense", "Glaciologue de Iakoutsk, elle a gelé une rue entière de Moscou pour arrêter des émeutiers."),
            ("noah_kaplan", "Noah Kaplan", "Control", "Échange sa position avec un objet ou une personne", "Ancien cascadeur, il a découvert son pouvoir en retrouvant un ami disparu à l'autre bout de New York."),
            ("tunde_oyelaran", "Tunde Oyelaran", "Support", "Crée des espaces dimensionnels pour stocker ou cacher", "Architecte d'Ibadan, il a caché un ouvrier dans un espace parallèle avant l'effondrement du bâtiment."),
            ("victor_sorel", "Victor Sorel", "Control", "Prend le contrôle de l'esprit de ses cibles", "Avocat bordelais, il met son pouvoir au service de The Dawn depuis la perte de sa fille."),
            ("odile_ferrand", "Odile Ferrand", "Support", "Ralentit ou accélère le temps autour d'elle", "Horlogère genevoise, son pouvoir est lié à la peur de perdre le temps."),
            ("louis_dubois", "Louis Dubois", "Control", "Crée et contrôle des champs magnétiques", "Ingénieur lyonnais, il a bloqué une porte en feu avec des chariots métalliques attirés vers lui."),
            ("marco_bellini", "Marco Bellini", "Damage", "Transforme la composition de la matière par contact", "Restaurateur florentin, il a changé des lingots volés en métal inoffensif."),
            ("zofia_kowalczyk", "Zofia Kowalczyk", "Damage", "Crée, amplifie, déforme ou annule les sons", "Violoncelliste de Cracovie, son archet a repoussé les décombres d'un attentat à Moscou."),
            ("mateo_vasquez", "Mateo Vasquez", "Support", "Contrôle le sang de ses cibles ou le sien", "Médecin des urgences de Mexico, il a ralenti l'hémorragie d'un policier mourant par la volonté."),
            ("ada_okonkwo", "Ada Okonkwo", "Support", "Manipule l'ADN des êtres vivants", "Généticienne de Lagos, elle a soigné un enfant grièvement blessé pendant une évacuation à Paris."),
            ("eero_nieminen", "Eero Nieminen", "Damage", "Augmente ou diminue localement la gravité", "Physicien d'Helsinki, il a arrêté une chute de plusieurs étages après l'accident de son laboratoire."),
            ("ji_hoon_park", "Ji-Hoon Park", "Precision", "Rend son corps invisible", "Garçon de bureau d'un hôtel parisien, il a sauvé des clients sans être vu."),
            ("anna_petrov", "Anna Petrov", "Precision", "Se téléporte vers des points qu'elle connaît", "Ballerine du Bolchoï, elle s'est téléportée sous une fusillade pour sauver un collègue."),
        };

        /// <summary>Noms des deux compétences de base de chaque agent (compétence 1 légère, compétence 2 spéciale).</summary>
        static Dictionary<string, (string First, string Second)> DawnSkillNames() => new Dictionary<string, (string, string)>
        {
            ["agent_d"] = ("Barrière de craie", "Créature dessinée"), ["agent_m"] = ("Vision du présent", "Futur fragmenté"),
            ["darius_brooks"] = ("Poutre propulsée", "Rempart de ferraille"), ["mei_lin_chen"] = ("Jet de flammes", "Brasier"),
            ["freya_lindqvist"] = ("Murmure mental", "Esprit partagé"), ["kwame_abara"] = ("Griffes du fauve", "Meute déchaînée"),
            ["giulia_rossi"] = ("Projection", "Tempête d'objets"), ["lucas_mendes"] = ("Frappe fantôme", "Corps de brume"),
            ["rin_takeda"] = ("Décharge radio", "Brouillage"), ["amara_hughes"] = ("Note apaisante", "Chœur de guérison"),
            ["ilona_varga"] = ("Ombre de la peur", "Cauchemar"), ["fatou_diallo"] = ("Foulée explosive", "Bouclier cinétique"),
            ["clement_lefevre"] = ("Mirage", "Grande illusion"), ["youssef_haddad"] = ("Écoute absolue", "Perception totale"),
            ["severine_morel"] = ("Drain vital", "Transfusion"), ["sam_okafor"] = ("Encaisser", "Onde de choc"),
            ["astrid_hallstrom"] = ("Vague", "Marée vive"), ["celeste_moreau"] = ("Brume toxique", "Nuage soporifique"),
            ["haruto_nakamura"] = ("Âme errante", "Souffle astral"), ["yelena_ivanova"] = ("Givre mordant", "Glace éternelle"),
            ["noah_kaplan"] = ("Permutation", "Échange forcé"), ["tunde_oyelaran"] = ("Poche d'espace", "Refuge plié"),
            ["victor_sorel"] = ("Emprise", "Volonté dominée"), ["odile_ferrand"] = ("Seconde suspendue", "Temps accéléré"),
            ["louis_dubois"] = ("Aimant", "Champ magnétique"), ["marco_bellini"] = ("Transmutation", "Or alchimique"),
            ["zofia_kowalczyk"] = ("Archet vibrant", "Crescendo"), ["mateo_vasquez"] = ("Pression sanguine", "Coagulation"),
            ["ada_okonkwo"] = ("Mutation", "Régénération cellulaire"), ["eero_nieminen"] = ("Poids mort", "Gravité écrasante"),
            ["ji_hoon_park"] = ("Coup invisible", "Disparition"), ["anna_petrov"] = ("Saut instantané", "Pirouette téléportée"),
        };

        static void AddDawnAgents(Dictionary<string, string> d)
        {
            var skillNames = DawnSkillNames();
            foreach (var (id, name, role, power, bio) in DawnAgents())
            {
                d["members." + id + ".name"] = name;
                d["members." + id + ".power"] = power;
                d["members." + id + ".bio"] = bio;
                d["archives.hero." + id] = name.ToUpperInvariant() + " : " + power.ToLowerInvariant() + ". " + bio;
                var skills = skillNames[id];
                d["skills." + id + "_a1"] = skills.First;
                d["skills." + id + "_a2"] = skills.Second;
            }
        }
    }
}
