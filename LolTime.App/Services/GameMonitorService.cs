using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;

namespace LolTime.App.Services;

public class GameStatusMessage
{
    public bool IsRunning { get; }
    public GameStatusMessage(bool isRunning) => IsRunning = isRunning;
}

public class GameMonitorService
{
    private readonly Timer _timer;
    private bool _wasRunning = false;
    private const int PollInterval = 1000; // 1 second

    public bool IsGameRunning { get; private set; }

    public GameMonitorService()
    {
        _timer = new Timer(CheckProcess, null, 0, PollInterval);
    }

    private void CheckProcess(object? state)
    {
        var processes = Process.GetProcesses();
        var isRunning = processes.Any(p => 
            p.ProcessName.Equals("LeagueClient", StringComparison.OrdinalIgnoreCase) || 
            p.ProcessName.Equals("League of Legends", StringComparison.OrdinalIgnoreCase));

        if (isRunning != _wasRunning)
        {
            _wasRunning = isRunning;
            IsGameRunning = isRunning;
            WeakReferenceMessenger.Default.Send(new GameStatusMessage(isRunning));
        }
    }
}

