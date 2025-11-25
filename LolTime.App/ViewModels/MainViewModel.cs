using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LolTime.App.Models;
using LolTime.App.Services;
using System.Collections.ObjectModel;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Velopack;
using Velopack.Sources;

namespace LolTime.App.ViewModels;

public partial class MainViewModel : ObservableObject, IRecipient<SessionEndedMessage>
{
    private readonly DataService _dataService;
    private readonly TimeTrackerService _timeTracker;
    private readonly ISnackbarService _snackbarService;
    private const string UpdateUrl = "https://github.com/RaspizDIYs/lol-time";

    public TimeTrackerService Tracker => _timeTracker;
    public AppSettings Settings => _dataService.Data.Settings;

    [ObservableProperty]
    private string _dailyLimitHours = "";

    [ObservableProperty]
    private string _weeklyLimitHours = "";
    
    [ObservableProperty]
    private string _appVersion = "v1.0.0";
    
    public ObservableCollection<DailyStats> History { get; } = new();

    public MainViewModel(DataService dataService, TimeTrackerService timeTracker, ISnackbarService snackbarService)
    {
        _dataService = dataService;
        _timeTracker = timeTracker;
        _snackbarService = snackbarService;
        
        WeakReferenceMessenger.Default.Register(this);

        DailyLimitHours = Settings.DailyLimit.TotalHours.ToString("0.##");
        WeeklyLimitHours = Settings.WeeklyLimit.TotalHours.ToString("0.##");
        
        // Get version
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        if (version != null)
        {
             AppVersion = $"v{version.Major}.{version.Minor}.{version.Build}";
        }
        
        LoadHistory();
    }

    // Команда для открытия окна из трея
    [RelayCommand]
    private void ShowWindow()
    {
        var window = Application.Current.MainWindow;
        if (window != null)
        {
            window.Show();
            window.WindowState = WindowState.Normal;
            window.Activate();
        }
    }

    // Команда для полного выхода из трея
    [RelayCommand]
    private void Exit()
    {
        if (Application.Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.CanClose = true;
        }
        Application.Current.Shutdown();
    }

    public void Receive(SessionEndedMessage message)
    {
        Application.Current.Dispatcher.Invoke(() => 
        {
            LoadHistory();
            _snackbarService.Show(
                "Сессия завершена", 
                $"Записано: {message.Session.Duration:hh\\:mm\\:ss}", 
                ControlAppearance.Info, 
                new SymbolIcon(SymbolRegular.Timer24), 
                TimeSpan.FromSeconds(5)
            );
        });
    }

    private void LoadHistory()
    {
        History.Clear();
        
        var sessions = _dataService.GetSessionsSnapshot();
        
        var grouped = sessions
            .GroupBy(s => s.Start.Date)
            .OrderByDescending(g => g.Key)
            .Select(g => new DailyStats 
            { 
                Date = g.Key, 
                TotalTime = TimeSpan.FromTicks(g.Sum(s => s.Duration.Ticks)) 
            });

        foreach (var item in grouped)
        {
            History.Add(item);
        }
    }

    [RelayCommand]
    private void SaveSettings()
    {
        if (double.TryParse(DailyLimitHours, out double daily))
            Settings.DailyLimit = TimeSpan.FromHours(daily);
            
        if (double.TryParse(WeeklyLimitHours, out double weekly))
            Settings.WeeklyLimit = TimeSpan.FromHours(weekly);

        _dataService.SaveData();

        _snackbarService.Show(
            "Настройки", 
            "Успешно сохранено!", 
            ControlAppearance.Success, 
            new SymbolIcon(SymbolRegular.Save24), 
            TimeSpan.FromSeconds(3)
        );
    }

    [RelayCommand]
    private void BrowseImage()
    {
        var dlg = new Microsoft.Win32.OpenFileDialog();
        dlg.Filter = "Image files (*.png;*.jpg)|*.png;*.jpg";
        if (dlg.ShowDialog() == true)
        {
            Settings.AlertImagePath = dlg.FileName;
        }
    }

    [RelayCommand]
    private void BrowseSound()
    {
        var dlg = new Microsoft.Win32.OpenFileDialog();
        dlg.Filter = "Audio files (*.wav;*.mp3)|*.wav;*.mp3";
        if (dlg.ShowDialog() == true)
        {
            Settings.AlertSoundPath = dlg.FileName;
        }
    }

    [RelayCommand]
    private async Task CheckUpdates()
    {
        try 
        {
            _snackbarService.Show(
                "Обновление", 
                "Проверяю наличие обновлений...", 
                ControlAppearance.Info, 
                new SymbolIcon(SymbolRegular.ArrowSync24), 
                TimeSpan.FromSeconds(2)
            );

            var source = new GithubSource(UpdateUrl, null, false);
            var mgr = new UpdateManager(source);

            // Check if installed via Velopack
            if (!mgr.IsInstalled)
            {
                 _snackbarService.Show(
                    "Обновление", 
                    "Приложение запущено в режиме отладки. Обновление невозможно.", 
                    ControlAppearance.Caution, 
                    new SymbolIcon(SymbolRegular.Warning24), 
                    TimeSpan.FromSeconds(4)
                );
                return;
            }

            var newVersion = await mgr.CheckForUpdatesAsync();
            if (newVersion != null)
            {
                await mgr.DownloadUpdatesAsync(newVersion);
                
                _snackbarService.Show(
                    "Обновление", 
                    "Обновление скачано. Перезагрузка...", 
                    ControlAppearance.Success, 
                    new SymbolIcon(SymbolRegular.ArrowSync24), 
                    TimeSpan.FromSeconds(3)
                );

                await Task.Delay(3000); // Give user time to read
                mgr.ApplyUpdatesAndRestart(newVersion);
            }
            else
            {
                 _snackbarService.Show(
                    "Обновление", 
                    "У вас установлена последняя версия!", 
                    ControlAppearance.Success, 
                    new SymbolIcon(SymbolRegular.Checkmark24), 
                    TimeSpan.FromSeconds(3)
                );
            }
        }
        catch (Exception ex)
        {
             _snackbarService.Show(
                "Ошибка обновления", 
                ex.Message, 
                ControlAppearance.Danger, 
                new SymbolIcon(SymbolRegular.ErrorCircle24), 
                TimeSpan.FromSeconds(5)
            );
        }
    }
}
