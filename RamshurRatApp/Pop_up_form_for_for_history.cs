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
    public partial class Pop_up_form_for_for_history : Form
    {
        public Pop_up_form_for_for_history()
        {
            InitializeComponent();
            this.FillComboHour();
            this.FillComboMin();
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        public void FillComboHour()
        {

            //System.Object[] ItemObject = new System.Object[60];
            List<object> time = new List<object>();
            for (int i = 0; i <= 24; i++)
            {
                //ItemObject[i] = "Item" + i;
                time.Add(i);
            }
            Combo_hour.Items.AddRange(time.ToArray());


        }

        private void button_plot_Click(object sender, EventArgs e)
        {
            if (this.Combo_hour.SelectedIndex== -1  )
            {
                MessageBox.Show("Select the channel to be used");
                return;
            }
        }

        private void FillComboMin()
        {

            //System.Object[] ItemObject = new System.Object[60];
            List<object> time = new List<object>();
            for (int i = 0; i <= 60; i++)
            {
                //ItemObject[i] = "Item" + i;
                time.Add(i);
            }
            combo_min1.Items.AddRange(time.ToArray());

        }

        

        
    }
}
