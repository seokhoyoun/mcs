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
        public int StepNo { get; set; }
        public string Action { get; set; }
        public string Position { get; set; }
        public List<string> CarrierIds { get; set; } = new List<string>();
        public List<Job> Jobs { get; set; } = new List<Job>();
    }
}
