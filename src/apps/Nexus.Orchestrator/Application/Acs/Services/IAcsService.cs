using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Orchestrator.Application.Acs.Models;

namespace Nexus.Orchestrator.Application.Acs.Services
{
    internal interface IAcsService
    {
        Task RunAsync(CancellationToken stoppingToken);

       
        Task SendExecutionPlanAsync(string planId, string lotId, int priority, List<ExecutionStep> steps, CancellationToken stoppingToken = default);
        Task SendCancelPlanAsync(string planId, string reason, CancellationToken stoppingToken = default);
        Task SendAbortPlanAsync(string planId, string reason, CancellationToken stoppingToken = default);
        Task SendPausePlanAsync(string planId, string reason, CancellationToken stoppingToken = default);
        Task SendResumePlanAsync(string planId, CancellationToken stoppingToken = default);
        Task SendSyncConfigAsync(object configData, CancellationToken stoppingToken = default);
        Task SendRequestAcsPlansAsync(CancellationToken stoppingToken = default);
        Task SendRequestAcsPlanHistoryAsync(List<string> planIds, CancellationToken stoppingToken = default);
        Task SendRequestAcsErrorListAsync(CancellationToken stoppingToken = default);

        bool HasRegisteredClient { get; }
        string GetRegisteredClientId { get; }
    }
}
