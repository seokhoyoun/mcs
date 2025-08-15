using Nexus.Core.Domain.Models.Locations;
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
        public IReadOnlyList<Location> CassettePorts => _cassettePorts.AsReadOnly();
        public IReadOnlyList<Location> TrayPorts => _trayPorts.AsReadOnly();
        public IReadOnlyList<Set> Sets => _sets.AsReadOnly();

        private readonly List<Location> _cassettePorts = new();
        private readonly List<Location> _trayPorts = new();
        private readonly List<Set> _sets = new();

        public Area(string id, string name, IReadOnlyList<Location> cassettePorts, IReadOnlyList<Location> trayPorts, IReadOnlyList<Set> sets)
        {
            Id = id;
            Name = name;

            _cassettePorts.AddRange(cassettePorts);
            _trayPorts.AddRange(trayPorts);
            _sets.AddRange(sets);

        }
    }
}