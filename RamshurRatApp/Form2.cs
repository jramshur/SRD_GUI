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
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private Ramshur mainform = null;

        public Form2(Form callingForm)
        {
        mainform = callingForm as Ramshur; 
        InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}
