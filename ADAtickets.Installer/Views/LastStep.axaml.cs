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
using System.Threading;
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

    private const string WINDOWS_DOCKERSERVER = "npipe://./pipe/docker_engine";
    private const string LINUX_DOCKERSERVER = "unix:///var/run/docker.sock";
    private const string WINDOWS_DOCKEREXE = @"C:\Program Files\Docker\Docker\resources\bin\docker.exe";
    private const string LINUX_DOCKEREXE = "/usr/bin/docker";

    private async Task ExecuteSetup()
    {
        // Get the ViewModel from DataContext
        if (DataContext is MainViewModel viewModel)
        {
            try
            {
                viewModel.PhaseText = Assets.Resources.EnvFileWriting;

                var tempPath = CreateRandomTempFolder();

                await Task.Run(() => WriteToEnvFileAsync(viewModel, tempPath));
                await Task.Delay(3000);

                viewModel.ProgressBarValue = 10;

                // Create a Docker client from the correct URI based on the OS
                using DockerClient client = new DockerClientConfiguration(
                OperatingSystem.IsWindows() ?
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

                await Task.Run(() => RunComposeAsync(viewModel, client, tempPath));
                await Task.Delay(3000);

                viewModel.ProgressBarValue = 90;

                CleanTempFiles(viewModel, tempPath);
                await Task.Delay(3000);

                viewModel.ProgressBarValue = 100;

                viewModel.IsLoadingVisible = false;
                viewModel.PhaseText = Assets.Resources.SetupCompleted;
            }
            catch (Exception e)
            {
                // Other unexpected errors
                viewModel.IsLoadingVisible = false;
                viewModel.PhaseText = Assets.Resources.ErrorOccurred + " " + e.GetType().ToString();
            }
        }
    }

    private static string CreateRandomTempFolder()
    {
        // Create a temporary folder with a random name
        string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        if (OperatingSystem.IsWindows())
        {
            Directory.CreateDirectory(tempPath);
        }
        else
        {
            Directory.CreateDirectory(tempPath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }

        return tempPath;
    }

    private static async Task WriteToEnvFileAsync(MainViewModel viewModel, string path)
    {
        // Get assembly containing the embedded resource
        var assembly = Assembly.GetExecutingAssembly();
        string resourceAssemblyPath = "ADAtickets.Installer.Assets.example.env";

        // Read the template
        using Stream? stream = assembly.GetManifestResourceStream(resourceAssemblyPath) ??
            throw new IOException($"Could not find embedded resource: {resourceAssemblyPath}");
        using StreamReader reader = new(stream);
        string templateContent = await reader.ReadToEndAsync();

        // Replace variables with ViewModel data
        // Database configuration
        string fileContent = Regex.Replace(templateContent, "^POSTGRESUSER=.*$", $"POSTGRESUSER={viewModel.DbUserName}", RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));
        fileContent = Regex.Replace(fileContent, "^POSTGRESPASSWORD=.*$", $"POSTGRESPASSWORD={viewModel.DbPassword}", RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));

        // SSL certificate configuration
        fileContent = Regex.Replace(fileContent, "^SSLCERTIFICATEDISKPATH=.*$", $"SSLCERTIFICATEDISKPATH={Path.GetDirectoryName(viewModel.SslCertificatePath)}", RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));
        fileContent = Regex.Replace(fileContent, "^SSLCERTIFICATENAME=.*$", $"SSLCERTIFICATENAME={Path.GetFileName(viewModel.SslCertificatePath)}", RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));
        fileContent = Regex.Replace(fileContent, "^SSLCERTIFICATEPASSWORD=.*$", $"SSLCERTIFICATEPASSWORD={viewModel.SslCertificatePassword}", RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));

        // App tag
        string? apiTag = await Dispatcher.UIThread.InvokeAsync(() => viewModel.ApiVersion?.Content?.ToString());
        fileContent = Regex.Replace(fileContent, "^APITAG=.*$", $"APITAG={apiTag}", RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));
        string? webTag = await Dispatcher.UIThread.InvokeAsync(() => viewModel.WebVersion?.Content?.ToString());
        fileContent = Regex.Replace(fileContent, "^WEBTAG=.*$", $"WEBTAG={webTag}", RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));

        // Tenant configuration
        fileContent = Regex.Replace(fileContent, "^TENANTID=.*$", $"TENANTID={viewModel.TenantId}", RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));
        fileContent = Regex.Replace(fileContent, "^EXTERNALTENANTID=.*$", $"EXTERNALTENANTID={viewModel.ExternalTenantId}", RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));

        // Entra ID configuration (API)
        fileContent = Regex.Replace(fileContent, "^APIAPPID=.*$", $"APIAPPID={viewModel.ApiAppId}", RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));

        // External Entra ID configuration (API)
        fileContent = Regex.Replace(fileContent, "^EXTERNALAPIAPPID=.*$", $"EXTERNALAPIAPPID={viewModel.ExternalApiAppId}", RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));

        // Entra ID configuration (web app)
        fileContent = Regex.Replace(fileContent, "^WEBAPPID=.*$", $"WEBAPPID={viewModel.WebAppId}", RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));

        // External Entra ID configuration (web app)
        fileContent = Regex.Replace(fileContent, "^EXTERNALWEBAPPID=.*$", $"EXTERNALWEBAPPID={viewModel.ExternalWebAppId}", RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));

        // Authentication configuration (API)
        fileContent = Regex.Replace(fileContent, "^APIAUTHCERTIFICATEDISKPATH=.*$", $"APIAUTHCERTIFICATEDISKPATH={Path.GetDirectoryName(viewModel.ApiAuthCertificatePath)}", RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));
        fileContent = Regex.Replace(fileContent, "^APIAUTHCERTIFICATENAME=.*$", $"APIAUTHCERTIFICATENAME={Path.GetFileName(viewModel.ApiAuthCertificatePath)}", RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));
        fileContent = Regex.Replace(fileContent, "^APIAUTHCERTIFICATEPASSWORD=.*$", $"APIAUTHCERTIFICATEPASSWORD={viewModel.ApiAuthCertificatePassword}", RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));

        // Authentication configuration (web app)
        fileContent = Regex.Replace(fileContent, "^WEBAUTHCERTIFICATEDISKPATH=.*$", $"WEBAUTHCERTIFICATEDISKPATH={Path.GetDirectoryName(viewModel.WebAuthCertificatePath)}", RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));
        fileContent = Regex.Replace(fileContent, "^WEBAUTHCERTIFICATENAME=.*$", $"WEBAUTHCERTIFICATENAME={Path.GetFileName(viewModel.WebAuthCertificatePath)}", RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));
        fileContent = Regex.Replace(fileContent, "^WEBAUTHCERTIFICATEPASSWORD=.*$", $"WEBAUTHCERTIFICATEPASSWORD={viewModel.WebAuthCertificatePassword}", RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));

        // Write to temporary location
        string tempEnvPath = Path.Combine(path, ".env");
        await File.WriteAllTextAsync(tempEnvPath, fileContent);
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
                        Tag = "17.5"
                    },
                    null,
                    new Progress<JSONMessage>(),
                    new CancellationTokenSource(TimeSpan.FromMinutes(3)).Token
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
                        Tag = await Dispatcher.UIThread.InvokeAsync(() => viewModel.ApiVersion?.Content?.ToString())
                    },
                    null,
                    new Progress<JSONMessage>(),
                    new CancellationTokenSource(TimeSpan.FromMinutes(3)).Token
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
                        Tag = await Dispatcher.UIThread.InvokeAsync(() => viewModel.WebVersion?.Content?.ToString())
                    },
                    null,
                    new Progress<JSONMessage>(),
                    new CancellationTokenSource(TimeSpan.FromMinutes(3)).Token
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

    private static async Task RunComposeAsync(MainViewModel viewModel, DockerClient client, string path)
    {
        while (true)
        {
            try
            {
                Dispatcher.UIThread.Post(() => viewModel.PhaseText = $"{Assets.Resources.ComposeStartup}");

                // Get assembly containing the embedded resource
                var assembly = Assembly.GetExecutingAssembly();
                string composeAssemblyPath = "ADAtickets.Installer.Assets.docker-compose.yml";
                string overrideAssemblyPath = "ADAtickets.Installer.Assets.docker-compose.override-prod.yml";

                // Read the template
                using Stream? composeStream = assembly.GetManifestResourceStream(composeAssemblyPath) ??
                    throw new IOException($"Could not find embedded resource: {composeAssemblyPath}");
                using StreamReader composeReader = new(composeStream);
                string composeContent = await composeReader.ReadToEndAsync();

                using Stream? overrideStream = assembly.GetManifestResourceStream(overrideAssemblyPath) ??
                    throw new IOException($"Could not find embedded resource: {overrideAssemblyPath}");
                using StreamReader reader = new(overrideStream);
                string overrideContent = await reader.ReadToEndAsync();

                // Write to temporary location
                string tempComposePath = Path.Combine(path, "docker-compose.yml");
                await File.WriteAllTextAsync(tempComposePath, composeContent);

                string tempOverridePath = Path.Combine(path, "docker-compose.override-prod.yml");
                await File.WriteAllTextAsync(tempOverridePath, overrideContent);

                // Find the Docker executable in the known paths
                var dockerExePath = OperatingSystem.IsWindows() ? WINDOWS_DOCKEREXE : LINUX_DOCKEREXE;

                // Run the docker-compose command
                var process = new Process
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
                process.Start();

                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException(await process.StandardError.ReadToEndAsync());
                }

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

    private static void CleanTempFiles(MainViewModel viewModel, string tempPath)
    {
        viewModel.PhaseText = $"{Assets.Resources.CleaningTemp}";

        if (Directory.Exists(tempPath))
        {
            Directory.Delete(tempPath, true);
        }
    }
}
