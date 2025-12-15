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
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using ADAtickets.Api.Services;
using ADAtickets.ServiceDefaults;
using ADAtickets.Shared.Enums;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using StackExchange.Redis;
using Status = ADAtickets.Shared.Enums.Status;

namespace ADAtickets.Api;

file static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder);

        var app = builder.Build();
        await ConfigureApplicationAsync(app);

        // Start the application.
        await app.RunAsync();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        // Add services to DI
        builder.Services.AddSingleton<IEmailSender<IdentityUser<Guid>>, EmailSender>();

        // Add service defaults for Aspire
        _ = builder.AddServiceDefaults();

        // Add PostgreSQL with Aspire
        builder.AddNpgsqlDbContext<AdaTicketsDbContext>("adatickets", configureDbContextOptions: static options =>
        {
            // Create the enumerations in the connected database.
            options.UseNpgsql(static npgsqlOptions =>
            {
                npgsqlOptions.MapEnum<Priority>("priority")
                    .MapEnum<Status>("status")
                    .MapEnum<TicketType>("ticket_type")
                    .EnableRetryOnFailure();
            });
        });

        // Add identity management with EF
        builder.Services.AddIdentityApiEndpoints<IdentityUser<Guid>>(static options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AdaTicketsDbContext>();

        // Configure DI for the authorization services
        builder.Services.AddAuthorization();

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

            // Persist data protection keys to the same multiplexer
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
            .AddJsonOptions(static options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });


        // Add OpenAPI documentation for the APIs.
        builder.Services.AddOpenApi(static options =>
        {
            options.AddDocumentTransformer(static (document, _, _) =>
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                document.Info.Version = $"{version?.Major}.{version?.Minor}.{version?.Build}";
                document.Info.Title = "ADAtickets API reference";
                document.Info.License = new OpenApiLicense
                {
                    Name = "GNU General Public License v3.0",
                    Url = new Uri("https://www.gnu.org/licenses/gpl-3.0.en.html")
                };
                return Task.CompletedTask;
            });

            options.AddScalarTransformers();
        });

        // Add the API explorer for endpoint discovery.
        builder.Services.AddEndpointsApiExplorer();

        // Add services used to return detailed error messages for failed requests.
        builder.Services.AddProblemDetails();

        // Add CORS support.
        builder.Services.AddCors(static options =>
        {
            options.AddDefaultPolicy(static policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        // Add real-time communication with SignalR.
        builder.Services.AddSignalR();
    }

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
        app.UseCors();
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

        // Map identity endpoints
        _ = app.MapIdentityApi<IdentityUser<Guid>>();

        // Map controllers endpoints
        app.MapControllers();
    }
}