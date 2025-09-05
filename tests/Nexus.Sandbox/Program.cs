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
            var redis = ConnectionMultiplexer.Connect("localhost:6379");
            var repo = new RedisTransportsRepository(redis);

            var seeders = new List<IDataSeeder>
            {
                new CassetteSeeder(repo)
            };

            foreach (var seeder in seeders)
            {
                await seeder.SeedAsync();
            }



            Console.WriteLine("Seeding completed.");
        }

      


    }
}
