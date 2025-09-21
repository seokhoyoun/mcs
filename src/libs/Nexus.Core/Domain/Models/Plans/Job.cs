using Nexus.Core.Domain.Models.Plans.Enums;
using Nexus.Core.Domain.Shared.Bases;

namespace Nexus.Core.Domain.Models.Plans
{
    public class Job : IEntity
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int JobNo { get; set; } = 0;

        public string FromLocationId { get; set; } = string.Empty;
        public string ToLocationId { get; set; } = string.Empty;

        public EJobStatus Status { get; set; } = EJobStatus.Pending;

        public Job(string id, string name, int jobNo, string fromLocationId, string toLocationId)
        {
            Id = id;
            Name = name;
            JobNo = jobNo;
            FromLocationId = fromLocationId;
            ToLocationId = toLocationId;
        }

    }
}

