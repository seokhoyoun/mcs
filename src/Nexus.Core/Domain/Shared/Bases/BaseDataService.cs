using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Shared.Bases
{
    /// <summary>
    /// 데이터 서비스의 기본 추상 구현
    /// </summary>
    /// <typeparam name="T">관리할 엔티티 타입</typeparam>
    /// <typeparam name="TKey">엔티티의 주 식별자 타입</typeparam>
    public abstract class BaseDataService<T, TKey> : IDataService<T, TKey> where T : class, IEntity
    {
        protected readonly ILogger _logger;
        
        protected BaseDataService(ILogger logger)
        {
            _logger = logger;
        }

        public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await GetAllEntitiesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "엔티티 목록 조회 중 오류 발생");
                throw;
            }
        }

        public virtual async Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await GetEntityByIdAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID {id}의 엔티티 조회 중 오류 발생");
                throw;
            }
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            try
            {
                return await AddEntityAsync(entity, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "엔티티 추가 중 오류 발생");
                throw;
            }
        }

        public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            try
            {
                return await UpdateEntityAsync(entity, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "엔티티 업데이트 중 오류 발생");
                throw;
            }
        }

        public virtual async Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await DeleteEntityAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID {id}의 엔티티 삭제 중 오류 발생");
                throw;
            }
        }

        public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await InitializeServiceAsync(cancellationToken);
                _logger.LogInformation($"{GetType().Name} 초기화 완료");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} 초기화 중 오류 발생");
                throw;
            }
        }

        #region Abstract Methods
        
        /// <summary>
        /// 모든 엔티티를 조회하는 구현을 제공합니다.
        /// </summary>
        protected abstract Task<IReadOnlyList<T>> GetAllEntitiesAsync(CancellationToken cancellationToken);
        
        /// <summary>
        /// ID로 특정 엔티티를 조회하는 구현을 제공합니다.
        /// </summary>
        protected abstract Task<T?> GetEntityByIdAsync(TKey id, CancellationToken cancellationToken);
        
        /// <summary>
        /// 엔티티를 추가하는 구현을 제공합니다.
        /// </summary>
        protected abstract Task<T> AddEntityAsync(T entity, CancellationToken cancellationToken);
        
        /// <summary>
        /// 엔티티를 업데이트하는 구현을 제공합니다.
        /// </summary>
        protected abstract Task<T> UpdateEntityAsync(T entity, CancellationToken cancellationToken);
        
        /// <summary>
        /// 엔티티를 삭제하는 구현을 제공합니다.
        /// </summary>
        protected abstract Task<bool> DeleteEntityAsync(TKey id, CancellationToken cancellationToken);
        
        /// <summary>
        /// 서비스를 초기화하는 구현을 제공합니다.
        /// </summary>
        protected abstract Task InitializeServiceAsync(CancellationToken cancellationToken);
        
        #endregion
    }
}