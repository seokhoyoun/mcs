using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Shared.Application.DTO;
using System.Collections.Generic;

namespace Nexus.Core.Domain.Models.Transports.Service
{
    public class TransportService
    {
        private readonly List<Cassette> _cassettes = new();
        private readonly List<Tray> _trays = new();
        private readonly List<Memory> _memories = new();

        private readonly Dictionary<string, ITransportable> _transportMap = new();

        public TransportService(ITransportsRepository repository)
        {
            foreach (MemoryState state in repository.GetAllMemories())
            {
                var memory = new Memory(state.Id, state.Name);
                _memories.Add(memory);
                _transportMap[memory.Id] = memory;
            }

            foreach (TrayState state in repository.GetAllTrays())
            {
                var memories = _memories.Where(m => state.MemoryIds.Contains(m.Id)).ToList();
                var tray = new Tray(state.Id, state.Name, memories);
                _trays.Add(tray);
                _transportMap[tray.Id] = tray;
            }

            foreach (CassetteState state in repository.GetAllCassettes())
            {
                var trays = _trays.Where(t => state.TrayIds.Contains(t.Id)).ToList();
                var cassette = new Cassette(state.Id, state.Name, trays);
                _cassettes.Add(cassette);
                _transportMap[cassette.Id] = cassette;
            }
        }

        public ITransportable? GetItemById(string currentItemId)
        {
            return _transportMap.TryGetValue(currentItemId, out var item) ? item : null;
        }
    }

}

