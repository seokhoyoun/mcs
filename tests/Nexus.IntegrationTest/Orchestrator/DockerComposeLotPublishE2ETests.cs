using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Areas.Enums;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Lots;
using Nexus.Core.Domain.Models.Lots.DTO;
using Nexus.Core.Domain.Models.Lots.Enums;
using Nexus.Core.Domain.Models.Plans;
using Nexus.Core.Domain.Models.Plans.Enums;
using Nexus.Core.Domain.Models.Robots;
using Nexus.Core.Domain.Models.Robots.Enums;
using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Shared.Bases;
using Nexus.Infrastructure.Persistence.Redis;
using StackExchange.Redis;

namespace Nexus.IntegrationTest.Orchestrator;

[CollectionDefinition("DockerComposeE2E", DisableParallelization = true)]
public class DockerComposeCollection : ICollectionFixture<DockerComposeFixture>
{
}

public class DockerComposeFixture : IDisposable
{
    private readonly string _composeArgs = "up -d";
    private readonly string _composeDownArgs = "down -v";
    private const int RedisHostPort = 6380;
    private readonly TimeSpan _startupTimeout = TimeSpan.FromSeconds(60);

    public DockerComposeFixture()
    {
        // 1) Redis만 먼저 기동 (itest override로 포트 6380 사용)
        RunDockerComposeCommand("--env-file .env.itest -f docker-compose.yml -f docker-compose.itest.yml up -d redis");
        WaitForRedis();
        // 2) Sandbox 시딩 수행
        RunSandboxSeed();
        // 3) 나머지 앱 서비스 기동 (오케스트레이터/게이트웨이)
        RunDockerComposeCommand("--env-file .env.itest -f docker-compose.yml -f docker-compose.itest.yml up -d --no-build nexus.orchestrator nexus.gateway");
        // 4) 앱 포트 준비 대기 (itest override 포트)
        WaitForTcp("127.0.0.1", 18081, _startupTimeout);
        WaitForTcp("127.0.0.1", 18082, _startupTimeout);
    }

    private void StartDockerCompose()
    {
        RunDockerComposeCommand("compose " + _composeArgs);
    }

    private void RunDockerComposeCommand(string args)
    {
        string? solutionRoot = FindSolutionRoot();
        if (solutionRoot == null)
        {
            throw new InvalidOperationException("솔루션 루트를 찾을 수 없습니다 (docker compose 실행 경로).");
        }
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = "docker";
        psi.Arguments = "compose " + args;
        psi.WorkingDirectory = solutionRoot;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        psi.UseShellExecute = false;
        using (Process proc = Process.Start(psi)!)
        {
            proc.WaitForExit();
            if (proc.ExitCode != 0)
            {
                string stdout = proc.StandardOutput.ReadToEnd();
                string stderr = proc.StandardError.ReadToEnd();
                throw new InvalidOperationException("docker compose 실행 실패: " + args + "\nSTDOUT:\n" + stdout + "\nSTDERR:\n" + stderr);
            }
        }
    }

