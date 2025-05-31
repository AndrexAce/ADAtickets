using ADAtickets.Installer.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using ReactiveUI;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;

namespace ADAtickets.Installer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private UserControl _currentView;
        private string? dbName;
        private string? dbPassword;
        private string? sslPassword;
        private string? sslPath;
        private string? volumePath;
        private string? version;

        public UserControl CurrentView
        {
            get => _currentView;
            set => this.RaiseAndSetIfChanged(ref _currentView, value);
        }

        [Required]
        public string? DbName
        {
            get => dbName;
            set => this.RaiseAndSetIfChanged(ref dbName, value);
        }

        [Required]
        public string? DbPassword
        {
            get => dbPassword;
            set => this.RaiseAndSetIfChanged(ref dbPassword, value);
        }

        [Required]
        public string? SslPassword
        {
            get => sslPassword;
            set => this.RaiseAndSetIfChanged(ref sslPassword, value);
        }

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9\/\\_.~:-]+$")]
        public string? SslPath
        {
            get => sslPath;
            set => this.RaiseAndSetIfChanged(ref sslPath, value);
        }

        [RegularExpression(@"^[a-zA-Z0-9\/\\_.~:-]+$")]
        public string? VolumePath
        {
            get => volumePath;
            set => this.RaiseAndSetIfChanged(ref volumePath, value);
        }

        [RegularExpression(@"^v\d+\.\d+\.\d+$")]
        public string? Version
        {
            get => version;
            set => this.RaiseAndSetIfChanged(ref version, value);
        }

        public static ICommand CloseWindow => ReactiveCommand.Create(ExitApp);
        public ICommand ChangeTheme => ReactiveCommand.Create(ApplyTheme);
        public ICommand NextStep => ReactiveCommand.Create(GoToSecondStep);

        public MainViewModel()
        {
            _currentView = new FirstStep { DataContext = this };
        }

        private static void ExitApp()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
                lifetime.Shutdown();
        }

        private void ApplyTheme()
        {
            var app = Application.Current;

            app.RequestedThemeVariant = app.ActualThemeVariant == ThemeVariant.Light
                ? ThemeVariant.Dark
                : ThemeVariant.Light;
        }

        private void GoToSecondStep()
        {
            var validationErrors = new List<ValidationResult>();

            if (Validator.TryValidateObject(this, new ValidationContext(this), validationErrors, true))
            {
                CurrentView = new SecondStep { DataContext = this };
            }
        }
    }
}