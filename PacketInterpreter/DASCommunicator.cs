using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomLogger;
using System.IO.Ports;
using VOManager;
using System.Threading;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Diagnostics;

namespace VOManager
{
    public delegate void SetDACStatusDelegate(bool HHUStatus, bool isConfiguration);
    public delegate void filecreation(string filename);
    public delegate void DirectoryCreationError(string directoryPath);



    public class DASCommunicator
    {
        public SerialPort serialPort;
        public static string path = string.Empty;
        public static bool CreateNewFile = false;
        private static ILogger m_logger = MyLogFactory.GetInstance(LoggerType.Log4Net).GetLogger(System.Reflection.Assembly.GetExecutingAssembly(), typeof(DASCommunicator));
        public ReadState stateVar;
        public Packet.Packet requestPacket;
        private static int DSN = -1;
        private static int ReqId = -1;
        private Packet.Packet responsePacket;
        private object synObj = new object();
        private AutoResetEvent waitHandle = new AutoResetEvent(false);
        public static event SetDACStatusDelegate SetStatusEvent;
        private static int NUMBER_OF_RECORDS = 0;
        private static int sampling_rate = 0;
        public static bool confirmation = false;
        private static int adc_resolution = 1;
        private static UInt16 ChannelValue = 0;
        private static int ChannelA = 0;
        private static int ChannelB = 0;
        public DateTime AdcCurrentDate = DateTime.Now;
        public static event filecreation filecreateEvent;
        private ArrayList adclist = new ArrayList();
        private static List<ADCVO> adclistchannel2 = new List<ADCVO>();
        private string fileName = String.Empty;
        private string directory = String.Empty;
        //Flag to do not perform any operation while handshake, set and get configuration.
        public bool isNewRequestSend = false;
        public bool isBothClicked = false;
        public bool isContineousStartClick = false;
        public bool isStopClick = false;
        private bool neglectValue = false;
        private byte lastRemoveByte;
        public bool isDataReceived = false;
        public static RequestQ.QueueThread<object> FileLoggingQueue;
        public static event  DirectoryCreationError DirectoryCreationErrorEvent;
        //
        int i = 0;

        public DASCommunicator()
        { }

        /// <summary>
        /// serialPort checker
        /// </summary>
        /// <returns></returns>

        public bool ISOpen()
        {
            return (serialPort != null && serialPort.IsOpen);
        }
        /// <summary>
        /// initialise the port
        /// </summary>
        /// <param name="comportvo"></param>
        /// <returns></returns>
        public bool Init(ComPortVO comportvo)
        {
            try
            {
                CloseSerialPort();
                serialPort = new SerialPort(comportvo.portName, comportvo.baudRate, comportvo.parity, comportvo.dataBits, comportvo.stopBits);
                serialPort.DtrEnable = true;
                serialPort.RtsEnable = true;
                serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);

                stateVar = new ReadState();
                stateVar.packetState = PacketState.START_OF_PACKET;
                stateVar.currentPos = 0;
                serialPort.WriteTimeout = 2000;
                ForceSetBaudRate(comportvo.portName, comportvo.baudRate);
                serialPort.Open();
                return true;
            }
            catch (Exception ex)
            {
                m_logger.Error(comportvo.portName, ex);
                return false;
            }
        }

