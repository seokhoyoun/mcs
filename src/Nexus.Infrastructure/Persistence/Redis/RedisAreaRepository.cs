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

        public async Task InitializeAreasAsync(IEnumerable<Area> areas)
        {
            var tasks = new List<Task>();
            foreach (var area in areas)
            {
                string key = $"area:{area.Id}";
                
            }
        }
    }
}