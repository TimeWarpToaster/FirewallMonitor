using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Xml;
using NetFwTypeLib;

namespace FWM_Client_02
{
    class Program
    {
        // TODO - Revise APPNAME and usage, split variable, look at other ways to eliminate mutex
        const string APPNAME = "FWM_Client_02";
        private static Mutex mutex = null;

        const string CLASSNAME = "Program";


        static bool isAppAdmin = false;
        static List<string> filePaths = new List<string>();
        static List<string> approvedIps = new List<string>();
        static DateTime dtLastRead = DateTime.Parse("2020-01-01 00:00:00.000");

        static Summary summary { get; set; }

        static string[] anyProtocol = new string[] { "TCP", "UDP" };

        static double elapsedApplication = -1;
        static double elapsedLoadAllData = -1;
        static double elapsedSaveAllData1 = -1;
        static double elapsedSaveAllData2 = -1;
        static double elapsedReadEvents = -1;
        static double elapsedGroupNewEvents = -1;
        static double elapsedSelectQualifying = -1;
        static double elapsedGroupQualifyingEvents = -1;
        static double elapsedUpdateCounts = -1;
        static double elapsedFWAdd = -1;
        static double elapsedFWExpire = -1;
        static double elapsedPublishSummary = -1;


        // Tests if an instance of app is already running by mutex
        private static bool AlreadyRunning()
        {
            const string location = "AlreadyRunning";
            bool retValue = true;// Do not allow a new instance, if we cannot determine if app is already running
            try
            {
                // Mutex name is app-name and instance

                // TODO - Prevent mutex theft by writing to encrypted file, and read at boot..
                bool createdNew;
                mutex = new Mutex(true, "Global\\" + APPNAME + "_" + U.GetSetting("InstanceName", ""), out createdNew);

                if (!createdNew)
                {
                    //Console.WriteLine(APPNAME + " is already running! Exiting the application.");
                }
                retValue = !createdNew;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }

        private static bool isAdminUser()
        {
            bool retValue = false;
            try
            {
                retValue = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception ex)
            {
                L.ex(CLASSNAME + ".isAdminUser", ex);
            }
            return retValue;
        }


        static void Main(string[] args)
        {
            const string location = CLASSNAME + ".Main";
            try
            {
                // TODO - Rework main to not use so many returns

                if (!DataMgr.loadAppSettings())
                {
                    L.err(location, "Failed to load application settings.");

                    // TODO - Decide on whether to halt all activity
                    // TODO - Decide what to do about LogPath and logging init
                    return;
                }
                if (!L.logInit(U.GetSetting("logPathClient", ""), U.GetSetting("EnableFileLogging", true)))
                {
                    L.err(location, "Failed to init file logging.");
                }
                L.l(location, "App Start - " + APPNAME + "...");

                DateTime dtApplicationStart = DateTime.Now;

                if (!U.GetSetting("AllowMultiInstance", false))
                {
                    if (AlreadyRunning())
                    {
                        // Multiple instance disallowed, abort launch
                        L.d(location, "Aborting launch due to multi-instance.");
                        return;
                    }
                }
                isAppAdmin = isAdminUser();
                L.l(location, "Running as admin: " + isAppAdmin);
                if (!isAppAdmin)
                {
                    L.l(location, "Administrative privelidge required.");
                    return;
                }


                L.l(location, "Initializing application.");
                if (!appInit())
                {
                    L.err(location, "Failed to initialize application.");
                }

                summary = new Summary();
                summary.CreateDateTime = DateTime.Now;
                summary.AppStartDT = DateTime.Now;

                c.maxToProcess = U.GetSetting("MaxToProcess", 50000);
                c.debug = U.GetSetting("DebugMode", false);

                L.l(location, "Finished app initialization.");

                // Only process if approved ips were added
                if (!addApprovedIps())
                {
                    L.err(location, "Failed to update approved IPs from configuration!");
                }
                else
                {
                    // Handles filtering and push to storage
                    List<IpBlock> ipBlocks = findFailedLogins();

                    if (!manageFirewall())
                    {
                        L.err(location, "Failed to manage firewall rules.");
                    }


                    DateTime dtStartUpdateCounts = DateTime.Now;
                    if (!updateIpAndBlockCounts())
                    {
                        L.err(location, "Failed to update IP and Block counts.");
                    }
                    elapsedUpdateCounts = (DateTime.Now - dtStartUpdateCounts).TotalMilliseconds;

                    // Output all stats to a report
                    DateTime dtStartSummary = DateTime.Now;
                    if (U.GetSetting("EnableAutomaticReport", true))
                    {
                        if (!createReport3(ref ipBlocks))
                        {
                            L.err(location, "Failed to create report!");
                            return;// Early Exit
                        }
                    }
                    elapsedPublishSummary = (DateTime.Now - dtStartSummary).TotalMilliseconds;

                    // Persist all data to storage
                    DateTime dtStartSave2 = DateTime.Now;
                    if (!DataMgr.saveAllData())
                    {
                        L.err(location, "An error occurred while saving data to storage.");
                    }
                    elapsedSaveAllData2 = (DateTime.Now - dtStartSave2).TotalMilliseconds;
                }

                elapsedApplication = (DateTime.Now - dtApplicationStart).TotalMilliseconds;

                L.l(location, "Elapsed Times :: " +
                    "Load Data (" + elapsedLoadAllData + "), " +
                    "Read Events (" + elapsedReadEvents + "), " +
                    "Save 1 (" + elapsedSaveAllData1 + "), " +
                    "Select Qualifying (" + elapsedSelectQualifying + "), " +
                    "Update Counts (" + elapsedUpdateCounts + "), " +
                    "FW Add (" + elapsedFWAdd + "), " + 
                    "FW Expire (" + elapsedFWExpire + "), " +
                    "Save 2 (" + elapsedSaveAllData2 + "), " +
                    "Publish Summary (" + elapsedPublishSummary + "), " + 
                    "Application (" + elapsedApplication + ")."
                );

                // Keep console open if not running in headless mode
                if (!U.GetSetting("isHeadless", false))
                {
                    L.l(location, "Application Finished.");
                    L.l(location, "Press any key to exit.");
                    Console.ReadKey();
                }

                // Quit application with success
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        private static bool appInit()
        {
            const string location = CLASSNAME + ".appInit";
            bool retVal = false;
            try
            {
                // Load Data
                int cntCriticalErrors = 0;

                // Load application ids
                long tempAppId = U.GetSetting("appId", 0L);
                string tempAppGuid = U.GetSetting("appGuid", "");

                if (!FileMgr.readAppData())
                {
                    //L.err(location, "Failed to read application data from file.");
                }

                // TODO - Decide if write is needed, maybe on failure
                if (FileMgr.writeAppData() <= 0)
                {
                    L.err(location, "Failed to save application data.");
                }

                // Load data from storage
                DateTime dtLoadDataStart = DateTime.Now;
                if (!DataMgr.loadAllData(true))
                {
                    L.err(location, "Failed to load some data from storage.");
                }
                elapsedLoadAllData = (DateTime.Now - dtLoadDataStart).TotalMilliseconds;

                // Flag result
                retVal = cntCriticalErrors == 0;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }


        private static bool addApprovedIps()
        {
            const string location = CLASSNAME + ".addApprovedIps";
            bool retValue = false;
            try
            {
                string delimitedIps = U.GetSetting("ApprovedIps", "");
                string[] ips = delimitedIps.Split(',');
                foreach (string s in ips) approvedIps.Add(s.Trim());
                retValue = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }



        private static bool manageFirewall()
        {
            const string location = CLASSNAME + ".manageFirewall";
            bool retVal = false;
            try
            {
                bool isManageFW = U.GetSetting("IsManageFW", false);
                L.l(location, "Firewall Management is: " + (isManageFW ? "On" : "Off"));
                if (!isManageFW) return true;

                // Get events over a date range
                DateTime dtStartSelectQualifying = DateTime.Now;

                int fwMinutesToReview = U.GetSetting("FWMinutesToReview", 10080);
                DateTime dtEnd = DateTime.Now;
                DateTime dtStart = dtEnd.AddMinutes(-1 * fwMinutesToReview);

                List<FailedLoginEvent> events = getFailedLoginEvents(dtStart, dtEnd);
                elapsedSelectQualifying = (DateTime.Now - dtStartSelectQualifying).TotalMilliseconds;

                DateTime dtStartGroupQualifying = DateTime.Now;

                // The difference between groupByIp and groupByIpForUi, is groupByIp merges back into 
                // original data and counts, groupByIpForUi takes an independent copy of data, and 
                // performs counts on only that data. Use ..ForUi to get firewall qualifications.
                List<IpBlock> ipBlocks = groupByIpForUi(events);
                elapsedGroupQualifyingEvents = (DateTime.Now - dtStartGroupQualifying).TotalMilliseconds;

                if (ipBlocks == null || ipBlocks.Count == 0)
                {
                    L.err(location, "Failed to identify failed logins!");
                    return retVal; //Early Exit
                }
                L.l(location, "Found (" + ipBlocks.Count + ") ip blocks for firewall consideration.");

                DateTime dtFWStart = DateTime.Now;

                // Wait upto 30-seconds for a file-lock 
                if (!FileMgr.lockFWRows(60))
                {
                    L.err(location, "Failed to acquire lock on firewall file.");
                    return retVal; // TODO - Decide on throwing error, this can happen
                }

                // Add rules
                DateTime dtStartFWAdd = DateTime.Now;
                if (!addFirewallRules(U.GetSetting("MinFailuresToBlock", 100), ipBlocks))//min failures before blocking
                {
                    L.err(location, "Failed to add firewall rules!");
                }
                elapsedFWAdd = (DateTime.Now - dtStartFWAdd).TotalMilliseconds;

                // Expire rules
                DateTime dtStartFWExpire = DateTime.Now;
                int expiredRules = expireFirewallRules();
                if (!FileMgr.unlockFWRows())
                {
                    L.err(location, "Failed to unlock firewall rows.");
                }
                elapsedFWExpire = (DateTime.Now - dtStartFWExpire).TotalMilliseconds;


                L.l(location, "Expired (" + expiredRules + ") FW rules.");
                summary.ElapsedFW = (float)(DateTime.Now - dtFWStart).TotalMilliseconds;

                // Flag success for completing
                retVal = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
                try
                {
                    if (!FileMgr.unlockFWRows()) L.err(location, "Failed to unlock file after exception.");
                }
                catch (Exception ex2) { /* Do Nothing */ }
            }
            return retVal;
        }

        private static List<IpBlock> findFailedLogins()
        {
            const string location = CLASSNAME + ".findFailedLogins";
            List<IpBlock> retVal = new List<IpBlock>();
            try
            {
                string folderEventPath = U.GetSetting("EventFolder", "");
                if (string.IsNullOrEmpty(folderEventPath))
                {
                    L.err(location, "Path to events folder is not supplied!");
                    return retVal;
                }
                filePaths = new List<string> { @folderEventPath + "Security.evtx" };

                // Read event logs, find failed logins at same time
                DateTime dtStartFLRead = DateTime.Now;
                List<FailedLoginEvent> failedLoginEvents = readEventLogs();
                if (failedLoginEvents.Count == 0)
                {
                    L.l(location, "No new events found.");
                    return retVal;// Early Exit
                }
                elapsedReadEvents = (DateTime.Now - dtStartFLRead).TotalMilliseconds;
                L.l(location, "Built (" + failedLoginEvents.Count + ") FailedLoginEvent objects in (" + elapsedReadEvents + ") ms.");

                // Ensure rows exist in memory for Block, IP, and Event
                if (!initObjects(failedLoginEvents))
                {
                    L.err(location, "Failed to initialize new failed login objects in storage.");
                }

                // Group events by ip and block
                DateTime dtStartGroupEvents = DateTime.Now;
                List<IpBlock> foundBlocks = groupByIp(failedLoginEvents);
                elapsedGroupNewEvents = (DateTime.Now - dtStartGroupEvents).TotalMilliseconds;

                // Save records
                DateTime dtStartSaveData1 = DateTime.Now;
                if (!DataMgr.saveAllData())
                {
                    L.err(location, "Failed to save new events to file.");
                }
                elapsedSaveAllData1 = (DateTime.Now - dtStartSaveData1).TotalMilliseconds;


                L.l(location, "Finished processing event logs.");
                retVal = foundBlocks;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        private static List<FailedLoginEvent> readEventLogs()
        {
            const string location = CLASSNAME + ".readEventLog";
            List<FailedLoginEvent> retVal = new List<FailedLoginEvent>();
            try
            {
                DateTime dtReadStart = DateTime.Now;
                int cntFilesRead = 0;
                foreach (string s in filePaths)
                {
                    // Get time of last execution. Ignore entries prior to last execution.
                    dtLastRead = DataMgr.getLastEventLogReadTime(true);
                    List<FailedLoginEvent> eventsInFile = readEventLog(s, dtLastRead);

                    if (eventsInFile == null)
                    {
                        L.err(location, "Failed to read event log (" + s + ")!");
                    }
                    else if (eventsInFile.Count == 0)
                    {
                        L.l(location, "Events list was empty.");
                        cntFilesRead++;
                    }
                    else
                    {
                        cntFilesRead++;
                    }
                    retVal.AddRange(eventsInFile);
                }
                L.l(location, "Finished reading (" + cntFilesRead + ") of (" + filePaths.Count + ") files.");

                // Record elapsed time
                summary.ElapsedRead = (float)(DateTime.Now - dtReadStart).TotalMilliseconds;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        private static List<FailedLoginEvent> readEventLog(string filePath, DateTime dtLastRead)
        {
            const string location = CLASSNAME + ".readEventLog";
            List<FailedLoginEvent> retVal = null;
            try
            {
                L.l(location, "Filtering logs newer than: " + dtLastRead.ToString(TAG.DTF));

                // First move file to a work folder outside of the system directory
                string archivePath = U.GetSetting("ArchiveFolder", "");
                string tempPath = filePath;
                if (!string.IsNullOrEmpty(archivePath))
                {
                    tempPath = @archivePath + @"\temp\Security.evtx";
                    if (!U.moveFile(@filePath, @tempPath))
                    {
                        L.err(location, "Failed to move file to work folder. Aborting scan...");
                        return retVal;// do not proceed on system copy when in archive mode
                    }
                }

                // Read event log file into memory, separate login failures
                int cnt = 0;
                int cntPreviousReads = 0;
                int cntUnrelated = 0;
                int cntAll = 0;
                bool hasReadFile = false;
                DateTime fileReadTime = DateTime.Now;
                using (var reader = new EventLogReader(@tempPath, PathType.FilePath))
                {
                    try
                    {
                        // Initialize output to not null, now that we know the file is reading
                        retVal = new List<FailedLoginEvent>();

                        EventRecord record;
                        while ((record = reader.ReadEvent()) != null && cnt < c.maxToProcess)
                        {
                            cntAll++;
                            using (record)
                            {
                                try
                                {
                                    if (record.TimeCreated == null) continue;
                                    if (record.TimeCreated < dtLastRead) continue;
                                    if (record.Keywords == null) continue;
                                    string keywords = Convert.ToString(record.Keywords);
                                    if (keywords != "0x8010000000000000" && keywords != "-9218868437227405312")
                                    {
                                        continue;
                                    }



                                    FailedLoginEvent fle = new FailedLoginEvent();
                                    if (!fle.fromEventRecord(record) || fle.CreateDateTime == null || fle.CreateDateTime.Year < 2000)
                                    {
                                        cntUnrelated++;
                                        continue;
                                    }
                                    else if (fle.CreateDateTime <= dtLastRead)
                                    {
                                        cntPreviousReads++;
                                        continue;
                                    }
                                    else if (approvedIps != null && approvedIps.IndexOf(fle.IpAddress) >= 0)
                                    {
                                        // Skip over any approved ips
                                        continue;
                                    }
                                    else if (fle.isFailedLogin)
                                    {
                                        summary.CntFaccess++;
                                        retVal.Add(fle);
                                    }

                                    cnt++;
                                }
                                catch (Exception exUsing)
                                {
                                    L.err(location, "Failed to update failed login event: " + exUsing.Message);
                                }
                            }
                        }
                        hasReadFile = true;

                        // Debug stub
                        if (U.isSystemPauses)
                        {
                            L.l(location, "Waiting input.");
                            Console.ReadKey();
                            L.l(location, "Proceeding.");
                        }
                    }
                    catch (Exception exUsing)
                    {
                        L.err(location, "Failed to update failed login events: " + exUsing.Message);
                    }
                }

                // Only if the file was accessed and read, save the last read date to memory and storage
                if (hasReadFile)
                {
                    U.LastReadDate = fileReadTime;
                    U.sLastReadDate = U.LastReadDate.ToString(TAG.DTF);
                    L.l(location, "Writing last read date (" + U.sLastReadDate + ").");
                    int cntRows = FileMgr.writeAppData();
                    if (cntRows <= 0)
                    {
                        L.err(location, "Failed to write last read time to storage.");
                    }
                }

                // Push file to archive
                if (!string.IsNullOrEmpty(archivePath))
                {
                    if (!U.moveFile(@tempPath, @archivePath + "Security_" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".evtx"))
                    {
                        L.err(location, "Failed to move work file to archive folder...");
                    }
                }

                L.l(location, "Finished adding events. Added (" + cnt + "), Reruns (" + cntPreviousReads + "), Unrelated Events (" + cntUnrelated + "), Read (" + cntAll + "), File (" + filePath + ").");
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }


        /*
         * initObjects - Creates event records in memory. Creates Ip and IpBlock records
         * in memory as needed.
         */
        private static bool initObjects(List<FailedLoginEvent> events)
        {
            const string location = CLASSNAME + ".initObjects";
            bool retVal = false;
            try
            {
                if (events == null)
                {
                    L.err(location, "Input events were null.");
                    return retVal; //Early Exit
                }
                L.l(location, "Searching (" + events.Count + ") events.");

                for (int idxEvent = 0; idxEvent < events.Count; idxEvent++)
                {
                    string ipAddress = events[idxEvent].IpAddress;
                    if (ipAddress == null || ipAddress.Length == 0) continue; // Skip events without an ip
                    if (ipAddress.Trim() == "-") continue; // Skip events with a dashed ip

                    string blockAddress = c.getBlockAddress(ipAddress);
                    if (blockAddress == null || blockAddress.Length == 0)
                    {
                        L.err(location, "Failed to indentify a block address.");
                        continue;
                    }

                    // See if block exists
                    IpBlock block = DataMgr.getIpBlockByBlockAddress(blockAddress, 1);
                    if (block == null || block.IpBlockId <= 0)
                    {
                        // Create a block now
                        block = new IpBlock();
                        block.CreateDateTime = DateTime.Now;
                        block.Active = true;
                        block.BlockAddress = blockAddress;
                        block.StartTime = events[idxEvent].CreateDateTime;
                        block.EndTime = block.StartTime;
                        block.IpBlockId = DataMgr.updateIpBlock(block);
                    }
                    // LastTime makes more sense than EndTime. When did it end? You don't know, until its over a while.
                    if (block.EndTime < events[idxEvent].CreateDateTime)
                    {
                        block.EndTime = events[idxEvent].CreateDateTime;
                    }
                    if (block.LastTime < events[idxEvent].CreateDateTime)
                    {
                        block.LastTime = events[idxEvent].CreateDateTime;
                    }

                    // See if ip exists
                    IpEvent ip = DataMgr.getIpByIpAddress(ipAddress, 1);
                    if (ip == null || ip.IpEventId <= 0)
                    {
                        // Create an ip now
                        ip.CreateDateTime = DateTime.Now;
                        ip.Active = true;
                        ip.IpBlockId = block.IpBlockId;
                        ip.BlockAddress = blockAddress;
                        ip.IpAddress = events[idxEvent].IpAddress;
                        ip.StartTime = events[idxEvent].CreateDateTime;
                        ip.EndTime = events[idxEvent].CreateDateTime;

                        // Create a summary cross-reference for Ip and Ip Block, on an Ip basis
                        XRFSum xrfSum = new XRFSum();
                        xrfSum.Active = 1;
                        xrfSum.CreateDateTime = DateTime.Now;
                        xrfSum.IpBlockId = block.IpBlockId;
                        xrfSum.IpId = ip.IpEventId;
                        xrfSum.SummaryId = summary.SummaryId;
                        xrfSum.XRFSumId = DataMgr.updateXrfSum(xrfSum);
                    }
                    if (ip.StartTime > events[idxEvent].CreateDateTime)
                    {
                        ip.StartTime = events[idxEvent].CreateDateTime;
                    }
                    if (ip.EndTime < events[idxEvent].CreateDateTime)
                    {
                        ip.EndTime = events[idxEvent].CreateDateTime;
                    }
                    ip.IpEventId = DataMgr.updateIp(ip);

                    // Identify failed login with ip
                    events[idxEvent].IpEventId = ip.IpEventId;
                    events[idxEvent].IpBlockId = block.IpBlockId;

                    // Save event to storage
                    events[idxEvent].FailedLoginEventId = DataMgr.updateFailedLoginEvent(events[idxEvent]);

                }

                // Flag success for completing.. there is some garbage in the data being skipped, making counts difficult.
                retVal = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        private static List<FailedLoginEvent> getFailedLoginEvents(DateTime dtStart, DateTime dtEnd)
        {
            const string location = CLASSNAME + ".getFailedLoginEvents";
            List<FailedLoginEvent> retVal = new List<FailedLoginEvent>();
            try
            {
                if (dtStart == null)
                {
                    L.err(location, "Start date not supplied.");
                    return retVal; //Early Exit
                }
                if (dtEnd == null)
                {
                    L.err(location, "End date not supplied.");
                    return retVal; //Early Exit
                }

                if (dtEnd < dtStart)
                {
                    DateTime temp = dtStart;
                    dtStart = dtEnd;
                    dtEnd = temp;
                }

                if (DataMgr.FailedLoginEvents == null)
                {
                    L.err(location, "Storage was invalid at lookup.");
                    return retVal; //Early Exit
                }

                List<FailedLoginEvent> rows = new List<FailedLoginEvent>();
                for (int i = 0; i < DataMgr.FailedLoginEvents.Count; i++)
                {
                    if (
                        DataMgr.FailedLoginEvents[i].CreateDateTime != null &&
                        DataMgr.FailedLoginEvents[i].CreateDateTime > dtStart &&
                        DataMgr.FailedLoginEvents[i].CreateDateTime <= dtEnd &&
                        DataMgr.FailedLoginEvents[i].IpAddress != null &&
                        DataMgr.FailedLoginEvents[i].IpAddress.Trim() != "-"
                        )
                    {
                        rows.Add(DataMgr.FailedLoginEvents[i]);
                    }
                }

                // Output Result
                retVal = rows;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        private static List<IpBlock> groupByIp(List<FailedLoginEvent> events)
        {
            const string location = CLASSNAME + ".groupByIp";
            List<IpBlock> retVal = new List<IpBlock>();
            try
            {
                if (events == null)
                {
                    L.err(location, "Input events were null.");
                    return retVal; //Early Exit
                }

                // At this point, it should be certain that IPs and Blocks exist in memory, go get them only.

                List<IpBlock> blocks = new List<IpBlock>();
                int cntBadIpId = 0;
                int cntBadIpBlockId = 0;
                for (int idxEvent = 0; idxEvent < events.Count; idxEvent++)
                {
                    // Find the block
                    if (events[idxEvent].IpBlockId <= 0)
                    {
                        //L.err(location, "Unknown block."); 
                        cntBadIpBlockId++;
                    }

                    int idxBlock = -1;
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if (blocks[i].IpBlockId == events[idxEvent].IpBlockId)
                        {
                            idxBlock = i;
                            break;
                        }
                    }

                    if (idxBlock < 0)
                    {
                        // Block does not exist in working data, go get from memory
                        string blockAddress = c.getBlockAddress(events[idxEvent].IpAddress);

                        IpBlock block = DataMgr.getIpBlockByBlockAddress(blockAddress, 1);
                        if (block == null)
                        {
                            //L.err(location, "Failed to locate block in memory.");
                            continue; //Loop
                        }
                        
                        blocks.Add(block);
                        for (int i = blocks.Count - 1; i >= 0; i--)
                        {
                            if (block.IpBlockId == blocks[i].IpBlockId)
                            {
                                idxBlock = i;
                                break;
                            }
                        }
                        if (idxBlock < 0)
                        {
                            L.err(location, "Failed to identify block address (" + blockAddress + ").");
                            continue;
                        }
                    }

                    // Make sure block has a list for IPs
                    if (blocks[idxBlock].IpEvents == null)
                    {
                        blocks[idxBlock].IpEvents = new List<IpEvent>();
                    }

                    // Find IP on block
                    int idxIp = -1;
                    for (int i = 0; i < blocks[idxBlock].IpEvents.Count; i++)
                    {
                        if (blocks[idxBlock].IpEvents[i].IpEventId == events[idxEvent].IpEventId)
                        {
                            idxIp = i;
                            break;
                        }
                    }

                    if (idxIp < 0)
                    {
                        // IP does not exist in working data, go get from memory
                        IpEvent ip = DataMgr.getIpByIpAddress(events[idxEvent].IpAddress, 1);
                        if (ip == null)
                        {
                            //L.err(location, "Failed to location IP in memory.");
                            continue;
                        }

                        // Add IP to Block
                        blocks[idxBlock].IpEvents.Add(ip);
                        for (int idxTemp = blocks[idxBlock].IpEvents.Count - 1; idxTemp >= 0; idxTemp--)
                        {
                            if (blocks[idxBlock].IpEvents[idxTemp].IpEventId == events[idxEvent].IpEventId)
                            {
                                idxIp = idxTemp;
                                break;
                            }
                        }
                        if (idxIp < 0)
                        {
                            L.err(location, "Failed to pull ip data.");
                            continue;
                        }
                    }


                    // Add current event to working data
                    if (blocks[idxBlock].IpEvents[idxIp].FailedLogins == null)
                    {
                        blocks[idxBlock].IpEvents[idxIp].FailedLogins = new List<FailedLoginEvent>();
                    }
                    blocks[idxBlock].IpEvents[idxIp].FailedLogins.Add(events[idxEvent]);


                    // See if username exists
                    if (blocks[idxBlock].UserNames == null)
                    {
                        blocks[idxBlock].UserNames = new Dictionary<string, int>();
                    }

                    string uname = events[idxEvent].TargetUserName;
                    if (uname != null && uname.Length > 0)
                    {
                        if (blocks[idxBlock].UserNames.ContainsKey(uname))
                        {
                            blocks[idxBlock].UserNames[uname]++;
                        }
                        else
                        {
                            blocks[idxBlock].UserNames.Add(uname, 1);
                        }
                    }
                }

                // Output Result
                retVal = blocks;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }



        private static List<IpBlock> groupByIpForUi(List<FailedLoginEvent> events)
        {
            // Counts are self-contained, according to the list. IP and IP Block attributes
            // reflect the list, and not storage. Counts and dates are from the list.
            const string location = CLASSNAME + ".groupByIpForUi";
            List<IpBlock> retVal = new List<IpBlock>();
            try
            {
                if (events == null)
                {
                    L.err(location, "Input events were null.");
                    return retVal; //Early Exit
                }

                List<IpBlock> blocks = new List<IpBlock>();
                int cntBadIpId = 0;
                int cntBadIpBlockId = 0;
                for (int idxEvent = 0; idxEvent < events.Count; idxEvent++)
                {
                    // Find the block
                    if (events[idxEvent].IpBlockId <= 0)
                    {
                        //L.err(location, "Unknown block."); 
                        cntBadIpBlockId++;
                        continue;
                    }

                    int idxBlock = -1;
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if (blocks[i].IpBlockId == events[idxEvent].IpBlockId)
                        {
                            idxBlock = i;
                            break;
                        }
                    }

                    if (idxBlock < 0)
                    {
                        // Block does not exist in working data, go get from memory
                        string blockAddress = c.getBlockAddress(events[idxEvent].IpAddress);

                        IpBlock record = DataMgr.getIpBlockByBlockAddress(blockAddress, 1);
                        if (record == null)
                        {
                            //L.err(location, "Failed to locate block in memory.");
                            continue; //Loop
                        }

                        // Create an empty ip block, and migrate non derived values
                        IpBlock block = new IpBlock();
                        block.IpBlockId = record.IpBlockId;
                        block.Active = record.Active;
                        block.CreateDateTime = record.CreateDateTime;
                        block.BlockAddress = record.BlockAddress;
                        block.LastTime = record.LastTime;// this stays the same
                        block.IpEvents = new List<IpEvent>();

                        blocks.Add(block);
                        for (int i = blocks.Count - 1; i >= 0; i--)
                        {
                            if (block.IpBlockId == blocks[i].IpBlockId)
                            {
                                idxBlock = i;
                                break;
                            }
                        }
                        if (idxBlock < 0)
                        {
                            L.err(location, "Failed to identify block address (" + blockAddress + ").");
                            continue;
                        }
                    }

                    // Find IP on block
                    int idxIp = -1;
                    for (int i = 0; i < blocks[idxBlock].IpEvents.Count; i++)
                    {
                        if (blocks[idxBlock].IpEvents[i].IpEventId == events[idxEvent].IpEventId)
                        {
                            idxIp = i;
                            break;
                        }
                    }

                    if (idxIp < 0)
                    {
                        // IP does not exist in working data, go get from memory
                        IpEvent record = DataMgr.getIpByIpAddress(events[idxEvent].IpAddress, 1);
                        if (record == null || record.IpEventId <= 0)
                        {
                            //L.err(location, "Failed to locate IP in memory.");
                            continue;
                        }

                        IpEvent ip = new IpEvent();
                        ip.IpEventId = record.IpEventId;
                        ip.IpBlockId = record.IpBlockId;
                        ip.Status = record.Status;
                        ip.IpId = record.IpId;
                        ip.Active = record.Active;
                        ip.CreateDateTime = record.CreateDateTime;
                        ip.IpAddress = record.IpAddress;
                        ip.BlockAddress = record.BlockAddress;

                        // Add IP to Block
                        blocks[idxBlock].IpEvents.Add(ip);

                        for (int i = blocks[idxBlock].IpEvents.Count - 1; i >= 0; i--)
                        {
                            if (record.IpEventId == blocks[idxBlock].IpEvents[i].IpEventId)
                            {
                                idxIp = i;
                                break;
                            }
                        }
                        if (idxIp < 0)
                        {
                            L.err(location, "Failed to identify ip index on block.");
                            continue;
                        }
                    }

                    // Add current event to working data
                    if (blocks[idxBlock].IpEvents[idxIp].FailedLogins == null)
                    {
                        blocks[idxBlock].IpEvents[idxIp].FailedLogins = new List<FailedLoginEvent>();
                    }
                    blocks[idxBlock].IpEvents[idxIp].FailedLogins.Add(events[idxEvent]);

                    //L.l(location, "Events on ip (" + blocks[idxBlock].IpEvents[idxIp].FailedLogins.Count + ").");

                    // Ensure username lists exist
                    if (blocks[idxBlock].UserNames == null)
                    {
                        blocks[idxBlock].UserNames = new Dictionary<string, int>();
                    }
                    if (blocks[idxBlock].IpEvents[idxIp].UserNames == null)
                    {
                        blocks[idxBlock].IpEvents[idxIp].UserNames = new Dictionary<string, int>();
                    }

                    // See if username exists
                    string uname = events[idxEvent].TargetUserName;
                    if (uname != null && uname.Length > 0)
                    {
                        if (blocks[idxBlock].UserNames.ContainsKey(uname))
                        {
                            blocks[idxBlock].UserNames[uname]++;
                        }
                        else
                        {
                            blocks[idxBlock].UserNames.Add(uname, 1);
                        }

                        if (blocks[idxBlock].IpEvents[idxIp].UserNames.ContainsKey(uname))
                        {
                            blocks[idxBlock].IpEvents[idxIp].UserNames[uname]++;
                        }
                        else
                        {
                            blocks[idxBlock].IpEvents[idxIp].UserNames.Add(uname, 1);
                        }
                    }
                }

                // Update overall counts
                for (int idxBlock = 0; idxBlock < blocks.Count; idxBlock++)
                {
                    if (blocks[idxBlock].IpEvents == null) continue;

                    blocks[idxBlock].CntIps = blocks[idxBlock].IpEvents.Count;
                    List<IpEvent> ips = blocks[idxBlock].IpEvents;

                    for (int idxIp = 0; idxIp < ips.Count; idxIp++)
                    {
                        if (ips[idxIp].FailedLogins == null) continue;

                        // Set failed login counts
                        ips[idxIp].CntFailedLogins = ips[idxIp].FailedLogins.Count;

                        blocks[idxBlock].CntFailedLogins += ips[idxIp].CntFailedLogins;

                        ips[idxIp].PercentOfTotal = (ips[idxIp].CntFailedLogins / events.Count) * 100;

                        List<FailedLoginEvent> fles = ips[idxIp].FailedLogins;
                        for (int idxEvent = 0; idxEvent < fles.Count; idxEvent++)
                        {
                            if (
                                blocks[idxBlock].StartTime == null ||
                                blocks[idxBlock].StartTime == c.nDt ||
                                blocks[idxBlock].StartTime > fles[idxEvent].CreateDateTime
                            )
                            {
                                blocks[idxBlock].StartTime = fles[idxEvent].CreateDateTime;
                            }

                            if (
                                blocks[idxBlock].EndTime == null ||
                                blocks[idxBlock].EndTime == c.nDt ||
                                blocks[idxBlock].EndTime < fles[idxEvent].CreateDateTime
                            )
                            {
                                blocks[idxBlock].EndTime = fles[idxEvent].CreateDateTime;
                            }

                            if (
                                ips[idxIp].StartTime == null ||
                                ips[idxIp].StartTime == c.nDt ||
                                ips[idxIp].StartTime > fles[idxEvent].CreateDateTime
                            )
                            {
                                ips[idxIp].StartTime = fles[idxEvent].CreateDateTime;
                            }
                            if (
                                ips[idxIp].EndTime == null ||
                                ips[idxIp].EndTime == c.nDt ||
                                ips[idxIp].EndTime < fles[idxEvent].CreateDateTime
                            )
                            {
                                //L.l(location, "Setting ip end date.");
                                ips[idxIp].EndTime = fles[idxEvent].CreateDateTime;
                            }
                        }

                        // Get a count of usernames attempted for IP
                        if (ips[idxIp].UserNames != null)
                        {
                            ips[idxIp].UserNamesAttempted = ips[idxIp].UserNames.Count;
                        }

                        // Get Ip average latency
                        if (ips[idxIp].StartTime != null && ips[idxIp].EndTime != null && ips[idxIp].CntFailedLogins > 0)
                        {
                            ips[idxIp].Elapsed = (ips[idxIp].EndTime - ips[idxIp].StartTime).TotalMilliseconds;
                            ips[idxIp].AverageLatency = ips[idxIp].Elapsed / ips[idxIp].CntFailedLogins;
                        }
                    }

                    // Get IpBlock average latency
                    if (blocks[idxBlock].StartTime != null && blocks[idxBlock].EndTime != null && blocks[idxBlock].CntFailedLogins > 0)
                    {
                        blocks[idxBlock].Elapsed = (blocks[idxBlock].EndTime - blocks[idxBlock].StartTime).TotalMilliseconds;
                        blocks[idxBlock].AverageLatency = blocks[idxBlock].Elapsed / blocks[idxBlock].CntFailedLogins;
                    }
                }


                // Output Result
                retVal = blocks;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }


        private static bool updateIpAndBlockCounts()
        {
            const string location = CLASSNAME + ".updateIpAndBlockCounts";
            bool retVal = false;
            try
            {
                int cntErrors = 0;
                if (DataMgr.FailedLoginEvents == null)
                {
                    cntErrors++;
                    L.err(location, "Events were null at count.");
                }
                if (DataMgr.IpEvents == null)
                {
                    cntErrors++;
                    L.err(location, "Ips were null at count.");
                }
                if (DataMgr.IpBlocks == null)
                {
                    cntErrors++;
                    L.err(location, "Ip blocks were null at count.");
                }

                // Iterate Blocks
                List<UName> unames = new List<UName>();
                for (int idxBlock = 0; idxBlock < DataMgr.IpBlocks.Count; idxBlock++)
                {
                    long ipBlockId = DataMgr.IpBlocks[idxBlock].IpBlockId;

                    int cntBlockFLE = 0;

                    // Counts are keyed by the IpEventId
                    Dictionary<long, int> ipIdCounts = new Dictionary<long, int>();
                    Dictionary<long, Dictionary<string, int>> uNameCounts = new Dictionary<long, Dictionary<string, int>>();


                    // Generate Counts
                    for (int idxEvent = 0; idxEvent < DataMgr.FailedLoginEvents.Count; idxEvent++)
                    {
                        if (ipBlockId == DataMgr.FailedLoginEvents[idxEvent].IpBlockId)
                        {
                            cntBlockFLE++;
                            long key = DataMgr.FailedLoginEvents[idxEvent].IpEventId;
                            if (!ipIdCounts.ContainsKey(key))
                            {
                                ipIdCounts.Add(key, 1);
                            }
                            else 
                            {
                                ipIdCounts[key]++;
                            }
                            if (!uNameCounts.ContainsKey(key))
                            {
                                uNameCounts.Add(key, new Dictionary<string, int>());
                            }
                            string uName = DataMgr.FailedLoginEvents[idxEvent].TargetUserName;
                            if (!uNameCounts[key].ContainsKey(uName))
                            {
                                uNameCounts[key].Add(uName, 1);
                            }
                            else
                            {
                                uNameCounts[key][uName]++;
                            }
                        }
                    }

                    // Update counts on Block
                    DataMgr.IpBlocks[idxBlock].CntFailedLogins = cntBlockFLE;
                    DataMgr.IpBlocks[idxBlock].CntIps = ipIdCounts.Count;

                    // Update counts on Ips associated with block
                    foreach (KeyValuePair<long, int> pair in ipIdCounts)
                    {
                        long ipEventId = pair.Key;
                        for (int idxIp = 0; idxIp < DataMgr.IpEvents.Count; idxIp++)
                        {
                            if (ipEventId == DataMgr.IpEvents[idxIp].IpEventId)
                            {
                                DataMgr.IpEvents[idxIp].CntFailedLogins = pair.Value;
                                break;
                            }
                        }
                    }

                    // Get a list of UNames
                    foreach (KeyValuePair<long, Dictionary<string, int>> pair in uNameCounts)
                    {
                        long ipEventId = pair.Key;

                        int cntIpUNames = 0;
                        foreach (KeyValuePair<string, int> cntUName in pair.Value)
                        {
                            int idxUName = DataMgr.getUNameIndex(ipEventId, cntUName.Key);
                            if (idxUName < 0)
                            {
                                UName uname = new UName();
                                uname.Active = 1;
                                uname.CreateDateTime = DateTime.Now;
                                uname.IpBlockId = ipBlockId;
                                uname.IpId = ipEventId;
                                uname.UserName = cntUName.Key;
                                uname.UNameId = DataMgr.updateUName(uname);
                                idxUName = DataMgr.getUNameIndex(ipEventId, cntUName.Key);// May be slow, faster manually backwards
                            }

                            if (idxUName >= DataMgr.UNames.Count)
                            {
                                L.err(location, "Invalid index returned.");
                                continue;
                            }
                            DataMgr.UNames[idxUName].Cnt = cntUName.Value;

                            // TODO - Test eliminating next stub
                            if (ipBlockId != DataMgr.UNames[idxUName].IpBlockId)
                            {
                                DataMgr.UNames[idxUName].IpBlockId = ipBlockId; // This shouldn't happen
                            }

                            unames.Add(DataMgr.UNames[idxUName]);
                            cntIpUNames++;
                        }

                        // Put uname count on ip
                        for (int idxIp = 0; idxIp < DataMgr.IpEvents.Count; idxIp++)
                        {
                            if (ipEventId == DataMgr.IpEvents[idxIp].IpEventId)
                            {
                                DataMgr.IpEvents[idxIp].UserNamesAttempted = cntIpUNames;
                                break;
                            }
                        }
                    }
                }

                // Flush all data to file. Consider timing here.
                if (!DataMgr.saveAllData())
                {
                    L.err(location, "Failed to save to file after loading counts.");
                }
                else
                {
                    // Flag success for completing
                    retVal = true;
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }



        private static bool sortBlocks(ref List<IpBlock> ipBlocks)
        {
            const string location = CLASSNAME + ".sortBlocks";
            bool retVal = false;
            try
            {
                DateTime dtFilterStart = DateTime.Now;// TODO - Eliminate this variable

                if (ipBlocks == null)
                {
                    L.err(location, "Input was null.");
                    return retVal; //Early Exit
                }

                ipBlocks.Sort((pair1, pair2) => pair1.CntFailedLogins.CompareTo(pair2.CntFailedLogins));
                ipBlocks.Reverse();

                summary.CntIpBlocks = ipBlocks.Count;
                summary.ElapsedFilter += (float)(DateTime.Now - dtFilterStart).TotalMilliseconds;

                // Flag success for completing
                retVal = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        private static bool createReport3(ref List<IpBlock> ipBlocks)
        {
            const string location = CLASSNAME + ".createReport3";
            bool retValue = false;
            try
            {
                DateTime dtReportStart = DateTime.Now;
                L.l(location, "Beginning to create report...");

                string reportPath = U.GetSetting("ReportPath", "");
                if (reportPath == null || reportPath.Length == 0)
                {
                    L.l(location, "Skipping report due to empty path.");
                    return true;// Flag success if we didn't need to
                }

                // TODO - Make file extension configurable
                string fullPath = reportPath + U.GetSetting("ReportFilePrefix", "") + DateTime.Now.ToString("yyyyMMdd_HHmmss.fff") + ".txt";

                if (!sortBlocks(ref ipBlocks))
                {
                    L.err(location, "Failed to sort blocks before reporting!");
                }

                // Setup format strings
                const string nl = "\n";
                const string nl2 = nl + nl;
                const string nl3 = nl2 + nl;
                const string tab = "    ";
                const string tab2 = tab + tab;
                const string tab3 = tab2 + tab;


                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Failed Logins:")
                .AppendLine(tab + summary.CntFaccess)
                .AppendLine("")

                .AppendLine("Ip Blocks:")
                .AppendLine(tab + ipBlocks.Count)
                .AppendLine("")
                .AppendLine(" -- ")
                .AppendLine("");

                for (int idxBlock = 0; idxBlock < ipBlocks.Count; idxBlock++)
                {
                    IpBlock block = ipBlocks[idxBlock];
                    sb.AppendLine(block.BlockAddress)
                    //.AppendLine(tab + "Failed Logins: " + block.Value.CntFailedLogins)
                    .AppendLine(tab + "Start Time: " + block.StartTime)
                    .AppendLine(tab + "End Time: " + block.EndTime)
                    //.AppendLine(tab + "Elapsed: " + block.Elapsed)
                    //.AppendLine(tab + "User Names: " + block.UserNames.Count)
                    //.AppendLine(tab + "Avg Latency: " + block.AverageLatency)
                    .AppendLine(tab + "Ip Count: " + block.CntIps);

                    foreach (KeyValuePair<string, int> ips in block.Ips)
                    {
                        sb.AppendLine(tab2 + ips.Key + " - " + (ips.Value) + " failures");
                    }
                    sb.AppendLine("");

                    for (int idxIp = 0; idxIp < block.IpEvents.Count; idxIp++)
                    {
                        IpEvent ip = block.IpEvents[idxIp];
                        sb.AppendLine(tab + ip.IpAddress)
                        .AppendLine(tab2 + "Failed Logins: " + (ip.FailedLogins.Count + 1))
                        .AppendLine(tab2 + "Start Time: " + ip.StartTime.ToString())
                        .AppendLine(tab2 + "End Time: " + ip.EndTime.ToString())
                        //.AppendLine(tab2 + "Elapsed: " + ip.Elapsed)
                        //.AppendLine(tab2 + "Least Latency: " + ip.Value.LowestLatency)
                        //.AppendLine(tab2 + "Avg Latency: " + ip.AverageLatency)
                        //.AppendLine(tab2 + "User Names: " + ip.UserNames.Count);

                        /*foreach (KeyValuePair<string, int> nameCnt in ip.UserNames)
                        {
                            sb.AppendLine(tab3 + nameCnt.Key + ": " + nameCnt.Value);
                        }*/
                        .AppendLine("");
                    }
                    sb.AppendLine("");
                    sb.AppendLine("");
                }

                // Stop report timer now
                summary.ElapsedReport = (float)(DateTime.Now - dtReportStart).TotalMilliseconds;
                summary.AppEndDT = DateTime.Now;

                // Add summary timers to report before outputting
                sb.AppendLine("")
                .AppendLine("")
                .AppendLine("")
                .AppendLine(" -- Summary Report -- ")
                .AppendLine("")
                .AppendLine("App Start:        " + summary.AppStartDT.ToString(TAG.DTF))
                .AppendLine("App End:          " + summary.AppEndDT.ToString(TAG.DTF))
                .AppendLine("")
                .AppendLine("Login Failures:   " + summary.CntFaccess)
                .AppendLine("Count IP Blocks:  " + summary.CntIpBlocks)
                .AppendLine("Count IPs:        " + summary.CntIps)
                .AppendLine("")
                .AppendLine(tab + "Read Elapsed:    " + summary.ElapsedRead)
                .AppendLine(tab + "Sort Elapsed:    " + summary.ElapsedFilter)
                .AppendLine(tab + "FW Elapsed:      " + summary.ElapsedFW)
                .AppendLine(tab + "Report Elapsed:  " + summary.ElapsedReport)
                .AppendLine(" -------------------- ")

                .AppendLine("")
                .AppendLine("")
                .AppendLine(" ----- FW Report ---- ")
                .AppendLine("")
                .AppendLine("Count New Rules:  " + summary.CntFWAdd)
                .AppendLine("Count Add Fail:   " + summary.CntFWAddFail)
                .AppendLine("Count Existed:    " + summary.CntFWExisted)
                .AppendLine("Count Processed:  " + summary.CntFWProcessed)
                .AppendLine("")
                .AppendLine("FW IPs Added:     " + summary.FWAddIps)
                .AppendLine("")
                .AppendLine("FW IP Add Errors: " + summary.FWIpAddFailures)
                .AppendLine("")
                .AppendLine("Pre-Existing IPs: " + summary.FWExisted)
                .AppendLine("")
                .AppendLine("New FW Rules:     [ " + summary.FWNewRules + " ]")

                .AppendLine(" -------------------- ");

                // Write report to log file
                FileMgr.writeText(fullPath, sb);

                // Write summary to memory
                summary.Rpt = sb.ToString();
                long updateId = DataMgr.updateSummary(summary);
                if (updateId <= 0)
                {
                    L.err(location, "Failed to update summary in memory.");
                }
                else
                {
                    // Push summaries to storage
                    int cntSummariesWritten = FileMgr.writeSummary(DataMgr.Summaries);
                    if (cntSummariesWritten != DataMgr.Summaries.Count)
                    {
                        L.err(location, "Error writing summaries. Written (" + cntSummariesWritten +
                            "), expected (" + DataMgr.Summaries.Count + ").");
                    }
                }

                // Flag success for completing
                retValue = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }

        private static bool createReport2(ref List<IpBlock> ipBlocks)
        {
            const string location = CLASSNAME + ".createReport2";
            bool retValue = false;
            try
            {
                DateTime dtReportStart = DateTime.Now;
                L.l(location, "Beginning to create report...");

                string reportPath = U.GetSetting("ReportPath", "");
                if (reportPath == null || reportPath.Length == 0)
                {
                    L.l(location, "Skipping report due to empty path.");
                    return true;// Flag success if we didn't need to
                }

                // TODO - Make file extension configurable
                string fullPath = reportPath + U.GetSetting("ReportFilePrefix", "") + DateTime.Now.ToString("yyyyMMdd_HHmmss.fff") + ".txt";

                if (!sortBlocks(ref ipBlocks))
                {
                    L.err(location, "Failed to sort blocks before reporting!");
                }

                // Setup format strings
                const string nl = "\n";
                const string nl2 = nl + nl;
                const string nl3 = nl2 + nl;
                const string tab = "    ";
                const string tab2 = tab + tab;
                const string tab3 = tab2 + tab;


                List<string> rpt = new List<string>();
                rpt.Add("Failed Logins:");
                rpt.Add(tab + summary.CntFaccess);
                rpt.Add("");

                rpt.Add("Ip Blocks:");
                rpt.Add(tab + ipBlocks.Count);
                rpt.Add("");
                rpt.Add(" -- ");
                rpt.Add("");

                for (int idxBlock = 0; idxBlock < ipBlocks.Count; idxBlock++)
                {
                    IpBlock block = ipBlocks[idxBlock];
                    rpt.Add(block.BlockAddress);
                    //rpt.Add(tab + "Failed Logins: " + block.Value.CntFailedLogins
                    rpt.Add(tab + "Start Time: " + block.StartTime);
                    rpt.Add(tab + "End Time: " + block.EndTime);
                    //rpt.Add(tab + "Elapsed: " + block.Elapsed);
                    //rpt.Add(tab + "User Names: " + block.UserNames.Count);
                    //rpt.Add(tab + "Avg Latency: " + block.AverageLatency);
                    rpt.Add(tab + "Ip Count: " + block.CntIps);

                    foreach (KeyValuePair<string, int> ips in block.Ips)
                    {
                        rpt.Add(tab2 + ips.Key + " - " + (ips.Value) + " failures");
                    }
                    rpt.Add("");

                    for (int idxIp = 0; idxIp < block.IpEvents.Count; idxIp++)
                    {
                        IpEvent ip = block.IpEvents[idxIp];
                        rpt.Add(tab + ip.IpAddress);
                        rpt.Add(tab2 + "Failed Logins: " + (ip.FailedLogins.Count + 1));
                        rpt.Add(tab2 + "Start Time: " + ip.StartTime.ToString());
                        rpt.Add(tab2 + "End Time: " + ip.EndTime.ToString());
                        //rpt.Add(tab2 + "Elapsed: " + ip.Elapsed);
                        //rpt.Add(tab2 + "Least Latency: " + ip.Value.LowestLatency);
                        //rpt.Add(tab2 + "Avg Latency: " + ip.AverageLatency);
                        //rpt.Add(tab2 + "User Names: " + ip.UserNames.Count);

                        /*foreach (KeyValuePair<string, int> nameCnt in ip.UserNames)
                        {
                            rpt.Add(tab3 + nameCnt.Key + ": " + nameCnt.Value);
                        }*/
                        rpt.Add("");
                    }
                    rpt.Add("");
                    rpt.Add("");
                }

                // Stop report timer now
                summary.ElapsedReport = (float)(DateTime.Now - dtReportStart).TotalMilliseconds;
                summary.AppEndDT = DateTime.Now;

                // Add summary timers to report before outputting
                rpt.Add("");
                rpt.Add("");
                rpt.Add("");
                rpt.Add(" -- Summary Report -- ");
                rpt.Add("");
                rpt.Add("App Start:        " + summary.AppStartDT.ToString(TAG.DTF));
                rpt.Add("App End:          " + summary.AppEndDT.ToString(TAG.DTF));
                rpt.Add("");
                rpt.Add("Login Failures:   " + summary.CntFaccess);
                rpt.Add("Count IP Blocks:  " + summary.CntIpBlocks);
                rpt.Add("Count IPs:        " + summary.CntIps);
                rpt.Add("");
                rpt.Add(tab + "Read Elapsed:    " + summary.ElapsedRead);
                rpt.Add(tab + "Sort Elapsed:    " + summary.ElapsedFilter);
                rpt.Add(tab + "FW Elapsed:      " + summary.ElapsedFW);
                rpt.Add(tab + "Report Elapsed:  " + summary.ElapsedReport);
                rpt.Add(" -------------------- ");

                rpt.Add("");
                rpt.Add("");
                rpt.Add(" ----- FW Report ---- ");
                rpt.Add("");
                rpt.Add("Count New Rules:  " + summary.CntFWAdd);
                rpt.Add("Count Add Fail:   " + summary.CntFWAddFail);
                rpt.Add("Count Existed:    " + summary.CntFWExisted);
                rpt.Add("Count Processed:  " + summary.CntFWProcessed);
                rpt.Add("");
                rpt.Add("FW IPs Added:     " + summary.FWAddIps);
                rpt.Add("");
                rpt.Add("FW IP Add Errors: " + summary.FWIpAddFailures);
                rpt.Add("");
                rpt.Add("Pre-Existing IPs: " + summary.FWExisted);
                rpt.Add("");
                rpt.Add("New FW Rules:     [ " + summary.FWNewRules + " ]");

                rpt.Add(" -------------------- ");

                // Write report to log file
                //L.logWriter(rpt);
                //FileMgr.writeText(fullPath, rpt);

                // Write report to summary and console
                StringBuilder sbRpt = new StringBuilder();
                for (int i = 0; i < rpt.Count; i++)
                {
                    sbRpt.AppendLine(rpt[i]);
                    //Console.WriteLine(rpt[i]);
                }
                FileMgr.writeText(fullPath, sbRpt);

                // Write summary to memory
                summary.Rpt = sbRpt.ToString();
                long updateId = DataMgr.updateSummary(summary);
                if (updateId <= 0)
                {
                    L.err(location, "Failed to update summary in memory.");
                }
                else
                {
                    // Push summaries to storage
                    int cntSummariesWritten = FileMgr.writeSummary(DataMgr.Summaries);
                    if (cntSummariesWritten != DataMgr.Summaries.Count)
                    {
                        L.err(location, "Error writing summaries. Written (" + cntSummariesWritten +
                            "), expected (" + DataMgr.Summaries.Count + ").");
                    }
                }

                // Flag success for completing
                retValue = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }

        public static bool pushFailedLoginsToFile(List<IpBlock> ipBlocks)
        {
            const string location = CLASSNAME + ".pushFailedLoginsToFile";
            bool retVal = false;
            try
            {
                L.l(location, "Beginning push to file for failed logins.");

                // TODO - Summary increment lines should move to caller
                summary.CntIpBlocks = ipBlocks.Count;
                for (int idxBlock = 0; idxBlock < ipBlocks.Count; idxBlock++)
                {
                    summary.CntIps += ipBlocks[idxBlock].IpEvents.Count;
                }

                // Push all relevent data to memory first
                for (int idxBlock = 0; idxBlock < ipBlocks.Count; idxBlock++)
                {
                    IpBlock block = ipBlocks[idxBlock];

                    for (int idxIp = 0; idxIp < block.IpEvents.Count; idxIp++)
                    {
                        IpEvent ip = block.IpEvents[idxIp];

                        // Push FailedLoginEvents for Ip
                        for (int idxEvent = 0; idxEvent < ip.FailedLogins.Count; idxEvent++)
                        {
                            // Ensure IpId is attached to FailedLoginEvent
                            DataMgr.updateFailedLoginEvent(ip.FailedLogins[idxEvent]);
                        }

                        // Push UNames for Ip
                        foreach (KeyValuePair<string, int> pair in ip.UserNames)
                        {
                            UName uname = new UName();
                            uname.UNameId = 0L;
                            uname.Active = 1;
                            uname.CreateDateTime = DateTime.Now;
                            uname.UserName = pair.Key;
                            uname.IpBlockId = block.IpBlockId;
                            uname.IpId = ip.IpId;
                            uname.Cnt = pair.Value;

                            DataMgr.updateUName(uname);
                        }

                        // Push XRFSum for Ip
                        XRFSum xrfSum = new XRFSum();
                        xrfSum.XRFSumId = 0L;
                        xrfSum.SummaryId = summary.SummaryId;
                        xrfSum.IpId = ip.IpId;
                        xrfSum.IpBlockId = block.IpBlockId;
                        xrfSum.Active = 1;
                        xrfSum.CreateDateTime = DateTime.Now;

                        DataMgr.updateXrfSum(xrfSum);


                        // Finally, Push Ip
                        DataMgr.updateIp(ip);
                    }

                    DataMgr.updateIpBlock(block);
                }

                // Push all relevent memory to storage
                int cntIpBlocksToFile = FileMgr.writeIpBlock(DataMgr.IpBlocks);
                if (cntIpBlocksToFile != DataMgr.IpBlocks.Count)
                {
                    L.err(location, "Failed to write IpBlocks. Wrote (" + cntIpBlocksToFile + "), expected (" + DataMgr.IpBlocks.Count + ").");
                }

                int cntIpsToFile = FileMgr.writeIpEvent(DataMgr.IpEvents);
                if (cntIpsToFile != DataMgr.IpEvents.Count)
                {
                    L.err(location, "Failed to write Ips. Wrote (" + cntIpsToFile + "), expected (" + DataMgr.IpEvents.Count + ").");
                }

                int cntUNamesToFile = FileMgr.writeUNames(DataMgr.UNames);
                if (cntUNamesToFile != DataMgr.UNames.Count)
                {
                    L.err(location, "Failed to write UNames. Wrote (" + cntUNamesToFile + "), expected (" + DataMgr.UNames.Count + ").");
                }

                int cntFailedLoginsToFile = FileMgr.writeFailedLoginEvents(DataMgr.FailedLoginEvents);
                if (cntFailedLoginsToFile != DataMgr.FailedLoginEvents.Count)
                {
                    L.err(location, "Failed to write failed logins. Wrote (" + cntFailedLoginsToFile +
                        "), expected (" + DataMgr.FailedLoginEvents.Count + ").");
                }

                // Flag success for completing
                L.l(location, "Finished pushing failed logins to DB.");
                retVal = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }


        public static bool addFirewallRules2(int minFailuresToBlock, List<IpBlock> ipBlocks)
        {
            const string location = CLASSNAME + ".addFirewallRules";
            bool retValue = false;
            try
            {
                L.l(location, "Creating firewall rules for IPs with (" + minFailuresToBlock + ") or more failures.");

                int msBetweenTestMin = U.GetSetting("MSBetweenFWTestMin", 250);
                int msBetweenTestMax = U.GetSetting("MSBetweenFWTestMax", 1200);
                int msBetweenAddMin = U.GetSetting("MSBetweenFWAddMin", 2000);
                int msBetweenAddMax = U.GetSetting("MSBetweenFWAddMax", 3800);
                int minutesToReview = U.GetSetting("FWMinutesToReview", 10080);
                DateTime dtScanStart = DateTime.Now.AddMinutes(-1 * minutesToReview);
                DateTime dtScanEnd = DateTime.Now;

                DateTime activeDate = DateTime.Now;
                int expireAfterDays = U.GetSetting("FWExpireAfterDays", 30);
                if (expireAfterDays == 0) expireAfterDays = 25000;// After 25k days, remove rule (pseudo forever)
                DateTime expiry = DateTime.Now.AddDays(expireAfterDays);

                string port = U.GetSetting("FWPort", "Any");
                string protocol = U.GetSetting("FWProtocol", "TCP");

                string prefix = U.GetSetting("FWPrefix", "ELPRule");



                Random r = new Random();
                StringBuilder sb = new StringBuilder();

                // Iterate blocks
                for (int idxIpBlock = 0; idxIpBlock < ipBlocks.Count; idxIpBlock++)
                {
                    IpBlock block = ipBlocks[idxIpBlock];
                    // Populate IpBlockId if missing
                    if (
                        block.IpBlockId <= 0 &&
                        block.BlockAddress != null &&
                        block.BlockAddress.Length > 0
                    )
                    {
                        long temp = DataMgr.getIpBlockIdByBlockAddress(block.BlockAddress, 1);
                        if (temp <= 0)
                        {
                            L.d(location, "Failed to locate block (" + block.BlockAddress + ").");
                            temp = 0;
                        }
                        else
                        {
                            block.IpBlockId = temp;
                        }
                    }

                    // Iterate IPs for block
                    for (int idxIpEvent = 0; idxIpEvent < block.IpEvents.Count; idxIpEvent++)
                    {
                        // TODO - Add a passlist check here for ip

                        IpEvent ip = block.IpEvents[idxIpEvent];
                        if (DataMgr.isFlagged(ip.IpAddress, minFailuresToBlock, dtScanStart, dtScanEnd))
                        {
                            ip.FlaggedThisScan = true;

                            // Populate IpId if missing
                            if (ip.IpId <= 0)
                            {
                                long temp = DataMgr.getIpIdByIpAddress(ip.IpAddress, (ip.Active ? 1 : 0));
                                if (temp > 0)
                                {
                                    ip.IpId = temp;
                                }
                            }

                            // Create an object
                            FWRow fw = new FWRow();
                            fw.CreateDateTime = DateTime.Now;
                            fw.Active = 1;
                            fw.IpBlockId = block.IpBlockId;
                            fw.IpId = ip.IpId;
                            fw.FWName = prefix + ip.IpAddress.Replace(".", "_");
                            fw.ActiveDate = activeDate;
                            fw.Expiry = expiry; // Expiry is pushed out for all new occurrences, existing or not
                            fw.Protocol = protocol;
                            fw.Port = port;
                            fw.IpAddress = ip.IpAddress;


                            // See if FW rule exists in DB before proceeding. Merge ActiveDate, TimesRefreshed. Evaluate Deactivated.

                            List<FWRow> fwRes = DataMgr.getFWByName(fw.FWName, fw.Active);
                            if (fwRes.Count != 1) L.err(location, "Found (" + fwRes.Count + ") rows for rule (" + fw.FWName + ").");
                            if (fwRes.Count > 0)
                            {
                                fw.FWId = fwRes[0].FWId;
                                fw.TimesRefreshed = fwRes[0].TimesRefreshed;
                                if (fwRes[0].ActiveDate != null)
                                {
                                    fw.ActiveDate = fwRes[0].ActiveDate;
                                }
                                if (fwRes[0].Deactivated != null)
                                {
                                    fw.Deactivated = fwRes[0].Deactivated;
                                }
                            }
                            fw.TimesRefreshed++;

                            // TODO - This next section does not technically appear to work, because the check of is-existing looks for any protocol,
                            // not a specific one in the or. 
                            if (!isFirewallRuleExisting(fw.FWName))
                            {
                                if (addFirewallRule(fw))
                                {
                                    //L.logger(location, "Added firewall rule for: IP (" + ip.Value.IpAddress + "), Rule (" + fw.FWName + ").", TAG.IMPORTANT);
                                    if (summary.FWAddIps.Length > 0) summary.FWAddIps.Append(",");
                                    summary.FWAddIps.Append(fw.IpAddress);

                                    if (summary.FWNewRules.Length > 0) summary.FWNewRules.Append(",");
                                    summary.FWNewRules
                                        .Append("{ ")
                                        .Append("IpAddr:\"").Append(fw.IpAddress).Append("\"")
                                        .Append(", Rule:\"").Append(fw.FWName).Append("\"")
                                        .Append(", Port:\"").Append(fw.Port).Append("\"")
                                        .Append(", Protocol:\"").Append(fw.Protocol).Append("\"")
                                        .Append(", Created:\"").Append(fw.ActiveDate.ToString("yyyy-MM-dd HH:mm:ss")).Append("\"")
                                        .Append(", Expiry:\"").Append(fw.Expiry.ToString("yyyy-MM-dd HH:mm:ss")).Append("\"")
                                        .Append(", Login Failures:\"").Append(ip.CntFailedLogins).Append("\"")
                                        .Append(" }");
                                    summary.CntFWAdd++;
                                }
                                else
                                {
                                    L.err(location, "Failed to add firewall rule for (" + fw.IpAddress + ").");
                                    if (summary.FWIpAddFailures.Length > 0) summary.FWIpAddFailures.Append(",");
                                    summary.FWIpAddFailures.Append(fw.IpAddress);
                                    summary.CntFWAddFail++;
                                }
                                // Sleep a random interval between rules
                                Thread.Sleep(r.Next(msBetweenAddMin, msBetweenAddMax));
                            }
                            else
                            {
                                // Rule exists, append log
                                if (summary.FWExisted.Length > 0) summary.FWExisted.Append(",");
                                summary.FWExisted.Append(ip.IpAddress);
                                summary.CntFWExisted++;
                            }

                            // Push FW to memory
                            long updateId = DataMgr.updateFW2(fw);
                            if (updateId <= 0)
                            {
                                L.err(location, "Failed to update id (" + updateId + ") in memory.");
                            }

                            summary.CntFWProcessed++;
                            if (summary.CntFWProcessed % 100 == 0)
                            {
                                L.l(location, "FW MGMT... - Added (" + summary.CntFWAdd + "), Failed (" + summary.CntFWAddFail +
                                    "), Existed (" + summary.CntFWExisted + "), Processed (" + summary.CntFWProcessed + ").");
                            }

                            // Sleep a random interval between checks
                            Thread.Sleep(r.Next(msBetweenTestMin, msBetweenTestMax));
                        }
                    }
                }

                // Write FW changes in memory to file
                int rowsWritten = FileMgr.writeFWRows(DataMgr.FWRows);
                if (rowsWritten != DataMgr.FWRows.Count)
                {
                    L.err(location, "Length mismatch between memory (" + DataMgr.FWRows.Count + ") and written (" + rowsWritten + ").");
                }

                L.l(location, "FW MGMT Complete - Added (" + summary.CntFWAdd + "), Failed (" + summary.CntFWAddFail +
                    "), Existed (" + summary.CntFWExisted + "), Processed (" + summary.CntFWProcessed + ").");


                //L.logger(location, "Finished adding firewall rules.");
                retValue = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }

        public static bool addFirewallRules(int minFailuresToBlock, List<IpBlock> ipBlocks)
        {
            const string location = CLASSNAME + ".addFirewallRules";
            bool retValue = false;
            try
            {
                L.l(location, "Creating firewall rules for IPs with (" + minFailuresToBlock + ") or more failures.");


                int msBetweenTestMin = U.GetSetting("MSBetweenFWTestMin", 250);
                int msBetweenTestMax = U.GetSetting("MSBetweenFWTestMax", 1200);
                int msBetweenAddMin = U.GetSetting("MSBetweenFWAddMin", 2000);
                int msBetweenAddMax = U.GetSetting("MSBetweenFWAddMax", 3800);

                DateTime activeDate = DateTime.Now;

                int expireAfterMinutes = U.GetSetting("FWExpireAfterMinutes", 10080);
                if (expireAfterMinutes <= 0) expireAfterMinutes = 21024000;// 40-years (pseudo forever)
                DateTime expiry = DateTime.Now.AddMinutes(expireAfterMinutes);

                string port = U.GetSetting("FWPort", "Any");
                string protocol = U.GetSetting("FWProtocol", "TCP");

                string prefix = U.GetSetting("FWPrefix", "FWMRule");

                Random r = new Random();
                StringBuilder sb = new StringBuilder();


                // Get latest data from file
                DataMgr.FWRows = FileMgr.readFWRows();
                if (DataMgr.FWRows == null)
                {
                    L.l(location, "Rules from storage were empty.");
                    DataMgr.FWRows = new List<FWRow>();
                }

                // Iterate blocks
                for (int idxIpBlock = 0; idxIpBlock < ipBlocks.Count; idxIpBlock++)
                {
                    IpBlock block = ipBlocks[idxIpBlock];
                    // Populate IpBlockId if missing
                    if (
                        block.IpBlockId <= 0 &&
                        block.BlockAddress != null &&
                        block.BlockAddress.Length > 0
                    )
                    {
                        long temp = DataMgr.getIpBlockIdByBlockAddress(block.BlockAddress, 1);
                        if (temp <= 0)
                        {
                            L.d(location, "Failed to locate block (" + block.BlockAddress + ").");
                            temp = 0;
                        }
                        else
                        {
                            block.IpBlockId = temp;
                        }
                    }


                    // Iterate IPs for block
                    for (int idxIpEvent = 0; idxIpEvent < block.IpEvents.Count; idxIpEvent++)
                    {
                        IpEvent ip = block.IpEvents[idxIpEvent];
                        
                        if (ip.FailedLogins.Count >= minFailuresToBlock)
                        {
                            //L.l(location, "Blocking ip (" + ip.IpAddress + ") with (" + ip.FailedLogins.Count + ") failures.");
                            ip.FlaggedThisScan = true;

                            // Populate IpId if missing
                            if (ip.IpId <= 0)
                            {
                                long temp = DataMgr.getIpIdByIpAddress(ip.IpAddress, (ip.Active ? 1 : 0));
                                if (temp > 0)
                                {
                                    ip.IpId = temp;
                                }
                            }

                            // Create an object
                            FWRow fw = new FWRow();
                            fw.CreateDateTime = DateTime.Now;
                            fw.Active = 1;
                            fw.IpBlockId = block.IpBlockId;
                            fw.IpId = ip.IpId;
                            fw.FWName = prefix + ip.IpAddress.Replace(".", "_");

                            // See if FW rule exists in DB before proceeding. Merge ActiveDate, TimesRefreshed. Evaluate Deactivated.
                            List<FWRow> fwRes = DataMgr.getFWByName(fw.FWName, fw.Active);
                            if (fwRes.Count > 1) L.l(location, "Found (" + fwRes.Count + ") rows for rule (" + fw.FWName + ").");
                            if (fwRes.Count > 0)
                            {
                                fw.FWId = fwRes[0].FWId;
                                if (fwRes[0].CreateDateTime != null && fwRes[0].CreateDateTime > c.nDt)
                                {
                                    fw.CreateDateTime = fwRes[0].CreateDateTime;
                                }
                                fw.TimesRefreshed = fwRes[0].TimesRefreshed;
                                if (fwRes[0].ActiveDate != null && fwRes[0].ActiveDate > c.nDt)
                                {
                                    fw.ActiveDate = fwRes[0].ActiveDate;
                                }
                                if (fwRes[0].Deactivated != null)
                                {
                                    fw.Deactivated = fwRes[0].Deactivated;
                                }

                                // Expiry is only pushed out, non-existent or if Expired=true
                                if (fwRes[0].Expiry != null && fwRes[0].Expiry != c.nDt && !fwRes[0].Expired)
                                {
                                    fw.Expiry = fwRes[0].Expiry;
                                }
                                fw.Expired = fwRes[0].Expired;
                            }

                            if (fw.Expiry == null || fw.Expiry == c.nDt || fw.Expiry == new DateTime())
                            {
                                fw.Expiry = expiry;
                            }
                            if (fw.ActiveDate == null || fw.ActiveDate == c.nDt || fw.ActiveDate == new DateTime())
                            {
                                fw.ActiveDate = activeDate;
                            }
                            fw.Expired = false;
                            fw.Protocol = protocol;
                            fw.Port = port;
                            fw.IpAddress = ip.IpAddress;


                            // Skip known rules that have been manually deactivated
                            if (fw.Deactivated != null && fw.Deactivated > c.nDt)
                            {
                                //L.l(location, "Skipping known rule (" + fw.FWName + ") that was manually deactivated.");
                                continue;
                            }

                            // TODO - This next section does not technically appear to work, because the check of is-existing looks for any protocol,
                            // not a specific one in the or. 
                            if (!isFirewallRuleExisting(fw.FWName))
                            {
                                if (addFirewallRule(fw))
                                {
                                    L.l(location, "Added firewall rule for: IP (" + fw.IpAddress + "), Rule (" + fw.FWName + ").");

                                    fw.TimesRefreshed++;
                                    fw.ActiveDate = activeDate;

                                    if (summary.FWAddIps.Length > 0) summary.FWAddIps.Append(",");
                                    summary.FWAddIps.Append(fw.IpAddress);

                                    if (summary.FWNewRules.Length > 0) summary.FWNewRules.Append(",");
                                    summary.FWNewRules
                                        .Append("{ ")
                                        .Append("IpAddr:\"").Append(fw.IpAddress).Append("\"")
                                        .Append(", Rule:\"").Append(fw.FWName).Append("\"")
                                        .Append(", Port:\"").Append(fw.Port).Append("\"")
                                        .Append(", Protocol:\"").Append(fw.Protocol).Append("\"")
                                        .Append(", Created:\"").Append(fw.ActiveDate.ToString("yyyy-MM-dd HH:mm:ss")).Append("\"")
                                        .Append(", Expiry:\"").Append(fw.Expiry.ToString("yyyy-MM-dd HH:mm:ss")).Append("\"")
                                        .Append(", Login Failures:\"").Append(ip.CntFailedLogins).Append("\"")
                                        .Append(" }");
                                    summary.CntFWAdd++;
                                }
                                else
                                {
                                    L.err(location, "Failed to add firewall rule for (" + fw.IpAddress + ").");
                                    if (summary.FWIpAddFailures.Length > 0) summary.FWIpAddFailures.Append(",");
                                    summary.FWIpAddFailures.Append(fw.IpAddress);
                                    summary.CntFWAddFail++;
                                }
                            }
                            else
                            {
                                // Rule exists, append log
                                if (summary.FWExisted.Length > 0) summary.FWExisted.Append(",");
                                summary.FWExisted.Append(ip.IpAddress);
                                summary.CntFWExisted++;
                            }

                            // Push FW to memory
                            long updateId = DataMgr.updateFW2(fw);
                            if (updateId <= 0)
                            {
                                L.err(location, "Failed to update id (" + updateId + ") in memory.");
                            }
                            summary.CntFWProcessed++;

                            if (summary.CntFWProcessed % 100 == 0)
                            {
                                L.l(location, "FW MGMT... - Added (" + summary.CntFWAdd + "), Failed (" + summary.CntFWAddFail +
                                    "), Existed (" + summary.CntFWExisted + "), Processed (" + summary.CntFWProcessed + ").");
                            }

                        }
                    }
                }

                // Write FW changes in memory to file
                if (summary.CntFWProcessed > 0)
                {
                    int rowsWritten = FileMgr.writeFWRows(DataMgr.FWRows);
                    if (rowsWritten != DataMgr.FWRows.Count)
                    {
                        L.err(location, "Length mismatch between memory (" + DataMgr.FWRows.Count + ") and written (" + rowsWritten + ").");
                    }
                }

                L.l(location, "FW MGMT Complete - Added (" + summary.CntFWAdd + "), Failed (" + summary.CntFWAddFail +
                    "), Existed (" + summary.CntFWExisted + "), Processed (" + summary.CntFWProcessed + ").");

                // Flag success for completing
                retValue = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }

        public static bool addFirewallRule(FWRow fw)
        {
            // ruleName should be unique. Using "MyRule"+ipaddr.Replace(".","_") as rule names for this app.
            // remoteAddress supports the following formats:
            // "1.2.3.4", "1.2.3.*", "1.2.3.0/24", and other formats supported by Windows Firewall rule
            // protocol can be set to "TCP", "UDP", ...
            // port can be set to "Any" or port number "8080"
            const string location = CLASSNAME + ".addFirewallRule";
            bool retValue = false;
            try
            {
                if (!isAppAdmin)
                {
                    L.err(location, "Ignoring request to add firewall rule as non-admin.");
                    return retValue;// Early Exit
                }
                else if (string.IsNullOrEmpty(fw.FWName) && string.IsNullOrEmpty(fw.IpAddress))
                {
                    L.d(location, "Skipping firewall rule with null or empty name/ip.");
                    return retValue;
                }

                bool errors = false;
                string temp = fw.Protocol.Trim().ToLower();
                if (temp.Equals("any") || temp.Equals("*") || temp.Equals("all"))
                {
                    // Block reasonable protocols upon wildcard, currently TCP and UDP
                    foreach (string s in anyProtocol)
                    {
                        string cmd = "/C netsh advfirewall firewall add rule name=\"" + fw.FWName + s.Substring(0, 1) + "\" dir=in action=block remoteip=" + fw.IpAddress + " remoteport=" + fw.Port + " protocol=" + s;
                        if (!execCmd(cmd, true, true))
                        {
                            errors = true;
                            if (c.debug) L.d(location, "Failed to create FW Rule (" + fw.FWName + s.Substring(0, 1) + ").");
                        }
                    }
                }
                else
                {
                    // Block specified protocol
                    string cmd = "/C netsh advfirewall firewall add rule name=\"" + fw.FWName + "\" dir=in action=block remoteip=" + fw.IpAddress + " remoteport=" + fw.Port + " protocol=" + fw.Protocol;
                    if (!execCmd(cmd, true, true))
                    {
                        errors = true;
                        if (c.debug) L.d(location, "Failed to create FW Rule (" + fw.FWName + ").");
                    }
                }

                if (errors)
                {
                    L.err(location, "Errors creating FW Rule set! Rule name (" + fw.FWName + "), Protocol (" + fw.Protocol + ").");
                }
                retValue = !errors;

            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }

        public static bool isFirewallRuleExisting(string ruleName)
        {
            const string location = CLASSNAME + ".isFirewallRuleExisting";
            bool retValue = false;
            try
            {
                string variant1 = ruleName + "T";//TCP rule using all/*/any protocol
                string variant2 = ruleName + "U";//UDP rule using all/*/any protocol

                Type tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                INetFwPolicy2 fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(tNetFwPolicy2);
                foreach (INetFwRule rule in fwPolicy2.Rules)
                {
                    if (rule.Name.IndexOf(ruleName) != -1)
                    {
                        retValue = true;
                    }
                    else if (rule.Name.IndexOf(variant1) != -1)
                    {
                        retValue = true;
                    }
                    else if (rule.Name.IndexOf(variant2) != -1)
                    {
                        retValue = true;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }


        public static int expireFirewallRules()
        {
            const string location = CLASSNAME + ".expireFirewallRules";
            int retValue = 0;// number of rules removed
            try
            {
                // Get all active DB rules with elapsed expirations
                List<FWRow> expiries = DataMgr.getFWExpiries();
                L.l(location, "Expiring (" + expiries.Count + ") FW rules..");

                // Test for admin rights before attempting to update firewall
                isAppAdmin = isAdminUser();// refresh
                if (!isAppAdmin)
                {
                    L.err(location, "Ignoring request to expire firewall rules as non-admin.");
                    return retValue;// Early Exit
                }

                int msBetweenTestMin = U.GetSetting("MSBetweenFWTestMin", 250);
                int msBetweenTestMax = U.GetSetting("MSBetweenFWTestMax", 1200);
                Random r = new Random();

                // Iterate DB rule expiries
                for (int i = 0; i < expiries.Count; i++)
                {
                    FWRow row = expiries[i];
                    if (row == null) continue; //Loop
                    if (row.FWName == null || row.FWName.Length == 0) continue; //Loop
                    if (row.Expiry == null || row.Expiry == c.nDt) continue; //Loop
                    if (row.Expired) continue; // Expireds are already eliminated at query, this is for clarity


                    // Check if rule exists
                    bool isARule = isFirewallRuleExisting(row.FWName);
                    if (!isARule)
                    {
                        // TODO - Decide later if this is an error condition
                        L.l(location, "Rule (" + row.FWName + ") does not exist in FW when attempting to expire.");

                        // Update our record to match found state
                        row.Expired = true;
                        int cntRows = DataMgr.updateFWExpired(row);

                        // TODO - Revise this count/log, cntRows is only one row
                        L.l(location, "Expired (" + cntRows + ") FW rows due but not found in firewall.");

                        continue;// Loop
                    }

                    string fwProtocol = row.Protocol.ToLower();
                    if (fwProtocol == "any" || fwProtocol == "all" || fwProtocol == "*")
                    {
                        foreach (string s in anyProtocol)
                        {
                            // Rules are appended with the leading character of protocol. e.g. FWName + "U" for UDP
                            string cmd = "/C netsh advfirewall firewall delete rule name=\"" + row.FWName + s.Substring(0, 1) + "\"";
                            if (!execCmd(cmd, true, true))
                            {
                                if (c.debug) L.d(location, "Failed to expire FW Rule (" + row.FWName + s.Substring(0, 1) + ").");
                            }
                        }
                    }
                    else
                    {
                        // Delete rule from firewall
                        string cmd = "/C netsh advfirewall firewall delete rule name=\"" + row.FWName + "\"";
                        if (!execCmd(cmd, true, true))
                        {
                            L.err(location, "Failed to exprire FW Rule (" + row.FWName + ").");
                        }
                    }

                    // See if rule still exists
                    isARule = isFirewallRuleExisting(row.FWName);
                    L.l(location, "Rule (" + row.FWName + ") test isExisting (" + isARule + ").");

                    if (!isARule)
                    {
                        // Deactivate FW rule in DB
                        row.Expired = true;
                        int cntRows = DataMgr.updateFWExpired(row);
                        L.l(location, "Expired (" + cntRows + ") FW rows in memory.");

                        if (row.FWId > 0)
                        {
                            L.l(location, "Deactivated FW rule id (" + row.FWId + "), by name (" + row.FWName + ").");
                            retValue++;
                        }
                        else
                        {
                            // TODO - Decide later on error condition
                            L.l(location, "Failed to deactivate FW rule in storage by name (" + row.FWName + ").");
                        }
                        summary.CntFWExpired++;// accept FW removal enough for summary count
                    }
                    else
                    {
                        summary.CntFWExpireFail++;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }

        public static bool execCmd(string cmd, bool requireAdmin, bool waitForExit)
        {
            const string location = CLASSNAME + ".execCmd";
            bool retValue = false;
            try
            {
                if (requireAdmin && !isAppAdmin)
                {
                    L.err(location, "Ignoring request to run command as non-admin. Command (" + cmd + ").");
                }
                else
                {
                    using (Process RunCmd = new Process())
                    {
                        try
                        {
                            RunCmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            RunCmd.StartInfo.FileName = "cmd.exe";
                            RunCmd.StartInfo.Arguments = cmd;
                            RunCmd.Start();
                            if (waitForExit) RunCmd.WaitForExit();
                            retValue = true;
                        }
                        catch (Exception ex)
                        {
                            L.ex(location, ex);
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


        private static void logEventRecord(EventRecord record, string inLocation)
        {
            const string location = ".logEventRecord";
            try
            {
                if (string.IsNullOrEmpty(inLocation)) inLocation = location;
                string msg = "";

                msg += "ActivityId: " + record.ActivityId + "\n";
                msg += "Bookmark: " + record.Bookmark.ToString() + "\n";
                msg += "Id: " + record.Id + "\n";
                msg += "Keywords: " + record.Keywords + "\n";
                foreach (string s in record.KeywordsDisplayNames)
                {
                    msg += "  " + s + "\n";
                }
                msg += "Level: " + record.Level + "\n";
                msg += "LevelDisplayName: " + record.LevelDisplayName + "\n";
                msg += "LogName: " + record.LogName + "\n";
                msg += "MachineName: " + record.MachineName + "\n";
                msg += "Opcode: " + record.Opcode + "\n";
                msg += "OpcodeDisplayName: " + record.OpcodeDisplayName + "\n";
                msg += "ProcessId: " + record.ProcessId + "\n";
                msg += "ProviderId: " + record.ProviderId + "\n";
                msg += "ProviderName: " + record.ProviderName + "\n";
                msg += "Qualifiers: " + record.Qualifiers + "\n";
                msg += "RecordId: " + record.RecordId + "\n";
                msg += "ReleatedActivityId: " + record.RelatedActivityId + "\n";
                msg += "Task: " + record.Task + "\n";
                msg += "TaskDisplayName: " + record.TaskDisplayName + "\n";
                msg += "ThreadId: " + record.ThreadId + "\n";
                msg += "TimeCreated: " + record.TimeCreated + "\n";
                msg += "UserId: " + record.UserId + "\n";
                msg += "Version: " + record.Version + "\n";
                msg += "ToXml: " + record.ToXml() + "\n";
                msg += "\n";

                L.l(inLocation, msg);
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

    }



}
