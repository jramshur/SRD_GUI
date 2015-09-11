using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VOManager;
using System.IO.Ports;
using CustomLogger;
using ComportCollection;


namespace RamshurRatApp
{
    public partial class PortDetails : Form
    {
        private static ILogger m_logger = MyLogFactory.GetInstance(LoggerType.Log4Net).GetLogger(System.Reflection.Assembly.GetExecutingAssembly(), typeof(PortDetails));
        public PortDetails()
        {
            InitializeComponent();
        }

        
        /// <summary>
        /// Initialise comport
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                ComPortVO comport = new ComPortVO();
                if (comboPort != null)
                {
                    comport.portName = comboPort.SelectedItem.ToString();
                    string baud_rate = System.Configuration.ConfigurationManager.AppSettings["BAUD_RATE"];
                    comport.baudRate = Convert.ToInt32(baud_rate);
                    m_logger.Debug("BAUD_RATE : " + baud_rate);
                    comport.parity = System.IO.Ports.Parity.None;
                    comport.dataBits = EnumAndConstants.DATA_BITS;
                    comport.stopBits = System.IO.Ports.StopBits.One;
                    if (Program.communicator.Init(comport) == false)
                        MessageBox.Show("Unable to open com port.");
                    else
                    {
                        EnumAndConstants.comportVO = comport;
                        if (Program.MainScreen.ConnectAndGetData())
                        {
                            this.DialogResult = System.Windows.Forms.DialogResult.OK;
                        }
                    }
                }
                //this.Close();
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
                MessageBox.Show("Please enter valid baud rate in configuration file.");
                //this.Close();
            }
            finally
            {
                this.Cursor = Cursors.Hand;
            }
        }

        /// <summary>
        /// Get all serial port names
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PortDetails_Load(object sender, EventArgs e)
        {
            
            //string [] prt = SerialPort.GetPortNames();
            List<string> comportList = Comport.GetComportList();
            comportList.Add("COM1");
            comportList.Add("COM2");
            string[] prt = comportList.ToArray<string>();
            IEnumerable<string> enu = prt.Distinct();
            comboPort.DataSource = null;
            String[] ports=enu.ToArray<string>();
            string[] portArray = new string[ports.Length];
           
            for (int i = 0; i < ports.Length; i++)
			{
                ports[i] = ports[i].Trim();
                for (int charChk = ports[i].Length - 1; charChk >= 0; charChk--)
                {
                    if (Char.IsDigit(ports[i][charChk]) == false)
                    {
                        ports[i] = ports[i].Remove(charChk);
                    }
                    else
                        break;
                }
                portArray[i] = ports[i];
 			}
        
            comboPort.DataSource = portArray;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void comboPort_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboPort_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

       
      

       
    }
}
