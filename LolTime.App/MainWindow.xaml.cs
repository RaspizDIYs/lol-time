using System;
using System.ComponentModel;
using System.Windows;
using LolTime.App.ViewModels;

namespace LolTime.App;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
        this.Hide();
    }
}
