using Prism.Ioc;
using Prism.Modularity;
using TimeWorkRecorder.Modules.TimeTracker.Services;
using TimeWorkRecorder.Modules.TimeTracker.Views;
using TimeWorkRecorder.Modules.TimeTracker.ViewModels;

namespace TimeWorkRecorder.Modules.TimeTracker
{
    public class TimeTrackerModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public TimeTrackerModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register the TimeTrackerService as a singleton
            containerRegistry.RegisterSingleton<ITimeTrackerService, TimeTrackerService>();
            // Register storage
            containerRegistry.RegisterSingleton<IStorageService, StorageService>();
            // Register the status bar view for the StatusRegion
            containerRegistry.RegisterForNavigation<StatusBarView, StatusBarViewModel>();
            // Register BreakDialog
            containerRegistry.RegisterDialog<Views.BreakDialog, ViewModels.BreakDialogViewModel>("BreakDialog");

        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            // Resolve and start the time tracker service so tracking begins when the module initializes
            var tracker = containerProvider.Resolve<ITimeTrackerService>();
            tracker?.Start();

            // Navigate the status bar into the StatusRegion so it shows on the shell
            _regionManager.RequestNavigate("StatusRegion", nameof(StatusBarView));
        }
    }
}
