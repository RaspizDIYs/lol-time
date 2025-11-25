using System;
using System.Windows;

namespace LolTime.App.Views;

public partial class ErrorWindow : Window
{
    private readonly string _details;

    private ErrorWindow(string message, string details)
    {
        InitializeComponent();
        _details = details;
        DetailsBox.Text = $"{message}{Environment.NewLine}{Environment.NewLine}{details}";
    }

    public static void Show(Exception ex)
    {
        var message = ex?.Message ?? "Неизвестная ошибка";
        var details = ex?.ToString() ?? message;

        if (Application.Current?.Dispatcher != null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var window = new ErrorWindow(message, details);
                window.ShowDialog();
            });
        }
        else
        {
            var window = new ErrorWindow(message, details);
            window.ShowDialog();
        }
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(_details);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

