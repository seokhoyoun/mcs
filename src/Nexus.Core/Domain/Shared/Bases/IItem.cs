namespace Nexus.Core.Domain.Shared.Bases
{
    /// <summary>
    /// ��� ��ǰ(��� ����, ���빰)�� �⺻ �������̽��Դϴ�.
    /// </summary>
    public interface IItem : IEntity
    {
        /// <summary>
        /// ��� ������ ���Ե� ��ǰ ���
        /// </summary>
        IReadOnlyList<IItem>? Items { get; }
    }
}