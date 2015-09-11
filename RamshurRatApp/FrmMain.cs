using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VOManager;
using System.Reflection;
using CustomLogger;
using System.Configuration;
using System.Collections;
using System.IO;
using RamshurRatApp.Properties;
using System.Threading;
using GraphLib;
using VOManager;

namespace RamshurRatApp
{
    public partial class FrmMain : Form
    {
        private static ILogger m_logger = MyLogFactory.GetInstance(LoggerType.Log4Net).GetLogger(System.Reflection.Assembly.GetExecutingAssembly(), typeof(FrmMain));
        public string sourceFile;
        private static List<string> filenames = null;
        public int Counter = 0;
        //public bool IscontinousMode = false;
        public string changedpath;
        private delegate void setDeviceStatus(bool status, bool isConfiguration);
        private setDeviceStatus setDelegateStatus;
        private Dictionary<int, double> samplingRateMap = null;
        public bool SaveFile = false;
        private delegate void drawGraph(string fileName);
        public String FileName;
        //private drawGraph drawGraphdelegate;
        private delegate void Graphdelegate(PlotterDisplayEx graphControl1, PlotterDisplayEx graphControl2);
        private Graphdelegate graphdelegate;

        private delegate void CreateFileDelegate(ArrayList adcConfigCollection, string directory, int channel1,int channel2);
        private CreateFileDelegate createFileDelegate;

        private int repeatCount = 0;
        //private ConfigurationVO config = null;
        private object obj = new object();
        private List<cPoint> firstChannelArray = null;
        private DateTime firstGraphRefreshTime = DateTime.Now;
        private List<cPoint> secondChannelArray = null;
        private DateTime secondGraphRefreshTime = DateTime.Now;
        private double firstChannelY = 0;
        private double secondChannelY = 0;
        private int xDisplayRange = 100;
        private Thread thread = null;
        private Thread fileThread = null;
        ArrayList adcConfigCollection = null;
        string directory = string.Empty;
        int ChannelA = 0;
        int ChannelB = 0;
        private int graphRefreshTime = 1000;
        public int ZoomMultiplier = 0;
        public int CurrentZoomValue = 0;
        Object random = new object();
        private BackgroundWorker backgroundWorker;
        object m_objLockConfig = new object();
        bool m_IsInSetConfig = false;
        DateTime m_RequestTime;
        DateTime RequestTime
        {
            get
            {
                return m_RequestTime;
            }
            set
            {
                m_RequestTime = value;
                lblConfigStatus.Visible = true;
            }
        }
        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;
        //        cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
        //        //return cp;
        //    }
        //}

        /// <summary>
        /// Construction of Main Form
        /// </summary>
        public FrmMain()
        {
            try
            {
                
                InitializeComponent();//INITILIZE METHODS AND VARIABLE
                this.Size = new Size(910, 738);
                //this.BackgroundImageLayout = ImageLayout.Stretch;
                double w = btnSet.Location.X + btnSet.Width;
                double x = w - ClearAllGraphsbutton.Width;
                //ClearAllGraphsbutton.Location = new Point((int)x, 17);
                xDisplayRange = Convert.ToInt32(ConfigurationManager.AppSettings["xDisplayRange"]);
                graphRefreshTime = Convert.ToInt32(ConfigurationManager.AppSettings["graphRefreshTime"]);

                //panel1.Width = panel3.Width = panel4.Width = panel5.Width = 302;
                //TODO:Testing. next and prev- START
                 //ApplicationUtil.ApplicationUtil.addFileNameToList(2, "27-Feb-2013_19_10_32_181.csv");
                 //ApplicationUtil.ApplicationUtil.addFileNameToList(4, "27-Feb-2013_19_10_32_184.csv");
                 //ApplicationUtil.ApplicationUtil.addFileNameToList(4, "27-Feb-2013_19_10_32_188.csv");
                 //ApplicationUtil.ApplicationUtil.addFileNameToList(4, "27-Feb-2013_19_10_32_189.csv");
                 //ApplicationUtil.ApplicationUtil.addFileNameToList(2, "27-Feb-2013_19_10_32_183.csv");
                 //ApplicationUtil.ApplicationUtil.addFileNameToList(2048, "27-Feb-2013_19_10_32_187.csv");
                //END
                txtXRange.Text = xDisplayRange.ToString();
                txtXRange.Enabled = false;
                btnSet.Enabled = false;
               
                plotterDisplayEx1.Load+=new EventHandler(plotterDisplayEx1_Load);
                plotterDisplayEx2.Load += new EventHandler(plotterDisplayEx1_Load);
                plotterDisplayEx3.Load += new EventHandler(plotterDisplayEx1_Load);
                plotterDisplayEx4.Load += new EventHandler(plotterDisplayEx1_Load);
                plotterDisplayEx5.Load += new EventHandler(plotterDisplayEx1_Load);
                plotterDisplayEx6.Load += new EventHandler(plotterDisplayEx1_Load);
                plotterDisplayEx7.Load += new EventHandler(plotterDisplayEx1_Load);
                plotterDisplayEx8.Load += new EventHandler(plotterDisplayEx1_Load);
                plotterDisplayEx9.Load += new EventHandler(plotterDisplayEx1_Load);
                plotterDisplayEx10.Load += new EventHandler(plotterDisplayEx1_Load);
                plotterDisplayEx11.Load += new EventHandler(plotterDisplayEx1_Load);
                plotterDisplayEx12.Load += new EventHandler(plotterDisplayEx1_Load);

               // RendeRTestGraph();

                
                setDelegateStatus = new setDeviceStatus(SetHHUMessage);
                //drawGraphdelegate = new drawGraph(DrawLiveGraph);
                button5.Visible = false;
                PlotterDisplayEx graph = new PlotterDisplayEx();
                graphdelegate = new Graphdelegate(RenderGraph);
                createFileDelegate = new CreateFileDelegate(createCSVFile);
                if (Program.communicator == null || Program.communicator.serialPort == null || Program.communicator.serialPort.IsOpen == false)
                {
                    deviceCon();
                }


                SetHeightOfGraph();

                SetDefaultUI();
              
                VOManager.DASCommunicator.SetStatusEvent += new VOManager.SetDACStatusDelegate(DASCommunicator_SetStatusEvent);
                VOManager.DASCommunicator.DirectoryCreationErrorEvent+=new VOManager.DirectoryCreationError(DASCommunicator_DirectoryCreationErrorEvent);
                //GraphInitialiser();

                //SetDirectoryPath
                Configuration oConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                string DirectoryPath = ConfigurationManager.AppSettings["DirectoryPath"];
                if (changedpath == null)
                {

                    lblDirectorypath.Text = ApplicationUtil.ApplicationUtil.GetApplicationPath();
                    String path = ApplicationUtil.ApplicationUtil.GetApplicationPath();
                    filenames = ApplicationUtil.ApplicationUtil.GetSubDirectoryFiles(path, ".csv");
                    changedpath = lblDirectorypath.Text;
                   
                }
                else
                {
                    lblDirectorypath.Text = changedpath;
                    
                    filenames = ApplicationUtil.ApplicationUtil.GetSubDirectoryFiles(changedpath, ".csv");
                }

                if (DirectoryPath != null && DirectoryPath != "")
                {
                    changedpath = DirectoryPath;
                    lblDirectorypath.Text = changedpath;
                    
                }

                ///Map for ADC resolution
                samplingRateMap = new Dictionary<int, double>();
                samplingRateMap.Add(0, 1);
                samplingRateMap.Add(1, 2);
                samplingRateMap.Add(2, 3);
                samplingRateMap.Add(3, 4);
                samplingRateMap.Add(4, 5);
                samplingRateMap.Add(5, 6);
                samplingRateMap.Add(6, 10);
                samplingRateMap.Add(7, 12.5);
                samplingRateMap.Add(8, 15);
                backgroundWorker = new BackgroundWorker();
                backgroundWorker.WorkerSupportsCancellation = true;
                backgroundWorker.DoWork += backgroundWorker_DoWork;
                backgroundWorker.RunWorkerCompleted +=new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);
                PlotterDisplayEx graphControl = new PlotterDisplayEx();
                //thread = new Thread(DoGraphActivity);

                //thread.IsBackground = true;
               // thread.Start();
               DASCommunicator.FileLoggingQueue = new RequestQ.QueueThread<object>();
               DASCommunicator. FileLoggingQueue.ProcessThreadObjectEvent += new RequestQ.QueueThread<object>.DelegateProcessThreadObject(FileLoggingQueue_ProcessThreadObjectEvent);
               DASCommunicator.FileLoggingQueue.Start();
               lblVersion.Text += Assembly.GetExecutingAssembly().GetName().Version.ToString();
               SetHHUMessage(false, false);
               timerStatusMsg.Start();
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
            }
        }

        /// <summary>
        /// Send Stop Continuous mode request
        /// </summary>
        public void Stop()
        {
            try
            {
                if (DASCommunicator.FileLoggingQueue != null)
                    DASCommunicator.FileLoggingQueue.Stop();
                ResponseCode code = Program.communicator.stopContinousRequest();
                Program.communicator.DeInit();
            }
            catch (Exception)
            {
            }
            
        }

        /// <summary>
        /// Histrory directory creation error event handler
        /// </summary>
        /// <param name="DirectoryName"></param>
        void DASCommunicator_DirectoryCreationErrorEvent(string DirectoryName)
        {
            MessageBox.Show("Could not create directory due to permission levels of the folder.\n"+DirectoryName+" .Please change the permissions and restart the application.", "Directory permission error", MessageBoxButtons.OK);
            return;
        }

        /// <summary>
        /// Histrory file logging handler
        /// </summary>
        /// <param name="obj"></param>
        void FileLoggingQueue_ProcessThreadObjectEvent(object obj)
        {
            VOManager.FileObjectVO file=(FileObjectVO)obj;
            ArrayList DataCollection = new ArrayList();
            DataCollection.Add(file.Data[0]);
            DataCollection.Add(((ArrayList)file.Data[1])[0]);
            if (((ArrayList)file.Data[1]).Count == 2)
            {
                DataCollection.Add(((ArrayList)file.Data[1])[1]);
            }
            
            if (SaveFile == true)
            {
                createCSVFile(DataCollection, file.filePath, file.channel1, file.channel2);
            }
            DoGraphActivity(DataCollection);
            
        }

