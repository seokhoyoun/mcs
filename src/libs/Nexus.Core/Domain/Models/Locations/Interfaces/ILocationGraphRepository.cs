using Nexus.Core.Domain.Models.Locations.Graphs;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Locations.Interfaces
{
    /// <summary>
    /// LocationGraph 저장/조회용 저장소 인터페이스입니다.
    /// </summary>
    public interface ILocationGraphRepository
    {
        Task<LocationGraphSnapshot?> GetAsync(CancellationToken cancellationToken = default);

        Task SaveAsync(LocationGraphSnapshot snapshot, CancellationToken cancellationToken = default);
    }
}
