/*
 * ADAtickets is a simple, lightweight, open source ticketing system
 * interacting with your enterprise's repositories on Azure DevOps 
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
using ADAtickets.ApiService.Models;
using ADAtickets.ApiService.Repositories;
using ADAtickets.ApiService.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
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
    public class Program
    {
        /// <summary>
        /// Creates a new instance of the Program class.
        /// </summary>
        protected Program() { }

        /// <summary>
        /// Entrypoint of the application.
        /// </summary>
        /// <param name="args">Additional arguments passed by command line.</param>
        /// <returns>The <see cref="Task"/> running the application.</returns>
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigureServices(builder);

            var app = builder.Build();
            await ConfigureApplication(app);

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
            // The context is pooled so that the application spends less time creating and destroying contexts.
            builder.Services.AddDbContextPool<ADAticketsDbContext>(options =>
            {
                // Configure the DBContext to use PostgreSQL.
                options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"), options =>
                {
                    // Create the enumerations in the connected database.
                    options.MapEnum<Priority>("priority")
                        .MapEnum<Status>("status")
                        .MapEnum<TicketType>("ticket_type")
                        .MapEnum<UserType>("user_type")
                        .EnableRetryOnFailure();
                })
                    .UseSnakeCaseNamingConvention();
            });

            // Configure EntraID authentication for Microsoft work accounts.
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"), "AzureAd");

            // Configure local authentication with Identity.
            builder.Services.AddIdentityApiEndpoints<IdentityUser<Guid>>(options =>
            {
                options.Password.RequiredLength = 8;

                options.SignIn.RequireConfirmedAccount = true;
                options.SignIn.RequireConfirmedEmail = true;

                options.User.RequireUniqueEmail = true;
            })
                .AddEntityFrameworkStores<ADAticketsDbContext>();

            // Configure the authorization cookie.
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/denied";
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;

                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = "ADAtickets";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

            // Configure the antiforgery token to prevent Cross-Site Request Forgery (CSRF) attacks.
            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-XSRF-TOKEN";
                options.FormFieldName = "__RequestVerificationToken";

                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = "ADAtickets-Xsrf";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

            const string identityScheme = "Identity.Application";
            const string azureAdScheme = "AzureAd";

            // Configure the authorization.
            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("AuthenticatedUserOnly", policy =>
                {
                    policy.RequireRole(nameof(UserType.User))
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(identityScheme, azureAdScheme);
                })
                .AddPolicy("AuthenticatedOperatorOnly", policy =>
                {
                    policy.RequireRole(nameof(UserType.Operator))
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(identityScheme, azureAdScheme);
                })
                .AddPolicy("AuthenticatedAdminOnly", policy =>
                {
                    policy.RequireRole(nameof(UserType.Admin))
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(identityScheme, azureAdScheme);
                })
                .AddPolicy("AuthenticatedOperator", policy =>
                {
                    policy.RequireRole(nameof(UserType.Operator), nameof(UserType.Admin))
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(identityScheme, azureAdScheme);
                })
                .AddPolicy("AuthenticatedEveryone", policy =>
                {
                    policy.RequireRole(nameof(UserType.User), nameof(UserType.Operator), nameof(UserType.Admin))
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(identityScheme, azureAdScheme);
                })
                .AddDefaultPolicy("Unauthenticated", policy => policy.RequireAssertion(_ => true));

            // Add services commonly used with controllers APIs.
            builder.Services
                .AddControllers(options =>// Require the APIs to respect the broswer request media type, and return a 406 Not Acceptable response if the media type is not supported.
                {
                    options.RespectBrowserAcceptHeader = true;
                    options.ReturnHttpNotAcceptable = true;
                })
                .ConfigureApiBehaviorOptions(options => // Configure the API behavior options to return either a JSON or XML 400 Bad Request response when the model state is invalid.
                {
                    options.InvalidModelStateResponseFactory = context =>
                        new BadRequestObjectResult(context.ModelState)
                        {
                            ContentTypes = { MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml }
                        };
                })
                .AddXmlSerializerFormatters() // Add XML serialization support for the APIs.
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())); // Configure enumerations serialization support.

            // Add Swagger documentation for the APIs.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "ADAtickets API",
                    Description = "Web MCV APIs to interact with the ADAtickets ticketing system.",
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

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

            // Add services used to return detailed error messages for failed requests.
            builder.Services.AddProblemDetails();

            // Configure the scoped (one per request) classes available for dependency injection.
            builder.Services.AddScoped<IEditRepository, EditRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<IPlatformRepository, PlatformRepository>();
            builder.Services.AddScoped<IReplyRepository, ReplyRepository>();
            builder.Services.AddScoped<ITicketRepository, TicketRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            // Add automapping of entities.
            builder.Services.AddAutoMapper(typeof(ADAticketsProfile));
        }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app">The <see cref="WebApplication"/> with the created services.</param>
        public static async Task ConfigureApplication(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                // Apply migrations on startup if the app is in development to ensure the database is up to date.
                var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ADAticketsDbContext>();
                await db.Database.MigrateAsync();

                // Map an endpoint to access the API documentation.
                app.UseSwagger(options =>
                {
                    options.RouteTemplate = "/openapi/{documentName}.json";
                });

                // Map an endpoint to access the API documentation via Scalar.
                app.MapScalarApiReference(options =>
                {
                    options.Theme = ScalarTheme.BluePlanet;
                });

                // When exceptions happen dunring development, show a detailed screen.
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Add the redirection from HTTP to HTTPS.
                app.UseHttpsRedirection();
            }

            // Create an exception handler to handle exceptions in APIs.
            app.UseExceptionHandler();

            // Configure an interceptor for 4xx and 5xx errors to return a JSON response with the error details.
            app.UseStatusCodePages();

            // Add the authentication middleware.
            app.UseAuthentication();

            // Add the authentication middleware.
            app.UseAuthorization();

            // Add the antiforgery middleware.
            app.UseAntiforgery();

            // Map the authentication endpoints.
            app.MapIdentityApi<IdentityUser<Guid>>();

            // Map the controllers endpoints.
            app.MapControllers();
        }
    }
}