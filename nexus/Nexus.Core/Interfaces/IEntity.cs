namespace Nexus.Core.Interfaces
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