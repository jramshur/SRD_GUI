using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VOManager
{
    /// <summary>
    /// Vo for ADC parameters
    /// </summary>
    public class ADCVO
    {
        public int AdcValue { get; set; }//ADC values
        public DateTime AdcDate { get; set; }
        public int Channel { get; set; }
        //ADC values for particular timing
    }
}
