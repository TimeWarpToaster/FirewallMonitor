//Firewall Monitor v04
//(c) 2026 - TimeWarpToaster

//https://www.gnu.org/licenses/gpl-3.0.html

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace FWM_Client_03
{


    public static class c
    {
        public static bool debug = true;
        //public static bool pauseOnDebug = false;
        public static bool debugBlock = false;
        public static bool debugIp = false;
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

        /*
        public static string safeString(XmlDocument doc, string tag, string defaultVal)
        {
            return safeString(doc, tag, defaultVal, false);
        }

        public static string safeString(XmlDocument doc, string tag, string defaultVal, bool outer)
        {
            const string location = "c.safeString";
            string retValue = defaultVal;
            try
            {
                if (doc != null)
                {
                    var nodes = doc.GetElementsByTagName(tag);
                    if (nodes != null && nodes.Count > 0)
                    {
                        retValue = (nodes[0].InnerText == null) ? defaultVal :
                            (outer) ? nodes[0].OuterXml : nodes[0].InnerText;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }
        

        public static string getDateString(DateTime dt)
        {
            try
            {
                if (dt != null) return dt.ToString(TAG.DTF);
                else return "";
            }
            catch (Exception ex) {  }
            return "";
        }
        public static string getString(string val)
        {
            return val == null ? "" : val;
        }



        public static bool getBool(Dictionary<string, object> values, string key)
        {
            return getBool(values, key, false);
        }
        public static bool getBool(Dictionary<string, object> values, string key, bool defaultVal)
        {
            try
            {
                if (values != null && values.Count > 0 && key != null && key.Length > 0 && values[key] != null)
                {
                    return Convert.ToBoolean(values[key]);
                }
            }
            catch (Exception ex) {  }
            return defaultVal;
        }

        public static DateTime getDate(Dictionary<string, object> values, string key)
        {
            try
            {
                if (values != null && values.Count > 0 && key != null && key.Length > 0 && values[key] != null)
                {
                    string temp = Convert.ToString(values[key]);
                    if (temp != null)
                    {
                        DateTime dt = DateTime.Parse(temp);
                        if (dt != null) return dt;
                    }
                }
            }
            catch (Exception ex) {  }
            return c.nDt;
        }

        public static double getDouble(Dictionary<string, object> values, string key)
        {
            return c.getDouble(values, key, 0d);
        }
        public static double getDouble(Dictionary<string, object> values, string key, double defaultVal)
        {
            try
            {
                if (values != null && values.Count > 0 && key != null && key.Length > 0 && values[key] != null)
                {
                    return Convert.ToDouble(values[key]);
                }
            }
            catch (Exception ex) {  }
            return defaultVal;
        }

        public static int getInt(Dictionary<string, object> values, string key)
        {
            return c.getInt(values, key, 0);
        }
        public static int getInt(Dictionary<string, object> values, string key, int defaultVal)
        {
            try
            {
                if (values != null && values.Count > 0 && key != null && key.Length > 0 && values[key] != null)
                {
                    return Convert.ToInt32(values[key]);
                }
            }
            catch (Exception ex) {  }
            return defaultVal;
        }

        public static long getLong(Dictionary<string, object> values, string key)
        {
            return c.getLong(values, key, 0L);
        }
        public static long getLong(Dictionary<string, object> values, string key, long defaultVal)
        {
            try
            {
                if (values != null && values.Count > 0 && key != null && key.Length > 0 && values[key] != null)
                {
                    return Convert.ToInt64(values[key]);
                }
            }
            catch (Exception ex) {  }
            return defaultVal;
        }

        public static string getString(Dictionary<string, object> values, string key)
        {
            return c.getString(values, key, "");
        }
        public static string getString(Dictionary<string, object> values, string key, string defaultVal)
        {
            try
            {
                if (values != null && values.Count > 0 && key != null && key.Length > 0 && values[key] != null)
                {
                    string val = Convert.ToString(values[key]);
                    if (val != null) return val;
                }
            }
            catch (Exception ex) { }
            return defaultVal;
        }


        public static bool getBool(JArray values, int idx)
        {
            return c.getBool(values, idx, false);
        }
        public static bool getBool(JArray values, int idx, bool defaultVal)
        {
            bool retVal = defaultVal;
            try
            {
                if (
                    values != null &&
                    idx >= 0 &&
                    idx < values.Count &&
                    values[idx] != null)
                {
                    // TODO - I do not like string conversion, but this is the safest way..
                    string temp = Convert.ToString(values[idx]);
                    retVal =
                        temp == "true" ||
                        temp == "1";
                }
            }
            catch (Exception ex) { }
            return retVal;
        }

        public static DateTime getDate(JArray values, int idx)
        {
            try
            {
                if (values != null && idx >= 0 && idx < values.Count)
                {
                    string temp = Convert.ToString(values[idx]);
                    DateTime tempDT = DateTime.Parse(temp);
                    if (tempDT != null) return tempDT;
                }
            }
            catch (Exception ex) {  }
            return c.nDt;
        }

        public static double getDouble(JArray values, int idx)
        {
            return c.getDouble(values, idx, 0d);
        }
        public static double getDouble(JArray values, int idx, double defaultVal)
        {
            try
            {
                if (values != null && idx >= 0 && idx < values.Count)
                {
                    return Convert.ToDouble(values[idx]);
                }
            }
            catch (Exception ex) {  }
            return defaultVal;
        }

        public static int getInt(JArray values, int idx)
        {
            return c.getInt(values, idx, 0);
        }
        public static int getInt(JArray values, int idx, int defaultVal)
        {
            try
            {
                if (values != null && idx >= 0 && idx < values.Count)
                {
                    return Convert.ToInt32(values[idx]);
                }
            }
            catch (Exception ex) {  }
            return defaultVal;
        }

        public static long getLong(JArray values, int idx)
        {
            return c.getLong(values, idx, 0L);
        }
        public static long getLong(JArray values, int idx, long defaultVal)
        {
            try
            {
                if (values != null && idx >= 0 && idx < values.Count)
                {
                    return Convert.ToInt64(values[idx]);
                }
            }
            catch (Exception ex) {  }
            return defaultVal;
        }

        public static string getString(JArray values, int idx)
        {
            return c.getString(values, idx, "");
        }
        public static string getString(JArray values, int idx, string defaultVal)
        {
            try
            {
                if (values != null && idx >= 0 && idx < values.Count)
                {
                    return Convert.ToString(values[idx]);
                }
            }
            catch (Exception ex) {  }
            return defaultVal;
        }




        public static bool getBool(JObject values, string key)
        {
            return c.getBool(values, key, false);
        }
        public static bool getBool(JObject values, string key, bool defaultVal)
        {
            bool retVal = defaultVal;
            try
            {
                if (
                    values != null &&
                    key != null &&
                    values[key] != null)
                {
                    // TODO - I do not like string conversion, but this is the safest way..
                    string temp = Convert.ToString(values[key]);
                    retVal =
                        temp == "true" ||
                        temp == "1";
                }
            }
            catch (Exception ex) { }
            return retVal;
        }

        public static DateTime getDate(JObject values, string key)
        {
            try
            {
                if (values != null && key != null && values[key] != null)
                {
                    string temp = Convert.ToString(values[key]);
                    DateTime tempDT = DateTime.Parse(temp);
                    if (tempDT != null) return tempDT;
                }
            }
            catch (Exception ex) {  }
            return c.nDt;
        }

        public static double getDouble(JObject values, string key)
        {
            return c.getDouble(values, key, 0d);
        }
        public static double getDouble(JObject values, string key, double defaultVal)
        {
            try
            {
                if (values != null && key != null && values[key] != null)
                {
                    return Convert.ToDouble(values[key]);
                }
            }
            catch (Exception ex) {  }
            return defaultVal;
        }

        public static int getInt(JObject values, string key)
        {
            return c.getInt(values, key, 0);
        }
        public static int getInt(JObject values, string key, int defaultVal)
        {
            try
            {
                if (values != null && key != null && values[key] != null)
                {
                    return Convert.ToInt32(values[key]);
                }
            }
            catch (Exception ex) {  }
            return defaultVal;
        }

        public static long getLong(JObject values, string key)
        {
            return c.getLong(values, key, 0L);
        }
        public static long getLong(JObject values, string key, long defaultVal)
        {
            try
            {
                if (values != null && key != null && values[key] != null)
                {
                    return Convert.ToInt64(values[key]);
                }
            }
            catch (Exception ex) { }
            return defaultVal;
        }

        public static string getString(JObject values, string key)
        {
            return c.getString(values, key, "");
        }
        public static string getString(JObject values, string key, string defaultVal)
        {
            try
            {
                if (values != null && key != null && values[key] != null)
                {
                    return Convert.ToString(values[key]);
                }
            }
            catch (Exception ex) {  }
            return defaultVal;
        }

        */
    }
}
