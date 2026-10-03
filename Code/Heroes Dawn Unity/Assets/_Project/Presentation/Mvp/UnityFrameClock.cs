using UnityEngine;

namespace N29.HeroesDawn.Presentation.Mvp
{
    public sealed class UnityFrameClock : IFrameClock
    {
        public float UnscaledDeltaTime => Time.unscaledDeltaTime;
    }
}
