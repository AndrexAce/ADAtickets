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
using ADAtickets.Web.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

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
            // Authentication and Authorization
            var authBuilder = builder.Services.AddAuthentication();
            authBuilder.AddMicrosoftIdentityWebApp(builder.Configuration, "Entra", "Entra", "EntraCookie")
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddDownstreamApi("ADAticketsAPI", builder.Configuration.GetSection("ADAticketsAPI"))
                .AddDistributedTokenCaches();
            authBuilder.AddMicrosoftIdentityWebApp(builder.Configuration, "ExternalEntra", "ExternalEntra", "ExternalEntraCookie")
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddDownstreamApi("ADAticketsAPI", builder.Configuration.GetSection("ADAticketsAPI"))
                .AddDistributedTokenCaches();

            builder.Services.AddAuthorization();

            builder.Services.AddCascadingAuthenticationState();

            // Basic services
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddDistributedMemoryCache();

            // Localization
            builder.Services.AddLocalization();

            // UI Components
            builder.Services.AddFluentUIComponents();
            builder.Services.AddDataGridEntityFrameworkAdapter();

            // Razor services
            builder.Services.AddRazorPages()
                .AddMicrosoftIdentityUI();

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddMicrosoftIdentityConsentHandler();
        }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app">The <see cref="WebApplication"/> with the created services.</param>
        public static void ConfigureApplication(WebApplication app)
        {
            // Configure localization
            var supportedCultures = new[] { "en-US", "it-IT" };
            var localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);
            app.UseRequestLocalization(localizationOptions);

            // Configure security based on environment
            if (!app.Environment.IsDevelopment())
            {
                // Enable production error handling and HTTPS requirements
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // Configure middleware pipeline
            app.UseHttpsRedirection();
            app.UseAntiforgery();

            // Authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Configure routing
            app.MapStaticAssets();
            app.MapControllers();
            app.MapRazorPages();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
