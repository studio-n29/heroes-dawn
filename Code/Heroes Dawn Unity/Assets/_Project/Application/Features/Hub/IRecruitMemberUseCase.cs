#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Hub
{
    /// <summary>Recrute un membre contre des PR ; il rejoint l'escouade s'il y a de la place.</summary>
    public interface IRecruitMemberUseCase
    {
        UniTask<Result> ExecuteAsync(MemberId id, CancellationToken ct = default);
    }
}
