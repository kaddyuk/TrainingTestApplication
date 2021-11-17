using EnvisionClient.Data;
using EnvisionClient.Data.Contexts;
using EnvisionClient.Data.Interfaces;
using System.Windows;
using TrainingTestApplication.ViewModels;

namespace TrainingTestApplication;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public IDbContextFactory<TechRecordsDbContext> DbContextFactory { get; set; }
    public App()
    {
        var connString = "Server=(local);Initial Catalog=ENVATS;Integrated Security=true";
        DbContextFactory = new DbContextFactory<TechRecordsDbContext>(connString);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var vm = new MainWindowViewModel(DbContextFactory);

        var window = new MainWindow { DataContext = vm };
        MainWindow = window;

        window.Show();

    }
}
