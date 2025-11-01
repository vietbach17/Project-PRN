using System.Configuration;
using System.Data;
using System.Windows;
using HotelManagementBLL;
using System;

namespace ManagementHotel
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Prevent app shutdown when LoginWindow closes
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var login = new LoginWindow();
            login.Closed += (_, __) =>
            {
                // After login window closes, decide to open main or shutdown
                if (AppSession.CurrentUser != null)
                {
                    try
                    {
                        var main = new MainWindow();
                        this.MainWindow = main;
                        this.ShutdownMode = ShutdownMode.OnMainWindowClose;
                        main.Show();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Startup error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Shutdown();
                    }
                }
                else
                {
                    Shutdown();
                }
            };
            login.Show();
        }
    }

}
