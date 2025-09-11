using Nexus.Core.Domain.Shared.Bases;

namespace Nexus.Core.Domain.Models.Robots.Interfaces
{
    /// <summary>
    /// 로봇 도메인 서비스 인터페이스
    /// </summary>
    public interface IRobotService : IDataService<Robot, string>
    {
        /// <summary>
        /// 메모리에 적재된 모든 로봇 컬렉션
        /// </summary>
        IReadOnlyList<Robot> Robots { get; }

        /// <summary>
        /// 로봇의 현재 위치를 갱신합니다.
        /// </summary>
        /// <param name="robotId">로봇 ID</param>
        /// <param name="position">새 위치</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>업데이트 성공 여부</returns>
        Task<bool> UpdatePositionAsync(string robotId, Position position, CancellationToken cancellationToken = default);

        /// <summary>
        /// 로봇의 현재 위치를 조회합니다.
        /// </summary>
        /// <param name="robotId">로봇 ID</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>현재 위치 또는 null</returns>
        Task<Position?> GetPositionAsync(string robotId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 초기화로 적재된 캐시에서 로봇을 조회합니다.
        /// </summary>
        /// <param name="id">로봇 ID</param>
        /// <returns>로봇 또는 null</returns>
        Robot? GetRobotByIdCached(string id);
    }
}

