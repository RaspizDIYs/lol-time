using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LolTime.App.Views;

public partial class AlertWindow : Window
{
    public AlertWindow(string message, string imagePath)
    {
        InitializeComponent();
        AlertText.Text = message;

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
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}

