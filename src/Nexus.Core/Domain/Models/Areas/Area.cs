using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Areas
{
    public class Area : IEntity
    {
        public required string Id { get; set; }
        public required string Name { get; set; }

        public IReadOnlyList<Location<Cassette>> CassettePorts => _cassettePorts.AsReadOnly();
        public IReadOnlyList<Location<Tray>> TrayPorts => _trayPorts.AsReadOnly();
        public IReadOnlyList<Set> Sets => _sets.AsReadOnly();

        private List<Location<Cassette>> _cassettePorts = new List<Location<Cassette>>();
        private List<Location<Tray>> _trayPorts = new List<Location<Tray>>();
        private List<Set> _sets = new List<Set>();

        public Area(string id, string name, List<Location<Cassette>> cassettePorts, List<Location<Tray>> trayPorts, List<Set> sets)
        {
            Id = id;
            Name = name;
            if (cassettePorts != null) _cassettePorts.AddRange(cassettePorts);
            if (trayPorts != null) _trayPorts.AddRange(trayPorts);
            if (sets != null) _sets.AddRange(sets);
        }
    }
}
