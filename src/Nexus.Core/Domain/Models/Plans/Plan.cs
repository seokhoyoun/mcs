using Nexus.Core.Domain.Shared.Interfaces;
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

        public IReadOnlyList<PlanStep> PlanSteps => _planSteps.AsReadOnly();
        private readonly List<PlanStep> _planSteps = new List<PlanStep>();
    }
}
