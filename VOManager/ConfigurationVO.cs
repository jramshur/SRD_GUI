using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VOManager
{
    /// <summary>
    /// Vo file for configuration parameters
    /// </summary>
    public class ConfigurationVO
    {
        static ConfigurationVO m_Config;
        public ConfigurationVO()
        {
        }

        static ConfigurationVO()
        {
            m_Config = new ConfigurationVO();
        }
        public static ConfigurationVO GetInstance()
        {
            return m_Config;
        }
        //Timong Parameters
        public float T1 { get; set; }
        public float T2 { get; set; }
        public float T3 { get; set; }
        public float T4 { get; set; }
        public float T5 { get; set; }
        public float T6 { get; set; }
        public float T7 { get; set; }
        public float T8 { get; set; }
        public float T9 { get; set; }
        public float T10 { get; set; }
        public int channel { get; set; }// Channel selection
        public int mode_operation { get; set; } //2 types of mode of operation(Continous Mode and Trigger Mode)
        public int adcresolution { get; set; }
        public int sampaling_rate { get; set; }//Number of samples per unit time
        public UInt16 ChannelValue { get; set; }
        public int T5VDAC { get; set; }
        public int T7VDAC { get; set; }
        public int NumberOfSelectedChannels { get; set; }


    }
}
