namespace N29.HeroesDawn.Presentation.Hub
{
    public sealed class UnityAppQuit : IAppQuit
    {
        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}
