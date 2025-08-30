using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Shared.Bases;

namespace Nexus.Core.Domain.Models.Areas.Interfaces
{
    /// <summary>
    /// 에어리어 관리를 위한 서비스 인터페이스
    /// </summary>
    public interface IAreaService : IDataService<Area, string>
    {
        /// <summary>
        /// 현재 관리 중인 모든 에어리어 목록을 가져옵니다.
        /// </summary>
        IReadOnlyList<Area> Areas { get; }

        /// <summary>
        /// 에어리어 서비스를 초기화합니다.
        /// 로컬 파일에서 에어리어 데이터를 로드하고 위치 서비스에 등록합니다.
        /// </summary>
        Task InitializeAreaService();

        /// <summary>
        /// 카세트 적재가 가능한 Area를 조회합니다.
        /// </summary>
        Area? GetAvailableAreaForCassette();

        /// <summary>
        /// 지정된 Area에서 사용 가능한 카세트 포트를 조회합니다.
        /// </summary>
        CassetteLocation? GetAvailableCassettePort(Area area);
    }
}