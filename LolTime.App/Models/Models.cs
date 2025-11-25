using System;

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

public class AppSettings
{
    public TimeSpan DailyLimit { get; set; } = TimeSpan.FromHours(2);
    public TimeSpan WeeklyLimit { get; set; } = TimeSpan.FromHours(10);
    public TimeSpan SessionLimit { get; set; } = TimeSpan.MaxValue;
    
    public string AlertImagePath { get; set; } = "";
    public string AlertSoundPath { get; set; } = "";
    public string AlertMessage { get; set; } = "ЧО ЕБЛАН? ЕЩЁ ЧАСИК? 😠";
    
    public bool AutoStart { get; set; } = true;
    public bool HardMode { get; set; } = false;
}

