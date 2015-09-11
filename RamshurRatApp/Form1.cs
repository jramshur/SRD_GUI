using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RamshurRatApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            stop.Visible = false;
            Start.Visible = false;
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {
           
    

        }

        private void Triggerd_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void radioButton2_Click(object sender, EventArgs e)
        {

            stop.Visible = true;
            Start.Visible = true;

        }

        private void Triggerd_Click(object sender, EventArgs e)
        {

            stop.Visible = false;
            Start.Visible = false;

        }
    }
}
