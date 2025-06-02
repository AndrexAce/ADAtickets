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
    private string? _apiVersion;
    private string? _webVersion;
    private string? _tenantId;
    private string? _tenantDomain;
    private string? _externalTenantId;
    private string? _externalTenantDomain;
    private string? _clientId;
    private string? _apiId;
    private string? _externalClientId;
    private string? _externalApiId;

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
    public string? ApiVersion
    {
        get => _apiVersion;
        set => this.RaiseAndSetIfChanged(ref _apiVersion, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    public string? WebVersion
    {
        get => _webVersion;
        set => this.RaiseAndSetIfChanged(ref _webVersion, value);
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

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    [RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
                     ErrorMessageResourceType = typeof(Assets.Resources),
                     ErrorMessageResourceName = "InvalidGuid")]
    public string? ClientId
    {
        get => _clientId;
        set => this.RaiseAndSetIfChanged(ref _clientId, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    [RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
                     ErrorMessageResourceType = typeof(Assets.Resources),
                     ErrorMessageResourceName = "InvalidGuid")]
    public string? ApiId
    {
        get => _apiId;
        set => this.RaiseAndSetIfChanged(ref _apiId, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    [RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
                     ErrorMessageResourceType = typeof(Assets.Resources),
                     ErrorMessageResourceName = "InvalidGuid")]
    public string? ExternalClientId
    {
        get => _externalClientId;
        set => this.RaiseAndSetIfChanged(ref _externalClientId, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    [RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
                     ErrorMessageResourceType = typeof(Assets.Resources),
                     ErrorMessageResourceName = "InvalidGuid")]
    public string? ExternalApiId
    {
        get => _externalApiId;
        set => this.RaiseAndSetIfChanged(ref _externalApiId, value);
    }

    public static ICommand ExitAppCommand => ReactiveCommand.Create(ExitApp);
    public static ICommand ChangeThemeCommand => ReactiveCommand.Create(ChangeTheme);
    public ICommand GoToSecondStepCommand => ReactiveCommand.Create(GoToSecondStep);
    public ICommand GoToThirdStepCommand => ReactiveCommand.Create(GoToThirdStep);
    public ICommand GoToLastStepCommand => ReactiveCommand.Create(GoToFinalStep);

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
        if (ValidateSecondStepDocker() && ValidateSecondStepAzure())
        {
            CurrentView = new ThirdStep { DataContext = this };

            RepositionWindow();
        }
    }

    private void GoToFinalStep()
    {
        if (ValidateThirdStep())
        {
            CurrentView = new LastStep { DataContext = this };

            RepositionWindow();
        }
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

    private bool ValidateSecondStepDocker()
    {
        var isValid = true;
        var context = new ValidationContext(this)
        {
            MemberName = nameof(DbUserName)
        };
        isValid &= Validator.TryValidateProperty(DbUserName, context, []);

        context.MemberName = nameof(DbPassword);
        isValid &= Validator.TryValidateProperty(DbPassword, context, []);

        context.MemberName = nameof(SslPassword);
        isValid &= Validator.TryValidateProperty(SslPassword, context, []);

        context.MemberName = nameof(SslPath);
        isValid &= Validator.TryValidateProperty(SslPath, context, []);

        context.MemberName = nameof(VolumePath);
        isValid &= Validator.TryValidateProperty(VolumePath, context, []);

        context.MemberName = nameof(ApiVersion);
        isValid &= Validator.TryValidateProperty(ApiVersion, context, []);

        context.MemberName = nameof(WebVersion);
        isValid &= Validator.TryValidateProperty(WebVersion, context, []);

        return isValid;
    }

    private bool ValidateSecondStepAzure()
    {
        var isValid = true;
        var context = new ValidationContext(this)
        {
            MemberName = nameof(TenantId)
        };
        isValid &= Validator.TryValidateProperty(TenantId, context, []);

        context.MemberName = nameof(TenantDomain);
        isValid &= Validator.TryValidateProperty(TenantDomain, context, []);

        context.MemberName = nameof(ExternalTenantId);
        isValid &= Validator.TryValidateProperty(ExternalTenantId, context, []);

        context.MemberName = nameof(ExternalTenantDomain);
        isValid &= Validator.TryValidateProperty(ExternalTenantDomain, context, []);

        return isValid;
    }

    private bool ValidateThirdStep()
    {
        var isValid = true;
        var context = new ValidationContext(this)
        {
            MemberName = nameof(ClientId)
        };
        isValid &= Validator.TryValidateProperty(ClientId, context, []);

        context.MemberName = nameof(ApiId);
        isValid &= Validator.TryValidateProperty(ApiId, context, []);

        context.MemberName = nameof(ExternalClientId);
        isValid &= Validator.TryValidateProperty(ExternalClientId, context, []);

        context.MemberName = nameof(ExternalApiId);
        isValid &= Validator.TryValidateProperty(ExternalApiId, context, []);

        return isValid;
    }
}