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
        protected readonly IRepository<T, TKey> _repository;

        protected BaseDataService(ILogger logger, IRepository<T, TKey> repository)
        {
            _repository = repository;
            _logger = logger;
        }

        public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _repository.GetAllAsync(cancellationToken);
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
                return await _repository.GetByIdAsync(id, cancellationToken);
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
                var result = await _repository.AddAsync(entity, cancellationToken);

                // 필요 시 이벤트 발행
                // await _eventPublisher.PublishAsync(new EntityCreatedEvent<T>(result), cancellationToken);

                return result;
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
                var result = await _repository.UpdateAsync(entity, cancellationToken);
                return result;
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
                return await _repository.DeleteAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID {id}의 엔티티 삭제 중 오류 발생");
                throw;
            }
        }

        public abstract Task InitializeAsync(CancellationToken cancellationToken = default);
      

    }
}