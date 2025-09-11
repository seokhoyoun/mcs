using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexus.Orchestrator.Application.Acs.Models;

namespace Nexus.Orchestrator.Application.Acs.Services
{
    internal class AcsService : IAcsService
    {
        private readonly ILogger<AcsService> _logger;
        private readonly HttpListener _httpListener;
        private readonly ConcurrentDictionary<string, WebSocket> _clients;
        private readonly ConcurrentDictionary<string, string> _clientInfos; // 클라이언트 ID와 추가 정보 저장
        private readonly string _address;

        // 등록된(Registration) 클라이언트 ID를 저장
        private string _registeredClientId;
        private bool _hasRegisteredClient;

        public AcsService(ILogger<AcsService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _httpListener = new HttpListener();
            _clients = new ConcurrentDictionary<string, WebSocket>();
            _clientInfos = new ConcurrentDictionary<string, string>();
            _address = configuration["AcsServer:Address"] ?? "http://*:8080/acs/";
            _registeredClientId = string.Empty;
            _hasRegisteredClient = false;
        }

        public async Task RunAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AcsService 시작 중...");

            try
            {
                // WebSocket 서버 시작
                _httpListener.Prefixes.Add(_address);
                _httpListener.Start();

                _logger.LogInformation($"WebSocket 서버가 {_address} 주소에서 시작되었습니다.");

                // 클라이언트 연결 대기 루프
                while (!stoppingToken.IsCancellationRequested)
                {
                    HttpListenerContext context = await _httpListener.GetContextAsync().ConfigureAwait(false);

                    if (context.Request.IsWebSocketRequest)
                    {
                        // 이미 등록된 클라이언트가 있는지 확인
                        if (_hasRegisteredClient && _clients.Count > 0)
                        {
                            _logger.LogWarning("이미 등록된 ACS 클라이언트가 있습니다. 새 연결을 거부합니다.");
                            context.Response.StatusCode = 403; // Forbidden
                            context.Response.Close();
                        }
                        else
                        {
                            ProcessWebSocketRequest(context, stoppingToken);
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AcsService 실행 중 오류 발생");
            }
            finally
            {
                // 서버 종료 시 연결된 모든 클라이언트 닫기
                await CloseAllClientsAsync();

                if (_httpListener.IsListening)
                {
                    _httpListener.Stop();
                    _httpListener.Close();
                }

                _logger.LogInformation("AcsService 종료됨");
            }
        }

        private async void ProcessWebSocketRequest(HttpListenerContext context, CancellationToken stoppingToken)
        {
            try
            {
                HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(subProtocol: null);
                WebSocket webSocket = webSocketContext.WebSocket;

                string clientId = Guid.NewGuid().ToString();
                _clients[clientId] = webSocket;

                _logger.LogInformation($"클라이언트 {clientId} 연결됨. 현재 연결 수: {_clients.Count}");

                await HandleClientAsync(clientId, webSocket, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WebSocket 요청 처리 중 오류 발생");
                context.Response.StatusCode = 500;
                context.Response.Close();
            }
        }

        private async Task HandleClientAsync(string clientId, WebSocket webSocket, CancellationToken stoppingToken)
        {
            byte[] buffer = new byte[4096];

            try
            {
                while (webSocket.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "클라이언트 연결 종료", stoppingToken);
                        _logger.LogInformation($"클라이언트 {clientId} 연결 종료됨");
                        break;
                    }

                    // 메시지 수신 완료 시까지 버퍼 읽기
                    if (result.EndOfMessage)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await ProcessMessageAsync(clientId, message, stoppingToken);
                    }
                }
            }
            catch (WebSocketException ex)
            {
                _logger.LogWarning(ex, $"WebSocket 예외 발생: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"클라이언트 {clientId} 처리 중 오류 발생");
            }
            finally
            {
                WebSocket? removedSocket;
                _clients.TryRemove(clientId, out removedSocket);
                _clientInfos.TryRemove(clientId, out _);

                // 등록된 클라이언트가 종료되면 등록 상태 초기화
                if (clientId == _registeredClientId)
                {
                    _hasRegisteredClient = false;
                    _registeredClientId = string.Empty;
                    _logger.LogInformation("등록된 ACS 클라이언트가 연결 해제되었습니다. 새 클라이언트가 연결될 수 있습니다.");
                }

                if (webSocket.State != WebSocketState.Closed)
                {
                    try
                    {
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.EndpointUnavailable,
                            "서버 종료",
                            CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "WebSocket 닫기 중 오류 발생");
                    }
                }

                _logger.LogInformation($"클라이언트 {clientId} 연결 해제됨. 현재 연결 수: {_clients.Count}");
            }
        }

