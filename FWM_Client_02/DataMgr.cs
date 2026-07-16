using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace FWM_Client_02
{
    /*
     * This class is just a bucket, to hold the apps running data
     */
    public static class DataMgr
    {
        const string CLASSNAME = "DataMgr";

        public static JObject appSettings = new JObject();
        public static List<FailedLoginEvent> FailedLoginEvents = new List<FailedLoginEvent>();
        public static List<FWRow> FWRows = new List<FWRow>();
        public static List<IpBlock> IpBlocks = new List<IpBlock>();
        public static List<IpEvent> IpEvents = new List<IpEvent>();
        public static List<Summary> Summaries = new List<Summary>();
        public static List<UName> UNames = new List<UName>();
        public static List<XRFSum> XRFSums = new List<XRFSum>();


        // Stubs. Move up once active. Mave classes to non-static
        public class FWMSystem
        {
            public bool isValid = false;
            public DateTime dtLastEventLogReadTime;
            public string sLastEventLogReadTime = "";
        }
        public static FWMSystem system = new FWMSystem();



        // Start where it makes sense. Only the main dispatch is well defined, with known input and output

        // To prevent breaking old code, the following function mimics generic db calls
        public static List<object> GenericFile(string sp, object parms)
        {
            const string location = CLASSNAME + ".GenericFile";
            List<object> retVal = new List<object>();
            try
            {
                if (sp == null || sp.Length == 0)
                {
                    L.err(location, "Input procedure was null or empty.");
                    return retVal; //Early Exit
                }
                if (parms == null)
                {
                    L.err(location, "Input parameters were null.");
                    return retVal; //Early Exit
                }

                switch (sp)
                {

                    case "usp_GetFWByName": retVal.Add(DataMgr.getFWByName((string)parms, 1)); break;
                    case "usp_GetFWExpiries": retVal.Add(DataMgr.getFWExpiries()); break;
                    case "usp_GetIpBlockById": retVal = DataMgr.getIpBlockById((Dictionary<string, object>)parms); break;
                    case "usp_GetLastEventLogReadTime": retVal.Add(DataMgr.getLastEventLogReadTime()); break;
                    //case "usp_GetUNameByNameIp":        retVal = DataMgr.getUNameByNameIp(parms); break;
                    case "usp_InsertFW2": retVal.Add(DataMgr.updateFW2((FWRow)parms)); break;
                    case "usp_InsertIp": retVal.Add(DataMgr.updateIp((IpEvent)parms)); break;
                    case "usp_InsertLoginEvents": retVal.Add(DataMgr.updateLoginEvents((FailedLoginEvent)parms)); break;
                    case "usp_InsertIpBlock": retVal.Add(DataMgr.updateIpBlock((IpBlock)parms)); break;
                    case "usp_InsertSummary": retVal.Add(DataMgr.updateSummary((Summary)parms)); break;
                    //case "usp_InsertUName":             retVal = DataMgr.insertUName(parms); break;
                    //case "usp_InsertXRFSum":            retVal = DataMgr.insertXRFSum(parms); break;
                    case "usp_IsExistFW": retVal.Add(DataMgr.isExistFW((FWRow)parms) == 0); break;
                    case "usp_IsExistIp": retVal.Add(DataMgr.isExistIp((IpEvent)parms) == 0); break;
                    case "usp_IsExistIpBlock": retVal.Add(DataMgr.isExistIpBlock((IpBlock)parms) == 0); break;
                    case "usp_IsExistSummary": retVal.Add(DataMgr.isExistSummary((Summary)parms) == 0); break;
                    case "usp_UpdateFW": retVal.Add(DataMgr.updateFW2((FWRow)parms)); break;
                    case "usp_UpdateFWDeactivate": retVal.Add(DataMgr.updateFWDeactivate((FWRow)parms)); break;
                    case "usp_UpdateIp": retVal.Add(DataMgr.updateIp((IpEvent)parms)); break;
                    case "usp_UpdateIpBlock": retVal.Add(DataMgr.updateIpBlock((IpBlock)parms)); break;
                    case "usp_UpdateIpBlockIpCnt": retVal.Add(DataMgr.updateIpBlockIpCnt((IpBlock)parms)); break;
                    case "usp_UpdateSummary": retVal.Add(DataMgr.updateSummary((Summary)parms)); break;
                    //case "usp_UpdateUName":             retVal = DataMgr.updateUName(parms); break;
                    default:
                        {
                            L.err(location, "Unknown procedure (" + sp + ").");
                        }
                        break;
                }

                if (retVal == null)
                {
                    retVal = new List<object>();
                    L.err(location, "Response was null. Initializing empty.");
                }

                L.d(location, "Returning (" + retVal.Count + ") rows for procedure (" + sp.Substring(4) + ").");
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }


        // Initialize data
        public static bool loadAllData(bool includeFWRows)
        {
            const string location = CLASSNAME + ".loadAllData";
            bool retVal = false;
            try
            {
                int cntLoadErrors = 0;

                /*DataMgr.system = FileMgr.readSystemFile();
                if (DataMgr.system == null || !DataMgr.system.isValid)
                {
                    cntLoadErrors++;
                    DataMgr.system = new FWMSystem();
                    L.err(location, "Critical error, failed to load system data.");
                    // TODO - Eliminate hard error, build from scratch (assume first init)
                    return retVal; //Early Exit
                }*/

                // Read in Summary, XRFSum, UName, and FailedLoginEvents first. They are not dependant upon child data.


                DataMgr.Summaries = FileMgr.readSummary();
                if (DataMgr.Summaries == null)
                {
                    DataMgr.Summaries = new List<Summary>();
                    cntLoadErrors++;
                }

                DataMgr.XRFSums = FileMgr.readXRFSum();
                if (DataMgr.XRFSums == null)
                {
                    DataMgr.XRFSums = new List<XRFSum>();
                    cntLoadErrors++;
                }

                DataMgr.UNames = FileMgr.readUNames();
                if (DataMgr.UNames == null)
                {
                    DataMgr.UNames = new List<UName>();
                    cntLoadErrors++;
                }

                DataMgr.FailedLoginEvents = FileMgr.readFailedLoginEvents();
                if (DataMgr.FailedLoginEvents == null)
                {
                    DataMgr.FailedLoginEvents = new List<FailedLoginEvent>();
                    cntLoadErrors++;
                }


                // Next read in Ip THEN IpBlock

                DataMgr.IpEvents = FileMgr.readIpEvent();
                if (DataMgr.IpEvents == null)
                {
                    DataMgr.IpEvents = new List<IpEvent>();
                    cntLoadErrors++;
                }

                DataMgr.IpBlocks = FileMgr.readIpBlock();
                if (DataMgr.IpBlocks == null)
                {
                    DataMgr.IpBlocks = new List<IpBlock>();
                    cntLoadErrors++;
                }


                // Finally, read in FW data
                if (includeFWRows)
                {
                    DataMgr.FWRows = FileMgr.readFWRows();
                    if (DataMgr.FWRows == null)
                    {
                        DataMgr.FWRows = new List<FWRow>();
                        cntLoadErrors++;
                    }
                }

                if (cntLoadErrors > 0)
                {
                    L.err(location, "Encountered (" + cntLoadErrors + ") file errors.");
                }

                L.l(location, "Loaded :: Events (" + DataMgr.FailedLoginEvents.Count + "), FW Rows (" + DataMgr.FWRows.Count +
                    "), Ip Blocks (" + DataMgr.IpBlocks.Count + "), Ips (" + DataMgr.IpEvents.Count + "), Summaries (" + DataMgr.Summaries.Count + ").");

                // Flag result
                retVal = cntLoadErrors == 0;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static bool saveAllData()
        {
            const string location = CLASSNAME + ".saveAllData";
            bool retVal = false;
            try
            {
                //bool errors = false;
                int cntTotalWritten = 0;
                int cntTotalExpected = 0;

                int cntFailedLogins = FileMgr.writeFailedLoginEvents(DataMgr.FailedLoginEvents);
                if (cntFailedLogins != DataMgr.FailedLoginEvents.Count)
                    L.err(location, "Wrote (" + cntFailedLogins + ") of (" + DataMgr.FailedLoginEvents.Count + ") failed login events.");
                cntTotalWritten += cntFailedLogins;
                cntTotalExpected += DataMgr.FailedLoginEvents.Count;


                int cntFWRow = FileMgr.writeFWRows(DataMgr.FWRows);
                if (cntFWRow != DataMgr.FWRows.Count)
                    L.err(location, "Wrote (" + cntFWRow + ") of (" + DataMgr.FWRows.Count + ") firewall rows.");
                cntTotalWritten += cntFWRow;
                cntTotalExpected += DataMgr.FWRows.Count;


                int cntIpBlock = FileMgr.writeIpBlock(DataMgr.IpBlocks);
                if (cntIpBlock != DataMgr.IpBlocks.Count)
                    L.err(location, "Wrote (" + cntIpBlock + ") of (" + DataMgr.IpBlocks.Count + ") ip blocks.");
                cntTotalWritten += cntIpBlock;
                cntTotalExpected += DataMgr.IpBlocks.Count;


                int cntIpEvent = FileMgr.writeIpEvent(DataMgr.IpEvents);
                if (cntIpEvent != DataMgr.IpEvents.Count)
                    L.err(location, "Wrote (" + cntIpEvent + ") of (" + DataMgr.IpEvents.Count + ") ip events.");
                cntTotalWritten += cntIpEvent;
                cntTotalExpected += DataMgr.IpEvents.Count;


                int cntSummary = FileMgr.writeSummary(DataMgr.Summaries);
                if (cntSummary != DataMgr.Summaries.Count)
                    L.err(location, "Wrote (" + cntSummary + ") of (" + DataMgr.Summaries.Count + ") summaries.");
                cntTotalWritten += cntSummary;
                cntTotalExpected += DataMgr.Summaries.Count;


                int cntUName = FileMgr.writeUNames(DataMgr.UNames);
                if (cntUName != DataMgr.UNames.Count)
                    L.err(location, "Wrote (" + cntUName + ") of (" + DataMgr.UNames.Count + ") unames.");
                cntTotalWritten += cntUName;
                cntTotalExpected += DataMgr.UNames.Count;


                int cntXRFSum = FileMgr.writeXRFSum(DataMgr.XRFSums);
                if (cntXRFSum != DataMgr.XRFSums.Count)
                    L.err(location, "Wrote (" + cntXRFSum + ") of (" + DataMgr.XRFSums.Count + ") xrf sums.");
                cntTotalWritten += cntXRFSum;
                cntTotalExpected += DataMgr.XRFSums.Count;


                L.l(location, "Wrote - " +
                    "Failed logins (" + cntFailedLogins + "), " +
                    "firewall rows (" + cntFWRow + "), " +
                    "ip blocks(" + cntIpBlock + "), " +
                    "ip events (" + cntIpEvent + "), " +
                    "summaries (" + cntSummary + "), " +
                    "unames (" + cntUName + "), " +
                    "xrf sums (" + cntXRFSum + ")" +
                    ".");

                // Flag result
                retVal = cntTotalWritten == cntTotalExpected;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }


        /*public static JObject getDefaultAppSettings()
        {
            const string location = CLASSNAME + ".getDefaultAppSettings";
            JObject retVal = new JObject();
            try
            {
                JObject temp = new JObject();

                // Shared between apps
                temp.Add("appId", 0L);
                temp.Add("appGuid", "");

                temp.Add("baseDirectory", @".\");
                //public string pathApplication = "";
                //public string pathAppSettings = "";
                //public string pathFailedLoginEvent = "";
                //public string pathFWRow = "";
                //public string pathIpBlock = "";
                //public string pathIpEvent = "";
                //public string pathSummary = "";
                //public string pathUName = "";
                //public string pathXRFSum = "";

                temp.Add("fileNameApplication", @"Data\application.bin");
                //temp.Add("fileNameAppSettings", "appsettings.bin");
                temp.Add("fileNameFailedLoginEvent", @"Data\datafile1.bin");
                temp.Add("fileNameFWRow", @"Data\datafile2.bin");
                temp.Add("fileNameIpBlock", @"Data\datafile3.bin");
                temp.Add("fileNameIpEvent", @"Data\datafile4.bin");
                temp.Add("fileNameSummary", @"Data\datafile5.bin");
                temp.Add("fileNameUName", @"Data\datafile6.bin");
                temp.Add("fileNameXRFSum", @"Data\datafile7.bin");


                // Viewer settings
                temp.Add("logPathViewer", @"Logs\Viewer\FWM_Viewer_Log.txt");


                // Client settings
                temp.Add("logPathClient", @"Logs\Client\FWM_Client_Log.txt");
                temp.Add("debugModeClient", false);

                temp.Add("maxToProcess", 100000);
                temp.Add("instanceName", "FWMClient02");
                temp.Add("allowMultiInstance", false);
                temp.Add("ApprovedIps", "");

                temp.Add("EventFolder", @"C:\Windows\System32\winevt\Logs\");
                temp.Add("ArchiveFolder", "");
                temp.Add("ReportPath", @"Reports\Client\");
                temp.Add("ReportFilePrefix", "Rpt_");


                temp.Add("IsManageFW", false);
                temp.Add("MinFailuresToBlock", 20);
                temp.Add("FWPrefix", "FWMRule");
                temp.Add("MSBetweenFWTestMin", 30);
                temp.Add("MSBetweenFWTestMax", 60);
                temp.Add("MSBetweenFWAddMin", 200);
                temp.Add("MSBetweenFWAddMax", 400);
                temp.Add("FWMinutesToReview", 10080);
                temp.Add("FWExpireAfterDays", 30);
                temp.Add("FWPort", "ANY");

                temp.Add("FWProtocol", "ANY");





                retVal = temp;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }*/

        public static bool loadAppSettings()
        {
            const string location = CLASSNAME + ".loadAppSettings";
            bool retVal = false;
            try
            {
                // Get the text as it is stored
                //L.l(location, "Reading app settings from storage.");
                if (!FileMgr.lockAppSettings(30))
                {
                    L.err(location, "Failed to obtain lock on app settings file before timeout.");
                    return retVal;
                }
                string fileSettings = FileMgr.readAppSettings();
                if (!FileMgr.unlockAppSettings())
                {
                    L.err(location, "Failed to release lock on app settings file.");
                }

                if (fileSettings == null || fileSettings.Length == 0)
                {
                    L.err(location, "Failed to locate application settings. Aborting.");
                    return retVal;

                    /*// Attempt to create app settings from defaults on failure
                    L.l(location, "Attempting to create app settings from scratch.");

                    JObject jSettings = DataMgr.getDefaultAppSettings();
                    if (!DataMgr.saveAppSettings(jSettings))
                    {
                        L.err(location, "Failed to create settings from scatch.");
                        return retVal;
                    }
                    fileSettings = FileMgr.readAppSettings();
                    if (fileSettings == null || fileSettings.Length == 0)
                    {
                        L.err(location, "Failed to read app settings after initial create.");
                        return retVal;
                    }*/
                }
                //L.l(location, "Finished reading app settings.");

                // Remove any encoding
                string decoded = U.decodeString(fileSettings);
                if (decoded == null || decoded.Length == 0)
                {
                    L.err(location, "Failed to reformat settings from file. Aborting.");
                    return retVal;

                    /*// Something happened to the file contents, attempt to recreate from defaults
                    L.l(location, "Attempting to recreate settings from defaults.");
                    JObject jSettings = DataMgr.getDefaultAppSettings();
                    if (!DataMgr.saveAppSettings(jSettings))
                    {
                        L.err(location, "Failed to create settings from scatch.");
                        return retVal;
                    }
                    fileSettings = FileMgr.readAppSettings();
                    if (fileSettings == null || fileSettings.Length == 0)
                    {
                        L.err(location, "Failed to read app settings after initial create.");
                        return retVal;
                    }
                    decoded = U.decodeString(fileSettings);
                    if (decoded == null || decoded.Length == 0)
                    {
                        L.err(location, "Failed to reformat settings.");
                        return retVal;
                    }*/
                }

                // Convert settings to JSON
                JObject obj = null;
                try
                {
                    obj = JObject.Parse(decoded);
                }
                catch (Exception exConv)
                {
                    L.err(location, "Failed to convert settings with error: " + exConv.Message);
                }
                if (obj == null)
                {
                    return retVal;//Exception should be only error
                }
                //L.l(location, "Settings: " + obj.ToString(Newtonsoft.Json.Formatting.None));

                // Push result to memory
                DataMgr.appSettings = obj;

                // Update all paths in FileMgr
                string baseDirectory = @U.GetSetting("baseDirectory", @".\");
                if (baseDirectory == null || baseDirectory.Length == 0)
                {
                    L.err(location, "Failed to identify base directory when setting paths.");
                }
                else
                {
                    int cntPathErrors = 0;

                    string fileNameApplication = U.GetSetting("fileNameApplication", "application.bin");
                    if (fileNameApplication.Length == 0) fileNameApplication = "application.bin";
                    if (!FileMgr.setPathApplication(@baseDirectory + fileNameApplication))
                    {
                        cntPathErrors++;
                        L.err(location, "Failed to set application path.");
                    }

                    /*
                    string fileNameAppSettings = U.GetSetting("fileNameAppSettings", "appsettings.bin");
                    if (fileNameAppSettings.Length == 0) fileNameAppSettings = "appsettings.bin";
                    if (!FileMgr.setPathAppSettings(baseDirectory + fileNameAppSettings))
                    {
                        cntPathErrors++;
                        L.err(location, "Failed to set app settings path.");
                    }
                    */

                    string fileNameFailedLoginEvent = U.GetSetting("fileNameFailedLoginEvent", "datafile1.bin");
                    if (fileNameFailedLoginEvent.Length == 0) fileNameFailedLoginEvent = "datafile1.bin";
                    if (!FileMgr.setPathFailedLoginEvents(@baseDirectory + fileNameFailedLoginEvent))
                    {
                        cntPathErrors++;
                        L.err(location, "Failed to set failed login events path.");
                    }

                    string fileNameFWRow = U.GetSetting("fileNameFWRow", "datafile2.bin");
                    if (fileNameFWRow.Length == 0) fileNameFWRow = "datafile2.bin";
                    if (!FileMgr.setPathFWRow(@baseDirectory + fileNameFWRow))
                    {
                        cntPathErrors++;
                        L.err(location, "Failed to set firewall path.");
                    }

                    string fileNameIpBlock = U.GetSetting("fileNameIpBlock", "datafile3.bin");
                    if (fileNameIpBlock.Length == 0) fileNameIpBlock = "datafile3.bin";
                    if (!FileMgr.setPathIpBlock(@baseDirectory + fileNameIpBlock))
                    {
                        cntPathErrors++;
                        L.err(location, "Failed to set ip block path.");
                    }

                    string fileNameIpEvent = U.GetSetting("fileNameIpEvent", "datafile4.bin");
                    if (fileNameIpEvent.Length == 0) fileNameIpEvent = "datafile4.bin";
                    if (!FileMgr.setPathIpEvent(@baseDirectory + fileNameIpEvent))
                    {
                        cntPathErrors++;
                        L.err(location, "Failed to set ip path.");
                    }

                    string fileNameSummary = U.GetSetting("fileNameSummary", "datafile5.bin");
                    if (fileNameSummary.Length == 0) fileNameSummary = "datafile5.bin";
                    if (!FileMgr.setPathSummary(@baseDirectory + fileNameSummary))
                    {
                        cntPathErrors++;
                        L.err(location, "Failed to set summary path.");
                    }

                    string fileNameUName = U.GetSetting("fileNameUName", "datafile6.bin");
                    if (fileNameUName.Length == 0) fileNameUName = "datafile6.bin";
                    if (!FileMgr.setPathUName(@baseDirectory + fileNameUName))
                    {
                        cntPathErrors++;
                        L.err(location, "Failed to set uname path.");
                    }

                    string fileNameXRFSum = U.GetSetting("fileNameXRFSum", "datafile7.bin");
                    if (fileNameXRFSum.Length == 0) fileNameXRFSum = "datafile7.bin";
                    if (!FileMgr.setPathXRFSum(@baseDirectory + fileNameXRFSum))
                    {
                        cntPathErrors++;
                        L.err(location, "Failed to set xrf sum path.");
                    }

                }

                retVal =
                    DataMgr.appSettings != null &&
                    DataMgr.appSettings.Count > 0;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static bool saveAppSettings(JObject fullSettings)
        {
            const string location = CLASSNAME + ".saveAppSettings";
            bool retVal = false;
            try
            {
                if (fullSettings == null)
                {
                    L.err(location, "Input was null at save.");
                    return retVal;
                }

                // TODO - Think about this replace going on in memory
                DataMgr.appSettings = fullSettings;

                // Ideally, we save as-is
                string temp = fullSettings.ToString(Newtonsoft.Json.Formatting.None);
                string output = U.encodeString(temp);

                if (!FileMgr.lockAppSettings(30))
                {
                    L.err(location, "Failed to save application settings due to existing lock timeout.");
                }
                else
                {
                    long lengthWritten = FileMgr.writeAppSettings(output);
                    if (!FileMgr.unlockAppSettings())
                    {
                        L.err(location, "Failed to release lock on app settings file.");
                    }

                    if (lengthWritten != output.Length)
                    {
                        L.err(location, "Length written (" + lengthWritten +
                            ") does not match expected (" + output.Length + ").");
                    }
                    else
                    {
                        retVal = true;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
                try
                {
                    if (!FileMgr.unlockAppSettings())
                    {
                        //L.err(location, "Failed to release lock on app settings file.");
                    }
                }
                catch (Exception ex2) { }
            }
            return retVal;
        }


        public static bool isFlagged(string ipAddress, int reqToFlag, DateTime dtScanStart, DateTime dtScanEnd)
        {
            const string location = CLASSNAME + ".isFlagged";
            bool retVal = false;
            try
            {
                int cntFailures = 0;
                for (int i = 0; !retVal && i < DataMgr.FailedLoginEvents.Count; i++)
                {
                    if (
                        ipAddress == DataMgr.FailedLoginEvents[i].IpAddress &&
                        DataMgr.FailedLoginEvents[i].isFailedLogin &&
                        dtScanStart <= DataMgr.FailedLoginEvents[i].CreateDateTime &&
                        dtScanEnd > DataMgr.FailedLoginEvents[i].CreateDateTime
                    )
                    {
                        cntFailures++;
                        if (cntFailures >= reqToFlag) retVal = true;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static int getFailedLoginEventIndex(long failedLoginEventId)
        {
            const string location = CLASSNAME + ".getFailedLoginEventIndex";
            int retVal = -1;
            try
            {
                if (DataMgr.FailedLoginEvents == null)
                {
                    L.err(location, "Failed login events were null at lookup.");
                    return retVal;
                }

                for (int i = 0; i < DataMgr.FailedLoginEvents.Count; i++)
                {
                    if (failedLoginEventId == DataMgr.FailedLoginEvents[i].FailedLoginEventId)
                    {
                        retVal = i;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static void getFailedLoginsForIp()
        {

        }
        public static List<FWRow> getFWByName(string fwname, int active)
        {
            const string location = CLASSNAME + ".getFWByName";
            List<FWRow> retVal = new List<FWRow>();
            try
            {
                // Check Input
                if (fwname == null || fwname.Length == 0)
                {
                    L.err(location, "Input parameters were null or empty.");
                    return retVal; //Early Exit
                }
                active = active == 1 ? 1 : 0;

                // Check memory data
                if (DataMgr.FWRows == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // Iterate
                List<FWRow> result = new List<FWRow>();
                for (int i = 0; i < DataMgr.FWRows.Count; i++)
                {
                    if (
                        DataMgr.FWRows[i] != null &&
                        DataMgr.FWRows[i].Active == active &&
                        DataMgr.FWRows[i].FWName == fwname
                    )
                    {
                        result.Add(DataMgr.FWRows[i]);
                    }
                }

                // Output result
                retVal = result;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static List<FWRow> getFWExpiries()
        {
            const string location = CLASSNAME + ".getFWExpiries";
            List<FWRow> retVal = new List<FWRow>();
            try
            {
                // Check memory data
                if (DataMgr.FWRows == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }
                L.l(location, "Checking (" + DataMgr.FWRows.Count + ") fw rows.");

                // Check parameters 
                // Note:  these are not parameters in the sp
                DateTime now = DateTime.Now;
                int active = 1;


                // Iterate data
                List<FWRow> output = new List<FWRow>();
                for (int i = 0; i < DataMgr.FWRows.Count; i++)
                {
                    if (
                        DataMgr.FWRows[i] != null &&
                        active == DataMgr.FWRows[i].Active &&
                        !DataMgr.FWRows[i].Expired &&

                        DataMgr.FWRows[i].Expiry != null &&
                        DataMgr.FWRows[i].Expiry <= now
                    )
                    {
                        // TODO - Needs to copy object, not reference
                        output.Add(DataMgr.FWRows[i]);
                    }

                    /*if (DataMgr.FWRows[i] != null && DataMgr.FWRows[i].Expiry != null)
                    {
                        L.l(location, "Expiry (" + DataMgr.FWRows[i].Expiry.ToString(TAG.DTF) + "), now (" + now.ToString(TAG.DTF) + ").");
                    }*/
                }

                // Output result
                retVal = output;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static List<object> getIpBlockById(Dictionary<string, object> parms)
        {
            const string location = CLASSNAME + ".getIpBlockById";
            List<object> retVal = new List<object>();
            try
            {
                // Check Input
                if (parms == null)
                {
                    L.err(location, "Input parameters were null.");
                    return retVal; //Early Exit
                }

                // Check memory data
                if (DataMgr.IpBlocks == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // Check parameters
                long ipBlockId = 0L;
                try
                {
                    ipBlockId = Convert.ToInt64(parms["IpBlockId"]);
                }
                catch (Exception exConv) { }
                if (ipBlockId <= 0)
                {
                    L.err(location, "Input block id was invalid.");
                    return retVal; //Early Exit
                }

                // Iterate data
                List<object> output = new List<object>();
                for (int i = 0; i < DataMgr.IpBlocks.Count; i++)
                {
                    if (DataMgr.IpBlocks[i] == null)
                    {
                        continue; //Loop
                    }

                    if (ipBlockId == DataMgr.IpBlocks[i].IpBlockId)
                    {
                        output.Add(DataMgr.IpBlocks[i]);
                    }
                }

                // Output result
                retVal = output;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static IpBlock getIpBlockById(long blockId)
        {
            const string location = CLASSNAME + ".getIpBlockById";
            IpBlock retVal = new IpBlock();
            try
            {
                if (blockId <= 0)
                {
                    L.err(location, "Input id was invalid.");
                    return retVal; //Early Exit
                }

                if (DataMgr.IpBlocks == null)
                {
                    L.err(location, "Memory for ip block was null.");
                    return retVal; //Early Exit
                }

                for (int i = 0; i < DataMgr.IpBlocks.Count; i++)
                {
                    if (DataMgr.IpBlocks[i] == null) continue;
                    if (blockId == DataMgr.IpBlocks[i].IpBlockId)
                    {
                        retVal = DataMgr.IpBlocks[i];
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static IpBlock getIpBlockByBlockAddress(string blockAddress, int active)
        {
            const string location = CLASSNAME + ".getIpBlockByBlockAddress";
            IpBlock retVal = null;
            try
            {
                if (blockAddress == null || blockAddress.Length == 0)
                {
                    L.err(location, "Input was null or empty.");
                    return retVal; //Early Exit
                }
                active = active == 1 ? 1 : 0;

                // Check memory data
                if (DataMgr.IpBlocks == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // Iterate
                // Stop at first valid object
                for (int i = 0; i < DataMgr.IpBlocks.Count; i++)
                {
                    if (
                        DataMgr.IpBlocks[i] != null &&
                        DataMgr.IpBlocks[i].Active == (active == 1 ? true : false) &&
                        DataMgr.IpBlocks[i].BlockAddress == blockAddress &&
                        DataMgr.IpBlocks[i].IpBlockId > 0
                    )
                    {
                        retVal = DataMgr.IpBlocks[i];
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static long getIpBlockIdByBlockAddress(string blockAddress, int active)
        {
            const string location = CLASSNAME + ".getIpBlockIdByBlockAddress";
            long retVal = 0L;
            try
            {
                if (blockAddress == null || blockAddress.Length == 0)
                {
                    L.err(location, "Input was null or empty.");
                    return retVal; //Early Exit
                }
                active = active == 1 ? 1 : 0;

                // Check memory data
                if (DataMgr.IpBlocks == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // Iterate
                // Stop at first valid object
                for (int i = 0; i < DataMgr.IpBlocks.Count; i++)
                {
                    if (
                        DataMgr.IpBlocks[i] != null &&
                        DataMgr.IpBlocks[i].Active == (active == 1 ? true : false) &&
                        DataMgr.IpBlocks[i].BlockAddress == blockAddress &&
                        DataMgr.IpBlocks[i].IpBlockId > 0
                    )
                    {
                        retVal = DataMgr.IpBlocks[i].IpBlockId;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static int getIpIndex(long ipId)
        {
            const string location = CLASSNAME + ".getIpIndex";
            int retVal = -1;
            try
            {
                if (DataMgr.IpEvents == null)
                {
                    L.err(location, "IPs were null when attempting to get index.");
                    return retVal;
                }

                for (int i = 0; i < DataMgr.IpEvents.Count; i++)
                {
                    if (ipId == DataMgr.IpEvents[i].IpEventId)
                    {
                        retVal = i;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static int getIpBlockIndex(long ipBlockId)
        {
            const string location = CLASSNAME + ".getIpBlockIndex";
            int retVal = -1;
            try
            {
                if (DataMgr.IpBlocks == null)
                {
                    //L.err(location, "IP Blocks were null when attempting to get index.");
                    return retVal;
                }

                for (int i = 0; i < DataMgr.IpBlocks.Count; i++)
                {
                    if (ipBlockId == DataMgr.IpBlocks[i].IpBlockId)
                    {
                        retVal = i;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static long getIpIdByIpAddress(string ipAddress, int active)
        {
            const string location = CLASSNAME + ".getIpIdByIpAddress";
            long retVal = 0L;
            try
            {
                if (ipAddress == null || ipAddress.Length == 0)
                {
                    L.err(location, "Input was null or empty.");
                    return retVal; //Early Exit
                }
                active = active == 1 ? 1 : 0;

                // Check memory data
                if (DataMgr.IpEvents == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // Iterate
                // Stop at first valid object
                for (int i = 0; retVal == 0 && i < DataMgr.IpEvents.Count; i++)
                {
                    if (
                        DataMgr.IpEvents[i] != null &&
                        DataMgr.IpEvents[i].Active == (active == 1 ? true : false) &&
                        DataMgr.IpEvents[i].IpAddress == ipAddress &&
                        DataMgr.IpEvents[i].IpId > 0
                    )
                    {
                        retVal = DataMgr.IpEvents[i].IpId;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static IpEvent getIpById(long ipEventId, int active)
        {
            const string location = CLASSNAME + ".getIpById";
            IpEvent retVal = null;
            try
            {
                if (ipEventId <= 0)
                {
                    L.err(location, "Input id was invalid.");
                    return retVal; //Early Exit
                }
                active = active == 1 ? 1 : 0;

                // Check memory data
                if (DataMgr.IpEvents == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // Iterate
                // Stop at first valid object
                for (int i = 0; i < DataMgr.IpEvents.Count; i++)
                {
                    if (
                        DataMgr.IpEvents[i] != null &&
                        DataMgr.IpEvents[i].Active == (active == 1 ? true : false) &&
                        DataMgr.IpEvents[i].IpEventId == ipEventId
                    )
                    {
                        retVal = DataMgr.IpEvents[i];
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static IpEvent getIpByIpAddress(string ipAddress, int active)
        {
            const string location = CLASSNAME + ".getIpByIpAddress";
            IpEvent retVal = new IpEvent();
            try
            {
                if (ipAddress == null || ipAddress.Length == 0)
                {
                    L.err(location, "Input was null or empty.");
                    return retVal; //Early Exit
                }
                active = active == 1 ? 1 : 0;

                // Check memory data
                if (DataMgr.IpEvents == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // Iterate
                // Stop at first valid object
                for (int i = 0; i < DataMgr.IpEvents.Count; i++)
                {
                    if (
                        DataMgr.IpEvents[i] != null &&
                        //DataMgr.IpEvents[i].Active == (active == 1 ? true : false) &&
                        DataMgr.IpEvents[i].IpAddress == ipAddress //&&
                        //DataMgr.IpEvents[i].IpEventId > 0
                    )
                    {
                        retVal = DataMgr.IpEvents[i];
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static DateTime getLastEventLogReadTime(bool fromAppData)
        {
            const string location = CLASSNAME + ".getLastEventLogReadTime(b)";
            DateTime retVal = c.nDt;
            try
            {
                if (!fromAppData)
                {
                    retVal = DataMgr.getLastEventLogReadTime();
                }
                else
                {
                    if (U.LastReadDate != null && U.LastReadDate != (new DateTime()))
                    {
                        retVal = U.LastReadDate;
                    }
                    else if (U.sLastReadDate != null && U.sLastReadDate.Length > 0)
                    {
                        try
                        {
                            U.LastReadDate = DateTime.Parse(U.sLastReadDate);
                            retVal = U.LastReadDate;
                        }
                        catch (Exception exConv) { }
                    }
                    /*string sReadTime = U.GetSetting("LastReadDate", "");
                    if (sReadTime.Length == 0)
                    {
                        return retVal;
                    }
                    else 
                    {
                        try
                        {
                            retVal = DateTime.Parse(sReadTime);
                        }
                        catch (Exception exConv) { }
                    }*/
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static DateTime getLastEventLogReadTime()
        {
            const string location = CLASSNAME + ".getLastEventLogReadTime";
            DateTime retVal = c.nDt;
            try
            {
                // Check memory data
                if (DataMgr.Summaries == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // Find most recent
                // Most recent summary is current app execution. Note: I don't like this
                if (DataMgr.Summaries.Count > 1)
                {
                    int idxLast = -1;
                    DateTime dtLast = c.nDt;
                    DateTime dtSecondLast = c.nDt;

                    L.l(location, "Checking (" + DataMgr.Summaries.Count + ") prior summaries for last app date.");
                    for (int i = 0; i < DataMgr.Summaries.Count; i++)
                    {
                        if (DataMgr.Summaries[i].AppStartDT == null)
                        {
                            // Invalid dt to compare
                            continue; //Loop
                        }
                        if (idxLast < 0 || DataMgr.Summaries[i].AppStartDT > dtLast)
                        {
                            idxLast = i;
                            dtSecondLast = dtLast;
                            dtLast = DataMgr.Summaries[i].AppStartDT;
                        }
                    }

                    // Verify a valid index and date, return date
                    if (dtSecondLast != null)
                    {
                        retVal = dtLast;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static int getUNameIndex(long ipEventId, string uname)
        {
            const string location = CLASSNAME + ".getUNameIndex";
            int retVal = -1;
            try
            {
                if (DataMgr.UNames == null)
                {
                    //L.err(location, "UNames were null when attempting to get index.");
                    return retVal;
                }

                for (int i = 0; i < DataMgr.UNames.Count; i++)
                {
                    if (
                        ipEventId == DataMgr.UNames[i].IpId &&
                        uname == DataMgr.UNames[i].UserName
                    )
                    {
                        retVal = i;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static long getUNameId(long ipEventId, string uname)
        {
            const string location = CLASSNAME + ".getUNameId";
            long retVal = 0;
            try
            {
                if (ipEventId <= 0) return retVal;
                if (uname == null) return retVal;
                if (DataMgr.UNames == null) return retVal;// Too much to error here

                for (int i = 0; i < DataMgr.UNames.Count; i++)
                {
                    if (
                        ipEventId == DataMgr.UNames[i].IpId && 
                        uname == DataMgr.UNames[i].UserName
                    )
                    {
                        retVal = DataMgr.UNames[i].UNameId;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static long isExistFW(FWRow row)
        {
            const string location = CLASSNAME + ".isExistFW";
            long retVal = 0L;
            try
            {
                // Check Input
                if (row == null)
                {
                    L.err(location, "Input parameters were null.");
                    return retVal; //Early Exit
                }

                // Check memory data
                if (DataMgr.FWRows == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // Check parameters
                if (row.FWId <= 0)
                {
                    L.err(location, "Input id (" + row.FWId + ") out of bounds.");
                    return retVal; //Early Exit
                }

                // Iterate data, accept first match
                for (int i = 0; i < DataMgr.FWRows.Count; i++)
                {
                    if (
                        DataMgr.FWRows[i].Active == 1 &&
                        row.FWId == DataMgr.FWRows[i].FWId
                    )
                    {
                        retVal = row.FWId;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static long isExistIp(IpEvent row)
        {
            const string location = CLASSNAME + ".isExistIp";
            long retVal = 0L;
            try
            {
                // Check Input
                if (row == null)
                {
                    L.err(location, "Input parameters were null.");
                    return retVal; //Early Exit
                }

                // Check memory data
                if (DataMgr.IpEvents == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // Check parameters
                if (row.IpId <= 0)
                {
                    L.err(location, "Input id (" + row.IpId + ") out of bounds.");
                    return retVal; //Early Exit
                }

                // Iterate data, accept first match
                for (int i = 0; i < DataMgr.IpEvents.Count; i++)
                {
                    if (
                        // TODO - Synchronize IpEvents.Active format to use int for consistency
                        DataMgr.IpEvents[i].Active == true &&
                        row.IpId == DataMgr.IpEvents[i].IpId
                    )
                    {
                        retVal = row.IpId;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static long isExistIpBlock(IpBlock row)
        {
            const string location = CLASSNAME + ".isExistIpBlock";
            long retVal = 0L;
            try
            {
                // Check Input
                if (row == null)
                {
                    L.err(location, "Input parameters were null.");
                    return retVal; //Early Exit
                }

                // Check memory data
                if (DataMgr.IpBlocks == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // Check parameters
                if (row.IpBlockId <= 0)
                {
                    L.err(location, "Input id (" + row.IpBlockId + ") out of bounds.");
                    return retVal; //Early Exit
                }

                // Iterate data, accept first match
                for (int i = 0; i < DataMgr.IpBlocks.Count; i++)
                {
                    if (
                        // TODO - Synchronize IpEvents.Active format to use int for consistency
                        DataMgr.IpBlocks[i].Active == true &&
                        row.IpBlockId == DataMgr.IpBlocks[i].IpBlockId
                    )
                    {
                        retVal = row.IpBlockId;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static long isExistSummary(Summary row)
        {
            const string location = CLASSNAME + ".isExistSummary";
            long retVal = 0L;
            try
            {
                // Check Input
                if (row == null)
                {
                    L.err(location, "Input parameters were null.");
                    return retVal; //Early Exit
                }

                // Check memory data
                if (DataMgr.Summaries == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // Check parameters
                if (row.SummaryId <= 0)
                {
                    L.err(location, "Input id (" + row.SummaryId + ") out of bounds.");
                    return retVal; //Early Exit
                }

                // Iterate data, accept first match
                for (int i = 0; i < DataMgr.Summaries.Count; i++)
                {
                    if (
                        DataMgr.Summaries[i].Active == 1 &&
                        row.SummaryId == DataMgr.Summaries[i].SummaryId
                    )
                    {
                        retVal = row.SummaryId;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        /*
        public static List<object> getUNameByNameIp(Dictionary<string, object> parms)
        {
            const string location = CLASSNAME + ".getUNameByNameIp";
            List<object> retVal = new List<object>();
            try
            {
                // Check Input
                if (parms == null)
                {
                    L.err(location, "Input parameters were null.");
                    return retVal; //Early Exit
                }

                // Check memory data
                if (DataMgr.UNames == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // Check parameters


                // Iterate data
                List<object> output = new List<object>();
                for (int i = 0; i < DataMgr.UNames.Count; i++)
                {

                }

                // Output result
                retVal = output;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }
        */


        /*
         * The inserts become a toss-up, between custom validating every field, or generically pushing safe 
         */
        public static long updateFailedLoginEvent(FailedLoginEvent row)
        {
            const string location = CLASSNAME + ".updateFailedLoginEvent";
            long retVal = 0L;// This is the rowId, not the index
            try
            {
                // Check Input
                if (row == null)
                {
                    L.err(location, "Input parameters were null.");
                    return retVal; //Early Exit
                }

                // Check memory data
                if (DataMgr.FailedLoginEvents == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // First, see if object exists
                long rowId = 0;
                int rowIdx = 0;
                int matches = 0;
                if (row.FailedLoginEventId > 0)
                {
                    for (int i = 0; i < DataMgr.FailedLoginEvents.Count; i++)
                    {
                        if (
                            //row.Active == 1 &&
                            row.FailedLoginEventId == DataMgr.FailedLoginEvents[i].FailedLoginEventId
                        )
                        {
                            // Take first
                            if (rowId == 0)
                            {
                                rowId = DataMgr.FailedLoginEvents[i].FailedLoginEventId;
                                rowIdx = i;
                                break;
                            }

                            // Count matches
                            matches++;
                        }
                    }
                }

                if (rowId <= 0)
                {
                    // Data does not exist, add

                    if (row.FailedLoginEventId > 0)
                    {
                        // We were looking for a specific row, and did not find it
                        L.err(location, "Row id (" + row.FailedLoginEventId + ") does not exist in data.");
                        return retVal; //Early Exit
                    }

                    // The row is added without an ID, then updated
                    DataMgr.FailedLoginEvents.Add(row);
                    int idx = DataMgr.FailedLoginEvents.Count - 1;

                    // Compare data at index
                    if (row.FailedLoginEventId != DataMgr.FailedLoginEvents[idx].FailedLoginEventId)
                    {
                        L.err(location, "Failed to lookup inserted record with id (" + row.FailedLoginEventId + ") at index (" + idx + ")");
                    }
                    else
                    {
                        // Update data at index with insert id
                        DataMgr.FailedLoginEvents[idx].FailedLoginEventId = idx + 1;
                        row.FailedLoginEventId = DataMgr.FailedLoginEvents[idx].FailedLoginEventId;
                        retVal = row.FailedLoginEventId;
                    }

                }
                else
                {
                    // Update exising row
                    if (matches > 1)
                    {
                        L.err(location, "Found more than one row matching id (" + row.FailedLoginEventId + ").");
                    }
                    else if (rowId <= 0)
                    {
                        L.err(location, "Failed to locate row id (" + rowId + ") for id (" + row.FailedLoginEventId + ").");
                    }
                    else if (rowIdx < 0)
                    {
                        L.err(location, "Failed to initialize index at id (" + row.FailedLoginEventId + ").");
                    }
                    else if (rowIdx >= DataMgr.FailedLoginEvents.Count)
                    {
                        L.err(location, "Record index (" + rowIdx + ") out of bounds (" + (DataMgr.FailedLoginEvents.Count - 1) + ").");
                    }
                    // Reevaluate data store
                    else if (row.FailedLoginEventId != DataMgr.FailedLoginEvents[rowIdx].FailedLoginEventId)
                    {
                        // Something happened, and data no longer matches found
                        L.err(location, "Data changed during update of id (" + row.FailedLoginEventId + ").");
                    }
                    else
                    {
                        // Update data from input
                        DataMgr.FailedLoginEvents[rowIdx] = row;
                        retVal = row.FailedLoginEventId;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static long updateFW2(FWRow row)
        {
            const string location = CLASSNAME + ".updateFW2";
            long retVal = 0L;// This is the rowId, not the index
            try
            {
                // Check Input
                if (row == null)
                {
                    L.err(location, "Input parameters were null.");
                    return retVal; //Early Exit
                }

                // Check memory data
                if (DataMgr.FWRows == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // First, see if object exists
                long rowId = 0;
                int rowIdx = 0;
                int matches = 0;
                if (row.FWId > 0)
                {
                    for (int i = 0; i < DataMgr.FWRows.Count; i++)
                    {
                        if (
                            //row.Active == 1 &&
                            row.FWId == DataMgr.FWRows[i].FWId
                        )
                        {
                            // Take first
                            if (rowId == 0)
                            {
                                rowId = DataMgr.FWRows[i].FWId;
                                rowIdx = i;
                            }

                            // Count matches
                            matches++;
                        }
                    }
                }

                if (rowId <= 0)
                {
                    // Data does not exist, add

                    if (row.FWId > 0)
                    {
                        // We were looking for a specific row, and did not find it
                        L.err(location, "Row id (" + row.FWId + ") does not exist in data.");
                        return retVal; //Early Exit
                    }

                    // The row is added without an ID, then updated
                    DataMgr.FWRows.Add(row);
                    int idx = DataMgr.FWRows.Count - 1;

                    // Compare data at index
                    if (row.FWId != DataMgr.FWRows[idx].FWId)
                    {
                        L.err(location, "Failed to lookup inserted record with id (" + row.FWId + ") at index (" + idx + ")");
                    }
                    else if (row.FWName != DataMgr.FWRows[idx].FWName)
                    {
                        L.err(location, "Inserted data does not match memory at index (" + idx + ").");
                    }
                    else
                    {
                        // Update data at index with insert id
                        DataMgr.FWRows[idx].FWId = idx + 1;
                        row.FWId = DataMgr.FWRows[idx].FWId;
                        retVal = row.FWId;
                    }

                }
                else
                {
                    // Update exising row
                    if (matches > 1)
                    {
                        L.err(location, "Found more than one row matching id (" + row.FWId + ").");
                    }
                    else if (rowId <= 0)
                    {
                        L.err(location, "Failed to locate row for id (" + row.FWId + ").");
                    }
                    else if (rowIdx < 0)
                    {
                        L.err(location, "Failed to initialize index at id (" + row.FWId + ").");
                    }
                    else if (rowIdx >= DataMgr.FWRows.Count)
                    {
                        L.err(location, "Record index (" + rowIdx + ") out of bounds (" + (DataMgr.FWRows.Count - 1) + ").");
                    }
                    // Reevaluate data store
                    else if (row.FWId != DataMgr.FWRows[rowIdx].FWId)
                    {
                        // Something happened, and data no longer matches found
                        L.err(location, "Data changed during update of id (" + row.FWId + ").");
                    }
                    else
                    {
                        // Update data from input
                        DataMgr.FWRows[rowIdx] = row;
                        retVal = row.FWId;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static int updateFWDeactivate(FWRow row)
        {
            const string location = CLASSNAME + ".updateFWDeactivate";
            int retVal = 0; // Rows affected
            try
            {
                // Check Input
                if (row == null)
                {
                    L.err(location, "Input parameters were null.");
                    return retVal; //Early Exit
                }

                // Check memory data
                if (DataMgr.FWRows == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // Check parameters
                for (int i = 0; i < DataMgr.FWRows.Count; i++)
                {
                    if (
                        DataMgr.FWRows[i].Active == 1 &&
                        row.FWId == DataMgr.FWRows[i].FWId
                    )
                    {
                        // TODO - Hardcoded for now.. this is a deactivate only function
                        DataMgr.FWRows[i].Active = 0;
                        retVal++;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static int updateFWExpired(FWRow row)
        {
            const string location = CLASSNAME + ".updateFWExpired";
            int retVal = 0; // Rows affected
            try
            {
                // Check Input
                if (row == null)
                {
                    L.err(location, "Input parameters were null.");
                    return retVal; //Early Exit
                }

                // Check memory data
                if (DataMgr.FWRows == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // Check parameters
                for (int i = 0; i < DataMgr.FWRows.Count; i++)
                {
                    if (
                        DataMgr.FWRows[i].Active == 1 &&
                        row.FWId == DataMgr.FWRows[i].FWId
                    )
                    {
                        DataMgr.FWRows[i].Expired = row.Expired;
                        retVal = 1;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static long updateIp(IpEvent row)
        {
            const string location = CLASSNAME + ".updateIp";
            long retVal = 0L;
            try
            {
                // Check Input
                if (row == null)
                {
                    L.err(location, "Input parameters were null.");
                    return retVal; //Early Exit
                }

                // Check memory data
                if (DataMgr.IpEvents == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // First, see if object exists
                long rowId = 0;
                int rowIdx = 0;
                int matches = 0;
                if (row.IpId > 0)
                {
                    for (int i = 0; i < DataMgr.IpEvents.Count; i++)
                    {
                        if (
                            //row.Active == 1 &&
                            row.IpId == DataMgr.IpEvents[i].IpId
                        )
                        {
                            // Take first
                            if (rowId == 0)
                            {
                                rowId = DataMgr.IpEvents[i].IpId;
                                rowIdx = i;
                            }

                            // Count matches
                            matches++;
                        }
                    }
                }

                if (rowId <= 0)
                {
                    // Quit if we were trying to update with an invalid id
                    if (row.IpId > 0)
                    {
                        // We were looking for a specific row, and did not find it
                        L.err(location, "Row id (" + row.IpId + ") does not exist in data.");
                        return retVal; //Early Exit
                    }

                    // Data does not exist, add

                    // The row is added without an ID, then updated
                    DataMgr.IpEvents.Add(row);
                    int idx = DataMgr.IpEvents.Count - 1;

                    // Compare data at index
                    if (row.IpId != DataMgr.IpEvents[idx].IpId)
                    {
                        L.err(location, "Failed to lookup inserted record with id (" + row.IpId + ") at index (" + idx + ")");
                    }
                    else if (row.IpAddress != DataMgr.IpEvents[idx].IpAddress)
                    {
                        L.err(location, "Inserted data does not match memory at index (" + idx + ").");
                    }
                    else
                    {
                        // Update data at index with insert id
                        DataMgr.IpEvents[idx].IpId = idx + 1;
                        row.IpId = DataMgr.IpEvents[idx].IpId;
                        retVal = row.IpId;
                    }

                }
                else
                {
                    // Update exising row
                    if (matches > 1)
                    {
                        L.err(location, "Found more than one row matching id (" + row.IpId + ").");
                    }
                    else if (rowId <= 0)
                    {
                        L.err(location, "Failed to locate row for id (" + row.IpId + ").");
                    }
                    else if (rowIdx < 0)
                    {
                        L.err(location, "Failed to initialize index at id (" + row.IpId + ").");
                    }
                    else if (rowIdx >= DataMgr.IpEvents.Count)
                    {
                        L.err(location, "Record index (" + rowIdx + ") out of bounds (" + (DataMgr.IpEvents.Count - 1) + ").");
                    }
                    // Reevaluate data store
                    else if (row.IpId != DataMgr.IpEvents[rowIdx].IpId)
                    {
                        // Something happened, and data no longer matches found
                        L.err(location, "Data changed during update of id (" + row.IpId + ").");
                    }
                    else
                    {
                        // Update data from input
                        DataMgr.IpEvents[rowIdx] = row;
                        retVal = row.IpId;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static long updateIpBlock(IpBlock row)
        {
            const string location = CLASSNAME + ".updateIpBlock";
            long retVal = 0L;
            try
            {
                // Check Input
                if (row == null)
                {
                    L.err(location, "Input parameters were null.");
                    return retVal; //Early Exit
                }

                // Check memory data
                if (DataMgr.IpBlocks == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // First, see if object exists
                long rowId = 0;
                int rowIdx = 0;
                int matches = 0;
                if (row.IpBlockId > 0)
                {
                    for (int i = 0; i < DataMgr.IpBlocks.Count; i++)
                    {
                        if (
                            //row.Active == 1 &&
                            row.IpBlockId == DataMgr.IpBlocks[i].IpBlockId
                        )
                        {
                            // Take first
                            if (rowId == 0)
                            {
                                rowId = DataMgr.IpBlocks[i].IpBlockId;
                                rowIdx = i;
                            }

                            // Count matches
                            matches++;
                        }
                    }
                }
                else if (row.BlockAddress != null && row.BlockAddress.Length > 0)
                {
                    // Block addresses are unique, see if an active is already assigned
                    for (int i = 0; i < DataMgr.IpBlocks.Count; i++)
                    {
                        if (
                            row.Active == true &&
                            row.BlockAddress == DataMgr.IpBlocks[i].BlockAddress
                        )
                        {
                            // Take first
                            if (rowId == 0)
                            {
                                rowId = DataMgr.IpBlocks[i].IpBlockId;
                                rowIdx = i;
                            }

                            // Count matches
                            matches++;
                        }
                    }
                }

                if (rowId <= 0)
                {
                    // Quit if we were trying to update with an invalid id
                    if (row.IpBlockId > 0)
                    {
                        // We were looking for a specific row, and did not find it
                        L.err(location, "Row id (" + row.IpBlockId + ") does not exist in data.");
                        return retVal; //Early Exit
                    }

                    // Data does not exist, add

                    // The row is added without an ID, then updated
                    DataMgr.IpBlocks.Add(row);
                    int idx = DataMgr.IpBlocks.Count - 1;

                    // Compare data at index
                    if (row.IpBlockId != DataMgr.IpBlocks[idx].IpBlockId)
                    {
                        L.err(location, "Failed to lookup inserted record with id (" + row.IpBlockId + ") at index (" + idx + ")");
                    }
                    else if (row.BlockAddress != DataMgr.IpBlocks[idx].BlockAddress)
                    {
                        L.err(location, "Inserted data does not match memory at index (" + idx + ").");
                    }
                    else
                    {
                        // Update data at index with insert id
                        DataMgr.IpBlocks[idx].IpBlockId = idx + 1;
                        row.IpBlockId = DataMgr.IpBlocks[idx].IpBlockId;
                        retVal = row.IpBlockId;
                    }

                }
                else
                {
                    // Update exising row
                    if (matches > 1)
                    {
                        L.err(location, "Found more than one row matching id (" + row.IpBlockId + ").");
                    }
                    else if (rowId <= 0)
                    {
                        L.err(location, "Failed to locate row for id (" + row.IpBlockId + ").");
                    }
                    else if (rowIdx < 0)
                    {
                        L.err(location, "Failed to initialize index at id (" + row.IpBlockId + ").");
                    }
                    else if (rowIdx >= DataMgr.IpBlocks.Count)
                    {
                        L.err(location, "Record index (" + rowIdx + ") out of bounds (" + (DataMgr.IpBlocks.Count - 1) + ").");
                    }
                    // Reevaluate data store
                    else if (row.IpBlockId != DataMgr.IpBlocks[rowIdx].IpBlockId)
                    {
                        // Something happened, and data no longer matches found
                        L.err(location, "Data changed during update of id (" + row.IpBlockId + ").");
                    }
                    else
                    {
                        // Update data from input
                        DataMgr.IpBlocks[rowIdx] = row;
                        retVal = row.IpBlockId;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static long updateIpBlockIpCnt(IpBlock row)
        {
            const string location = CLASSNAME + ".updateIpBlockIpCnt";
            long retVal = 0L;
            try
            {
                // Check Input
                if (row == null)
                {
                    L.err(location, "Input parameters were null.");
                    return retVal; //Early Exit
                }

                // Check memory data
                if (DataMgr.IpBlocks == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // Check parameters
                for (int i = 0; i < DataMgr.IpBlocks.Count; i++)
                {
                    if (
                        // TODO - Synchronize IpBlocks.Active to use int for consistency
                        DataMgr.IpBlocks[i].Active == true &&
                        row.IpBlockId == DataMgr.FWRows[i].IpBlockId
                    )
                    {
                        // TODO - Hardcoded for now.. this is a deactivate only function
                        DataMgr.IpBlocks[i].CntIps = row.CntIps;
                        retVal++;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static long updateLoginEvents(FailedLoginEvent row)
        {
            const string location = CLASSNAME + ".updateLoginEvents";
            long retVal = 0L;
            try
            {
                // Check Input
                if (row == null)
                {
                    L.err(location, "Input parameters were null.");
                    return retVal; //Early Exit
                }

                // Check memory data
                if (DataMgr.FailedLoginEvents == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // First, see if object exists
                long rowId = 0;
                int rowIdx = 0;
                int matches = 0;
                if (row.FailedLoginEventId > 0)
                {
                    for (int i = 0; i < DataMgr.FailedLoginEvents.Count; i++)
                    {
                        if (
                            //row.Active == 1 &&
                            row.FailedLoginEventId == DataMgr.FailedLoginEvents[i].FailedLoginEventId
                        )
                        {
                            // Take first
                            if (rowId == 0)
                            {
                                rowId = DataMgr.FailedLoginEvents[i].FailedLoginEventId;
                                rowIdx = i;
                            }

                            // Count matches
                            matches++;
                        }
                    }
                }

                if (rowId <= 0)
                {
                    // Quit if we were trying to update with an invalid id
                    if (row.FailedLoginEventId > 0)
                    {
                        // We were looking for a specific row, and did not find it
                        L.err(location, "Row id (" + row.FailedLoginEventId + ") does not exist in data.");
                        return retVal; //Early Exit
                    }

                    // Data does not exist, add

                    // The row is added without an ID, then updated
                    DataMgr.FailedLoginEvents.Add(row);
                    int idx = DataMgr.FailedLoginEvents.Count - 1;

                    // Compare data at index
                    if (row.FailedLoginEventId != DataMgr.FailedLoginEvents[idx].FailedLoginEventId)
                    {
                        L.err(location, "Failed to lookup inserted record with id (" + row.FailedLoginEventId + ") at index (" + idx + ")");
                    }
                    // TODO - for now assume everything has an address
                    else if (row.IpAddress != DataMgr.FailedLoginEvents[idx].IpAddress)
                    {
                        L.err(location, "Inserted data does not match memory at index (" + idx + ").");
                    }
                    else
                    {
                        // Update data at index with insert id
                        DataMgr.FailedLoginEvents[idx].FailedLoginEventId = idx + 1;
                        row.FailedLoginEventId = DataMgr.FailedLoginEvents[idx].FailedLoginEventId;
                        retVal = row.FailedLoginEventId;
                    }

                }
                else
                {
                    // Update exising row
                    if (matches > 1)
                    {
                        L.err(location, "Found more than one row matching id (" + row.FailedLoginEventId + ").");
                    }
                    else if (rowId <= 0)
                    {
                        L.err(location, "Failed to locate row for id (" + row.FailedLoginEventId + ").");
                    }
                    else if (rowIdx < 0)
                    {
                        L.err(location, "Failed to initialize index at id (" + row.FailedLoginEventId + ").");
                    }
                    else if (rowIdx >= DataMgr.FailedLoginEvents.Count)
                    {
                        L.err(location, "Record index (" + rowIdx + ") out of bounds (" + (DataMgr.FailedLoginEvents.Count - 1) + ").");
                    }
                    // Reevaluate data store
                    else if (row.FailedLoginEventId != DataMgr.FailedLoginEvents[rowIdx].FailedLoginEventId)
                    {
                        // Something happened, and data no longer matches found
                        L.err(location, "Data changed during update of id (" + row.FailedLoginEventId + ").");
                    }
                    else
                    {
                        // Update data from input
                        DataMgr.FailedLoginEvents[rowIdx] = row;
                        retVal = row.FailedLoginEventId;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static long updateSummary(Summary row)
        {
            const string location = CLASSNAME + ".updateSummary";
            long retVal = 0L;
            try
            {
                // Check Input
                if (row == null)
                {
                    L.err(location, "Input parameters were null.");
                    return retVal; //Early Exit
                }

                // Check memory data
                if (DataMgr.Summaries == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // First, see if object exists
                long rowId = 0;
                int rowIdx = 0;
                int matches = 0;
                if (row.SummaryId > 0)
                {
                    for (int i = 0; i < DataMgr.Summaries.Count; i++)
                    {
                        if (
                            //row.Active == 1 &&
                            row.SummaryId == DataMgr.Summaries[i].SummaryId
                        )
                        {
                            // Take first
                            if (rowId == 0)
                            {
                                rowId = DataMgr.Summaries[i].SummaryId;
                                rowIdx = i;
                            }

                            // Count matches
                            matches++;
                        }
                    }
                }

                if (rowId <= 0)
                {
                    // Quit if we were trying to update with an invalid id
                    if (row.SummaryId > 0)
                    {
                        // We were looking for a specific row, and did not find it
                        L.err(location, "Row id (" + row.SummaryId + ") does not exist in data.");
                        return retVal; //Early Exit
                    }

                    // Data does not exist, add

                    // The row is added without an ID, then updated
                    DataMgr.Summaries.Add(row);
                    int idx = DataMgr.Summaries.Count - 1;

                    // Compare data at index
                    if (row.SummaryId != DataMgr.Summaries[idx].SummaryId)
                    {
                        L.err(location, "Failed to lookup inserted record with id (" + row.SummaryId + ") at index (" + idx + ")");
                    }
                    else if (row.AppStartDT != DataMgr.Summaries[idx].AppStartDT)
                    {
                        L.err(location, "Inserted data does not match memory at index (" + idx + ").");
                    }
                    else
                    {
                        // Update data at index with insert id
                        DataMgr.Summaries[idx].SummaryId = idx + 1;
                        row.SummaryId = DataMgr.Summaries[idx].SummaryId;
                        retVal = row.SummaryId;
                    }

                }
                else
                {
                    // Update exising row
                    if (matches > 1)
                    {
                        L.err(location, "Found more than one row matching id (" + row.SummaryId + ").");
                    }
                    else if (rowId <= 0)
                    {
                        L.err(location, "Failed to locate row for id (" + row.SummaryId + ").");
                    }
                    else if (rowIdx < 0)
                    {
                        L.err(location, "Failed to initialize index at id (" + row.SummaryId + ").");
                    }
                    else if (rowIdx >= DataMgr.Summaries.Count)
                    {
                        L.err(location, "Record index (" + rowIdx + ") out of bounds (" + (DataMgr.Summaries.Count - 1) + ").");
                    }
                    // Reevaluate data store
                    else if (row.SummaryId != DataMgr.Summaries[rowIdx].SummaryId)
                    {
                        // Something happened, and data no longer matches found
                        L.err(location, "Data changed during update of id (" + row.SummaryId + ").");
                    }
                    else
                    {
                        // Update data from input
                        DataMgr.Summaries[rowIdx] = row;
                        retVal = row.SummaryId;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static long updateUName(UName row)
        {
            const string location = CLASSNAME + ".updateUName";
            long retVal = 0L;
            try
            {
                // Check Input
                if (row == null)
                {
                    L.err(location, "Input parameters were null.");
                    return retVal; //Early Exit
                }

                // Check memory data
                if (DataMgr.UNames == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // First, see if object exists
                long rowId = 0;
                int rowIdx = 0;
                int matches = 0;
                if (row.UNameId > 0)
                {
                    // Try by Id first
                    for (int i = 0; i < DataMgr.UNames.Count; i++)
                    {
                        if (
                            //row.Active == 1 &&
                            row.UNameId == DataMgr.UNames[i].UNameId
                        )
                        {
                            // Take first
                            if (rowId == 0)
                            {
                                rowId = DataMgr.UNames[i].UNameId;
                                rowIdx = i;
                            }

                            // Count matches
                            matches++;
                        }
                    }
                }
                else
                {
                    // Try by child data
                    if (row.UserName != null && row.UserName.Length > 0 && row.IpId > 0 && row.IpBlockId > 0)
                    {
                        for (int i = 0; i < DataMgr.UNames.Count; i++)
                        {
                            if (
                                DataMgr.UNames[i].Active == 1 &&
                                row.IpId == DataMgr.UNames[i].IpId &&
                                row.IpBlockId == DataMgr.UNames[i].IpBlockId &&
                                row.UserName == DataMgr.UNames[i].UserName
                            )
                            {
                                rowId = DataMgr.UNames[i].UNameId;
                            }
                        }
                    }
                }

                if (rowId <= 0)
                {
                    // Quit if we were trying to update with an invalid id
                    if (row.UNameId > 0)
                    {
                        // We were looking for a specific row, and did not find it
                        L.err(location, "Row id (" + row.UNameId + ") does not exist in data.");
                        return retVal; //Early Exit
                    }

                    // Data does not exist, add

                    // The row is added without an ID, then updated
                    DataMgr.UNames.Add(row);
                    int idx = DataMgr.UNames.Count - 1;

                    // Compare data at index
                    if (row.UNameId != DataMgr.UNames[idx].UNameId)
                    {
                        L.err(location, "Failed to lookup inserted record with id (" + row.UNameId + ") at index (" + idx + ")");
                    }
                    else if (
                        row.UserName != DataMgr.UNames[idx].UserName ||
                        row.IpId != DataMgr.UNames[idx].IpId
                    )
                    {
                        L.err(location, "Inserted data does not match memory at index (" + idx + ").");
                    }
                    else
                    {
                        // Update data at index with insert id
                        DataMgr.UNames[idx].UNameId = idx + 1;
                        row.UNameId = DataMgr.UNames[idx].UNameId;
                        retVal = row.UNameId;
                    }

                }
                else
                {
                    // Update exising row
                    if (matches > 1)
                    {
                        L.err(location, "Found more than one row matching id (" + row.UNameId + ").");
                    }
                    else if (rowId <= 0)
                    {
                        L.err(location, "Failed to locate row for id (" + row.UNameId + ").");
                    }
                    else if (rowIdx < 0)
                    {
                        L.err(location, "Failed to initialize index at id (" + row.UNameId + ").");
                    }
                    else if (rowIdx >= DataMgr.UNames.Count)
                    {
                        L.err(location, "Record index (" + rowIdx + ") out of bounds (" + (DataMgr.UNames.Count - 1) + ").");
                    }
                    // Reevaluate data store
                    else if (row.UNameId != DataMgr.UNames[rowIdx].UNameId)
                    {
                        // Something happened, and data no longer matches found
                        L.err(location, "Data changed during update of id (" + row.UNameId + ").");
                    }
                    else
                    {
                        // Update data from input
                        DataMgr.UNames[rowIdx] = row;
                        retVal = row.UNameId;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static long updateXrfSum(XRFSum row)
        {
            const string location = CLASSNAME + ".updateXrfSum";
            long retVal = 0L;
            try
            {
                // Check Input
                if (row == null)
                {
                    L.err(location, "Input parameters were null.");
                    return retVal; //Early Exit
                }

                // Check memory data
                if (DataMgr.XRFSums == null)
                {
                    // TODO - Remove this log, will spam on error
                    L.err(location, "Storage data was not initialized at lookup.");
                    return retVal; //Early Exit
                }

                // First, see if object exists
                long rowId = 0;
                int rowIdx = 0;
                int matches = 0;
                if (row.XRFSumId > 0)
                {
                    // Try by Id first
                    for (int i = 0; i < DataMgr.XRFSums.Count; i++)
                    {
                        if (
                            //row.Active == 1 &&
                            row.XRFSumId == DataMgr.XRFSums[i].XRFSumId
                        )
                        {
                            // Take first
                            if (rowId == 0)
                            {
                                rowId = DataMgr.XRFSums[i].XRFSumId;
                                rowIdx = i;
                            }

                            // Count matches
                            matches++;
                        }
                    }
                }

                if (rowId <= 0)
                {
                    // Quit if we were trying to update with an invalid id
                    if (row.XRFSumId > 0)
                    {
                        // We were looking for a specific row, and did not find it
                        L.err(location, "Row id (" + row.XRFSumId + ") does not exist in data.");
                        return retVal; //Early Exit
                    }

                    // Data does not exist, add

                    // The row is added without an ID, then updated
                    DataMgr.XRFSums.Add(row);
                    int idx = DataMgr.XRFSums.Count - 1;

                    // Compare data at index
                    if (row.XRFSumId != DataMgr.XRFSums[idx].XRFSumId)
                    {
                        L.err(location, "Failed to lookup inserted record with id (" + row.XRFSumId + ") at index (" + idx + ")");
                    }
                    else if (
                        row.IpId != DataMgr.XRFSums[idx].IpId
                    )
                    {
                        L.err(location, "Inserted data does not match memory at index (" + idx + ").");
                    }
                    else
                    {
                        // Update data at index with insert id
                        DataMgr.XRFSums[idx].XRFSumId = idx + 1;
                        row.XRFSumId = DataMgr.XRFSums[idx].XRFSumId;
                        retVal = row.XRFSumId;
                    }

                }
                else
                {
                    // Update exising row
                    if (matches > 1)
                    {
                        L.err(location, "Found more than one row matching id (" + row.XRFSumId + ").");
                    }
                    else if (rowId <= 0)
                    {
                        L.err(location, "Failed to locate row for id (" + row.XRFSumId + ").");
                    }
                    else if (rowIdx < 0)
                    {
                        L.err(location, "Failed to initialize index at id (" + row.XRFSumId + ").");
                    }
                    else if (rowIdx >= DataMgr.XRFSums.Count)
                    {
                        L.err(location, "Record index (" + rowIdx + ") out of bounds (" + (DataMgr.XRFSums.Count - 1) + ").");
                    }
                    // Reevaluate data store
                    else if (row.XRFSumId != DataMgr.XRFSums[rowIdx].XRFSumId)
                    {
                        // Something happened, and data no longer matches found
                        L.err(location, "Data changed during update of id (" + row.XRFSumId + ").");
                    }
                    else
                    {
                        // Update data from input
                        DataMgr.XRFSums[rowIdx] = row;
                        retVal = row.XRFSumId;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }










    }
}
