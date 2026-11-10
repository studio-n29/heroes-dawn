using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Positions des agents (remplacent les rôles) — bloc A de la 1.0.0.</summary>
    public sealed class PositionTests
    {
        [Test, Description("Vérifie que les valeurs de Position gardent celles de l'ancien rôle (0 à 4) — parce que agents.json et les sauvegardes stockent l'entier")]
        public void Les_valeurs_de_Position_gardent_celles_de_l_ancien_role()
        {
            Assert.That((int)Position.FrontLine, Is.EqualTo(0));
            Assert.That((int)Position.Attacker, Is.EqualTo(1));
            Assert.That((int)Position.Controller, Is.EqualTo(2));
            Assert.That((int)Position.Support, Is.EqualTo(3));
            Assert.That((int)Position.Ranged, Is.EqualTo(4));
        }
    }
}