        private async Task ProcessMessageAsync(string clientId, string message, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"클라이언트 {clientId}로부터 메시지 수신: {message}");

            try
            {
                // JSON 파싱
                AcsMessage? messageObj = JsonSerializer.Deserialize<AcsMessage>(message);

                if (messageObj == null)
                {
                    _logger.LogWarning($"메시지 형식이 올바르지 않습니다: {message}");
                    return;
                }

                // Ack 메시지인지 확인
                if (IsAckMessage(messageObj.Command))
                {
                    // Ack 메시지는 로깅만 하고 종료
                    LogAckMessage(messageObj);
                    return;
                }

                // Registration 메시지가 아닌데 등록된 클라이언트가 아니면 처리하지 않음
                if (messageObj.Command != "Registration" && !_hasRegisteredClient)
                {
                    _logger.LogWarning($"등록되지 않은 클라이언트({clientId})의 메시지({messageObj.Command}) 처리를 거부합니다.");
                    return;
                }

                // Registration 메시지가 아닌데 등록된 클라이언트가 다른 클라이언트면 처리하지 않음
                if (messageObj.Command != "Registration" && _hasRegisteredClient && clientId != _registeredClientId)
                {
                    _logger.LogWarning($"등록되지 않은 클라이언트({clientId})의 메시지({messageObj.Command}) 처리를 거부합니다. 등록된 클라이언트: {_registeredClientId}");
                    return;
                }

                // 메시지 타입에 따른 처리
                switch (messageObj.Command)
                {
                    case "Registration":
                        await HandleRegistrationAsync(clientId, messageObj, stoppingToken);
                        break;
                    case "PlanReport":
                        await HandlePlanReportAsync(clientId, messageObj, stoppingToken);
                        break;
                    case "StepReport":
                        await HandleStepReportAsync(clientId, messageObj, stoppingToken);
                        break;
                    case "JobReport":
                        await HandleJobReportAsync(clientId, messageObj, stoppingToken);
                        break;
                    case "ErrorReport":
                        await HandleErrorReportAsync(clientId, messageObj, stoppingToken);
                        break;
                    case "RobotStatusUpdate":
                        await HandleRobotStatusUpdateAsync(clientId, messageObj, stoppingToken);
                        break;
                    case "TscStateUpdate":
                        await HandleTscStateUpdateAsync(clientId, messageObj, stoppingToken);
                        break;
                    case "CancelResultReport":
                        await HandleCancelResultReportAsync(clientId, messageObj, stoppingToken);
                        break;
                    case "AbortResultReport":
                        await HandleAbortResultReportAsync(clientId, messageObj, stoppingToken);
                        break;
                    case "PauseResultReport":
                        await HandlePauseResultReportAsync(clientId, messageObj, stoppingToken);
                        break;
                    case "ResumeResultReport":
                        await HandleResumeResultReportAsync(clientId, messageObj, stoppingToken);
                        break;
                    case "AcsCommStateUpdate":
                        await HandleAcsCommStateUpdateAsync(clientId, messageObj, stoppingToken);
                        break;
                    default:
                        _logger.LogWarning($"지원하지 않는 명령: {messageObj.Command}");
                        break;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"JSON 파싱 오류: {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"메시지 처리 중 오류 발생: {message}");
            }
        }

