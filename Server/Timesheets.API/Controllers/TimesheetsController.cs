using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timesheets.API.Data;
using Timesheets.Shared;

namespace Timesheets.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimesheetsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TimesheetsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<TimeEntryModel>> PostTimeEntry(TimeEntryModel timeEntry)
        {
            if (TimeEntryExists(timeEntry.OutlookID))
            {
                // Detach local instance if tracked (not an issue here as we create new context per request usually,
                // but checking Any doesn't track. However, if we loaded it, we need to handle tracking.)
                // Since we just check Any, we aren't tracking it.
                // But EntityState.Modified requires attaching.

                // Better: Check if it exists, if so update.
                _context.Update(timeEntry); // Update handles Attach and setting State to Modified
            }
            else
            {
                _context.TimeEntries.Add(timeEntry);
            }

            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTimeEntriesForWeek), new { date = timeEntry.Date.ToString("yyyy-MM-dd") }, timeEntry);
        }

        [HttpPost("batch")]
        public async Task<IActionResult> PostBatch(List<TimeEntryModel> entries)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                foreach (var entry in entries)
                {
                    if (TimeEntryExists(entry.OutlookID))
                    {
                        _context.Update(entry);
                    }
                    else
                    {
                        _context.TimeEntries.Add(entry);
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Error processing batch");
            }
        }

        [HttpGet("week/{date}")]
        public async Task<ActionResult<IEnumerable<TimeEntryModel>>> GetTimeEntriesForWeek(DateTime date)
        {
            // Assume Monday start
            var dayOfWeek = date.DayOfWeek;
            // If Sunday (0), we want it to be part of the previous week or start of new?
            // ISO8601 says Monday is 1. .NET DayOfWeek Sunday is 0.
            // Let's assume Monday is start.
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startOfWeek = date.Date.AddDays(-1 * diff);
            var endOfWeek = startOfWeek.AddDays(7);

            return await _context.TimeEntries
                .Where(t => t.Date >= startOfWeek && t.Date < endOfWeek)
                .ToListAsync();
        }

        private bool TimeEntryExists(string id)
        {
            return _context.TimeEntries.AsNoTracking().Any(e => e.OutlookID == id);
        }
    }
}
