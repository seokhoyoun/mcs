using Nexus.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Models
{
    public class Tray : ITransportable
    {
        public required string Id { get; set; }
        public required string Name { get; set; }


        private readonly List<Memory> _memories = new List<Memory>();
        public IReadOnlyList<IItem> Items => _memories.AsReadOnly();

        public Tray()
        {
        }
        public Tray(string id, string name, List<Memory> memories)
        {
            Id = id;
            Name = name;

            if (memories != null)
                _memories.AddRange(memories);
        }

    }
}
