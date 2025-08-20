using Nexus.Core.Domain.Models.Plans.Enums;
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
        public string Position { get; set; }
        public EPlanStepAction Action { get; set; }
        public EPlanStepStatus Status { get; set; } = EPlanStepStatus.Pending;

        public List<string> CarrierIds { get; set; } = new List<string>();
        public List<Job> Jobs { get; set; } = new List<Job>();

        public PlanStep(string id, string name, int stepNo, EPlanStepAction action, string position)
        {
            Id = id;
            Name = name;
            StepNo = stepNo;
            Action = action;
            Position = position;
        }
    }
}
