using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Locations.ValueObjects;
using Nexus.Core.Domain.Models.Transports.Enums;
using Nexus.Infrastructure.Persistence.Redis;
using StackExchange.Redis;

internal class Program
{
    private static async Task Main(string[] args)
    {
        string connectionString = GetConnectionString();
        IConnectionMultiplexer mux = ConnectionMultiplexer.Connect(connectionString);

        ISpaceRepository spaceRepository = new RedisSpaceRepository(mux);

        // 단일 Space와 CarrierLocation 샘플 데이터를 추가합니다.
        CarrierLocation port = new CarrierLocation("SPC01.PORT01", "SamplePort01", "spec:default", ECargoKind.Unknown);
        SpaceSpecification spec = new SpaceSpecification("spec:space", new List<string> { "spec:default" }, new List<ECargoKind> { ECargoKind.Unknown });
        Space space = new Space("SPACE01", "Sample Space", spec, new List<CarrierLocation> { port }, new List<ISpace>());

        await spaceRepository.AddAsync(space);

        Console.WriteLine("Seeded Space and CarrierLocation to Redis.");
    }

    private static string GetConnectionString()
    {
        string? env = Environment.GetEnvironmentVariable("Redis__ConnectionString");
        if (!string.IsNullOrWhiteSpace(env))
        {
            return env!;
        }
        return "localhost:6379";
    }
}
