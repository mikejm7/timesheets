using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Timesheets.Shared;
using timesheets.Services;

namespace Timesheets.Tests
{
    public class OfflineQueueConcurrencyTests
    {
        private readonly ITestOutputHelper _output;

        public OfflineQueueConcurrencyTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task RunRaceConditionTest()
        {
            _output.WriteLine("Starting Concurrency Stress Test on OfflineQueueService...");

            // Setup Services (Ensure ApiService is mocked to always return true for SubmitBatchAsync)
            var apiService = new MockApiService("https://localhost", "test-key");
            var queueService = new OfflineQueueService();

            int writerThreadCount = 9;
            int entriesPerWriter = 250;
            int totalExpected = writerThreadCount * entriesPerWriter;

            var tasks = new List<Task>();
            var cts = new CancellationTokenSource();

            int fileLockExceptions = 0;
            int totalQueued = 0;

            _output.WriteLine($"Spawning {writerThreadCount} writer threads...");
            _output.WriteLine($"Spawning 1 aggressive reader thread...");

            // 1. Start 9 Aggressive Writer Threads
            for (int i = 0; i < writerThreadCount; i++)
            {
                int threadId = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < entriesPerWriter; j++)
                    {
                        try
                        {
                            var entry = new TimeEntryModel
                            {
                                OutlookID = $"Thread{threadId}-Entry{j}",
                                JobId = "Job-101",
                                TaskId = "Testing",
                                RT = 1.0,
                                Date = DateTime.Now
                            };

                            queueService.Enqueue(entry);
                            Interlocked.Increment(ref totalQueued);
                        }
                        catch (Exception ex)
                        {
                            // If the lock fails, we will get IOExceptions here
                            Interlocked.Increment(ref fileLockExceptions);
                            _output.WriteLine($"Writer Exception: {ex.Message}");
                        }
                    }
                }));
            }

            // 2. Start 1 Aggressive Reader/Processor Thread
            var processorTask = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await queueService.ProcessQueueAsync(apiService);
                        // Tiny delay to ensure we overlap with writers
                        await Task.Delay(10);
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref fileLockExceptions);
                        _output.WriteLine($"Reader Exception: {ex.Message}");
                    }
                }
            });

            // 3. Wait for all writers to finish their loops
            await Task.WhenAll(tasks);

            // 4. Shut down the processor
            cts.Cancel();
            await processorTask;

            // 5. Run one final sweep to clear out the remaining file contents
            await queueService.ProcessQueueAsync(apiService);

            // 6. Results
            _output.WriteLine(new string('-', 50));
            _output.WriteLine("TEST COMPLETE");
            _output.WriteLine($"Total Entries Attempted: {totalExpected}");
            _output.WriteLine($"Total Entries Queued: {totalQueued}");
            _output.WriteLine($"Total File Lock Exceptions: {fileLockExceptions}");

            Assert.Equal(0, fileLockExceptions);
            Assert.Equal(totalExpected, totalQueued);
        }
    }
}
