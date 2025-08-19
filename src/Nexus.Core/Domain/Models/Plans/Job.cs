
namespace Nexus.Scheduler.Domain.Models.Plans
{
    public class Job
    {
        public string JobId { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
    }
}