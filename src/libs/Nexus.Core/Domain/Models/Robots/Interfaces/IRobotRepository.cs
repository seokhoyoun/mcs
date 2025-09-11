using Nexus.Core.Domain.Shared.Bases;

namespace Nexus.Core.Domain.Models.Robots.Interfaces
{
    public interface IRobotRepository : IRepository<Robots.Robot, string>
    {
        /// <summary>
        /// 로봇의 위치 정보만 업데이트합니다.
        /// </summary>
        /// <param name="robotId">로봇 ID</param>
        /// <param name="position">새로운 위치</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>업데이트 성공 여부</returns>
        Task<bool> UpdatePositionAsync(string robotId, Position position, CancellationToken cancellationToken = default);

        /// <summary>
        /// 로봇의 현재 위치를 조회합니다.
        /// </summary>
        /// <param name="robotId">로봇 ID</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>로봇의 위치 정보</returns>
        Task<Position?> GetPositionAsync(string robotId, CancellationToken cancellationToken = default);
    }
}
