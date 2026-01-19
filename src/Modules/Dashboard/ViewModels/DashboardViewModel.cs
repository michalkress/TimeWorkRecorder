using Prism.Mvvm;

namespace TimeWorkRecorder.Modules.Dashboard.ViewModels
{
    public class DashboardViewModel : BindableBase
    {
        private string _title = "Dashboard";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public DashboardViewModel()
        {
        }
    }
}