    private void WaitForRedis()
    {
        DateTime deadline = DateTime.UtcNow.Add(_startupTimeout);
        bool connected = false;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    client.ReceiveTimeout = 2000;
                    client.SendTimeout = 2000;
                    client.Connect("127.0.0.1", RedisHostPort);
                    connected = true;
                    break;
                }
            }
            catch
            {
                Thread.Sleep(1000);
            }
        }

        if (!connected)
        {
            throw new InvalidOperationException("Redis가 시작되지 않았습니다 (localhost:6379).");
        }
    }

    private void WaitForTcp(string host, int port, TimeSpan timeout)
    {
        DateTime deadline = DateTime.UtcNow.Add(timeout);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    client.ReceiveTimeout = 2000;
                    client.SendTimeout = 2000;
                    client.Connect(host, port);
                    return;
                }
            }
            catch
            {
                Thread.Sleep(1000);
            }
        }
        throw new InvalidOperationException($"서비스가 시작되지 않았습니다 ({host}:{port}).");
    }

    private void RunSandboxSeed()
    {
        string? solutionRoot = FindSolutionRoot();
        if (solutionRoot == null)
        {
            throw new InvalidOperationException("솔루션 루트를 찾을 수 없습니다 (Nexus.sln 미발견).");
        }
        string sandboxProj = System.IO.Path.Combine(solutionRoot, "tests", "Nexus.Sandbox");

        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = "dotnet";
        // 기본 시드 전체 수행 (Lot 포함)
        psi.Arguments = $"run -c Debug --project \"{sandboxProj}\"";
        psi.Environment["ITEST_REDIS_PORT"] = RedisHostPort.ToString();
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        psi.UseShellExecute = false;
        using (Process proc = Process.Start(psi)!)
        {
            // Wait up to 2 minutes for seeding
            if (!proc.WaitForExit((int)TimeSpan.FromMinutes(2).TotalMilliseconds))
            {
                try { proc.Kill(true); } catch { }
                throw new TimeoutException("Sandbox seeding timed out.");
            }
            if (proc.ExitCode != 0)
            {
                throw new InvalidOperationException($"Sandbox seeding failed (exit {proc.ExitCode}).");
            }
        }
    }

    private string? FindSolutionRoot()
    {
        string dir = AppContext.BaseDirectory;
        for (int i = 0; i < 10; i++)
        {
            if (System.IO.File.Exists(System.IO.Path.Combine(dir, "Nexus.sln")))
            {
                return dir;
            }
            string? parent = System.IO.Directory.GetParent(dir)?.FullName;
            if (parent == null)
            {
                break;
            }
            dir = parent;
        }
        return null;
    }

    public void Dispose()
    {
        try
        {
            string? solutionRoot = FindSolutionRoot();
            if (solutionRoot == null)
            {
                return;
            }
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "docker";
            psi.Arguments = "compose --env-file .env.itest -f docker-compose.yml -f docker-compose.itest.yml " + _composeDownArgs;
            psi.WorkingDirectory = solutionRoot;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            using (Process proc = Process.Start(psi)!)
            {
                proc.WaitForExit();
            }
        }
        catch
        {
            // best-effort cleanup
        }
    }
}

