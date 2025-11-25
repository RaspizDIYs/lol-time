using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LolTime.App.Models;

namespace LolTime.App.Services;

public class SessionEndedMessage(Session session)
{
    public Session Session { get; } = session;
}

public partial class TimeTrackerService : ObservableObject, IRecipient<GameStatusMessage>, IDisposable
{
    private readonly DataService _dataService;
    private readonly AlertService _alertService;
    private DateTime? _currentSessionStart;
    private readonly Timer _updateTimer;
    private bool _disposed;

    [ObservableProperty] private TimeSpan _todayTime;
    [ObservableProperty] private TimeSpan _weekTime;
    [ObservableProperty] private TimeSpan _weekendTime;
    [ObservableProperty] private TimeSpan _monthTime;
    [ObservableProperty] private TimeSpan _totalTime;
    [ObservableProperty] private TimeSpan _currentSessionDuration;

    public TimeTrackerService(DataService dataService, AlertService alertService)
    {
        _dataService = dataService;
        _alertService = alertService;
        WeakReferenceMessenger.Default.Register(this);
        
        CalculateStats();
        _updateTimer = new Timer(UpdateTick, null, 1000, 1000);
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
                
                _dataService.AddSession(session);
                
                _currentSessionStart = null;
                CurrentSessionDuration = TimeSpan.Zero;
                CalculateStats();
                
                WeakReferenceMessenger.Default.Send(new SessionEndedMessage(session));
            }
        }
    }

    private void UpdateTick(object? state)
    {
        if (_disposed) return;

        if (_currentSessionStart != null)
        {
            CurrentSessionDuration = DateTime.Now - _currentSessionStart.Value;
            CalculateStats(includeCurrent: true);
            CheckLimits();
        }
    }

    private void CalculateStats(bool includeCurrent = false)
    {
        var now = DateTime.Now;
        var today = now.Date;
        
        var diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
        var startOfWeek = today.AddDays(-1 * diff);

        var sessions = _dataService.GetSessionsSnapshot();
        
        var todaySessions = sessions.Where(s => s.Start.Date == today);
        var weekSessions = sessions.Where(s => s.Start.Date >= startOfWeek);
        var weekendSessions = weekSessions.Where(s => s.Start.DayOfWeek == DayOfWeek.Saturday || s.Start.DayOfWeek == DayOfWeek.Sunday);
        var monthSessions = sessions.Where(s => s.Start.Month == now.Month && s.Start.Year == now.Year);

        TimeSpan todaySum = TimeSpan.Zero;
        foreach(var s in todaySessions) todaySum += s.Duration;
        
        TimeSpan weekSum = TimeSpan.Zero;
        foreach(var s in weekSessions) weekSum += s.Duration;

        TimeSpan weekendSum = TimeSpan.Zero;
        foreach(var s in weekendSessions) weekendSum += s.Duration;
        
        TimeSpan monthSum = TimeSpan.Zero;
        foreach(var s in monthSessions) monthSum += s.Duration;

        TimeSpan totalSum = TimeSpan.Zero;
        foreach(var s in sessions) totalSum += s.Duration;

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

        if (limits.SessionLimit != TimeSpan.MaxValue && CurrentSessionDuration > limits.SessionLimit)
        {
            alertNeeded = true;
            message = $"{limits.AlertMessage}\n(Session Limit Exceeded)";
        }
        
        if (limits.DailyLimit != TimeSpan.MaxValue && TodayTime > limits.DailyLimit)
        {
            alertNeeded = true;
            message = $"{limits.AlertMessage}\n(Daily Limit Exceeded)";
        }

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

    public void Dispose()
    {
        _disposed = true;
        _updateTimer?.Dispose();
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}
