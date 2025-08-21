using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Domain.Shared.Bases;
using Nexus.Shared.Application.DTO;

namespace Nexus.Core.Domain.Models.Locations.Interfaces
{
    public interface ILocationRepository : IRepository<Location, string>
    {
        /// <summary>
        /// 특정 위치의 상태를 조회합니다.
        /// </summary>
        /// <param name="locationId">위치 ID</param>
        /// <returns>위치 상태 정보</returns>
        Task<LocationState> GetStateAsync(string locationId);
        
        /// <summary>
        /// 특정 타입의 모든 위치를 조회합니다.
        /// </summary>
        /// <typeparam name="T">위치 타입</typeparam>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>해당 타입의 위치 목록</returns>
        Task<IReadOnlyList<T>> GetLocationsByTypeAsync<T>(CancellationToken cancellationToken = default) where T : Location;
        
        /// <summary>
        /// 특정 영역의 모든 위치를 조회합니다.
        /// </summary>
        /// <param name="areaId">영역 ID</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>해당 영역의 위치 목록</returns>
        Task<IReadOnlyList<Location>> GetLocationsByAreaAsync(string areaId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// 특정 위치의 상태를 업데이트합니다.
        /// </summary>
        /// <param name="locationId">위치 ID</param>
        /// <param name="state">새 상태</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>업데이트 성공 여부</returns>
        Task<bool> UpdateLocationStateAsync(string locationId, LocationState state, CancellationToken cancellationToken = default);
    }
}
