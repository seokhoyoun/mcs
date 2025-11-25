using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Shared.Bases
{
    /// <summary>
    /// 기본 Repository 패턴을 정의하는 제네릭 인터페이스입니다.
    /// </summary>
    /// <typeparam name="T">엔티티 타입</typeparam>
    /// <typeparam name="TKey">엔티티 식별자 타입</typeparam>
    public interface IRepository<T, TKey> where T : class, IEntity
    {
        /// <summary>
        /// 모든 엔티티를 조회합니다.
        /// </summary>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>엔티티 목록</returns>
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 지정된 조건을 만족하는 엔티티를 조회합니다.
        /// </summary>
        /// <param name="predicate">조회 조건</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>조건을 만족하는 엔티티 목록</returns>
        Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

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
        /// 여러 엔티티를 일괄 추가합니다.
        /// </summary>
        /// <param name="entities">추가할 엔티티 목록</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>추가된 엔티티 목록</returns>
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// 기존 엔티티를 업데이트합니다.
        /// </summary>
        /// <param name="entity">업데이트할 엔티티</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>업데이트된 엔티티</returns>
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// 여러 엔티티를 일괄 업데이트합니다.
        /// </summary>
        /// <param name="entities">업데이트할 엔티티 목록</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>작업 완료 여부</returns>
        Task<bool> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// 주어진 식별자의 엔티티를 삭제합니다.
        /// </summary>
        /// <param name="id">삭제할 엔티티의 식별자</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>삭제 성공 여부</returns>
        Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default);

        /// <summary>
        /// 주어진 엔티티를 삭제합니다.
        /// </summary>
        /// <param name="entity">삭제할 엔티티</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>삭제 성공 여부</returns>
        Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// 여러 엔티티를 일괄 삭제합니다.
        /// </summary>
        /// <param name="entities">삭제할 엔티티 목록</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>작업 완료 여부</returns>
        Task<bool> DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// 지정된 조건을 만족하는 엔티티가 존재하는지 확인합니다.
        /// </summary>
        /// <param name="predicate">확인할 조건</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>조건을 만족하는 엔티티가 있으면 true, 없으면 false</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// 지정된 조건을 만족하는 엔티티의 개수를 반환합니다.
        /// </summary>
        /// <param name="predicate">조건</param>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        /// <returns>조건을 만족하는 엔티티 개수</returns>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    }
}
