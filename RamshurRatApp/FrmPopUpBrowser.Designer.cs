namespace RamshurRatApp
{
    partial class FrmPopUpBrowser
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
            this.label1 = new System.Windows.Forms.Label();
            this.button_brawse = new System.Windows.Forms.Button();
            this.button_set = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.TxtBrowse = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Path";
            // 
            // button_brawse
            // 
            this.button_brawse.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_brawse.Location = new System.Drawing.Point(302, 40);
            this.button_brawse.Name = "button_brawse";
            this.button_brawse.Size = new System.Drawing.Size(68, 23);
            this.button_brawse.TabIndex = 2;
            this.button_brawse.Text = "Browse";
            this.button_brawse.UseVisualStyleBackColor = true;
            this.button_brawse.Click += new System.EventHandler(this.button_brawse_Click);
            // 
            // button_set
            // 
            this.button_set.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_set.Location = new System.Drawing.Point(54, 85);
            this.button_set.Name = "button_set";
            this.button_set.Size = new System.Drawing.Size(53, 23);
            this.button_set.TabIndex = 3;
            this.button_set.Text = "Set";
            this.button_set.UseVisualStyleBackColor = true;
            this.button_set.Click += new System.EventHandler(this.button_set_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1_FileOk);
            // 
            // TxtBrowse
            // 
            this.TxtBrowse.Location = new System.Drawing.Point(54, 42);
            this.TxtBrowse.Name = "TxtBrowse";
            this.TxtBrowse.Size = new System.Drawing.Size(242, 20);
            this.TxtBrowse.TabIndex = 4;
            this.TxtBrowse.TextChanged += new System.EventHandler(this.TxtBrowse_TextChanged);
            // 
            // FrmPopUpBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 122);
            this.Controls.Add(this.TxtBrowse);
            this.Controls.Add(this.button_set);
            this.Controls.Add(this.button_brawse);
            this.Controls.Add(this.label1);
            this.Name = "FrmPopUpBrowser";
            this.Text = "PopupBrowse";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_brawse;
        private System.Windows.Forms.Button button_set;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox TxtBrowse;
    }
}