using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VOManager;
using System.Collections;

namespace VOManager
{
    public class CacheStore
    {
        private static CacheStore instance = new CacheStore();

        private CacheStore()
        { }

        public static CacheStore getInstance()
        {
            return instance;
        }

        //ADC list
        public List<ADCVO> adcValueList { get; set; }

        //Data to put in file and graph
        public ArrayList comblist { get; set; }

        public int ChannelA = 0;
        public int ChannelB = 0;

        public string directory = string.Empty;

    }
}
