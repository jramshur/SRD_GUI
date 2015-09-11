////////////////////////////////////////////////////////////////////////////////
//
// <copyright file="ApplicationUtil.cs">
// MAVEN SYSTEM PRIVATE LIMITED
//
// Copyright 2012 Maven System Private Limited Incorporated
//
// Author:NIKHILB-PC
//
// All Rights Reserved.
//
// NOTICE: Maven System permits you to use, modify, and distribute this file
//
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Data;
using System.Windows.Forms;
using System.Reflection;
using System.Web;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Globalization;
using CustomLogger;
using System.Diagnostics;
using System.Configuration;
using System.Text;
using VOManager;
using System.Collections;

namespace ApplicationUtil
{
    /// <summary>
    /// Author: Nikhil Bindoo
    /// </summary>
    public class ApplicationUtil
    {
        private static ILogger m_logger = MyLogFactory.GetInstance(LoggerType.Log4Net).GetLogger(System.Reflection.Assembly.GetExecutingAssembly(), typeof(ApplicationUtil));
        //CSV file extension
        private const String CSV_FILE_EXTENSION = ".csv";
        //public static bool CreateDirectory = true;
        private static Boolean headerDone = false;
        public static bool savefile = false;
        public static bool configWrite = false;
        public static string CurrentFolderPath = string.Empty;
        public static bool IscontinousMode = false;
        public static bool IsDirectoryCreateAllowed = true;
        
        public static bool isSaveADCValuesOnly = false;
        public static List<ADCVO> adcValueList = null;
        public static int previousValue = 0;
        public static DateTime minDatetime = DateTime.Now;
        public static double SAMPLING_RATE_DATE_ADD_VALUE = 0.10;
        public static GraphFileCollectionSingleton fileStore = GraphFileCollectionSingleton.getInstance();
        public static int maxADC = 0;
        public static int minADC = 0;
        public static UInt16 FirstRecievedChannelValue = 0;
        
        /// <summary>
        /// ReadFunction for configlist
        /// </summary>
        /// <returns></returns>
        public static List<ConfigurationVO> ReadconfigCsvfile() 
        {
            List<ConfigurationVO> Configlist = new List<ConfigurationVO>();
            try
            {
                string filePath = Assembly.GetExecutingAssembly().Location;
                int position = filePath.LastIndexOf('\\');
                filePath = filePath.Remove(position);
                filePath += EnumAndConstants.CONFIGURATION_FILE;

                StreamReader reader = new StreamReader(File.OpenRead(filePath));
                while (!reader.EndOfStream)
                {
                    string[] line = reader.ReadLine().Split(',');
                    if (line[0] == "") continue;
                    if (line[0].Equals("T1")) continue;


                    ConfigurationVO config = ConfigurationVO.GetInstance();
                    config.T1 = Convert.ToInt32(line[0]);
                    config.T2 = Convert.ToInt32(line[1]);
                    config.T3 = Convert.ToInt32(line[2]);
                    config.T4 = Convert.ToInt32(line[3]);
                    config.T5 = Convert.ToInt32(line[4]);
                    config.T6 = Convert.ToInt32(line[5]);
                    config.T7 = Convert.ToInt32(line[6]);
                    config.T8 = Convert.ToInt32(line[7]);
                    config.T9 = Convert.ToInt32(line[8]);
                    config.T10 = Convert.ToInt32(line[9]);
                    
                    config.mode_operation = Convert.ToInt32(line[11]);
                    config.adcresolution = Convert.ToInt32(line[12]);
                    config.sampaling_rate = Convert.ToInt32(line[13]);
                    config.ChannelValue = Convert.ToUInt16(line[14]);
                    config.T5VDAC = Convert.ToUInt16(line[15]);
                    config.T7VDAC = Convert.ToUInt16(line[16]);
                    Configlist.Add(config);

                }
                reader.Close();
                return Configlist;
            }
            catch(Exception ex) 
            {
                m_logger.Error(ex);
                return Configlist;
            }
        }
        /// <summary>
        /// Read ConfigList and ADCList
        /// </summary>
        /// <returns></returns>

