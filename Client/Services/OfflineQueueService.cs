using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Timesheets.Shared;
using Newtonsoft.Json;

namespace timesheets.Services
{
    public class OfflineQueueService
    {
        private readonly string _filePath;

        public OfflineQueueService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _filePath = Path.Combine(appData, "timesheets_offline_queue.json");
        }

        public void Enqueue(TimeEntryModel entry)
        {
            try
            {
                string json = JsonConvert.SerializeObject(entry);
                File.AppendAllText(_filePath, json + Environment.NewLine);
            }
            catch (Exception) { /* Best effort */ }
        }

        public void EnqueueBatch(List<TimeEntryModel> entries)
        {
            try
            {
                 foreach(var entry in entries)
                 {
                     string json = JsonConvert.SerializeObject(entry);
                     File.AppendAllText(_filePath, json + Environment.NewLine);
                 }
            }
            catch (Exception) { /* Best effort */ }
        }

        public async Task ProcessQueueAsync(ApiService apiService)
        {
            if (!File.Exists(_filePath)) return;

            List<TimeEntryModel> allEntries = new List<TimeEntryModel>();

            try
            {
                var lines = File.ReadAllLines(_filePath);
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        try
                        {
                            var entry = JsonConvert.DeserializeObject<TimeEntryModel>(line);
                            if (entry != null) allEntries.Add(entry);
                        }
                        catch { /* Corrupt line */ }
                    }
                }
            }
            catch { return; } // File lock or other issue

            if (allEntries.Count == 0)
            {
                try { File.Delete(_filePath); } catch {}
                return;
            }

            // Try to submit in batch. Disable auto-queue to avoid duplication if it fails.
            bool success = await apiService.SubmitBatchAsync(allEntries, enableOfflineQueue: false);

            if (success)
            {
                try
                {
                    File.Delete(_filePath);
                }
                catch {}
            }
            // If failed, entries remain in file.
        }
    }
}
