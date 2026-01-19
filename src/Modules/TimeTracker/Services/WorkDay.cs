using System;
using System.Collections.Generic;

namespace TimeWorkRecorder.Modules.TimeTracker.Services
{
    public class WorkDay
    {
        // The date this workday represents (date portion only)
        public DateTime Date { get; set; }

        // When the user logged in
        public DateTime LoginTime { get; set; }

        // When tracking started
        public DateTime StartTime { get; set; }

        // When tracking stopped (if any)
        public DateTime? StopTime { get; set; }

        // Active minutes excluding breaks
        public double ActiveMinutes { get; set; }

        // Break records during the day
        public List<BreakRecord> Breaks { get; set; } = new();
    }
}
