using Nexus.Core.Domain.Models.Transports.Interfaces;

namespace Nexus.Core.Domain.Models.Transports.Interfaces
{
    /// <summary>
    /// 운송 아이템 관리를 위한 서비스 인터페이스
    /// </summary>
    public interface ITransportService
    {
        /// <summary>
        /// ID로 운송 가능한 아이템을 조회합니다.
        /// </summary>
        /// <param name="currentItemId">아이템 ID</param>
        /// <returns>해당 ID의 운송 가능한 아이템 또는 null</returns>
        ITransportable? GetItemById(string currentItemId);
    }
}