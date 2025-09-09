using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Transports.Enums;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nexus.Core.Domain.Models.Transports
{
    public class Cassette : ITransportable
    {
        public const int MAX_TRAY_CAPACITY = 6;

        public string Id { get; }
        public string Name { get; }
        public ETransportType TransportType => ETransportType.Cassette;
        public IReadOnlyList<Tray> Trays => _trays.AsReadOnly();

        private readonly List<Tray> _trays = new List<Tray>();

        // 수량 정보 속성
        public int TrayCount => _trays.Count;
        public int MaxTrayCapacity => MAX_TRAY_CAPACITY;
        public int AvailableTraySlots => MAX_TRAY_CAPACITY - TrayCount;
        public int TotalMemoryCount => _trays.Sum(tray => tray.Memories.Count);
        public int MaxMemoryCapacity => MAX_TRAY_CAPACITY * Tray.MAX_MEMORY_CAPACITY;
        public int AvailableMemorySlots => MaxMemoryCapacity - TotalMemoryCount;
        public bool IsFull => TrayCount >= MAX_TRAY_CAPACITY;
        public bool IsEmpty => TrayCount == 0;

        public Cassette(string id, string name, List<Tray> trays)
        {
            if (trays.Count > MAX_TRAY_CAPACITY)
            {
                throw new InvalidOperationException($"Cassette cannot hold more than {MAX_TRAY_CAPACITY} trays. Attempted to add {trays.Count} trays.");
            }

            Id = id;
            Name = name;
            _trays.AddRange(trays);
        }

        // Tray 관리 메서드
        public bool CanAddTray()
        {
            return TrayCount < MAX_TRAY_CAPACITY;
        }

        public void AddTray(Tray tray)
        {
            if (!CanAddTray())
            {
                throw new InvalidOperationException($"Cannot add tray. Cassette is at maximum capacity ({MAX_TRAY_CAPACITY} trays).");
            }

            if (tray.Memories.Count > Tray.MAX_MEMORY_CAPACITY)
            {
                throw new InvalidOperationException($"Tray cannot hold more than {Tray.MAX_MEMORY_CAPACITY} memories. Tray has {tray.Memories.Count} memories.");
            }

            _trays.Add(tray);
        }

        public bool RemoveTray(string trayId)
        {
            Tray? tray = _trays.FirstOrDefault(t => t.Id == trayId);
            if (tray != null)
            {
                return _trays.Remove(tray);
            }
            return false;
        }

        public Tray? GetTrayById(string trayId)
        {
            return _trays.FirstOrDefault(t => t.Id == trayId);
        }

        // 가득 찬 Cassette 생성 팩토리 메서드
   
    }
}
