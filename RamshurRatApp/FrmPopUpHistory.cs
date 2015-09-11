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
    public partial class FrmPopUpHistory : Form
    {
        public static DateTime StartDate = DateTime.Now;//VARIABLE DECLARATION
        public static DateTime EndDate = DateTime.Now;//VARIABLE DECLARATION

        public FrmPopUpHistory()
        {
            InitializeComponent();
        }

        private FrmPSoCMain mainfrm = null;

        public FrmPopUpHistory(Form callingForm)
        {
            mainfrm = callingForm as FrmPSoCMain;
            InitializeComponent();
        }
        
        public DateTime GetStartDate()//GETTER METHOD FOR START DATE
        {
            return StartDate;
        }
        public DateTime GetEndDate()//GETTER METHOD FOR END DATE
        {
            return EndDate;
        }
        
        /// <summary>
        /// Get Historical data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_plot_Click(object sender, EventArgs e)
        {
            if (EndDatePicker.Value == null || StartDatePicker.Value == null) 
            {
                MessageBox.Show("Please select date field");
                return;
            
            }//validaton of date field

            if (EndTimePicker.Value == null || StartTimePicker.Value == null)
            {
                MessageBox.Show("Please select date field");
                return;

            }//validaton of date field

            StartDate = new DateTime(StartDatePicker.Value.Year, StartDatePicker.Value.Month, StartDatePicker.Value.Day, 0, 0, 0);
            EndDate = new DateTime(EndDatePicker.Value.Year, EndDatePicker.Value.Month, EndDatePicker.Value.Day, 0, 0, 0);

            double totalDays = EndDate.Subtract(StartDate).TotalDays;

            List<DateTime> dateCollection = new List<DateTime>();
            for (int i = 0; i < totalDays; i++)
            {
                dateCollection.Add(StartDate.AddDays(i));
            }
            Program.MainScreen.FillComboBox(dateCollection);
            this.Hide();
        }
    }
}