        public static ArrayList ReadconfigadcCsvfile(string filePath)
        {
            ArrayList list = new ArrayList();
            List<ConfigurationVO> Configcollection = new List<ConfigurationVO>();
            List<ADCVO> adccollection = new List<ADCVO>();
            List<ADCVO> adccollectionchannel2 = new List<ADCVO>();
            try
            {
                StreamReader reader = new StreamReader(File.OpenRead(filePath));
                int Case_Number = 0;
                int channel_Ref = 0;
                int channelA= 0;
                int channelB = 0;

                while (!reader.EndOfStream)
                {
                    string[] line = reader.ReadLine().Split(',');
                    if (line[0] == "") continue;
                    if (line[0].Equals("T1"))
                    {
                        Case_Number = 1;
                        continue;
                    }
                    else if (line[0].Equals("Channel"))
                    {
                        //if (line[0].Equals("Channel 1"))
                        if (channelA == 0)
                            channelA = int.Parse(line[1]);
                        else {
                            channelB = int.Parse(line[1]);
                        }
                        //else if (line[0].Equals("Channel 2"))
                           // channel = 2;
                        Case_Number = 2;
                        continue;
                    }

                    switch (Case_Number)
                    {
                        case 1:
                            ConfigurationVO config = ConfigurationVO.GetInstance();
                            config.T1 = Convert.ToInt32(line[0])/ EnumAndConstants.COMMON_FACTOR;
                            config.T2 = Convert.ToInt32(line[1])/ EnumAndConstants.COMMON_FACTOR;
                            config.T3 = Convert.ToInt32(line[2])/ EnumAndConstants.COMMON_FACTOR;
                            config.T4 = Convert.ToInt32(line[3])/ EnumAndConstants.COMMON_FACTOR;
                            config.T5 = Convert.ToInt32(line[4])/ EnumAndConstants.COMMON_FACTOR;
                            config.T6 = Convert.ToInt32(line[5])/ EnumAndConstants.COMMON_FACTOR;
                            config.T7 = Convert.ToInt32(line[6])/ EnumAndConstants.COMMON_FACTOR;
                            config.T8 = Convert.ToInt32(line[7])/ EnumAndConstants.COMMON_FACTOR;
                            config.T9 = Convert.ToInt32(line[8])/ EnumAndConstants.COMMON_FACTOR;
                            config.T10 = Convert.ToInt32(line[9])/ EnumAndConstants.COMMON_FACTOR;
                            
                            //channel_Ref = config.channel;
                            config.ChannelValue = Convert.ToUInt16(line[10]);
                            config.mode_operation = Convert.ToInt32(line[11]);
                            config.adcresolution = Convert.ToInt32(line[12]);
                            config.sampaling_rate = Convert.ToInt32(line[13]);
                            
                            config.T5VDAC = Convert.ToUInt16(line[14]);
                            config.T7VDAC = Convert.ToUInt16(line[15]);
                            Configcollection.Add(config);
                            break;
                        case 2:
                            if(line[0].Equals("ADC Value")|| line[0].Equals("Channel 2"))
                                continue;
                            
                            ADCVO adcvo = new ADCVO();
                            adcvo.AdcValue = int.Parse(line[0]);
                            adcvo.AdcDate = new DateTime(Convert.ToInt64(line[1]));
                            if (channelA != 0 && channelB == 0)
                            {
                                adcvo.Channel = channelA;
                                adccollection.Add(adcvo);
                            }
                            else if (channelB != 0)
                            {
                                adcvo.Channel = channelB;
                                adccollectionchannel2.Add(adcvo);
                            }
                            else
                                adccollection.Add(adcvo);
                                break;
                    }

                }
                list.Add(Configcollection);
                list.Add(adccollection);
                if (channelB!=0) 
                list.Add(adccollectionchannel2);
                reader.Close();
                return list;
            }
            catch (Exception ex)
            {
                m_logger.Error(ex);
                return list;
            }
        }
       
        

        /// <summary>
        /// Get file path
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static String GetFilePath(String fullPath)
        {
            if (fullPath == null || fullPath == "")
                return "";

            String filePath = String.Empty;
            if (null != fullPath && fullPath.Length > 0)
            {
                int index = fullPath.LastIndexOf("\\");
                filePath = fullPath.Substring(0, index);
            }
            return filePath;
        }

