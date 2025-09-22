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
        private bool _initialized = false;
        private readonly object _initLock = new object();
        private Task? _initTask;

        public TransportService(
            ILogger<LocationService> logger, 
            ITransportRepository transportRepository) : base(logger, transportRepository)
        {
            _transportRepository = transportRepository;
        }

        private async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
        {
            if (_initialized)
            {
                return;
            }
            Task? startTask = null;
            lock (_initLock)
            {
                if (_initialized)
                {
                    return;
                }
                if (_initTask == null)
                {
                    _initTask = InitializeCoreAsync(cancellationToken);
                }
                startTask = _initTask;
            }
            await startTask;
        }

        private async Task InitializeCoreAsync(CancellationToken cancellationToken)
        {
            try
            {
                IReadOnlyList<ITransportable> allTransports = await _transportRepository.GetAllAsync(cancellationToken);

                _cassettes.Clear();
                _trays.Clear();
                _memories.Clear();
                _transportMap.Clear();

                if (allTransports == null || allTransports.Count == 0)
                {
                    _logger.LogWarning("초기화된 Transport 데이터가 없습니다.");
                }
                else
                {
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
                _initialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TransportService 초기화 중 오류 발생");
                throw;
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
