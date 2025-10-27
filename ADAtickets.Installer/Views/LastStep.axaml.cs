﻿/*
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

using ADAtickets.Installer.ViewModels;
using ADAtickets.Shared.Constants;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform;

namespace ADAtickets.Installer.Views;

internal sealed partial class LastStep : UserControl
{
    private const string WindowsDockerServer = "npipe://./pipe/docker_engine";
    private const string MacOsDockerServer = "unix:///var/run/docker.sock";

    private static readonly string[] LinuxDockerServerCandidates =
    [
        "/var/run/docker.sock",
        $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.docker/desktop/docker.sock",
        $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.docker/desktop/docker-cli.sock"
    ];

    private const string WindowsDockerExe = @"C:\Program Files\Docker\Docker\resources\bin\docker.exe";

    private static readonly string[] MacOsDockerExeCandidates =
    [
        "/usr/local/bin/docker",
        "/opt/homebrew/bin/docker",
        "/Applications/Docker.app/Contents/Resources/bin/docker"
    ];

    private static readonly string[] LinuxDockerExeCandidates =
    [
        "/usr/bin/docker",
        "/bin/docker",
        "/usr/local/bin/docker",
        "/opt/docker-desktop/bin/docker"
    ];

    private static Uri GetDockerServerUri()
    {
        if (OperatingSystem.IsWindows()) return new Uri(WindowsDockerServer);

        return OperatingSystem.IsMacOS()
            ? new Uri(MacOsDockerServer)
            : new Uri("unix://" + LinuxDockerServerCandidates.First(File.Exists));
    }

    private static string GetDockerExePath()
    {
        if (OperatingSystem.IsWindows()) return WindowsDockerExe;

        var candidates = OperatingSystem.IsMacOS() ? MacOsDockerExeCandidates : LinuxDockerExeCandidates;
        return candidates.First(File.Exists);
    }

    public LastStep()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _ = ExecuteSetupAsync();
    }

    private async Task ExecuteSetupAsync()
    {
        // Used to store the path of the temporary folder with the .env and .yml files
        string? tempPath = null;

        // Get the ViewModel from DataContext
        if (DataContext is MainViewModel viewModel)
            try
            {
                viewModel.PhaseText = Assets.Resources.EnvFileWriting;

                tempPath = CreateRandomTempFolder();

                if (viewModel.UploadedEnvFilePath is not null)
                {
                    // If the user provided a custom .env file, copy it to the temporary folder
                    var destinationPath = Path.Combine(tempPath, ".env");
                    File.Copy(viewModel.UploadedEnvFilePath, destinationPath, true);
                }
                else
                {
                    // Otherwise, create a new .env file based on the template and user input
                    await Task.Run(() => WriteToEnvFileAsync(viewModel, tempPath));
                }

                viewModel.ProgressBarValue = 5;

                // Create a Docker client from the correct URI based on the OS
                using var client = new DockerClientConfiguration(GetDockerServerUri()).CreateClient();

                await Task.Run(() => PullDbContainerAsync(viewModel, client));

                viewModel.ProgressBarValue = 25;

                await Task.Run(() => PullCacheContainerAsync(viewModel, client));

                viewModel.ProgressBarValue = 45;

                await Task.Run(() => PullApiAsync(viewModel, client));

                viewModel.ProgressBarValue = 65;

                await Task.Run(() => PullWebAppAsync(viewModel, client));

                viewModel.ProgressBarValue = 85;

                await Task.Run(() => RunComposeAsync(viewModel, client, tempPath));

                viewModel.ProgressBarValue = 95;

                CleanTempFiles(viewModel, tempPath);

                viewModel.ProgressBarValue = 100;

                viewModel.IsLoadingVisible = false;
                viewModel.PhaseText = Assets.Resources.SetupCompleted;
            }
            catch (Exception e)
            {
                CleanTempFiles(viewModel, tempPath);

                viewModel.IsLoadingVisible = false;
                viewModel.PhaseText = Assets.Resources.ErrorOccurred + Environment.NewLine + e.Message;
            }
    }

    private static string CreateRandomTempFolder()
    {
        // Create a temporary folder with a random name
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        _ = OperatingSystem.IsWindows()
            ? Directory.CreateDirectory(tempPath)
            : Directory.CreateDirectory(tempPath,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);

        return tempPath;
    }

    private static async Task WriteToEnvFileAsync(MainViewModel viewModel, string path)
    {
        // Get the resource
        var envFileStream =
            AssetLoader.Open(new Uri($"avares://{Assembly.GetExecutingAssembly().GetName().Name}/Assets/example.env"));
        using StreamReader reader = new(envFileStream);
        var templateContent = await reader.ReadToEndAsync();

        // Replace variables with ViewModel data
        // Database configuration
        var fileContent = Regex.Replace(templateContent, "^POSTGRESUSER=.*$", $"POSTGRESUSER={viewModel.DbUserName}",
            RegexOptions.Compiled | RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));
        fileContent = Regex.Replace(fileContent, "^POSTGRESPASSWORD=.*$", $"POSTGRESPASSWORD={viewModel.DbPassword}",
            RegexOptions.Compiled | RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));

        // SSL certificate configuration
        fileContent = Regex.Replace(fileContent, "^SSLCERTIFICATEDISKPATH=.*$",
            $"SSLCERTIFICATEDISKPATH={Path.GetDirectoryName(viewModel.SslCertificatePath)}",
            RegexOptions.Compiled | RegexOptions.Multiline,
            TimeSpan.FromMilliseconds(100));
        fileContent = Regex.Replace(fileContent, "^SSLCERTIFICATENAME=.*$",
            $"SSLCERTIFICATENAME={Path.GetFileName(viewModel.SslCertificatePath)}",
            RegexOptions.Compiled | RegexOptions.Multiline,
            TimeSpan.FromMilliseconds(100));
        fileContent = Regex.Replace(fileContent, "^SSLCERTIFICATEPASSWORD=.*$",
            $"SSLCERTIFICATEPASSWORD={viewModel.SslCertificatePassword}",
            RegexOptions.Compiled | RegexOptions.Multiline,
            TimeSpan.FromMilliseconds(100));

        // DevOps configuration
        fileContent = Regex.Replace(fileContent, "^DEVOPSORGANIZATIONNAME=.*$",
            $"DEVOPSORGANIZATIONNAME={viewModel.DevOpsOrganizationName}",
            RegexOptions.Compiled | RegexOptions.Multiline,
            TimeSpan.FromMilliseconds(100));

        // Tenant configuration
        fileContent = Regex.Replace(fileContent, "^TENANTID=.*$", $"TENANTID={viewModel.TenantId}",
            RegexOptions.Compiled | RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));
        fileContent = Regex.Replace(fileContent, "^EXTERNALTENANTID=.*$",
            $"EXTERNALTENANTID={viewModel.ExternalTenantId}", RegexOptions.Compiled | RegexOptions.Multiline,
            TimeSpan.FromMilliseconds(100));

        // Entra ID configuration (API)
        fileContent = Regex.Replace(fileContent, "^APIAPPID=.*$", $"APIAPPID={viewModel.ApiAppId}",
            RegexOptions.Compiled | RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));

        // External Entra ID configuration (API)
        fileContent = Regex.Replace(fileContent, "^EXTERNALAPIAPPID=.*$",
            $"EXTERNALAPIAPPID={viewModel.ExternalApiAppId}", RegexOptions.Compiled | RegexOptions.Multiline,
            TimeSpan.FromMilliseconds(100));

        // Entra ID configuration (web app)
        fileContent = Regex.Replace(fileContent, "^WEBAPPID=.*$", $"WEBAPPID={viewModel.WebAppId}",
            RegexOptions.Compiled | RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));

        // External Entra ID configuration (web app)
        fileContent = Regex.Replace(fileContent, "^EXTERNALWEBAPPID=.*$",
            $"EXTERNALWEBAPPID={viewModel.ExternalWebAppId}", RegexOptions.Compiled | RegexOptions.Multiline,
            TimeSpan.FromMilliseconds(100));

        // Authentication configuration (API)
        fileContent = Regex.Replace(fileContent, "^APIAUTHCERTIFICATEDISKPATH=.*$",
            $"APIAUTHCERTIFICATEDISKPATH={Path.GetDirectoryName(viewModel.ApiAuthCertificatePath)}",
            RegexOptions.Compiled | RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));
        fileContent = Regex.Replace(fileContent, "^APIAUTHCERTIFICATENAME=.*$",
            $"APIAUTHCERTIFICATENAME={Path.GetFileName(viewModel.ApiAuthCertificatePath)}",
            RegexOptions.Compiled | RegexOptions.Multiline,
            TimeSpan.FromMilliseconds(100));
        fileContent = Regex.Replace(fileContent, "^APIAUTHCERTIFICATEPASSWORD=.*$",
            $"APIAUTHCERTIFICATEPASSWORD={viewModel.ApiAuthCertificatePassword}",
            RegexOptions.Compiled | RegexOptions.Multiline,
            TimeSpan.FromMilliseconds(100));

        // Authentication configuration (web app)
        fileContent = Regex.Replace(fileContent, "^WEBAUTHCERTIFICATEDISKPATH=.*$",
            $"WEBAUTHCERTIFICATEDISKPATH={Path.GetDirectoryName(viewModel.WebAuthCertificatePath)}",
            RegexOptions.Compiled | RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));
        fileContent = Regex.Replace(fileContent, "^WEBAUTHCERTIFICATENAME=.*$",
            $"WEBAUTHCERTIFICATENAME={Path.GetFileName(viewModel.WebAuthCertificatePath)}",
            RegexOptions.Compiled | RegexOptions.Multiline,
            TimeSpan.FromMilliseconds(100));
        fileContent = Regex.Replace(fileContent, "^WEBAUTHCERTIFICATEPASSWORD=.*$",
            $"WEBAUTHCERTIFICATEPASSWORD={viewModel.WebAuthCertificatePassword}",
            RegexOptions.Compiled | RegexOptions.Multiline,
            TimeSpan.FromMilliseconds(100));

        // Webhooks configuration (API)
        fileContent = Regex.Replace(fileContent, "^SERVICEPRINCIPALID=.*$",
            $"SERVICEPRINCIPALID={viewModel.ServicePrincipalId}",
            RegexOptions.Compiled | RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));
        fileContent = Regex.Replace(fileContent, "^BASICAUTHUSERNAME=.*$",
            $"BASICAUTHUSERNAME={viewModel.BasicAuthUsername}",
            RegexOptions.Compiled | RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));
        fileContent = Regex.Replace(fileContent, "^BASICAUTHPASSWORD=.*$",
            $"BASICAUTHPASSWORD={viewModel.BasicAuthPassword}",
            RegexOptions.Compiled | RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));

        // Write to the temporary location
        var tempEnvPath = Path.Combine(path, ".env");
        await File.WriteAllTextAsync(tempEnvPath, fileContent);
    }

    private static async Task PullCacheContainerAsync(MainViewModel viewModel, DockerClient client)
    {
        while (true)
            try
            {
                Dispatcher.UIThread.Post(() => { viewModel.PhaseText = $"{Assets.Resources.CacheContainerPull}"; });

                // Pull the Redis image
                await client.Images.CreateImageAsync(
                    new ImagesCreateParameters
                    {
                        FromImage = "redis",
                        Tag = Service.CacheVersion
                    },
                    null,
                    new Progress<JSONMessage>(),
                    new CancellationTokenSource(TimeSpan.FromMinutes(3)).Token
                );

                return;
            }
            catch (Exception exception) when (exception is TimeoutException or HttpRequestException)
            {
                // If Docker was closed, wait for it to be reopened
                Dispatcher.UIThread.Post(() => { viewModel.PhaseText = $"{Assets.Resources.WaitingDocker}"; });

                await WaitForDockerAsync(viewModel, client);
            }
    }

    private static async Task PullDbContainerAsync(MainViewModel viewModel, DockerClient client)
    {
        while (true)
            try
            {
                Dispatcher.UIThread.Post(() => { viewModel.PhaseText = $"{Assets.Resources.DbContainerPull}"; });

                // Pull the PostgreSQL image
                await client.Images.CreateImageAsync(
                    new ImagesCreateParameters
                    {
                        FromImage = "postgres",
                        Tag = Service.DatabaseVersion
                    },
                    null,
                    new Progress<JSONMessage>(),
                    new CancellationTokenSource(TimeSpan.FromMinutes(3)).Token
                );

                return;
            }
            catch (Exception exception) when (exception is TimeoutException or HttpRequestException)
            {
                // If Docker was closed, wait for it to be reopened
                Dispatcher.UIThread.Post(() => { viewModel.PhaseText = $"{Assets.Resources.WaitingDocker}"; });

                await WaitForDockerAsync(viewModel, client);
            }
    }

    private static async Task PullApiAsync(MainViewModel viewModel, DockerClient client)
    {
        while (true)
            try
            {
                Dispatcher.UIThread.Post(() => { viewModel.PhaseText = $"{Assets.Resources.ApiContainerPull}"; });

                // Pull the API image
                await client.Images.CreateImageAsync(
                    new ImagesCreateParameters
                    {
                        FromImage = "ghcr.io/andrexace/adatickets-api",
                        Tag = "latest"
                    },
                    null,
                    new Progress<JSONMessage>(),
                    new CancellationTokenSource(TimeSpan.FromMinutes(3)).Token
                );

                return;
            }
            catch (Exception exception) when (exception is TimeoutException or HttpRequestException)
            {
                // If Docker was closed, wait for it to be reopened
                Dispatcher.UIThread.Post(() => { viewModel.PhaseText = $"{Assets.Resources.WaitingDocker}"; });

                await WaitForDockerAsync(viewModel, client);
            }
    }

    private static async Task PullWebAppAsync(MainViewModel viewModel, DockerClient client)
    {
        while (true)
            try
            {
                Dispatcher.UIThread.Post(() => { viewModel.PhaseText = $"{Assets.Resources.WebContainerPull}"; });

                // Pull the web image
                await client.Images.CreateImageAsync(
                    new ImagesCreateParameters
                    {
                        FromImage = "ghcr.io/andrexace/adatickets-web",
                        Tag = "latest"
                    },
                    null,
                    new Progress<JSONMessage>(),
                    new CancellationTokenSource(TimeSpan.FromMinutes(3)).Token
                );

                return;
            }
            catch (Exception exception) when (exception is TimeoutException or HttpRequestException)
            {
                // If Docker was closed, wait for it to be reopened
                Dispatcher.UIThread.Post(() => { viewModel.PhaseText = $"{Assets.Resources.WaitingDocker}"; });

                await WaitForDockerAsync(viewModel, client);
            }
    }

    private static async Task RunComposeAsync(MainViewModel viewModel, DockerClient client, string path)
    {
        while (true)
            try
            {
                Dispatcher.UIThread.Post(() => { viewModel.PhaseText = $"{Assets.Resources.ComposeStartup}"; });

                // Get the resource
                var composeStream =
                    AssetLoader.Open(
                        new Uri(
                            $"avares://{Assembly.GetExecutingAssembly().GetName().Name}/Assets/docker-compose.yml"));
                var overrideStream = AssetLoader.Open(new Uri(
                    $"avares://{Assembly.GetExecutingAssembly().GetName().Name}/Assets/docker-compose.override-prod.yml"));

                // Read the templates
                using StreamReader composeReader = new(composeStream);
                var composeContent = await composeReader.ReadToEndAsync();

                using StreamReader reader = new(overrideStream);
                var overrideContent = await reader.ReadToEndAsync();

                // Write to the temporary location
                var tempComposePath = Path.Combine(path, "docker-compose.yml");
                await File.WriteAllTextAsync(tempComposePath, composeContent);

                var tempOverridePath = Path.Combine(path, "docker-compose.override-prod.yml");
                await File.WriteAllTextAsync(tempOverridePath, overrideContent);

                // Find the Docker executable in the known paths
                var dockerExePath = GetDockerExePath();

                // Run the docker-compose command
                Process process = new()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = dockerExePath,
                        Arguments = "compose -f docker-compose.yml -f docker-compose.override-prod.yml up -d",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = path
                    }
                };
                _ = process.Start();

                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                    throw new InvalidOperationException(await process.StandardError.ReadToEndAsync());

                return;
            }
            catch (Exception exception) when (exception is TimeoutException or HttpRequestException)
            {
                // If Docker was closed, wait for it to be reopened
                Dispatcher.UIThread.Post(() => { viewModel.PhaseText = $"{Assets.Resources.WaitingDocker}"; });

                await WaitForDockerAsync(viewModel, client);
            }
    }

    private static async Task WaitForDockerAsync(MainViewModel viewModel, DockerClient client)
    {
        while (true)
            try
            {
                // Ping the Docker server to check if it's available
                await client.System.PingAsync();

                return;
            }
            catch (Exception exception) when (exception is TimeoutException or HttpRequestException)
            {
                // If it's unavailable, update the message and wait for it to be ready
                await Task.Delay(5000);

                Dispatcher.UIThread.Post(() => { viewModel.PhaseText = $"{Assets.Resources.StillWaitingDocker}"; });
            }
    }

    private static void CleanTempFiles(MainViewModel viewModel, string? tempPath)
    {
        viewModel.PhaseText = $"{Assets.Resources.CleaningTemp}";

        if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
    }
}