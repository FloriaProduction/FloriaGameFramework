using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using System.ComponentModel.Design;

namespace FloriaGF
{
    static class TimeGF
    {
        public static ulong time()
        {
            return Convert.ToUInt64(DateTime.Now.TimeOfDay.TotalMilliseconds);
        }

        // marker: [last_call_time, delay]
        static Dictionary<string, double[]> _every_dict = new();
        public static bool every(string maker, double delay)
        {
            if (!_every_dict.ContainsKey(maker))
            {
                _every_dict[maker] = [time(), delay];
            } 
            
            if (_every_dict[maker][0] + _every_dict[maker][1] < time())
            { 
                _every_dict[maker][0] = time();
                return true;
            }

            return false;
        }


        // marker: [count_tick, currect_tick], 
        static Dictionary<string, double[]> _everyTick_dict = new();
        static double getTick(double delay)
        {
            return (time() - AppGF.start_time) / delay;
        }
        public static bool everyTick(string marker, double delay)
        {
            if (double.IsInfinity(delay)) return true;

            if (_everyTick_dict.ContainsKey(marker))
            {
                double[] pair = _everyTick_dict[marker];
                if (pair[0] < getTick(delay) - pair[1]) {
                    pair[0]++;
                    return true;
                }
            }
            else
                _everyTick_dict[marker] = [0, getTick(delay)];

            return false;
        }
    }
}
