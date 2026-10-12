#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes de la carte de la ville, des zones 1.2 et 1.3 et de leurs ennemis.</summary>
    public static partial class Loc
    {
        static partial void ExtendMap(Dictionary<string, string> d)
        {
            d["page.citymap"] = "New York · Acte 1";
            d["map.act1"] = "ACTE 1 · PREMIÈRE NUIT";
            d["map.act"] = "ACTE {0}";
            d["map.act_soon"] = "ACTE {0} · BIENTÔT";
            d["map.borough.brooklyn"] = "BROOKLYN";
            d["map.borough.queens"] = "QUEENS";
            d["map.borough.bronx"] = "BRONX";
            d["map.borough.manhattan"] = "MANHATTAN";
            d["map.borough.staten"] = "STATEN ISLAND";
            d["map.status.Locked"] = "Verrouillée";
            d["map.status.Available"] = "Disponible";
            d["map.status.Done"] = "Validée";
            d["map.deploy"] = "DÉPLOYER";
            d["map.replay"] = "REJOUER";
            d["map.locked"] = "VERROUILLÉE";
            d["map.chips"] = "Niveau conseillé {0} · {1} cases · {2} combats";
            d["map.rewards"] = "Validation : {0} Crédits · {1} PR";
            d["map.locked_zone"] = "Valide d'abord : {0}.";
            d["map.locked_act"] = "{0} : ce quartier s'ouvrira dans une prochaine mise à jour.";
            d["errors.zone_locked"] = "Cette zone n'est pas encore ouverte.";

            d["zones.ny_1_1.short"] = "Red Hook";
            d["zones.ny_1_2.name"] = "BROWNSVILLE — LES OTAGES";
            d["zones.ny_1_2.short"] = "Brownsville";
            d["zones.ny_1_2.desc"] = "La situation est plus critique que prévu : l'AMIF traite avec Antonio Kribelchi, dit « Crocodile », un mafieux méta-humain qui trafique des méta-humains. L'AMIF les arrête pour les lui vendre et financer son armement. Votre équipe s'introduit à Brownsville pour libérer les otages.";
            d["zones.ny_1_3.name"] = "CONEY ISLAND — LE PIÈGE";
            d["zones.ny_1_3.short"] = "Coney Island";
            d["zones.ny_1_3.desc"] = "Après Brownsville, Crocodile et ses hommes défient la Guilde : ils ont pris Coney Island en otage pour attirer votre équipe. C'est un piège, mais il faut sauver les innocents.";
            d["zones.ny_1_4.name"] = "DUMBO ET LE PONT DE BROOKLYN";
            d["zones.ny_1_4.short"] = "Dumbo";
            d["zones.ny_1_4.code"] = "Zone 1.4";
            d["zones.ny_1_4.desc"] = "Après l'incident de Coney Island, l'AMIF a quadrillé Brooklyn pour neutraliser votre équipe et le gang de Crocodile d'un seul coup. Il faut percer le cordon et tenir le pont.";
            d["objectives.ny12.infiltrate"] = "S'introduire dans le quartier de Brownsville";
            d["objectives.ny12.lieutenant"] = "Neutraliser le lieutenant de Crocodile";
            d["objectives.ny12.hostages"] = "Libérer les otages (crocheter les cellules)";
            d["objectives.ny12.evacuate"] = "Mettre les otages à l'abri";
            d["objectives.ny13.civilians"] = "Évacuer les civils pris au piège";
            d["objectives.ny13.trap"] = "Briser le piège des hommes de Crocodile";
            d["objectives.ny13.crocodile"] = "Vaincre Crocodile";
            d["objectives.ny14.cordon"] = "Percer le cordon de l'AMIF";
            d["objectives.ny14.network"] = "Couper le réseau de surveillance de l'AMIF";
            d["objectives.ny14.bridge"] = "Tenir le pont face au Titan de l'AMIF";
            d["qg.fil.zone_ny_1_1"] = "L'AMIF fait transiter des méta-humains kidnappés par Red Hook. Infiltre le quai, coupe le courant, trouve les documents de transport, arrête l'échange et fuis avec les preuves.";
            d["qg.fil.zone_ny_1_2"] = "Les preuves mènent à Crocodile, un mafieux méta-humain à qui l'AMIF vend ses prisonniers. Infiltre Brownsville, neutralise son lieutenant et libère les otages.";
            d["qg.fil.zone_ny_1_3"] = "Crocodile a pris Coney Island en otage : c'est un piège. Évacue les civils, brise le piège de ses hommes et affronte-le.";
            d["qg.fil.zone_ny_1_4"] = "L'AMIF a quadrillé Brooklyn. Perce son cordon, coupe son réseau de surveillance et tiens le pont : Rayner a un atout à jouer.";

            d["enemies.sniper.name"] = "Tireur d'élite";
            d["enemies.canal_chief.name"] = "Lieutenant de Crocodile";
            d["enemies.bridge_titan.name"] = "TITAN DE L'AMIF";
            d["enemies.crocodile.name"] = "CROCODILE";
            d["enemies.amif_trooper.name"] = "Soldat de l'AMIF";
            d["enemies.amif_marksman.name"] = "Tireur de l'AMIF";
            d["enemies.amif_captain.name"] = "Capitaine de l'AMIF";
            d["skills.crocodile_a1"] = "Morsure d'acier";
            d["skills.crocodile_a2"] = "Coup de queue";
            d["skills.crocodile_a3"] = "Cuirasse d'écailles";
            d["skills.amif_trooper_a1"] = "Tir de fusil";
            d["skills.amif_trooper_a2"] = "Tir de barrage";
            d["skills.amif_marksman_a1"] = "Tir ajusté";
            d["skills.amif_marksman_a2"] = "Balle traçante";
            d["skills.amif_captain_a1"] = "Coup de crosse";
            d["skills.amif_captain_a2"] = "Ordre de charge";
            d["skills.amif_captain_a3"] = "Bouclier tactique";
            d["skills.sniper_a1"] = "Tir ajusté";
            d["skills.sniper_a2"] = "Balle marquante";
            d["skills.canal_chief_a1"] = "Crochet du docker";
            d["skills.canal_chief_a2"] = "Vague d'acide";
            d["skills.canal_chief_a3"] = "Garde de fer";
            d["skills.bridge_titan_a1"] = "Poing d'acier";
            d["skills.bridge_titan_a2"] = "Pile du pont";
            d["skills.bridge_titan_a3"] = "Rupture de câble";
        }
    }
}
