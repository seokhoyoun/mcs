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
                _logger.LogError(ex, "��ƼƼ ��� ��ȸ �� ���� �߻�");
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
                _logger.LogError(ex, $"ID {id}�� ��ƼƼ ��ȸ �� ���� �߻�");
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
                _logger.LogError(ex, "��ƼƼ �߰� �� ���� �߻�");
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
                _logger.LogError(ex, "��ƼƼ ������Ʈ �� ���� �߻�");
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
                _logger.LogError(ex, $"ID {id}�� ��ƼƼ ���� �� ���� �߻�");
                throw;
            }
        }

        public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await InitializeServiceAsync(cancellationToken);
                _logger.LogInformation($"{GetType().Name} �ʱ�ȭ �Ϸ�");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} �ʱ�ȭ �� ���� �߻�");
                throw;
            }
        }

        #region Abstract Methods
        
        /// <summary>
        /// ��� ��ƼƼ�� ��ȸ�ϴ� ������ �����մϴ�.
        /// </summary>
        protected abstract Task<IReadOnlyList<T>> GetAllEntitiesAsync(CancellationToken cancellationToken);
        
        /// <summary>
        /// ID�� Ư�� ��ƼƼ�� ��ȸ�ϴ� ������ �����մϴ�.
        /// </summary>
        protected abstract Task<T?> GetEntityByIdAsync(TKey id, CancellationToken cancellationToken);
        
        /// <summary>
        /// ��ƼƼ�� �߰��ϴ� ������ �����մϴ�.
        /// </summary>
        protected abstract Task<T> AddEntityAsync(T entity, CancellationToken cancellationToken);
        
        /// <summary>
        /// ��ƼƼ�� ������Ʈ�ϴ� ������ �����մϴ�.
        /// </summary>
        protected abstract Task<T> UpdateEntityAsync(T entity, CancellationToken cancellationToken);
        
        /// <summary>
        /// ��ƼƼ�� �����ϴ� ������ �����մϴ�.
        /// </summary>
        protected abstract Task<bool> DeleteEntityAsync(TKey id, CancellationToken cancellationToken);
        
        /// <summary>
        /// ���񽺸� �ʱ�ȭ�ϴ� ������ �����մϴ�.
        /// </summary>
        protected abstract Task InitializeServiceAsync(CancellationToken cancellationToken);
        
        #endregion
    }
}