using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Domain.Shared.Bases;

namespace Nexus.Core.Domain.Models.Transports.Interfaces
{
    /// <summary>
    /// 운송 아이템 관리를 위한 서비스 인터페이스
    /// </summary>
    public interface ITransportService : IDataService<ITransportable, string>
    {
        IReadOnlyList<Cassette> GetAllCassettes();

        IReadOnlyList<Tray> GetAllTrays();

        IReadOnlyList<Memory> GetAllMemories();

        IReadOnlyList<ITransportable> GetAllTransports();
    }
}