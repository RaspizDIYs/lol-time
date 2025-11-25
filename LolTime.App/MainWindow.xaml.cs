using System.ComponentModel;
using System.Windows;
using LolTime.App.ViewModels;
using Wpf.Ui.Controls;

namespace LolTime.App;

public partial class MainWindow : FluentWindow
{
    // Флаг для реального закрытия приложения
    public bool CanClose { get; set; } = false;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // Если не был вызван явный Exit, просто скрываем окно
        if (!CanClose)
        {
            e.Cancel = true;
            this.Hide();
        }
        else
        {
            base.OnClosing(e);
        }
    }
}
