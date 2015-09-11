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
    public partial class FrmPopUpBrowser : Form
    {
        public FrmPopUpBrowser()
        {
            InitializeComponent();
        }
         private FrmMain mainfrm = null;

        public FrmPopUpBrowser(Form callingForm)
        {
            mainfrm = callingForm as FrmMain;
            InitializeComponent();
        }
        private void button_set_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(TxtBrowse.Text))
                
            {

                MessageBox.Show("Please fill Path field");
                return;
            
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void button_brawse_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();

        }

        private void TxtBrowse_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
