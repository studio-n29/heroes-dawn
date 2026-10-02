using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    public sealed class LinkDef
    {
        public CaseId A { get; }
        public CaseId B { get; }
        public LinkDef(CaseId a, CaseId b) { A = a; B = b; }
    }
}
