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
using System.IO;
using System.Windows.Input;

namespace ADAtickets.Installer.ViewModels;

class MainViewModel : ReactiveObject
{
    private UserControl _currentView;
    private string? _dbUserName;
    private string? _dbPassword;
    private string? _sslCertificatePath;
    private string? _sslCertificatePassword;
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
    private string? _authCertificatePath;
    private string? _authCertificatePassword;
    private string _phaseText = "";
    private int progressBarValue = 0;

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
    [RegularExpression(@"^[a-zA-Z0-9\/\\_.~:-]+$",
                     ErrorMessageResourceType = typeof(Assets.Resources),
                     ErrorMessageResourceName = "InvalidPath")]
    public string? SslCertificatePath
    {
        get => _sslCertificatePath;
        set => this.RaiseAndSetIfChanged(ref _sslCertificatePath, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    public string? SslCertificatePassword
    {
        get => _sslCertificatePassword;
        set => this.RaiseAndSetIfChanged(ref _sslCertificatePassword, value);
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

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    [RegularExpression(@"^[a-zA-Z0-9\/\\_.~:-]+$",
                     ErrorMessageResourceType = typeof(Assets.Resources),
                     ErrorMessageResourceName = "InvalidPath")]
    public string? AuthCertificatePath
    {
        get => _authCertificatePath;
        set => this.RaiseAndSetIfChanged(ref _authCertificatePath, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    public string? AuthCertificatePassword
    {
        get => _authCertificatePassword;
        set => this.RaiseAndSetIfChanged(ref _authCertificatePassword, value);
    }

    public string PhaseText
    {
        get => _phaseText;
        set => this.RaiseAndSetIfChanged(ref _phaseText, value);
    }

    public int ProgressBarValue
    {
        get => progressBarValue;
        set => this.RaiseAndSetIfChanged(ref progressBarValue, value);
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

        context.MemberName = nameof(SslCertificatePath);
        isValid &= Validator.TryValidateProperty(SslCertificatePath, context, []);
        isValid &= File.Exists(SslCertificatePath);

        context.MemberName = nameof(SslCertificatePassword);
        isValid &= Validator.TryValidateProperty(SslCertificatePassword, context, []);

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

        context.MemberName = nameof(AuthCertificatePath);
        isValid &= Validator.TryValidateProperty(AuthCertificatePassword, context, []);
        isValid &= File.Exists(AuthCertificatePath);

        context.MemberName = nameof(AuthCertificatePassword);
        isValid &= Validator.TryValidateProperty(AuthCertificatePassword, context, []);

        return isValid;
    }
}