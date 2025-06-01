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
using ADAtickets.Installer.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Threading;
using ReactiveUI;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;

namespace ADAtickets.Installer.ViewModels;

class MainViewModel : ReactiveObject
{
    private UserControl _currentView;
    private string? _dbUserName;
    private string? _dbPassword;
    private string? _sslPassword;
    private string? _sslPath;
    private string? _volumePath;
    private string? _version;
    private string? _tenantId;
    private string? _tenantDomain;
    private string? _externalTenantId;
    private string? _externalTenantDomain;

    public UserControl CurrentView
    {
        get => _currentView;
        set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    public string? DbUserName
    {
        get => _dbUserName;
        set => this.RaiseAndSetIfChanged(ref _dbUserName, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    public string? DbPassword
    {
        get => _dbPassword;
        set => this.RaiseAndSetIfChanged(ref _dbPassword, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    public string? SslPassword
    {
        get => _sslPassword;
        set => this.RaiseAndSetIfChanged(ref _sslPassword, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    [RegularExpression(@"^[a-zA-Z0-9\/\\_.~:-]+$",
                     ErrorMessageResourceType = typeof(Assets.Resources),
                     ErrorMessageResourceName = "InvalidPath")]
    public string? SslPath
    {
        get => _sslPath;
        set => this.RaiseAndSetIfChanged(ref _sslPath, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    [RegularExpression(@"^[a-zA-Z0-9\/\\_.~:-]+$",
                     ErrorMessageResourceType = typeof(Assets.Resources),
                     ErrorMessageResourceName = "InvalidPath")]
    public string? VolumePath
    {
        get => _volumePath;
        set => this.RaiseAndSetIfChanged(ref _volumePath, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    public string? Version
    {
        get => _version;
        set => this.RaiseAndSetIfChanged(ref _version, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    [RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
                     ErrorMessageResourceType = typeof(Assets.Resources),
                     ErrorMessageResourceName = "InvalidGuid")]
    public string? TenantId
    {
        get => _tenantId;
        set => this.RaiseAndSetIfChanged(ref _tenantId, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    [RegularExpression(@"^[a-zA-Z0-9\-\.]+\.onmicrosoft\.com$",
                     ErrorMessageResourceType = typeof(Assets.Resources),
                     ErrorMessageResourceName = "InvalidDomain")]
    public string? TenantDomain
    {
        get => _tenantDomain;
        set => this.RaiseAndSetIfChanged(ref _tenantDomain, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    [RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
                     ErrorMessageResourceType = typeof(Assets.Resources),
                     ErrorMessageResourceName = "InvalidGuid")]
    public string? ExternalTenantId
    {
        get => _externalTenantId;
        set => this.RaiseAndSetIfChanged(ref _externalTenantId, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    [RegularExpression(@"^[a-zA-Z0-9\-\.]+\.onmicrosoft\.com$",
                     ErrorMessageResourceType = typeof(Assets.Resources),
                     ErrorMessageResourceName = "InvalidDomain")]
    public string? ExternalTenantDomain
    {
        get => _externalTenantDomain;
        set => this.RaiseAndSetIfChanged(ref _externalTenantDomain, value);
    }

    public static ICommand ExitAppCommand => ReactiveCommand.Create(ExitApp);
    public static ICommand ChangeThemeCommand => ReactiveCommand.Create(ChangeTheme);
    public ICommand GoToSecondStepCommand => ReactiveCommand.Create(GoToSecondStep);
    public ICommand GoToThirdStepCommand => ReactiveCommand.Create(GoToThirdStep);

    public MainViewModel()
    {
        _currentView = new FirstStep { DataContext = this };
    }

    private static void ExitApp()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            lifetime.Shutdown();
    }

    private static void ChangeTheme()
    {
        var app = Application.Current;

        if (app != null)
            app.RequestedThemeVariant = app.ActualThemeVariant == ThemeVariant.Light
            ? ThemeVariant.Dark
            : ThemeVariant.Light;
    }

    private void GoToSecondStep()
    {
        CurrentView = new SecondStep { DataContext = this };

        RepositionWindow();
    }

    private void GoToThirdStep()
    {
        CurrentView = new ThirdStep { DataContext = this };

        RepositionWindow();
    }

    private static void RepositionWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = desktop.MainWindow;
            if (mainWindow != null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    var screen = mainWindow.Screens.ScreenFromWindow(mainWindow);
                    if (screen != null)
                    {
                        double left = (screen.Bounds.Width - mainWindow.Bounds.Width) / 2;
                        double top = (screen.Bounds.Height - mainWindow.Bounds.Height) / 2;
                        mainWindow.Position = new PixelPoint((int)left, (int)top);
                    }
                });
            }
        }
    }
}