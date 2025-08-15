using Nexus.Core.Domain.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Plans
{
    public class PlanStep : IEntity
    {
        public string Id { get; }

        public string Name { get; }
    }
}
