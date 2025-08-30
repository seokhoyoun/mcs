using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nexus.Orchestrator.Application.Acs.Models
{
    internal class AcsMessage
    {
        [JsonPropertyName("command")]
        public string Command { get; set; } = default!;

        [JsonPropertyName("transactionId")]
        public string TransactionId { get; set; } = default!;

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = default!;

        [JsonPropertyName("result")]
        public string Result { get; set; } = default!;

        [JsonPropertyName("message")]
        public string Message { get; set; } = default!;

        [JsonPropertyName("payload")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Payload { get; set; }
    }

    #region 공통 Payload 클래스

    internal class EmptyPayload
    {
        // 빈 payload를 위한 클래스
    }

    #endregion

    #region MCS→ACS Payload 클래스
    
    internal class ExecutionPlanPayload
    {
        [JsonPropertyName("planId")]
        public string PlanId { get; set; } = default!;

        [JsonPropertyName("lotId")]
        public string LotId { get; set; } = default!;

        [JsonPropertyName("priority")]
        public int Priority { get; set; }

        [JsonPropertyName("steps")]
        public List<ExecutionStep> Steps { get; set; } = default!;
    }

    internal class CancelPlanPayload
    {
        [JsonPropertyName("planId")]
        public string PlanId { get; set; } = default!;

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = default!;
    }

    internal class AbortPlanPayload
    {
        [JsonPropertyName("planId")]
        public string PlanId { get; set; } = default!;

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = default!;
    }

    internal class PausePlanPayload
    {
        [JsonPropertyName("planId")]
        public string PlanId { get; set; } = default!;

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = default!;
    }

    internal class ResumePlanPayload
    {
        [JsonPropertyName("planId")]
        public string PlanId { get; set; } = default!;
    }

    internal class RequestAcsPlanHistoryPayload
    {
        [JsonPropertyName("planIds")]
        public List<string> PlanIds { get; set; } = default!;
    }

    #endregion

    #region ACS→MCS Payload 클래스
    
    internal class RegistrationPayload
    {
        // Registration 명령을 위한 필드들
    }

    internal class PlanReportPayload
    {
        [JsonPropertyName("planId")]
        public string PlanId { get; set; } = default!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = default!;

        [JsonPropertyName("message")]
        public string Message { get; set; } = default!;
    }

    internal class StepReportPayload
    {
        [JsonPropertyName("planId")]
        public string PlanId { get; set; } = default!;

        [JsonPropertyName("robotId")]
        public string RobotId { get; set; } = default!;

        [JsonPropertyName("stepNo")]
        public int StepNo { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = default!;

        [JsonPropertyName("message")]
        public string Message { get; set; } = default!;
    }

    internal class JobReportPayload
    {
        [JsonPropertyName("planId")]
        public string PlanId { get; set; } = default!;

        [JsonPropertyName("robotId")]
        public string RobotId { get; set; } = default!;

        [JsonPropertyName("stepNo")]
        public int StepNo { get; set; }

        [JsonPropertyName("jobId")]
        public string JobId { get; set; } = default!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = default!;

        [JsonPropertyName("message")]
        public string Message { get; set; } = default!;
    }

    internal class ErrorReportPayload
    {
        [JsonPropertyName("robotId")]
        public string RobotId { get; set; } = default!;

        [JsonPropertyName("planId")]
        public string? PlanId { get; set; }

        [JsonPropertyName("state")]
        public bool State { get; set; }

        [JsonPropertyName("stepNo")]
        public int? StepNo { get; set; }

        [JsonPropertyName("jobId")]
        public string? JobId { get; set; }

        [JsonPropertyName("errorCode")]
        public string ErrorCode { get; set; } = default!;

        [JsonPropertyName("level")]
        public string Level { get; set; } = default!;

        [JsonPropertyName("message")]
        public string Message { get; set; } = default!;
    }

    internal class RobotStatusUpdatePayload
    {
        [JsonPropertyName("robotId")]
        public string RobotId { get; set; } = default!;

        [JsonPropertyName("robotType")]
        public string RobotType { get; set; } = default!;

        [JsonPropertyName("robotStatus")]
        public string RobotStatus { get; set; } = default!;

        [JsonPropertyName("position")]
        public string Position { get; set; } = default!;

        [JsonPropertyName("carrierIds")]
        public List<string?> CarrierIds { get; set; } = default!;

        [JsonPropertyName("planId")]
        public string? PlanId { get; set; }

        [JsonPropertyName("stepNo")]
        public int? StepNo { get; set; }

        [JsonPropertyName("jobId")]
        public string? JobId { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = default!;
    }

    internal class TscStateUpdatePayload
    {
        [JsonPropertyName("state")]
        public string State { get; set; } = default!;
    }

    internal class CancelResultReportPayload
    {
        [JsonPropertyName("planId")]
        public string PlanId { get; set; } = default!;

        [JsonPropertyName("result")]
        public string Result { get; set; } = default!;

        [JsonPropertyName("message")]
        public string Message { get; set; } = default!;
    }

    internal class AbortResultReportPayload
    {
        [JsonPropertyName("planId")]
        public string PlanId { get; set; } = default!;

        [JsonPropertyName("result")]
        public string Result { get; set; } = default!;

        [JsonPropertyName("message")]
        public string Message { get; set; } = default!;
    }

    internal class PauseResultReportPayload
    {
        [JsonPropertyName("planId")]
        public string PlanId { get; set; } = default!;

        [JsonPropertyName("result")]
        public string Result { get; set; } = default!;

        [JsonPropertyName("message")]
        public string Message { get; set; } = default!;
    }

    internal class ResumeResultReportPayload
    {
        [JsonPropertyName("planId")]
        public string PlanId { get; set; } = default!;

        [JsonPropertyName("result")]
        public string Result { get; set; } = default!;

        [JsonPropertyName("message")]
        public string Message { get; set; } = default!;
    }

    internal class AcsCommStateUpdatePayload
    {
        [JsonPropertyName("isConnected")]
        public bool IsConnected { get; set; }
    }

    internal class RobotPositionUpdatePayload
    {
        [JsonPropertyName("robots")]
        public List<RobotPosition> Robots { get; set; } = default!;

        internal class RobotPosition
        {
            [JsonPropertyName("robotId")]
            public string RobotId { get; set; } = default!;

            [JsonPropertyName("x")]
            public double X { get; set; }

            [JsonPropertyName("y")]
            public double Y { get; set; }

            [JsonPropertyName("angle")]
            public double Angle { get; set; }

            [JsonPropertyName("battery")]
            public int Battery { get; set; }
        }
    }

    internal class RequestAcsPlansAckPayload
    {
        [JsonPropertyName("plans")]
        public List<AcsPlanStatus> Plans { get; set; } = default!;

        internal class AcsPlanStatus
        {
            [JsonPropertyName("planId")]
            public string PlanId { get; set; } = default!;

            [JsonPropertyName("robotId")]
            public string RobotId { get; set; } = default!;

            [JsonPropertyName("status")]
            public string Status { get; set; } = default!;

            [JsonPropertyName("stepNo")]
            public int StepNo { get; set; }

            [JsonPropertyName("jobId")]
            public string? JobId { get; set; }

            [JsonPropertyName("currentAction")]
            public string? CurrentAction { get; set; }

            [JsonPropertyName("startTime")]
            public string StartTime { get; set; } = default!;

            [JsonPropertyName("endTime")]
            public string? EndTime { get; set; }
        }
    }

    internal class RequestAcsPlanHistoryAckPayload
    {
        [JsonPropertyName("plans")]
        public List<AcsPlanHistory> Plans { get; set; } = default!;

        internal class AcsPlanHistory
        {
            [JsonPropertyName("planId")]
            public string PlanId { get; set; } = default!;

            [JsonPropertyName("robotId")]
            public string RobotId { get; set; } = default!;

            [JsonPropertyName("status")]
            public string Status { get; set; } = default!;

            [JsonPropertyName("stepNo")]
            public int StepNo { get; set; }

            [JsonPropertyName("jobId")]
            public string? JobId { get; set; }

            [JsonPropertyName("currentAction")]
            public string? CurrentAction { get; set; }

            [JsonPropertyName("startTime")]
            public string StartTime { get; set; } = default!;

            [JsonPropertyName("endTime")]
            public string? EndTime { get; set; }
        }
    }

    internal class RequestAcsErrorListAckPayload
    {
        [JsonPropertyName("errors")]
        public List<AcsError> Errors { get; set; } = default!;

        internal class AcsError
        {
            [JsonPropertyName("robotId")]
            public string RobotId { get; set; } = default!;

            [JsonPropertyName("planId")]
            public string? PlanId { get; set; }

            [JsonPropertyName("state")]
            public bool State { get; set; }

            [JsonPropertyName("stepNo")]
            public int? StepNo { get; set; }

            [JsonPropertyName("jobId")]
            public string? JobId { get; set; }

            [JsonPropertyName("errorCode")]
            public string ErrorCode { get; set; } = default!;

            [JsonPropertyName("level")]
            public string Level { get; set; } = default!;

            [JsonPropertyName("message")]
            public string Message { get; set; } = default!;
        }
    }

    #endregion

    internal class ExecutionStep
    {
        [JsonPropertyName("stepNo")]
        public int StepNo { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; } = default!;

        [JsonPropertyName("position")]
        public string Position { get; set; } = default!;

        [JsonPropertyName("carrierIds")]
        public List<string> CarrierIds { get; set; } = default!;

        [JsonPropertyName("jobs")]
        public List<ExecutionJob> Jobs { get; set; } = default!;
    }

    internal class ExecutionJob
    {
        [JsonPropertyName("jobId")]
        public string JobId { get; set; } = default!;

        [JsonPropertyName("from")]
        public string From { get; set; } = default!;

        [JsonPropertyName("to")]
        public string To { get; set; } = default!;
    }
}