using System;
using System.Collections.Generic;

namespace Nexus.Core.Domain.Models.Transports.Extensions
{
    public static class TransportExtensions
    {
       
        public static void InitializeFullCassette(this Cassette cassette)
        {
            var trays = new List<Tray>();

            for (int trayIndex = 1; trayIndex <= Cassette.MAX_TRAY_CAPACITY; trayIndex++)
            {
                var trayId = $"{cassette.Id}_T{trayIndex:D2}";
                var tray = new Tray(id: trayId,
                                    name: trayId,
                                    memories: new List<Memory>());
                tray.InitializeFullTray();

                trays.Add(tray);
            }

        }

     
        public static void InitializeFullTray(this Tray tray)
        {
            var memories = new List<Memory>();

            for (int memoryIndex = 1; memoryIndex <= Tray.MAX_MEMORY_CAPACITY; memoryIndex++)
            {
                var memoryId = $"{tray.Id}_M{memoryIndex:D2}";
                memories.Add(new Memory(id: memoryId,
                                        name: string.Empty));
            }

        }

     
    }
}