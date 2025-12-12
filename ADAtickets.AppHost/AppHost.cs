/*
 * ADAtickets is a simple, lightweight, open source ticketing system
 * interacting with your enterprise repositories on Azure DevOps
 * with a two-way synchronization.
 * Copyright (C) 2025  Andrea Lucchese
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Projects;

namespace ADAtickets.AppHost;

file abstract class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        // Add the Docker Compose environment
        builder.AddDockerComposeEnvironment("compose")
            .WithDashboard(false);

        // Add the Redis cache
        var cache = builder.AddRedis("redis")
            .WithDataVolume("dev-redis-data")
            .WithArgs("--appendonly", "yes")
            .PublishAsDockerComposeService(static (_, service) => { service.Name = "redis"; });

        // Add the Postgres database with PgAdmin
        var database = builder.AddPostgres("postgres")
            .WithDataVolume("dev-postgres-data")
            .WithPgAdmin(static pgadmin =>
                pgadmin.WithVolume("dev-pgadmin-data", "/var/lib/pgadmin"), "pgadmin")
            .AddDatabase("adatickets");

        // Add the Minio storage
        var storage = builder.AddMinioContainer("minio")
            .WithDataVolume("dev-minio-data")
            .PublishAsDockerComposeService(static (_, service) => { service.Name = "minio"; });

        // Add the API project
        builder.AddProject<ADAtickets_Api>("api")
            .WithReference(cache)
            .WithReference(database)
            .WithReference(storage)
            .WaitFor(cache)
            .WaitFor(database)
            .WaitFor(storage)
            .PublishAsDockerComposeService(static (_, service) =>
            {
                service.Name = "api";
                service.Image = "ghcr.io/andrexace/adatickets-api:latest";
            });

        builder.Build().Run();
    }
}