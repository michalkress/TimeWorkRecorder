using System;
using System.Collections.Generic;

namespace TimeWorkRecorder.Modules.TimeTracker.Services
{
    public interface ITimeTrackerService
    {
        DateTime StartTime { get; }
        DateTime? StopTime { get; }

        // System login time
        DateTime LoginTime { get; }

        // Total active minutes excluding breaks
        double ActiveMinutes { get; }

        // Recorded breaks since start
        IReadOnlyList<BreakRecord> Breaks { get; }

        void Start();
        void Stop();
        TimeSpan GetElapsed();
        void Tick(TimeSpan interval);
        IEnumerable<WorkDay> LoadAll();
    }
}
