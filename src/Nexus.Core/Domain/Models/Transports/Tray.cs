using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Domain.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Transports
{
    public class Tray : ITransportable
    {
        public string Id { get; }
        public string Name { get;  }

        private readonly List<Memory> _memories = new List<Memory>();
        public IReadOnlyList<IItem> Items => _memories.AsReadOnly();

        public Tray(string id, string name, List<Memory> memories)
        {
            Id = id;
            Name = name;

            if (memories != null)
                _memories.AddRange(memories);
        }

    }
}
