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

    public AlertService(DataService dataService)
    {
        _dataService = dataService;
    }

    public void TriggerAlert(string message)
    {
        // Don't spam alerts. Wait at least 5 minutes between alerts if user ignores? 
        // Or if it's "Hard Mode", maybe every minute?
        // Let's say every 5 minutes for now.
        if ((DateTime.Now - _lastAlertTime).TotalMinutes < 5 && !_dataService.Data.Settings.HardMode)
        {
            return;
        }
        
        // If hard mode, maybe every 30 seconds?
        if ((DateTime.Now - _lastAlertTime).TotalSeconds < 30 && _dataService.Data.Settings.HardMode)
        {
             return;
        }

        _lastAlertTime = DateTime.Now;

        Application.Current.Dispatcher.Invoke(() =>
        {
            // Play Sound
            PlaySound();

            // Show Window
            var alertWindow = new AlertWindow(message, _dataService.Data.Settings.AlertImagePath);
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

