using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    public sealed class ObjectiveDef
    {
        public string Id { get; }
        public ObjectiveType Type { get; }
        public CaseId Target { get; }
        public string NameKey { get; }
        public ObjectiveDef(string id, ObjectiveType type, CaseId target, string nameKey) { Id = id; Type = type; Target = target; NameKey = nameKey; }
    }
}
