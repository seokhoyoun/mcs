using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Shared.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Stockers.Interfaces
{
    public interface IStockerRepository : IRepository<Stocker, string>
    {
      
    }
}
