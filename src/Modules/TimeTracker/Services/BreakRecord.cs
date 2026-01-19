using System;

namespace TimeWorkRecorder.Modules.TimeTracker.Services
{
    public class BreakRecord
    {
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public string? Reason { get; set; }

        public TimeSpan Duration => (End ?? DateTime.Now) - Start;
    }
}
