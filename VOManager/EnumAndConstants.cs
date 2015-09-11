using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VOManager
{
    public class EnumAndConstants
    {
        public static int START_OF_PACKET = 0x55AA;
        public const string FILE_NAME = "\\AdcDetails.csv";
        public const string CONFIGURATION_FILE = "\\config.csv";
        public static int WAIT_NO_DATA_INTERVAL = 25;
        public const int SOP_LENGTH = 1;
        public const int HEADER_LENGTH = 4;
        public const int PAYLOAD_LENGTH_POSITION = 3;
        public const int PROTOCOL_VER = 1;
        public const UInt16 PROTOCOL_LENGTH = 0;
        public const UInt16 START_FRAME = 0x55AA;
        public static int waitInterDataInterval = 2000;
        public const int RETRY_COUNT = 2;
        public const int RESPONCE_CODE = 6;
        public const string CSV_FORMAT = ".csv";
        public const int START_FRAME_1 = 85;
        public const int START_FRAME_2 = 170;
        public const int BAUD_RATE = 115200;
        public const int DATA_BITS = 8;
        public const float COMMON_FACTOR = 10f;
        public const int BIT_CHECKER = 1;
        public const int T1 = 1;
        public const int T2 = 1;
        public const double T3 = 1;
        public const int T4 = 1;
        public const int T5 = 1;
        public const double T6 = 1;
        public const int T7 = 1;
        public const int T8 = 4;
        public const int T9 = 1;
        public const int T10 = 50;
        public const int YrefGraphLocation=40;
        public const string HELP_FILE = "\\Proposal-ADC logging and display using PSoC controller 30 Aug 2012.pdf";
        public const int CONSTANT_VALUE = 2;
        public const int STOP_RETRY_COUNT = 3;

        //Comport object
        public static ComPortVO comportVO { get; set; }

        //Operation mode constants
        public const int TRIGGER = 1;
        public const int CONTINEOUS = 2;
        public const int EXTERNAL_SIMULATION = 3;
        public const int EXTERNAL_RECORDING = 4;
        public const int LOOK_AHEAD = 5;
    }

    public enum ADCStatus//Status of start of frame and end of frame
    {
        START_ADC=6,
        END_ADC
    }

    public enum ResponseCode
    {
        SUCCESS,
        MODULE_BUSY,
        FAILURE,
        INVALID,
        INVALID_ARGUMENTS,
        TIMEOUT
    }

    public enum CHANNEL_DETAILS
    {
        CHANNEL_1 = 1,
        CHANNEL_2,
    }
    public enum PacketState
    {
        START_OF_PACKET = 0,
        HEADER = 1,
        PAYLOAD = 2,
        ADC_COUNT = 3,
        CONTINUOUS_ADC_COUNT
    }

    public enum ResponceStatus
    {
        Header,
        Payload,
        End_of_Payload,
        CRC_Invalid
    }
    public enum RequestId//Different Types of RequestCode
    {
        HANDSHAKE_WIRELESS_DEVICE=1,
        SET_CONFIGIRATION,
        START_CONTINOUS,
        STOP_CONTINOUS,
        GET_CONFIGURATION,
        SEND_READ_RESPONSE
    }
    public enum ResponceID //Different types of ResponceCode
    {
        NORMAL_RESPONSE,
        INVALID_LENGTH,
        INVALID_DATA,
        INVALID_PROTOCOLVERSION,
        INVALID_CRC,
        INVALID_RESPONCE_HANDLER
    }
}
