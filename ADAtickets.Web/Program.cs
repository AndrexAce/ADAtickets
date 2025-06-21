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
using ADAtickets.Client.Extensions;
using ADAtickets.Shared.Constants;
using ADAtickets.Web.Components;
using Microsoft.AspNetCore.Authorization;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.Net.Http.Headers;

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
                options.DefaultChallengeScheme = Scheme.PolicySchemeDefault;
            });
            authBuilder.AddMicrosoftIdentityWebApp(builder.Configuration.GetSection(Scheme.OpenIdConnectDefault), Scheme.OpenIdConnectDefault, Scheme.CookieDefault)
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddDownstreamApi(Service.API, builder.Configuration.GetSection(Service.API))
                .AddDistributedTokenCaches();
            authBuilder.AddMicrosoftIdentityWebApp(builder.Configuration.GetSection(Scheme.ExternalOpenIdConnectDefault), Scheme.ExternalOpenIdConnectDefault, Scheme.ExternalCookieDefault)
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddDownstreamApi(Service.ExternalAPI, builder.Configuration.GetSection(Service.ExternalAPI))
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

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString(Service.Cache);
            });

            builder.Services.AddCascadingAuthenticationState();

            // Add authorization policies.
            CreatePolicies(builder.Services.AddAuthorizationBuilder());

            // Add access to the HTTP context.
            builder.Services.AddHttpContextAccessor();

            // Add localization services.
            builder.Services.AddLocalization();

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
                // Enable production error handling and HTTPS requirements
                app.UseExceptionHandler("/Error");
            }
            else
            {
                // Add HTTPS redirection for production.
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            // Add authentication middleware.
            app.UseAuthentication();

            // Add authorization middleware.
            app.UseAuthorization();

            // Configure antiforgery protection.
            app.UseAntiforgery();

            // Map static data paths.
            app.MapStaticAssets();

            // Map the controllers endpoints for business logic APIs.
            app.MapControllers();

            // Map Razor pages paths.
            app.MapRazorPages();

            // Map Razor components paths.
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();
        }

        private static void CreatePolicies(AuthorizationBuilder builder)
        {
            builder.AddDefaultPolicy(Policy.AdminOnly, policy =>
            {
                policy.RequireAuthenticatedUser()
                .RequireRole("Azure DevOps Administrator")
                .AddAuthenticationSchemes(Scheme.OpenIdConnectDefault);

            })
            .AddPolicy(Policy.OperatorOrAdmin, policy =>
            {
                policy.RequireAuthenticatedUser()
                .AddAuthenticationSchemes(Scheme.OpenIdConnectDefault);
            })
            .AddPolicy(Policy.Everyone, policy =>
            {
                policy.RequireAuthenticatedUser()
                .AddAuthenticationSchemes(Scheme.OpenIdConnectDefault, Scheme.ExternalOpenIdConnectDefault);
            });
        }
    }
}