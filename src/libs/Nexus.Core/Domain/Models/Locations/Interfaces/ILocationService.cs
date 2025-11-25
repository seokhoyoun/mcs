using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Core.Domain.Models.Locations;
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
        /// ID로 Carrier 위치를 조회합니다.
        /// </summary>
        /// <param name="id">Carrier 위치 ID</param>
        /// <returns>해당 ID의 Carrier 위치 또는 null</returns>
        Task<CarrierLocation?> GetCarrierLocationByIdAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// ID로 마커 위치를 조회합니다. (포지션 전용)
        /// </summary>
        /// <param name="id">마커 위치 ID</param>
        /// <returns>해당 ID의 마커 위치 또는 null</returns>
        Task<MarkerLocation?> GetMarkerLocationByIdAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// 현재 아이템 ID로 Carrier 위치를 조회합니다.
        /// </summary>
        /// <param name="itemId">위치에 적재된 아이템 ID</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>해당 아이템을 보유한 Carrier 위치 또는 null</returns>
        Task<CarrierLocation?> FindCarrierLocationByItemIdAsync(string itemId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 위치에 아이템을 할당합니다. 이미 점유 중이면 false를 반환합니다.
        /// </summary>
        /// <param name="locationId">위치 ID</param>
        /// <param name="itemId">아이템 ID</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>성공 여부</returns>
        Task<bool> TryAssignItemAsync(string locationId, string itemId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 위치에서 아이템을 해제합니다. 비어있으면 false를 반환합니다.
        /// </summary>
        /// <param name="locationId">위치 ID</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>성공 여부</returns>
        Task<bool> TryClearItemAsync(string locationId, CancellationToken cancellationToken = default);

    
    }
}
