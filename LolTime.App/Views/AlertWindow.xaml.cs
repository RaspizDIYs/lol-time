using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LolTime.App.Views;

public partial class AlertWindow : Window
{
    private readonly Action? _onSnooze;
    private readonly Action? _onClose;

    public AlertWindow(string message, string imagePath, int snoozeLevel, Action? onSnooze = null, Action? onClose = null)
    {
        InitializeComponent();
        AlertText.Text = message;
        _onSnooze = onSnooze;
        _onClose = onClose;

        if (!string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath))
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                AlertImage.Source = bitmap;
            }
            catch { }
        }

        UpdateSnoozeButton(snoozeLevel);
    }

    // Default constructor for XAML designer compatibility (optional but good practice)
    public AlertWindow() : this("Test Message", "", 0) { }

    private void UpdateSnoozeButton(int level)
    {
        string[] phrases = new[]
        {
            "Ещё 10 минуточек",
            "Уверен?",
            "Оно тебе надо?",
            "Серьезно?",
            "Чел, ты...",
            "Может хватит?",
            "Еблан, куда ещё?",
            "ЗАВЯЗЫВАЙ ДАВАЙ",
            "ВЫРУБАЙ НАХУЙ",
            "Я УДАЛЮ ВИНДУ",
            "ПОТРОГАЙ ТРАВУ"
        };

        string text = level < phrases.Length ? phrases[level] : "СДОХНУТЬ ХОЧЕШЬ?";
        SnoozeButton.Content = text;
        
        // Make it redder/scarier as level increases
        if (level > 5)
        {
            SnoozeButton.Background = System.Windows.Media.Brushes.DarkRed;
            SnoozeButton.FontWeight = FontWeights.Bold;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        _onClose?.Invoke();
        this.Close();
    }

    private void SnoozeButton_Click(object sender, RoutedEventArgs e)
    {
        _onSnooze?.Invoke();
        this.Close();
    }
}
