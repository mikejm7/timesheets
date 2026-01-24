using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using timesheets.Models;
using timesheets.Services;

namespace timesheets.Forms
{
    public partial class TimeEntryForm : Form
    {
        private TimeEntryModel _model;
        private ApiService _apiService;

        public TimeEntryModel Model => _model;

        public TimeEntryForm(TimeEntryModel model)
        {
            InitializeComponent();
            _model = model;
            _apiService = new ApiService();

            this.Load += TimeEntryForm_Load;
            this.cmbJob.SelectedIndexChanged += CmbJob_SelectedIndexChanged;

            this.numRT.ValueChanged += UpdateTotalHours;
            this.numOT.ValueChanged += UpdateTotalHours;
            this.numDT.ValueChanged += UpdateTotalHours;
            this.numTravel.ValueChanged += UpdateTotalHours;

            this.btnSave.Click += BtnSave_Click;
        }

        private async void TimeEntryForm_Load(object sender, EventArgs e)
        {
            try
            {
                var jobs = await _apiService.GetJobsAsync();
                cmbJob.DataSource = new BindingSource(jobs, null);
                cmbJob.DisplayMember = "Value";
                cmbJob.ValueMember = "Key";

                // Pre-fill
                if (!string.IsNullOrEmpty(_model.JobId))
                {
                    cmbJob.SelectedValue = _model.JobId;
                    await LoadTasks(_model.JobId);
                    if (!string.IsNullOrEmpty(_model.TaskId))
                    {
                        cmbTask.SelectedValue = _model.TaskId;
                    }
                }

                numRT.Value = (decimal)_model.RT;
                numOT.Value = (decimal)_model.OT;
                numDT.Value = (decimal)_model.DT;
                numTravel.Value = (decimal)_model.Travel;
                txtNotes.Text = _model.Notes;

                UpdateTotalHours(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }

        private async void CmbJob_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbJob.SelectedValue != null)
            {
                var jobId = ((KeyValuePair<string, string>)cmbJob.SelectedItem).Key;
                await LoadTasks(jobId);
            }
        }

        private async Task LoadTasks(string jobId)
        {
            var tasks = await _apiService.GetTasksAsync(jobId);
            cmbTask.DataSource = new BindingSource(tasks, null);
            cmbTask.DisplayMember = "Value";
            cmbTask.ValueMember = "Key";
        }

        private void UpdateTotalHours(object sender, EventArgs e)
        {
            decimal total = numRT.Value + numOT.Value + numDT.Value + numTravel.Value;
            lblTotalHours.Text = total.ToString("F2");
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            _model.RT = (double)numRT.Value;
            _model.OT = (double)numOT.Value;
            _model.DT = (double)numDT.Value;
            _model.Travel = (double)numTravel.Value;
            _model.TotalHours = _model.RT + _model.OT + _model.DT + _model.Travel;

            if (cmbJob.SelectedItem != null)
                _model.JobId = ((KeyValuePair<string, string>)cmbJob.SelectedItem).Key;

            if (cmbTask.SelectedItem != null)
                _model.TaskId = ((KeyValuePair<string, string>)cmbTask.SelectedItem).Key;

            _model.Notes = txtNotes.Text;

            // DialogResult is already set to OK in designer
        }
    }
}
