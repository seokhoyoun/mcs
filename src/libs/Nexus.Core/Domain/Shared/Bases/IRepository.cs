using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Shared.Bases
{
    /// <summary>
    /// �⺻ Repository ������ �����ϴ� ���׸� �������̽��Դϴ�.
    /// </summary>
    /// <typeparam name="T">��ƼƼ Ÿ��</typeparam>
    /// <typeparam name="TKey">��ƼƼ �ĺ��� Ÿ��</typeparam>
    public interface IRepository<T, TKey> where T : class, IEntity
    {
        /// <summary>
        /// ��� ��ƼƼ�� ��ȸ�մϴ�.
        /// </summary>
        /// <param name="cancellationToken">�۾� ��� ��ū</param>
        /// <returns>��ƼƼ ���</returns>
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// ������ ������ �����ϴ� ��ƼƼ�� ��ȸ�մϴ�.
        /// </summary>
        /// <param name="predicate">��ȸ ����</param>
        /// <param name="cancellationToken">�۾� ��� ��ū</param>
        /// <returns>������ �����ϴ� ��ƼƼ ���</returns>
        Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

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
        /// ���� ��ƼƼ�� �ϰ� �߰��մϴ�.
        /// </summary>
        /// <param name="entities">�߰��� ��ƼƼ ���</param>
        /// <param name="cancellationToken">�۾� ��� ��ū</param>
        /// <returns>�߰��� ��ƼƼ ���</returns>
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// ���� ��ƼƼ�� ������Ʈ�մϴ�.
        /// </summary>
        /// <param name="entity">������Ʈ�� ��ƼƼ</param>
        /// <param name="cancellationToken">�۾� ��� ��ū</param>
        /// <returns>������Ʈ�� ��ƼƼ</returns>
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// ���� ��ƼƼ�� �ϰ� ������Ʈ�մϴ�.
        /// </summary>
        /// <param name="entities">������Ʈ�� ��ƼƼ ���</param>
        /// <param name="cancellationToken">�۾� ��� ��ū</param>
        /// <returns>�۾� �Ϸ� ����</returns>
        Task<bool> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// �־��� �ĺ����� ��ƼƼ�� �����մϴ�.
        /// </summary>
        /// <param name="id">������ ��ƼƼ�� �ĺ���</param>
        /// <param name="cancellationToken">�۾� ��� ��ū</param>
        /// <returns>���� ���� ����</returns>
        Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default);

        /// <summary>
        /// �־��� ��ƼƼ�� �����մϴ�.
        /// </summary>
        /// <param name="entity">������ ��ƼƼ</param>
        /// <param name="cancellationToken">�۾� ��� ��ū</param>
        /// <returns>���� ���� ����</returns>
        Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// ���� ��ƼƼ�� �ϰ� �����մϴ�.
        /// </summary>
        /// <param name="entities">������ ��ƼƼ ���</param>
        /// <param name="cancellationToken">�۾� ��� ��ū</param>
        /// <returns>�۾� �Ϸ� ����</returns>
        Task<bool> DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// ������ ������ �����ϴ� ��ƼƼ�� �����ϴ��� Ȯ���մϴ�.
        /// </summary>
        /// <param name="predicate">Ȯ���� ����</param>
        /// <param name="cancellationToken">�۾� ��� ��ū</param>
        /// <returns>������ �����ϴ� ��ƼƼ�� ������ true, ������ false</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// ������ ������ �����ϴ� ��ƼƼ�� ������ ��ȯ�մϴ�.
        /// </summary>
        /// <param name="predicate">����</param>
        /// <param name="cancellationToken">�۾� ��� ��ū</param>
        /// <returns>������ �����ϴ� ��ƼƼ ����</returns>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    }
}