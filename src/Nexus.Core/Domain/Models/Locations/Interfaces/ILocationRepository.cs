using Nexus.Core.Domain.Models.Transports.Interfaces;

namespace Nexus.Core.Domain.Models.Locations.Interfaces
{
    public interface ILocationRepository
    {
        Task<IEnumerable<Location<ITransportable>>> GetAllAsync();
    }
}
