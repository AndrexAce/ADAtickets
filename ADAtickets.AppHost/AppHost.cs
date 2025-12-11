namespace ADAtickets.AppHost;

public abstract class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        // Add the Redis cache
        var cache = builder.AddRedis("Redis")
            .WithDataVolume("dev-redis-data")
            .WithArgs("--appendonly", "yes");

        // Add the database
        var database = builder.AddPostgres("Postgres")
            .WithDataVolume("dev-database-data")
            .WithPgWeb(containerName: "PgAdmin")
            .AddDatabase("ADAtickets");

        builder.Build().Run();
    }
}