using Nexus.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Models
{
    public class Memory : ITransportable
    {
        public required string Id { get; set; }
        public required string Name { get; set; }


        public IReadOnlyList<IItem>? Items { get; } = null;

        public Memory()
        {
        }

        public Memory(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
   
}
