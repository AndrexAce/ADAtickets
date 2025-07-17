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
using ADAtickets.Client.Extensions;
using ADAtickets.Shared.Constants;
using ADAtickets.Web.Components;
using ADAtickets.Web.Components.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.Net.Http.Headers;
using StackExchange.Redis;

namespace ADAtickets.Web
{
    /// <summary>
    /// Bootstrap class for the application.
    /// </summary>
    static class Program
    {
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
            ConfigureApplication(app);

            // Start the application.
            await app.RunAsync();
        }

        /// <summary>
        /// Configures the services used by the application.
        /// </summary>
        /// <param name="builder">The <see cref="WebApplicationBuilder"/> that creates the required services.</param>
        public static void ConfigureServices(WebApplicationBuilder builder)
        {
            // Add authentication and authorization services for both Entra ID and Entra External ID.
            var authBuilder = builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = Scheme.PolicySchemeDefault;
            });
            authBuilder.AddMicrosoftIdentityWebApp(builder.Configuration.GetSection(Scheme.OpenIdConnectDefault), Scheme.OpenIdConnectDefault, Scheme.CookieDefault)
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddDownstreamApi(Service.API, builder.Configuration.GetSection(Service.API))
                .AddDownstreamApi(Service.Graph, builder.Configuration.GetSection(Service.Graph))
                .AddDistributedTokenCaches();
            authBuilder.AddMicrosoftIdentityWebApp(builder.Configuration.GetSection(Scheme.ExternalOpenIdConnectDefault), Scheme.ExternalOpenIdConnectDefault, Scheme.ExternalCookieDefault)
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddDownstreamApi(Service.ExternalAPI, builder.Configuration.GetSection(Service.ExternalAPI))
                .AddDownstreamApi(Service.ExternalGraph, builder.Configuration.GetSection(Service.Graph))
                .AddDistributedTokenCaches();
            authBuilder.AddPolicyScheme(Scheme.PolicySchemeDefault, null, options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    string? authorization = context.Request.Headers[HeaderNames.Cookie];

                    if (authorization is not null)
                        return authorization.Contains(Scheme.ExternalCookieDefault) ? Scheme.ExternalOpenIdConnectDefault : Scheme.OpenIdConnectDefault;
                    else
                        return Scheme.ExternalOpenIdConnectDefault;
                };
            });

            // Add Redis cache and data protection persistance layers
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString(Service.Cache);
            });

            builder.Services.AddDataProtection()
                .PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString(Service.Cache)!), "DataProtection-Keys");

            builder.Services.AddCascadingAuthenticationState();

            // Add authorization policies.
            CreatePolicies(builder.Services.AddAuthorizationBuilder(), builder.Configuration);

            // Add access to the HTTP context.
            builder.Services.AddHttpContextAccessor();

            // Add localization services.
            builder.Services.AddLocalization();

            // Add access to the HTTP client class.
            builder.Services.AddHttpClient();

            // Add Fluent UI components.
            builder.Services.AddFluentUIComponents();

            // Add services related to Razor pages and components.
            builder.Services.AddRazorPages()
                .AddMicrosoftIdentityUI();

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddMicrosoftIdentityConsentHandler();

            // Add ADAtickets client services.
            builder.Services.AddADAticketsClient();

            // Add automapping of entities.
            builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
        }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app">The <see cref="WebApplication"/> with the created services.</param>
        public static void ConfigureApplication(WebApplication app)
        {
            // Configure localization.
            var supportedCultures = new[] { "en-US", "it-IT" };
            var localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);
            app.UseRequestLocalization(localizationOptions);

            // Configure security based on environment.
            if (app.Environment.IsDevelopment())
            {
                // Use a detailed exception page.
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Use a more user-friendly error page.
                app.UseExceptionHandler("/error");

                // Add HTTPS redirection for production.
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            // Allows personalized status code pages.
            app.UseStatusCodePagesWithRedirects("/error/{0}");

            // Add authentication middleware.
            app.UseAuthentication();

            // Add authorization middleware.
            app.UseAuthorization();

            // Configure antiforgery protection.
            app.UseAntiforgery();

            // Map static data paths.
            app.MapStaticAssets();

            // Map the controllers endpoints.
            app.MapControllers();

            // Map Razor pages paths.
            app.MapRazorPages();

            // Map Razor components paths.
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();
        }

        private static void CreatePolicies(AuthorizationBuilder authorizationBuilder, ConfigurationManager configuration)
        {
            authorizationBuilder.AddDefaultPolicy(Policy.AdminOnly, policy =>
            {
                policy.RequireAuthenticatedUser()
                // Directory roles are exposed with the "wids" claim in the ID token.
                // The value of this claim is the standard ID for the Azure DevOps Administrator Entra directory role.
                .RequireClaim("wids", "e3973bdf-4987-49ae-837a-ba8e231c7286")
                .RequireClaim("utid", configuration["Entra:TenantId"]!)
                .AddAuthenticationSchemes(Scheme.OpenIdConnectDefault, Scheme.ExternalOpenIdConnectDefault);
            })
            .AddPolicy(Policy.UserOnly, policy =>
            {
                policy.RequireAuthenticatedUser()
                .RequireClaim("utid", configuration["ExternalEntra:TenantId"]!)
                .AddAuthenticationSchemes(Scheme.OpenIdConnectDefault, Scheme.ExternalOpenIdConnectDefault);
            })
            .AddPolicy(Policy.OperatorOrAdmin, policy =>
            {
                policy.RequireAuthenticatedUser()
                .RequireClaim("utid", configuration["Entra:TenantId"]!)
                .AddAuthenticationSchemes(Scheme.OpenIdConnectDefault, Scheme.ExternalOpenIdConnectDefault);
            })
            .AddPolicy(Policy.Everyone, policy =>
            {
                policy.RequireAuthenticatedUser()
                .AddAuthenticationSchemes(Scheme.OpenIdConnectDefault, Scheme.ExternalOpenIdConnectDefault);
            });
        }
    }
}