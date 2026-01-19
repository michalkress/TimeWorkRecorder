using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using System.Windows;
using TimeWorkRecorder.Modules.TimeTracker.Services;

namespace TimeWorkRecorder
{
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            // Resolve the shell (MainWindow) from the Prism/Unity container
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // App-level registrations
            containerRegistry.RegisterSingleton<IStorageService, StorageService>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            // Register DashboardModule so Prism will load it
            moduleCatalog.AddModule<Modules.Dashboard.DashboardModule>();

            // Register TimeTrackerModule so Prism will load it
            moduleCatalog.AddModule<Modules.TimeTracker.TimeTrackerModule>();
        }
    }
}
