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
        /// 특정 에어리어에 있는 모든 세트를 조회합니다.
        /// </summary>
        /// <param name="areaId">에어리어 ID</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>세트 목록</returns>
        Task<IReadOnlyList<Set>> GetSetsByAreaIdAsync(string areaId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// 주어진 Area 목록을 저장소에 초기화(업서트)합니다.
        /// </summary>
        /// <param name="areas">초기화할 Areas</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        Task InitializeAreasAsync(IEnumerable<Area> areas, CancellationToken cancellationToken = default);
        
    }
}
