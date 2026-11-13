#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>
    /// Textes du chapitre Sergueiv (Moscou, parcours d'Agent M) : ennemis, zones, objectifs, dialogues, fragments, archives, carte.
    /// Le français complète le dictionnaire de base (<c>ExtendMoscow</c>) ; l'anglais est fusionné dans <c>En</c> (<see cref="WithMoscowEnglish"/>).
    /// Source des répliques et des fragments : spec 2026-11-13, annexes A et B.
    /// </summary>
    public static partial class Loc
    {
        static partial void ExtendMoscow(Dictionary<string, string> d)
        {
            // --- Ennemis ---
            d["enemies.mos_guard.name"] = "Garde de la mafia";
            d["enemies.mos_smuggler.name"] = "Contrebandier";
            d["enemies.mos_enforcer.name"] = "Homme de main";
            d["enemies.mos_passeur.name"] = "LE PASSEUR";
            d["enemies.mos_passeur.phase2"] = "Les clients arrivent";
            d["enemies.mos_passeur.phase3"] = "Tout doit partir";
            d["skills.mos_guard_a1"] = "Coup de crosse";
            d["skills.mos_guard_a2"] = "Cordon de sécurité";
            d["skills.mos_smuggler_a1"] = "Tir d'avertissement";
            d["skills.mos_smuggler_a2"] = "Livraison express";
            d["skills.mos_enforcer_a1"] = "Poing ganté";
            d["skills.mos_enforcer_a2"] = "Visite de courtoisie";
            d["skills.mos_enforcer_a3"] = "Manteau doublé";
            d["skills.mos_passeur_a1"] = "Marchandage";
            d["skills.mos_passeur_a2"] = "Cargaison scellée";
            d["skills.mos_passeur_a3"] = "Livraison";
            d["skills.mos_passeur_a4"] = "Mise en demeure";
            d["skills.mos_passeur_p2_a2"] = "Clients pressés";
            d["archives.enemy.mos_guard"] = "Garde de la mafia : un cordon humain posté aux entrées. Il se couvre d'un bouclier et attire les coups.";
            d["archives.enemy.mos_smuggler"] = "Contrebandier : tireur de la filière du maglev. Il marque sa cible avant de la livrer au plomb.";
            d["archives.enemy.mos_enforcer"] = "Homme de main : l'élite du comité d'accueil. Ses visites de courtoisie secouent toute l'escouade.";
            d["archives.enemy.mos_passeur"] = "LE PASSEUR : il ne parle jamais de victimes ni d'ennemis, seulement de clients. Tout, chez lui, a un prix.";

            // --- Zones (chapitre Sergueiv) ---
            d["zones.mos_1_1.name"] = "GARE DU MAGLEV — L'ACCUEIL";
            d["zones.mos_1_1.short"] = "Gare du maglev";
            d["zones.mos_1_1.code"] = "Zone M1.1";
            d["zones.mos_1_1.desc"] = "Moscou t'ouvre grand les bras : hologrammes à ton nom, quais balayés, sourires parfaits. Trop parfaits. Un cargo scellé dort pourtant sur la voie 9, sans manifeste. Traverse l'accueil officiel et vois ce qu'on cherche à te cacher.";
            d["zones.mos_1_2.name"] = "MONASTÈRE — LES ARCHIVES";
            d["zones.mos_1_2.short"] = "Monastère";
            d["zones.mos_1_2.code"] = "Zone M1.2";
            d["zones.mos_1_2.desc"] = "Le cargo ne figure sur aucun manifeste, mais quelqu'un a gardé la trace des livraisons : les archives du monastère, fermées au public. Fais-toi passer pour la restauratrice de la fresque et fouille ce qu'elle cache.";
            d["zones.mos_1_3.name"] = "VOIE DU MAGLEV — LE CONVOI";
            d["zones.mos_1_3.short"] = "Voie du maglev";
            d["zones.mos_1_3.code"] = "Zone M1.3";
            d["zones.mos_1_3.desc"] = "Le registre mène à un convoi qui n'existe pas, éclairé comme une fête sous la ville. Rattrape-le, brise son escorte et ouvre les caisses : elles portent un code que personne n'utilise plus.";
            d["zones.mos_1_4.name"] = "DÉPÔT DU MAGLEV — LE PASSEUR";
            d["zones.mos_1_4.short"] = "Dépôt";
            d["zones.mos_1_4.code"] = "Zone M1.4";
            d["zones.mos_1_4.desc"] = "Le convoi finit ici, dans un dépôt où tout a un prix. Son maître ne dit jamais victime ni ennemi, seulement client. Fouille le dépôt, brise sa garde et fais face au Passeur.";
            d["qg.fil.zone_mos_1_1"] = "Moscou t'accueille à bras ouverts, et c'est ce qui inquiète. Traverse la gare, repère le cargo scellé et sors avant que le comité d'accueil ne se referme.";
            d["qg.fil.zone_mos_1_2"] = "Le cargo ne mène nulle part sur le papier. Les archives du monastère, elles, savent tout : entre, fais pivoter la fresque et emporte le registre.";
            d["qg.fil.zone_mos_1_3"] = "Le registre parle d'un convoi sous la ville. Rattrape-le sur la voie du maglev, brise son escorte et ouvre ses caisses.";
            d["qg.fil.zone_mos_1_4"] = "Tout converge vers le dépôt du maglev. Le Passeur t'y attend, il a organisé l'accueil. Fais-lui comprendre que tu n'es pas un client.";

            // --- Objectifs ---
            d["objectives.mos11.welcome"] = "Franchir l'accueil officiel";
            d["objectives.mos11.platforms"] = "Se glisser derrière les quais";
            d["objectives.mos11.escort"] = "Neutraliser l'escorte du cargo";
            d["objectives.mos11.leave"] = "Quitter la gare";
            d["objectives.mos12.archives"] = "Entrer dans les archives";
            d["objectives.mos12.fresco"] = "Faire pivoter la fresque";
            d["objectives.mos12.guardian"] = "Vaincre le gardien des archives";
            d["objectives.mos12.register"] = "Emporter le registre";
            d["objectives.mos13.convoy"] = "Rattraper le convoi";
            d["objectives.mos13.escort"] = "Briser l'escorte du convoi";
            d["objectives.mos13.crates"] = "Ouvrir les caisses";
            d["objectives.mos14.depot"] = "Fouiller le dépôt";
            d["objectives.mos14.guard"] = "Briser la garde du dépôt";
            d["objectives.mos14.passeur"] = "Vaincre le Passeur";

            // --- Dialogues (annexe A de la spec) ---
            d["speaker.agent_m"] = "Agent M";
            d["speaker.passeur"] = "Le Passeur";
            d["speaker.enforcer"] = "Homme de main";
            d["narration.mos_1_1.n14.before.0"] = "« Bienvenue à Moscou. Le comité d'accueil vous prie de rebrousser chemin. »";
            d["narration.mos_1_1.n14.before.1"] = "« Un accueil trop parfait cache toujours quelque chose. »";
            d["narration.mos_1_1.n14.after.0"] = "« Ce cargo scellé n'est sur aucun manifeste. Je vois les camions qui l'ont déposé… trois jours avant nous. »";
            d["narration.mos_1_1.n19.after.0"] = "« On sort par les archives du monastère. Si quelqu'un nous y attend, ce n'est pas pour nous souhaiter la bienvenue. »";
            d["narration.mos_1_2.n14.before.0"] = "« Les archives sont fermées au public. »";
            d["narration.mos_1_2.n14.before.1"] = "« Pas pour la restauratrice de la fresque. »";
            d["narration.mos_1_2.n14.after.0"] = "« Derrière la fresque, un registre. Je l'avais vu avant de le toucher. »";
            d["narration.mos_1_2.n18.after.0"] = "« Un registre, la moitié d'une preuve. L'autre moitié est du côté des raffineries. »";
            d["narration.mos_1_3.n14.before.0"] = "« Ce convoi n'existe pas. Vous n'avez rien vu. »";
            d["narration.mos_1_3.n14.after.0"] = "« Ces caisses portent un code soviétique. Quelqu'un déterre de vieilles recherches. »";
            d["narration.mos_1_3.n18.after.0"] = "« Un code d'avant l'Union sur des caisses de cette année. Retiens-le. »";
            d["narration.mos_1_4.n18.before.0"] = "« Vous êtes en avance. D'ordinaire mes clients prennent rendez-vous. »";
            d["narration.mos_1_4.n18.before.1"] = "« Je ne dis pas victimes. Je ne dis pas adversaires. Je dis : clients. »";
            d["narration.mos_1_4.n18.before.2"] = "« N'écoute pas. Il compte déjà ce qu'il peut vendre de nous. »";
            d["narration.mos_1_4.n18.after.0"] = "« Vous croyez avoir trouvé quelque chose ? On vous a laissé trouver. L'accueil, c'est moi qui l'ai organisé. »";
            d["narration.mos_1_4.n18.after.1"] = "« Alors c'était pour nous tenir loin de Kapotnya. »";
            d["narration.mos_1_4.n18.after.2"] = "« Alors c'est là que nous irons. »";

            // --- Fragments de souvenirs (annexe B de la spec) et archives de zone ---
            d["fragment.mos_maglev_welcome.title"] = "L'accueil parfait";
            d["fragment.mos_maglev_welcome.text"] = "Les hologrammes saluaient l'Aube par son nom, les quais étaient balayés, les sourires, parfaits. Un cargo scellé dormait pourtant sur la voie 9, sans nom et sans manifeste. Moscou t'ouvrait grand les bras, et regardait ailleurs.";
            d["fragment.mos_monastery_register.title"] = "Le registre sous la fresque";
            d["fragment.mos_monastery_register.text"] = "Tu as restauré mille fresques sans jamais chercher ce qu'elles cachaient. Celle-ci couvrait un registre : des noms, des dates, des livraisons. Quelqu'un avait peint par-dessus une vérité, et l'avait confiée aux saints.";
            d["fragment.mos_convoy_crates.title"] = "Des caisses marquées";
            d["fragment.mos_convoy_crates.text"] = "Le convoi filait sous la ville, éclairé comme une fête. Les caisses portaient un code que personne n'utilise plus. Il y avait des mains neuves sur de très vieilles recherches.";
            d["fragment.mos_depot_passeur.title"] = "Le Passeur";
            d["fragment.mos_depot_passeur.text"] = "Il ne disait jamais victime, jamais ennemi : seulement client. Dans son dépôt, tout avait un prix, même l'accueil qu'on t'avait réservé. Ce soir, tu as compris que la ville entière avait été invitée à regarder ailleurs.";
            d["archives.zone.zone_mos_1_1"] = "Gare du maglev : l'accueil de Moscou était trop parfait pour être honnête. Derrière les quais, un cargo scellé et sans manifeste, déposé trois jours avant l'arrivée de l'Aube.";
            d["archives.zone.zone_mos_1_2"] = "Monastère-archives : la fresque cachait un registre de livraisons, noms et dates compris. C'est la moitié d'une preuve ; l'autre moitié se trouve du côté des raffineries.";
            d["archives.zone.zone_mos_1_3"] = "Voie du maglev : un convoi qui n'existe pas filait sous la ville. Ses caisses portaient un code d'avant l'Union : quelqu'un déterre de très vieilles recherches.";
            d["archives.zone.zone_mos_1_4"] = "Dépôt du maglev : le Passeur, qui ne parle que de clients, avait organisé l'accueil pour tenir l'Aube loin de Kapotnya. C'est précisément là qu'elle ira.";

            // --- Carte de Moscou ---
            d["map.borough.sergueiv"] = "SERGUEIV";
            d["map.borough.kapotnya"] = "KAPOTNYA";
            d["map.borough.sadovoye"] = "SADOVOYE KOLTSO";
            d["map.borough.place_rouge"] = "PLACE ROUGE";
            d["map.chapter"] = "CHAPITRE {0}";
            d["map.chapter_soon"] = "CHAPITRE {0} · BIENTÔT";
        }

        /// <summary>Ajoute les textes anglais de Moscou au dictionnaire anglais (méthode statique sans état : sûre dans un initialiseur de champ).</summary>
        static Dictionary<string, string> WithMoscowEnglish(Dictionary<string, string> d)
        {
            // --- Enemies ---
            d["enemies.mos_guard.name"] = "Mob Guard";
            d["enemies.mos_smuggler.name"] = "Trafficker";
            d["enemies.mos_enforcer.name"] = "Enforcer";
            d["enemies.mos_passeur.name"] = "THE SMUGGLER";
            d["enemies.mos_passeur.phase2"] = "The Customers Arrive";
            d["enemies.mos_passeur.phase3"] = "Everything Must Go";
            d["skills.mos_guard_a1"] = "Rifle Butt";
            d["skills.mos_guard_a2"] = "Security Cordon";
            d["skills.mos_smuggler_a1"] = "Warning Shot";
            d["skills.mos_smuggler_a2"] = "Express Delivery";
            d["skills.mos_enforcer_a1"] = "Gloved Fist";
            d["skills.mos_enforcer_a2"] = "Courtesy Call";
            d["skills.mos_enforcer_a3"] = "Padded Coat";
            d["skills.mos_passeur_a1"] = "Haggling";
            d["skills.mos_passeur_a2"] = "Sealed Cargo";
            d["skills.mos_passeur_a3"] = "Delivery";
            d["skills.mos_passeur_a4"] = "Formal Notice";
            d["skills.mos_passeur_p2_a2"] = "Impatient Customers";
            d["archives.enemy.mos_guard"] = "Mob guard: a human cordon posted at the entrances. He raises a shield and draws the blows.";
            d["archives.enemy.mos_smuggler"] = "Smuggler: a gunman of the maglev route. He marks his target before delivering lead.";
            d["archives.enemy.mos_enforcer"] = "Enforcer: the elite of the welcoming committee. His courtesy calls rattle the whole squad.";
            d["archives.enemy.mos_passeur"] = "THE SMUGGLER: he never says victims or enemies, only customers. Everything he owns has a price.";

            // --- Zones (Sergueiv chapter) ---
            d["zones.mos_1_1.name"] = "MAGLEV STATION — THE WELCOME";
            d["zones.mos_1_1.short"] = "Maglev Station";
            d["zones.mos_1_1.code"] = "Zone M1.1";
            d["zones.mos_1_1.desc"] = "Moscow opens its arms wide: holograms greeting you by name, swept platforms, flawless smiles. Too flawless. Yet a sealed cargo sits on track 9, with no manifest. Make it through the official welcome and see what they are trying to hide.";
            d["zones.mos_1_2.name"] = "MONASTERY — THE ARCHIVES";
            d["zones.mos_1_2.short"] = "Monastery";
            d["zones.mos_1_2.code"] = "Zone M1.2";
            d["zones.mos_1_2.desc"] = "The cargo is on no manifest, but someone kept track of the deliveries: the monastery archives, closed to the public. Pass yourself off as the woman restoring the fresco and search what it hides.";
            d["zones.mos_1_3.name"] = "MAGLEV LINE — THE CONVOY";
            d["zones.mos_1_3.short"] = "Maglev Line";
            d["zones.mos_1_3.code"] = "Zone M1.3";
            d["zones.mos_1_3.desc"] = "The ledger leads to a convoy that does not exist, lit like a festival beneath the city. Catch up with it, break its escort and open the crates: they carry a code nobody uses anymore.";
            d["zones.mos_1_4.name"] = "MAGLEV DEPOT — THE SMUGGLER";
            d["zones.mos_1_4.short"] = "Depot";
            d["zones.mos_1_4.code"] = "Zone M1.4";
            d["zones.mos_1_4.desc"] = "The convoy ends here, in a depot where everything has a price. Its master never says victim or enemy, only customer. Search the depot, break its guard and face the Smuggler.";
            d["qg.fil.zone_mos_1_1"] = "Moscow welcomes you with open arms, and that is what worries us. Cross the station, spot the sealed cargo and get out before the welcoming committee closes in.";
            d["qg.fil.zone_mos_1_2"] = "On paper the cargo leads nowhere. The monastery archives know everything: go in, swivel the fresco and take the ledger.";
            d["qg.fil.zone_mos_1_3"] = "The ledger mentions a convoy beneath the city. Catch it on the maglev line, break its escort and open its crates.";
            d["qg.fil.zone_mos_1_4"] = "Everything converges on the maglev depot. The Smuggler is waiting; he arranged the welcome. Make it clear you are not a customer.";

            // --- Objectives ---
            d["objectives.mos11.welcome"] = "Get through the official welcome";
            d["objectives.mos11.platforms"] = "Slip behind the platforms";
            d["objectives.mos11.escort"] = "Take down the cargo escort";
            d["objectives.mos11.leave"] = "Leave the station";
            d["objectives.mos12.archives"] = "Enter the archives";
            d["objectives.mos12.fresco"] = "Swivel the fresco";
            d["objectives.mos12.guardian"] = "Defeat the archives guardian";
            d["objectives.mos12.register"] = "Take the ledger";
            d["objectives.mos13.convoy"] = "Catch up with the convoy";
            d["objectives.mos13.escort"] = "Break the convoy escort";
            d["objectives.mos13.crates"] = "Open the crates";
            d["objectives.mos14.depot"] = "Search the depot";
            d["objectives.mos14.guard"] = "Break the depot guard";
            d["objectives.mos14.passeur"] = "Defeat the Smuggler";

            // --- Dialogue (spec appendix A) ---
            d["speaker.agent_m"] = "Agent M";
            d["speaker.passeur"] = "The Smuggler";
            d["speaker.enforcer"] = "Enforcer";
            d["narration.mos_1_1.n14.before.0"] = "\"Welcome to Moscow. The welcoming committee kindly asks you to turn back.\"";
            d["narration.mos_1_1.n14.before.1"] = "\"A welcome that perfect is always hiding something.\"";
            d["narration.mos_1_1.n14.after.0"] = "\"This sealed cargo is on no manifest. I can see the trucks that left it here… three days before we arrived.\"";
            d["narration.mos_1_1.n19.after.0"] = "\"We leave through the monastery archives. If someone's waiting there, it isn't to welcome us.\"";
            d["narration.mos_1_2.n14.before.0"] = "\"The archives are closed to the public.\"";
            d["narration.mos_1_2.n14.before.1"] = "\"Not for the woman restoring the fresco.\"";
            d["narration.mos_1_2.n14.after.0"] = "\"Behind the fresco, a ledger. I saw it before I touched it.\"";
            d["narration.mos_1_2.n18.after.0"] = "\"A ledger: half a proof. The other half is out by the refineries.\"";
            d["narration.mos_1_3.n14.before.0"] = "\"This convoy doesn't exist. You saw nothing.\"";
            d["narration.mos_1_3.n14.after.0"] = "\"These crates carry a Soviet code. Someone is digging up old research.\"";
            d["narration.mos_1_3.n18.after.0"] = "\"A pre-Union code on crates from this year. Remember it.\"";
            d["narration.mos_1_4.n18.before.0"] = "\"You're early. My clients usually make an appointment.\"";
            d["narration.mos_1_4.n18.before.1"] = "\"I don't say victims. I don't say adversaries. I say: customers.\"";
            d["narration.mos_1_4.n18.before.2"] = "\"Don't listen. He's already counting what he can sell of us.\"";
            d["narration.mos_1_4.n18.after.0"] = "\"You think you found something? We let you find it. I arranged the welcome.\"";
            d["narration.mos_1_4.n18.after.1"] = "\"So that was to keep us away from Kapotnya.\"";
            d["narration.mos_1_4.n18.after.2"] = "\"Then that is where we go.\"";

            // --- Memory fragments (spec appendix B) and zone archives ---
            d["fragment.mos_maglev_welcome.title"] = "The Perfect Welcome";
            d["fragment.mos_maglev_welcome.text"] = "Holograms greeted The Dawn by name, the platforms were swept, the smiles flawless. Yet a sealed cargo sat on track 9, with no name and no manifest. Moscow opened its arms wide, and looked the other way.";
            d["fragment.mos_monastery_register.title"] = "The Ledger Under the Fresco";
            d["fragment.mos_monastery_register.text"] = "You have restored a thousand frescoes without ever looking for what they hid. This one covered a ledger: names, dates, deliveries. Someone had painted over the truth and entrusted it to the saints.";
            d["fragment.mos_convoy_crates.title"] = "Marked Crates";
            d["fragment.mos_convoy_crates.text"] = "The convoy raced beneath the city, lit like a festival. The crates bore a code nobody uses anymore. New hands on very old research.";
            d["fragment.mos_depot_passeur.title"] = "The Smuggler";
            d["fragment.mos_depot_passeur.text"] = "He never said victim, never enemy: only customer. In his depot everything had a price, even the welcome prepared for you. Tonight you understood the whole city had been invited to look elsewhere.";
            d["archives.zone.zone_mos_1_1"] = "Maglev station: Moscow's welcome was too perfect to be honest. Behind the platforms, a sealed cargo with no manifest, left three days before The Dawn arrived.";
            d["archives.zone.zone_mos_1_2"] = "Monastery archives: the fresco hid a ledger of deliveries, names and dates included. It is half a proof; the other half lies out by the refineries.";
            d["archives.zone.zone_mos_1_3"] = "Maglev line: a convoy that does not exist raced beneath the city. Its crates carried a pre-Union code: someone is digging up very old research.";
            d["archives.zone.zone_mos_1_4"] = "Maglev depot: the Smuggler, who speaks only of customers, had arranged the welcome to keep The Dawn away from Kapotnya. That is exactly where it will go.";

            // --- Moscow map ---
            d["map.borough.sergueiv"] = "SERGUEIV";
            d["map.borough.kapotnya"] = "KAPOTNYA";
            d["map.borough.sadovoye"] = "SADOVOYE KOLTSO";
            d["map.borough.place_rouge"] = "RED SQUARE";
            d["map.chapter"] = "CHAPTER {0}";
            d["map.chapter_soon"] = "CHAPTER {0} · COMING SOON";
            return d;
        }
    }
}
