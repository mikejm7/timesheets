using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Timesheets.Shared;
using Newtonsoft.Json;

namespace timesheets.Services
{
    public class ApiService
    {
        private readonly HttpClient _client;

        public ApiService()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(GetBaseUrl());
        }

        private string GetBaseUrl()
        {
#if DEBUG
            return "https://localhost:7053/";
#else
            return "https://api.your-production-site.com/";
#endif
        }

        private class JobDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
        }

        public async Task<Dictionary<string, string>> GetJobsAsync()
        {
            try
            {
                var response = await _client.GetAsync("api/jobs");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var jobs = JsonConvert.DeserializeObject<List<JobDto>>(json);
                // Map Code to Name
                return jobs.ToDictionary(j => j.Code, j => j.Name);
            }
            catch (Exception)
            {
                // Fallback or empty if offline
                return new Dictionary<string, string>();
            }
        }

        public async Task<Dictionary<string, string>> GetTasksAsync(string jobId)
        {
            // API doesn't support tasks yet, return empty
            return await Task.FromResult(new Dictionary<string, string>());
        }

        public async Task<bool> SubmitTimeEntryAsync(TimeEntryModel entry)
        {
            try
            {
                string json = JsonConvert.SerializeObject(entry);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync("api/timesheets", content);
                if (response.IsSuccessStatusCode)
                {
                    entry.Status = "Submitted";
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> SubmitBatchAsync(List<TimeEntryModel> entries)
        {
             try
            {
                string json = JsonConvert.SerializeObject(entries);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync("api/timesheets/batch", content);
                if (response.IsSuccessStatusCode)
                {
                    foreach (var entry in entries)
                    {
                        entry.Status = "Submitted";
                    }
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> LockWeekAsync(DateTime weekStart)
        {
            // Mock logic
            await Task.Delay(500);
            return true;
        }
    }
}
