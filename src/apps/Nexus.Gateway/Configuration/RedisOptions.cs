namespace Nexus.Gateway.Configuration
{
    public class RedisOptions
    {
        public const string SECTION_NAME = "Redis";

        public string ConnectionString { get; set; } = "localhost:6379";
        public bool AbortConnect { get; set; } = false;
        public int ConnectTimeout { get; set; } = 5000;
        public int SyncTimeout { get; set; } = 5000;

        public string GetConnectionString()
        {
            return $"{ConnectionString},abortConnect={AbortConnect},connectTimeout={ConnectTimeout},syncTimeout={SyncTimeout}";
        }
    }
}