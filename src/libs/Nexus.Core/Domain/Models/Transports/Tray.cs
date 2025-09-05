using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Transports.Enums;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nexus.Core.Domain.Models.Transports
{
    public class Tray : ITransportable
    {
        public string Id { get; }
        public string Name { get; }
        public ETransportType TransportType => ETransportType.Tray;
        public IReadOnlyList<Memory> Memories => _memories.AsReadOnly();

        public int MemoryCount => _memories.Count;
        public int MaxMemoryCapacity => MAX_MEMORY_CAPACITY;
        public int AvailableMemorySlots => MAX_MEMORY_CAPACITY - MemoryCount;
        public bool IsFull => MemoryCount >= MAX_MEMORY_CAPACITY;
        public bool IsEmpty => MemoryCount == 0;

        public const int MAX_MEMORY_CAPACITY = 25;

        private readonly List<Memory> _memories = new List<Memory>();

        public Tray(string id, string name, List<Memory> memories)
        {
            if (memories.Count > MAX_MEMORY_CAPACITY)
            {
                throw new InvalidOperationException($"Tray cannot hold more than {MAX_MEMORY_CAPACITY} memories. Attempted to add {memories.Count} memories.");
            }

            Id = id;
            Name = name;
            _memories.AddRange(memories);
        }

        // Memory 관리 메서드
        public bool CanAddMemory()
        {
            return MemoryCount < MAX_MEMORY_CAPACITY;
        }
        public void AddMemory(Memory memory)
        {
            if (!CanAddMemory())
            {
                throw new InvalidOperationException($"Cannot add memory. Tray is at maximum capacity ({MAX_MEMORY_CAPACITY} memories).");
            }

            _memories.Add(memory);
        }
        public bool RemoveMemory(string memoryId)
        {
            var memory = _memories.FirstOrDefault(m => m.Id == memoryId);
            if (memory != null)
            {
                return _memories.Remove(memory);
            }
            return false;
        }

        public Memory? GetMemoryById(string memoryId)
        {
            return _memories.FirstOrDefault(m => m.Id == memoryId);
        }

        public static Tray CreateFullTray(string trayId, string trayName = "")
        {
            var memories = new List<Memory>();

            for (int memoryIndex = 1; memoryIndex <= MAX_MEMORY_CAPACITY; memoryIndex++)
            {
                var memoryId = Guid.NewGuid().ToString();
                memories.Add(new Memory(memoryId, ""));
            }

            return new Tray(trayId, trayName, memories);
        }
    }
}