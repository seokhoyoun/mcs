using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
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
    private readonly TimeSpan _startupTimeout = TimeSpan.FromSeconds(60);

    public DockerComposeFixture()
    {
        StartDockerCompose();
        WaitForRedis();
        RunSandboxSeed();
        // After seeding, ensure app ports are ready
        WaitForTcp("127.0.0.1", 8081, _startupTimeout);
        WaitForTcp("127.0.0.1", 8082, _startupTimeout);
    }

    private void StartDockerCompose()
    {
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = "docker";
        psi.Arguments = "compose " + _composeArgs;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        psi.UseShellExecute = false;

        using (Process proc = Process.Start(psi)!)
        {
            proc.WaitForExit();
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
                    client.Connect("127.0.0.1", 6379);
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
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "docker";
            psi.Arguments = "compose " + _composeDownArgs;
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
    [Fact]
    [Trait("Category", "E2E-DockerCompose-FullStack")]
    public async Task PublishLot_Message_TriggersPlanGeneration()
    {
        string redisConn = "localhost:6379";
        IConnectionMultiplexer mux = await ConnectionMultiplexer.ConnectAsync(redisConn);

        // Arrange: seed minimal data required by orchestrator
        RedisLocationRepository locationRepo = new RedisLocationRepository(mux);
        RedisAreaRepository areaRepo = new RedisAreaRepository(mux, locationRepo);
        RedisRobotRepository robotRepo = new RedisRobotRepository(mux, locationRepo);
        RedisTransportRepository transportRepo = new RedisTransportRepository(mux);
        RedisLotRepository lotRepo = new RedisLotRepository(mux, transportRepo);

        // Clean slate for this lot id
        string lotId = "LOT-E2E-001";
        await lotRepo.DeleteAsync(lotId);

        // Create cassette locations that match cassette ids used below
        CassetteLocation cl1 = new CassetteLocation("CST11", "CassetteLocation_CST11");
        CassetteLocation cl2 = new CassetteLocation("CST12", "CassetteLocation_CST12");
        await locationRepo.AddAsync(cl1);
        await locationRepo.AddAsync(cl2);

        // Create an area with a couple of empty cassette ports
        List<CassetteLocation> areaCassettePorts = new List<CassetteLocation>();
        areaCassettePorts.Add(new CassetteLocation("A10.CP01", "A10_CP01"));
        areaCassettePorts.Add(new CassetteLocation("A10.CP02", "A10_CP02"));
        List<TrayLocation> areaTrayPorts = new List<TrayLocation>();
        List<Set> sets = new List<Set>();
        Area area = new Area("A10", "Area10", areaCassettePorts, areaTrayPorts, sets);
        area.Status = EAreaStatus.Idle;
        await areaRepo.AddAsync(area);

        // Seed at least one robot so orchestrator can simulate
        List<Location> robotLocations = new List<Location>();
        Robot robot = new Robot("RBT01", "Robot01", ERobotType.Logistics, robotLocations);
        await robotRepo.AddAsync(robot);

        // Seed transport items referenced by lot step
        List<Tray> trays1 = new List<Tray>();
        List<Tray> trays2 = new List<Tray>();
        Cassette cs1 = new Cassette("CST11", "Cassette 11", trays1);
        Cassette cs2 = new Cassette("CST12", "Cassette 12", trays2);
        await transportRepo.AddAsync(cs1);
        await transportRepo.AddAsync(cs2);

        // Directly seed the Lot into Redis via repository (요청자 요구사항)
        List<string> cassetteIds = new List<string>();
        cassetteIds.Add("CST11");
        cassetteIds.Add("CST12");

        Lot lot = new Lot(
            id: lotId,
            name: "E2E Lot",
            status: ELotStatus.None,
            priority: 1,
            receivedTime: DateTime.UtcNow,
            purpose: "E2E",
            evalNo: "E2E-001",
            partNo: "PT-001",
            qty: 0,
            option: string.Empty,
            line: "L1",
            cassetteIds: cassetteIds
        );

        LotStep step = new LotStep(
            id: lotId + "_01",
            lotId: lot.Id,
            name: lotId + "_01",
            loadingType: 1,
            dpcType: "DPC",
            chipset: "CH",
            pgm: "PGM",
            planPercent: 100,
            status: ELotStatus.None
        );
        step.CassetteIds = new List<string>(cassetteIds);
        lot.LotSteps.Add(step);
        await lotRepo.AddAsync(lot);

        // Act: publish event to redis channel (Portal 동작 대체)
        ISubscriber sub = mux.GetSubscriber();
        LotPublishedEventDto dto = new LotPublishedEventDto();
        dto.Event = "LotPublished";
        dto.LotId = lotId;
        dto.Name = "E2E Lot";
        dto.Status = ELotStatus.Waiting.ToString();
        dto.Timestamp = DateTime.UtcNow;
        string payload = JsonSerializer.Serialize(dto);
        await sub.PublishAsync(RedisChannel.Literal("events:lot:publish"), payload);

        // Assert: poll until orchestrator processes and creates plan groups
        DateTime until = DateTime.UtcNow.AddSeconds(30);
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
                                    if (pg.Plans.Count == cassetteIds.Count)
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
}
