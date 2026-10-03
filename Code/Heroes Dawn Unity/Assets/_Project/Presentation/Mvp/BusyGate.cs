namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>Garde anti-doublon : une seule action à la fois (double clic, raccourci répété).</summary>
    public sealed class BusyGate
    {
        public bool IsBusy { get; private set; }

        /// <summary>Entre si libre ; false si une action est déjà en cours.</summary>
        public bool TryEnter()
        {
            if (IsBusy) return false;
            IsBusy = true;
            return true;
        }

        public void Exit() => IsBusy = false;
    }
}
