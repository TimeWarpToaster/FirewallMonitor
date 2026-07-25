using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FWM_Client_03
{
    public class AgeData
    {
        const string CLASSNAME = "AgeData";


        // Work on the data itself. Use DataMgr directly. This process needs to
        // run alone. Place at tail of all-other app execution. 


        public bool removeOlderThan(DateTime dtAge)
        {
            const string location = CLASSNAME + ".removeOlderThan";
            bool retVal = false;
            try
            {
                if (dtAge == null || dtAge == c.nDt)
                {
                    L.err(location, "Input age was invalid.");
                    return retVal;
                }
                L.l(location, "Aging data older than (" + dtAge.ToString(TAG.DTF) + ").");

                // Reload all data from storage
                if (!DataMgr.loadAllData(false))// Treat firewall rows as a special case later
                {
                    L.err(location, "Failed to update data in memory prior to starting.");
                    return retVal;
                }

                // Error check memory
                if (DataMgr.IpBlocks == null) DataMgr.IpBlocks = new List<IpBlock>();
                if (DataMgr.IpEvents == null) DataMgr.IpEvents = new List<IpEvent>();
                if (DataMgr.FailedLoginEvents == null) DataMgr.FailedLoginEvents = new List<FailedLoginEvent>();
                if (DataMgr.UNames == null) DataMgr.UNames = new List<UName>();

                // Get original counts before aging
                int cntOrigBlock = DataMgr.IpBlocks.Count;
                int cntOrigIp = DataMgr.IpEvents.Count;
                int cntOrigEv = DataMgr.FailedLoginEvents.Count;
                int cntOrigUName = DataMgr.UNames.Count;


                L.l(location, "Reviewing :: blocks (" + DataMgr.IpBlocks.Count + "), ips (" + DataMgr.IpEvents.Count +
                    "), events (" + DataMgr.FailedLoginEvents.Count + "), unames (" + DataMgr.UNames.Count + ").");

                // Create a bucket for what is being saved
                List<IpBlock> blocks = new List<IpBlock>();


                // NOTE - This is a first process to perserve data

                // Get a list of current (non exp or deact) firewall rules, keep active fw block and ip as default
                List<FWRow> runningRules = DataMgr.getFWRunningRules();
                for (int idxFW = 0; idxFW < runningRules.Count; idxFW++)
                {
                    if (runningRules[idxFW] == null) continue;

                    long blockId = runningRules[idxFW].IpBlockId;

                    int idxBlock = -1;
                    for (int idxB = 0; idxB < blocks.Count; idxB++)
                    {
                        if (blocks[idxB] == null) continue;
                        if (blockId == blocks[idxB].IpBlockId)
                        {
                            idxBlock = idxB;
                            break;
                        }
                    }

                    if (idxBlock < 0)
                    {
                        IpBlock block = DataMgr.getIpBlockById(blockId);
                        if (block != null && block.IpBlockId >= 0)
                        {
                            IpBlock temp = new IpBlock();
                            if (temp.fromClone(block, false))
                            {
                                blocks.Add(temp);
                                for (int idxB = blocks.Count - 1; idxB >= 0; idxB--)
                                {
                                    if (blockId == blocks[idxB].IpBlockId)
                                    {
                                        idxBlock = idxB;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (idxBlock < 0)
                    {
                        L.l(location, "Failed to locate block id (" + blockId + ") in memory.");
                        continue;
                    }
                    if (blocks[idxBlock].IpEvents == null)
                        blocks[idxBlock].IpEvents = new List<IpEvent>();


                    long ipId = runningRules[idxFW].IpId;
                    int idxIp = -1;
                    for (int idxI = 0; idxI < blocks[idxBlock].IpEvents.Count; idxI++)
                    {
                        if (blocks[idxBlock].IpEvents[idxI] == null) continue;
                        if (ipId == blocks[idxBlock].IpEvents[idxI].IpEventId)
                        {
                            idxIp = idxI;
                            break;
                        }
                    }

                    if (idxIp < 0)
                    {
                        IpEvent ip = DataMgr.getIpById(ipId, 1);
                        //L.l(location, "FW preserve ip event id (" + ipId + ", " + (ip == null ? "null" : Convert.ToString(ip.IpEventId)) + ").");
                        if (ip == null) continue;

                        IpEvent temp = new IpEvent();
                        if (!temp.fromClone(ip, false))
                        {
                            L.err(location, "Failed to clone ip to pereserve list.");
                            continue;
                        }

                        blocks[idxBlock].IpEvents.Add(temp);
                        for (int i = blocks[idxBlock].IpEvents.Count - 1; i >= 0; i--)
                        {
                            if (ipId == blocks[idxBlock].IpEvents[i].IpEventId)
                            {
                                idxIp = i;
                                break;
                            }
                        }
                    }

                    if (idxIp < 0)
                    {
                        L.err(location, "Failed to find ip id (" + ipId + ") in memory.");
                        L.err(location, "FW preserve error.");
                        continue;
                    }

                    if (blocks[idxBlock].IpEvents[idxIp].FailedLogins == null)
                        blocks[idxBlock].IpEvents[idxIp].FailedLogins = new List<FailedLoginEvent>();
                    if (blocks[idxBlock].IpEvents[idxIp].UserNames == null)
                        blocks[idxBlock].IpEvents[idxIp].UserNames = new Dictionary<string, int>();
                }


                // NOTE - This is a whole second process to preserve some data an not other, using same list
                // Keep anything with a current event to support it.

                // Iterate events into nested buckets
                for (int idxEv = 0; idxEv < DataMgr.FailedLoginEvents.Count; idxEv++)
                {
                    long blockId = DataMgr.FailedLoginEvents[idxEv].IpBlockId;
                    if (blockId < 0) continue; // should be <= 0
                    long ipId = DataMgr.FailedLoginEvents[idxEv].IpEventId;
                    if (ipId < 0) continue; // should be <= 0

                    if (
                        DataMgr.FailedLoginEvents[idxEv].CreateDateTime == null ||
                        DataMgr.FailedLoginEvents[idxEv].CreateDateTime == c.nDt ||
                        // TODO - The dtAge line has to move down. We are dropping old 
                        // rows before evaluating whether an active firewall rule exists.
                        DataMgr.FailedLoginEvents[idxEv].CreateDateTime < dtAge 
                    )
                    {
                        continue;
                    }
                    //L.l(location, "Evaluating event index (" + idxEv + ").");

                    //if (idxEv < 10) L.l(location, "Evaluating block id (" + blockId + "), ip id (" + ipId + ").");

                    // See if block exists in output
                    int idxBlock = -1;
                    for (int idxB = 0; idxB < blocks.Count; idxB++)
                    {
                        if (blocks[idxB] == null) continue;
                        if (blockId == blocks[idxB].IpBlockId)
                        {
                            idxBlock = idxB;
                            break;
                        }
                    }

                    if (idxBlock < 0)
                    {
                        // Go get block from memory
                        IpBlock block = DataMgr.getIpBlockById(blockId);
                        if (block == null)
                        {
                            L.err(location, "Failed to locate block in storage.");
                            continue;
                        }
                        else
                        {
                            //if (idxEv < 10) L.l(location, "Fetched block id (" + block.IpBlockId + ").");

                            IpBlock temp = new IpBlock();
                            if (!temp.fromClone(block, false)) 
                            {
                                L.err(location, "Failed to form block id (" + block.IpBlockId + ") from clone.");
                                continue;
                            }
                            blocks.Add(temp);
                            for (int i = blocks.Count -1; i >= 0; i--)
                            {
                                if (blocks[i] == null) continue;
                                if (block.IpBlockId == blocks[i].IpBlockId)
                                {
                                    idxBlock = i;
                                    break;
                                }
                            }
                            if (idxBlock < 0)
                            {
                                L.err(location, "Failed to locate block in storage.");
                                continue;
                            }
                            else
                            {
                                // Ensure IP storage exists
                                blocks[idxBlock].IpEvents = new List<IpEvent>();
                            }
                        }
                    }
                    //if (idxEv < 10) L.l(location, "Block index (" + idxBlock + ").");


                    // See if IP exists
                    int idxIp = -1;
                    for (int idxI = 0; idxI < blocks[idxBlock].IpEvents.Count; idxI++)
                    {
                        if (ipId == blocks[idxBlock].IpEvents[idxI].IpEventId)
                        {
                            idxIp = idxI;
                            break;
                        }
                    }

                    if (idxIp < 0)
                    {
                        // Go get IP from memory
                        IpEvent ipEvent = DataMgr.getIpById(ipId, 1);
                        if (ipEvent == null)
                        {
                            ipEvent = DataMgr.getIpById(ipId, 0);
                            if (ipEvent == null)
                            {
                                L.err(location, "Failed to locate IP id in memory.");
                                continue;
                            }
                        }

                        // TODO - Check Ip id here, to ensure no active firewall rules exist

                        IpEvent temp = new IpEvent();
                        if (!temp.fromClone(ipEvent, false))
                        {
                            L.err(location, "Failed to form ip id (" + ipEvent.IpEventId + ") from clone.");
                            continue;
                        }
                        blocks[idxBlock].IpEvents.Add(ipEvent);

                        // Get IP index locally
                        for (int i = blocks[idxBlock].IpEvents.Count - 1; i >= 0; i--)
                        {
                            if (ipEvent.IpEventId == blocks[idxBlock].IpEvents[i].IpEventId)
                            {
                                idxIp = i;
                                break;
                            }
                        }

                        // Validate IP index, establish IP's lists
                        if (idxIp < 0)
                        {
                            L.err(location, "Failed to move IP into output.");
                            continue;
                        }
                        else
                        {
                            // Ensure IP has event storage
                            blocks[idxBlock].IpEvents[idxIp].FailedLogins = new List<FailedLoginEvent>();
                            blocks[idxBlock].IpEvents[idxIp].UserNames = new Dictionary<string, int>();
                        }
                    }// END if ipId < 0

                    //if (idxEv < 10) L.l(location, "Ip index (" + idxIp + ").");

                    // Add event to IP in output
                    FailedLoginEvent ev = new FailedLoginEvent();
                    if (!ev.fromClone(DataMgr.FailedLoginEvents[idxEv], false))
                    {
                        L.err(location, "Failed to form failed login event id (" + 
                            DataMgr.FailedLoginEvents[idxEv].FailedLoginEventId + ") from clone.");
                        continue; // no username without event
                    }
                    blocks[idxBlock].IpEvents[idxIp].FailedLogins.Add(ev);

                    // Establish username storage
                    if (blocks[idxBlock].UserNames == null)
                        blocks[idxBlock].UserNames = new Dictionary<string, int>();
                    if (blocks[idxBlock].IpEvents[idxIp].UserNames == null)
                        blocks[idxBlock].IpEvents[idxIp].UserNames = new Dictionary<string, int>();

                    // Update uname counts
                    string uname = DataMgr.FailedLoginEvents[idxEv].TargetUserName;
                    if (uname != null && uname.Length > 0)
                    {
                        if (blocks[idxBlock].UserNames.ContainsKey(uname))
                            blocks[idxBlock].UserNames[uname]++;
                        else
                            blocks[idxBlock].UserNames.Add(uname, 1);

                        if (blocks[idxBlock].IpEvents[idxIp].UserNames.ContainsKey(uname))
                            blocks[idxBlock].IpEvents[idxIp].UserNames[uname]++;
                        else
                            blocks[idxBlock].IpEvents[idxIp].UserNames.Add(uname, 1);
                    }

                }// END for events

                L.l(location, "Finished iterating (" + DataMgr.FailedLoginEvents.Count + ") events.");

                // Get a count of total events for percents
                int cntEvents = 0;
                for (int idxBlock = 0; idxBlock < blocks.Count; idxBlock++)
                    for (int idxIp = 0; idxIp < blocks[idxBlock].IpEvents.Count; idxIp++)
                        cntEvents += blocks[idxBlock].IpEvents[idxIp].FailedLogins.Count;

                //L.l(location, "Updating counts based upon (" + cntEvents + ") events.");

                // Do some counts of our own for data quality
                int cntFilteredBlock = 0;
                int cntFilteredIp = 0;
                int cntFilteredEv = 0;
                int cntFilteredUName = 0;

                // Update overall counts
                for (int idxBlock = 0; idxBlock < blocks.Count; idxBlock++)
                {
                    cntFilteredBlock++;
                    if (blocks[idxBlock].IpEvents == null)
                    {
                        L.l(location, "Skipping block with null ip list.");
                        continue;
                    }

                    blocks[idxBlock].CntIps = blocks[idxBlock].IpEvents.Count;
                    List<IpEvent> ips = blocks[idxBlock].IpEvents;

                    for (int idxIp = 0; idxIp < ips.Count; idxIp++)
                    {
                        cntFilteredIp++;
                        if (ips[idxIp].FailedLogins == null) continue;

                        // Set failed login counts
                        cntFilteredEv += ips[idxIp].FailedLogins.Count;
                        cntFilteredUName += ips[idxIp].UserNames.Count;
                        ips[idxIp].CntFailedLogins = ips[idxIp].FailedLogins.Count;

                        blocks[idxBlock].CntFailedLogins += ips[idxIp].CntFailedLogins;

                        ips[idxIp].PercentOfTotal = cntEvents > 0 ? (ips[idxIp].CntFailedLogins / cntEvents) * 100 : 0;

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

                // We have the most current set of data, clear memory and repopulate
                // This means a deliberate clearing of UNames, as the rows are not being preserved or updated

                DataMgr.FailedLoginEvents = new List<FailedLoginEvent>();
                //DataMgr.FWRows = new List<FWRow>();
                DataMgr.IpBlocks = new List<IpBlock>();
                DataMgr.IpEvents = new List<IpEvent>();
                //DataMgr.Summaries = new List<Summary>();
                DataMgr.UNames = new List<UName>();
                //DataMgr.XRFSums = new List<XRFSum>();
                int unameId = 0;
                for (int idxBlock = 0; idxBlock < blocks.Count; idxBlock++)
                {
                    if (blocks[idxBlock] == null) continue;

                    DataMgr.IpBlocks.Add(blocks[idxBlock]);

                    for (int idxIp = 0; idxIp < blocks[idxBlock].IpEvents.Count; idxIp++)
                    {
                        if (blocks[idxBlock].IpEvents[idxIp] == null) continue;

                        DataMgr.IpEvents.Add(blocks[idxBlock].IpEvents[idxIp]);

                        /*
                        L.l(location, "IP (" + blocks[idxBlock].IpEvents[idxIp].IpAddress + 
                            ") events (" + blocks[idxBlock].IpEvents[idxIp].FailedLogins.Count + ").");
                        */
                        for (int idxEv = 0; idxEv < blocks[idxBlock].IpEvents[idxIp].FailedLogins.Count; idxEv++)
                        {
                            DataMgr.FailedLoginEvents.Add(blocks[idxBlock].IpEvents[idxIp].FailedLogins[idxEv]);
                        }

                        foreach (KeyValuePair<string, int> kv in blocks[idxBlock].IpEvents[idxIp].UserNames)
                        {
                            // This next block means the ids for UNames are changing on a per-aging basis.
                            // There is no lookup being performed. As the lowest data, this should be fine
                            // but still should be corrected.
                            unameId++;
                            UName uname = new UName();
                            uname.UNameId = unameId;
                            uname.Active = 1;
                            uname.CreateDateTime = DateTime.Now;
                            uname.UserName = kv.Key;
                            uname.IpBlockId = blocks[idxBlock].IpBlockId;
                            uname.IpId = blocks[idxBlock].IpEvents[idxIp].IpEventId;
                            uname.Cnt = kv.Value;
                            DataMgr.UNames.Add(uname);
                        }
                    }
                }


                // Write tables that were aged
                int blocksWritten = FileMgr.writeIpBlock(DataMgr.IpBlocks);
                int ipsWritten = FileMgr.writeIpEvent(DataMgr.IpEvents);
                int evWritten = FileMgr.writeFailedLoginEvents(DataMgr.FailedLoginEvents);
                int unameWritten = FileMgr.writeUNames(DataMgr.UNames);

                L.l(location, "Original :: blocks (" + cntOrigBlock + "), ips (" + cntOrigIp +
                    "), events (" + cntOrigEv + "), unames (" + cntOrigUName + ").");

                L.l(location, "Filtered :: blocks (" + cntFilteredBlock + "), ips (" + cntFilteredIp +
                    "), events (" + cntFilteredEv + "), unames (" + cntFilteredUName + ").");

                L.l(location, "Preserved :: blocks (" + blocksWritten + "), ips (" + ipsWritten + 
                    "), events (" + evWritten + "), unames (" + unameWritten + ").");

                // Flag success for completing
                retVal = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }
    }
}
