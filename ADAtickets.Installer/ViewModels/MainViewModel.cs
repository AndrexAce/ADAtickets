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

sealed class MainViewModel : ReactiveObject
{
    private UserControl _currentView;
    private string? _dbUserName;
    private string? _dbPassword;
    private string? _sslCertificatePath;
    private string? _sslCertificatePassword;
    private ComboBoxItem? _apiVersion;
    private ComboBoxItem? _webVersion;
    private string? _devOpsOrganizationName;
    private string? _tenantId;
    private string? _externalTenantId;
    private string? _apiAppId;
    private string? _externalApiAppId;
    private string? _webAppId;
    private string? _externalWebAppId;
    private string? _apiAuthCertificatePath;
    private string? _apiAuthCertificatePassword;
    private string? _webAuthCertificatePath;
    private string? _webAuthCertificatePassword;
    private string _phaseText = "";
    private int _progressBarValue = 0;
    private bool _isLoadingVisible = true;

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
    public ComboBoxItem? ApiVersion
    {
        get => _apiVersion;
        set => this.RaiseAndSetIfChanged(ref _apiVersion, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    public ComboBoxItem? WebVersion
    {
        get => _webVersion;
        set => this.RaiseAndSetIfChanged(ref _webVersion, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    [RegularExpression(@"^[a-zA-Z0-9][a-zA-Z0-9\-]{0,48}[a-zA-Z0-9]$",
                     ErrorMessageResourceType = typeof(Assets.Resources),
                     ErrorMessageResourceName = "InvalidOrganizationName")]
    public string? DevOpsOrganizationName
    {
        get => _devOpsOrganizationName;
        set => this.RaiseAndSetIfChanged(ref _devOpsOrganizationName, value);
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
    [RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
                     ErrorMessageResourceType = typeof(Assets.Resources),
                     ErrorMessageResourceName = "InvalidGuid")]
    public string? ApiAppId
    {
        get => _apiAppId;
        set => this.RaiseAndSetIfChanged(ref _apiAppId, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    [RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
                     ErrorMessageResourceType = typeof(Assets.Resources),
                     ErrorMessageResourceName = "InvalidGuid")]
    public string? ExternalApiAppId
    {
        get => _externalApiAppId;
        set => this.RaiseAndSetIfChanged(ref _externalApiAppId, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
          ErrorMessageResourceName = "FieldRequired")]
    [RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
                 ErrorMessageResourceType = typeof(Assets.Resources),
                 ErrorMessageResourceName = "InvalidGuid")]
    public string? WebAppId
    {
        get => _webAppId;
        set => this.RaiseAndSetIfChanged(ref _webAppId, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
          ErrorMessageResourceName = "FieldRequired")]
    [RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
                 ErrorMessageResourceType = typeof(Assets.Resources),
                 ErrorMessageResourceName = "InvalidGuid")]
    public string? ExternalWebAppId
    {
        get => _externalWebAppId;
        set => this.RaiseAndSetIfChanged(ref _externalWebAppId, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
          ErrorMessageResourceName = "FieldRequired")]
    [RegularExpression(@"^[a-zA-Z0-9\/\\_.~:-]+$",
                 ErrorMessageResourceType = typeof(Assets.Resources),
                 ErrorMessageResourceName = "InvalidPath")]
    public string? ApiAuthCertificatePath
    {
        get => _apiAuthCertificatePath;
        set => this.RaiseAndSetIfChanged(ref _apiAuthCertificatePath, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    public string? ApiAuthCertificatePassword
    {
        get => _apiAuthCertificatePassword;
        set => this.RaiseAndSetIfChanged(ref _apiAuthCertificatePassword, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    [RegularExpression(@"^[a-zA-Z0-9\/\\_.~:-]+$",
                     ErrorMessageResourceType = typeof(Assets.Resources),
                     ErrorMessageResourceName = "InvalidPath")]
    public string? WebAuthCertificatePath
    {
        get => _webAuthCertificatePath;
        set => this.RaiseAndSetIfChanged(ref _webAuthCertificatePath, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    public string? WebAuthCertificatePassword
    {
        get => _webAuthCertificatePassword;
        set => this.RaiseAndSetIfChanged(ref _webAuthCertificatePassword, value);
    }

    public string PhaseText
    {
        get => _phaseText;
        set => this.RaiseAndSetIfChanged(ref _phaseText, value);
    }

    public int ProgressBarValue
    {
        get => _progressBarValue;
        set => this.RaiseAndSetIfChanged(ref _progressBarValue, value);
    }

    public bool IsLoadingVisible
    {
        get => _isLoadingVisible;
        set => this.RaiseAndSetIfChanged(ref _isLoadingVisible, value);
    }

    public static ICommand ExitAppCommand => ReactiveCommand.Create(ExitApp);
    public static ICommand ChangeThemeCommand => ReactiveCommand.Create(ChangeTheme);
    public ICommand GoToSecondStepCommand => ReactiveCommand.Create(GoToSecondStep);
    public ICommand GoToThirdStepCommand => ReactiveCommand.Create(GoToThirdStep);
    public ICommand GoToLastStepCommand => ReactiveCommand.Create(GoToFinalStep);
    public ICommand GoToPreviousStepCommand => ReactiveCommand.Create(GoToSecondStep);

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

        context.MemberName = nameof(ExternalTenantId);
        isValid &= Validator.TryValidateProperty(ExternalTenantId, context, []);

        context.MemberName = nameof(DevOpsOrganizationName);
        isValid &= Validator.TryValidateProperty(DevOpsOrganizationName, context, []);

        return isValid;
    }

    private bool ValidateThirdStep()
    {
        var isValid = true;
        var context = new ValidationContext(this)
        {
            MemberName = nameof(WebAppId)
        };
        isValid &= Validator.TryValidateProperty(WebAppId, context, []);

        context.MemberName = nameof(ApiAppId);
        isValid &= Validator.TryValidateProperty(ApiAppId, context, []);

        context.MemberName = nameof(ExternalWebAppId);
        isValid &= Validator.TryValidateProperty(ExternalWebAppId, context, []);

        context.MemberName = nameof(ExternalApiAppId);
        isValid &= Validator.TryValidateProperty(ExternalApiAppId, context, []);

        context.MemberName = nameof(WebAuthCertificatePath);
        isValid &= Validator.TryValidateProperty(WebAuthCertificatePassword, context, []);
        isValid &= File.Exists(WebAuthCertificatePath);

        context.MemberName = nameof(WebAuthCertificatePassword);
        isValid &= Validator.TryValidateProperty(WebAuthCertificatePassword, context, []);

        context.MemberName = nameof(ApiAuthCertificatePath);
        isValid &= Validator.TryValidateProperty(ApiAuthCertificatePassword, context, []);
        isValid &= File.Exists(ApiAuthCertificatePath);

        context.MemberName = nameof(ApiAuthCertificatePassword);
        isValid &= Validator.TryValidateProperty(ApiAuthCertificatePassword, context, []);

        return isValid;
    }
}