using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace timesheets.Models
{
    public class TimeEntryModel
    {
        public string OutlookID { get; set; }
        public string JobId { get; set; }
        public string TaskId { get; set; }
        public DateTime Date { get; set; }

        // Calculated property or updated by logic
        public double TotalHours { get; set; }

        public double RT { get; set; }
        public double OT { get; set; }
        public double DT { get; set; }
        public double Travel { get; set; }

        public string Notes { get; set; }
        public string Status { get; set; } // "Draft" or "Submitted"

        public TimeEntryModel()
        {
            Status = "Draft";
            RT = 0;
            OT = 0;
            DT = 0;
            Travel = 0;
            TotalHours = 0;
        }
    }
}
