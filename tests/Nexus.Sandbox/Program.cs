using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Infrastructure.Persistence.Redis;
using Nexus.Sandbox.Seed;
using Nexus.Sandbox.Seed.Interfaces;
using StackExchange.Redis;
using System.Data;
using System.Net;
using System.Text.Json;

namespace Nexus.Sandbox
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
          

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379,allowAdmin=true");

            // Always FLUSHALL before running seeders
            EndPoint[] endpoints = redis.GetEndPoints();
            for (int i = 0; i < endpoints.Length; i++)
            {
                IServer server = redis.GetServer(endpoints[i]);
                try
                {
                    server.FlushAllDatabases();
                    Console.WriteLine($"Redis FLUSHALL executed on {server.EndPoint}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Redis FLUSHALL failed on {server.EndPoint}: {ex.Message}");
                }
            }
            RedisLocationRepository locationRepo = new RedisLocationRepository(redis);
            RedisTransportRepository transportRepo = new RedisTransportRepository(redis);
            RedisAreaRepository areaRepo = new RedisAreaRepository(redis, locationRepo);
            RedisStockerRepository stockerRepo = new RedisStockerRepository(redis, locationRepo);
            RedisRobotRepository robotRepo = new RedisRobotRepository(redis, locationRepo);
            RedisDimensionRepository dimesionRepo = new RedisDimensionRepository(redis);
            RedisLotRepository lotRepo = new RedisLotRepository(redis, transportRepo);

            List<IDataSeeder> seeders = new List<IDataSeeder>();
            seeders.Add(new DimensionSeeder(dimesionRepo));
            seeders.Add(new CassetteSeeder(transportRepo));
            seeders.Add(new AreaSeeder(areaRepo, dimesionRepo));
            seeders.Add(new StockerSeeder(stockerRepo, dimesionRepo));
            seeders.Add(new MarkerSeeder(locationRepo, areaRepo, stockerRepo));
            seeders.Add(new RobotSeeder(robotRepo, locationRepo, dimesionRepo));
            seeders.Add(new LotSeeder(lotRepo));
            

            foreach (IDataSeeder seeder in seeders)
            {
                await seeder.SeedAsync();
                Console.WriteLine($"{seeder.GetType()} Seeding completed.");
            }


            Console.WriteLine("Seeding completed.");
        }

      


    }
}