        // Ack 메시지인지 확인하는 메서드
        private bool IsAckMessage(string command)
        {
            return command.EndsWith("Ack");
        }

        // Ack 메시지 로깅
        private void LogAckMessage(AcsMessage message)
        {
            string result = message.Result ?? "Unknown";
            string transactionId = message.TransactionId ?? "Unknown";

            if (result.Equals("Success", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation($"Ack 메시지 수신: {message.Command}, TransactionId: {transactionId}, 결과: {result}");
            }
            else
            {
                _logger.LogWarning($"Ack 메시지 수신 (실패): {message.Command}, TransactionId: {transactionId}, 결과: {result}, 메시지: {message.Message}");
            }
        }

        #region Message Handlers

        private async Task HandleRegistrationAsync(string clientId, AcsMessage message, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Registration 요청 처리 중: {clientId}");

            // 이미 등록된 클라이언트가 있는 경우
            if (_hasRegisteredClient && clientId != _registeredClientId)
            {
                _logger.LogWarning($"이미 등록된 클라이언트가 있습니다. clientId: {_registeredClientId}");

                AcsMessage failResponse = new AcsMessage
                {
                    Command = "RegistrationAck",
                    TransactionId = message.TransactionId,
                    Timestamp = DateTime.Now.ToString("o"),
                    Result = "Fail",
                    Message = "이미 등록된 클라이언트가 있습니다.",
                    Payload = new EmptyPayload()
                };

                await SendMessageAsync(clientId, failResponse, stoppingToken);

                // 거부된 클라이언트는 연결 종료
                if (_clients.TryGetValue(clientId, out WebSocket? webSocket) && webSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.PolicyViolation,
                            "이미 등록된 클라이언트가 있습니다",
                            stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"거부된 클라이언트 {clientId} 연결 종료 중 오류 발생");
                    }
                }

                return;
            }

            // Registration 요청 처리 시 클라이언트 정보 저장
            try
            {
                // 페이로드에서 robotId 등 필요한 정보 추출하여 저장
                if (message.Payload != null)
                {
                    string payload = JsonSerializer.Serialize(message.Payload);
                    _clientInfos[clientId] = payload;
                }

                // 클라이언트를 등록 상태로 설정
                _registeredClientId = clientId;
                _hasRegisteredClient = true;

                _logger.LogInformation($"클라이언트 {clientId}가 등록되었습니다.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "클라이언트 정보 저장 중 오류 발생");
            }

            AcsMessage response = new AcsMessage
            {
                Command = "RegistrationAck",
                TransactionId = message.TransactionId,
                Timestamp = DateTime.Now.ToString("o"),
                Result = "Success",
                Message = "등록이 완료되었습니다.",
                Payload = new EmptyPayload()
            };

            await SendMessageAsync(clientId, response, stoppingToken);
        }

        private async Task HandlePlanReportAsync(string clientId, AcsMessage message, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"PlanReport 요청 처리 중: {clientId}");

            AcsMessage response = new AcsMessage
            {
                Command = "PlanReportAck",
                TransactionId = message.TransactionId,
                Timestamp = DateTime.Now.ToString("o"),
                Result = "Success",
                Message = "PlanReport received.",
                Payload = new { }
            };

            await SendMessageAsync(clientId, response, stoppingToken);
        }

        private async Task HandleStepReportAsync(string clientId, AcsMessage message, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"StepReport 요청 처리 중: {clientId}");

            AcsMessage response = new AcsMessage
            {
                Command = "StepReportAck",
                TransactionId = message.TransactionId,
                Timestamp = DateTime.Now.ToString("o"),
                Result = "Success",
                Message = "Step report received.",
                Payload = new { }
            };

