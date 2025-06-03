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
using ADAtickets.Installer.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ADAtickets.Installer.Views;

partial class LastStep : UserControl
{
    public LastStep()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _ = ExecuteSetup();
    }

    static private bool IsWindows() => Environment.OSVersion.Platform == PlatformID.Win32NT;

    private const string WINDOWS_DOCKERSERVER = "npipe://./pipe/docker_engine";
    private const string LINUX_DOCKERSERVER = "unix:///var/run/docker.sock";

    private async Task ExecuteSetup()
    {
        // Get the ViewModel from DataContext
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.PhaseText = Assets.Resources.EnvFileWriting;

            var tempEnvPath = await Task.Run(() => WriteToEnvFileAsync(viewModel));
            await Task.Delay(3000);

            viewModel.ProgressBarValue = 10;

            // Create a Docker client from the correct URI based on the OS
            using DockerClient client = new DockerClientConfiguration(
            IsWindows() ?
                new Uri(WINDOWS_DOCKERSERVER) :
                new Uri(LINUX_DOCKERSERVER)
            ).CreateClient();

            await Task.Run(() => PullDbContainerAsync(viewModel, client));
            await Task.Delay(3000);

            viewModel.ProgressBarValue = 30;

            await Task.Run(() => PullAPIAsync(viewModel, client));
            await Task.Delay(3000);

            viewModel.ProgressBarValue = 50;

            await Task.Run(() => PullWebAppAsync(viewModel, client));
            await Task.Delay(3000);

            viewModel.ProgressBarValue = 70;

            var tempComposePaths = await Task.Run(() => RunComposeAsync(viewModel, client));
            await Task.Delay(3000);

            viewModel.ProgressBarValue = 90;

            CleanTempFiles(viewModel, tempEnvPath, tempComposePaths);
            await Task.Delay(3000);

            viewModel.ProgressBarValue = 100;
        }
    }

    private static async Task<string> WriteToEnvFileAsync(MainViewModel viewModel)
    {
        // Get assembly containing the embedded resource
        var assembly = Assembly.GetExecutingAssembly();
        string resourcePath = "ADAtickets.Installer.Assets.example.env";

        // Read the template
        using Stream? stream = assembly.GetManifestResourceStream(resourcePath) ??
            throw new IOException($"Could not find embedded resource: {resourcePath}");
        using StreamReader reader = new(stream);
        string templateContent = await reader.ReadToEndAsync();

        // Replace variables with ViewModel data
        // Database configuration
        string fileContent = Regex.Replace(templateContent, "^POSTGRESUSER=.*$", $"POSTGRESUSER={viewModel.DbUserName}");
        fileContent = Regex.Replace(fileContent, "^POSTGRESPASSWORD=.*$", $"POSTGRESPASSWORD={viewModel.DbPassword}");

        // SSL certificate configuration
        fileContent = Regex.Replace(fileContent, "^SSLCERTIFICATEDISKPATH=.*$", $"SSLCERTIFICATEDISKPATH={viewModel.SslCertificatePath}");
        fileContent = Regex.Replace(fileContent, "^SSLCERTIFICATENAME=.*$", $"SSLCERTIFICATENAME={Path.GetFileName(viewModel.SslCertificatePath)}");
        fileContent = Regex.Replace(fileContent, "^SSLCERTIFICATEPASSWORD=.*$", $"SSLCERTIFICATEPASSWORD={viewModel.SslCertificatePassword}");

        // App tag
        fileContent = Regex.Replace(fileContent, "^APITAG=.*$", $"APITAG={viewModel.ApiVersion}");
        fileContent = Regex.Replace(fileContent, "^CLIENTTAG=.*$", $"CLIENTTAG={viewModel.WebVersion}");

        // Entra ID configuration (API)
        fileContent = Regex.Replace(fileContent, "^TENANTID=.*$", $"TENANTID={viewModel.TenantId}");
        fileContent = Regex.Replace(fileContent, "^APIAPPID=.*$", $"APIAPPID={viewModel.ApiId}");

        // External Entra ID configuration (API)
        fileContent = Regex.Replace(fileContent, "^EXTERNALTENANTID=.*$", $"EXTERNALTENANTID={viewModel.ExternalTenantId}");
        fileContent = Regex.Replace(fileContent, "^EXTERNALAPIAPPID=.*$", $"EXTERNALAPIAPPID={viewModel.ExternalApiId}");

        // Entra ID configuration (web app)
        fileContent = Regex.Replace(fileContent, "^CLIENTAPPID=.*$", $"CLIENTAPPID={viewModel.ClientId}");
        fileContent = Regex.Replace(fileContent, "^TENANTNAME=.*$", $"TENANTNAME={viewModel.TenantDomain![..viewModel.TenantDomain!.IndexOf('.')]}");

        // External Entra ID configuration (web app)
        fileContent = Regex.Replace(fileContent, "^EXTERNALCLIENTAPPID=.*$", $"EXTERNALCLIENTAPPID={viewModel.ExternalClientId}");
        fileContent = Regex.Replace(fileContent, "^EXTERNALTENANTNAME=.*$", $"EXTERNALTENANTNAME={viewModel.ExternalTenantDomain![..viewModel.ExternalTenantDomain!.IndexOf('.')]}");

        // Entra global configuration (web app)
        fileContent = Regex.Replace(fileContent, "^AUTHCERTIFICATEDISKPATH=.*$", $"AUTHCERTIFICATEDISKPATH={viewModel.AuthCertificatePath}");
        fileContent = Regex.Replace(fileContent, "^AUTHCERTIFICATENAME=.*$", $"AUTHCERTIFICATENAME={Path.GetFileName(viewModel.AuthCertificatePath)}");
        fileContent = Regex.Replace(fileContent, "^AUTHCERTIFICATEPASSWORD=.*$", $"AUTHCERTIFICATEPASSWORD={viewModel.AuthCertificatePassword}");

        // Write to temporary location
        string tempEnvPath = Path.Combine(Path.GetTempPath(), ".env");
        await File.WriteAllTextAsync(tempEnvPath, fileContent);

        return tempEnvPath;
    }

    private static async Task PullDbContainerAsync(MainViewModel viewModel, DockerClient client)
    {
        while (true)
        {
            try
            {
                Dispatcher.UIThread.Post(() => viewModel.PhaseText = $"{Assets.Resources.DbContainerPull}");

                // Pull the PostgreSQL image
                await client.Images.CreateImageAsync(
                    new ImagesCreateParameters
                    {
                        FromImage = "postgres",
                        Tag = "latest"
                    },
                    null,
                    new Progress<JSONMessage>()
                );

                return;
            }
            catch (TimeoutException)
            {
                // If Docker was closed, wait for it to be reopened
                Dispatcher.UIThread.Post(() => viewModel.PhaseText = $"{Assets.Resources.WaitingDocker}");

                await WaitForDockerAsync(viewModel, client);
            }
        }
    }

    private static async Task PullAPIAsync(MainViewModel viewModel, DockerClient client)
    {
        while (true)
        {
            try
            {
                Dispatcher.UIThread.Post(() => viewModel.PhaseText = $"{Assets.Resources.ApiContainerPull}");

                // Pull the API image
                await client.Images.CreateImageAsync(
                    new ImagesCreateParameters
                    {
                        FromImage = "ghcr.io/andrexace/adatickets-api",
                        Tag = viewModel.ApiVersion
                    },
                    null,
                    new Progress<JSONMessage>()
                );

                return;
            }
            catch (TimeoutException)
            {
                // If Docker was closed, wait for it to be reopened
                Dispatcher.UIThread.Post(() => viewModel.PhaseText = $"{Assets.Resources.WaitingDocker}");

                await WaitForDockerAsync(viewModel, client);
            }
        }
    }

    private static async Task PullWebAppAsync(MainViewModel viewModel, DockerClient client)
    {
        while (true)
        {
            try
            {
                Dispatcher.UIThread.Post(() => viewModel.PhaseText = $"{Assets.Resources.WebContainerPull}");

                // Pull the web image
                await client.Images.CreateImageAsync(
                    new ImagesCreateParameters
                    {
                        FromImage = "ghcr.io/andrexace/adatickets-web",
                        Tag = viewModel.WebVersion
                    },
                    null,
                    new Progress<JSONMessage>()
                );

                return;
            }
            catch (TimeoutException)
            {
                // If Docker was closed, wait for it to be reopened
                Dispatcher.UIThread.Post(() => viewModel.PhaseText = $"{Assets.Resources.WaitingDocker}");

                await WaitForDockerAsync(viewModel, client);
            }
        }
    }

    private static async Task<(string, string)> RunComposeAsync(MainViewModel viewModel, DockerClient client)
    {
        while (true)
        {
            try
            {
                Dispatcher.UIThread.Post(() => viewModel.PhaseText = $"{Assets.Resources.ComposeStartup}");

                // Get assembly containing the embedded resource
                var assembly = Assembly.GetExecutingAssembly();
                string composePath = "ADAtickets.Installer.Assets.docker-compose.yml";
                string overridePath = "ADAtickets.Installer.Assets.docker-compose.override-prod.yml";

                // Read the template
                using Stream? composeStream = assembly.GetManifestResourceStream(composePath) ??
                    throw new IOException($"Could not find embedded resource: {composePath}");
                using StreamReader composeReader = new(composeStream);
                string composeContent = await composeReader.ReadToEndAsync();

                using Stream? overrideStream = assembly.GetManifestResourceStream(overridePath) ??
                    throw new IOException($"Could not find embedded resource: {overridePath}");
                using StreamReader reader = new(overrideStream);
                string overrideContent = await reader.ReadToEndAsync();

                // Write to temporary location
                string tempComposePath = Path.Combine(Path.GetTempPath(), "docker-compose.yml");
                await File.WriteAllTextAsync(tempComposePath, composeContent);

                string tempOverridePath = Path.Combine(Path.GetTempPath(), "docker-compose.override-prod.yml");
                await File.WriteAllTextAsync(tempOverridePath, overrideContent);

                // Run the docker-compose command
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "docker",
                        Arguments = "compose -f docker-compose.yml -f docker-compose.override-prod.yml up -d",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = false,
                        WorkingDirectory = Path.GetTempPath()
                    }
                };
                process.Start();
                await process.WaitForExitAsync();

                return (tempComposePath, tempOverridePath);
            }
            catch (TimeoutException)
            {
                // If Docker was closed, wait for it to be reopened
                Dispatcher.UIThread.Post(() => viewModel.PhaseText = $"{Assets.Resources.WaitingDocker}");

                await WaitForDockerAsync(viewModel, client);
            }
        }
    }

    private static async Task WaitForDockerAsync(MainViewModel viewModel, DockerClient client)
    {
        while (true)
        {
            try
            {
                // Ping the Docker server to check if it's available
                await client.System.PingAsync();

                return;
            }
            catch (TimeoutException)
            {
                // If it's unavailable, update the message and wait for it to be ready
                await Task.Delay(5000);

                Dispatcher.UIThread.Post(() => viewModel.PhaseText = $"{Assets.Resources.StillWaitingDocker}");
            }
        }
    }

    private static void CleanTempFiles(MainViewModel viewModel, string tempEnvPath, (string, string) tempComposePaths)
    {
        viewModel.PhaseText = $"{Assets.Resources.CleaningTemp}";

        // Delete the temporary .env file
        if (File.Exists(tempEnvPath))
        {
            File.Delete(tempEnvPath);
        }

        // Delete the temporary docker-compose files
        if (File.Exists(tempComposePaths.Item1))
        {
            File.Delete(tempComposePaths.Item1);
        }
        if (File.Exists(tempComposePaths.Item2))
        {
            File.Delete(tempComposePaths.Item2);
        }
    }
}
