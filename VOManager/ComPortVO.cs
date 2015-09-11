using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace VOManager
{
    /// <summary>
    /// 
    /// </summary>
    public class ComPortVO
    {
        public string portName { get; set; }
        public int baudRate { get; set; }
        public Parity parity { get; set; }
        public int dataBits { get; set; }
        public StopBits stopBits { get; set; }
    }
}
