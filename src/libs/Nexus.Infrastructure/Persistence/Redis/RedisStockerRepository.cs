using Nexus.Core.Domain.Models.Stockers;
using Nexus.Core.Domain.Models.Stockers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Infrastructure.Persistence.Redis
{
    public class RedisStockerRepository : IStockerRepository
    {
        public Task<IReadOnlyList<Stocker>> GetAllStockersAsync()
        {
            throw new NotImplementedException();
        }
    }
}
