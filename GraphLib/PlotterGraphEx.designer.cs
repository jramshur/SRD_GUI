namespace GraphLib
{
    partial class PlotterDisplayEx
    {
       
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        
        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlotterDisplayEx));
            this.imgList1 = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.gPane = new GraphLib.PlotterGraphPaneEx();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectGraphsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // imgList1
            // 
            this.imgList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgList1.ImageStream")));
            this.imgList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imgList1.Images.SetKeyName(0, "media-playback-start.png");
            this.imgList1.Images.SetKeyName(1, "media-playback-stop.png");
            this.imgList1.Images.SetKeyName(2, "media-playback-pause.png");
            this.imgList1.Images.SetKeyName(3, "printer.png");
            // 
            // splitContainer1
            // 
            this.splitContainer1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.Color.Transparent;
            this.splitContainer1.Panel1.Controls.Add(this.gPane);
            this.splitContainer1.Panel1MinSize = 20;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.Color.Transparent;
            this.splitContainer1.Panel2Collapsed = true;
            this.splitContainer1.Panel2MinSize = 5;
            this.splitContainer1.Size = new System.Drawing.Size(598, 339);
            this.splitContainer1.SplitterDistance = 315;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 2;
            this.splitContainer1.TabStop = false;
            // 
            // gPane
            // 
            this.gPane.BackColor = System.Drawing.Color.Transparent;
            this.gPane.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gPane.EnableDrawLegend = true;
            this.gPane.Location = new System.Drawing.Point(0, 0);
            this.gPane.Margin = new System.Windows.Forms.Padding(0);
            this.gPane.Name = "gPane";
            this.gPane.Size = new System.Drawing.Size(598, 339);
            this.gPane.TabIndex = 1;
            this.gPane.XaxisName = null;
            this.gPane.YaxisName = null;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectGraphsToolStripMenuItem,
            this.toolStripSeparator1});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(117, 32);
            // 
            // selectGraphsToolStripMenuItem
            // 
            this.selectGraphsToolStripMenuItem.Name = "selectGraphsToolStripMenuItem";
            this.selectGraphsToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.selectGraphsToolStripMenuItem.Text = "Options";
            this.selectGraphsToolStripMenuItem.Click += new System.EventHandler(this.selectGraphsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(113, 6);
            // 
            // PlotterDisplayEx
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.splitContainer1);
            this.Name = "PlotterDisplayEx";
            this.Size = new System.Drawing.Size(598, 339);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        public PlotterGraphPaneEx gPane;
        public System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem selectGraphsToolStripMenuItem;
        private System.Windows.Forms.ImageList imgList1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;

    }
}
