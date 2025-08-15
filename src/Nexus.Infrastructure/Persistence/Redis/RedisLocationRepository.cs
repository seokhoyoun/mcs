using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Shared.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Infrastructure.Persistence.Redis
{
    public class RedisLocationRepository : ILocationRepository
    {
        private readonly Dictionary<string, Location<ITransportable>> _staticLocations;

        public Task<LocationState> GetStateAsync(string locationId)
        {
            throw new NotImplementedException();
        }
    }
}
