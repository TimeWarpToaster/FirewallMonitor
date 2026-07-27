//Firewall Monitor v04
//(c) 2026 - TimeWarpToaster

//https://www.gnu.org/licenses/gpl-3.0.html

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FWM_Client_03
{
    public class Counts
    {
        public const string CLASSNAME = "Counts";

        public long CntEvents = 0;
        public long CntEvents30Days = 0;
        public long CntEvents7Days = 0;
        public long CntEvents1Day = 0;
        public long CntEvents6Hrs = 0;
        public long CntEvents1Hr = 0;
        public long CntEvents30Min = 0;

        public long CntUNames = 0;

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }






    }


}
