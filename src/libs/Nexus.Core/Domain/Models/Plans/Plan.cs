using Nexus.Core.Domain.Shared.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Plans
{
    public class Plan : IEntity
    {
        public string Id { get; }

        public string Name { get; }

        public List<PlanStep> PlanSteps { get; } = new List<PlanStep>();

        public Plan(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
