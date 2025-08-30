using Nexus.Core.Domain.Models.Transports.Enums;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Shared.Application.DTO;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nexus.Core.Domain.Models.Transports.Services
{
    public class TransportService : ITransportService
    {
        private readonly List<Cassette> _cassettes = new();
        private readonly List<Tray> _trays = new();
        private readonly List<Memory> _memories = new();

        private readonly Dictionary<string, ITransportable> _transportMap = new();

        public TransportService(ITransportsRepository repository)
        {
            InitializeTransportDataAsync(repository).GetAwaiter().GetResult();
        }

        private async Task InitializeTransportDataAsync(ITransportsRepository repository)
        {
            // 모든 Transport 데이터 조회
            var allTransports = await repository.GetAllAsync();

            // 타입별로 분류하여 초기화
            foreach (var transport in allTransports)
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

        public ITransportable? GetItemById(string currentItemId)
        {
            return _transportMap.TryGetValue(currentItemId, out var item) ? item : null;
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
            var allTransports = new List<ITransportable>();
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

        public bool ContainsTransport(string id)
        {
            return _transportMap.ContainsKey(id);
        }

        public int GetTotalTransportCount()
        {
            return _transportMap.Count;
        }

        public int GetCassetteCount()
        {
            return _cassettes.Count;
        }

        public int GetTrayCount()
        {
            return _trays.Count;
        }

        public int GetMemoryCount()
        {
            return _memories.Count;
        }
    }
}