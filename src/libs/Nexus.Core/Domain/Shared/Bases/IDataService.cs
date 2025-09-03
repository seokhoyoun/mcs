using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Shared.Bases
{
    /// <summary>
    /// �⺻ ������ ���� ���� �������̽�
    /// </summary>
    /// <typeparam name="T">������ ��ƼƼ Ÿ��</typeparam>
    /// <typeparam name="TKey">��ƼƼ�� �� �ĺ��� Ÿ��</typeparam>
    public interface IDataService<T, TKey> : IService where T : class, IEntity
    {
        /// <summary>
        /// ��� ��ƼƼ�� ��ȸ�մϴ�.
        /// </summary>
        /// <param name="cancellationToken">�۾� ��� ��ū</param>
        /// <returns>��ƼƼ ���</returns>
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// �־��� �ĺ��ڷ� ��ƼƼ�� ��ȸ�մϴ�.
        /// </summary>
        /// <param name="id">��ƼƼ �ĺ���</param>
        /// <param name="cancellationToken">�۾� ��� ��ū</param>
        /// <returns>�ĺ��ڿ� �ش��ϴ� ��ƼƼ �Ǵ� null</returns>
        Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

        /// <summary>
        /// ���ο� ��ƼƼ�� �߰��մϴ�.
        /// </summary>
        /// <param name="entity">�߰��� ��ƼƼ</param>
        /// <param name="cancellationToken">�۾� ��� ��ū</param>
        /// <returns>�߰��� ��ƼƼ</returns>
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// ���� ��ƼƼ�� ������Ʈ�մϴ�.
        /// </summary>
        /// <param name="entity">������Ʈ�� ��ƼƼ</param>
        /// <param name="cancellationToken">�۾� ��� ��ū</param>
        /// <returns>������Ʈ�� ��ƼƼ</returns>
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// �־��� �ĺ����� ��ƼƼ�� �����մϴ�.
        /// </summary>
        /// <param name="id">������ ��ƼƼ�� �ĺ���</param>
        /// <param name="cancellationToken">�۾� ��� ��ū</param>
        /// <returns>���� ���� ����</returns>
        Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default);

    
    }
}