namespace RamshurRatApp
{
    partial class FrmPopUpHistory
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
            this.Label_for_start_date = new System.Windows.Forms.Label();
            this.Label_for_end_date = new System.Windows.Forms.Label();
            this.EndDatePicker = new System.Windows.Forms.DateTimePicker();
            this.StartDatePicker = new System.Windows.Forms.DateTimePicker();
            this.button_plot = new System.Windows.Forms.Button();
            this.LableHour = new System.Windows.Forms.Label();
            this.LabelHour2 = new System.Windows.Forms.Label();
            this.StartTimePicker = new System.Windows.Forms.DateTimePicker();
            this.EndTimePicker = new System.Windows.Forms.DateTimePicker();
            this.SuspendLayout();
            // 
            // Label_for_start_date
            // 
            this.Label_for_start_date.AutoSize = true;
            this.Label_for_start_date.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label_for_start_date.Location = new System.Drawing.Point(1, 13);
            this.Label_for_start_date.Name = "Label_for_start_date";
            this.Label_for_start_date.Size = new System.Drawing.Size(67, 13);
            this.Label_for_start_date.TabIndex = 0;
            this.Label_for_start_date.Text = " Start date";
            // 
            // Label_for_end_date
            // 
            this.Label_for_end_date.AutoSize = true;
            this.Label_for_end_date.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label_for_end_date.Location = new System.Drawing.Point(1, 44);
            this.Label_for_end_date.Name = "Label_for_end_date";
            this.Label_for_end_date.Size = new System.Drawing.Size(62, 13);
            this.Label_for_end_date.TabIndex = 1;
            this.Label_for_end_date.Text = " End date";
            // 
            // EndDatePicker
            // 
            this.EndDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.EndDatePicker.Location = new System.Drawing.Point(74, 42);
            this.EndDatePicker.Name = "EndDatePicker";
            this.EndDatePicker.Size = new System.Drawing.Size(100, 20);
            this.EndDatePicker.TabIndex = 2;
            // 
            // StartDatePicker
            // 
            this.StartDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.StartDatePicker.Location = new System.Drawing.Point(74, 12);
            this.StartDatePicker.Name = "StartDatePicker";
            this.StartDatePicker.Size = new System.Drawing.Size(100, 20);
            this.StartDatePicker.TabIndex = 3;
            // 
            // button_plot
            // 
            this.button_plot.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_plot.Location = new System.Drawing.Point(258, 76);
            this.button_plot.Name = "button_plot";
            this.button_plot.Size = new System.Drawing.Size(75, 23);
            this.button_plot.TabIndex = 9;
            this.button_plot.Text = "Save";
            this.button_plot.UseVisualStyleBackColor = true;
            this.button_plot.Click += new System.EventHandler(this.button_plot_Click);
            // 
            // LableHour
            // 
            this.LableHour.AutoSize = true;
            this.LableHour.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LableHour.Location = new System.Drawing.Point(189, 13);
            this.LableHour.Name = "LableHour";
            this.LableHour.Size = new System.Drawing.Size(52, 13);
            this.LableHour.TabIndex = 10;
            this.LableHour.Text = "Hr : Min";
            // 
            // LabelHour2
            // 
            this.LabelHour2.AutoSize = true;
            this.LabelHour2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelHour2.Location = new System.Drawing.Point(189, 44);
            this.LabelHour2.Name = "LabelHour2";
            this.LabelHour2.Size = new System.Drawing.Size(52, 13);
            this.LabelHour2.TabIndex = 12;
            this.LabelHour2.Text = "Hr : Min";
            // 
            // StartTimePicker
            // 
            this.StartTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.StartTimePicker.Location = new System.Drawing.Point(247, 12);
            this.StartTimePicker.Name = "StartTimePicker";
            this.StartTimePicker.ShowUpDown = true;
            this.StartTimePicker.Size = new System.Drawing.Size(86, 20);
            this.StartTimePicker.TabIndex = 13;
            // 
            // EndTimePicker
            // 
            this.EndTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.EndTimePicker.Location = new System.Drawing.Point(247, 42);
            this.EndTimePicker.Name = "EndTimePicker";
            this.EndTimePicker.ShowUpDown = true;
            this.EndTimePicker.Size = new System.Drawing.Size(86, 20);
            this.EndTimePicker.TabIndex = 14;
            // 
            // FrmPopUpHistory
            // 
            this.AcceptButton = this.button_plot;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(354, 112);
            this.Controls.Add(this.EndTimePicker);
            this.Controls.Add(this.StartTimePicker);
            this.Controls.Add(this.LabelHour2);
            this.Controls.Add(this.LableHour);
            this.Controls.Add(this.button_plot);
            this.Controls.Add(this.StartDatePicker);
            this.Controls.Add(this.EndDatePicker);
            this.Controls.Add(this.Label_for_end_date);
            this.Controls.Add(this.Label_for_start_date);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(370, 150);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(370, 150);
            this.Name = "FrmPopUpHistory";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "History details";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label Label_for_start_date;
        private System.Windows.Forms.Label Label_for_end_date;
        private System.Windows.Forms.DateTimePicker EndDatePicker;
        private System.Windows.Forms.DateTimePicker StartDatePicker;
        private System.Windows.Forms.Button button_plot;
        private System.Windows.Forms.Label LableHour;
        private System.Windows.Forms.Label LabelHour2;
        private System.Windows.Forms.DateTimePicker StartTimePicker;
        private System.Windows.Forms.DateTimePicker EndTimePicker;
    }
}