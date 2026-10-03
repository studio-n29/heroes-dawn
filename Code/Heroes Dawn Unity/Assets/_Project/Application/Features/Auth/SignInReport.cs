#nullable enable
namespace N29.HeroesDawn.Application.Features.Auth
{
    public sealed class SignInReport
    {
        public bool NewProfile { get; }
        public int FatigueRecovered { get; }
        public SignInReport(bool newProfile, int fatigueRecovered) { NewProfile = newProfile; FatigueRecovered = fatigueRecovered; }
    }
}
