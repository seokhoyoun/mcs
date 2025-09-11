using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Shared.Bases;

namespace Nexus.Core.Domain.Models.Locations.Interfaces
{
    /// <summary>
    /// 위치 관리를 위한 서비스 인터페이스
    /// </summary>
    public interface ILocationService : IService
    {
        /// <summary>
        /// 여러 위치를 서비스에 추가합니다.
        /// </summary>
        /// <param name="locations">추가할 위치 목록</param>
        void AddLocations(IEnumerable<Location> locations);

        /// <summary>
        /// ID로 카세트 위치를 조회합니다.
        /// </summary>
        /// <param name="id">카세트 위치 ID</param>
        /// <returns>해당 ID의 카세트 위치 또는 null</returns>
        CassetteLocation? GetCassetteLocationById(string id);

        /// <summary>
        /// ID로 트레이 위치를 조회합니다.
        /// </summary>
        /// <param name="id">트레이 위치 ID</param>
        /// <returns>해당 ID의 트레이 위치 또는 null</returns>
        TrayLocation? GetTrayLocationById(string id);

        /// <summary>
        /// ID로 메모리 위치를 조회합니다.
        /// </summary>
        /// <param name="id">메모리 위치 ID</param>
        /// <returns>해당 ID의 메모리 위치 또는 null</returns>
        MemoryLocation? GetMemoryLocationById(string id);

        /// <summary>
        /// ID로 마커 위치를 조회합니다. (포지션 전용)
        /// </summary>
        /// <param name="id">마커 위치 ID</param>
        /// <returns>해당 ID의 마커 위치 또는 null</returns>
        MarkerLocation? GetMarkerLocationById(string id);

     
    }
}
