using Microsoft.Extensions.Logging;
using Nexus.Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Shared.Bases
{
    public class DataService<T, TKey> : BaseDataService<T, TKey> where T : class, IEntity
    {
        private readonly IRepository<T, TKey> _repository;
        private readonly IEventPublisher _eventPublisher;

        public DataService(
            ILogger<DataService<T, TKey>> logger,
            IRepository<T, TKey> repository,
            IEventPublisher eventPublisher)
            : base(logger)
        {
            _repository = repository;
            _eventPublisher = eventPublisher;
        }

        protected override async Task<T> AddEntityAsync(T entity, CancellationToken cancellationToken)
        {
            var result = await _repository.AddAsync(entity, cancellationToken);
            
            // 필요 시 이벤트 발행
            // await _eventPublisher.PublishAsync(new EntityCreatedEvent<T>(result), cancellationToken);
            
            return result;
        }

        protected override async Task<bool> DeleteEntityAsync(TKey id, CancellationToken cancellationToken)
        {
            var result = await _repository.DeleteAsync(id, cancellationToken);
            
            // 필요 시 이벤트 발행
            // if (result)
            // {
            //     await _eventPublisher.PublishAsync(new EntityDeletedEvent<TKey>(id), cancellationToken);
            // }
            
            return result;
        }

        protected override async Task<IReadOnlyList<T>> GetAllEntitiesAsync(CancellationToken cancellationToken)
        {
            return await _repository.GetAllAsync(cancellationToken);
        }

        protected override async Task<T?> GetEntityByIdAsync(TKey id, CancellationToken cancellationToken)
        {
            return await _repository.GetByIdAsync(id, cancellationToken);
        }

        protected override async Task InitializeServiceAsync(CancellationToken cancellationToken)
        {
            // 서비스 초기화 로직
            // 예: 캐시 초기화, 설정 로드 등
            _logger.LogInformation($"{typeof(T).Name} 데이터 서비스 초기화 중...");
            
            // 필요한 초기화 작업 수행
            await Task.CompletedTask;
        }

        protected override async Task<T> UpdateEntityAsync(T entity, CancellationToken cancellationToken)
        {
            var result = await _repository.UpdateAsync(entity, cancellationToken);
            
            // 필요 시 이벤트 발행
            // await _eventPublisher.PublishAsync(new EntityUpdatedEvent<T>(result), cancellationToken);
            
            return result;
        }
    }
}
