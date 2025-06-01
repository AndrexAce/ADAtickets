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
    private string? dbUserName;
    private string? dbPassword;
    private string? sslPassword;
    private string? sslPath;
    private string? volumePath;
    private string? version;
    private string? entraAuthority;
    private string? entraAudience;
    private string? entraExternalAuthority;
    private string? entraExternalAudience;

    public UserControl CurrentView
    {
        get => _currentView;
        set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    public string? DbUserName
    {
        get => dbUserName;
        set => this.RaiseAndSetIfChanged(ref dbUserName, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    public string? DbPassword
    {
        get => dbPassword;
        set => this.RaiseAndSetIfChanged(ref dbPassword, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    public string? SslPassword
    {
        get => sslPassword;
        set => this.RaiseAndSetIfChanged(ref sslPassword, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    [RegularExpression(@"^[a-zA-Z0-9\/\\_.~:-]+$",
                     ErrorMessageResourceType = typeof(Assets.Resources),
                     ErrorMessageResourceName = "InvalidPath")]
    public string? SslPath
    {
        get => sslPath;
        set => this.RaiseAndSetIfChanged(ref sslPath, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    [RegularExpression(@"^[a-zA-Z0-9\/\\_.~:-]+$",
                     ErrorMessageResourceType = typeof(Assets.Resources),
                     ErrorMessageResourceName = "InvalidPath")]
    public string? VolumePath
    {
        get => volumePath;
        set => this.RaiseAndSetIfChanged(ref volumePath, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    public string? Version
    {
        get => version;
        set => this.RaiseAndSetIfChanged(ref version, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    public string? EntraAuthority
    {
        get => entraAuthority;
        set => this.RaiseAndSetIfChanged(ref entraAuthority, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    public string? EntraAudience
    {
        get => entraAudience;
        set => this.RaiseAndSetIfChanged(ref entraAudience, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    public string? EntraExternalAuthority
    {
        get => entraExternalAuthority;
        set => this.RaiseAndSetIfChanged(ref entraExternalAuthority, value);
    }

    [Required(ErrorMessageResourceType = typeof(Assets.Resources),
              ErrorMessageResourceName = "FieldRequired")]
    public string? EntraExternalAudience
    {
        get => entraExternalAudience;
        set => this.RaiseAndSetIfChanged(ref entraExternalAudience, value);
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