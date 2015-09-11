
using System;

namespace GraphLib
{
    public class Utilities
    {
        /// <summary>
        /// returns the most significant decimal digit
        /// 250 -> 100
        /// 350 -> 100
        /// 12 -> 10
        /// 5 -> 1
        /// 0.5 .> 0.1
        /// .....
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        static public double MostSignificantDigit(double Value)
        {
            double n = 1;

            double val_abs = Math.Abs(Value);
            double sig = 1.0f * Math.Sign(Value);

            if (val_abs > 1)
            {
                while (n < val_abs)
                {
                    n *= 10.0f;
                }

                return (double)((int)(sig * n / 10));
            }
            else // n <= 1
            {
                while (n > val_abs)
                {
                    n /= 10.0f;
                }

                return sig * n;
            }
        }
    }
}