using Prism.Mvvm;
using Prism.Commands;
using System;
using System.Timers;
using TimeWorkRecorder.Modules.TimeTracker.Services;
using Timer = System.Timers.Timer;

namespace TimeWorkRecorder.Modules.TimeTracker.ViewModels
{
    public class StatusBarViewModel : BindableBase
    {
        private readonly ITimeTrackerService _timeTrackerService;
        private readonly Timer _timer;

        private string _statusText = "Idle";
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        private string _elapsed = "00:00:00";
        public string Elapsed
        {
            get => _elapsed;
            set => SetProperty(ref _elapsed, value);
        }

        public StatusBarViewModel(ITimeTrackerService timeTrackerService)
        {
            _timeTrackerService = timeTrackerService;

            _timer = new Timer(1000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();

            UpdateState();
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            // Ensure Tick and UI updates run on the UI thread
            try
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    // advance active minutes only when tracking is active
                    _timeTrackerService.Tick(TimeSpan.FromSeconds(1));
                    UpdateState();
                }));
            }
            catch
            {
                // if dispatcher not available, fallback to direct update
                _timeTrackerService.Tick(TimeSpan.FromSeconds(1));
                UpdateState();
            }
        }

        private void UpdateState()
        {
            if (_timeTrackerService.StartTime == default)
            {
                StatusText = "Idle";
                Elapsed = "00:00:00";
            }
            else
            {
                StatusText = _timeTrackerService.StopTime.HasValue ? "Stopped" : "Running";
                Elapsed = _timeTrackerService.GetElapsed().ToString(@"hh\:mm\:ss");
            }
        }
    }
}
