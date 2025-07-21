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
using ADAtickets.ApiService.Repositories;
using ADAtickets.ApiService.Services;
using ADAtickets.Shared.Constants;
using ADAtickets.Shared.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json.Serialization;

namespace ADAtickets.ApiService
{
    /// <summary>
    /// Bootstrap class for the application.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Entrypoint of the application.
        /// </summary>
        /// <param name="args">Additional arguments passed by command line.</param>
        /// <returns>The <see cref="Task"/> running the application.</returns>
        public static async Task Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            ConfigureServices(builder);

            WebApplication app = builder.Build();
            await ConfigureApplicationAsync(app);

            // Start the application.
            await app.RunAsync();
        }

        /// <summary>
        /// Configures the services used by the application.
        /// </summary>
        /// <param name="builder">The <see cref="WebApplicationBuilder"/> that creates the required services.</param>
        public static void ConfigureServices(WebApplicationBuilder builder)
        {
            // Add the DBContext to execute queries against the database.
            _ = builder.Services.AddDbContextPool<ADAticketsDbContext>(options =>
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
            AuthenticationBuilder authBuilder = builder.Services.AddAuthentication();
            _ = authBuilder.AddMicrosoftIdentityWebApi(builder.Configuration.GetSection(Scheme.OpenIdConnectDefault), Scheme.OpenIdConnectDefault);
            _ = authBuilder.AddMicrosoftIdentityWebApi(builder.Configuration.GetSection(Scheme.ExternalOpenIdConnectDefault), Scheme.ExternalOpenIdConnectDefault);

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
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            // Add Swagger documentation for the APIs.
            _ = builder.Services.AddEndpointsApiExplorer();
            _ = builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc($"v{Service.APIVersion}", new OpenApiInfo
                {
                    Version = $"v{Service.APIVersion}",
                    Title = "ADAtickets API",
                    Description = "Web API to interact with the ADAtickets ticketing system with authentication endpoints and JWT validation.",
                    Contact = new OpenApiContact
                    {
                        Name = "Andrea Lucchese",
                        Email = "andrylook14@gmail.com"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "GPL v3",
                        Url = new("https://github.com/AndrexAce/ADAtickets/blob/master/LICENSE.txt")
                    }
                });

                string xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
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

            // Add automapping of entities.
            _ = builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
        }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app">The <see cref="WebApplication"/> with the created services.</param>
        public static async Task ConfigureApplicationAsync(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                // Apply migrations on startup if the app is in development
                IServiceScope scope = app.Services.CreateScope();
                ADAticketsDbContext db = scope.ServiceProvider.GetRequiredService<ADAticketsDbContext>();
                await db.Database.MigrateAsync();

                // Map endpoint to access the API documentation.
                _ = app.UseSwagger(options =>
                {
                    options.RouteTemplate = "/openapi/{documentName}.json";
                });

                // Map endpoint to access the API documentation via Scalar.
                _ = app.MapScalarApiReference(options =>
                {
                    options.Theme = ScalarTheme.BluePlanet;
                });

                // Show detailed exception screen during development.
                _ = app.UseDeveloperExceptionPage();
            }

            // Configure forwarded headers for reverse proxy scenarios
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // Create an exception handler for APIs.
            _ = app.UseExceptionHandler();

            // Configure interceptor for 4xx and 5xx errors.
            _ = app.UseStatusCodePages();

            // Add authentication middleware.
            _ = app.UseAuthentication();

            // Add authorization middleware.
            _ = app.UseAuthorization();

            // Map the controllers endpoints for business logic APIs.
            _ = app.MapControllers();
        }

        private static void CreatePolicies(AuthorizationBuilder authorizationBuilder, ConfigurationManager configuration)
        {
            _ = authorizationBuilder.AddDefaultPolicy(Policy.AdminOnly, policy =>
            {
                _ = policy.RequireAuthenticatedUser()
                // Directory roles are exposed with the "wids" claim in the ID token.
                // The value of this claim is the standard ID for the Azure DevOps Administrator Entra directory role.
                .RequireClaim("wids", "e3973bdf-4987-49ae-837a-ba8e231c7286")
                .RequireClaim("utid", configuration["Entra:TenantId"]!)
                .AddAuthenticationSchemes(Scheme.OpenIdConnectDefault, Scheme.ExternalOpenIdConnectDefault);
            })
            .AddPolicy(Policy.UserOnly, policy =>
            {
                _ = policy.RequireAuthenticatedUser()
                .RequireClaim("utid", configuration["ExternalEntra:TenantId"]!)
                .AddAuthenticationSchemes(Scheme.OpenIdConnectDefault, Scheme.ExternalOpenIdConnectDefault);
            })
            .AddPolicy(Policy.OperatorOrAdmin, policy =>
            {
                _ = policy.RequireAuthenticatedUser()
                .RequireClaim("utid", configuration["Entra:TenantId"]!)
                .AddAuthenticationSchemes(Scheme.OpenIdConnectDefault, Scheme.ExternalOpenIdConnectDefault);
            })
            .AddPolicy(Policy.Everyone, policy =>
            {
                _ = policy.RequireAuthenticatedUser()
                .AddAuthenticationSchemes(Scheme.OpenIdConnectDefault, Scheme.ExternalOpenIdConnectDefault);
            });
        }
    }
}