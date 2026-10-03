namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>Contenu d'une confirmation (textes localisés).</summary>
    public readonly struct ConfirmData
    {
        public readonly string Title;
        public readonly string Body;
        public readonly string Ok;
        public readonly string Cancel;
        public readonly Tone OkTone;

        public ConfirmData(string title, string body, string ok, string cancel, Tone okTone)
        {
            Title = title; Body = body; Ok = ok; Cancel = cancel; OkTone = okTone;
        }
    }
}
