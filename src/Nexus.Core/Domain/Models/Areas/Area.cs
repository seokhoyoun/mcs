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
        public string Id { get; private set; }
        public string Name { get; private set; }

        public IReadOnlyList<Location<Cassette>> CassettePorts => _cassettePorts.AsReadOnly();
        public IReadOnlyList<Location<Tray>> TrayPorts => _trayPorts.AsReadOnly();
        public IReadOnlyList<Set> Sets => _sets.AsReadOnly();

        private List<Location<Cassette>> _cassettePorts = new List<Location<Cassette>>();
        private List<Location<Tray>> _trayPorts = new List<Location<Tray>>();
        private List<Set> _sets = new List<Set>();

        public Area(string id, string name, IEnumerable<Location<Cassette>> cassettePorts, IEnumerable<Location<Tray>> trayPorts, IEnumerable<Set> sets)
        {
            Id = id;
            Name = name;

            if (cassettePorts != null) _cassettePorts.AddRange(cassettePorts);
            if (trayPorts != null) _trayPorts.AddRange(trayPorts);
            if (sets != null) _sets.AddRange(sets);
        }
    }
}
