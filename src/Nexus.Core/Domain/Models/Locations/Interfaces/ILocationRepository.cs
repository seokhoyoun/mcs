using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Shared.Application.DTO;

namespace Nexus.Core.Domain.Models.Locations.Interfaces
{
    public interface ILocationRepository
    {
        Task<LocationState> GetStateAsync(string locationId);
    }
}
