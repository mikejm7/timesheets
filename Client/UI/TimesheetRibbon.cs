using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Office.Tools.Ribbon;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Office.Core;
using timesheets.Services;
using Timesheets.Shared;
using System.Windows.Forms;
using System.Reflection;

namespace timesheets.UI
{
    [ComVisible(true)]
    public class TimesheetRibbon : Microsoft.Office.Core.IRibbonExtensibility
    {
        private Microsoft.Office.Core.IRibbonUI ribbon;
        private ApiService _apiService;

        // Cache for dropdowns
        private Dictionary<string, string> _jobs = new Dictionary<string, string>();
        private Dictionary<string, string> _tasks = new Dictionary<string, string>();

        private string _selectedJobId;
        private string _selectedTaskId;

        public TimesheetRibbon()
        {
            _apiService = new ApiService();
        }

        #region IRibbonExtensibility Members

        public string GetCustomUI(string ribbonID)
        {
            return GetResourceText("timesheets.UI.TimesheetRibbon.xml");
        }

        #endregion

        #region Ribbon Callbacks

        public void Ribbon_Load(Microsoft.Office.Core.IRibbonUI ribbonUI)
        {
            this.ribbon = ribbonUI;
            LoadJobs();
        }

        private async void LoadJobs()
        {
            try
            {
                _jobs = await _apiService.GetJobsAsync();
                if (_jobs.Count > 0)
                {
                    _selectedJobId = _jobs.Keys.First();
                    await LoadTasks(_selectedJobId);
                }
                ribbon.InvalidateControl("cmbActiveJob");
            }
            catch { }
        }

        private async System.Threading.Tasks.Task LoadTasks(string jobId)
        {
            try
            {
                _tasks = await _apiService.GetTasksAsync(jobId);
                if (_tasks.Count > 0)
                    _selectedTaskId = _tasks.Keys.First();
                else
                    _selectedTaskId = null;

                ribbon.InvalidateControl("cmbActiveTask");
            }
            catch { }
        }

        // Dropdown: Jobs
        public int GetJobCount(Microsoft.Office.Core.IRibbonControl control)
        {
            return _jobs.Count;
        }

        public string GetJobLabel(Microsoft.Office.Core.IRibbonControl control, int index)
        {
            return _jobs.Values.ElementAt(index);
        }

        public string GetJobID(Microsoft.Office.Core.IRibbonControl control, int index)
        {
            return _jobs.Keys.ElementAt(index);
        }

        public int GetSelectedJobIndex(Microsoft.Office.Core.IRibbonControl control)
        {
            if (_selectedJobId == null) return 0;
            return _jobs.Keys.ToList().IndexOf(_selectedJobId);
        }

        public async void OnJobChanged(Microsoft.Office.Core.IRibbonControl control, string selectedId, int index)
        {
            _selectedJobId = selectedId;
            await LoadTasks(selectedId);
        }

        // Dropdown: Tasks
        public int GetTaskCount(Microsoft.Office.Core.IRibbonControl control)
        {
            return _tasks.Count;
        }

        public string GetTaskLabel(Microsoft.Office.Core.IRibbonControl control, int index)
        {
            return _tasks.Values.ElementAt(index);
        }

        public string GetTaskID(Microsoft.Office.Core.IRibbonControl control, int index)
        {
            return _tasks.Keys.ElementAt(index);
        }

        public int GetSelectedTaskIndex(Microsoft.Office.Core.IRibbonControl control)
        {
            if (_selectedTaskId == null) return 0;
            return _tasks.Keys.ToList().IndexOf(_selectedTaskId);
        }

        public void OnTaskChanged(Microsoft.Office.Core.IRibbonControl control, string selectedId, int index)
        {
            _selectedTaskId = selectedId;
        }

