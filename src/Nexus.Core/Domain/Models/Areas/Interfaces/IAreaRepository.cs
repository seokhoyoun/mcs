using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Areas.Interfaces
{
    public interface IAreaRepository
    {
        Task InitializeAreasAsync(IEnumerable<Area> areas);

    }
}
