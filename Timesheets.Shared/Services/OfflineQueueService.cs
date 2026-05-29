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
        private static readonly object _lock = new object();

        public OfflineQueueService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _filePath = Path.Combine(appData, "timesheets_offline_queue.json");
        }

        public void Enqueue(TimeEntryModel entry)
        {
            lock (_lock)
            {
                try
                {
                    string json = JsonConvert.SerializeObject(entry);
                    File.AppendAllText(_filePath, json + Environment.NewLine);
                }
                catch (Exception) { /* Best effort */ }
            }
        }

        public void EnqueueBatch(List<TimeEntryModel> entries)
        {
            lock (_lock)
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
        }

        public async Task ProcessQueueAsync(ApiService apiService)
        {
            string processingPath = _filePath + ".processing";
            bool hasProcessingFile = false;

            lock (_lock)
            {
                // Recover from previous crash
                if (File.Exists(processingPath))
                {
                    try
                    {
                        var leftover = File.ReadAllText(processingPath);
                        File.AppendAllText(_filePath, leftover);
                        File.Delete(processingPath);
                    }
                    catch { return; }
                }

                if (File.Exists(_filePath))
                {
                    try
                    {
                        File.Move(_filePath, processingPath);
                        hasProcessingFile = true;
                    }
                    catch { return; }
                }
            }

            if (!hasProcessingFile) return;

            List<TimeEntryModel> allEntries = new List<TimeEntryModel>();

            try
            {
                var lines = File.ReadAllLines(processingPath);
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
            catch { return; }

            if (allEntries.Count == 0)
            {
                try { File.Delete(processingPath); } catch {}
                return;
            }

            bool success = await apiService.SubmitBatchAsync(allEntries, enableOfflineQueue: false);

            if (success)
            {
                try
                {
                    File.Delete(processingPath);
                }
                catch {}
            }
            else
            {
                // Restore data
                lock (_lock)
                {
                    try
                    {
                        var content = File.ReadAllText(processingPath);
                        File.AppendAllText(_filePath, content);
                        File.Delete(processingPath);
                    }
                    catch { /* Best effort */ }
                }
            }
        }
    }
}
