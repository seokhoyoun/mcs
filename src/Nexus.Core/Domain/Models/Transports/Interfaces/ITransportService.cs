using Nexus.Core.Domain.Models.Transports.Interfaces;

namespace Nexus.Core.Domain.Models.Transports.Interfaces
{
    /// <summary>
    /// ��� ������ ������ ���� ���� �������̽�
    /// </summary>
    public interface ITransportService
    {
        /// <summary>
        /// ID�� ��� ������ �������� ��ȸ�մϴ�.
        /// </summary>
        /// <param name="currentItemId">������ ID</param>
        /// <returns>�ش� ID�� ��� ������ ������ �Ǵ� null</returns>
        ITransportable? GetItemById(string currentItemId);
    }
}