using System;
using System.Windows.Forms;
using System.Drawing;

namespace timesheets.UI
{
    public class DashboardControl : UserControl
    {
        private Label lblProgress;
        private ProgressBar progressBar;

        public DashboardControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.lblProgress = new Label();
            this.progressBar = new ProgressBar();
            this.SuspendLayout();

            // lblProgress
            this.lblProgress.AutoSize = true;
            this.lblProgress.Location = new Point(10, 10);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new Size(100, 20);
            this.lblProgress.Text = "0 / 40 Hours Logged";

            // progressBar
            this.progressBar.Location = new Point(10, 40);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new Size(130, 23);
            this.progressBar.Maximum = 40;
            this.progressBar.Value = 0;

            // DashboardControl
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.progressBar);
            this.Name = "DashboardControl";
            this.Size = new Size(150, 100);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        public void UpdateProgress(double hours)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<double>(UpdateProgress), hours);
                return;
            }

            lblProgress.Text = $"{hours:F1} / 40 Hours Logged";

            int value = (int)hours;
            if (value > 40) value = 40;
            if (value < 0) value = 0;
            progressBar.Value = value;
        }
    }
}
