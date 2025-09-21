using Nexus.Core.Domain.Models.Lots;
using Nexus.Core.Domain.Models.Lots.Enums;
using Nexus.Infrastructure.Persistence.Redis;
using Nexus.Sandbox.Seed.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nexus.Sandbox.Seed
{
    internal class LotSeeder : IDataSeeder
    {
        private readonly RedisLotRepository _lotRepo;

        public LotSeeder(RedisLotRepository lotRepo)
        {
            _lotRepo = lotRepo;
        }

        public async Task SeedAsync()
        {
            // Lot 1
            List<string> lot1Cassettes = new List<string>();
            lot1Cassettes.Add("CST01");
            lot1Cassettes.Add("CST02");
            Lot lot1 = new Lot(
                id: "LOT-SBX-001",
                name: "Sandbox Lot 1",
                status: ELotStatus.None,
                priority: 1,
                receivedTime: DateTime.UtcNow,
                purpose: "DEV",
                evalNo: "EV-001",
                partNo: "PT-001",
                qty: 0,
                option: string.Empty,
                line: "L1",
                cassetteIds: lot1Cassettes
            );

            LotStep lot1Step1 = new LotStep(
                id: "LOT-SBX-001_01",
                lotId: lot1.Id,
                name: "LOT-SBX-001_01",
                loadingType: 1,
                dpcType: "DPC-A",
                chipset: "CH-01",
                pgm: "PGM-01",
                planPercent: 100,
                status: ELotStatus.None
            );
            lot1Step1.CassetteIds = new List<string>(lot1Cassettes);
            lot1.LotSteps.Add(lot1Step1);

            LotStep lot1Step2 = new LotStep(
                id: "LOT-SBX-001_02",
                lotId: lot1.Id,
                name: "LOT-SBX-001_02",
                loadingType: 1,
                dpcType: "DPC-A",
                chipset: "CH-01",
                pgm: "PGM-01",
                planPercent: 100,
                status: ELotStatus.None
            );
            lot1Step2.CassetteIds = new List<string>(lot1Cassettes);
            lot1.LotSteps.Add(lot1Step2);

            await _lotRepo.AddAsync(lot1);

            // Lot 2
            List<string> lot2Cassettes = new List<string>();
            lot2Cassettes.Add("CST03");
            lot2Cassettes.Add("CST04");
            lot2Cassettes.Add("CST05");
            Lot lot2 = new Lot(
                id: "LOT-SBX-002",
                name: "Sandbox Lot 2",
                status: ELotStatus.None,
                priority: 2,
                receivedTime: DateTime.UtcNow,
                purpose: "DEV",
                evalNo: "EV-002",
                partNo: "PT-002",
                qty: 0,
                option: string.Empty,
                line: "L1",
                cassetteIds: lot2Cassettes
            );

            LotStep lot2Step1 = new LotStep(
                id: "LOT-SBX-002_01",
                lotId: lot2.Id,
                name: "LOT-SBX-002_01",
                loadingType: 1,
                dpcType: "DPC-B",
                chipset: "CH-02",
                pgm: "PGM-02",
                planPercent: 100,
                status: ELotStatus.None
            );
            lot2Step1.CassetteIds = new List<string>(lot2Cassettes);
            lot2.LotSteps.Add(lot2Step1);

            await _lotRepo.AddAsync(lot2);
        }
    }
}

