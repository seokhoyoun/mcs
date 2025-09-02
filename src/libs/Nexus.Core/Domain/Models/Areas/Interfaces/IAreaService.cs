using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Shared.Bases;

namespace Nexus.Core.Domain.Models.Areas.Interfaces
{
    /// <summary>
    /// ����� ������ ���� ���� �������̽�
    /// </summary>
    public interface IAreaService : IDataService<Area, string>
    {
        /// <summary>
        /// ���� ���� ���� ��� ����� ����� �����ɴϴ�.
        /// </summary>
        IReadOnlyList<Area> Areas { get; }

        /// <summary>
        /// ī��Ʈ ���簡 ������ Area�� ��ȸ�մϴ�.
        /// </summary>
        Area? GetAvailableAreaForCassette();

        /// <summary>
        /// ������ Area���� ��� ������ ī��Ʈ ��Ʈ�� ��ȸ�մϴ�.
        /// </summary>
        CassetteLocation? GetAvailableCassettePort(Area area);
    }
}