using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Domain.Shared.Bases;
using Nexus.Shared.Application.DTO;
using System.Collections.Generic;

public interface ITransportsRepository : IRepository<ITransportable, string>
{
    IEnumerable<CassetteState> GetAllCassettes();
    CassetteState? GetCassetteById(string id);
    void SaveCassette(CassetteState cassette);
    void DeleteCassette(string id);

    IEnumerable<TrayState> GetAllTrays();
    TrayState? GetTrayById(string id);
    void SaveTray(TrayState tray);
    void DeleteTray(string id);

    IEnumerable<MemoryState> GetAllMemories();
    MemoryState? GetMemoryById(string id);
    void SaveMemory(MemoryState memory);
    void DeleteMemory(string id);

    // Set 자료구조 활용을 위한 추가 메서드
    void AddTrayToCassette(string cassetteId, string trayId);
    void RemoveTrayFromCassette(string cassetteId, string trayId);
    void AddMemoryToTray(string trayId, string memoryId);
    void RemoveMemoryFromTray(string trayId, string memoryId);
}
