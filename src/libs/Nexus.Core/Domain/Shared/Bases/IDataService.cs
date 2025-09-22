using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Shared.Bases
{

    public interface IDataService<T, TKey> : IService where T : class, IEntity
    {
 
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);


        Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);


        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);


        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

 
        Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default);

    
    }
}
