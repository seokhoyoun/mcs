using Nexus.Core.Domain.Models.Locations.Base;

namespace Nexus.Core.Domain.Models.Locations.Interfaces
{
    /// <summary>
    /// ��ġ ������ ���� ���� �������̽�
    /// </summary>
    public interface ILocationService
    {
        /// <summary>
        /// ���� ��ġ�� ���񽺿� �߰��մϴ�.
        /// </summary>
        /// <param name="locations">�߰��� ��ġ ���</param>
        void AddLocations(IEnumerable<Location> locations);

        /// <summary>
        /// ID�� ī��Ʈ ��ġ�� ��ȸ�մϴ�.
        /// </summary>
        /// <param name="id">ī��Ʈ ��ġ ID</param>
        /// <returns>�ش� ID�� ī��Ʈ ��ġ �Ǵ� null</returns>
        CassetteLocation? GetCassetteLocationById(string id);

        /// <summary>
        /// ID�� Ʈ���� ��ġ�� ��ȸ�մϴ�.
        /// </summary>
        /// <param name="id">Ʈ���� ��ġ ID</param>
        /// <returns>�ش� ID�� Ʈ���� ��ġ �Ǵ� null</returns>
        TrayLocation? GetTrayLocationById(string id);

        /// <summary>
        /// ID�� �޸� ��ġ�� ��ȸ�մϴ�.
        /// </summary>
        /// <param name="id">�޸� ��ġ ID</param>
        /// <returns>�ش� ID�� �޸� ��ġ �Ǵ� null</returns>
        MemoryLocation? GetMemoryLocationById(string id);

        /// <summary>
        /// ����ҿ��� LocationState�� ��ȸ�Ͽ� Location ��ü�� ���¸� ����ȭ�մϴ�.
        /// </summary>
        /// <param name="locationId">����ȭ�� Location�� ID</param>
        Task RefreshLocationStateAsync(string locationId);
    }
}