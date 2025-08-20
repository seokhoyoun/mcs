using Nexus.Core.Domain.Models.Areas.Enums;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Nexus.Core.Domain.Models.Areas
{
    public class Area : IEntity
    {
        public string Id { get; }
        public string Name { get; }
        public IReadOnlyList<CassetteLocation> CassetteLocations => _cassetteLocations.AsReadOnly();
        public IReadOnlyList<TrayLocation> TrayLocations => _trayLocations.AsReadOnly();
        public IReadOnlyList<Set> Sets => _sets.AsReadOnly();
        public EAreaStatus Status { get; set; } = EAreaStatus.Idle;

        private readonly List<CassetteLocation> _cassetteLocations = new();
        private readonly List<TrayLocation> _trayLocations = new();
        private readonly List<Set> _sets = new();

        public Area(string id, string name, IReadOnlyList<CassetteLocation> cassetteLocations, IReadOnlyList<TrayLocation> trayLocations, IReadOnlyList<Set> sets)
        {
            Id = id;
            Name = name;

            _cassetteLocations.AddRange(cassetteLocations);
            _trayLocations.AddRange(trayLocations);
            _sets.AddRange(sets);

        }
    }
}