        /// <summary>
        /// Check for File Exist
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Boolean CheckFileExists(String filePath)
        {
            if (filePath == null || filePath == "")
                return false;
            Boolean result = false;
            if (File.Exists(filePath))
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Check for Directory Exist
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Boolean CheckDirectoryExists(String dirPath)
        {
            if (dirPath == null || dirPath == "")
                return false;
            Boolean result = false;
            if (Directory.Exists(dirPath))
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Create File 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Boolean CreateFile(String filePath)
        {
            Boolean result = false;
            try
            {
                FileStream fileStream = File.Create(filePath);
                fileStream.Close();
                fileStream.Dispose();
                result = true;
            }
            catch (Exception ex)
            {
                return result;
            }
            return result;
        }

        /// <summary>
        /// Get File name with date time
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static String GetFileNameWithDateTime(String extension)
        {
            if (extension == null )
                return "";
            if (extension == "")
            {
                StringBuilder builder1 = new StringBuilder(String.Format("{0:dd-MMM-yyyy_HH_mm_ss_fff}", DateTime.Now));
                return builder1.ToString();
            }

            StringBuilder builder = new StringBuilder(String.Format("{0:dd-MMM-yyyy_HH_mm_ss_fff}", DateTime.Now)).Append(extension);
            return builder.ToString();
        }
        /// <summary>
        /// Get all sub directory files
        /// </summary>
        /// <param name="baseDir"></param>
        
/// <param name="routeName"></param>
        /// <returns></returns>
        public static List<String> GetSubDirectoryFiles(String baseDir, String extension)
        {
            List<String> fileNameList = new List<String>();
            try
            {
                if (extension == null || extension == "")
                    return fileNameList;

                DirectoryInfo dirInfo = new DirectoryInfo(baseDir);
                foreach (FileInfo fileInfo in dirInfo.GetFiles(extension, SearchOption.AllDirectories))
                {
                    fileNameList.Add(fileInfo.Name);
                }
                return fileNameList;
            }
            catch (Exception ex)
            {
                return fileNameList;
            }
        }

        /// <summary>
        /// Get File name with extension
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static String GetFileName(String filePath)
        {
            if (filePath != null && filePath == "")
                return Path.GetFileName(filePath);
            return "";
        }

        /// <summary>
        /// Get filename without extension
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static String GetFileNameWithoutExtension(String filePath)
        {
            if (filePath == null || filePath == "")
                return "";
            return Path.GetFileNameWithoutExtension(filePath);
        }

        /// <summary>
        /// Create CSV for ADC and configuration parameters
        /// </summary>
        /// <param name="config"></param>
        /// <param name="csvNameWithExt"></param>
        public static void CreateCsvArrayList(ArrayList config, string csvNameWithExt, int ChannelOne, int ChannelTwo)
        {
            try
            {
                if (config == null || config.Count == 0)
                    return;

                List<ConfigurationVO> configList = (List<ConfigurationVO>)config[0];
                int channel = configList[0].channel;

                string newLine = Environment.NewLine;
                bool isNewFile = false;
                if (savefile == false)
                {
                    isNewFile = true;
                    if (File.Exists(csvNameWithExt))
                    {
                        File.Delete(csvNameWithExt);
                    }
                }
                else
                {
                    if (!File.Exists(csvNameWithExt))
                    {
                        isNewFile = true;
                    }

                }
                if (!Directory.Exists(Path.GetDirectoryName(csvNameWithExt)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(csvNameWithExt));
                }

                using (StreamWriter sw = new StreamWriter(csvNameWithExt, true))
                {
                    StringBuilder sb = null;

                    sb = new StringBuilder();
                    if (isNewFile)
                    {
                        for (int j = 1; j <= 10; j++)
                        {
                            sb.Append("T" + j);
                            sb.Append(",");
                        }
                        sb.Append("CHANNEL_VALUE");
                        sb.Append(",");
                        sb.Append("MODE_OPERATION");
                        sb.Append(",");
                        sb.Append("ADCRESOLUTION");
                        sb.Append(",");
                        sb.Append("SAMPALING_RATE");
                        sb.Append(",");
                        sb.Append("T5_VDAC_VALUE");
                        sb.Append(",");
                        sb.Append("T7_VDAC_VALUE");
                        sb.Append(",");
                        sb.Append("\n");

                        ConfigurationVO ConfigVO = configList[0];
                        if (ConfigVO == null)
                            return;
                        sb.Append(ConfigVO.T1);
                        sb.Append(",");
                        sb.Append(ConfigVO.T2);
                        sb.Append(",");
                        sb.Append(ConfigVO.T3);
                        sb.Append(",");
                        sb.Append(ConfigVO.T4);
                        sb.Append(",");
                        sb.Append(ConfigVO.T5);
                        sb.Append(",");
                        sb.Append(ConfigVO.T6);
                        sb.Append(",");
                        sb.Append(ConfigVO.T7);
                        sb.Append(",");
                        sb.Append(ConfigVO.T8);
                        sb.Append(",");
                        sb.Append(ConfigVO.T9);
                        sb.Append(",");
                        sb.Append(ConfigVO.T10);
                        sb.Append(",");

                        sb.Append(ConfigVO.ChannelValue);
                        sb.Append(",");
                        sb.Append(ConfigVO.mode_operation);
                        sb.Append(",");
                        sb.Append(ConfigVO.adcresolution);
                        sb.Append(",");
                        sb.Append(ConfigVO.sampaling_rate);
                        sb.Append(",");
                        sb.Append(ConfigVO.T5VDAC);
                        sb.Append(",");
                        sb.Append(ConfigVO.T7VDAC);

                        sb.Append("\n");
                        sw.WriteLine(sb.ToString());
                    }
                    //Add Adcvalues
                    sb = sb.Remove(0, sb.Length);
                    sb.Append("Channel");
                    sb.Append(",");
                    sb.Append(ChannelOne);
                    sb.Append("\n");
                    sb.Append("ADC Value");
                    sb.Append(",");
                    sb.Append("Date");
                    sb.Append("\n");
                    List<ADCVO> AdcCollection = null;
                    AdcCollection = (List<ADCVO>)config[1];
                    if (AdcCollection == null || AdcCollection.Count == 0) return;

                    for (int iterator = 0; iterator < AdcCollection.Count; iterator++)
                    {
                        ADCVO AdcVo = AdcCollection.ElementAt(iterator);
                        if (AdcVo == null)
                            continue;
                        sb.Append(AdcVo.AdcValue);
                        sb.Append(",");
                        sb.Append(AdcVo.AdcDate.Ticks);
                        sb.Append("\n");
                    }
                    sw.WriteLine(sb.ToString());
                    sb = sb.Remove(0, sb.Length);
                    // both channels
                    if (config.Count > 2)
                    {
                        AdcCollection = (List<ADCVO>)config[2];
                        sb.Append("Channel");
                        sb.Append(",");
                        sb.Append(ChannelTwo);
                        sb.Append("\n");
                        sb.Append("ADC Value");
                        sb.Append(",");
                        sb.Append("Date");
                        sb.Append("\n");
                        for (int iterator = 0; iterator < AdcCollection.Count; iterator++)
                        {
                            ADCVO AdcVo = AdcCollection.ElementAt(iterator);
                            if (AdcVo == null)
                                continue;
                            sb.Append(AdcVo.AdcValue);
                            sb.Append(",");
                            sb.Append(AdcVo.AdcDate.Ticks);
                            sb.Append("\n");
                        }
                        sw.WriteLine(sb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                m_logger.Error(ex, csvNameWithExt);
            }
        }

        /// <summary>
        /// Creates the CSV from a generic list.
        /// </summary>;
        /// <typeparam name="T"></typeparam>;
        /// <param name="list">The list.</param>;
        /// <param name="csvNameWithExt">Name of CSV (w/ path) w/ file ext.</param>;
        public static void CreateCSVFromGenericList<T>(List<T> list, string csvNameWithExt)
        {
            if (list == null || list.Count == 0) return;

            //get type from 0th member
            Type t = list[0].GetType();
            string newLine = Environment.NewLine;
            if (!Directory.Exists(Path.GetDirectoryName(csvNameWithExt)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(csvNameWithExt));
            }
            using (var sw = new StreamWriter(csvNameWithExt))
            {
                //make a new instance of the class name we figured out to get its props
                object o = Activator.CreateInstance(t);
                //gets all properties
                PropertyInfo[] props = o.GetType().GetProperties();

                //foreach of the properties in class above, write out properties
                //this is the header row
                foreach (PropertyInfo pi in props)
                {
                    sw.Write(pi.Name.ToUpper() + ",");
                }
                sw.Write(newLine);

                //this acts as datarow
                foreach (T item in list)
                {
                    //this acts as datacolumn
                    foreach (PropertyInfo pi in props)
                    {
                        //this is the row+col intersection (the value)
                        string whatToWrite =
                            Convert.ToString(item.GetType()
                                                 .GetProperty(pi.Name)
                                                 .GetValue(item, null))
                                .Replace(',', ' ') + ',';

                        sw.Write(whatToWrite);

                    }
                    sw.Write(newLine);
                }
            }
        }


        /// <summary>
        /// Get File path for address binary file
        /// </summary>
        /// <returns></returns>
        public static string GetApplicationPath()
        {
            string filePath = string.Empty;

            //get path till debug/release folder
            filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return filePath;
        }

        public static bool checkSpecialCharacter(string inputvalue)
        {
            Regex isnumber = new Regex("[^0-9a-zA-z]");
            return !isnumber.IsMatch(inputvalue);
        }
        /// <summary>
        /// Check for number validator
        /// </summary>
        /// <param name="inputvalue"></param>
        /// <returns></returns>
        public static bool IsItNumber(string inputvalue)
        {
            Regex isnumber = new Regex("[^0-9]");
            return !isnumber.IsMatch(inputvalue);
        }

        /// <summary>
        /// Create directory for given file path
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string CreateDirectory(string fileName)
        {
            if (!Directory.Exists(fileName))
            {
                Directory.CreateDirectory(fileName);
            }
            return fileName;
        }

        /// <summary>
        /// Check multiple instance of application
        /// </summary>
        /// <returns></returns>
        public static bool CheckMultipleInstance(string processName)
        {
            try
            {
                Process[] process = Process.GetProcessesByName(processName);
                if (process.Length > 1)
                    return true;
            }
            catch (Exception ex)
            {
                m_logger.Error("Error :: " + ex);
                return false;
            }
            return false;
        }

        static int[] MaskArray = { 1, 3, 7, 15, 31, 63, 127, 255 };

        public static void GetValueFromNumber(UInt16 originalNumber, out List<int> Channels)
        {
            
            int channel1 = -1;
            Channels = new List<int>();
            int channel2 = -1;
            try
            {
                
                    for (int i = 0; i < 12; i++)
                    {

                        if ((originalNumber & 1) == 1)
                        {
                            if (channel1 == -1)
                            {
                                channel1 = i + 1;
                                Channels.Add(channel1);
                            }
                            else
                            {
                                channel2 = i + 1;
                                Channels.Add(channel2);
                            }
                        }
                        originalNumber >>= 1;
                    }
                

               
            }
            catch (Exception)
            {
            }
        }

        public static void addFileNameToList(UInt16 channelValue, string filename)
        {
            if (fileStore == null)
            {
                fileStore = GraphFileCollectionSingleton.getInstance();
            }
          
          Dictionary<UInt16, List<string>> map = new Dictionary<ushort, List<string>>();
          if (fileStore.channelFileMap == null)
          {
              fileStore.channelFileMap = new Dictionary<ushort, List<string>>();
          }
          map = fileStore.channelFileMap;

          if (map.ContainsKey(channelValue))
            {
                List<string> files = map[channelValue];
                files.Add(filename);
                map[channelValue]=files;

            }

            else
            {
                List<string> files = new List<string>();
                files.Add(filename);
                map.Add(channelValue, files);

            
            }

          fileStore.channelFileMap = map;
                  
        }


        public static void CalculateMinMaxADCValues(List<ADCVO> adcList)
        {
            maxADC = 0;
            minADC = 0;
            foreach (var adc in adcList)
            {
                if (minADC == maxADC && maxADC == 0)
                {
                    maxADC = adc.AdcValue;
                    minADC = adc.AdcValue;
                }
                if (adc.AdcValue > maxADC)
                {
                    maxADC = adc.AdcValue;
                }
                else if(adc.AdcValue < minADC)
                {
                    minADC = adc.AdcValue;
                }
            }
       }

        public static void CreateCsvArrayListVersion2(ArrayList config, string csvNameWithExt, int ChannelOne, int ChannelTwo,bool CreateNew)
        {

            if (config == null || config.Count == 0)
                return;

            List<ConfigurationVO> configList = (List<ConfigurationVO>)config[0];
            int channel = configList[0].channel;

            string newLine = Environment.NewLine;
            bool isNewFile = false;
            if (savefile == false)
            {
                isNewFile = true;
                if (File.Exists(csvNameWithExt))
                {
                    File.Delete(csvNameWithExt);
                }
            }
            else
            {
                if (!Directory.Exists(Path.GetDirectoryName(csvNameWithExt)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(csvNameWithExt));
                }
                if (!File.Exists(csvNameWithExt)&& CreateNew==true)
                {
                    isNewFile = true;
                }

                if (File.Exists(csvNameWithExt) && CreateNew == false)
                {
                    isNewFile = false;
                }
                if (File.Exists(csvNameWithExt) && CreateNew == true)
                {
                    isNewFile = true;
                }

            }
            
            using (StreamWriter sw = new StreamWriter(csvNameWithExt, true))
            {
                StringBuilder sb = null;

                sb = new StringBuilder();
                if (isNewFile)
                {
                    for (int j = 1; j <= 10; j++)
                    {
                        sb.Append("T" + j);
                        sb.Append(",");
                    }
                    sb.Append("CHANNEL_VALUE");
                    sb.Append(",");
                    sb.Append("MODE_OPERATION");
                    sb.Append(",");
                    sb.Append("ADCRESOLUTION");
                    sb.Append(",");
                    sb.Append("SAMPALING_RATE");
                    sb.Append(",");
                    sb.Append("T5_VDAC_VALUE");
                    sb.Append(",");
                    sb.Append("T7_VDAC_VALUE");
                    sb.Append(",");
                    sb.Append("\n");

                    ConfigurationVO ConfigVO = configList[0];
                    if (ConfigVO == null)
                        return;
                    sb.Append(ConfigVO.T1);
                    sb.Append(",");
                    sb.Append(ConfigVO.T2);
                    sb.Append(",");
                    sb.Append(ConfigVO.T3);
                    sb.Append(",");
                    sb.Append(ConfigVO.T4);
                    sb.Append(",");
                    sb.Append(ConfigVO.T5);
                    sb.Append(",");
                    sb.Append(ConfigVO.T6);
                    sb.Append(",");
                    sb.Append(ConfigVO.T7);
                    sb.Append(",");
                    sb.Append(ConfigVO.T8);
                    sb.Append(",");
                    sb.Append(ConfigVO.T9);
                    sb.Append(",");
                    sb.Append(ConfigVO.T10);
                    sb.Append(",");

                    sb.Append(ConfigVO.ChannelValue);
                    sb.Append(",");
                    sb.Append(ConfigVO.mode_operation);
                    sb.Append(",");
                    sb.Append(ConfigVO.adcresolution);
                    sb.Append(",");
                    sb.Append(ConfigVO.sampaling_rate);
                    sb.Append(",");
                    sb.Append(ConfigVO.T5VDAC);
                    sb.Append(",");
                    sb.Append(ConfigVO.T7VDAC);

                    sb.Append("\n");
                    sw.WriteLine(sb.ToString());
                }
                //Add Adcvalues
                sb = sb.Remove(0, sb.Length);
                sb.Append("Channel ");
                sb.Append(",");
                sb.Append(ChannelOne);
                sb.Append("\n");
                sb.Append("ADC Value");
                sb.Append(",");
                sb.Append("Date");
                sb.Append("\n");
                List<ADCVO> AdcCollection = null;
                AdcCollection = (List<ADCVO>)config[1];
                if (AdcCollection == null || AdcCollection.Count == 0) return;

                for (int iterator = 0; iterator < AdcCollection.Count; iterator++)
                {
                    ADCVO AdcVo = AdcCollection.ElementAt(iterator);
                    if (AdcVo == null)
                        continue;
                    sb.Append(AdcVo.AdcValue);
                    sb.Append(",");
                    sb.Append(AdcVo.AdcDate.Ticks);
                    sb.Append("\n");
                }
                sw.WriteLine(sb.ToString());
                sb = sb.Remove(0, sb.Length);
                // both channels
                if (config.Count > 2)
                {
                    AdcCollection = (List<ADCVO>)config[2];
                    sb.Append("Channel ");
                    sb.Append(",");
                    sb.Append(ChannelTwo);
                    sb.Append("\n");
                    sb.Append("ADC Value");
                    sb.Append(",");
                    sb.Append("Date");
                    sb.Append("\n");
                    for (int iterator = 0; iterator < AdcCollection.Count; iterator++)
                    {
                        ADCVO AdcVo = AdcCollection.ElementAt(iterator);
                        if (AdcVo == null)
                            continue;
                        sb.Append(AdcVo.AdcValue);
                        sb.Append(",");
                        sb.Append(AdcVo.AdcDate.Ticks);
                        sb.Append("\n");
                    }
                    sw.WriteLine(sb.ToString());
                }
            }
        }


    }
}
