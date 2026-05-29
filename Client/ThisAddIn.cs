using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Outlook = Microsoft.Office.Interop.Outlook;
using Office = Microsoft.Office.Core;
using timesheets.Services;
using timesheets.Forms;
using Timesheets.Shared;
using System.Windows.Forms;

namespace timesheets
{
    public partial class ThisAddIn
    {
        private Outlook.Inspectors _inspectors;
        private Outlook.Items _timesheetItems;
        private ApiService _apiService;
        private bool _isUpdating = false;
        private System.Windows.Forms.Timer _offlineTimer;
        private Microsoft.Office.Tools.CustomTaskPane _dashboardPane;
        private UI.DashboardControl _dashboardControl;

        public ApiService ApiService => _apiService;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            // Initialize ApiService
            _apiService = new ApiService(Properties.Settings.Default.ApiBaseUrl, Properties.Settings.Default.ApiKey);

            // Offline Timer
            _offlineTimer = new System.Windows.Forms.Timer();
            _offlineTimer.Interval = 300000; // 5 mins
            _offlineTimer.Tick += async (s, ev) =>
            {
                _offlineTimer.Stop();
                try
                {
                    await _apiService.ProcessOfflineQueueAsync();
                }
                finally
                {
                    _offlineTimer.Start();
                }
            };
            _offlineTimer.Start();

            // Dashboard
            _dashboardControl = new UI.DashboardControl();
            _dashboardPane = this.CustomTaskPanes.Add(_dashboardControl, "Weekly Progress");
            _dashboardPane.DockPosition = Office.MsoCTPDockPosition.msoCTPDockPositionRight;
            _dashboardPane.Visible = true;

            // Hook Inspectors to intercept opening items
            _inspectors = this.Application.Inspectors;
            _inspectors.NewInspector += Inspectors_NewInspector;

            // Hook Calendar Items for Smart Resize - ONLY in Timesheets folder
            Outlook.MAPIFolder timesheetFolder = GetTimesheetFolder();
            if (timesheetFolder != null)
            {
                _timesheetItems = timesheetFolder.Items;
                _timesheetItems.ItemChange += Items_ItemChange;
            }

            UpdateDashboard();
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            // Clean up
        }

