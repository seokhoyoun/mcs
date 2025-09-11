using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexus.Orchestrator.Application.Acs.Models;

namespace Nexus.Orchestrator.Application.Acs.Services
{
    // Lightweight, test-friendly ACS service that does not open sockets
    internal class AcsSimulationService : IAcsService
    {
        private readonly ILogger<AcsSimulationService> _logger;
        private string _registeredClientId = string.Empty;
        private bool _hasRegisteredClient = false;

        public AcsSimulationService(ILogger<AcsSimulationService> logger)
        {
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AcsSimulationService running (no network)");
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(500, stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {
                // normal on shutdown
            }
            _logger.LogInformation("AcsSimulationService stopped");
        }

        public Task SendToRegisteredClientAsync(AcsMessage message, CancellationToken stoppingToken = default)
        {
            if (!_hasRegisteredClient)
            {
                _logger.LogWarning("[SIM] No registered client; dropping message {Command}", message.Command);
                return Task.CompletedTask;
            }
            _logger.LogInformation("[SIM] Sent to registered client {ClientId}: {Command}", _registeredClientId, message.Command);
            return Task.CompletedTask;
        }

        public Task SendToRobotAsync(string robotId, AcsMessage message, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("[SIM] SendToRobot {RobotId}: {Command}", robotId, message.Command);
            return Task.CompletedTask;
        }

        public Task SendExecutionPlanAsync(string planId, string lotId, int priority, List<ExecutionStep> steps, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("[SIM] ExecutionPlan planId={PlanId}, lotId={LotId}, steps={Count}", planId, lotId, steps.Count);
            return Task.CompletedTask;
        }

        public Task SendCancelPlanAsync(string planId, string reason, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("[SIM] CancelPlan planId={PlanId}, reason={Reason}", planId, reason);
            return Task.CompletedTask;
        }

        public Task SendAbortPlanAsync(string planId, string reason, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("[SIM] AbortPlan planId={PlanId}, reason={Reason}", planId, reason);
            return Task.CompletedTask;
        }

        public Task SendPausePlanAsync(string planId, string reason, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("[SIM] PausePlan planId={PlanId}, reason={Reason}", planId, reason);
            return Task.CompletedTask;
        }

        public Task SendResumePlanAsync(string planId, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("[SIM] ResumePlan planId={PlanId}", planId);
            return Task.CompletedTask;
        }

        public Task SendSyncConfigAsync(object configData, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("[SIM] SyncConfig payloadType={Type}", configData.GetType().Name);
            return Task.CompletedTask;
        }

        public Task SendRequestAcsPlansAsync(CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("[SIM] RequestAcsPlans");
            return Task.CompletedTask;
        }

        public Task SendRequestAcsPlanHistoryAsync(List<string> planIds, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("[SIM] RequestAcsPlanHistory count={Count}", planIds.Count);
            return Task.CompletedTask;
        }

        public Task SendRequestAcsErrorListAsync(CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("[SIM] RequestAcsErrorList");
            return Task.CompletedTask;
        }

        public bool HasRegisteredClient => _hasRegisteredClient;

        public string GetRegisteredClientId => _registeredClientId;

        // Helper for tests
        public void SimulateRegister(string clientId)
        {
            _registeredClientId = clientId;
            _hasRegisteredClient = true;
        }
    }
}

