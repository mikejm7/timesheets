namespace timesheets.Forms
{
    partial class TimeEntryForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmbJob = new System.Windows.Forms.ComboBox();
            this.cmbTask = new System.Windows.Forms.ComboBox();
            this.lblJob = new System.Windows.Forms.Label();
            this.lblTask = new System.Windows.Forms.Label();
            this.numRT = new System.Windows.Forms.NumericUpDown();
            this.numOT = new System.Windows.Forms.NumericUpDown();
            this.numDT = new System.Windows.Forms.NumericUpDown();
            this.numTravel = new System.Windows.Forms.NumericUpDown();
            this.lblRT = new System.Windows.Forms.Label();
            this.lblOT = new System.Windows.Forms.Label();
            this.lblDT = new System.Windows.Forms.Label();
            this.lblTravel = new System.Windows.Forms.Label();
            this.txtNotes = new System.Windows.Forms.TextBox();
            this.lblNotes = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblTotalLabel = new System.Windows.Forms.Label();
            this.lblTotalHours = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numRT)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOT)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDT)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTravel)).BeginInit();
            this.SuspendLayout();
            //
            // cmbJob
            //
            this.cmbJob.FormattingEnabled = true;
            this.cmbJob.Location = new System.Drawing.Point(100, 20);
            this.cmbJob.Name = "cmbJob";
            this.cmbJob.Size = new System.Drawing.Size(200, 21);
            this.cmbJob.TabIndex = 0;
            //
            // cmbTask
            //
            this.cmbTask.FormattingEnabled = true;
            this.cmbTask.Location = new System.Drawing.Point(100, 60);
            this.cmbTask.Name = "cmbTask";
            this.cmbTask.Size = new System.Drawing.Size(200, 21);
            this.cmbTask.TabIndex = 1;
            //
            // lblJob
            //
            this.lblJob.AutoSize = true;
            this.lblJob.Location = new System.Drawing.Point(20, 23);
            this.lblJob.Name = "lblJob";
            this.lblJob.Size = new System.Drawing.Size(24, 13);
            this.lblJob.TabIndex = 2;
            this.lblJob.Text = "Job";
            //
            // lblTask
            //
            this.lblTask.AutoSize = true;
            this.lblTask.Location = new System.Drawing.Point(20, 63);
            this.lblTask.Name = "lblTask";
            this.lblTask.Size = new System.Drawing.Size(31, 13);
            this.lblTask.TabIndex = 3;
            this.lblTask.Text = "Task";
            //
            // numRT
            //
            this.numRT.DecimalPlaces = 2;
            this.numRT.Location = new System.Drawing.Point(100, 100);
            this.numRT.Name = "numRT";
            this.numRT.Size = new System.Drawing.Size(80, 20);
            this.numRT.TabIndex = 4;
            //
            // numOT
            //
            this.numOT.DecimalPlaces = 2;
            this.numOT.Location = new System.Drawing.Point(100, 140);
            this.numOT.Name = "numOT";
            this.numOT.Size = new System.Drawing.Size(80, 20);
            this.numOT.TabIndex = 5;
            //
            // numDT
            //
            this.numDT.DecimalPlaces = 2;
            this.numDT.Location = new System.Drawing.Point(250, 100);
            this.numDT.Name = "numDT";
            this.numDT.Size = new System.Drawing.Size(80, 20);
            this.numDT.TabIndex = 6;
            //
            // numTravel
            //
            this.numTravel.DecimalPlaces = 2;
            this.numTravel.Location = new System.Drawing.Point(250, 140);
            this.numTravel.Name = "numTravel";
            this.numTravel.Size = new System.Drawing.Size(80, 20);
            this.numTravel.TabIndex = 7;
            //
            // lblRT
            //
            this.lblRT.AutoSize = true;
            this.lblRT.Location = new System.Drawing.Point(20, 102);
            this.lblRT.Name = "lblRT";
            this.lblRT.Size = new System.Drawing.Size(22, 13);
            this.lblRT.TabIndex = 8;
            this.lblRT.Text = "RT";
            //
            // lblOT
            //
            this.lblOT.AutoSize = true;
            this.lblOT.Location = new System.Drawing.Point(20, 142);
            this.lblOT.Name = "lblOT";
            this.lblOT.Size = new System.Drawing.Size(22, 13);
            this.lblOT.TabIndex = 9;
            this.lblOT.Text = "OT";
            //
            // lblDT
            //
            this.lblDT.AutoSize = true;
            this.lblDT.Location = new System.Drawing.Point(200, 102);
            this.lblDT.Name = "lblDT";
            this.lblDT.Size = new System.Drawing.Size(22, 13);
            this.lblDT.TabIndex = 10;
            this.lblDT.Text = "DT";
            //
            // lblTravel
            //
            this.lblTravel.AutoSize = true;
            this.lblTravel.Location = new System.Drawing.Point(200, 142);
            this.lblTravel.Name = "lblTravel";
            this.lblTravel.Size = new System.Drawing.Size(37, 13);
            this.lblTravel.TabIndex = 11;
            this.lblTravel.Text = "Travel";
            //
            // txtNotes
            //
            this.txtNotes.Location = new System.Drawing.Point(100, 180);
            this.txtNotes.Multiline = true;
            this.txtNotes.Name = "txtNotes";
            this.txtNotes.Size = new System.Drawing.Size(230, 60);
            this.txtNotes.TabIndex = 12;
            //
            // lblNotes
            //
            this.lblNotes.AutoSize = true;
            this.lblNotes.Location = new System.Drawing.Point(20, 183);
            this.lblNotes.Name = "lblNotes";
            this.lblNotes.Size = new System.Drawing.Size(35, 13);
            this.lblNotes.TabIndex = 13;
            this.lblNotes.Text = "Notes";
            //
            // btnSave
            //
            this.btnSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSave.Location = new System.Drawing.Point(174, 280);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 14;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            //
            // btnCancel
            //
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(255, 280);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 15;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            //
            // lblTotalLabel
            //
            this.lblTotalLabel.AutoSize = true;
            this.lblTotalLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalLabel.Location = new System.Drawing.Point(20, 255);
            this.lblTotalLabel.Name = "lblTotalLabel";
            this.lblTotalLabel.Size = new System.Drawing.Size(77, 13);
            this.lblTotalLabel.TabIndex = 16;
            this.lblTotalLabel.Text = "Total Hours:";
            //
            // lblTotalHours
            //
            this.lblTotalHours.AutoSize = true;
            this.lblTotalHours.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalHours.Location = new System.Drawing.Point(103, 255);
            this.lblTotalHours.Name = "lblTotalHours";
            this.lblTotalHours.Size = new System.Drawing.Size(32, 13);
            this.lblTotalHours.TabIndex = 17;
            this.lblTotalHours.Text = "0.00";
            //
            // TimeEntryForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 320);
            this.Controls.Add(this.lblTotalHours);
            this.Controls.Add(this.lblTotalLabel);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.lblNotes);
            this.Controls.Add(this.txtNotes);
            this.Controls.Add(this.lblTravel);
            this.Controls.Add(this.lblDT);
            this.Controls.Add(this.lblOT);
            this.Controls.Add(this.lblRT);
            this.Controls.Add(this.numTravel);
            this.Controls.Add(this.numDT);
            this.Controls.Add(this.numOT);
            this.Controls.Add(this.numRT);
            this.Controls.Add(this.lblTask);
            this.Controls.Add(this.lblJob);
            this.Controls.Add(this.cmbTask);
            this.Controls.Add(this.cmbJob);
            this.Name = "TimeEntryForm";
            this.Text = "Edit Time Entry";
            ((System.ComponentModel.ISupportInitialize)(this.numRT)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOT)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDT)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTravel)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbJob;
        private System.Windows.Forms.ComboBox cmbTask;
        private System.Windows.Forms.Label lblJob;
        private System.Windows.Forms.Label lblTask;
        private System.Windows.Forms.NumericUpDown numRT;
        private System.Windows.Forms.NumericUpDown numOT;
        private System.Windows.Forms.NumericUpDown numDT;
        private System.Windows.Forms.NumericUpDown numTravel;
        private System.Windows.Forms.Label lblRT;
        private System.Windows.Forms.Label lblOT;
        private System.Windows.Forms.Label lblDT;
        private System.Windows.Forms.Label lblTravel;
        private System.Windows.Forms.TextBox txtNotes;
        private System.Windows.Forms.Label lblNotes;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblTotalLabel;
        private System.Windows.Forms.Label lblTotalHours;
    }
}
