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
        private List<KeyValuePair<string, string>> _jobs = new List<KeyValuePair<string, string>>();
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
                var apiJobs = await _apiService.GetJobsAsync();
                _jobs = new List<KeyValuePair<string, string>>();

                // Load Recents
                var recents = Properties.Settings.Default.RecentJobIds;
                if (recents != null && recents.Count > 0)
                {
                    bool addedHeader = false;
                    foreach (string id in recents)
                    {
                        if (apiJobs.ContainsKey(id))
                        {
                            _jobs.Add(new KeyValuePair<string, string>(id, apiJobs[id]));
                            addedHeader = true;
                        }
                    }
                    if (addedHeader)
                    {
                        _jobs.Add(new KeyValuePair<string, string>("-1", "--- Recent ---"));
                    }
                }

                // Add All Jobs
                foreach (var kvp in apiJobs)
                {
                    _jobs.Add(kvp);
                }

                if (_jobs.Count > 0)
                {
                    _selectedJobId = _jobs.First().Key;
                    if (_selectedJobId == "-1" && _jobs.Count > 1) _selectedJobId = _jobs[1].Key; // Skip separator if selected by default
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
            return _jobs[index].Value;
        }

        public string GetJobID(Microsoft.Office.Core.IRibbonControl control, int index)
        {
            return _jobs[index].Key;
        }

        public int GetSelectedJobIndex(Microsoft.Office.Core.IRibbonControl control)
        {
            if (_selectedJobId == null) return 0;
            // Find index of selected ID.
            // Note: Since we have duplicates (recents), this might pick the first occurrence (recent).
            // That's fine/desired.
            for (int i = 0; i < _jobs.Count; i++)
            {
                if (_jobs[i].Key == _selectedJobId) return i;
            }
            return 0;
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
                if (string.IsNullOrEmpty(_selectedJobId) || _selectedJobId == "-1" || string.IsNullOrEmpty(_selectedTaskId))
                {
                    MessageBox.Show("Please select a valid Job and Task first.");
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
                        // Smart Guessing
                        string effectiveJobId = _selectedJobId;
                        string subject = appt.Subject ?? "";

                        // Find a job that matches subject
                        var matched = _jobs.FirstOrDefault(j => j.Key != "-1" && subject.IndexOf(j.Value, StringComparison.OrdinalIgnoreCase) >= 0);
                        if (!string.IsNullOrEmpty(matched.Key))
                        {
                            effectiveJobId = matched.Key;
                        }

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
                                Globals.ThisAddIn.SetUserProperty(movedItem, "JobID", effectiveJobId);
                                Globals.ThisAddIn.SetUserProperty(movedItem, "TaskID", _selectedTaskId);
                                Globals.ThisAddIn.SetUserProperty(movedItem, "TimeStatus", "Draft");
                                Globals.ThisAddIn.SetUserProperty(movedItem, "RT", rt);

                                // Update Visuals on the COPY
                                // We need Name for effectiveJobId. Lookup in _jobs.
                                string jobName = _jobs.FirstOrDefault(j => j.Key == effectiveJobId).Value ?? effectiveJobId;
                                string taskName = _tasks.ContainsKey(_selectedTaskId) ? _tasks[_selectedTaskId] : _selectedTaskId;

                                movedItem.Subject = $"{jobName} - {taskName}";
                                movedItem.Categories = "Draft Time";
                                movedItem.Save();

                                // Add to batch using the COPY's details
                                batchEntries.Add(new TimeEntryModel
                                {
                                    OutlookID = movedItem.EntryID,
                                    JobId = effectiveJobId,
                                    TaskId = _selectedTaskId,
                                    Date = movedItem.Start,
                                    RT = rt,
                                    TotalHours = rt,
                                    Status = "Draft"
                                });

                                // Add to Recents
                                AddToRecents(effectiveJobId);
                            }
                        }
                    }
                }

                if (batchEntries.Count > 0)
                {
                    await _apiService.SubmitBatchAsync(batchEntries);
                    MessageBox.Show($"Assigned {batchEntries.Count} items.");
                    Globals.ThisAddIn.UpdateDashboard();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error assigning items: " + ex.Message);
            }
        }

        private void AddToRecents(string id)
        {
            if (id == "-1") return;
            var settings = Properties.Settings.Default;
            if (settings.RecentJobIds == null) settings.RecentJobIds = new System.Collections.Specialized.StringCollection();

            var recents = settings.RecentJobIds;
            if (recents.Contains(id))
            {
                recents.Remove(id);
            }
            recents.Insert(0, id);

            while (recents.Count > 5)
            {
                recents.RemoveAt(recents.Count - 1);
            }

            settings.Save();
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
