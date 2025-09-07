using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Shared.Bases;

namespace Nexus.Core.Domain.Models.Locations.Interfaces
{
    public interface ILocationRepository : IRepository<Location, string>
    {
        /// <summary>
        /// 위치 타입(enum)으로 구분하여 모든 위치를 조회합니다.
        /// </summary>
        /// <param name="locationType">위치 타입</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>해당 타입의 위치 목록</returns>
        Task<IReadOnlyList<Location>> GetLocationsByTypeAsync(ELocationType locationType, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// 특정 영역의 모든 위치를 조회합니다.
        /// </summary>
        /// <param name="areaId">영역 ID</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>해당 영역의 위치 목록</returns>
        Task<IReadOnlyList<Location>> GetLocationsByAreaAsync(string areaId, CancellationToken cancellationToken = default);
        
        
    }
}
