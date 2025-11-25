using System;
using System.Windows;
using Velopack;

namespace LolTime.App;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try 
        {
            // It's important to Run() the VelopackApp as early as possible.
            // If it returns true, the app should exit immediately.
            VelopackApp.Build()
                .WithFirstRun((v) => MessageBox.Show("Спасибо за установку LolTime!"))
                .Run();

            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fatal Error: {ex.Message}");
        }
    }
}

