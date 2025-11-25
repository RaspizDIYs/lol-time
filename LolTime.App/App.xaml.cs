using System.Windows;
using H.NotifyIcon;
using LolTime.App.Services;
using LolTime.App.ViewModels;
using Velopack;
using Velopack.Sources;

namespace LolTime.App;

public partial class App : Application
{
    private TaskbarIcon? _notifyIcon;
    private MainWindow? _mainWindow;
    private DataService? _dataService;
    private GameMonitorService? _gameMonitor;
    private TimeTrackerService? _timeTracker;
    private AlertService? _alertService;

    // TODO: ЗАМЕНИ НА СВОЙ РЕПОЗИТОРИЙ
    // Например: "https://github.com/Sacha/LolTime"
    private const string UpdateUrl = "https://github.com/RaspizDIYs/lol-time";

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Check for updates in background
        await CheckForUpdates();

        // Services
        _dataService = new DataService();
        _alertService = new AlertService(_dataService);
        _timeTracker = new TimeTrackerService(_dataService, _alertService);
        _gameMonitor = new GameMonitorService(); 

        // ViewModel
        var mainViewModel = new MainViewModel(_dataService, _timeTracker);

        // Window
        _mainWindow = new MainWindow(mainViewModel);

        // Tray Icon
        _notifyIcon = new TaskbarIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            ToolTipText = $"LolTime v{VelopackApp.GetAppSettings()?.DisplayVersion ?? "Dev"}"
        };
        
        _notifyIcon.TrayMouseDoubleClick += (s, args) => ShowMainWindow();

        // Context Menu
        var contextMenu = new System.Windows.Controls.ContextMenu();
        var exitItem = new System.Windows.Controls.MenuItem { Header = "Exit" };
        exitItem.Click += (s, args) => Shutdown();
        var showItem = new System.Windows.Controls.MenuItem { Header = "Show Stats" };
        showItem.Click += (s, args) => ShowMainWindow();
        
        contextMenu.Items.Add(showItem);
        contextMenu.Items.Add(exitItem);
        _notifyIcon.ContextMenu = contextMenu;
    }

    private async Task CheckForUpdates()
    {
        // if (UpdateUrl.Contains("YOUR_GITHUB_USERNAME")) return; // Don't check if url is not set

        try 
        {
            // Используем GithubSource для обновлений
            var source = new GithubSource(UpdateUrl, null, false);
            var mgr = new UpdateManager(source);

            if (mgr.IsInstalled)
            {
                // Проверяем обновления
                var newVersion = await mgr.CheckForUpdatesAsync();
                if (newVersion != null)
                {
                    // Скачиваем
                    await mgr.DownloadUpdatesAsync(newVersion);
                    
                    // Применяем и перезапускаем (опционально можно просто подготовить и обновить при выходе)
                    // Здесь мы применяем и перезапускаем сразу, или можно спросить пользователя.
                    // Для простоты - перезапуск.
                    mgr.ApplyUpdatesAndRestart(newVersion, args: null);
                }
            }
        }
        catch 
        {
            // Ignore errors (no internet, github down, etc)
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
        _notifyIcon?.Dispose();
        _dataService?.SaveData();
        base.OnExit(e);
    }
}
