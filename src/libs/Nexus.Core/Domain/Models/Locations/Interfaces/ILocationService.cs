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
        /// ID로 로봇 위치를 조회합니다.
        /// </summary>
        /// <param name="id">로봇 위치 ID</param>
        /// <returns>해당 ID의 로봇 위치 또는 null</returns>
        RobotLocation? GetRobotLocationById(string id);

        /// <summary>
        /// 저장소에서 LocationState를 조회하여 Location 객체의 상태를 동기화합니다.
        /// </summary>
        /// <param name="locationId">동기화할 Location의 ID</param>
        Task RefreshLocationStateAsync(string locationId);
    }
}