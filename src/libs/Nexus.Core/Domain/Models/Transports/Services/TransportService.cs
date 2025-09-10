using Microsoft.Extensions.Logging;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Locations.Services;
using Nexus.Core.Domain.Models.Transports.Enums;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Domain.Shared.Bases;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nexus.Core.Domain.Models.Transports.Services
{
    public class TransportService : BaseDataService<ITransportable, string>, ITransportService
    {
        ITransportRepository _transportRepository;

        private readonly List<Cassette> _cassettes = new();
        private readonly List<Tray> _trays = new();
        private readonly List<Memory> _memories = new();

        private readonly Dictionary<string, ITransportable> _transportMap = new();

        public TransportService(
            ILogger<LocationService> logger, 
            ITransportRepository transportRepository) : base(logger, transportRepository)
        {
            _transportRepository = transportRepository;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            // 모든 Transport 데이터 조회
            IReadOnlyList<ITransportable> allTransports = await _transportRepository.GetAllAsync();

            // 타입별로 분류하여 초기화
            foreach (ITransportable transport in allTransports)
            {
                switch (transport.TransportType)
                {
                    case ETransportType.Cassette:
                        _cassettes.Add((Cassette)transport);
                        break;
                    case ETransportType.Tray:
                        _trays.Add((Tray)transport);
                        break;
                    case ETransportType.Memory:
                        _memories.Add((Memory)transport);
                        break;
                    default:
                        Debug.Assert(false, $"Unknown transport type: {transport.TransportType}");
                        break;
                }
                _transportMap[transport.Id] = transport;
            }
        }

        public IReadOnlyList<Cassette> GetAllCassettes()
        {
            return _cassettes.AsReadOnly();
        }

        public IReadOnlyList<Tray> GetAllTrays()
        {
            return _trays.AsReadOnly();
        }

        public IReadOnlyList<Memory> GetAllMemories()
        {
            return _memories.AsReadOnly();
        }

        public IReadOnlyList<ITransportable> GetAllTransports()
        {
            List<ITransportable> allTransports = new List<ITransportable>();
            allTransports.AddRange(_cassettes);
            allTransports.AddRange(_trays);
            allTransports.AddRange(_memories);
            return allTransports.AsReadOnly();
        }

        public Cassette? GetCassetteById(string id)
        {
            return _cassettes.FirstOrDefault(c => c.Id == id);
        }

        public Tray? GetTrayById(string id)
        {
            return _trays.FirstOrDefault(t => t.Id == id);
        }

        public Memory? GetMemoryById(string id)
        {
            return _memories.FirstOrDefault(m => m.Id == id);
        }

  
    }
}
