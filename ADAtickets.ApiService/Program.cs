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

using ADAtickets.ApiService.Configs;
using ADAtickets.ApiService.Controllers;
using ADAtickets.ApiService.Hubs;
using ADAtickets.ApiService.Repositories;
using ADAtickets.ApiService.Services;
using ADAtickets.Shared.Constants;
using ADAtickets.Shared.Extensions;
using ADAtickets.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using StackExchange.Redis;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json.Serialization;

namespace ADAtickets.ApiService;

/// <summary>
///     Bootstrap class for the application.
/// </summary>
internal static class Program
{
    /// <summary>
    ///     Entrypoint of the application.
    /// </summary>
    /// <param name="args">Additional arguments passed by command line.</param>
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
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        // Add the DBContext to execute queries against the database.
        _ = builder.Services.AddDbContext<ADAticketsDbContext>(options =>
        {
            // Configure the DBContext to use PostgreSQL.
            _ = options.UseNpgsql(builder.Configuration.GetConnectionString(Service.Database), options =>
                {
                    // Create the enumerations in the connected database.
                    _ = options.MapEnum<Priority>("priority")
                        .MapEnum<Status>("status")
                        .MapEnum<TicketType>("ticket_type")
                        .MapEnum<UserType>("user_type")
                        .EnableRetryOnFailure();
                })
                .UseSnakeCaseNamingConvention();
        });

        // Add JWTs decoding for both Entra ID and Entra External ID.
        var authBuilder = builder.Services.AddAuthentication();
        _ = authBuilder.AddMicrosoftIdentityWebApi(builder.Configuration.GetSection(Scheme.OpenIdConnectDefault),
            Scheme.OpenIdConnectDefault);
        _ = authBuilder.AddMicrosoftIdentityWebApi(
            builder.Configuration.GetSection(Scheme.ExternalOpenIdConnectDefault), Scheme.ExternalOpenIdConnectDefault);

        // Add authorization policies.
        CreatePolicies(builder.Services.AddAuthorizationBuilder(), builder.Configuration);

        // Add services commonly used with controllers APIs.
        _ = builder.Services
            .AddControllers(options =>
            {
                // Require the APIs to respect the browser request media type
                options.RespectBrowserAcceptHeader = true;
                options.ReturnHttpNotAcceptable = true;
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                // Configure API behavior for invalid model state
                options.InvalidModelStateResponseFactory = context =>
                    new BadRequestObjectResult(context.ModelState)
                    {
                        ContentTypes = { MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml }
                    };
            })
            .AddXmlSerializerFormatters()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

