using Nexus.Core.Domain.Models.Areas;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Nexus.Gateway.Services.Commands
{
    public class CreateAreaCommand
    {
        public List<AreaInfo> Areas { get; }

        public CreateAreaCommand(List<AreaInfo> areas)
        {
            Areas = areas;
        }
    }

    public class AreaInfo
    {
        public string id { get; }

        public string name { get; }

        public List<CassetteLocation> cassetteLocations { get; }

        public List<TrayLocation> trayLocations { get; }

        public List<SetInfo> sets { get; }

        public AreaInfo(
            string id,
            string name,
            List<CassetteLocation> cassetteLocations,
            List<TrayLocation> trayLocations,
            List<SetInfo> sets)
        {
            this.id = id;
            this.name = name;
            this.cassetteLocations = cassetteLocations;
            this.trayLocations = trayLocations;
            this.sets = sets;
        }
    }

    public class CassetteLocation
    {
  

        public string id { get; }

        public string name { get; }

        public int locationType { get; }

        public int status { get; }

        public CassetteLocation(
   
            string id,
            string name,
            int locationType,
            int status)
        {

            this.id = id;
            this.name = name;
            this.locationType = locationType;
            this.status = status;
        }
    }

    public class TrayLocation
    {
        public string id { get; }

        public string name { get; }

        public int locationType { get; }

        public int status { get; }


        public TrayLocation(
            string id,
            string name,
            int locationType,
            int status)
        {
            this.id = id;
            this.name = name;
            this.locationType = locationType;
            this.status = status;

        }
    }

    public class SetInfo
    {
        public string id { get; }

        public string name { get; }

        public List<MemoryPort> memoryPorts { get; }

        public SetInfo(
            string id,
            string name,
            List<MemoryPort> memoryPorts)
        {
            this.id = id;
            this.name = name;
            this.memoryPorts = memoryPorts;
        }
    }

    public class MemoryPort
    {

        public string id { get; }

        public string name { get; }

        public int locationType { get; }

        public int status { get; }

        public MemoryPort(
            string id,
            string name,
            int locationType,
            int status)
        {
            this.id = id;
            this.name = name;
            this.locationType = locationType;
            this.status = status;
        }
    }
}