using System.Collections.Generic;
using System.Threading.Tasks;
using Timesheets.Shared;
using timesheets.Services;

namespace Timesheets.Tests
{
    public class MockApiService : ApiService
    {
        public MockApiService(string baseUrl, string apiKey) : base(baseUrl, apiKey)
        {
        }

        public override Task<bool> SubmitBatchAsync(List<TimeEntryModel> entries, bool enableOfflineQueue = true)
        {
            return Task.FromResult(true);
        }
    }
}
