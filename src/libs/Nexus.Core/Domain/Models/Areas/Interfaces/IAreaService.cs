using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Shared.Bases;

namespace Nexus.Core.Domain.Models.Areas.Interfaces
{
    public interface IAreaService : IDataService<Area, string>
    {
        Task<Area?> GetAvailableAreaForCassetteAsync(CancellationToken cancellationToken = default);

        Task<CassetteLocation?> GetAvailableCassetteLocationAsync(Area area, CancellationToken cancellationToken = default);

        Task<Area?> GetEmptyAreaForCassettesAsync(int requiredCassetteSlots, CancellationToken cancellationToken = default);
    }
}
