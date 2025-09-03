using Nexus.Core.Domain.Shared.Events;

namespace Nexus.Core.Domain.Shared.Bases
{
    /// <summary>
    /// ���� �ĺ��ڸ� ������ ��ü�� ���� �������̽��Դϴ�.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// ���� �ĺ���
        /// </summary>
        string Id { get; }

        /// <summary>
        /// �̸�
        /// </summary>
        string Name { get; }
     
    }
}