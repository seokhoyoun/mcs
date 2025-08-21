using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Shared.Bases;

namespace Nexus.Core.Domain.Models.Areas.Interfaces
{
    public interface IAreaRepository : IRepository<Area, string>
    {
        /// <summary>
        /// 모든 에어리어 데이터를 초기화합니다.
        /// </summary>
        /// <param name="areas">초기화할 에어리어 목록</param>
        /// <returns>완료 Task</returns>
        Task InitializeAreasAsync(IEnumerable<Area> areas);
        
        /// <summary>
        /// 특정 에어리어에 있는 모든 세트를 조회합니다.
        /// </summary>
        /// <param name="areaId">에어리어 ID</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>세트 목록</returns>
        Task<IReadOnlyList<Set>> GetSetsByAreaIdAsync(string areaId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// 특정 에어리어의 사용 가능한 카세트 포트를 조회합니다.
        /// </summary>
        /// <param name="areaId">에어리어 ID</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>사용 가능한 카세트 포트 목록</returns>
        Task<IReadOnlyList<CassetteLocation>> GetAvailableCassettePortsAsync(string areaId, CancellationToken cancellationToken = default);
    }
}
