using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Domain.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Transports
{
    public class Memory : ITransportable
    {
        public string Id { get; }
        public string Name { get; }

        public IReadOnlyList<IItem>? Items { get; } = null;

        public Memory(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
   
}
