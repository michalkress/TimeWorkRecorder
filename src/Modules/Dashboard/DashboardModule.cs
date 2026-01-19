using Prism.Ioc;
using Prism.Modularity;
using TimeWorkRecorder.Modules.Dashboard.Views;
using TimeWorkRecorder.Modules.Dashboard.ViewModels;

namespace TimeWorkRecorder.Modules.Dashboard
{
    public class DashboardModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public DashboardModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register the DashboardView for navigation and associate the VM
            containerRegistry.RegisterForNavigation<DashboardView, DashboardViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            // Navigate to the dashboard in the MainRegion when the module initializes
            _regionManager.RequestNavigate("MainRegion", nameof(DashboardView));
        }
    }
}