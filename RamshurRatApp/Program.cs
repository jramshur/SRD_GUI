using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using VOManager;
using System.Threading;
using System.Diagnostics;
using CustomLogger;
using System.Reflection;



namespace RamshurRatApp
{
    static class Program
    {
        private static ILogger m_logger = MyLogFactory.GetInstance(LoggerType.Log4Net).GetLogger(System.Reflection.Assembly.GetExecutingAssembly(), typeof(Program));

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        public static FrmMain MainScreen = null;
        public static VOManager.DASCommunicator communicator = null;
        public static int Mode_Operation = EnumAndConstants.TRIGGER;
        public static bool IsOnlineMode = true;
        //public static ConfigurationVO ConfigVo = new ConfigurationVO();
        //public static bool confirmation;
        
        /// <summary>
        /// Start Point Method
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                m_logger.Info("VERSION", Assembly.GetExecutingAssembly().GetName().Version);
                m_logger.Info("OS", Environment.OSVersion);
                m_logger.Info("Path",Application.StartupPath);
                bool createdNew = true;
                //Global Mutex for SIngle instance application
                using (Mutex mutex = new Mutex(true, "PSoC Data Logger", out createdNew))
                {
                    //Check if mutex obtained
                    if (createdNew)
                    {
                        Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
                        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        communicator = new VOManager.DASCommunicator();
                        MainScreen = new FrmMain();//Start Application from Main form
                        Application.Run(MainScreen);
                    }
                    else//Display message that the appliocation is running
                    {
                        Process current = Process.GetCurrentProcess();
                        foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                        {
                            if (process.Id != current.Id)
                            {
                                MessageBox.Show("The application is already open", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                m_logger.Error(ex);
            }
            try
            {
                MainScreen.Stop();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Unhandled exception event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            m_logger.Error(e.ExceptionObject);
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Unhandled Thread event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            m_logger.Error(e.Exception);
            Thread.Sleep(1000);
        }
    }
}
