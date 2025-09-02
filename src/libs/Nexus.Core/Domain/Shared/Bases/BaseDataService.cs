using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Shared.Bases
{
    /// <summary>
    /// ������ ������ �⺻ �߻� ����
    /// </summary>
    /// <typeparam name="T">������ ��ƼƼ Ÿ��</typeparam>
    /// <typeparam name="TKey">��ƼƼ�� �� �ĺ��� Ÿ��</typeparam>
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
                _logger.LogError(ex, "��ƼƼ ��� ��ȸ �� ���� �߻�");
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
                _logger.LogError(ex, $"ID {id}�� ��ƼƼ ��ȸ �� ���� �߻�");
                throw;
            }
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _repository.AddAsync(entity, cancellationToken);

                // �ʿ� �� �̺�Ʈ ����
                // await _eventPublisher.PublishAsync(new EntityCreatedEvent<T>(result), cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "��ƼƼ �߰� �� ���� �߻�");
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
                _logger.LogError(ex, "��ƼƼ ������Ʈ �� ���� �߻�");
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
                _logger.LogError(ex, $"ID {id}�� ��ƼƼ ���� �� ���� �߻�");
                throw;
            }
        }

        public abstract Task InitializeAsync(CancellationToken cancellationToken = default);
      

    }
}