using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using timesheets.Models;
using Newtonsoft.Json;

namespace timesheets.Services
{
    public class ApiService
    {
        // Mock data
        private Dictionary<string, string> _jobs = new Dictionary<string, string>
        {
            { "J001", "Project Alpha" },
            { "J002", "Project Beta" },
            { "J003", "Internal Ops" }
        };

        private Dictionary<string, Dictionary<string, string>> _tasks = new Dictionary<string, Dictionary<string, string>>
        {
            { "J001", new Dictionary<string, string> { { "T01", "Design" }, { "T02", "Dev" } } },
            { "J002", new Dictionary<string, string> { { "T03", "Testing" }, { "T04", "Deploy" } } },
            { "J003", new Dictionary<string, string> { { "T05", "Meeting" }, { "T06", "Admin" } } }
        };

        public async Task<Dictionary<string, string>> GetJobsAsync()
        {
            await Task.Delay(500); // Simulate network latency
            return new Dictionary<string, string>(_jobs);
        }

        public async Task<Dictionary<string, string>> GetTasksAsync(string jobId)
        {
            await Task.Delay(300);
            if (_tasks.ContainsKey(jobId))
            {
                return new Dictionary<string, string>(_tasks[jobId]);
            }
            return new Dictionary<string, string>();
        }

        public async Task<bool> SubmitTimeEntryAsync(TimeEntryModel entry)
        {
            await Task.Delay(1000);
            string json = JsonConvert.SerializeObject(entry);
            // Simulate sending JSON to API
            // Console.WriteLine($"Submitting: {json}");
            entry.Status = "Submitted";
            return true;
        }

        public async Task<bool> SubmitBatchAsync(List<TimeEntryModel> entries)
        {
            await Task.Delay(1500);
            foreach (var entry in entries)
            {
                // Simulate processing
                entry.Status = "Submitted";
            }
            return true;
        }

        public async Task<bool> LockWeekAsync(DateTime weekStart)
        {
            await Task.Delay(500);
            // Simulate locking logic
            return true;
        }
    }
}
