using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LolTime.App.Models;

public class Session
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public TimeSpan Duration => End - Start;
}

public class DailyStats
{
    public DateTime Date { get; set; }
    public TimeSpan TotalTime { get; set; }
}

public class AppData
{
    public List<Session> Sessions { get; set; } = new();
    public AppSettings Settings { get; set; } = new();
}

public partial class AppSettings : ObservableObject
{
    [ObservableProperty]
    private TimeSpan _dailyLimit = TimeSpan.FromHours(2);

    [ObservableProperty]
    private TimeSpan _weeklyLimit = TimeSpan.FromHours(10);

    [ObservableProperty]
    private TimeSpan _sessionLimit = TimeSpan.MaxValue;
    
    [ObservableProperty]
    private string _alertImagePath = "";

    [ObservableProperty]
    private string _alertSoundPath = "";

    [ObservableProperty]
    private string _alertMessage = "ЧО ЕБЛАН? ЕЩЁ ЧАСИК?";
    
    [ObservableProperty]
    private bool _autoStart = true;

    [ObservableProperty]
    private bool _hardMode = false;
}
