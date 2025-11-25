using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LolTime.App.Models;

namespace LolTime.App.Services;

public partial class TimeTrackerService : ObservableObject, IRecipient<GameStatusMessage>
{
    private readonly DataService _dataService;
    private readonly AlertService _alertService; // We'll create this next
    private DateTime? _currentSessionStart;
    private System.Threading.Timer _updateTimer;

    [ObservableProperty]
    private TimeSpan _todayTime;
    
    [ObservableProperty]
    private TimeSpan _weekTime;

    [ObservableProperty]
    private TimeSpan _weekendTime;

    [ObservableProperty]
    private TimeSpan _monthTime;

    [ObservableProperty]
    private TimeSpan _totalTime;

    [ObservableProperty]
    private TimeSpan _currentSessionDuration;

    public TimeTrackerService(DataService dataService, AlertService alertService)
    {
        _dataService = dataService;
        _alertService = alertService;
        WeakReferenceMessenger.Default.Register(this);
        
        CalculateStats();
        _updateTimer = new System.Threading.Timer(UpdateTick, null, 1000, 1000);
    }

    public void Receive(GameStatusMessage message)
    {
        if (message.IsRunning)
        {
            if (_currentSessionStart == null)
            {
                _currentSessionStart = DateTime.Now;
            }
        }
        else
        {
            if (_currentSessionStart != null)
            {
                var end = DateTime.Now;
                var session = new Session { Start = _currentSessionStart.Value, End = end };
                _dataService.Data.Sessions.Add(session);
                _dataService.SaveData();
                _currentSessionStart = null;
                CurrentSessionDuration = TimeSpan.Zero;
                CalculateStats();
            }
        }
    }

    private void UpdateTick(object? state)
    {
        if (_currentSessionStart != null)
        {
            CurrentSessionDuration = DateTime.Now - _currentSessionStart.Value;
            
            // Real-time stats update
            // We add current session to "Today" for display purposes without saving yet
            // Actually, let's just re-calculate stats including the current pending session
            
            CalculateStats(includeCurrent: true);
            CheckLimits();
        }
    }

    private void CalculateStats(bool includeCurrent = false)
    {
        var now = DateTime.Now;
        var today = now.Date;
        
        // Calculate start of week (Monday)
        var diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
        var startOfWeek = today.AddDays(-1 * diff);

        var sessions = _dataService.Data.Sessions;
        
        var todaySessions = sessions.Where(s => s.Start.Date == today).ToList();
        var weekSessions = sessions.Where(s => s.Start.Date >= startOfWeek).ToList();
        var weekendSessions = sessions.Where(s => s.Start.DayOfWeek == DayOfWeek.Saturday || s.Start.DayOfWeek == DayOfWeek.Sunday).ToList(); // All time weekends
        // "Time for weekend" usually means "This weekend" or "Weekends in general"?
        // Task says "Time for day", "Time for week", "Time for weekend", "Total".
        // Usually "Time for weekend" in a weekly context implies "This week's weekend".
        // But if today is Monday, weekend is 0. 
        // Let's assume "Time for weekend" means "Time played during weekends this week" (which happens only if today is Sat/Sun) OR just global "Weekend stats"?
        // Let's stick to "This week's weekend" for now as it fits the dashboard context.
        var thisWeekendSessions = weekSessions.Where(s => s.Start.DayOfWeek == DayOfWeek.Saturday || s.Start.DayOfWeek == DayOfWeek.Sunday).ToList();
        var monthSessions = sessions.Where(s => s.Start.Month == now.Month && s.Start.Year == now.Year).ToList();

        TimeSpan todaySum = TimeSpan.Zero;
        TimeSpan weekSum = TimeSpan.Zero;
        TimeSpan weekendSum = TimeSpan.Zero;
        TimeSpan monthSum = TimeSpan.Zero;
        TimeSpan totalSum = TimeSpan.Zero;

        foreach (var s in todaySessions) todaySum += s.Duration;
        foreach (var s in weekSessions) weekSum += s.Duration;
        foreach (var s in thisWeekendSessions) weekendSum += s.Duration;
        foreach (var s in monthSessions) monthSum += s.Duration;
        foreach (var s in sessions) totalSum += s.Duration;

        if (includeCurrent && _currentSessionStart != null)
        {
            var currentDur = DateTime.Now - _currentSessionStart.Value;
            todaySum += currentDur;
            weekSum += currentDur;
            monthSum += currentDur;
            totalSum += currentDur;
            if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
            {
                weekendSum += currentDur;
            }
        }

        TodayTime = todaySum;
        WeekTime = weekSum;
        WeekendTime = weekendSum;
        MonthTime = monthSum;
        TotalTime = totalSum;
    }

    private void CheckLimits()
    {
        var limits = _dataService.Data.Settings;
        bool alertNeeded = false;
        string message = limits.AlertMessage;

        // Check Session Limit
        if (limits.SessionLimit != TimeSpan.MaxValue && CurrentSessionDuration > limits.SessionLimit)
        {
            alertNeeded = true;
            message = $"{limits.AlertMessage}\n(Session Limit Exceeded)";
        }
        
        // Check Daily Limit
        if (limits.DailyLimit != TimeSpan.MaxValue && TodayTime > limits.DailyLimit)
        {
            alertNeeded = true;
            message = $"{limits.AlertMessage}\n(Daily Limit Exceeded)";
        }

        // Check Weekly Limit
        if (limits.WeeklyLimit != TimeSpan.MaxValue && WeekTime > limits.WeeklyLimit)
        {
            alertNeeded = true;
            message = $"{limits.AlertMessage}\n(Weekly Limit Exceeded)";
        }

        if (alertNeeded)
        {
            _alertService.TriggerAlert(message);
        }
    }
}

