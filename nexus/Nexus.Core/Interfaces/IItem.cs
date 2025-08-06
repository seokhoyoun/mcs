namespace Nexus.Core.Interfaces
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