using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VOManager
{
    public class ReadState
    {
        public ReadState()
        {

        }
        public byte[] ReadBuffer { get; set; }
        public byte[] Header { get; set; }
        public int currentPos { get; set; }
        public DateTime lastRead { get; set; }
        public ResponceStatus state { get; set; }
        PacketState m_packetState;
        public byte LastSopRead { get; set; }

        public PacketState packetState
        {
            get
            {
                return m_packetState;
            }
            set
            {
                if (value == PacketState.START_OF_PACKET)
                {
                    LastSopRead = 0;
                    ReadBuffer = new byte[EnumAndConstants.SOP_LENGTH];
                    currentPos = 0;
                }
                if (value == PacketState.HEADER)
                {
                    ReadBuffer = new byte[EnumAndConstants.HEADER_LENGTH];
                    currentPos = 0;
                }
                else if (value == PacketState.PAYLOAD)
                {
                    Header = ReadBuffer;
                    int payloadlen = Header[EnumAndConstants.PAYLOAD_LENGTH_POSITION];
                    ReadBuffer = new byte[payloadlen + 1];
                    currentPos = 0;
                }
                else if (value == PacketState.ADC_COUNT)
                {

                }
                else if (value == PacketState.CONTINUOUS_ADC_COUNT)
                {

                } m_packetState = value;
            }
        }
    }   
}