        // Button: Assign Selected
        public async void AssignSelected_Click(Microsoft.Office.Core.IRibbonControl control)
        {
            try
            {
                if (string.IsNullOrEmpty(_selectedJobId) || string.IsNullOrEmpty(_selectedTaskId))
                {
                    MessageBox.Show("Please select a Job and Task first.");
                    return;
                }

                Microsoft.Office.Interop.Outlook.Explorer explorer = Globals.ThisAddIn.Application.ActiveExplorer();
                if (explorer == null || explorer.Selection.Count == 0) return;

                Outlook.MAPIFolder timesheetFolder = Globals.ThisAddIn.GetTimesheetFolder();
                if (timesheetFolder == null)
                {
                    MessageBox.Show("Could not find or create Timesheets folder.");
                    return;
                }

                List<TimeEntryModel> batchEntries = new List<TimeEntryModel>();

                foreach (object item in explorer.Selection)
                {
                    if (item is AppointmentItem appt)
                    {
                        // Create Copy
                        AppointmentItem copy = appt.Copy() as AppointmentItem;
                        if (copy != null)
                        {
                            // Move to Timesheets folder
                            AppointmentItem movedItem = copy.Move(timesheetFolder) as AppointmentItem;

                            if (movedItem != null)
                            {
                                // Calculate RT (Duration is in minutes, convert to hours)
                                double rt = movedItem.Duration / 60.0;

                                // Update Properties on the COPY
                                Globals.ThisAddIn.SetUserProperty(movedItem, "JobID", _selectedJobId);
                                Globals.ThisAddIn.SetUserProperty(movedItem, "TaskID", _selectedTaskId);
                                Globals.ThisAddIn.SetUserProperty(movedItem, "TimeStatus", "Draft");
                                Globals.ThisAddIn.SetUserProperty(movedItem, "RT", rt);

                                // Update Visuals on the COPY
                                string jobName = _jobs.ContainsKey(_selectedJobId) ? _jobs[_selectedJobId] : _selectedJobId;
                                string taskName = _tasks.ContainsKey(_selectedTaskId) ? _tasks[_selectedTaskId] : _selectedTaskId;

                                movedItem.Subject = $"{jobName} - {taskName}";
                                movedItem.Categories = "Draft Time";
                                movedItem.Save();

                                // Add to batch using the COPY's details
                                batchEntries.Add(new TimeEntryModel
                                {
                                    OutlookID = movedItem.EntryID,
                                    JobId = _selectedJobId,
                                    TaskId = _selectedTaskId,
                                    Date = movedItem.Start,
                                    RT = rt,
                                    TotalHours = rt,
                                    Status = "Draft"
                                });
                            }
                        }
                    }
                }

                if (batchEntries.Count > 0)
                {
                    await _apiService.SubmitBatchAsync(batchEntries);
                    MessageBox.Show($"Assigned {batchEntries.Count} items.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error assigning items: " + ex.Message);
            }
        }

        // Button: Submit Week
        public async void SubmitWeek_Click(Microsoft.Office.Core.IRibbonControl control)
        {
            try
            {
                // Implementation for submitting week
                // Logic would involve finding all items in range and submitting them
                await _apiService.LockWeekAsync(DateTime.Now);
                MessageBox.Show("Week submitted (Mock).");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error submitting week: " + ex.Message);
            }
        }

        #endregion

        #region Helpers

        private static string GetResourceText(string resourceName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resourceNames = asm.GetManifestResourceNames();
            for (int i = 0; i < resourceNames.Length; ++i)
            {
                if (string.Compare(resourceName, resourceNames[i], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    using (System.IO.StreamReader resourceReader = new System.IO.StreamReader(asm.GetManifestResourceStream(resourceNames[i])))
                    {
                        if (resourceReader != null)
                        {
                            return resourceReader.ReadToEnd();
                        }
                    }
                }
            }
            return null;
        }

        private string GetUserProperty(AppointmentItem item, string name)
        {
            UserProperty prop = item.UserProperties.Find(name, true);
            if (prop != null) return prop.Value.ToString();
            return null;
        }

        #endregion
    }
}
