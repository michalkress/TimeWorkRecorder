using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using Prism.Dialogs;
using Prism;
using System.Windows;

namespace TimeWorkRecorder.Modules.TimeTracker.Services
{
    public class TimeTrackerService : ITimeTrackerService
    {
        public DateTime StartTime { get; private set; }
        public DateTime? StopTime { get; private set; }

        public DateTime LoginTime { get; private set; }

        private readonly IStorageService _storageService;
        private readonly IDialogService _dialogService;

        private readonly List<BreakRecord> _breaks = new(); 
        public IReadOnlyList<BreakRecord> Breaks => _breaks.AsReadOnly();

        private DateTime? _currentBreakStart;

        private readonly object _sync = new();

        private double _activeMinutes;
        private bool _isLocked;

        public double ActiveMinutes
        {
            get
            {
                lock (_sync)
                {
                    return _activeMinutes;
                }
            }
        }

        // Called periodically to update internal counters
        public void Tick(TimeSpan interval)
        {
            lock (_sync)
            {
                // Only count active minutes if tracking has started, not stopped and not locked
                if (StartTime != default && !StopTime.HasValue && !_isLocked)
                {
                    _activeMinutes += interval.TotalMinutes;
                }
            }
        }

        public TimeTrackerService(IStorageService storageService, IDialogService dialogService)
        {
            _storageService = storageService;
            _dialogService = dialogService;
            StartTime = DateTime.MinValue;
            StopTime = null;

            // Capture initial login time
            LoginTime = DateTime.Now;

            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            SystemEvents.SessionEnding += SystemEvents_SessionEnding;            
        }

        private void SystemEvents_SessionEnding(object? sender, SessionEndingEventArgs e)
        {
            // Ensure tracking is stopped and saved when the session is ending (logoff/shutdown)
            try
            {
                Stop();
                SaveCurrentWorkDay();
            }
            catch { }
        }

        private void SystemEvents_PowerModeChanged(object? sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend)
            {
                // Start break on suspend
                StopTime = DateTime.Now;
                BeginBreak("Suspend");
            }
            else if (e.Mode == PowerModes.Resume)
            {
                // Resume tracking after resume
                EndBreak("Resume");
                // clear StopTime so tracking continues
                StopTime = null;
            }
        }

        private void SystemEvents_SessionSwitch(object? sender, SessionSwitchEventArgs e)
        {
            // Handle session events: logon, lock, unlock
            if (e.Reason == SessionSwitchReason.SessionLogon)
            {
                // Start tracking on initial logon if not already started
                if (StartTime == default)
                    StartTime = DateTime.Now;
            }
            else if (e.Reason == SessionSwitchReason.SessionLock)
            {
                // mark stop time when session is locked and mark locked state
                StopTime = DateTime.Now;
                lock (_sync) { _isLocked = true; }
                BeginBreak("Lock");
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                // If tracking hasn't started yet, start now
                if (StartTime == default)
                    StartTime = DateTime.Now;

                // clear StopTime so tracking resumes after unlock and clear locked flag
                StopTime = null;
                lock (_sync) { _isLocked = false; }

                EndBreak("Unlock");
            }
            else if (e.Reason == SessionSwitchReason.SessionLogoff)
            {
                // mark stop time on logoff
                StopTime = DateTime.Now;
            }
        }

        private void BeginBreak(string reason)
        {
            lock (_sync)
            {
                if (_currentBreakStart.HasValue)
                    return; // already in break
                _currentBreakStart = DateTime.Now;
                _breaks.Add(new BreakRecord { Start = _currentBreakStart.Value, Reason = reason });
            }
        }

        private void EndBreak(string reason)
        {
            lock (_sync)
            {
                if (!_currentBreakStart.HasValue)
                    return;
                var start = _currentBreakStart.Value;
                var end = DateTime.Now;
                _currentBreakStart = null;
                var last = _breaks.LastOrDefault();
                if (last != null && last.Start == start)
                {
                    last.End = end;

                    var duration = last.Duration;
                    // If break longer than 15 minutes, ask the user via BreakDialog on UI thread
                    if (duration > TimeSpan.FromMinutes(15) && _dialogService != null)
                    {
                        try
                        {
                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                var parameters = new DialogParameters
                                {
                                    { "Start", last.Start },
                                    { "End", last.End },
                                    { "Duration", duration }
                                };

                                _dialogService.ShowDialog("BreakDialog", parameters, r =>
                                {
                                    lock (_sync)
                                    {
                                        if (r?.Parameters != null && last != null)
                                        {
                                            if (r.Result == ButtonResult.OK && r.Parameters.ContainsKey("Reason"))
                                            {
                                                last.Reason = r.Parameters.GetValue<string>("Reason");
                                            }
                                            else
                                            {
                                                // default reason
                                                last.Reason = reason;
                                            }
                                        }
                                    }
                                });
                            }));
                        }
                        catch
                        {
                            last.Reason = reason;
                        }
                    }
                    else
                    {
                        // short break, just set reason
                        last.Reason = reason;
                    }
                }
                else
                {
                    _breaks.Add(new BreakRecord { Start = start, End = end, Reason = reason });
                }
            }
        }

        public void Start()
        {
            StartTime = DateTime.Now;
            StopTime = null;
            LoginTime = DateTime.Now;
            lock (_sync)
            {
                _breaks.Clear();
                _currentBreakStart = null;
            }
        }

        public void Stop()
        {
            StopTime = DateTime.Now;
            // Persist workday when stopping
            SaveCurrentWorkDay();
        }

        public TimeSpan GetElapsed()
        {
            var end = StopTime ?? DateTime.Now;
            if (StartTime == DateTime.MinValue)
                return TimeSpan.Zero;
            return end - StartTime;
        }

        public IEnumerable<WorkDay> LoadAll()
        {
            try
            {
                return _storageService?.LoadAll() ?? Enumerable.Empty<WorkDay>();
            }
            catch
            {
                return Enumerable.Empty<WorkDay>();
            }
        }

        public void AddBreakEntry(DateTime start, DateTime end, string type)
        {
            lock (_sync)
            {
                var entry = new BreakRecord { Start = start, End = end, Reason = type };
                _breaks.Add(entry);
            }

            // persist updated workday
            try
            {
                SaveCurrentWorkDay();
            }
            catch { }
        }

        // Optional cleanup
        ~TimeTrackerService()
        {
            try
            {
                SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
                SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
                SystemEvents.SessionEnding -= SystemEvents_SessionEnding;
            }
            catch { }
        }

        private void SaveCurrentWorkDay()
        {
            try
            {
                if (StartTime == DateTime.MinValue)
                    return; // nothing to save

                var workDay = new WorkDay
                {
                    Date = StartTime.Date,
                    LoginTime = LoginTime,
                    StartTime = StartTime,
                    StopTime = StopTime,
                    ActiveMinutes = ActiveMinutes,
                    Breaks = _breaks.Select(b => new BreakRecord { Start = b.Start, End = b.End, Reason = b.Reason }).ToList()
                };

                _storageService?.SaveWorkDay(workDay);
            }
            catch { }
        }
    }
}
