using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Infrastructure.Persistence.Redis
{
    internal class RedisTransportsRepository : ITransportsRepository
    {
        public IEnumerable<Cassette> GetAllCassettes()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Memory> GetAllMemories()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Tray> GetAllTrays()
        {
            throw new NotImplementedException();
        }
    }
}
