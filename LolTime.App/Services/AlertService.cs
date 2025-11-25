using System.Windows;
using System.Windows.Media;
using System.IO;
using LolTime.App.Views; // We'll create this
using LolTime.App.Models;

namespace LolTime.App.Services;

public class AlertService
{
    private readonly DataService _dataService;
    private DateTime _lastAlertTime = DateTime.MinValue;
    private MediaPlayer _player = new MediaPlayer();
    private int _snoozeCount = 0;
    private bool _isSnoozed = false;

    public AlertService(DataService dataService)
    {
        _dataService = dataService;
    }

    public void TriggerAlert(string message)
    {
        double cooldownMinutes = 5;
        
        if (_isSnoozed)
        {
            cooldownMinutes = 10; // Snooze duration
        }
        else if (_dataService.Data.Settings.HardMode)
        {
            cooldownMinutes = 0.5; // 30 seconds
        }

        if ((DateTime.Now - _lastAlertTime).TotalMinutes < cooldownMinutes)
        {
            return;
        }

        _lastAlertTime = DateTime.Now;

        Application.Current.Dispatcher.Invoke(() =>
        {
            // Play Sound
            PlaySound();

            // Show Window
            var alertWindow = new AlertWindow(
                message, 
                _dataService.Data.Settings.AlertImagePath, 
                _snoozeCount,
                onSnooze: () => 
                {
                    _snoozeCount++;
                    _isSnoozed = true;
                    // Reset timer to now so the 10 min cooldown starts
                    _lastAlertTime = DateTime.Now; 
                },
                onClose: () =>
                {
                    _snoozeCount = 0;
                    _isSnoozed = false;
                }
            );
            
            alertWindow.Show();
            alertWindow.Activate();
            alertWindow.Topmost = true;
            alertWindow.Focus();
        });
    }

    private void PlaySound()
    {
        try 
        {
            string path = _dataService.Data.Settings.AlertSoundPath;
            if (File.Exists(path))
            {
                _player.Open(new Uri(path));
                _player.Play();
            }
            else
            {
                // Fallback sounds? Handled by specific path selection in settings ideally.
                // Or use system beep if no sound.
                System.Media.SystemSounds.Exclamation.Play();
            }
        }
        catch { }
    }
}
