using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Shared.Bases
{
    /// <summary>
    /// 기본 데이터 관리 서비스 인터페이스
    /// </summary>
    /// <typeparam name="T">관리할 엔티티 타입</typeparam>
    /// <typeparam name="TKey">엔티티의 주 식별자 타입</typeparam>
    public interface IDataService<T, TKey> : IService where T : class, IEntity
    {
        /// <summary>
        /// 모든 엔티티를 조회합니다.
        /// </summary>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>엔티티 목록</returns>
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 주어진 식별자로 엔티티를 조회합니다.
        /// </summary>
        /// <param name="id">엔티티 식별자</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>식별자에 해당하는 엔티티 또는 null</returns>
        Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

        /// <summary>
        /// 새로운 엔티티를 추가합니다.
        /// </summary>
        /// <param name="entity">추가할 엔티티</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>추가된 엔티티</returns>
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// 기존 엔티티를 업데이트합니다.
        /// </summary>
        /// <param name="entity">업데이트할 엔티티</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>업데이트된 엔티티</returns>
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// 주어진 식별자의 엔티티를 삭제합니다.
        /// </summary>
        /// <param name="id">삭제할 엔티티의 식별자</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>삭제 성공 여부</returns>
        Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default);

    
    }
}