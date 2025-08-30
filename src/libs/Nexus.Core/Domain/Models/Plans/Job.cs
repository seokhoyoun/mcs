using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Plans.Enums;
using Nexus.Core.Domain.Shared.Bases;

namespace Nexus.Core.Domain.Models.Plans
{
    public class Job : IEntity
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int JobNo { get; set; } = 0;

        public Location FromLocation { get; set; }
        public Location ToLocation { get; set; }

        public EJobStatus Status { get; set; } = EJobStatus.Pending;

        public Job(string id, string name, int jobNo, Location fromLocation, Location toLocation)
        {
            Id = id;
            Name = name;
            JobNo = jobNo;
            FromLocation = fromLocation;
            ToLocation = toLocation;
        }

    }
}