        public void CloseSerialPort()
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen == true)
                {
                    serialPort.DiscardOutBuffer();
                    serialPort.DiscardInBuffer();
                    serialPort.Close();
                    //serialPort.Dispose();
                }
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
            }
        }

        public void ClearLoggingQueue()
        {
            try
            {
                FileLoggingQueue.Clear();
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
            }
        }
        private void ForceSetBaudRate(string portName, int baudRate)
        {
            if (Type.GetType("Mono.Runtime") == null) return; //It is not mono === not linux! 
            string arg = String.Format("-F {0} speed {1}", portName, baudRate);
            var proc = new Process
            {
                EnableRaisingEvents = false,
                StartInfo = { FileName = @"stty", Arguments = arg }
            };
            proc.Start();
            proc.WaitForExit();
        }

        /// <summary>
        /// De-initialiser serial port
        /// </summary>
        /// <returns></returns>
        public bool DeInit()
        {
            try
            {
                CloseSerialPort();
                return true;
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// Serial port receiver listener
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                //lock (serialPort)
                {
                    ReadPacket(serialPort.BytesToRead, ((SerialPort)sender));
                }
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
            }
        }

        /// <summary>
        /// Received packed details
        /// </summary>
        /// <param name="bytesToRead"></param>
        /// <param name="serialPort"></param>
        public void ReadPacket(int bytesToRead, SerialPort serialPort)
        {
            try
            {
                if (serialPort.IsOpen == false)
                    return;

                while (serialPort.BytesToRead > 0)
                {
                    ///For continuous ADC count packet
                    if (stateVar.packetState == PacketState.CONTINUOUS_ADC_COUNT)
                    {
                        //if (adc_resolution == (int)EnumAndConstants.BIT_CHECKER)
                        //{
                        //     NUMBER_OF_RECORDS = serialPort.BytesToRead;
                        //     stateVar.ReadBuffer = new byte[NUMBER_OF_RECORDS];
                        //// }
                        //else
                        //{
                        int bytesAvailable = serialPort.BytesToRead;
                        if (bytesAvailable % 2 != 0)
                            NUMBER_OF_RECORDS = bytesAvailable - 1;
                        else
                            NUMBER_OF_RECORDS = bytesAvailable;
                        stateVar.ReadBuffer = new byte[NUMBER_OF_RECORDS];
                        stateVar.currentPos = 0;

                        NUMBER_OF_RECORDS /= 2;
                        if (NUMBER_OF_RECORDS <= 0)
                            return;
                        //}
                    }
                    int bytesread=0;
                    lock (serialPort)
                    {
                        bytesread = serialPort.Read(stateVar.ReadBuffer, stateVar.currentPos, stateVar.ReadBuffer.Length - stateVar.currentPos);
                    }
                    stateVar.currentPos += bytesread;

                    if (stateVar.currentPos < stateVar.ReadBuffer.Length)
                        continue;

                    stateVar.currentPos = 0;

                    switch (stateVar.packetState)
                    {
                        case PacketState.START_OF_PACKET:
                            {
                                if (stateVar.ReadBuffer[0] == EnumAndConstants.START_FRAME_2 && stateVar.LastSopRead == EnumAndConstants.START_FRAME_1)
                                {
                                    stateVar.packetState = PacketState.HEADER;
                                }
                                stateVar.LastSopRead = stateVar.ReadBuffer[0];
                                break;
                            }
                        case PacketState.HEADER:
                            {
                                stateVar.packetState = PacketState.PAYLOAD;
                                break;
                            }
                        case PacketState.PAYLOAD:
                            {
                                responsePacket = new Packet.Packet(stateVar.Header.Length + stateVar.ReadBuffer.Length + EnumAndConstants.CONSTANT_VALUE);
                                responsePacket.Append((byte)EnumAndConstants.START_FRAME_1);
                                responsePacket.Append((byte)EnumAndConstants.START_FRAME_2);
                                responsePacket.Append(stateVar.Header);
                                responsePacket.Append(stateVar.ReadBuffer);

                                stateVar.packetState = PacketState.START_OF_PACKET;
                                responsePacket.DataLength = responsePacket.CurrentPos;
                                responsePacket.CurrentPos = 0;

                                string payloadstr = "";
                                for (int ctr = 0; ctr < responsePacket.DataLength; ctr++)
                                {
                                    payloadstr += " " + responsePacket.DataArray[ctr].ToString("X2");
                                }
                                m_logger.Debug("Received Payload", payloadstr);

                                if (responsePacket.DataArray[0] == EnumAndConstants.START_FRAME_1 && responsePacket.DataArray[1] == EnumAndConstants.START_FRAME_2)
                                {
                                    if (calculateCheckSum(responsePacket, true) != responsePacket.DataArray[responsePacket.DataLength - 1])
                                    {
                                        m_logger.Debug("Received packet is invalid, CheckSum not matched.");
                                        return;
                                    }
                                }
                                ParsingData(responsePacket);
                                break;
                            }
                        case PacketState.ADC_COUNT:
                            {
                                
                                ResponsePacketCreation();
                                break;
                            }
                        case PacketState.CONTINUOUS_ADC_COUNT:
                            {
                                if (!isStopClick)
                                    ResponsePacketCreation();
                                break;
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
        /// Response packet creation
        /// </summary>
        private void ResponsePacketCreation()
        {
            try
            {
                responsePacket = new Packet.Packet(stateVar.ReadBuffer.Length);
                responsePacket.Append(stateVar.ReadBuffer);

                responsePacket.DataLength = responsePacket.CurrentPos;
                responsePacket.CurrentPos = 0;
                string payloadstr = "";
                for (int ctr = 0; ctr < responsePacket.DataLength; ctr++)
                {
                    payloadstr += " " + responsePacket.DataArray[ctr].ToString("X2");
                }
                m_logger.Debug("Received Payload", payloadstr);

                if (responsePacket.DataArray[0] == EnumAndConstants.START_FRAME_1 && responsePacket.DataArray[1] == EnumAndConstants.START_FRAME_2)
                {
                    ParsingData(responsePacket);
                }
                else
                {
                    GetADCDetails(responsePacket);
                }
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
            }
        }

        /// <summary>
        /// Parsing of Data packet
        /// </summary>
        /// <param name="receivedPacket"></param>
        public void ParsingData(Packet.Packet receivedPacket)
        {
            int responceId = -1;
            try
            {
                receivedPacket.CurrentPos = 0;
                receivedPacket.RemoveBytes(EnumAndConstants.CONSTANT_VALUE);//SOF
                receivedPacket.RemoveByte();//Protocol version
                receivedPacket.RemoveByte();//Unique number
                responceId = receivedPacket.RemoveByte();//Request ID : 6
                int PacketLength = receivedPacket.RemoveByte(); //Packet length
                if ((responceId == (int)ADCStatus.START_ADC || responceId == (int)ADCStatus.END_ADC) && isNewRequestSend == true)
                {
                    return;
                }
                if (responceId == (int)ADCStatus.START_ADC || responceId == (int)ADCStatus.END_ADC)
                {
                    GetTriggerModeRequest(receivedPacket, responceId);
                    return;
                }
                isNewRequestSend = false;
                if (PacketLength > 0)
                {
                    GetConfigurationDetails(receivedPacket);
                }
                receivedPacket.RemoveByte(); //CRC
                waitHandle.Set();

            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
            }
        }

        /// <summary>
        /// Get trigger mode values
        /// </summary>
        /// <param name="receivedPacket"></param>
        /// <param name="responceId"></param>
        private void GetTriggerModeRequest(Packet.Packet receivedPacket, int responceId)
        {
            if (responceId == (int)ADCStatus.START_ADC)
            {
                //TODO : Make 2 bytes
                ChannelValue = receivedPacket.RemoveUInt16();//CHANNEL ID
                //if (ApplicationUtil.ApplicationUtil.FirstRecievedChannelValue == 0)
                //{
                //    ApplicationUtil.ApplicationUtil.FirstRecievedChannelValue = ChannelValue;
                //}
                //else
                //{
                //    if (ChannelValue == ApplicationUtil.ApplicationUtil.FirstRecievedChannelValue)
                //    {
                //        CreateNewFile = true;

                //    }
                //}
                
                List<int> channels = new List<int>();
                ApplicationUtil.ApplicationUtil.GetValueFromNumber(ChannelValue, out channels);
                if (channels.Count == 1 && channels.Count!=0)
                {
                    //adc_resolution = 1;
                    ChannelA = channels[0];
                    ChannelB = -1;
                    isBothClicked = false;
                }
                else if (channels.Count > 1 && channels.Count != 0)
                {
                    //adc_resolution = 0;
                    ChannelA = channels[0];
                    ChannelB = channels[1];
                    isBothClicked = true;
                
                }
                //As per new Doc
                //adc_resolution = receivedPacket.RemoveByte();//adc resolution
                //TODO : 2 byte
                sampling_rate = receivedPacket.RemoveByte();//sampling rate
                receivedPacket.UseLittleEndian = true;
                NUMBER_OF_RECORDS = receivedPacket.RemoveUInt16();//Number of records
                //if (adc_resolution != EnumAndConstants.BIT_CHECKER)
                //{
                //    NUMBER_OF_RECORDS /= 2;
                //}
                receivedPacket.UseLittleEndian = false;

                stateVar = new ReadState();
                stateVar.currentPos = 0;
                stateVar.packetState = PacketState.ADC_COUNT;
                //if (adc_resolution == EnumAndConstants.BIT_CHECKER)
                //{
                stateVar.ReadBuffer = new byte[NUMBER_OF_RECORDS * EnumAndConstants.CONSTANT_VALUE];
                //}
                //else
                //{
                //    stateVar.ReadBuffer = new byte[NUMBER_OF_RECORDS * EnumAndConstants.CONSTANT_VALUE]; 
                //}
            }
            receivedPacket.RemoveByte(); //CRC
            waitHandle.Set();
        }

        /// <summary>
        /// Dummy ADC value creation
        /// </summary>
        public void DummyADC()
        {

            Packet.Packet rPacket = new Packet.Packet(1024);
            NUMBER_OF_RECORDS = 1000;
            adc_resolution = EnumAndConstants.BIT_CHECKER;
            int cnt = 0;
            for (int i = 0; i < NUMBER_OF_RECORDS; i++)
            {
                rPacket.Append((byte)cnt++);
                if (cnt == 200)
                    cnt = 0;
            }
            AdcCurrentDate = DateTime.Now;
            GetADCDetails(rPacket);
        }

        /// <summary>
        /// Get ADC Details from response packet
        /// </summary>
        /// <param name="receivedPacket"></param>
        private void GetADCDetails(Packet.Packet receivedPacket)
        {
            try
            {
                adclist = new ArrayList();
                receivedPacket.CurrentPos = 0;

                List<ConfigurationVO> configlist = ApplicationUtil.ApplicationUtil.ReadconfigCsvfile();
                List<ADCVO> firstChannel = new List<ADCVO>();
                List<ADCVO> secondChannel = new List<ADCVO>();
                List<ADCVO> singleChannel = new List<ADCVO>();
                receivedPacket.UseLittleEndian = true;
                //
                if ( (adc_resolution == 2) || (adc_resolution == 3) && isContineousStartClick)
                {
                    if (neglectValue)
                    {
                        Array.Resize(ref receivedPacket.DataArray, receivedPacket.DataArray.Length + 1);
                        Array.Copy(receivedPacket.DataArray, 0, receivedPacket.DataArray, 1, receivedPacket.DataArray.Length - 1);
                        receivedPacket.CurrentPos = 0;
                        receivedPacket.Append(lastRemoveByte);
                        if (IsEvenValue(receivedPacket.DataArray.Length))
                        {
                            m_logger.Debug("E");
                            NUMBER_OF_RECORDS += 1;
                        }
                        neglectValue = false;
                    }

                    if (!IsEvenValue(receivedPacket.DataArray.Length))
                    {
                        m_logger.Debug("O");
                        neglectValue = true;
                        receivedPacket.CurrentPos = receivedPacket.DataArray.Length - 1;
                        lastRemoveByte = receivedPacket.RemoveByte();
                    }
                    receivedPacket.UseLittleEndian = false;
                    receivedPacket.CurrentPos = 0;
                    m_logger.Debug("N : " + NUMBER_OF_RECORDS + " L : " + responsePacket.DataArray.Length);
                }

                for (int i = 0; i < NUMBER_OF_RECORDS; i++)
                {
                    if (receivedPacket.CurrentPos + 2 < receivedPacket.DataLength)
                    {
                        ADCVO adc = new ADCVO();
                        //receivedPacket.UseLittleEndian = true;
                        //if (adc_resolution == EnumAndConstants.BIT_CHECKER)
                        receivedPacket.UseLittleEndian = false;
                        adc.AdcValue = receivedPacket.RemoveInt16();
                        //else if (adc_resolution == 2)
                        //{
                        //    adc.AdcValue = receivedPacket.RemoveUInt16();
                        //    adc.AdcValue &= 0x3ff;
                        //}
                        //else if (adc_resolution == 3)
                        //{
                        //    adc.AdcValue = (receivedPacket.RemoveUInt16() & 0xfff);
                        //    adc.AdcValue &= 0xfff;
                        //}
                        //receivedPacket.UseLittleEndian = false;
                        //m_logger.Debug("AV : " + adc.AdcValue);
                        if (sampling_rate == 0)
                        {
                            ApplicationUtil.ApplicationUtil.SAMPLING_RATE_DATE_ADD_VALUE = 100;
                            AdcCurrentDate = AdcCurrentDate.AddTicks(1000000);
                        }
                        if (sampling_rate == EnumAndConstants.BIT_CHECKER)
                        {
                            ApplicationUtil.ApplicationUtil.SAMPLING_RATE_DATE_ADD_VALUE = 0.5;
                            AdcCurrentDate = AdcCurrentDate.AddTicks(5000);
                        }
                        else if (sampling_rate == 2)
                        {
                            ApplicationUtil.ApplicationUtil.SAMPLING_RATE_DATE_ADD_VALUE = 0.3333;
                            AdcCurrentDate = AdcCurrentDate.AddTicks(3333);
                        }
                        else if (sampling_rate == 3)
                        {
                            ApplicationUtil.ApplicationUtil.SAMPLING_RATE_DATE_ADD_VALUE = 0.2500;
                            AdcCurrentDate = AdcCurrentDate.AddTicks(2500);
                        }
                        else if (sampling_rate == 4)
                        {
                            ApplicationUtil.ApplicationUtil.SAMPLING_RATE_DATE_ADD_VALUE = 0.2000;
                            AdcCurrentDate = AdcCurrentDate.AddTicks(2000);
                        }
                        else if (sampling_rate == 5)
                        {
                            ApplicationUtil.ApplicationUtil.SAMPLING_RATE_DATE_ADD_VALUE = 0.1666;
                            AdcCurrentDate = AdcCurrentDate.AddTicks(1666);
                        }
                        else if (sampling_rate == 6)
                        {
                            ApplicationUtil.ApplicationUtil.SAMPLING_RATE_DATE_ADD_VALUE = 0.1000;
                            AdcCurrentDate = AdcCurrentDate.AddTicks(1000);
                        }
                        else if (sampling_rate == 7)
                        {
                            ApplicationUtil.ApplicationUtil.SAMPLING_RATE_DATE_ADD_VALUE = 0.0833;
                            AdcCurrentDate = AdcCurrentDate.AddTicks(833);
                        }
                        else if (sampling_rate == 8)
                        {
                            ApplicationUtil.ApplicationUtil.SAMPLING_RATE_DATE_ADD_VALUE = 0.0666;
                            AdcCurrentDate = AdcCurrentDate.AddTicks(666);
                        }
                        adc.AdcDate = AdcCurrentDate;
                        if (i == 0 && adc.AdcValue == 0)
                            continue;

                        if (isBothClicked)
                        {
                            if (IsEvenValue(i))
                            {
                                adc.Channel = ChannelA;
                                firstChannel.Add(adc);
                                m_logger.Debug(ChannelA, adc.AdcDate, adc.AdcValue);
                            }
                            else
                            {
                                adc.Channel = ChannelB;
                                secondChannel.Add(adc);
                                m_logger.Debug(ChannelB, adc.AdcDate, adc.AdcValue);
                            }
                        }
                        else
                        {
                            adc.Channel = ChannelA;
                            singleChannel.Add(adc);
                            m_logger.Debug(ChannelA, adc.AdcDate, adc.AdcValue);
                        }
                    }
                }

                // m_logger.Debug("AdcCurrentDate : " + AdcCurrentDate.Ticks);
                if (singleChannel.Count == 0)
                {

                    adclist.Add(firstChannel);
                    adclist.Add(secondChannel);
                    //m_logger.Debug(ChannelA, firstChannel,secondChannel);
                }
                else
                {
                    adclist.Add(singleChannel);
                }
                if (isContineousStartClick)
                {
                    ApplicationUtil.ApplicationUtil.adcValueList = singleChannel;
                }
                else
                {
                    ApplicationUtil.ApplicationUtil.adcValueList = null;
                }

                if (stateVar.packetState != PacketState.CONTINUOUS_ADC_COUNT)
                {
                    stateVar = new ReadState();
                    stateVar.packetState = PacketState.START_OF_PACKET;
                    stateVar.currentPos = 0;
                }
                
                int operationMode = configlist[0].mode_operation;
                
                

                //if (Channel > 2)
                  //  Channel = 1;

                ArrayList comblist = new ArrayList();
                ArrayList comblist_Other = new ArrayList();

                comblist.Add(configlist);
                comblist.Add(adclist);//create arraylist containing configlist and adclist

                if (adclist == null || adclist.Count == 0)
                    return;
                fileName = ApplicationUtil.ApplicationUtil.GetFileNameWithDateTime(EnumAndConstants.CSV_FORMAT);//get name of file
                if (ApplicationUtil.ApplicationUtil.CurrentFolderPath == "" || ApplicationUtil.ApplicationUtil.CurrentFolderPath == null)
                {
                    ApplicationUtil.ApplicationUtil.CurrentFolderPath = Path.Combine(ApplicationUtil.ApplicationUtil.GetApplicationPath(), ApplicationUtil.ApplicationUtil.GetFileNameWithDateTime(""));
                }
                directory = Path.Combine(ApplicationUtil.ApplicationUtil.CurrentFolderPath, fileName);
                if (stateVar.packetState != PacketState.CONTINUOUS_ADC_COUNT)
                {
                    //directory = System.Configuration.ConfigurationManager.AppSettings["DirectoryPath"];
                    try
                    {
                        if (!Directory.Exists(ApplicationUtil.ApplicationUtil.CurrentFolderPath) && ApplicationUtil.ApplicationUtil.IsDirectoryCreateAllowed == true)
                        {
                            
                            if (!Directory.Exists(ApplicationUtil.ApplicationUtil.CurrentFolderPath))
                            {
                                Directory.CreateDirectory(ApplicationUtil.ApplicationUtil.CurrentFolderPath+"1");
                                Directory.Delete(ApplicationUtil.ApplicationUtil.CurrentFolderPath+"1");
                            }
                        }
                        // throw new IOException();
                    }
                    catch (IOException e)
                    {
                        ApplicationUtil.ApplicationUtil.IsDirectoryCreateAllowed = false;
                        if (DirectoryCreationErrorEvent != null)
                            DirectoryCreationErrorEvent(ApplicationUtil.ApplicationUtil.CurrentFolderPath);
                    }
                }
                if (operationMode == 1)
                    confirmation = true;
                FileObjectVO file = new FileObjectVO();
                //CacheStore.getInstance().comblist = null;
                //CacheStore.getInstance().ChannelA = ChannelA;
                //CacheStore.getInstance().ChannelB = ChannelB;
                //CacheStore.getInstance().directory = directory;
                //CacheStore.getInstance().comblist = comblist;
                file.Data = comblist;
                file.channel1 = ChannelA;
                file.channel2 = ChannelB;
                file.filePath = directory;
                FileLoggingQueue.EnqueObject(file);

                isDataReceived = true;
                //ApplicationUtil.ApplicationUtil.addFileNameToList(configlist[0].ChannelValue, fileName);
                //ApplicationUtil.ApplicationUtil.CreateCsvArrayList(comblist, directory, ChannelA, ChannelB);//create arraylist containing configlist adclist and directories
                //if (CreateNewFile == true)
                //{
                    
                   // ApplicationUtil.ApplicationUtil.CreateCsvArrayListVersion2(comblist,directory, ChannelA, ChannelB, true);
                //}
                //else
                //{
                //    //ApplicationUtil.ApplicationUtil.CreateCsvArrayList(comblist, directory, ChannelA, ChannelB);//create arraylist containing configlist adclist and directories
                //    ApplicationUtil.ApplicationUtil.CreateCsvArrayListVersion2(comblist, directory, ChannelA, ChannelB, false);
                
                //}

                // read the csv for both channels selected mode
                //if (filecreateEvent != null)
                //  filecreateEvent(directory);
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
            }
        }

        /// <summary>
        /// Check if Even value`
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool IsEvenValue(int value)
        {
            bool result = value % 2 == 0 ? true : false;
            return result;
        }

        /// <summary>
        /// Get configuration details and create CSV
        /// </summary>
        /// <param name="receivedPacket"></param>
        private static void GetConfigurationDetails(Packet.Packet receivedPacket)
        {
            try
            {
                ConfigurationVO configVo = ConfigurationVO.GetInstance(); 
                //receivedPacket.RemoveByte();
                
                configVo.mode_operation = receivedPacket.RemoveByte(); //Mode of Operation
                configVo.NumberOfSelectedChannels = receivedPacket.RemoveByte();
                configVo.ChannelValue = receivedPacket.RemoveUInt16(); // Channel
                //configVo.adcresolution = receivedPacket.RemoveByte(); // ADC resolution
                //adc_resolution = configVo.adcresolution;
                configVo.sampaling_rate = receivedPacket.RemoveByte(); // Sampling rate
                sampling_rate = configVo.sampaling_rate;
                // MAITREYEE:two paramters added according to the new requirement.
                configVo.T5VDAC = receivedPacket.RemoveByte();
                configVo.T7VDAC = receivedPacket.RemoveByte();

                receivedPacket.UseLittleEndian = true;
                configVo.T1 = receivedPacket.RemoveUInt16();
                configVo.T2 = receivedPacket.RemoveUInt16();
                configVo.T3 = receivedPacket.RemoveUInt16();
                configVo.T4 = receivedPacket.RemoveUInt16();
                configVo.T5 = receivedPacket.RemoveUInt16();
                configVo.T6 = receivedPacket.RemoveUInt16();
                configVo.T7 = receivedPacket.RemoveUInt16();
                configVo.T8 = receivedPacket.RemoveUInt16();
                configVo.T9 = receivedPacket.RemoveUInt16();
                configVo.T10 = receivedPacket.RemoveUInt16();
                receivedPacket.UseLittleEndian = false;
                List<ConfigurationVO> configlist = new List<ConfigurationVO>();
                configlist.Add(configVo);

                string applicationConfigurationPath = ApplicationUtil.ApplicationUtil.GetApplicationPath();
                applicationConfigurationPath += EnumAndConstants.CONFIGURATION_FILE;

                if (ApplicationUtil.ApplicationUtil.CheckFileExists(applicationConfigurationPath) == true)
                {
                    File.Delete(applicationConfigurationPath);
                }

                ApplicationUtil.ApplicationUtil.CreateCSVFromGenericList(configlist, applicationConfigurationPath);
                //if (null != SetStatusEvent)
                //    SetStatusEvent(true, true);
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
            }
        }

        /// <summary>
        /// Send packet on serial port
        /// </summary>
        /// <param name="port"></param>
        /// <param name="packet"></param>
        /// <returns></returns>
        public ResponseCode sendRequest(SerialPort port, ref Packet.Packet packet)
        {
            int retryCount = 0;
            int WaitInterval = EnumAndConstants.waitInterDataInterval;
            try
            {
                lock (synObj)
                {
                    port.DiscardInBuffer();
                    port.DiscardOutBuffer();

                    packet.DataLength = packet.CurrentPos;
                    string payloadstr = "";
                    for (int ctr = 0; ctr < packet.DataLength; ctr++)
                    {
                        payloadstr += " " + packet.DataArray[ctr].ToString("X2");
                    }
                    m_logger.Debug("Send Payload", payloadstr);

                    waitHandle.Reset();
                    lock (port)
                    {
                        port.Write(packet.DataArray, 0, packet.CurrentPos); //Write on Port
                        m_logger.Debug(DateTime.Now.ToLongTimeString());
                        //Thread.Sleep(EnumAndConstants.WAIT_NO_DATA_INTERVAL);
                    }

                    bool receivedResponse = false;
                    while (retryCount < EnumAndConstants.RETRY_COUNT)
                    {
                        if ((waitHandle.WaitOne(WaitInterval, false)) == true)
                        {
                            receivedResponse = true;
                            responsePacket.CurrentPos = 0;
                            responsePacket.RemoveBytes(2); //SOF
                            responsePacket.RemoveByte(); //Protocol version
                            responsePacket.RemoveByte(); // DSN

                            int responseId = responsePacket.RemoveByte(); //Responce code

                            if (responseId != (int)ResponseCode.SUCCESS)
                                return ResponseCode.INVALID;
                            else
                                return ResponseCode.SUCCESS;
                        }
                        retryCount++;
                    }
                    if (receivedResponse == false)
                        return ResponseCode.TIMEOUT;
                    else
                        return ResponseCode.FAILURE;
                }
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
                return ResponseCode.FAILURE;
            }
        }

        /// <summary>
        /// response packet
        /// </summary>
        /// <returns></returns>
        public ResponseCode sendResponceCode(int requestId)
        {
            try
            {
                //stateVar = new ReadState();
                //stateVar.packetState = PacketState.HEADER;
                //stateVar.currentPos = 0;

                Packet.Packet packet = new Packet.Packet(1048);
                packet.Append(EnumAndConstants.START_FRAME);
                packet.Append((byte)EnumAndConstants.PROTOCOL_VER);//VERSION 1
                DSN = new Random().Next();
                packet.Append((byte)DSN);
                packet.Append((byte)requestId);//RESPONCE 00
                packet.Append((byte)0x01);//LENGTH 0
                packet.Append((byte)ResponceID.NORMAL_RESPONSE);//responce normal
                packet.Append((byte)calculateCheckSum(packet, false));
                return sendRequest(serialPort, ref packet);

            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
                return ResponseCode.FAILURE;
            }
        }
        /// <summary>
        /// function for handshaking with wireless device
        /// </summary>
        /// <returns></returns>
        public ResponseCode handshakeWirelessDevice()
        {
            try
            {
                stateVar = new ReadState();
                stateVar.packetState = PacketState.START_OF_PACKET;
                stateVar.currentPos = 0;

                Packet.Packet packet = new Packet.Packet(1048);
                packet.Append(EnumAndConstants.START_FRAME);//DOF
                packet.Append((byte)EnumAndConstants.PROTOCOL_VER); //Version : 1
                DSN = new Random().Next(); //Random number
                packet.Append((byte)DSN);
                packet.Append((byte)RequestId.HANDSHAKE_WIRELESS_DEVICE); // Request : 0x01
                packet.Append((byte)EnumAndConstants.PROTOCOL_LENGTH); //Length : 0x00
                packet.Append((byte)calculateCheckSum(packet, false));

                return sendRequest(serialPort, ref packet);
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
                return ResponseCode.FAILURE;

            }
        }
        /// <summary>
        /// function for request setConfiguration
        /// </summary>
        /// <returns></returns>
        public ResponseCode setConfigurationRequest(ConfigurationVO configVo)
        {
            try
            {
                stateVar = new ReadState();
                stateVar.packetState = PacketState.START_OF_PACKET;
                stateVar.currentPos = 0;

                Packet.Packet packet = new Packet.Packet(1048);
                packet.Append(EnumAndConstants.START_FRAME);//SOF
                packet.Append((byte)EnumAndConstants.PROTOCOL_VER); //Version : 1
                DSN = new Random().Next();//Random number
                packet.Append((byte)DSN);
                packet.Append((byte)RequestId.SET_CONFIGIRATION); //Request : 2
                packet.Append((byte)28); // Length : 0
                packet.Append((byte)configVo.mode_operation);
                packet.Append((byte)configVo.NumberOfSelectedChannels);
                packet.Append((UInt16)configVo.ChannelValue);

                adc_resolution = configVo.adcresolution;
                //packet.Append((byte)configVo.adcresolution);

                sampling_rate = configVo.sampaling_rate;
                packet.Append((byte)configVo.sampaling_rate);
                packet.Append((byte)configVo.T5VDAC);
                packet.Append((byte)configVo.T7VDAC);
                packet.Append((UInt16)configVo.T1);
                packet.Append((UInt16)configVo.T2);
                packet.Append((UInt16)configVo.T3);
                packet.Append((UInt16)configVo.T4);
                packet.Append((UInt16)configVo.T5);
                packet.Append((UInt16)configVo.T6);
                packet.Append((UInt16)configVo.T7);
                packet.Append((UInt16)configVo.T8);
                packet.Append((UInt16)configVo.T9);
                packet.Append((UInt16)configVo.T10);
                packet.Append((byte)calculateCheckSum(packet, false));
                GraphFileCollectionSingleton.getInstance().channelFileMap = new Dictionary<ushort, List<string>>();
                //Append packetfields
                return sendRequest(serialPort, ref packet);
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
                return ResponseCode.FAILURE;
            }
        }
        /// <summary>
        /// function for start continous request
        /// </summary>
        /// <returns></returns>
        public ResponseCode startContinousRequest()
        {
            try
            {
                stateVar = new ReadState();
                stateVar.packetState = PacketState.START_OF_PACKET;
                stateVar.currentPos = 0;

                Packet.Packet packet = new Packet.Packet(1048);
                packet.Append(EnumAndConstants.START_FRAME);//SOF
                packet.Append((byte)EnumAndConstants.PROTOCOL_VER);//Version : 1
                DSN = new Random().Next();//Random number
                packet.Append((byte)DSN);
                packet.Append((byte)RequestId.START_CONTINOUS);//Request : 3
                packet.Append((byte)EnumAndConstants.PROTOCOL_LENGTH);//Length : 0
                packet.Append((byte)calculateCheckSum(packet, false));//CRC

                ResponseCode resp = sendRequest(serialPort, ref packet);
                if (resp == ResponseCode.SUCCESS)
                {
                    stateVar = new ReadState();
                    stateVar.packetState = PacketState.CONTINUOUS_ADC_COUNT;
                    stateVar.currentPos = 0;
                    fileName = ApplicationUtil.ApplicationUtil.GetFileNameWithDateTime(EnumAndConstants.CSV_FORMAT);//get name of file
                    directory = System.Configuration.ConfigurationManager.AppSettings["DirectoryPath"];
                    directory = Path.Combine(directory, fileName);
                }
                return resp;
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
                return ResponseCode.FAILURE;
            }
        }
        /// <summary>
        /// stop continous request
        /// </summary>
        /// <returns></returns>
        public ResponseCode stopContinousRequest()
        {
            try
            {
                Packet.Packet packet = new Packet.Packet(1048);
                packet.Append(EnumAndConstants.START_FRAME);//SOF
                packet.Append((byte)EnumAndConstants.PROTOCOL_VER);//Version : 1
                DSN = new Random().Next();//Random number
                packet.Append((byte)DSN);
                packet.Append((byte)RequestId.STOP_CONTINOUS); //Request : 4
                packet.Append((byte)EnumAndConstants.PROTOCOL_LENGTH);//Length : 0
                packet.Append((byte)calculateCheckSum(packet, false));//CRC
                stateVar = new ReadState();
                stateVar.packetState = PacketState.START_OF_PACKET;
                ResponseCode resp = sendRequest(serialPort, ref packet);
                //if (resp == ResponseCode.SUCCESS)
                {
                    stateVar = new ReadState();
                    stateVar.packetState = PacketState.START_OF_PACKET;
                    stateVar.currentPos = 0;
                }
                return resp;
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
                return ResponseCode.FAILURE;
            }
        }

        /// <summary>
        /// stop continous request
        /// </summary>
        /// <returns></returns>
        public ResponseCode getConfigurationDetails()
        {
            try
            {
                stateVar = new ReadState();
                stateVar.packetState = PacketState.START_OF_PACKET;
                stateVar.currentPos = 0;

                Packet.Packet packet = new Packet.Packet(1048);
                packet.Append(EnumAndConstants.START_FRAME);//SOF
                packet.Append((byte)EnumAndConstants.PROTOCOL_VER);//Version : 1
                DSN = new Random().Next();//Random number
                packet.Append((byte)DSN);
                packet.Append((byte)RequestId.GET_CONFIGURATION); //Request : 5
                packet.Append((byte)EnumAndConstants.PROTOCOL_LENGTH);//Length : 0
                packet.Append((byte)calculateCheckSum(packet, false));//CRC

                return sendRequest(serialPort, ref packet);
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
                return ResponseCode.FAILURE;
            }
        }

        /// <summary>
        /// return XOR value of entire packet exluding checksum
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public byte calculateCheckSum(Packet.Packet packet, bool containsChkSum)
        {
            try
            {
                byte XORCheckSum = 0;

                if (containsChkSum)
                {
                    for (int i = 0; i < packet.DataArray.Length - 1; i++)
                    {
                        XORCheckSum ^= packet.DataArray[i];
                    }
                }
                else
                {
                    for (int i = 0; i < packet.DataArray.Length; i++)
                    {
                        XORCheckSum ^= packet.DataArray[i];
                    }
                }

                return XORCheckSum;
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
                return (byte)0;
            }
        }
    }
}
