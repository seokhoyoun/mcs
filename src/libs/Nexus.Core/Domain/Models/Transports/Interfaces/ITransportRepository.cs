using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Models.Transports.DTO;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Domain.Shared.Bases;
using System.Collections.Generic;

public interface ITransportRepository : IRepository<ITransportable, string>
{
    // Set 자료구조 활용을 위한 추가 메서드
    void AddTrayToCassette(string cassetteId, string trayId);
    void RemoveTrayFromCassette(string cassetteId, string trayId);
    void AddMemoryToTray(string trayId, string memoryId);
    void RemoveMemoryFromTray(string trayId, string memoryId);

    Task<IReadOnlyList<Cassette>> GetCassettesWithoutTraysAsync();
    Task<IReadOnlyList<Tray>> GetTraysWithoutMemoriesAsync(string cassetteId);
    Task<IReadOnlyList<Memory>> GetMemoriesAsync(string trayId);

    // Convenience accessor for UI mapping without altering models
    Task<string?> GetMemoryLocationIdAsync(string memoryId);

    Task<CassetteHierarchyDto> GetCassetteHierarchyAsync();
}
