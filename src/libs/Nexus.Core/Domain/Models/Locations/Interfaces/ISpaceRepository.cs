using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Shared.Bases;

namespace Nexus.Core.Domain.Models.Locations.Interfaces
{
    /// <summary>
    /// Space 저장소 인터페이스입니다.
    /// </summary>
    public interface ISpaceRepository : IRepository<Space, string>
    {
    }
}
