using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Shared.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Stockers
{
    public class Stocker : IEntity
    {
        public string Id { get; }
        public string Name { get; }

        public IReadOnlyList<CassetteLocation> CassetteLocations => _cassetteLocations.AsReadOnly();
        public IReadOnlyList<TrayLocation> TrayLocations => _trayLocations.AsReadOnly();

        private List<CassetteLocation> _cassetteLocations = new List<CassetteLocation>();
        private List<TrayLocation> _trayLocations = new List<TrayLocation>();

        public Stocker(string id, string name, IReadOnlyList<CassetteLocation> cassetteLocations, IReadOnlyList<TrayLocation> trayLocations)
        {
            Id = id;
            Name = name;

            _cassetteLocations.AddRange(cassetteLocations);
            _trayLocations.AddRange(trayLocations);
        }
    }
}
