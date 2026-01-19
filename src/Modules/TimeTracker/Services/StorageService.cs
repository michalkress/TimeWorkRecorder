using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Reflection;

namespace TimeWorkRecorder.Modules.TimeTracker.Services
{
    public class StorageService : IStorageService
    {
        private readonly string _dataFolder;
        private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

        public StorageService()
        {
            // Store data folder in application directory under 'data'
            var exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? AppContext.BaseDirectory;
            _dataFolder = Path.Combine(exePath, "data");
            Directory.CreateDirectory(_dataFolder);
        }

        public void SaveWorkDay(WorkDay workDay)
        {
            var file = Path.Combine(_dataFolder, $"{workDay.Date:yyyy-MM-dd}.json");
            var json = JsonSerializer.Serialize(workDay, _options);
            File.WriteAllText(file, json);
        }

        public WorkDay? LoadWorkDay(DateTime date)
        {
            var file = Path.Combine(_dataFolder, $"{date:yyyy-MM-dd}.json");
            if (!File.Exists(file))
                return null;
            var json = File.ReadAllText(file);
            return JsonSerializer.Deserialize<WorkDay>(json, _options);
        }

        public IEnumerable<WorkDay> LoadAll()
        {
            var files = Directory.EnumerateFiles(_dataFolder, "*.json")
                .Select(f => JsonSerializer.Deserialize<WorkDay>(File.ReadAllText(f), _options))
                .Where(w => w != null)!
                .Cast<WorkDay>()
                .OrderByDescending(w => w.Date);

            return files;
        }
    }
}