        /// <summary>
        /// Backsround worker callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (backgroundWorker.IsBusy)
            {
                backgroundWorker.CancelAsync();
            }
        }

        /// <summary>
        /// Set defaukt height of all graph
        /// </summary>
        private void SetHeightOfGraph()
        {
            int height = tableLayoutPanelWaveForm.Height;
            int graphHeight = (height / 3) - 10;
            plotterDisplayEx1.Height = graphHeight;
            plotterDisplayEx2.Height = graphHeight;
            plotterDisplayEx3.Height = graphHeight;
            plotterDisplayEx4.Height = graphHeight;
            plotterDisplayEx5.Height = graphHeight;
            plotterDisplayEx6.Height = graphHeight;
            plotterDisplayEx7.Height = graphHeight;
            plotterDisplayEx8.Height = graphHeight;
            plotterDisplayEx9.Height = graphHeight;
            plotterDisplayEx10.Height = graphHeight;
            plotterDisplayEx11.Height = graphHeight;
            plotterDisplayEx12.Height = graphHeight;

            plotterDisplayEx1.Location = new Point(plotterDisplayEx1.Location.X, plotterDisplayEx1.Location.Y);
            channel5label.Location = new Point(channel5label.Location.X, plotterDisplayEx1.Location.Y + plotterDisplayEx1.Height + 5);
            plotterDisplayEx5.Location = new Point(plotterDisplayEx5.Location.X, channel5label.Location.Y + channel5label.Height + 5);
            channellabel9.Location = new Point(channellabel9.Location.X, plotterDisplayEx5.Location.Y + plotterDisplayEx5.Height + 5);
            plotterDisplayEx9.Location = new Point(plotterDisplayEx9.Location.X, channellabel9.Location.Y + channellabel9.Height + 5);

            plotterDisplayEx2.Location = new Point(plotterDisplayEx2.Location.X, plotterDisplayEx2.Location.Y);
            channel6label.Location = new Point(channel6label.Location.X, plotterDisplayEx2.Location.Y + plotterDisplayEx2.Height + 5);
            plotterDisplayEx6.Location = new Point(plotterDisplayEx6.Location.X, channel5label.Location.Y + channel6label.Height + 5);
            channellabel10.Location = new Point(channellabel10.Location.X, plotterDisplayEx6.Location.Y + plotterDisplayEx6.Height + 5);
            plotterDisplayEx10.Location = new Point(plotterDisplayEx10.Location.X, channellabel10.Location.Y + channellabel10.Height + 5);

            plotterDisplayEx3.Location = new Point(plotterDisplayEx3.Location.X, plotterDisplayEx3.Location.Y);
            channellabel7.Location = new Point(channellabel7.Location.X, plotterDisplayEx3.Location.Y + plotterDisplayEx3.Height + 5);
            plotterDisplayEx7.Location = new Point(plotterDisplayEx7.Location.X, channellabel7.Location.Y + channellabel7.Height + 5);
            channellabel11.Location = new Point(channellabel11.Location.X, plotterDisplayEx7.Location.Y + plotterDisplayEx7.Height + 5);
            plotterDisplayEx11.Location = new Point(plotterDisplayEx11.Location.X, channellabel11.Location.Y + channellabel11.Height + 5);

            plotterDisplayEx4.Location = new Point(plotterDisplayEx4.Location.X, plotterDisplayEx4.Location.Y);
            channellabel8.Location = new Point(channellabel8.Location.X, plotterDisplayEx4.Location.Y + plotterDisplayEx4.Height + 5);
            plotterDisplayEx8.Location = new Point(plotterDisplayEx8.Location.X, channellabel8.Location.Y + channellabel8.Height + 5);
            channellabel12.Location = new Point(channellabel12.Location.X, plotterDisplayEx8.Location.Y + plotterDisplayEx8.Height + 5);
            plotterDisplayEx12.Location = new Point(plotterDisplayEx12.Location.X, channellabel12.Location.Y + channellabel12.Height + 5);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            createCSVFile(adcConfigCollection, directory, ChannelA, ChannelB);
            //backgroundWorker.ReportProgress(100);
        }

        /// <summary>
        /// Continuous running thread which render data on graph
        /// </summary>
        private void DoGraphActivity(ArrayList data)
        {

            try
            {
                if (null != Program.communicator)
                {
                    //Check if stop button is not pressed and data received from device
                    //if (!Program.communicator.isStopClick && Program.communicator.isDataReceived)
                    //Swanand: No need to check isDataReceived
                    if (!Program.communicator.isStopClick)
                    {
                        adcConfigCollection = data;
                        if (adcConfigCollection != null && adcConfigCollection.Count > 0)
                        {
                            DrawGraph(adcConfigCollection);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
            }
        }

        /// <summary>
        /// Save Raw DC values to file
        /// </summary>
        private void saveADCValues()
        {
            createCSVFile(adcConfigCollection, directory, ChannelA, ChannelB);
        }

        /// <summary>
        /// Create CSV file for ADC values
        /// </summary>
        /// <param name="adcConfigCollection"></param>
        /// <param name="directory"></param>
        /// <param name="channel"></param>
        private void createCSVFile(ArrayList adcConfigCollection, string directory, int channelOne,int channelTwo)
        {
            if (SaveFile == true)
            {
                ApplicationUtil.ApplicationUtil.CreateCsvArrayList(adcConfigCollection, directory, channelOne, channelTwo);
            }
        }

        /// <summary>
        /// Set UI default values
        /// </summary>
        private void SetDefaultUI()
        {
            HideShowSaveLabels(false);
            btnNext.Enabled = false;
            btnPrevious.Enabled = false;
            samplingratecomboBox.SelectedIndex = 8;
            radioButtonLive.Checked = true;
            SetHHUMessage(false, false);
            textT1.ReadOnly = false;
            textT2.ReadOnly = false;
            textT3.ReadOnly = false;
            textT4.ReadOnly = false;
            textT5.ReadOnly = false;
            textT6.ReadOnly = false;
            textT7.ReadOnly = false;
            textT8.ReadOnly = false;
            textT9.ReadOnly = false;
            textT10.ReadOnly = false;

            btnSet.Enabled = false;
            txtXRange.Enabled = Continousmoderadiobutton.Checked && Continousmoderadiobutton.Enabled;
            lblDisplayRange.Enabled = true;
            btnSet.Visible = true;
            txtXRange.Visible = true;
            lblDisplayRange.Visible = true;

            btnStop.Visible = false;
            btnStart.Visible = false;

            T5VDACtextBox.ReadOnly = false;
            T7VDACtextBox.ReadOnly = false;

            toolStripConnectionStatus.Text = "Device Disconnected";
           
        }

        //protected override void OnPaintBackground(PaintEventArgs e)
        //{
        //    this.BackColor = SystemColors.Control;
        //    panel1.BackColor = SystemColors.Control;
        //    panelGraph.BackColor = SystemColors.Control;
        //    panel3.BackColor = SystemColors.Control;
        //    panel4.BackColor = SystemColors.Control;
        //    panel5.BackColor = SystemColors.Control;
        //    panel6.BackColor = SystemColors.Control;

        //    panelGraph.BorderStyle = BorderStyle.Fixed3D;
        //    panel1.BorderStyle = BorderStyle.Fixed3D;
        //    panel3.BorderStyle = BorderStyle.Fixed3D;
        //    panel4.BorderStyle = BorderStyle.Fixed3D;
        //    panel5.BorderStyle = BorderStyle.Fixed3D;
        //    panel6.BorderStyle = BorderStyle.Fixed3D;
        //}

        /// <summary>
        /// 
        /// </summary>
        private void deviceCon()
        {
            //panel5.Enabled = false;
            //panel3.Enabled = false;
            //panel4.Enabled = false;
            panel1.Enabled = false;

            //panel5.Enabled = true;
            //panel3.Enabled = true;
            //panel4.Enabled = true;
            //panel1.Enabled = true;
        }

        /// <summary>
        /// Render graph implementaion
        /// </summary>
        private void RenderGraph(PlotterDisplayEx graphControl1, PlotterDisplayEx graphControl2)
        {
            // changes: 2 diff graph controls for each channel that would be shown simultaneuosly 
            try
            {
                if (graphControl1 != null)
                {

                    if (ConfigurationVO.GetInstance().mode_operation != EnumAndConstants.CONTINEOUS)
                    {
                        graphControl1.SetDisplayRangeX(0, xDisplayRange);
                        graphControl1.gPane.grid_distance_x = Math.Round(xDisplayRange / 5f, 1, MidpointRounding.ToEven);
                        //graphControl1.gPane.grid_distance_x =xDisplayRange ;
                    }
                    else
                    {
                        xDisplayRange =int.Parse( txtXRange.Text);
                        graphControl1.SetDisplayRangeX(0, xDisplayRange);
                        graphControl1.gPane.grid_distance_x =(int) (0.70*xDisplayRange);
                    }
                    if(!graphControl1.gPane.isPaintinginProgress)
                    graphControl1.Refresh();
                }
                if (graphControl2 != null)
                {
                    graphControl2.SetDisplayRangeX(0, xDisplayRange);
                    graphControl2.gPane.grid_distance_x = Math.Round(xDisplayRange / 5f, 1, MidpointRounding.ToEven);
                    if (!graphControl1.gPane.isPaintinginProgress)
                    graphControl2.Refresh();
                    
                }
                Program.communicator.isDataReceived = false;
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
            }
        }

        /// <summary>
        /// Set Status os DAS device connection
        /// </summary>
        /// <param name="HHUStatus"></param>
        void DASCommunicator_SetStatusEvent(bool HHUStatus, bool isConfiguration)
        {
            this.Invoke(setDelegateStatus, HHUStatus, isConfiguration);
        }

        /// <summary>
        /// Set Message and Image for HHU
        /// </summary>
        /// <param name="HHUConnected"></param>
        public void SetHHUMessage(bool HHUConnected, bool isConfiguration)
        {
            try
            {
                if (HHUConnected)
                {
                   
                    toolStripConnectionStatus.ForeColor = Color.Green;
                    toolStripConnectionStatus.Text = Settings1.Default.HHU_CONNECTED;
                    
                    btnSetConfiguration.Enabled = true;
                    btnGetConfiguration.Enabled = true;
                    btnStart.Enabled = true;
                    Triggerdmoderadiobutton.Enabled = true;
                    externalRecording.Enabled = true;
                    externalSimulation.Enabled = true;
                    lookAhead.Enabled = true;
                    Continousmoderadiobutton.Enabled = true;
                }
                else
                {
                    Program.communicator.CloseSerialPort();                   
                    toolStripConnectionStatus.ForeColor = Color.Red;
                    toolStripConnectionStatus.Text = Settings1.Default.HHU_DisCONNECTED;

                    Triggerdmoderadiobutton.Enabled = false;
                    externalRecording.Enabled = false;
                    externalSimulation.Enabled = false;
                    lookAhead.Enabled = false;
                    Continousmoderadiobutton.Enabled = false;
                    btnStart.Enabled = false;
                    btnSetConfiguration.Enabled = false;
                    btnGetConfiguration.Enabled = false;
                }
                if (isConfiguration)
                {
                    GetConfigurationFromFile();
                    panel1.Enabled = true;
                    //panel3.Enabled = true;
                    //panel4.Enabled = true;
                    //panel5.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
            }
        }

        /// <summary>
        /// validation and set configurations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetConfigbutton_Click(object sender, EventArgs e)
        {
            if (m_IsInSetConfig)
                return;
            try
            {
                m_IsInSetConfig = true;
                //btnSetConfiguration.Enabled = false;

                this.Cursor = Cursors.WaitCursor;
                UInt16 channelVal = getSelectedChannelValue();

                if (radioButtonhstry.Checked == true)
                {
                    MessageBox.Show("Please select the live mode for operation and then change the configuration", "Mode of operation error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;

                }
                int channelselectionCount = getTheNumberOfChannelsSelected();
                if (Continousmoderadiobutton.Checked == false && lookAhead.Checked == false && channelVal == 0 && channelselectionCount > 2)
                {
                    MessageBox.Show("Please select even number of channels", "Channel selection error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (Continousmoderadiobutton.Checked && channelselectionCount > 1)
                {
                    MessageBox.Show("Only one channel should be selected in the continous mode of operation", "Channel selection error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (externalRecording.Checked && channelselectionCount > 2)
                {
                    MessageBox.Show("Not more than two channels can be selected in the external recording mode of operation", "Channel selection error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (lookAhead.Checked && channelselectionCount > 1)
                {
                    MessageBox.Show("Only one channel should be selected in the look ahead mode of operation", "Channel selection error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // validation of checkboxes and radiobuttons
                //if (!(this.chanlrdiobttn1.Checked || this.chanlrdiobttn2.Checked || this.chanlrdiobttn3.Checked))
                //{
                //    MessageBox.Show(Settings1.Default.chnl, "Channel  Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    return;
                //}
                if (!(this.Triggerdmoderadiobutton.Checked || this.Continousmoderadiobutton.Checked || this.externalRecording.Checked || this.externalSimulation.Checked || this.lookAhead.Checked))
                {
                    MessageBox.Show(Settings1.Default.mode, "Mode of operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // validation of textboxes for null string
                if (String.IsNullOrEmpty(textT1.Text) || String.IsNullOrEmpty(textT3.Text) || String.IsNullOrEmpty(textT4.Text) || String.IsNullOrEmpty(textT5.Text) || String.IsNullOrEmpty(textT9.Text) || String.IsNullOrEmpty(textT2.Text) || String.IsNullOrEmpty(textT6.Text) || String.IsNullOrEmpty(textT7.Text) || String.IsNullOrEmpty(textT8.Text) || String.IsNullOrEmpty(textT10.Text))
                {
                    MessageBox.Show(Settings1.Default.CONFIG_PARAM, "Parameters not filled", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // dependencies and parameter range validations
                Double valueT1 = double.Parse(textT1.Text);
                Double valueT2 = double.Parse(textT2.Text);
                Double valueT3 = double.Parse(textT3.Text);
                Double valueT4 = double.Parse(textT4.Text);
                Double valueT5 = double.Parse(textT5.Text) / 1000;
                Double valueT6 = double.Parse(textT6.Text) / 1000;
                Double valueT7 = double.Parse(textT7.Text) / 1000;
                Double valueT8 = double.Parse(textT8.Text);
                Double valueT9 = double.Parse(textT9.Text) / 1000;
                Double valueT10 = double.Parse(textT10.Text);
                int T5VDACValue = 0;
                int T7VDACValue = 0;
                Double T3Time = valueT3 * valueT8;
                Double T4Time = (valueT1 * 3600) / valueT4;

                Double add = valueT5 + valueT6 + valueT7 + valueT9;

                if (T5VDACtextBox.Text.Contains('.'))
                {
                    MessageBox.Show("Please enter only integer values for T5 VDAC value.(Range: 0-255)", "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    T5VDACValue = int.Parse(T5VDACtextBox.Text);
                }
                if (T7VDACtextBox.Text.Contains('.'))
                {
                    MessageBox.Show("Please enter only integer values for T7 VDAC value.(Range: 0-255)", "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    T7VDACValue = int.Parse(T7VDACtextBox.Text);
                }
                if (T5VDACValue > 255 || T5VDACValue < 0)
                {
                    MessageBox.Show("Please enter a valid value of T5 VDAC value.(Range: 1-255)", "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (T7VDACValue > 255 || T7VDACValue < 0)
                {
                    MessageBox.Show("Please enter a valid value of T7 VDAC value.(Range: 1-255)", "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if ((valueT1 > 6) || (valueT1 < 1))
                {
                    MessageBox.Show(Settings1.Default.param_range, "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if ((valueT2 > 15) || (valueT2 < 1))
                {
                    MessageBox.Show(Settings1.Default.param_rangeT2, "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if ((valueT3 > 50) || (valueT3 < 1))
                {
                    MessageBox.Show(Settings1.Default.param_rangeT3, "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if ((valueT4 > 65535) || (valueT4 < 1))
                {
                    MessageBox.Show(Settings1.Default.param_rangeT4, "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (((valueT5 * 1000) > 5) || ((valueT5 * 1000) < 0.5))
                {
                    MessageBox.Show(Settings1.Default.param_rangeT5, "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (((valueT6 * 1000) > 5) || ((valueT6 * 1000) < 0))
                {
                    MessageBox.Show(Settings1.Default.param_rangeT6, "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (((valueT7 * 1000) > 5) || ((valueT7 * 1000) < 0))
                {
                    MessageBox.Show(Settings1.Default.param_rangeT7, "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if ((valueT8 > 5) || (valueT8 < 0.1))
                {
                    MessageBox.Show(Settings1.Default.param_rangeT8, "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (((valueT9 * 1000) > 10) || ((valueT9 * 1000) < 0))
                {
                    MessageBox.Show(Settings1.Default.param_rangeT9, "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if ((valueT10 > 100) || (valueT10 < 50))
                {
                    MessageBox.Show(Settings1.Default.param_rangeT10, "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!(T4Time > (valueT3 * valueT8)))
                {
                    MessageBox.Show(Settings1.Default.T1T4T3_dependancy, "Dependancy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!((valueT1 * 3600) >= T4Time))
                {
                    MessageBox.Show("Value of T1 and T4 not valid", "Dependancy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (add > valueT8)// the parameter dependncy T5+T6+T7+T9 < T8
                {
                    MessageBox.Show(Settings1.Default.T8T5T6T7dependency, "Dependancy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!(valueT8 * 1000 > valueT10))
                {
                    MessageBox.Show("The value of T8 should be greater than that of T10", "Dependancy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                //validation for samplingrate field
                if (samplingratecomboBox.SelectedIndex == -1)
                {
                    MessageBox.Show(Settings1.Default.samplingrate, "Sampling Rate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }               
                //creating the .csv file and success msg display
                else
                {
                    valueT5 *= 1000;
                    valueT6 *= 1000;
                    valueT7 *= 1000;
                    valueT9 *= 1000;
                    List<ConfigurationVO> configList = new List<ConfigurationVO>();
                    ConfigurationVO configVo = ConfigurationVO.GetInstance();
                    configVo.NumberOfSelectedChannels = getTheNumberOfChannelsSelected();
                    Program.communicator.AdcCurrentDate = DateTime.Now;
                    if (Continousmoderadiobutton.Checked)
                    {
                        configVo.mode_operation = EnumAndConstants.CONTINEOUS;
                    }
                    else if (Triggerdmoderadiobutton.Checked)
                    {
                        configVo.mode_operation = EnumAndConstants.TRIGGER;
                    }
                    else if (externalSimulation.Checked)
                    {
                        configVo.mode_operation = EnumAndConstants.EXTERNAL_SIMULATION;
                    }
                    else if (externalRecording.Checked)
                    {
                        configVo.mode_operation = EnumAndConstants.EXTERNAL_RECORDING;
                    }
                    else if (lookAhead.Checked)
                    {
                        configVo.mode_operation = EnumAndConstants.LOOK_AHEAD;
                    }
                    if (channelselectionCount > 1)
                    {
                        Program.communicator.isBothClicked = true;
                    }
                    Program.Mode_Operation = configVo.mode_operation;
                    configVo.ChannelValue = channelVal;
                    //if (chanlrdiobttn1.Checked)
                    //{
                    //    configVo.channel = 1;
                    //}
                    //else if (chanlrdiobttn2.Checked)
                    //{
                    //    configVo.channel = 2;
                    //}
                    //else
                    //{
                    //   
                    //    configVo.channel = 3;
                    //}

                    configVo.T1 = (int)(valueT1 * EnumAndConstants.COMMON_FACTOR);
                    configVo.T2 = (int)(valueT2 * EnumAndConstants.COMMON_FACTOR);
                    configVo.T3 = (int)(valueT3 * EnumAndConstants.COMMON_FACTOR);
                    configVo.T4 = (int)(valueT4 * EnumAndConstants.COMMON_FACTOR);
                    configVo.T5 = (int)(valueT5 * EnumAndConstants.COMMON_FACTOR);
                    configVo.T6 = (int)(valueT6 * EnumAndConstants.COMMON_FACTOR);
                    configVo.T7 = (int)(valueT7 * EnumAndConstants.COMMON_FACTOR);
                    configVo.T8 = (int)(valueT8 * EnumAndConstants.COMMON_FACTOR);
                    configVo.T9 = (int)(valueT9 * EnumAndConstants.COMMON_FACTOR);
                    configVo.T10 = (int)(valueT10 * EnumAndConstants.COMMON_FACTOR);
                    configVo.T5VDAC = T5VDACValue;
                    configVo.T7VDAC = T7VDACValue;
                    //string selection=(string)samplingratecomboBox.SelectedValue;
                    //double SR=double.Parse(selection);

                    //foreach (var item in samplingRateMap)
                    //{
                    //    if (item.Value == SR)
                    //    {
                    //        configVo.sampaling_rate = item.Key;
                    //        break;
                    //    }
                    //}
                    configVo.sampaling_rate = samplingratecomboBox.SelectedIndex;
                    int adcvalue = 4;
                    configVo.adcresolution = 16;
                    configList.Add(configVo);
                    lblConfigStatus.Text = "";
                    //btnSetConfiguration.Enabled = false;

                    Program.communicator.isNewRequestSend = true;
                    CacheStore.getInstance().comblist = null;
                    CacheStore.getInstance().adcValueList = null;
                    ResponseCode resp = Program.communicator.setConfigurationRequest(configVo);
                    RequestTime = DateTime.Now;
                    if (resp == ResponseCode.SUCCESS)
                    {
                        lock (m_objLockConfig)
                        {
                            Program.communicator.ClearLoggingQueue();
                            SetGraphDefaultValue();
                            clearAllGraphs();
                        }
                        string path = Assembly.GetExecutingAssembly().Location;
                        int edit = path.LastIndexOf("\\");
                        path = path.Remove(edit);
                        path += EnumAndConstants.CONFIGURATION_FILE;

                        if (configVo.mode_operation == 2)
                        {
                            btnStop.Visible = true;
                            btnStart.Visible = true;
                        }

                        //if (configVo.channel == 1)
                        //{
                        //    channel1Label.Visible = true;
                        //    channel2Label.Visible = false;
                        //}
                        //else if (configVo.channel == 2)
                        //{
                        //    channel2Label.Visible = true;
                        //    channel1Label.Visible = false;
                        //}
                        //else
                        //{
                        //    channel1Label.Visible = true;
                        //    channel2Label.Visible = true;
                        //}
                        ApplicationUtil.ApplicationUtil.CreateCSVFromGenericList(configList, path);
                        //zedGraphControl.GraphPane.CurveList.Clear();
                        lblConfigStatus.ForeColor = Color.Green;
                        lblConfigStatus.Text = " Configurations set sucessfully.";
                        // create a folder with date time stamp of the set config action. to dump the file related to those configurations in the folder.
                        string DatetimeStampFolder = Path.Combine(changedpath, ApplicationUtil.ApplicationUtil.GetFileNameWithDateTime(""));
                        //Directory.CreateDirectory(DatetimeStampFolder);
                        //changedpath = DatetimeStampFolder;
                        ApplicationUtil.ApplicationUtil.CurrentFolderPath = DatetimeStampFolder;
                        ApplicationUtil.ApplicationUtil.IsDirectoryCreateAllowed = true;
                        txtXRange.Enabled = Continousmoderadiobutton.Checked;
                        //try
                        //{
                        //    Directory.CreateDirectory(Path.Combine(changedpath, "temp"));
                        //}
                        //catch (Exception xce)
                        //{
                        //    m_logger.Error(xce);
                        //    MessageBox.Show("Cannot create directory for recording due to the target directory permission levels", "Dependancy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //}
                        //Directory.Delete(Path.Combine(changedpath, "temp"));
                        //Configuration oConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        //oConfig.AppSettings.Settings["CurrentFolderPath"].Value = DatetimeStampFolder;
                        //oConfig.Save(ConfigurationSaveMode.Modified);
                        //ApplicationUtil.ApplicationUtil.FirstRecievedChannelValue = 0;
                    }
                    else
                    {
                        if (resp == VOManager.ResponseCode.TIMEOUT)
                            SetHHUMessage(false, false);
                        Program.communicator.isNewRequestSend = false;
                        lblConfigStatus.ForeColor = Color.Red;
                        lblConfigStatus.Text = "Unable to set configurations.";
                    }
                }
            }
            catch (Exception ex)
            {
                Program.communicator.isNewRequestSend = false;
                lblConfigStatus.ForeColor = Color.Red;
                lblConfigStatus.Text = "Unable to set configurations.";
                m_logger.Error(ex);
            }
            finally
            {
                this.Cursor = Cursors.Hand;
                m_IsInSetConfig = false;
            }
        }


        /// <summary>
        /// limits  txtboxes additions to numbers and a decimal point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textT1_KeyPress(object sender, KeyPressEventArgs e)//VALIDATION OF TEXT BOX
        {
            KeyValidation(sender, e);
        }


        /// <summary>
        /// limits  txtbox additions to numbers and a decimal point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textT2_KeyPress(object sender, KeyPressEventArgs e) //VALIDATION OF TEXT BOX
        {
            KeyValidation(sender, e);
        }

        /// <summary>
        /// limits  txtboxes additions to numbers and a decimal point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textT6_KeyPress(object sender, KeyPressEventArgs e)//VALIDATION OF TEXT BOX
        {
            KeyValidation(sender, e);
        }



        /// <summary>
        /// limits  txtboxes additions to numbers and a decimal point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textT3_KeyPress(object sender, KeyPressEventArgs e)//VALIDATION OF TEXT BOX
        {
            KeyValidation(sender, e);
        }


        /// <summary>
        /// limits  txtboxes additions to numbers and a decimal point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textT4_KeyPress(object sender, KeyPressEventArgs e)//VALIDATION OF TEXT BOX
        {
            KeyValidation(sender, e);
        }


        /// <summary>
        /// limits  txtboxes additions to numbers and a decimal point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textT5_KeyPress(object sender, KeyPressEventArgs e)//VALIDATION OF TEXT BOX
        {
            KeyValidation(sender, e);
        }



        /// <summary>
        /// limits  txtboxes additions to numbers and a decimal point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textT9_KeyPress(object sender, KeyPressEventArgs e)//VALIDATION OF TEXT BOX
        {
            KeyValidation(sender, e);
        }

        /// <summary>
        /// limits  txtboxes additions to numbers and a decimal point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textT7_KeyPress(object sender, KeyPressEventArgs e)//VALIDATION OF TEXT BOX
        {
            KeyValidation(sender, e);
        }

        /// <summary>
        /// Validation of key entered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyValidation(object sender, KeyPressEventArgs e)
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


        /// <summary>
        /// limits  txtboxes additions to numbers and a decimal point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textT8_KeyPress(object sender, KeyPressEventArgs e)//VALIDATION OF TEXT BOX
        {
            KeyValidation(sender, e);
        }

        /// <summary>
        /// limits  txtboxes additions to numbers and a decimal point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textT10_KeyPress(object sender, KeyPressEventArgs e)//VALIDATION OF TEXT BOX
        {
            KeyValidation(sender, e);
        }

        /// <summary>
        /// SHOW START N STOP BUYTTON AFTER CLICKING ON CONTINOUS MODE
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Continousmoderadiobutton_Click(object sender, EventArgs e)//
        {
            btnStop.Visible = false;
            btnStart.Visible = false;
            txtXRange.Enabled = true;
            btnSet.Enabled = true;
            //chanlrdiobttn3.Visible = false;
            //chanlrdiobttn1.Checked = true;
            //chanlrdiobttn1.Enabled = true;
            //chanlrdiobttn2.Enabled = true;
            HideShowSaveLabels(true);
        }

        private void HideShowSaveLabels(bool flag)
        {
            continuousFileSaveChk.Visible = true;
        }


        /// <summary>
        /// hide start and stop buttons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Triggerdmoderadiobutton_Click(object sender, EventArgs e)//
        {
            btnStop.Visible = false;
            btnStart.Visible = false;
            txtXRange.Enabled = false;
            btnSet.Enabled = false;
            //chanlrdiobttn1.Enabled = true;
            //chanlrdiobttn2.Enabled = true;
            //chanlrdiobttn3.Enabled = true;
            //chanlrdiobttn3.Visible = true;
            HideShowSaveLabels(false);
        }

        /// <summary>
        /// shows the hstry related buttons(date,time etc)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButtonhstry_Click(object sender, EventArgs e)//
        {
            try
            {
                Program.IsOnlineMode = false;
                SetGraphDefaultValue();
                OpenFileDialog openFileDialoghstry = new OpenFileDialog();
                openFileDialoghstry.Filter = "CSV files | *.csv";
                if (changedpath == null)
                    openFileDialoghstry.InitialDirectory = ApplicationUtil.ApplicationUtil.GetApplicationPath();
                else
                    openFileDialoghstry.InitialDirectory = changedpath;
                //openFileDialoghstry.Multiselect = true;
                if (openFileDialoghstry.ShowDialog() == DialogResult.OK)
                {
                    sourceFile = openFileDialoghstry.FileName;

                    ArrayList collection = ApplicationUtil.ApplicationUtil.ReadconfigadcCsvfile(sourceFile);

                    if (collection == null || collection.Count < 2)
                        return;

                    int index = sourceFile.LastIndexOf("\\");
                    string sfile = sourceFile.Remove(index);
                    filenames = ApplicationUtil.ApplicationUtil.GetSubDirectoryFiles(sfile, "*.csv");
                    if (filenames.Count > 0)
                    {
                        btnPrevious.Enabled = true;
                        btnNext.Enabled = true;
                    }
                    sourceFile = sourceFile.Substring(index + 1);
                    ApplicationUtil.ApplicationUtil.CurrentFolderPath = sfile;
                    // list of elements in the file
                    //TODO: MAitreyee
                    DrawGraph(collection);
                }
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
            }
        }

        

        /// <summary>
        /// Draw graph
        /// </summary>
        /// <param name="collection"></param>
        private void DrawGraph(ArrayList collection)
        {
            PlotterDisplayEx graphControl1 = new PlotterDisplayEx();
            PlotterDisplayEx graphControl2 = new PlotterDisplayEx();
            lock (m_objLockConfig)
            {
                int channel = 1;

                List<ConfigurationVO> configlist = (List<ConfigurationVO>)collection[0];
                int channel_ref = configlist[0].ChannelValue;

                int channelA = 0;
                int channelB = 0;
                if ((ConfigurationVO.GetInstance().mode_operation != EnumAndConstants.CONTINEOUS))
                {
                    if (((List<ADCVO>)collection[1])[0].Channel != 0)
                    {
                        channelA = ((List<ADCVO>)collection[1])[0].Channel;
                    }
                    if (collection.Count > 2)
                    {
                        channelB = ((List<ADCVO>)collection[2])[2].Channel;
                    }
                }
                else
                {
                    List<int> channels = new List<int>();
                    ApplicationUtil.ApplicationUtil.GetValueFromNumber((UInt16)channel_ref, out channels);
                    channelA = channels[0];
                }

                // chek for channel values being -1
                if (channelA != 0)
                {
                    graphControl1 = GetTargetGraphControlForChannelB(channelA);
                }
                if (channelB != 0)
                {
                    graphControl2 = GetTargetGraphControlForChannelB(channelB);
                }
                else
                    graphControl2 = null;

                List<ADCVO> adclist = (List<ADCVO>)collection[1];
                //Calculate the max and min y axis values to initialize the graph
                ApplicationUtil.ApplicationUtil.CalculateMinMaxADCValues(adclist);

                int adcResolution = configlist[0].adcresolution;
                if (adclist.Count == 0)
                {
                    return;
                }

                repeatCount = adclist.Count;
                int maxadcForChannelTwo = 0;
                List<ADCVO> adclistChannelTwo = new List<ADCVO>();
                if (collection.Count == 3)
                {
                    adclistChannelTwo = (List<ADCVO>)collection[2];
                    if (adclistChannelTwo.Count > 0)
                        maxadcForChannelTwo = adclistChannelTwo[adclistChannelTwo.Count - 1].AdcValue;
                }
                else
                {
                    channel = channel_ref;
                }

                DateTime minDate = DateTime.Now;
                minDate = adclist[0].AdcDate;

                if (null != firstChannelArray)
                {
                    if (firstChannelArray.Count >= 100000)
                    {
                        firstChannelArray.RemoveRange(0, 25000);
                    }
                }

                graphControl1.DataSources[0].GraphColor = Color.Green;

                if (firstChannelArray == null)
                {
                    firstChannelArray = new List<cPoint>();
                    firstChannelY = 0;
                }
                if (Program.Mode_Operation != EnumAndConstants.CONTINEOUS)
                {
                    firstChannelArray = new List<cPoint>();
                    firstChannelY = 0;
                }

                if (Program.IsOnlineMode && Program.Mode_Operation != configlist[0].mode_operation)
                    return;
                if (configlist[0].mode_operation != EnumAndConstants.CONTINEOUS)
                {
                    firstChannelArray = new List<cPoint>();
                    firstChannelY = 0;
                }

                int tempCounter = 0;

                while (tempCounter < adclist.Count)
                {
                    ADCVO point = adclist.ElementAt(tempCounter);
                    double adv = Convert.ToDouble(point.AdcValue);
                    firstChannelY += ApplicationUtil.ApplicationUtil.SAMPLING_RATE_DATE_ADD_VALUE;
                    string d = string.Format("{0:0.00}", firstChannelY);
                    double xPoint = double.Parse(d);
                    cPoint cpoint = new cPoint();
                    cpoint.x = xPoint;
                    cpoint.y = adv;
                    firstChannelArray.Add(cpoint);
                    tempCounter++;
                }

                if ((ConfigurationVO.GetInstance().mode_operation != EnumAndConstants.CONTINEOUS))
                {
                    xDisplayRange = (int)(firstChannelArray.ElementAt(firstChannelArray.Count - 1)).x;
                }
                //xDisplayRange =(int)((0.30 * maxX));
                if (graphControl1.DataSources.Count > 1)
                    graphControl1.DataSources.RemoveRange(1, graphControl1.DataSources.Count - 1);
                //initialize the control as per the adc values and x axis values.
                graphControl1.DataSources[0].Samples = firstChannelArray.ToArray<cPoint>();


                if (graphControl1.DataSources[0].YMax == 0 && graphControl1.DataSources[0].YMin == 0)
                {
                    graphControl1.DataSources[0].YMax = ApplicationUtil.ApplicationUtil.maxADC;
                    
                    graphControl1.DataSources[0].YD1 = ApplicationUtil.ApplicationUtil.maxADC;
                    graphControl1.DataSources[0].YMin = ApplicationUtil.ApplicationUtil.minADC;
                    graphControl1.DataSources[0].YD0 = ApplicationUtil.ApplicationUtil.minADC;
                }
                else
                {
                    if (graphControl1.gPane.maxY < ApplicationUtil.ApplicationUtil.maxADC)
                    {
                        graphControl1.DataSources[0].YMax = ApplicationUtil.ApplicationUtil.maxADC;
                        graphControl1.DataSources[0].YD1 = ApplicationUtil.ApplicationUtil.maxADC;
                    }
                    if (graphControl1.gPane.minY > ApplicationUtil.ApplicationUtil.minADC)
                    {
                        graphControl1.DataSources[0].YMin = ApplicationUtil.ApplicationUtil.minADC;
                        graphControl1.DataSources[0].YD0 = ApplicationUtil.ApplicationUtil.minADC;
                    }
                }
                //graphControl1.DataSources[0].AutoScaleY = true;
                graphControl1.DataSources[0].off_Y = 0;
                graphControl1.gPane.off_X = 0;
                graphControl1.gPane.grid_distance_y = 5;
                
                List<PlotterDisplayEx> graphList = new List<PlotterDisplayEx>();
                graphList.Add(graphControl1);
                DrawGraphWithZoom(graphList,ZoomMultiplier);
                if (graphControl2 != null)
                {
                    if (graphControl2.DataSources.Count > 1)
                        graphControl2.DataSources.RemoveRange(1, graphControl2.DataSources.Count - 1);
                }
                if (collection.Count == 3)
                {
                    if (adclistChannelTwo.Count == 0)
                        return;
                    minDate = adclistChannelTwo[0].AdcDate;
                    ApplicationUtil.ApplicationUtil.CalculateMinMaxADCValues(adclistChannelTwo);
                    if (channelB != -1)
                    {
                        if (graphControl2 != null)
                        {
                            if (secondChannelArray == null || configlist[0].mode_operation == EnumAndConstants.TRIGGER || configlist[0].mode_operation == EnumAndConstants.EXTERNAL_SIMULATION || configlist[0].mode_operation == EnumAndConstants.EXTERNAL_RECORDING || configlist[0].mode_operation == EnumAndConstants.LOOK_AHEAD)
                            {
                                secondChannelArray = new List<cPoint>();
                                secondChannelY = 0;
                            }

                            tempCounter = 0;
                            while (tempCounter < adclistChannelTwo.Count)
                            {
                                ADCVO point = adclistChannelTwo.ElementAt(tempCounter);
                                double adv = Convert.ToDouble(point.AdcValue);
                                secondChannelY += ApplicationUtil.ApplicationUtil.SAMPLING_RATE_DATE_ADD_VALUE;
                                string d = string.Format("{0:0.00}", secondChannelY);
                                double yPoint = double.Parse(d);
                                cPoint cpoint = new cPoint();
                                cpoint.x = yPoint;
                                cpoint.y = adv;
                                secondChannelArray.Add(cpoint);
                                tempCounter++;
                            }
                            // Changes:MAITREYEE

                            graphControl2.DataSources[0].Samples = secondChannelArray.ToArray<cPoint>();
                            graphControl2.DataSources[0].SetGridOriginY(0f);
                            graphControl2.DataSources[0].GraphColor = Color.Green;
                            // graphControl2.DataSources[0].Samples = firstChannelArray.ToArray<cPoint>();
                            if (graphControl2.DataSources[0].YMax == 0 && graphControl2.DataSources[0].YMin == 0)
                            {
                                graphControl2.DataSources[0].YMax = ApplicationUtil.ApplicationUtil.maxADC;

                                graphControl2.DataSources[0].YD1 = ApplicationUtil.ApplicationUtil.maxADC;
                                graphControl2.DataSources[0].YMin = ApplicationUtil.ApplicationUtil.minADC;
                                graphControl2.DataSources[0].YD0 = ApplicationUtil.ApplicationUtil.minADC;
                            }
                            else
                            {
                                if (graphControl2.gPane.maxY < ApplicationUtil.ApplicationUtil.maxADC)
                                {
                                    graphControl2.DataSources[0].YMax = ApplicationUtil.ApplicationUtil.maxADC;
                                    graphControl2.DataSources[0].YD1 = ApplicationUtil.ApplicationUtil.maxADC;
                                }
                                if (graphControl2.gPane.minY > ApplicationUtil.ApplicationUtil.minADC)
                                {
                                    graphControl2.DataSources[0].YMin = ApplicationUtil.ApplicationUtil.minADC;
                                    graphControl2.DataSources[0].YD0 = ApplicationUtil.ApplicationUtil.minADC;
                                }
                            }
                            graphControl2.DataSources[0].off_Y = 0;
                            graphControl2.gPane.off_X = 0;
                            graphControl2.gPane.grid_distance_y = 5;

                            graphList = new List<PlotterDisplayEx>();
                            graphList.Add(graphControl2);
                            DrawGraphWithZoom(graphList, ZoomMultiplier);
                        }
                    }
                }

            }
            if ((DateTime.Now - secondGraphRefreshTime).TotalMilliseconds > graphRefreshTime)
            {
                this.Invoke(graphdelegate, graphControl1, graphControl2);
                secondGraphRefreshTime = DateTime.Now;
            }
        }

        

        private PlotterDisplayEx GetTargetGraphControlForChannelB(int channelB)
        {
            if(channelB!=-1)
            {
            switch (channelB)
            {
                case 1: return plotterDisplayEx1;
                    
                case 2: return plotterDisplayEx2;
                    
                case 3: return plotterDisplayEx3;
                    
                case 4: return plotterDisplayEx4;
                    
                case 5: return plotterDisplayEx5;
                    
                case 6: return plotterDisplayEx6;
                    
                case 7: return plotterDisplayEx7;
                   
                case 8: return plotterDisplayEx8;
                    
                case 9: return plotterDisplayEx9;
                    
                case 10: return plotterDisplayEx10;
                   
                case 11: return plotterDisplayEx11;
                   
                case 12: return plotterDisplayEx12;
                    

            }
            }

            return null;
        }

        /// <summary>
        /// Change location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void changelocationbutton_Click(object sender, EventArgs e)
        {
            int a = 0, b = 0;
            
            try
            {
                Configuration oConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                string DirectoryPath = ConfigurationManager.AppSettings["DirectoryPath"];

                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    changedpath = folderBrowserDialog.SelectedPath;
                    lblDirectorypath.Text = changedpath;
                    oConfig.AppSettings.Settings["DirectoryPath"].Value = folderBrowserDialog.SelectedPath;
                    oConfig.Save(ConfigurationSaveMode.Modified);
                }
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
            }
        }

        private void radioButtonLive_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonLive.Checked)
            {
                Program.IsOnlineMode = true;
                btnNext.Enabled = false;
                btnPrevious.Enabled = false;
            }
        }
        /// <summary>
        /// Showing next graph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void nxtbutton_Click(object sender, EventArgs e)
        {

            //GET THE FILE NAME LIST
            try
            {
                //nextTest();
                int i = 0;
                int j = 0;
                string fname = null;

                // GET THE CURRENT FILE  INDEX AND ITS SUCCESSOR FILE INDEX
                string file_to_use = null;

                do
                {

                    fname = filenames[i];
                    i++;
                    if (i >= filenames.Count)
                    {
                        MessageBox.Show("This is the last file in collection", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //btnNext.Enabled = false;
                        //btnPrevious.Enabled = true;
                        i = filenames.Count-1;
                        return;
                    }
                    else
                    {
                        btnNext.Enabled = true;
                        btnPrevious.Enabled = true;
                    }
                } while (!(fname == sourceFile));

                file_to_use = filenames[i];
                sourceFile = file_to_use;

                ArrayList collection = ApplicationUtil.ApplicationUtil.ReadconfigadcCsvfile(Path.Combine(ApplicationUtil.ApplicationUtil.CurrentFolderPath, file_to_use));
                List<ADCVO> adclist = (List<ADCVO>)collection[1];
                SetGraphDefaultValue();
                if (adclist.Count == 0)
                {
                    MessageBox.Show("This file is empty or in a invalid format.Please select another file", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    //MAITREYEE
                    DrawGraph(collection);
                }
                //    //PLOT THE GRAPH BASED ON THE FILE
            }
            catch (Exception E)
            {
                m_logger.Error(E);
                return;
            }
        }

        /// <summary>
        /// showing previous path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Prevbutton_Click(object sender, EventArgs e)
        {
            try
            {
                //previousTest();
                int j = 0;
                int i = 0;
                string fname = null;
                // GET THE CURRENT FILE  INDEX AND ITS SUCCESSOR FILE INDEX
                string file_to_use = null;
                do
                {
                    fname = filenames[i];
                    i++;
                    if (i > 0)
                    {
                        btnPrevious.Enabled = true;
                    }

                } while (!(fname == sourceFile));

                i -= 2;
                if (i < 0)
                {
                    //btnPrevious.Enabled = false;
                    //btnNext.Enabled = true;
                    MessageBox.Show(Settings1.Default.First_File, "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    i = 0;
                    return;
                }

                //READ THE Previous FILE
                file_to_use = filenames[i];
                sourceFile = file_to_use;
                ArrayList collection = ApplicationUtil.ApplicationUtil.ReadconfigadcCsvfile(Path.Combine(ApplicationUtil.ApplicationUtil.CurrentFolderPath, file_to_use));
                //PLOT THE GRAPH BASED ON THE FILE
                List<ADCVO> adclist = (List<ADCVO>)collection[1];
                if (adclist.Count == 0)
                {
                    MessageBox.Show("This file is empty or in a invalid format.Please select another file", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SetGraphDefaultValue();
                }
                else
                {
                    //TODO:MAITREYEE:For test
                    SetGraphDefaultValue();
                    DrawGraph(collection);
                    DrawGraphWithZoom(getTheActiveGraphControls(), ZoomMultiplier);
                }
            }
            catch (Exception E)
            {
                m_logger.Error(E);
                return;
            }
        }

        /// <summary>
        /// Start continuous mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Start_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                SetGraphDefaultValue();
                ApplicationUtil.ApplicationUtil.adcValueList = new List<ADCVO>();
                //DialogResult confirm = MessageBox.Show("Do you want to save the files created?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                //if (confirm == DialogResult.Yes)
                //{
                //    PacketInterpreter.DASCommunicator.confirmation = true;
                //    SaveFile = true;
                //    ApplicationUtil.ApplicationUtil.savefile = true;
                //    ApplicationUtil.ApplicationUtil.isSaveADCValuesOnly = false;
                //}
                //else
                //{
                //    PacketInterpreter.DASCommunicator.confirmation = false;
                //    SaveFile = false;
                //    ApplicationUtil.ApplicationUtil.isSaveADCValuesOnly = false;
                //    ApplicationUtil.ApplicationUtil.savefile = false;
                //}

                if (continuousFileSaveChk.Checked == true)
                {
                    VOManager.DASCommunicator.confirmation = true;
                    SaveFile = true;
                    ApplicationUtil.ApplicationUtil.savefile = true;
                    ApplicationUtil.ApplicationUtil.isSaveADCValuesOnly = false;
                }
                else
                {
                    VOManager.DASCommunicator.confirmation = false;
                    SaveFile = false;
                    ApplicationUtil.ApplicationUtil.isSaveADCValuesOnly = false;
                    ApplicationUtil.ApplicationUtil.savefile = false;
                }

                if (Program.communicator.startContinousRequest() == ResponseCode.SUCCESS)
                {
                    Program.communicator.isContineousStartClick = true;
                    
                }
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
            }
            finally
            {
                this.Cursor = Cursors.Hand;
            }
        }

        /// <summary>
        /// Stop continuous mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stop_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                int retryCount = 0;
                while (true)
                {
                    Program.communicator.isStopClick = true;

                    if (Program.communicator.stopContinousRequest() == ResponseCode.SUCCESS)
                    {
                        CacheStore.getInstance().comblist = null;
                        CacheStore.getInstance().adcValueList = null;
                        Program.communicator.isContineousStartClick = false;
                        ApplicationUtil.ApplicationUtil.previousValue = 0;
                        ApplicationUtil.ApplicationUtil.adcValueList = new List<ADCVO>();
                        ApplicationUtil.ApplicationUtil.isSaveADCValuesOnly = false;
                        Triggerdmoderadiobutton.Enabled = true;
                        externalRecording.Enabled = true;
                        externalSimulation.Enabled = true;
                        lookAhead.Enabled = true;
                        btnStart.Enabled = true;
                        if (SaveFile == false)
                        {
                            if (!String.IsNullOrEmpty(CacheStore.getInstance().directory))
                            {
                                File.Delete(CacheStore.getInstance().directory);
                            }
                        }
                        break;
                    }
                    else
                    {
                        if (retryCount == EnumAndConstants.STOP_RETRY_COUNT)
                            break;
                        retryCount++;
                    }
                }
                Program.communicator.isStopClick = false;
                VOManager.DASCommunicator.confirmation = false;
                btnSetConfiguration.Enabled = true;
                btnGetConfiguration.Enabled = true;
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
            }
            finally
            {
                this.Cursor = Cursors.Hand;
            }
        }

        /// <summary>
        /// Close the Port setting
        /// </summary>
        private void CloseAndSendHandshake()
        {
            Program.communicator.DeInit();
            if (Program.communicator.Init(EnumAndConstants.comportVO) == true)
            {
                Program.MainScreen.ConnectAndGetData();
            }
        }

        private void ChangeGraphStatus(bool changeFlag)
        {
        }

        /// <summary>
        /// show port details
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void btnComPort_Click(object sender, EventArgs e)
        {
            PortDetails portDetails = new PortDetails();
            portDetails.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ConnectAndGetData();
        }

        /// <summary>
        /// Set graph to default 
        /// </summary>
        private void SetGraphDefaultValue()
        {
            //plotterDisplayEx1.DataSources[0].Samples = new cPoint[0];
            //plotterDisplayEx1.DataSources[1].Samples = new cPoint[0];
            //plotterDisplayEx1.Refresh();
            clearAllGraphs();
            firstChannelArray = null;
            secondChannelArray = null;
            firstChannelY = 0;
            secondChannelY = 0;
            //txtXRange.Text = "";
            //lblDisplayRange.Enabled = true;
            //txtXRange.Enabled = true;
            //btnSet.Enabled = true;
        }

        /// <summary>
        /// Get configuration details from device.
        /// </summary>
        public bool ConnectAndGetData()
        {
            
            Program.communicator.isNewRequestSend = true;
            if (Program.communicator.handshakeWirelessDevice() == ResponseCode.SUCCESS)
            {
                SetGraphDefaultValue();
                panel1.Enabled = true;
                //panel3.Enabled = true;
                //panel4.Enabled = true;
                //panel5.Enabled = true;
                Program.communicator.isNewRequestSend = true;
                if (Program.communicator.getConfigurationDetails() == ResponseCode.SUCCESS)
                {
                    SetHHUMessage(true, false);
                    GetConfigurationFromFile();
                    return true;
                }
                else
                {
                    SetHHUMessage(false, false);
                    Program.communicator.isNewRequestSend = false;
                }
            }
            else
            {
                MessageBox.Show(Settings1.Default.CONNECTION2, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        public IEnumerable<Control> GetChildrenRecursive(Control parent)
        {
            var controls = new List<Control>();
            foreach (Control child in parent.Controls)
                controls.AddRange(GetChildrenRecursive(child));
            controls.Add(parent); //fix
            return controls;
        }


        private void GetConfigbtn_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;


                lblConfigStatus.Text = "";
                btnGetConfiguration.Enabled = false;
                Program.communicator.isNewRequestSend = true;
                if (Program.communicator.serialPort == null || Program.communicator.serialPort.IsOpen == false)
                {
                    Program.communicator.isNewRequestSend = false;
                    MessageBox.Show(Settings1.Default.CONNECTION1, "Serial port not open", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                ResponseCode resp = Program.communicator.getConfigurationDetails();
                RequestTime = DateTime.Now;
                if (resp == ResponseCode.SUCCESS)
                {
                    SetGraphDefaultValue();
                    GetConfigurationFromFile();
                    lblConfigStatus.Text = "";
                    lblConfigStatus.Text = " Configurations fetched successfully.";
                    lblConfigStatus.ForeColor = Color.Green;
                    btnGetConfiguration.Enabled = true;
                }
                else
                {
                    if (resp == ResponseCode.TIMEOUT)
                    {
                        SetHHUMessage(false, false);
                    }
                    Program.communicator.isNewRequestSend = false;
                    lblConfigStatus.ForeColor = Color.Red;
                    lblConfigStatus.Text = " Unable to fetch configurations.";
                }
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
            }
            finally
            {
                this.Cursor = Cursors.Hand;
            }
        }

        /// <summary>
        /// Get configuration from File
        /// </summary>
        private void GetConfigurationFromFile()
        {
            try
            {
                List<ConfigurationVO> configList = ApplicationUtil.ApplicationUtil.ReadconfigCsvfile();
                if (configList == null || configList.Count == 0)
                    return;

                ConfigurationVO configVo = configList[0] as ConfigurationVO;
                int channel_ref = configList[0].ChannelValue;

                List<int> channels = new List<int>();
                ApplicationUtil.ApplicationUtil.GetValueFromNumber((UInt16)channel_ref, out channels);
                for (int count = 0; count < channels.Count; count++)
                {
                    CheckBox selectedChannelcontrol = new CheckBox();
                    selectedChannelcontrol = getTargetChannelCheckBox(channels.ElementAt(count));
                    selectedChannelcontrol.Checked = true;
                }


                textT1.Text = Convert.ToString(configVo.T1 / EnumAndConstants.COMMON_FACTOR);
                textT2.Text = Convert.ToString(configVo.T2 / EnumAndConstants.COMMON_FACTOR);
                textT3.Text = Convert.ToString(configVo.T3 / EnumAndConstants.COMMON_FACTOR);
                textT4.Text = Convert.ToString(configVo.T4 / EnumAndConstants.COMMON_FACTOR);
                textT5.Text = Convert.ToString(configVo.T5 / EnumAndConstants.COMMON_FACTOR);
                textT6.Text = Convert.ToString(configVo.T6 / EnumAndConstants.COMMON_FACTOR);
                textT7.Text = Convert.ToString(configVo.T7 / EnumAndConstants.COMMON_FACTOR);
                textT8.Text = Convert.ToString(configVo.T8 / EnumAndConstants.COMMON_FACTOR);
                textT9.Text = Convert.ToString(configVo.T9 / EnumAndConstants.COMMON_FACTOR);
                textT10.Text = Convert.ToString(configVo.T10 / EnumAndConstants.COMMON_FACTOR);
                T5VDACtextBox.Text = Convert.ToString(configVo.T5VDAC);
                T7VDACtextBox.Text = Convert.ToString(configVo.T7VDAC);


                if (configVo.mode_operation == EnumAndConstants.TRIGGER)
                {
                    Triggerdmoderadiobutton.Checked = true;
                    // chanlrdiobttn3.Visible = true;
                }
                else if (configVo.mode_operation == EnumAndConstants.CONTINEOUS)
                {
                    Continousmoderadiobutton.Checked = true;
                    btnStart.Visible = true;
                    btnStop.Visible = true;
                    //chanlrdiobttn3.Visible = false;
                }
                else if (configVo.mode_operation == EnumAndConstants.EXTERNAL_SIMULATION)
                {
                    externalSimulation.Checked = true;
                    externalSimulation_Click(null, null);
                }
                else if (configVo.mode_operation == EnumAndConstants.EXTERNAL_RECORDING)
                {
                    externalRecording.Checked = true;
                    externalRecording_Click(null, null);
                }
                else if (configVo.mode_operation == EnumAndConstants.LOOK_AHEAD)
                {
                    lookAhead.Checked = true;
                    lookAhead_Click(null, null);
                }
                //write a funtion to translate the channel value from the bits to the actual channels to be selected.

                if (configVo.channel == 1)
                {
                    //chanlrdiobttn1.Checked = true;
                }
                else if (configVo.channel == 2)
                {
                    //chanlrdiobttn2.Checked = true;
                }
                else
                {
                    //chanlrdiobttn3.Checked = true;
                }
                samplingratecomboBox.SelectedIndex = configVo.sampaling_rate;
                //comboBox1.SelectedIndex = config.adcresolution - 1;
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
            }
            finally
            {
                
            }
        }

        /// <summary>
        /// Reset timing configuration values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void resetbttn_Click(object sender, EventArgs e)
        {
            
            DialogResult dialogResult = MessageBox.Show("Do you want to reset the timing configurations values", "Reset values confirmation", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                textT1.Text = Convert.ToString(EnumAndConstants.T1);
                textT2.Text = Convert.ToString(EnumAndConstants.T2);
                textT3.Text = Convert.ToString(EnumAndConstants.T3);
                textT4.Text = Convert.ToString(EnumAndConstants.T4);
                textT5.Text = Convert.ToString(EnumAndConstants.T5);
                textT6.Text = Convert.ToString(EnumAndConstants.T6);
                textT7.Text = Convert.ToString(EnumAndConstants.T7);
                textT8.Text = Convert.ToString(EnumAndConstants.T8);
                textT9.Text = Convert.ToString(EnumAndConstants.T9);
                textT10.Text = Convert.ToString(EnumAndConstants.T10);

            }

        }

        private void toolStripStatusLabel_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Settings1.Default.URL);
        }

        /// <summary>
        /// Open Guidance manual
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void helpButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Settings1.Default.GUIADANCE);
        }

        /// <summary>
        /// Open Help document
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            string HelpFilePath = ApplicationUtil.ApplicationUtil.GetApplicationPath();
            HelpFilePath += EnumAndConstants.HELP_FILE;
            if (File.Exists(HelpFilePath) == true)
            {
                System.Diagnostics.Process.Start(HelpFilePath);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Program.communicator.DummyADC();
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {

            Program.communicator.DummyADC();
        }

        private void Triggerdmoderadiobutton_CheckedChanged(object sender, EventArgs e)
        {
           
            ChangeGraphStatus(false);
        }

        private void Continousmoderadiobutton_CheckedChanged(object sender, EventArgs e)
        {
            
            ChangeGraphStatus(false);
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            List<ConfigurationVO> configlist = ApplicationUtil.ApplicationUtil.ReadconfigCsvfile();
            List<ADCVO> adcList = new List<ADCVO>();
            DateTime date = DateTime.Now;
            for (int i = 0; i < 10; i++)
            {
                ADCVO adc = new ADCVO();
                adc.AdcValue = i;
                date = date.AddMilliseconds(10);
                adc.AdcDate = date;

                adcList.Add(adc);
            }
            ArrayList comblist = new ArrayList();
            comblist.Add(configlist);
            comblist.Add(adcList);
            ApplicationUtil.ApplicationUtil.CreateCsvArrayList(comblist, @"D:\RamshurRatApp\Application\RamshurRatApp\bin\x86\Debug\try.csv", 1,0);

        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtXRange.Text))
            {
                MessageBox.Show(Settings1.Default.xrange_empty, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int enterValue = Convert.ToInt32(txtXRange.Text);
            if (enterValue < 1 || enterValue > 1000)
            {
                MessageBox.Show(Settings1.Default.xrange_less_error, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("xDisplayRange");
            config.AppSettings.Settings.Add("xDisplayRange", txtXRange.Text);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            xDisplayRange = Convert.ToInt32(txtXRange.Text);
            MessageBox.Show("The x display range changed to"+ " "+ xDisplayRange, "Success", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void txtXRange_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void externalSimulation_Click(object sender, EventArgs e)
        {
            //chanlrdiobttn1.Enabled = false;
            //chanlrdiobttn2.Enabled = false;
            //chanlrdiobttn3.Enabled = false;
            //chanlrdiobttn3.Visible = false;
            btnStart.Visible = false;
            btnStop.Visible = false;
            txtXRange.Enabled = false;
            btnSet.Enabled = false;
            HideShowSaveLabels(false);
        }

        private void externalRecording_Click(object sender, EventArgs e)
        {
            //chanlrdiobttn1.Enabled = false;
            //chanlrdiobttn2.Enabled = false;
            //chanlrdiobttn3.Visible = true;
            //chanlrdiobttn3.Enabled = true;
            //chanlrdiobttn3.Checked = true;
            btnStart.Visible = false;
            btnStop.Visible = false;
            txtXRange.Enabled = false;
            btnSet.Enabled = false;
            HideShowSaveLabels(false);
        }

        private void lookAhead_Click(object sender, EventArgs e)
        {
            //chanlrdiobttn1.Enabled = true;
            //chanlrdiobttn2.Enabled = true;
            //chanlrdiobttn1.Checked = true;
            //chanlrdiobttn3.Checked = false;
            //chanlrdiobttn3.Enabled = false;
            //chanlrdiobttn3.Visible = false;
            btnStart.Visible = false;
            btnStop.Visible = false;
            txtXRange.Enabled = false;
            btnSet.Enabled = false;
            HideShowSaveLabels(false);
        }

        /// <summary>
        /// Checked it save file check box is selected or not.
        /// Depending on checked state we have to save file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void continuousFileSaveChk_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).CheckState == CheckState.Checked)
            {
                SaveFile = true;
            }
            else
            {
                SaveFile = false;
            }
        }

        private void plotterDisplayEx1_Load(object sender, EventArgs e)
        {
            
            ((PlotterDisplayEx)sender).DataSources.Add(new DataSource());
            //((PlotterDisplayEx)sender).DataSources.Add(new DataSource());
            //plotterDisplayEx1.SetDisplayRangeX(0, 100);
            //((PlotterDisplayEx)sender).DataSources[0].SetGridOriginY(0f);
            //((PlotterDisplayEx)sender).DataSources[0].grid_distance_y=2;
            //((PlotterDisplayEx)sender).DataSources[0].grid_off_y = 2;
            //((PlotterDisplayEx)sender).PlaySpeed = 1;
            //((PlotterDisplayEx)sender).SetGridOriginX(0);
            //((PlotterDisplayEx)sender).gPane.grid_distance_x = 2;
           // ((PlotterDisplayEx)sender).gPane.XaxisName = "Time in milliseconds";
            //((PlotterDisplayEx)sender).gPane.YaxisName = "ADC values";

        }

        public static UInt16 ConvertToByte(BitArray bits)
        {
            if (bits.Count == 0)
            {
                return 0;
            }
            if (bits.Count != 16)
            {
                throw new ArgumentException("illegal number of bits");
            }

            UInt16 b = 0;
            if (bits.Get(0)) b++;
            if (bits.Get(1)) b += 2;
            if (bits.Get(2)) b += 4;
            if (bits.Get(3)) b += 8;
            if (bits.Get(4)) b += 16;
            if (bits.Get(5)) b += 32;
            if (bits.Get(6)) b += 64;
            if (bits.Get(7)) b += 128;
            if (bits.Get(8)) b += 256;
            if (bits.Get(9)) b += 512;
            if (bits.Get(10)) b += 1024;
            if (bits.Get(11)) b += 2048;
            if (bits.Get(12)) b += 4096;
            if (bits.Get(13)) b += 8192;
            return b;
        }

        private UInt16 getSelectedChannelValue()
        {
            bool[] collection = new bool[16];
            List<bool> ValidCollection = new List<bool>();
            collection[0] = Channel1checkBox.Checked;
            collection[1] = Channel2checkBox.Checked;
            collection[2] = Channel3checkBox.Checked;
            collection[3] = Channel4checkBox.Checked;
            collection[4] = Channel5checkBox.Checked;
            collection[5] = Channel6checkBox.Checked;
            collection[6] = Channel7checkBox.Checked;
            collection[7] = Channel8checkBox.Checked;
            collection[8] = Channel9checkBox.Checked;
            collection[9] = Channel10checkBox.Checked;
            collection[10] = Channel11checkBox.Checked;
            collection[11] = Channel12checkBox.Checked;
            BitArray bitArray = new BitArray(0);
            UInt16 channelValue = 0;
            for (int count = 0; count < collection.Count(); count++)
            {
                if (collection[count] == true)
                {
                    ValidCollection.Add(collection[count]);
                }
            
            }
            int channelselectionCount = getTheNumberOfChannelsSelected();
            if (Continousmoderadiobutton.Checked == false && lookAhead.Checked == false && channelselectionCount > 2 && ValidCollection.Count % 2 != 0)
            {
                return channelValue;
                
            }

            else
            {
                bitArray = new BitArray(collection);
                channelValue = ConvertToByte(bitArray);
                return channelValue;
            }
            


        }

        private void Channel1checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Channel1checkBox.Checked)
            {
               // ArrayList points = ApplicationUtil.ApplicationUtil.ReadconfigadcCsvfile("D:\\Projects\\27-Feb-2013_19_10_32_181.csv");
                //CacheStore.getInstance().comblist = points;
               // DrawGraph(points);
                //DoGraphActivity();
            }
        }


        private CheckBox getTargetChannelCheckBox(int channel)
        {

            switch (channel)
            {
                case 1: return Channel1checkBox;
                case 2: return Channel2checkBox;
                case 3: return Channel3checkBox;
                case 4: return Channel4checkBox;
                case 5: return Channel5checkBox;
                case 6: return Channel6checkBox;
                case 7: return Channel7checkBox;
                case 8: return Channel8checkBox;
                case 9: return Channel9checkBox;
                case 10: return Channel10checkBox;
                case 11: return Channel11checkBox;
                case 12: return Channel12checkBox;
            }
            return null;
        
        }

        private int getTheNumberOfChannelsSelected()
        {
            int channelCount = 0;
            if (Channel1checkBox.Checked)
            {
                channelCount++;
            }
           
            if (Channel2checkBox.Checked)
            {
                channelCount++;
            }
            if (Channel3checkBox.Checked)
            {
                channelCount++;
            }
            if (Channel4checkBox.Checked)
            {
                channelCount++;
            }
            if (Channel5checkBox.Checked)
            {
                channelCount++;
            }
            if (Channel6checkBox.Checked)
            {
                channelCount++;
            }
            if (Channel7checkBox.Checked)
            {
                channelCount++;
            }
            if (Channel8checkBox.Checked)
            {
                channelCount++;
            }
            if (Channel9checkBox.Checked)
            {
                channelCount++;
            }
            if (Channel10checkBox.Checked)
            {
                channelCount++;
            }
            if (Channel11checkBox.Checked)
            {
                channelCount++;
            }
            if (Channel12checkBox.Checked)
            {
                channelCount++;
            }
            return channelCount;
        
        }


        private void previousTest()
        {
            //Dictionary<UInt16, List<string>> dict = ApplicationUtil.ApplicationUtil.fileStore.channelFileMap;
            //int index = 0;
            //List<string> files = new List<string>();
            //string Containingdirectory = null;
           
            //    foreach (var item in dict)
            //    {
            //        UInt16 channelVal = item.Key;
            //        List<int> channels = new List<int>();
            //        ApplicationUtil.ApplicationUtil.GetValueFromNumber(channelVal,out channels);
            //        files = item.Value;
            //        int indexOfCurrent = files.FindIndex(v => v.Equals(sourceFile.ElementAt(count)));
            //        if (indexOfCurrent == -1)
            //        {
            //            continue;
            //        }
            //        index = indexOfCurrent - 1;
            //        if (index < 0)
            //        {
            //            btnPrevious.Enabled = false;
            //            btnNext.Enabled = true;
            //            if(channels.Count==1)
            //            MessageBox.Show(Settings1.Default.First_File+"in channel"+channels[0], "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //            else
            //                MessageBox.Show(Settings1.Default.First_File + "in channel" + channels[0]+"," +channels[1]  +"Please select another file with desired channels.", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        
            //            return;
            //        }
            //        break;
            //    }
            //    string filepath = files.ElementAt(index);
            //    sourceFile[count] = filepath;
            //    if (changedpath == null)
            //    {
            //        Containingdirectory = ApplicationUtil.ApplicationUtil.GetApplicationPath();
            //    }
            //    else
            //    {
            //        Containingdirectory = changedpath;
            //    }
            //    Containingdirectory = Path.Combine(Containingdirectory, filepath);
            //    ArrayList points = ApplicationUtil.ApplicationUtil.ReadconfigadcCsvfile(Containingdirectory);
            //    List<ADCVO> adclist = (List<ADCVO>)points[1];
            //    SetGraphDefaultValue();
            //    if (adclist.Count == 0)
            //    {
            //        MessageBox.Show("This file is empty or in a invalid format.Please select another file", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        return;
            //    }

            //    DrawGraph(points);
           
        }


        private void nextTest()
        {
            //Dictionary<UInt16, List<string>> dict = ApplicationUtil.ApplicationUtil.fileStore.channelFileMap;
            //int index = 0;
            //List<string> files = new List<string>();
            //string Containingdirectory = null;
            
            //    foreach (var item in dict)
            //    {
            //        UInt16 channelVal = item.Key;
            //        List<int> channels = new List<int>();
            //        ApplicationUtil.ApplicationUtil.GetValueFromNumber(channelVal, out channels);
            //        files = item.Value;


            //        //int indexOfCurrent = files.FindIndex(v => v.Equals(sourceFile.ElementAt(count)));
            //        if (indexOfCurrent == -1)
            //        {
            //            continue;
            //        }
            //        index = indexOfCurrent + 1;
            //        if (index >= files.Count)
            //        {
            //            if (channels.Count == 1)
            //                MessageBox.Show("This is the last file in collection in channel" + channels[0], "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //            else
            //                MessageBox.Show("This is the last file in collection in channels" + channels[0] + "," + channels[1]+".Please select another file with the desired channels and then press next.", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        
            //            btnNext.Enabled = false;
            //            btnPrevious.Enabled = true;
            //            return;

            //        }
                    
            //        break;

            //    }

            //    string filepath = files.ElementAt(index);
            //    sourceFile[count] = filepath;
            //    if (changedpath == null)
            //    {
            //        Containingdirectory = ApplicationUtil.ApplicationUtil.GetApplicationPath();
            //    }
            //    else
            //    {
            //        Containingdirectory = changedpath;
            //    }

            //    Containingdirectory = Path.Combine(Containingdirectory, filepath);
            //    ArrayList points = ApplicationUtil.ApplicationUtil.ReadconfigadcCsvfile(Containingdirectory);
            //    List<ADCVO> adclist = (List<ADCVO>)points[1];
            //    SetGraphDefaultValue();
            //    if (adclist.Count == 0)
            //    {
            //        MessageBox.Show("This file is empty or in a invalid format.Please select another file", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        return;
            //    }

            //    //MAITREYEE
            //    DrawGraph(points);

            
        }

        private void ClearAllGraphsbutton_Click(object sender, EventArgs e)
        {
            clearAllGraphs();          
                      
        }

        private void clearAllGraphs()
        {

            PlotterDisplayEx[] graphArr = new PlotterDisplayEx[] { plotterDisplayEx1, plotterDisplayEx2, plotterDisplayEx3, plotterDisplayEx4, plotterDisplayEx5, plotterDisplayEx6,
            plotterDisplayEx7,plotterDisplayEx8,plotterDisplayEx9,plotterDisplayEx10,plotterDisplayEx11,plotterDisplayEx12};
            foreach (PlotterDisplayEx ctrGraph in graphArr)
            {
                DataSource dataSource= new DataSource();
                ctrGraph.DataSources.Clear();
                ctrGraph.DataSources.Add(dataSource);
                ctrGraph.gPane.minY = 0;
                ctrGraph.gPane.maxY = 0;
                if(!this.InvokeRequired)
                    ctrGraph.Refresh();
            }
            ZoomMultiplier = 0;
        }

        private void ZoomInbutton_Click(object sender, EventArgs e)
        {
            List<PlotterDisplayEx> controls = getTheActiveGraphControls();
            increaseYgridDistance(controls);
        }

        private bool DrawGraphWithZoom(List<PlotterDisplayEx> graphControls, int ZoomMultiplier)
        {
            bool isZoomed = false;
            foreach (var control in graphControls)
            {
                double currentmaxY;
                double currentminY;
                //bool currentGraphZoomed = false;
                //double currentmaxY = control.DataSources[0].YMax;
                //double currentminY = control.DataSources[0].YMin;
                //if (currentmaxY == currentminY)
                //{
                //    currentminY = 0;
                //}
                //double dif = currentmaxY - currentminY;
                //currentminY = currentminY - (Math.Abs(dif) * (0.10 * ZoomMultiplier));
                //currentmaxY = currentmaxY + (Math.Abs(dif) * (0.10 * ZoomMultiplier));
                //if (currentmaxY > currentminY)
                //{
                //    currentGraphZoomed = true;
                //    isZoomed = true;
                //}
                //if (!currentGraphZoomed)
                //{
                //    currentmaxY = control.DataSources[0].YMax;
                //    currentminY = control.DataSources[0].YMin;
                //    if (control.DataSources[0].YMax == control.DataSources[0].YMin)
                //        currentminY = 0;
                //    currentminY = currentminY - (Math.Abs(dif) * (0.10 * -4));
                //    currentmaxY = currentmaxY + (Math.Abs(dif) * (0.10 * -4));
                //    currentminY += (5 * (-4 - ZoomMultiplier));
                //    currentmaxY -= (5 * (-4 - ZoomMultiplier));
                //}
                //if (currentmaxY > currentminY)
                //{
                //    currentGraphZoomed = true;
                //    isZoomed = true;
                //}
                //if (currentGraphZoomed)
                //{
                //    control.gPane.minY = currentminY;
                //    control.gPane.maxY = currentmaxY;
                //    control.gPane.grid_distance_y = 5;
                //    if (!this.InvokeRequired)
                //        control.Refresh();
                //}

                try
                {
                    currentmaxY = Convert.ToInt32(textYRange.Text);
                    currentminY = -1 * currentmaxY;
                }
                catch
                {
                    currentmaxY = 10000;
                    currentminY = -10000;
                }

                control.gPane.minY = currentminY;
                control.gPane.maxY = currentmaxY;
                control.gPane.grid_distance_y = 5;
                if (!this.InvokeRequired)
                    control.Refresh();

            }
            
            /*Old code*/
            //foreach (var control in graphControls)
            //{
            //    double currentmaxY = control.DataSources[0].YD1;
            //    double currentminY = control.DataSources[0].YD0;

            //    if (currentmaxY == currentminY)
            //    {
            //        currentminY = 0;
            //    }
            //    currentminY = currentminY - (Math.Abs(control.DataSources[0].YD0) * (0.10 * ZoomMultiplier));
            //    currentmaxY = currentmaxY + (Math.Abs(control.DataSources[0].YD1) * (0.10 * ZoomMultiplier));
            //    if (currentmaxY > currentminY)
            //    {
            //        control.gPane.minY = currentminY;
            //        control.gPane.maxY = currentmaxY;
            //        control.gPane.grid_distance_y = 5;
            //        control.Refresh();
            //        isZoomed = true;
            //    }
            //}

            return isZoomed;
        }

        private void increaseYgridDistance(List<PlotterDisplayEx> graphControls)
        {
            ZoomMultiplier--;
            bool isZoomed = DrawGraphWithZoom(graphControls, ZoomMultiplier);
            
            if (!isZoomed)
                ZoomMultiplier++;
        }

        private void decreaseYgridDistance(List<PlotterDisplayEx> graphControls)
        {
            ZoomMultiplier++;
            bool isZoomed = DrawGraphWithZoom(graphControls, ZoomMultiplier);
            if (!isZoomed)
                ZoomMultiplier--;
        }

        private void ZoomOutbutton_Click(object sender, EventArgs e)
        {
            List<PlotterDisplayEx> controls = getTheActiveGraphControls();
            decreaseYgridDistance(controls);
               
        }

        private List<PlotterDisplayEx> getTheActiveGraphControls()
        {
            List<PlotterDisplayEx> activeGraphs = new List<PlotterDisplayEx>();
            if (plotterDisplayEx1.DataSources[0].Samples!=null)
            {

                activeGraphs.Add(plotterDisplayEx1);
            }
            if (plotterDisplayEx2.DataSources[0].Samples!= null)
            {

                activeGraphs.Add(plotterDisplayEx2);
            }
            if (plotterDisplayEx3.DataSources[0].Samples != null)
            {

                activeGraphs.Add(plotterDisplayEx3);
            }
            if (plotterDisplayEx4.DataSources[0].Samples != null)
            {

                activeGraphs.Add(plotterDisplayEx4);
            }
            if (plotterDisplayEx5.DataSources[0].Samples != null)
            {

                activeGraphs.Add(plotterDisplayEx5);
            }
            if (plotterDisplayEx6.DataSources[0].Samples != null)
            {

                activeGraphs.Add(plotterDisplayEx6);
            }
            if (plotterDisplayEx7.DataSources[0].Samples != null)
            {

                activeGraphs.Add(plotterDisplayEx7);
            }
            if (plotterDisplayEx12.DataSources[0].Samples != null)
            {

                activeGraphs.Add(plotterDisplayEx12);
            }
            if (plotterDisplayEx8.DataSources[0].Samples != null)
            {

                activeGraphs.Add(plotterDisplayEx8);
            }
            if (plotterDisplayEx9.DataSources[0].Samples != null)
            {

                activeGraphs.Add(plotterDisplayEx9);
            }

            if (plotterDisplayEx10.DataSources[0].Samples != null)
            {

                activeGraphs.Add(plotterDisplayEx10);
            }
            if (plotterDisplayEx11.DataSources[0].Samples != null)
            {

                activeGraphs.Add(plotterDisplayEx11);
            }
            return activeGraphs;
        }

        private void timerStatusMsg_Tick(object sender, EventArgs e)
        {
            try
            {
                if ((DateTime.Now - m_RequestTime).TotalSeconds > 15)
                {
                    lblConfigStatus.Visible = false;
                }
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
            }
        }

    





    }
}