        protected override Office.IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            return new UI.TimesheetRibbon();
        }

        /// <summary>
        /// Intercepts the opening of an Inspector window.
        /// If the item is a tracked Appointment (has JobID), it closes the native window
        /// and opens the custom WinForms dialog instead.
        /// </summary>
        /// <param name="Inspector">The inspector being opened.</param>
        private void Inspectors_NewInspector(Outlook.Inspector Inspector)
        {
            if (Inspector.CurrentItem is Outlook.AppointmentItem appt)
            {
                // Ensure we only interfere if the item is in the "Timesheets" folder
                if (appt.Parent is Outlook.Folder folder && folder.Name == "Timesheets")
                {
                    // Check if this appointment is tracked by our system
                    string jobId = GetUserProperty(appt, "JobID");

                    if (!string.IsNullOrEmpty(jobId))
                    {
                        // It's a tracked item. Close the native inspector.
                        // olDiscard ensures changes aren't prompted for save, though we haven't made any yet.
                        ((Outlook._Inspector)Inspector).Close(Outlook.OlInspectorClose.olDiscard);

                        // Open the Custom Form
                        OpenCustomForm(appt);
                    }
                }
            }
        }

        private void OpenCustomForm(Outlook.AppointmentItem appt)
        {
            try
            {
                // Map Appointment to Model
                TimeEntryModel model = new TimeEntryModel
                {
                    OutlookID = appt.EntryID,
                    JobId = GetUserProperty(appt, "JobID"),
                    TaskId = GetUserProperty(appt, "TaskID"),
                    Date = appt.Start,
                    RT = GetUserPropertyDouble(appt, "RT"),
                    OT = GetUserPropertyDouble(appt, "OT"),
                    DT = GetUserPropertyDouble(appt, "DT"),
                    Travel = GetUserPropertyDouble(appt, "Travel"),
                    Notes = appt.Body, // Or a specific UserProperty
                    Status = GetUserProperty(appt, "TimeStatus") ?? "Draft"
                };

                // If RT is 0 (newly converted?), default to Duration
                if (model.RT == 0 && model.TotalHours == 0)
                {
                    model.RT = appt.Duration / 60.0;
                    model.TotalHours = model.RT;
                }

                // Show Dialog
                using (TimeEntryForm form = new TimeEntryForm(model))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        // Update Appointment
                        UpdateAppointmentFromModel(appt, form.Model);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening timesheet form: " + ex.Message);
            }
        }

        private void UpdateAppointmentFromModel(Outlook.AppointmentItem appt, TimeEntryModel model)
        {
            _isUpdating = true;
            try
            {
                SetUserProperty(appt, "JobID", model.JobId);
                SetUserProperty(appt, "TaskID", model.TaskId);
                SetUserProperty(appt, "RT", model.RT);
                SetUserProperty(appt, "OT", model.OT);
                SetUserProperty(appt, "DT", model.DT);
                SetUserProperty(appt, "Travel", model.Travel);
                SetUserProperty(appt, "TimeStatus", model.Status);

                appt.Body = model.Notes;

                // Update Visuals
                UpdateVisuals(appt, model.JobId, model.TaskId); // Pass names if we had them, or lookup

                appt.Save();

                // Sync to API
                SafeSubmitAsync(model);
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private async void SafeSubmitAsync(TimeEntryModel model)
        {
            try
            {
                await _apiService.SubmitTimeEntryAsync(model);
            }
            catch (Exception ex)
            {
                // In a real app, log this.
                System.Diagnostics.Debug.WriteLine($"Error submitting time entry: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the ItemChange event to detect dragging/resizing.
        /// Updates the RT (Regular Time) based on the new Duration.
        /// </summary>
        /// <param name="Item">The item that changed.</param>
        private void Items_ItemChange(object Item)
        {
            UpdateDashboard();

            if (_isUpdating) return;
            if (!(Item is Outlook.AppointmentItem appt)) return;

            string jobId = GetUserProperty(appt, "JobID");
            if (string.IsNullOrEmpty(jobId)) return;

            string status = GetUserProperty(appt, "TimeStatus");
            if (status == "Submitted")
            {
                // Revert changes if possible, or warn.
                // Reverting in ItemChange is hard because we don't have the old state easily unless we track it.
                // For now, just warn.
                MessageBox.Show("This timesheet is submitted and cannot be modified.");
                return;
            }

            if (status == "Draft")
            {
                _isUpdating = true;
                try
                {
                    // Calculate New RT
                    double durationHours = appt.Duration / 60.0;
                    double ot = GetUserPropertyDouble(appt, "OT");
                    double dt = GetUserPropertyDouble(appt, "DT");
                    double travel = GetUserPropertyDouble(appt, "Travel");

                    double newRT = durationHours - ot - dt - travel;

                    if (newRT < 0) newRT = 0; // Prevent negative

                    SetUserProperty(appt, "RT", newRT);

                    // Sync
                    TimeEntryModel model = new TimeEntryModel
                    {
                        OutlookID = appt.EntryID,
                        JobId = jobId,
                        TaskId = GetUserProperty(appt, "TaskID"),
                        Date = appt.Start,
                        RT = newRT,
                        OT = ot,
                        DT = dt,
                        Travel = travel,
                        Status = status
                    };

                    // Fire and forget sync
                    SafeSubmitAsync(model);
                }
                finally
                {
                    _isUpdating = false;
                }
            }
        }

        #region Helper Methods

        public Outlook.MAPIFolder GetTimesheetFolder()
        {
            Outlook.Folder calendar = this.Application.Session.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderCalendar) as Outlook.Folder;
            try
            {
                return calendar.Folders["Timesheets"];
            }
            catch
            {
                return calendar.Folders.Add("Timesheets", Outlook.OlDefaultFolders.olFolderCalendar);
            }
        }

        public void SetUserProperty(Outlook.AppointmentItem item, string name, object value)
        {
            Outlook.UserProperty prop = item.UserProperties.Find(name, true);
            if (prop == null)
            {
                prop = item.UserProperties.Add(name, Outlook.OlUserPropertyType.olText, true, 1);
            }
            prop.Value = value;
        }

        private string GetUserProperty(Outlook.AppointmentItem item, string name)
        {
            Outlook.UserProperty prop = item.UserProperties.Find(name, true);
            if (prop != null) return prop.Value.ToString();
            return null;
        }

        private double GetUserPropertyDouble(Outlook.AppointmentItem item, string name)
        {
            Outlook.UserProperty prop = item.UserProperties.Find(name, true);
            if (prop != null)
            {
                if (double.TryParse(prop.Value.ToString(), out double result))
                    return result;
            }
            return 0;
        }

        public void UpdateVisuals(Outlook.AppointmentItem item, string jobName, string taskName)
        {
            // Ideally we should lookup IDs to Names if we only have IDs here.
            // For now assuming the passed values are what we want to display or IDs.
            // If they are IDs, we might want to fetch names, but that's async.
            // We'll trust the ribbon provided names or format with IDs if names aren't available.

            // Note: If calling from ItemChange, we don't have the Names easily available without querying API.
            // Simplification: Just update Category. Subject update usually happens on Assignment.

            item.Categories = "Draft Time";
        }

        public void UpdateDashboard()
        {
            try
            {
                Outlook.MAPIFolder folder = GetTimesheetFolder();
                if (folder == null) return;

                DateTime today = DateTime.Today;
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                DateTime startOfWeek = today.AddDays(-1 * diff).Date;
                DateTime endOfWeek = startOfWeek.AddDays(7).Date;

                // Restrict items
                string filter = $"[Start] >= '{startOfWeek:g}' AND [Start] < '{endOfWeek:g}'";
                Outlook.Items items = folder.Items.Restrict(filter);

                double totalMinutes = 0;
                foreach (object item in items)
                {
                    if (item is Outlook.AppointmentItem appt)
                    {
                        totalMinutes += appt.Duration;
                    }
                }

                if (_dashboardControl != null)
                {
                    _dashboardControl.UpdateProgress(totalMinutes / 60.0);
                }
            }
            catch { /* Best effort */ }
        }

        #endregion
    }
}
