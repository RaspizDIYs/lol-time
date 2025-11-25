using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using LolTime.App.Services;
using LolTime.App.ViewModels;
using Velopack;
using Velopack.Sources;
using LolTime.App.Views;
using Wpf.Ui;

namespace LolTime.App;

public partial class App : Application
{
    private MainWindow? _mainWindow;
    
    private DataService? _dataService;
    private GameMonitorService? _gameMonitor;
    private TimeTrackerService? _timeTracker;
    private AlertService? _alertService;
    private ISnackbarService? _snackbarService;
    
    private bool _isHandlingError;

    public App()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
    }

    private const string UpdateUrl = "https://github.com/RaspizDIYs/lol-time";

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        await CheckForUpdates();

        // Services
        _dataService = new DataService();
        _alertService = new AlertService(_dataService);
        _timeTracker = new TimeTrackerService(_dataService, _alertService);
        _gameMonitor = new GameMonitorService(); 
        _snackbarService = new SnackbarService();

        // ViewModel
        var mainViewModel = new MainViewModel(_dataService, _timeTracker, _snackbarService);

        // Window
        _mainWindow = new MainWindow(mainViewModel);
        
        // Link Snackbar
        _snackbarService.SetSnackbarPresenter(_mainWindow.SnackbarPresenter);

        ShowMainWindow();
    }

    private async Task CheckForUpdates()
    {
        try 
        {
            var source = new GithubSource(UpdateUrl, null, false);
            var mgr = new UpdateManager(source);

            if (mgr.IsInstalled)
            {
                var newVersion = await mgr.CheckForUpdatesAsync();
                if (newVersion != null)
                {
                    await mgr.DownloadUpdatesAsync(newVersion);
                    mgr.ApplyUpdatesAndRestart(newVersion);
                }
            }
        }
        catch 
        {
            // Ignore errors
        }
    }

    private void ShowMainWindow()
    {
        if (_mainWindow == null) return;
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _gameMonitor?.Dispose();
        _timeTracker?.Dispose();
        _dataService?.SaveData();
        
        base.OnExit(e);
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        ShowError(e.Exception);
        e.Handled = true;
    }

    private void CurrentDomainOnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        ShowError(e.ExceptionObject as Exception ?? new Exception("Неизвестная ошибка"));
    }

    private void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        ShowError(e.Exception);
        e.SetObserved();
    }

    private void ShowError(Exception ex)
    {
        if (_isHandlingError)
            return;

        _isHandlingError = true;
        try
        {
            ErrorWindow.Show(ex);
        }
        finally
        {
            _isHandlingError = false;
        }
    }
}
