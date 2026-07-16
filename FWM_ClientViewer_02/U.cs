using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

using Newtonsoft.Json.Linq;

namespace FWM_ClientViewer_02
{
    public static class U
    {
        const string CLASSNAME = "U";

        public static bool isSystemPauses = false;

        public static long appId = 0L;
        public static string appGuid = "";

        public static string sLastReadDate = "";
        public static DateTime LastReadDate;




        public static string decodeString(string inVal)
        {
            const string location = CLASSNAME + ".decodeString";
            string retVal = "";
            try
            {
                if (inVal == null) return retVal; //Early Exit
                if (inVal.Length == 0) return retVal;

                string s = inVal;
                //L.l(location, "RX Length (" + s.Length + ").");
                byte[] temp = Convert.FromBase64String(s);
                //L.l(location, "Bytes after first decode (" + temp.Length + ").");
                s = Encoding.ASCII.GetString(temp);
                //L.l(location, "String after first decode (" + decode.Length + ").");
                temp = Convert.FromBase64String(s);
                //L.l(location, "Bytes after second decode (" + temp.Length + ").");
                s = Encoding.ASCII.GetString(temp);

                retVal = s;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static string encodeString(string inVal)
        {
            const string location = CLASSNAME + ".encodeString";
            string retVal = "";
            try
            {
                if (inVal == null) return retVal; //Early Exit
                if (inVal.Length == 0) return retVal;

                string temp = inVal;
                temp = U.toBase64(temp);
                temp = U.toBase64(temp);
                retVal = temp;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static string escapeCsvField(string field)
        {
            const string location = CLASSNAME + ".escapeCsvField";
            string retVal = "";
            try
            {
                if (field == null) return retVal;

                if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
                {
                    retVal = "\"" + field.Replace("\"", "\"\"") + "\"";
                }
                else 
                {
                    retVal = field;
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        /*
        public static List<Dictionary<string, Object>> GenericDB(string sp, Dictionary<string, object> parms)
        {
            const string location = CLASSNAME + ".GenericDB";
            List<Dictionary<string, object>> retValue = new List<Dictionary<string, object>>();
            try
            {
                if (string.IsNullOrEmpty(U.GetConfig("dbconnect", "")))
                {
                    return retValue;
                }
                if (!db.initDb())
                {
                    //c.logger(location, "Failed to initialize database!", TAG.ERROR);
                    return retValue;
                }


                //string errmsg = "";
                string dupeName = "";
                using (SqlConnection sc = new SqlConnection(db.dbconnect()))
                {

                    sc.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = sc;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = sp;
                        //command.CommandTimeout = 900; // seconds, 900=15min
                        command.CommandTimeout = 0;
                        command.Parameters.Clear();
                        try
                        {
                            foreach (KeyValuePair<string, object> pair in parms)
                            {
                                command.Parameters.AddWithValue(pair.Key, pair.Value);

                            }
                        }
                        catch (Exception ex2)
                        {
                            L.err(location, "Failed to Add Parms: " + ex2.Message);
                        }
                        try
                        {
                            using (SqlDataReader sr = command.ExecuteReader())
                            {
                                while (sr.Read())
                                {
                                    Dictionary<string, Object> resultPair = new Dictionary<string, object>();
                                    bool addedOk = true;
                                    for (int i = 0; i < sr.FieldCount; i++)
                                    {
                                        try
                                        {
                                            if (sr.GetValue(i) == null || sr.GetValue(i) is DBNull) // never return null
                                            {
                                                resultPair.Add(sr.GetName(i), "");
                                            }
                                            else
                                            {
                                                resultPair.Add(sr.GetName(i), sr.GetValue(i));
                                            }
                                        }
                                        catch (Exception ex2)
                                        {
                                            addedOk = false;
                                            dupeName = sr.GetName(i);
                                        }
                                    }
                                    // now add to list
                                    try
                                    {
                                        if (addedOk)
                                        {
                                            retValue.Add(resultPair); // note could fail if there is already a same-named element
                                        }
                                        else
                                        {
                                            L.err(location, "Stored Procedure (" + sp +
                                                ") Returned Duplicate Named Field: " + dupeName);
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                L.ex(location, "Failed to execute SP (" + sp + "): " + ex.Message);
                            }
                            catch (Exception ex2)
                            {
                                L.ex(location, "Failed to execute SP: " + ex.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }


        public static long GenericGetOrdinal(string sp, Dictionary<string, object> parms)
        {
            const string location = CLASSNAME + ".GenericGetOrdinal";
            long retValue = -1L;
            try
            {
                if (string.IsNullOrEmpty(U.GetConfig("dbconnect", "")))
                {
                    return retValue;
                }

                if (!db.initDb())
                {
                    //c.logger(location, "Failed to initialize database!", TAG.ERROR);
                    return retValue;
                }


                List<Dictionary<string, object>> temp = U.GenericDB(sp, parms);
                if (temp.Count == 0 || temp[0].Count == 0)
                {
                    return retValue;
                }
                //retValue = Convert.ToInt64(temp[0][0]);
                //foreach (Dictionary<string, object> temp2 in temp)
                //{
                foreach (KeyValuePair<string, object> pair in temp[0])
                {
                    retValue = Convert.ToInt64(pair.Value);
                    break;// note, only deal with first item
                }
                //break;// note only deal with first row returned
                //}
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }
        */

        /*
        public static string GetConfig(string key)
        {
            string retValue = "";
            try
            {
                ConfigurationManager.RefreshSection("appSettings");
                if (!ConfigurationManager.AppSettings.AllKeys.Contains(key))
                {
                    return retValue;
                }
                retValue = ConfigurationManager.AppSettings[key];
                if (string.IsNullOrEmpty(retValue))
                {
                    retValue = "";
                }
            }
            catch (Exception ex)
            {
                L.ex("Utilities.GetConfig(key)", ex);
            }
            return retValue;
        }

        public static bool GetConfig(string key, bool defualtValue)
        {
            bool retValue = defualtValue;
            try
            {
                string temp = GetConfig(key);
                if (!string.IsNullOrEmpty(temp))
                {
                    retValue = Convert.ToBoolean(temp);
                }
            }
            catch (Exception ex)
            {
                L.ex("Utilities.GetConfig(key,bool)", ex);
            }
            return retValue;
        }

        public static int GetConfig(string key, int defualtValue)
        {
            int retValue = defualtValue;
            try
            {
                string temp = GetConfig(key);
                if (!string.IsNullOrEmpty(temp))
                {
                    retValue = Convert.ToInt32(temp);
                }
            }
            catch (Exception ex)
            {
                L.ex("Utilities.GetConfig(key,int)", ex);
            }
            return retValue;
        }

        public static long GetConfig(string key, long defualtValue)
        {
            long retValue = defualtValue;
            try
            {
                string temp = GetConfig(key);
                if (!string.IsNullOrEmpty(temp))
                {
                    retValue = Convert.ToInt64(temp);
                }
            }
            catch (Exception ex)
            {
                L.ex("Utilities.GetConfig(key,long)", ex);
            }
            return retValue;
        }

        public static string GetConfig(string key, string defualtValue)
        {
            string retValue = defualtValue;
            try
            {
                retValue = GetConfig(key);
                if (string.IsNullOrEmpty(retValue))
                {
                    retValue = defualtValue;
                }
            }
            catch (Exception ex)
            {
                L.ex("Utilities.GetConfig(key,string)", ex);
            }
            return retValue;
        }
        */

        public static object GetSetting(string key)
        {
            const string location = CLASSNAME + ".GetSetting(s)";
            object retVal = null;
            try
            {
                if (key == null || key.Length == 0)
                {
                    L.err(location, "Key was null or empty.");
                    return retVal;
                }

                if (DataMgr.appSettings == null || DataMgr.appSettings.Count == 0)
                {
                    L.err(location, "App settings in memory was null or empty.");
                    return retVal;
                }

                if (DataMgr.appSettings.ContainsKey(key))
                {
                    retVal = DataMgr.appSettings[key];
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static bool GetSetting(string key, bool defaultVal)
        {
            const string location = CLASSNAME + ".GetSetting(s,b)";
            bool retVal = defaultVal;
            try
            {
                object val = U.GetSetting(key);
                if (val == null)
                {
                    // TODO - Decide whether to create value
                    return retVal;
                }
                retVal = Convert.ToBoolean(val);
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static int GetSetting(string key, int defaultVal)
        {
            const string location = CLASSNAME + ".GetSetting(s,i)";
            int retVal = defaultVal;
            try
            {
                object val = U.GetSetting(key);
                if (val == null)
                {
                    return retVal;
                }
                retVal = Convert.ToInt32(val);
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static long GetSetting(string key, long defaultVal)
        {
            const string location = CLASSNAME + ".GetSetting(s,l)";
            long retVal = defaultVal;
            try
            {
                object val = U.GetSetting(key);
                if (val == null)
                {
                    return retVal;
                }
                retVal = Convert.ToInt64(val);
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static string GetSetting(string key, string defaultVal)
        {
            const string location = CLASSNAME + ".GetSetting";
            string retVal = defaultVal == null ? "" : defaultVal;
            try
            {
                object val = U.GetSetting(key);
                if (val == null)
                {
                    return retVal;
                }
                retVal = Convert.ToString(val);
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static bool SetSetting(string key, bool value)
        {
            const string location = CLASSNAME + "SetSetting(s,b)";
            bool retVal = false;
            try
            {
                if (key == null || key.Length == 0)
                {
                    L.err(location, "Key was null or empty.");
                    return retVal;
                }

                if (DataMgr.appSettings == null)
                {
                    L.err(location, "App settings was null or empty at set.");
                    return retVal;
                }

                if (DataMgr.appSettings.ContainsKey(key))
                {
                    DataMgr.appSettings[key] = value;
                }
                else 
                {
                    DataMgr.appSettings.Add(key, value);
                }

                // Flag Result
                retVal = value == (bool)DataMgr.appSettings[key];
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static bool SetSetting(string key, int value)
        {
            const string location = CLASSNAME + ".SetSetting(s,i)";
            bool retVal = false;
            try
            {
                if (key == null || key.Length == 0)
                {
                    L.err(location, "Key was null or empty.");
                    return retVal;
                }

                if (DataMgr.appSettings == null)
                {
                    L.err(location, "App settings was null or empty at set.");
                    return retVal;
                }

                if (DataMgr.appSettings.ContainsKey(key))
                {
                    DataMgr.appSettings[key] = value;
                }
                else
                {
                    DataMgr.appSettings.Add(key, value);
                }

                // Flag Result
                retVal = value == (int)DataMgr.appSettings[key];
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static bool SetSetting(string key, long value)
        {
            const string location = CLASSNAME + ".SetSetting(s,l)";
            bool retVal = false;
            try
            {
                if (key == null || key.Length == 0)
                {
                    L.err(location, "Key was null or empty.");
                    return retVal;
                }

                if (DataMgr.appSettings == null)
                {
                    L.err(location, "App settings was null or empty at set.");
                    return retVal;
                }

                if (DataMgr.appSettings.ContainsKey(key))
                {
                    DataMgr.appSettings[key] = value;
                }
                else
                {
                    DataMgr.appSettings.Add(key, value);
                }

                // Flag Result
                retVal = value == (long)DataMgr.appSettings[key];
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static bool SetSetting(string key, string value)
        {
            const string location = CLASSNAME + ".SetSetting(s,s)";
            bool retVal = false;
            try
            {
                if (key == null || key.Length == 0)
                {
                    L.err(location, "Key was null or empty.");
                    return retVal;
                }

                if (value == null)
                {
                    value = "";//Use an empty string, there is no null in storage
                }

                if (DataMgr.appSettings == null)
                {
                    L.err(location, "App settings was null or empty at set.");
                    return retVal;
                }

                if (DataMgr.appSettings.ContainsKey(key))
                {
                    DataMgr.appSettings[key] = value;
                }
                else
                {
                    DataMgr.appSettings.Add(key, value);
                }

                // Flag Result
                retVal = value == (string)DataMgr.appSettings[key];
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }




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
        */



        public static string getDateString(DateTime dt)
        {
            try
            {
                if (dt != null) return dt.ToString(TAG.DTF);
                else return "";
            }
            catch (Exception ex) { /* Do Nothing */ }
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
            catch (Exception ex) { /* Do Nothing */ }
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
            catch (Exception ex) { /* Do Nothing */ }
            return c.nDt;
        }

        public static double getDouble(Dictionary<string, object> values, string key)
        {
            return U.getDouble(values, key, 0d);
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
            catch (Exception ex) { /* Do Nothing */ }
            return defaultVal;
        }

        public static int getInt(Dictionary<string, object> values, string key)
        {
            return U.getInt(values, key, 0);
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
            catch (Exception ex) { /* Do Nothing */ }
            return defaultVal;
        }

        public static long getLong(Dictionary<string, object> values, string key)
        {
            return U.getLong(values, key, 0L);
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
            catch (Exception ex) { /* Do Nothing */ }
            return defaultVal;
        }

        public static string getString(Dictionary<string, object> values, string key)
        {
            return U.getString(values, key, "");
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
            catch (Exception ex) { /* Do Nothing */}
            return defaultVal;
        }


        public static bool getBool(JArray values, int idx)
        {
            return U.getBool(values, idx, false);
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
            catch (Exception ex) { /* Do Nothing */ }
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
            catch (Exception ex) { /* Do Nothing */ }
            return c.nDt;
        }

        public static double getDouble(JArray values, int idx)
        {
            return U.getDouble(values, idx, 0d);
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
            catch (Exception ex) { /* Do Nothing */ }
            return defaultVal;
        }

        public static int getInt(JArray values, int idx)
        {
            return U.getInt(values, idx, 0);
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
            catch (Exception ex) { /* Do Nothing */ }
            return defaultVal;
        }

        public static long getLong(JArray values, int idx)
        {
            return U.getLong(values, idx, 0L);
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
            catch (Exception ex) { /* Do Nothing */ }
            return defaultVal;
        }

        public static string getString(JArray values, int idx)
        {
            return U.getString(values, idx, "");
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
            catch (Exception ex) { /* Do Nothing */ }
            return defaultVal;
        }




        public static bool getBool(JObject values, string key)
        {
            return U.getBool(values, key, false);
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
            catch (Exception ex) { /* Do Nothing */ }
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
            catch (Exception ex) { /* Do Nothing */ }
            return c.nDt;
        }

        public static double getDouble(JObject values, string key)
        {
            return U.getDouble(values, key, 0d);
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
            catch (Exception ex) { /* Do Nothing */ }
            return defaultVal;
        }

        public static int getInt(JObject values, string key)
        {
            return U.getInt(values, key, 0);
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
            catch (Exception ex) { /* Do Nothing */ }
            return defaultVal;
        }

        public static long getLong(JObject values, string key)
        {
            return U.getLong(values, key, 0L);
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
            catch (Exception ex) { /* Do Nothing */ }
            return defaultVal;
        }

        public static string getString(JObject values, string key)
        {
            return U.getString(values, key, "");
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
            catch (Exception ex) { /* Do Nothing */ }
            return defaultVal;
        }



        public static bool moveFile(string pathFrom, string pathTo)
        {
            bool retValue = false;
            try
            {
                if (string.IsNullOrEmpty(pathFrom)) return retValue;
                if (string.IsNullOrEmpty(pathTo)) return retValue;
                if (!File.Exists(@pathFrom)) return retValue;

                FileInfo fi = new FileInfo(@pathFrom);
                fi.MoveTo(@pathTo);
                retValue = File.Exists(@pathTo);
            }
            catch (Exception ex)
            {
                L.ex(CLASSNAME + ".moveFile", ex);
            }
            return retValue;
        }



        public static string toBase64(string inVal)
        {
            const string location = CLASSNAME + ".toBase64";
            string retVal = "";
            try
            {
                if (inVal == null || inVal.Length == 0)
                {
                    //L.err(location, "Input was null or empty.");
                    return retVal; //Early Exit
                }

                byte[] bytes = Encoding.ASCII.GetBytes(inVal);
                retVal = Convert.ToBase64String(bytes);
                bytes = null;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }



    }
}