[Collection("DockerComposeE2E")]
public class DockerComposeLotPublishE2ETests
{
    private const int RedisHostPort = 6380;
    [Fact(Timeout = 120000)]
    [Trait("Category", "E2E-DockerCompose-FullStack")]
    public async Task PublishLot_Message_TriggersPlanGeneration()
    {
        string redisConn = "localhost:" + RedisHostPort.ToString();
        IConnectionMultiplexer mux = await ConnectionMultiplexer.ConnectAsync(redisConn);

        // 오케스트레이터가 채널 구독을 시작했는지 확인 (구독자 수가 1 이상일 때까지 대기)
        await WaitForOrchestratorSubscriptionAsync(mux, "events:lot:publish", TimeSpan.FromSeconds(30));

        // 시드에서 카세트(CST01/02) 적재 상태가 Redis에 반영될 때까지 대기
        await WaitForCassetteAssignmentsAsync(mux, new[] { "CST01", "CST02" }, TimeSpan.FromSeconds(30));

        // Arrange: seed minimal data required by orchestrator
        RedisLocationRepository locationRepo = new RedisLocationRepository(mux);
        RedisAreaRepository areaRepo = new RedisAreaRepository(mux, locationRepo);
        RedisRobotRepository robotRepo = new RedisRobotRepository(mux, locationRepo);
        RedisTransportRepository transportRepo = new RedisTransportRepository(mux);
        RedisLotRepository lotRepo = new RedisLotRepository(mux, transportRepo);

        // Sandbox에서 미리 추가한 Lot을 사용 (신규 생성 금지)
        string lotId = "LOT-SBX-001";
        Lot? seeded = await lotRepo.GetByIdAsync(lotId);
        if (seeded == null)
        {
            throw new InvalidOperationException("Sandbox 시드 데이터에서 LOT-SBX-001을 찾을 수 없습니다.");
        }
        int expectedCassetteCount = 0;
        if (seeded.LotSteps != null && seeded.LotSteps.Count > 0)
        {
            LotStep firstStep = seeded.LotSteps[0];
            if (firstStep.CassetteIds != null)
            {
                expectedCassetteCount = firstStep.CassetteIds.Count;
            }
        }

        // Act: publish event to redis channel (Portal 동작 대체)
        ISubscriber sub = mux.GetSubscriber();
        LotPublishedEventDto dto = new LotPublishedEventDto();
        dto.Event = "LotPublished";
        dto.LotId = lotId;
        dto.Name = "Sandbox Lot 1";
        dto.Status = ELotStatus.Waiting.ToString();
        dto.Timestamp = DateTime.UtcNow;
        string payload = JsonSerializer.Serialize(dto);
        await sub.PublishAsync(RedisChannel.Literal("events:lot:publish"), payload);
        DateTime lastPublish = DateTime.UtcNow;

        // Assert: poll until orchestrator processes and creates plan groups
        DateTime until = DateTime.UtcNow.AddSeconds(45);
        bool ok = false;
        while (DateTime.UtcNow < until)
        {
            Lot? updated = await lotRepo.GetByIdAsync(lotId);
            if (updated != null)
            {
                if (updated.Status == ELotStatus.Assigned)
                {
                    if (updated.LotSteps != null && updated.LotSteps.Count > 0)
                    {
                        LotStep s = updated.LotSteps[0];
                        if (s.PlanGroups != null && s.PlanGroups.Count > 0)
                        {
                            foreach (PlanGroup pg in s.PlanGroups)
                            {
                                if (pg.GroupType == EPlanGroupType.StockerToArea)
                                {
                                    if (pg.Plans.Count == expectedCassetteCount)
                                    {
                                        ok = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (ok)
            {
                break;
            }
           
            await Task.Delay(1000);
        }

        Assert.True(ok, "오케스트레이터가 PlanGroup을 생성하지 않았습니다.");
    }

    private static async Task WaitForOrchestratorSubscriptionAsync(IConnectionMultiplexer mux, string channel, TimeSpan timeout)
    {
        DateTime deadline = DateTime.UtcNow.Add(timeout);
        EndPoint[] endpoints = mux.GetEndPoints();
        if (endpoints == null || endpoints.Length == 0)
        {
            throw new InvalidOperationException("Redis EndPoints를 확인할 수 없습니다.");
        }

        IServer server = mux.GetServer(endpoints[0]);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                RedisResult result = server.Execute("PUBSUB", new object[] { "NUMSUB", channel });
                RedisResult[] arr = (RedisResult[])result;
                long subscribers = 0;
                if (arr != null && arr.Length >= 2)
                {
                    // arr[0] = channel name, arr[1] = subscriber count
                    if (arr[1].Resp2Type == ResultType.Integer)
                    {
                        subscribers = (long)arr[1];
                    }
                    else
                    {
                        string? s = arr[1].ToString();
                        if (s != null)
                        {
                            if (!long.TryParse(s, out subscribers))
                            {
                                subscribers = 0;
                            }
                        }
                    }
                }

                if (subscribers >= 1)
                {
                    return;
                }
            }
            catch
            {
                // ignore and retry
            }

            await Task.Delay(500);
        }

        throw new InvalidOperationException("오케스트레이터가 이벤트 채널 구독을 시작하지 않았습니다.");
    }

    private static async Task WaitForCassetteAssignmentsAsync(IConnectionMultiplexer mux, IEnumerable<string> cassetteIds, TimeSpan timeout)
    {
        IDatabase db = mux.GetDatabase();
        DateTime deadline = DateTime.UtcNow.Add(timeout);
        HashSet<string> targets = new HashSet<string>(cassetteIds, StringComparer.OrdinalIgnoreCase);
        while (DateTime.UtcNow < deadline)
        {
            // 모든 카세트 위치를 순회하여 current_item_id 매칭 확인
            RedisValue[] ids = await db.SetMembersAsync("cassette_locations:all");
            HashSet<string> remaining = new HashSet<string>(targets, StringComparer.OrdinalIgnoreCase);
            foreach (RedisValue rv in ids)
            {
                string id = rv.ToString();
                if (string.IsNullOrEmpty(id)) { continue; }
                HashEntry[] hash = await db.HashGetAllAsync("cassette_location:" + id);
                if (hash == null || hash.Length == 0) { continue; }
                string currentItemId = GetHashValue(hash, "current_item_id");
                if (!string.IsNullOrEmpty(currentItemId))
                {
                    remaining.Remove(currentItemId);
                }
            }
            if (remaining.Count == 0)
            {
                return; // 모든 대상 카세트가 어떤 위치에든 적재됨
            }
            await Task.Delay(500);
        }
        throw new InvalidOperationException("시드 카세트 적재 상태가 준비되지 않았습니다 (CST01/02).");
    }

    private static string GetHashValue(HashEntry[] entries, string name)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].Name == name)
            {
                return entries[i].Value;
            }
        }
        return string.Empty;
    }

    private static void AssertTcpOpen(string host, int port)
    {
        using (TcpClient client = new TcpClient())
        {
            client.ReceiveTimeout = 2000;
            client.SendTimeout = 2000;
            client.Connect(host, port);
        }
    }

    // HTTP 엔드포인트가 보장되지 않을 수 있어 TCP 연결 확인만 사용
}
