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

using System.Net.Mime;
using ADAtickets.ServiceDefaults;
using ADAtickets.Shared.Enums;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Scalar.AspNetCore;
using StackExchange.Redis;
using Status = ADAtickets.Shared.Enums.Status;

namespace ADAtickets.Api;

/// <summary>
///     Bootstrap class for the application.
/// </summary>
file static class Program
{
    /// <summary>
    ///     Entrypoint of the application.
    /// </summary>
    /// <param name="args">Additional arguments passed by the command line.</param>
    /// <returns>The <see cref="Task" /> running the application.</returns>
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder);

        var app = builder.Build();
        await ConfigureApplicationAsync(app);

        // Start the application.
        await app.RunAsync();
    }

    /// <summary>
    ///     Configures the services used by the application.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder" /> that creates the required services.</param>
    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        // Add service defaults for Aspire
        _ = builder.AddServiceDefaults();

        // Add PostgreSQL with Aspire
        builder.AddNpgsqlDbContext<AdaTicketsDbContext>("ADAtickets", configureDbContextOptions: static options =>
        {
            // Create the enumerations in the connected database.
            options.UseNpgsql(static npgsqlOptions =>
            {
                npgsqlOptions.MapEnum<Priority>("priority")
                    .MapEnum<Status>("status")
                    .MapEnum<TicketType>("ticket_type")
                    .MapEnum<UserType>("user_type")
                    .EnableRetryOnFailure();
            });
        });

        // Add Redis with Aspire
        builder.AddRedisClient("redis");
        builder.AddRedisDistributedCache("redis");

        // Add Redis for data protection keys storage
        var redisConnection = builder.Configuration.GetConnectionString("redis");

        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            // Register a single ConnectionMultiplexer instance for the app lifetime
            var mux = ConnectionMultiplexer.Connect(redisConnection);
            builder.Services.AddSingleton<IConnectionMultiplexer>(mux);

            // Persist data protection keys to the SAME multiplexer
            builder.Services.AddDataProtection()
                .PersistKeysToStackExchangeRedis(mux, "ApiService-DataProtection-Keys");
        }

        // Add MinIO storage with Aspire
        builder.AddMinioClient("minio");

        // Add services commonly used with controllers APIs.
        builder.Services
            .AddControllers(static options =>
            {
                options.RespectBrowserAcceptHeader = true;
                options.ReturnHttpNotAcceptable = true;
            })
            .ConfigureApiBehaviorOptions(static options =>
            {
                options.InvalidModelStateResponseFactory = static context =>
                    new BadRequestObjectResult(context.ModelState)
                    {
                        ContentTypes = { MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml }
                    };
            })
            .AddXmlSerializerFormatters()
            .AddNewtonsoftJson(static options =>
            {
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

        // Add OpenAPI documentation for the APIs.
        builder.Services.AddOpenApi(static options => options.AddScalarTransformers());

        // Add the API explorer for endpoint discovery.
        builder.Services.AddEndpointsApiExplorer();

        // Add services used to return detailed error messages for failed requests.
        builder.Services.AddProblemDetails();

        // Add real-time communication with SignalR.
        builder.Services.AddSignalR();
    }


    /// <summary>
    ///     Configures the application.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication" /> with the created services.</param>
    private static async Task ConfigureApplicationAsync(WebApplication app)
    {
        // Add error handling and OpenAPI in development environment
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        else
        {
            app.UseExceptionHandler();
        }

        // Add middlewares
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.UseAuthentication();
        app.UseAuthorization();

        // Apply migrations on startup
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AdaTicketsDbContext>();
            await db.Database.MigrateAsync();
        }

        // Map default endpoints (health checks, etc.) with Aspire
        _ = app.MapDefaultEndpoints();

        // Map controllers endpoints
        app.MapControllers();
    }
}