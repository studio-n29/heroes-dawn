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
            d["zones.ny_1_2.name"] = "GOWANUS — LE CANAL";
            d["zones.ny_1_2.short"] = "Gowanus";
            d["zones.ny_1_2.desc"] = "La crue a noyé le canal et le gang en profite pour piller les entrepôts. Une famille est coincée au deuxième étage d'un immeuble.";
            d["zones.ny_1_3.name"] = "DUMBO — LE PONT";
            d["zones.ny_1_3.short"] = "Dumbo";
            d["zones.ny_1_3.desc"] = "Le Titan du pont tient le passage vers le reste de la ville. Seule une escouade rodée le fera tomber.";
            d["objectives.ny12.chief"] = "Battre le chef du gang du canal";
            d["objectives.ny12.pump"] = "Relancer la pompe du canal";
            d["objectives.ny12.bridge"] = "Atteindre la passerelle";
            d["objectives.ny13.titan"] = "Vaincre le Titan du pont";
            d["objectives.ny13.cables"] = "Rétablir les câbles du pont";
            d["objectives.ny13.lieutenant"] = "Battre le lieutenant du Titan";
            d["qg.fil.zone_ny_1_1"] = "La bande de Brick tient les quais de Red Hook. Coupe le générateur, atteins la grue du terminal, puis fais tomber le chef du gang.";
            d["qg.fil.zone_ny_1_2"] = "Le canal de Gowanus a débordé. Relance la pompe, bats le chef du gang et rejoins la passerelle.";
            d["qg.fil.zone_ny_1_3"] = "Dumbo est la dernière marche de l'Acte 1 : le Titan du pont ne se laissera pas faire. Rétablis les câbles avant de l'affronter.";

            d["enemies.sniper.name"] = "Tireur d'élite";
            d["enemies.canal_chief.name"] = "Chef du canal";
            d["enemies.bridge_titan.name"] = "TITAN DU PONT";
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