            await SendMessageAsync(clientId, response, stoppingToken);
        }

        private async Task HandleJobReportAsync(string clientId, AcsMessage message, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"JobReport 요청 처리 중: {clientId}");

            AcsMessage response = new AcsMessage
            {
                Command = "JobReportAck",
                TransactionId = message.TransactionId,
                Timestamp = DateTime.Now.ToString("o"),
                Result = "Success",
                Message = "Job report received.",
                Payload = new { }
            };

            await SendMessageAsync(clientId, response, stoppingToken);
        }

        private async Task HandleErrorReportAsync(string clientId, AcsMessage message, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"ErrorReport 요청 처리 중: {clientId}");

            AcsMessage response = new AcsMessage
            {
                Command = "ErrorReportAck",
                TransactionId = message.TransactionId,
                Timestamp = DateTime.Now.ToString("o"),
                Result = "Success",
                Message = "Error report received.",
                Payload = new { }
            };

            await SendMessageAsync(clientId, response, stoppingToken);
        }

        private async Task HandleRobotStatusUpdateAsync(string clientId, AcsMessage message, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"RobotStatusUpdate 요청 처리 중: {clientId}");

            AcsMessage response = new AcsMessage
            {
                Command = "RobotStatusUpdateAck",
                TransactionId = message.TransactionId,
                Timestamp = DateTime.Now.ToString("o"),
                Result = "Success",
                Message = "Status update received.",
                Payload = new { }
            };

            await SendMessageAsync(clientId, response, stoppingToken);
        }

        private async Task HandleTscStateUpdateAsync(string clientId, AcsMessage message, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"TscStateUpdate 요청 처리 중: {clientId}");

            AcsMessage response = new AcsMessage
            {
                Command = "TscStateUpdateAck",
                TransactionId = message.TransactionId,
                Timestamp = DateTime.Now.ToString("o"),
                Result = "Success",
                Message = "TSC state update received successfully.",
                Payload = new { }
            };

            await SendMessageAsync(clientId, response, stoppingToken);
        }

        private async Task HandleCancelResultReportAsync(string clientId, AcsMessage message, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"CancelResultReport 요청 처리 중: {clientId}");

            AcsMessage response = new AcsMessage
            {
                Command = "CancelResultReportAck",
                TransactionId = message.TransactionId,
                Timestamp = DateTime.Now.ToString("o"),
                Result = "Success",
                Message = "Cancellation result received.",
                Payload = new { }
            };

            await SendMessageAsync(clientId, response, stoppingToken);
        }

        private async Task HandleAbortResultReportAsync(string clientId, AcsMessage message, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"AbortResultReport 요청 처리 중: {clientId}");

            AcsMessage response = new AcsMessage
            {
                Command = "AbortResultReportAck",
                TransactionId = message.TransactionId,
                Timestamp = DateTime.Now.ToString("o"),
                Result = "Success",
                Message = "Abort result received.",
                Payload = new { }
            };

            await SendMessageAsync(clientId, response, stoppingToken);
        }

        private async Task HandlePauseResultReportAsync(string clientId, AcsMessage message, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"PauseResultReport 요청 처리 중: {clientId}");

            AcsMessage response = new AcsMessage
            {
                Command = "PauseResultReportAck",
                TransactionId = message.TransactionId,
                Timestamp = DateTime.Now.ToString("o"),
                Result = "Success",
                Message = "Pause result received.",
                Payload = new { }
            };

            await SendMessageAsync(clientId, response, stoppingToken);
        }

        private async Task HandleResumeResultReportAsync(string clientId, AcsMessage message, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"ResumeResultReport 요청 처리 중: {clientId}");

            AcsMessage response = new AcsMessage
            {
                Command = "ResumeResultReportAck",
                TransactionId = message.TransactionId,
                Timestamp = DateTime.Now.ToString("o"),
                Result = "Success",
                Message = "Resume result received.",
                Payload = new { }
            };

            await SendMessageAsync(clientId, response, stoppingToken);
        }

        private async Task HandleAcsCommStateUpdateAsync(string clientId, AcsMessage message, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"AcsCommStateUpdate 요청 처리 중: {clientId}");

            AcsMessage response = new AcsMessage
            {
                Command = "AcsCommStateUpdateAck",
                TransactionId = message.TransactionId,
                Timestamp = DateTime.Now.ToString("o"),
                Result = "Success",
                Message = "Comm state received.",
                Payload = new { }
            };

            await SendMessageAsync(clientId, response, stoppingToken);
        }

        #endregion

        private async Task SendMessageAsync(string clientId, AcsMessage message, CancellationToken stoppingToken)
        {
            if (!_clients.TryGetValue(clientId, out WebSocket? webSocket) || webSocket.State != WebSocketState.Open)
            {
                _logger.LogWarning($"클라이언트 {clientId}가 연결되어 있지 않습니다.");
                return;
            }

            try
            {
                string json = JsonSerializer.Serialize(message);
                byte[] buffer = Encoding.UTF8.GetBytes(json);

                await webSocket.SendAsync(
                    new ArraySegment<byte>(buffer),
                    WebSocketMessageType.Text,
                    true,
                    stoppingToken);

                _logger.LogInformation($"클라이언트 {clientId}에게 메시지 전송됨: {json}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"클라이언트 {clientId}에게 메시지 전송 중 오류 발생");
            }
        }

        private async Task CloseAllClientsAsync()
        {
            foreach (KeyValuePair<string, WebSocket> clientPair in _clients)
            {
                string clientId = clientPair.Key;
                WebSocket webSocket = clientPair.Value;

                if (webSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "서버 종료",
                            CancellationToken.None);

                        _logger.LogInformation($"클라이언트 {clientId} 연결 종료됨");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"클라이언트 {clientId} 연결 종료 중 오류 발생");
                    }
                }
            }

            _clients.Clear();
            _clientInfos.Clear();
            _hasRegisteredClient = false;
            _registeredClientId = string.Empty;
        }

        #region MCS to ACS Message Methods

        // ExecutionPlan 메시지 전송
        public async Task SendExecutionPlanAsync(string planId, string lotId, int priority, List<ExecutionStep> steps, CancellationToken stoppingToken = default)
        {
            if (!_hasRegisteredClient)
            {
                _logger.LogWarning("등록된 ACS 클라이언트가 없습니다. ExecutionPlan을 전송할 수 없습니다.");
                return;
            }

            ExecutionPlanPayload payload = new ExecutionPlanPayload
            {
                PlanId = planId,
                LotId = lotId,
                Priority = priority,
                Steps = steps
            };

            AcsMessage message = new AcsMessage
            {
                Command = "ExecutionPlan",
                TransactionId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now.ToString("o"),
                Payload = payload
            };

            await SendMessageAsync(_registeredClientId, message, stoppingToken);
        }

        // CancelPlan 메시지 전송
        public async Task SendCancelPlanAsync(string planId, string reason, CancellationToken stoppingToken = default)
        {
            if (!_hasRegisteredClient)
            {
                _logger.LogWarning("등록된 ACS 클라이언트가 없습니다. CancelPlan을 전송할 수 없습니다.");
                return;
            }

            CancelPlanPayload payload = new CancelPlanPayload
            {
                PlanId = planId,
                Reason = reason
            };

            AcsMessage message = new AcsMessage
            {
                Command = "CancelPlan",
                TransactionId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now.ToString("o"),
                Payload = payload
            };

            await SendMessageAsync(_registeredClientId, message, stoppingToken);
        }

        // AbortPlan 메시지 전송
        public async Task SendAbortPlanAsync(string planId, string reason, CancellationToken stoppingToken = default)
        {
            if (!_hasRegisteredClient)
            {
                _logger.LogWarning("등록된 ACS 클라이언트가 없습니다. AbortPlan을 전송할 수 없습니다.");
                return;
            }

            AcsMessage message = new AcsMessage
            {
                Command = "AbortPlan",
                TransactionId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now.ToString("o"),
                Payload = new
                {
                    planId,
                    reason
                }
            };

            await SendMessageAsync(_registeredClientId, message, stoppingToken);
        }

        // PausePlan 메시지 전송
        public async Task SendPausePlanAsync(string planId, string reason, CancellationToken stoppingToken = default)
        {
            if (!_hasRegisteredClient)
            {
                _logger.LogWarning("등록된 ACS 클라이언트가 없습니다. PausePlan을 전송할 수 없습니다.");
                return;
            }

            AcsMessage message = new AcsMessage
            {
                Command = "PausePlan",
                TransactionId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now.ToString("o"),
                Payload = new
                {
                    planId,
                    reason
                }
            };

            await SendMessageAsync(_registeredClientId, message, stoppingToken);
        }

        // ResumePlan 메시지 전송
        public async Task SendResumePlanAsync(string planId, CancellationToken stoppingToken = default)
        {
            if (!_hasRegisteredClient)
            {
                _logger.LogWarning("등록된 ACS 클라이언트가 없습니다. ResumePlan을 전송할 수 없습니다.");
                return;
            }

            AcsMessage message = new AcsMessage
            {
                Command = "ResumePlan",
                TransactionId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now.ToString("o"),
                Payload = new
                {
                    planId
                }
            };

            await SendMessageAsync(_registeredClientId, message, stoppingToken);
        }

        // SyncConfig 메시지 전송
        public async Task SendSyncConfigAsync(object configData, CancellationToken stoppingToken = default)
        {
            if (!_hasRegisteredClient)
            {
                _logger.LogWarning("등록된 ACS 클라이언트가 없습니다. SyncConfig를 전송할 수 없습니다.");
                return;
            }

            AcsMessage message = new AcsMessage
            {
                Command = "SyncConfig",
                TransactionId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now.ToString("o"),
                Payload = configData
            };

            await SendMessageAsync(_registeredClientId, message, stoppingToken);
        }

        // RequestAcsPlans 메시지 전송
        public async Task SendRequestAcsPlansAsync(CancellationToken stoppingToken = default)
        {
            if (!_hasRegisteredClient)
            {
                _logger.LogWarning("등록된 ACS 클라이언트가 없습니다. RequestAcsPlans을 전송할 수 없습니다.");
                return;
            }

            AcsMessage message = new AcsMessage
            {
                Command = "RequestAcsPlans",
                TransactionId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now.ToString("o"),
                Payload = new { }
            };

            await SendMessageAsync(_registeredClientId, message, stoppingToken);
        }

        // RequestAcsPlanHistory 메시지 전송
        public async Task SendRequestAcsPlanHistoryAsync(List<string> planIds, CancellationToken stoppingToken = default)
        {
            if (!_hasRegisteredClient)
            {
                _logger.LogWarning("등록된 ACS 클라이언트가 없습니다. RequestAcsPlanHistory를 전송할 수 없습니다.");
                return;
            }

            AcsMessage message = new AcsMessage
            {
                Command = "RequestAcsPlanHistory",
                TransactionId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now.ToString("o"),
                Payload = new
                {
                    planIds
                }
            };

            await SendMessageAsync(_registeredClientId, message, stoppingToken);
        }

        // RequestAcsErrorList 메시지 전송
        public async Task SendRequestAcsErrorListAsync(CancellationToken stoppingToken = default)
        {
            if (!_hasRegisteredClient)
            {
                _logger.LogWarning("등록된 ACS 클라이언트가 없습니다. RequestAcsErrorList를 전송할 수 없습니다.");
                return;
            }

            AcsMessage message = new AcsMessage
            {
                Command = "RequestAcsErrorList",
                TransactionId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now.ToString("o"),
                Payload = new { }
            };

            await SendMessageAsync(_registeredClientId, message, stoppingToken);
        }

        // 등록된 클라이언트 여부 확인
        public bool HasRegisteredClient => _hasRegisteredClient;

        // 등록된 클라이언트 ID 가져오기
        public string GetRegisteredClientId => _registeredClientId;

        #endregion
    }
}
