using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VoManager;
using System.Reflection;

namespace RamshurRatApp
{
    public partial class Ramshur : Form
    {
        public Ramshur()
        {
            InitializeComponent();
            stop.Visible = false;
            Start.Visible = false;
            this.FillComboDay();
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {



        }
        private void Commitbutton_Click(object sender, EventArgs e)
        {

            if (!(this.checkBox1.Checked || this.Channl2chkbox.Checked))
            {
                MessageBox.Show("Select the channel to be used");
                return;
            }
            if (!(this.Triggerdmoderadiobutton.Checked || this.Continousmoderadiobutton.Checked))
            {
                MessageBox.Show("Select the mode of operation ");
                return;
            }


            if (String.IsNullOrEmpty(txtT1.Text) || String.IsNullOrEmpty(textT3.Text) || String.IsNullOrEmpty(textT4.Text) || String.IsNullOrEmpty(textT5.Text) || String.IsNullOrEmpty(textT9.Text) || String.IsNullOrEmpty(textT2.Text) || String.IsNullOrEmpty(textT6.Text) || String.IsNullOrEmpty(textT7.Text) || String.IsNullOrEmpty(textT8.Text) || String.IsNullOrEmpty(textT10.Text))
            {
                MessageBox.Show("Please fill all the configuration paramters");
            }
            if (comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Please select ADC resolution");
            }
            else
            {
                List<ConfigurationVO> configList = new List<ConfigurationVO>();
                ConfigurationVO configVo = new ConfigurationVO();
                if (Continousmoderadiobutton.Checked)
                    configVo.mode_operation = 1;
                else
                    configVo.mode_operation = 2;
                if (checkBox1.Checked)
                    configVo.channel = 1;
                else
                    configVo.channel = 2;

                configVo.T1 = txtT1.Text;
                configVo.T2 = textT3.Text;
                configVo.T3 = textT4.Text;
                configVo.T4 = textT5.Text;
                configVo.T5 = textT9.Text;
                configVo.T6 = textT2.Text;
                configVo.T7 = textT6.Text;
                configVo.T8 = textT7.Text;
                configVo.T9 = textT8.Text;
                configVo.T10 = textT10.Text;
                configVo.adcresolution = comboBox1.SelectedItem.ToString();
                configList.Add(configVo);
                string path = Assembly.GetExecutingAssembly().Location;
                int edit= path.LastIndexOf("\\");
                path= path.Remove(edit);
                path+=EnumAndConstant.Configuration_File;
                ApplicationUtil.ApplicationUtil.CreateCSVFromGenericList(configList,path);
                labelpathdispl.Text = path;




                MessageBox.Show("Configurations saved Succesfully");
            }
        }

        private void txtT1_KeyPress(object sender, KeyPressEventArgs e)
        {
            
            if (!char.IsControl(e.KeyChar)
                      && !char.IsDigit(e.KeyChar)
               && e.KeyChar != '.')
            {
                e.Handled = true;
            }
            

            // only allow one decimal point
            if (e.KeyChar == '.'
                && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
            
            
        }

        private void textT2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
        && !char.IsDigit(e.KeyChar)
        && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if (e.KeyChar == '.'
                && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }
        private void textT6_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
        && !char.IsDigit(e.KeyChar)
        && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if (e.KeyChar == '.'
                && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        private void textT3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
        && !char.IsDigit(e.KeyChar)
        && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if (e.KeyChar == '.'
                && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }
        private void textT4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
        && !char.IsDigit(e.KeyChar)
        && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if (e.KeyChar == '.'
                && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        private void textT5_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
        && !char.IsDigit(e.KeyChar)
        && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if (e.KeyChar == '.'
                && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        private void textT9_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
        && !char.IsDigit(e.KeyChar)
        && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if (e.KeyChar == '.'
                && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        private void textT7_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
        && !char.IsDigit(e.KeyChar)
        && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if (e.KeyChar == '.'
                && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        private void textT8_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if (e.KeyChar == '.' && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        private void textT10_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if (e.KeyChar == '.'
                && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        private void Continousmoderadiobutton_Click(object sender, EventArgs e)
        {

            stop.Visible = true;
            Start.Visible = true;
        }

        private void Triggerdmoderadiobutton_Click(object sender, EventArgs e)
        {
            stop.Visible = false;
            Start.Visible = false;
        }

        

        private void zedGraphControl1_Load(object sender, EventArgs e)
        {


        }



        private void radioButtonhstry_CheckedChanged(object sender, EventArgs e)
        {
            Pop_up_form_for_for_history frm_hstry = new Pop_up_form_for_for_history(this);
            frm_hstry.Show();
        }
        

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void buttonedit_Click(object sender, EventArgs e)
        {
            txtT1.Text = "";
            textT2.Text = "";
            textT3.Text = "";
            textT4.Text = "";
            textT5.Text = "";
            textT6.Text = "";
            textT8.Text = "";
            textT7.Text = "";
            textT9.Text = "";
            textT10.Text = "";


        }

        private void change_loc_button_Click(object sender, EventArgs e)
        {
            Form2 frm_browse = new Form2(this);
            frm_browse.Show();
        }

        private void FillComboDay()
        {

             DateTime startDate = Pop_up_form_for_for_history.GetStartDate();
             DateTime endDate = Pop_up_form_for_for_history.GetEndDate();
             
             while (startDate <= endDate)
             {
                 comboBoxDay.Items.Add(startDate);
                     
                 startDate.AddDays(1);
             }
            
            
        }



        #region Graph helper methods
        #endregion

    }
}
