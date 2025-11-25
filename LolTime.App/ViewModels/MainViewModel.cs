using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LolTime.App.Models;
using LolTime.App.Services;
using System.Collections.ObjectModel;
// using System.Windows.Forms;

namespace LolTime.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly DataService _dataService;
    private readonly TimeTrackerService _timeTracker;

    public TimeTrackerService Tracker => _timeTracker;

    [ObservableProperty]
    private AppSettings _settings;

    [ObservableProperty]
    private string _dailyLimitHours;

    [ObservableProperty]
    private string _weeklyLimitHours;
    
    // Stats for History Tab
    public ObservableCollection<DailyStats> History { get; } = new();

    public MainViewModel(DataService dataService, TimeTrackerService timeTracker)
    {
        _dataService = dataService;
        _timeTracker = timeTracker;
        Settings = _dataService.Data.Settings;

        DailyLimitHours = Settings.DailyLimit.TotalHours.ToString("0.##");
        WeeklyLimitHours = Settings.WeeklyLimit.TotalHours.ToString("0.##");
        
        LoadHistory();
    }

    private void LoadHistory()
    {
        var grouped = _dataService.Data.Sessions
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
    }

    [RelayCommand]
    private void BrowseImage()
    {
        var dlg = new Microsoft.Win32.OpenFileDialog();
        dlg.Filter = "Image files (*.png;*.jpg)|*.png;*.jpg";
        if (dlg.ShowDialog() == true)
        {
            Settings.AlertImagePath = dlg.FileName;
            OnPropertyChanged(nameof(Settings)); // Force update
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
            OnPropertyChanged(nameof(Settings));
        }
    }
}

