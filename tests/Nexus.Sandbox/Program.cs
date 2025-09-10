using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Infrastructure.Persistence.Redis;
using Nexus.Sandbox.Seed;
using Nexus.Sandbox.Seed.Interfaces;
using StackExchange.Redis;
using System.Data;
using System.Text.Json;

namespace Nexus.Sandbox
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379");
            RedisLocationRepository locationRepo = new RedisLocationRepository(redis);
            RedisTransportRepository transportRepo = new RedisTransportRepository(redis);
            RedisAreaRepository areaRepo = new RedisAreaRepository(redis, locationRepo);
            RedisStockerRepository stockerRepo = new RedisStockerRepository(redis, locationRepo);

            List<IDataSeeder> seeders = new List<IDataSeeder>
            {
                new CassetteSeeder(transportRepo),
                new AreaSeeder(areaRepo),
                new StockerSeeder(stockerRepo)
            };

            foreach (IDataSeeder seeder in seeders)
            {
                await seeder.SeedAsync();
                
            }


            Console.WriteLine("Seeding completed.");
        }

      


    }
}
