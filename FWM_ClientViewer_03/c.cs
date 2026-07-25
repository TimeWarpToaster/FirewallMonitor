using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace FWM_ClientViewer_03
{


    public static class c
    {
        public static bool debug = true;
        //public static bool pauseOnDebug = false;
        public static bool debugBlock = false;
        public static bool debugIp = false;
        public static bool isAppAdmin = false;
        public static DateTime nDt = DateTime.Parse("2020-01-01 00:00:00.000");// TODO - initialize this elsewhere
        public static Dictionary<string, IpBlock> ipBlocks = new Dictionary<string, IpBlock>();

        //public const string dtf = "yyyy-MM-dd hh:mm:ss.fff";

        // Configs
        //public static string path = ".\\";
        public static string path = @"C:\Windows\System32\winevt\Logs\Security.evtx";
        public static string sourceType = TAG.FOLDER;


        public static int maxToProcess = 100;





        public static string getBlockAddress(string blockAddress)
        {
            //const string location = CLASSNAME + ".getBlockAddress";
            string retValue = blockAddress;
            try
            {
                // Identify IP block by first-three, auto-corrects if full-IP is used
                String[] split = blockAddress.Split('.');
                retValue = split[0] + "." + split[1] + "." + split[2];
            }
            catch (Exception ex)
            {
                //logger(location, ex.Message, TAG.EXCEPTION);
            }
            return retValue;
        }
    }
}
