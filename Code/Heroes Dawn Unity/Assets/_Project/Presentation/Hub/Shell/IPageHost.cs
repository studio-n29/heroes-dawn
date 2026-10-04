#nullable enable
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Zone de page de la coque : y instancie un prefab de page, puis le détruit.</summary>
    public interface IPageHost
    {
        GameObject Spawn(GameObject pagePrefab);
        void Despawn(GameObject page);
    }
}
