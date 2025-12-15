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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace ADAtickets.ServiceDefaults;

/// <summary>
///     Defines common extensions to configure <see cref="IHostApplicationBuilder" /> Aspire services.
/// </summary>
public static class HostApplicationBuilderExtensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Adds common service defaults to the application builder.
        /// </summary>
        /// <returns>The <see cref="IHostApplicationBuilder" />.</returns>
        public IHostApplicationBuilder AddServiceDefaults()
        {
            _ = builder.ConfigureOpenTelemetry();

            _ = builder.AddDefaultHealthChecks();

            _ = builder.Services.AddServiceDiscovery();

            builder.Services.ConfigureHttpClientDefaults(static http =>
            {
                // Turn on resilience by default
                http.AddStandardResilienceHandler();

                // Turn on service discovery by default
                http.AddServiceDiscovery();
            });

            return builder;
        }

        private IHostApplicationBuilder ConfigureOpenTelemetry()
        {
            builder.Logging.AddOpenTelemetry(static logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });

            builder.Services.AddOpenTelemetry()
                .WithMetrics(static metrics =>
                {
                    metrics.AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation();
                })
                .WithTracing(tracing =>
                {
                    tracing.AddSource(builder.Environment.ApplicationName)
                        .AddAspNetCoreInstrumentation(static options =>
                            // Exclude health check requests from tracing
                        {
                            options.Filter = static context =>
                                !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                                && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath);
                        })
                        .AddHttpClientInstrumentation();
                });

            _ = builder.AddOpenTelemetryExporters();

            return builder;
        }

        private IHostApplicationBuilder AddOpenTelemetryExporters()
        {
            var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

            if (useOtlpExporter) builder.Services.AddOpenTelemetry().UseOtlpExporter();

            return builder;
        }

        private IHostApplicationBuilder AddDefaultHealthChecks()
        {
            builder.Services.AddHealthChecks()
                // Add a default liveness check to ensure the app is responsive
                .AddCheck("self", static () => HealthCheckResult.Healthy(), ["live"]);

            return builder;
        }
    }
}