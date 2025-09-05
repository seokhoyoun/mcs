using Microsoft.Extensions.Logging;
using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Areas.Interfaces;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Transports;
using Nexus.Shared.Application.DTO;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nexus.Infrastructure.Persistence.Redis
{
    public class RedisAreaRepository : IAreaRepository
    {
        private readonly IDatabase _redisDb;
        private readonly ILogger<RedisAreaRepository> _logger;

        public RedisAreaRepository(IConnectionMultiplexer connection, ILogger<RedisAreaRepository> logger)
        {
            _redisDb = connection.GetDatabase();
            _logger = logger;
        }

        public Task<Area> AddAsync(Area entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Area>> AddRangeAsync(IEnumerable<Area> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAsync(Expression<Func<Area, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(Area entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteRangeAsync(IEnumerable<Area> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(Expression<Func<Area, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Area>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Area>> GetAsync(Expression<Func<Area, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<CassetteLocation>> GetAvailableCassettePortsAsync(string areaId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Area?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Set>> GetSetsByAreaIdAsync(string areaId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task InitializeAreasAsync(IEnumerable<Area> areas)
        {
           
        }

        public Task<Area> UpdateAsync(Area entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateRangeAsync(IEnumerable<Area> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}