        // Add Swagger documentation for the APIs.
        _ = builder.Services.AddEndpointsApiExplorer();
        _ = builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc($"v{Service.APIVersion}", new OpenApiInfo
            {
                Version = $"v{Service.APIVersion}",
                Title = "ADAtickets API",
                Description =
                    "Web API to interact with the ADAtickets ticketing system with authentication endpoints and JWT validation.",
                Contact = new OpenApiContact
                {
                    Name = "Andrea Lucchese",
                    Email = "andrylook14@gmail.com"
                },
                License = new OpenApiLicense
                {
                    Name = "GPL v3",
                    Url = new Uri("https://github.com/AndrexAce/ADAtickets/blob/master/LICENSE.txt")
                }
            });

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });

        // Add services used to return detailed error messages for failed requests.
        _ = builder.Services.AddProblemDetails();

        // Configure the scoped repositories for dependency injection.
        _ = builder.Services.AddScoped<IAttachmentRepository, AttachmentRepository>();
        _ = builder.Services.AddScoped<IEditRepository, EditRepository>();
        _ = builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
        _ = builder.Services.AddScoped<IPlatformRepository, PlatformRepository>();
        _ = builder.Services.AddScoped<IReplyRepository, ReplyRepository>();
        _ = builder.Services.AddScoped<ITicketRepository, TicketRepository>();
        _ = builder.Services.AddScoped<IUserRepository, UserRepository>();
        _ = builder.Services.AddScoped<IUserPlatformRepository, UserPlatformRepository>();
        _ = builder.Services.AddScoped<IUserNotificationRepository, UserNotificationRepository>();
        _ = builder.Services.AddScoped<NotificationsController>();
        _ = builder.Services.AddScoped<EditsController>();
        _ = builder.Services.AddScoped<AzureDevOpsController>();

        // Add automapping of entities.
        _ = builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

        // Add Redis cache and data protection persistance layers
        _ = builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString(Service.Cache);
        });

        if (!builder.Environment.IsStaging())
            _ = builder.Services.AddDataProtection()
                .PersistKeysToStackExchangeRedis(
                    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString(Service.Cache)!),
                    "ApiService-DataProtection-Keys");

        // Add real-time communication with SignalR.
        builder.Services.AddSignalR();
    }

    /// <summary>
    ///     Configures the application.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication" /> with the created services.</param>
    public static async Task ConfigureApplicationAsync(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // Apply migrations on startup if the app is in development
            var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ADAticketsDbContext>();
            await db.Database.MigrateAsync();

            // Map endpoint to access the API documentation.
            _ = app.UseSwagger(options => { options.RouteTemplate = "/openapi/{documentName}.json"; });

            // Map endpoint to access the API documentation via Scalar.
            _ = app.MapScalarApiReference(options => { options.Theme = ScalarTheme.BluePlanet; });

            // Show detailed exception screen during development.
            _ = app.UseDeveloperExceptionPage();
        }

        // Create an exception handler for APIs.
        _ = app.UseExceptionHandler();

        // Configure interceptor for 4xx and 5xx errors.
        _ = app.UseStatusCodePages();

        // Add authentication middleware.
        _ = app.UseAuthentication();

        // Add authorization middleware.
        _ = app.UseAuthorization();

        // Enable serving static files.
        _ = app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory()),
            RequestPath = ""
        });

        // Map the controllers endpoints for business logic APIs.
        _ = app.MapControllers();

        // Map hubs endpoints.
        app.MapHub<TicketsHub>("/ticketsHub");
        app.MapHub<NotificationsHub>("/notificationsHub");
    }

    private static void CreatePolicies(AuthorizationBuilder authorizationBuilder, ConfigurationManager configuration)
    {
        _ = authorizationBuilder.AddDefaultPolicy(Policy.AdminOnly, policy =>
            {
                _ = policy.RequireAuthenticatedUser()
                    .RequireAssertion(context =>
                    {
                        return context.User.IsDevOpsAdmin() &&
                               context.User.GetTenantId() == configuration["Entra:TenantId"];
                    })
                    .AddAuthenticationSchemes(Scheme.OpenIdConnectDefault, Scheme.ExternalOpenIdConnectDefault);
            })
            .AddPolicy(Policy.UserOnly, policy =>
            {
                _ = policy.RequireAuthenticatedUser()
                    .RequireAssertion(context =>
                    {
                        return context.User.GetTenantId() == configuration["ExternalEntra:TenantId"];
                    })
                    .AddAuthenticationSchemes(Scheme.OpenIdConnectDefault, Scheme.ExternalOpenIdConnectDefault);
            })
            .AddPolicy(Policy.OperatorOrAdmin, policy =>
            {
                _ = policy.RequireAuthenticatedUser()
                    .RequireAssertion(context =>
                    {
                        return context.User.GetTenantId() == configuration["Entra:TenantId"];
                    })
                    .AddAuthenticationSchemes(Scheme.OpenIdConnectDefault, Scheme.ExternalOpenIdConnectDefault);
            })
            .AddPolicy(Policy.Everyone, policy =>
            {
                _ = policy.RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(Scheme.OpenIdConnectDefault, Scheme.ExternalOpenIdConnectDefault);
            });
    }
}