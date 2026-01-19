using System;
using System.Collections.Generic;

namespace TimeWorkRecorder.Modules.TimeTracker.Services
{
    public interface IStorageService
    {
        void SaveWorkDay(WorkDay workDay);
        WorkDay? LoadWorkDay(DateTime date);
        IEnumerable<WorkDay> LoadAll();
    }
}
