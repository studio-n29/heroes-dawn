#nullable enable
using N29.HeroesDawn.Presentation.Hub;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>Confirmation d'achat d'un héros (M31) : héros, combo phare, solde avant / coût / après, ou manque et piste pour gagner des PR.</summary>
    public struct RecruitConfirmData
    {
        public string Title;
        public MemberCardData Card;
        public string Combo;
        public string Before;
        public string Cost;
        public string After;
        public bool CanAfford;
        /// <summary>« Il manque 290 PR » quand le solde ne suffit pas ; vide sinon.</summary>
        public string Missing;
        public string HowToEarn;
        public string Ok;
        public string Cancel;
    }
}
