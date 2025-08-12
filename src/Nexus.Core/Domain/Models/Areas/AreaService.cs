using Nexus.Core.Domain.Models.Areas.Interfaces;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Areas
{
    public class AreaService
    {
        private readonly IAreaRepository _areaRepository;
        private readonly List<Area> _areas;

        public IReadOnlyList<Area> Areas => _areas.AsReadOnly();

        public AreaService(IAreaRepository areaRepository)
        {
            _areaRepository = areaRepository;
            _areas = new List<Area>();
        }

        public async Task InitializeAreaService()
        {
            var areas = await _areaRepository.GetAllAreasAsync();
            _areas.AddRange(areas);
        }
    }
}
