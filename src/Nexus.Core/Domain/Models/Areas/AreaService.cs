using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Areas
{
    internal class AreaService
    {
        private readonly LocationService _locationService;
        private readonly List<Area> _areas = new();

        public IReadOnlyList<Area> Areas => _areas;

        public AreaService(LocationService locationService)
        {
            _locationService = locationService;

        }
    }
}
