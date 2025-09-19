using Nexus.Core.Domain.Models.Lots;
using Nexus.Core.Domain.Models.Lots.Enums;
using Nexus.Core.Domain.Shared.Bases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Lots.Interfaces
{
    public interface ILotRepository : IRepository<Lot, string>
    {
        /// <summary>
        /// 특정 상태의 모든 Lot을 조회합니다.
        /// </summary>
        /// <param name="status">조회할 Lot 상태</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>해당 상태의 Lot 목록</returns>
        Task<IReadOnlyList<Lot>> GetLotsByStatusAsync(ELotStatus status, CancellationToken cancellationToken = default);

        /// <summary>
        /// 특정 시간 범위 내에 생성된 Lot을 조회합니다.
        /// </summary>
        /// <param name="startDate">시작 일시</param>
        /// <param name="endDate">종료 일시</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>해당 기간에 생성된 Lot 목록</returns>
        Task<IReadOnlyList<Lot>> GetLotsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// 특정 Lot에 Step을 추가합니다.
        /// </summary>
        /// <param name="lotId">Lot ID</param>
        /// <param name="lotStep">추가할 LotStep</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>성공 여부</returns>
        Task<bool> AddLotStepAsync(string lotId, LotStep lotStep, CancellationToken cancellationToken = default);

  
        Task<IReadOnlyList<LotStep>> GetLotStepsByLotIdAsync(string lotId, CancellationToken cancellationToken = default);


     
    }
}
