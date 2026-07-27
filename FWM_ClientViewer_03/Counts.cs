//Firewall Monitor v04
//(c) 2026 - TimeWarpToaster

//https://www.gnu.org/licenses/gpl-3.0.html

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FWM_ClientViewer_03
{
    public class Counts
    {
        public const string CLASSNAME = "Counts";

        /*public long CntEvents = 0;
        public long CntEvents30Days = 0;
        public long CntEvents7Days = 0;
        public long CntEvents1Day = 0;
        public long CntEvents6Hrs = 0;
        public long CntEvents1Hr = 0;
        public long CntEvents30Min = 0;

        public long CntUNames = 0;*/

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<Dictionary<string, object>> blocks = new List<Dictionary<string, object>>();


        /*
         * List<Block>{
         * index,
         * id,
         * cntFailedLogins,
         * cntUNames
         * List<Ip>{
         * index,
         * id,
         * cntFailedLogins,
         * cntUNames,
         * List<UName>{
         * index,
         * id, 
         * cnt
         * }
         * }
         * 
         * }
         * 
         */

         public bool fromEventList(List<FailedLoginEvent> events)
         {
            const string location = CLASSNAME + ".fromEventList";
            bool retVal = false;
            try
            {
                if (events == null)
                {
                    L.err(location, "Input events were null.");
                    return retVal;
                }

                this.blocks = new List<Dictionary<string, object>>();

                for (int i = 0; i < events.Count; i++)
                {
                    long blockId = events[i].IpBlockId;

                    // See if block exists
                    int idxBlock = -1;
                    for (int j = 0; j < this.blocks.Count; j++)
                    {
                        if (blockId == U.getLong(this.blocks[j], "BlockId"))
                        {
                            idxBlock = j;
                            break;
                        }
                    }

                    // Add block if not found
                    if (idxBlock < 0)
                    {
                        Dictionary<string, object> row = new Dictionary<string, object>();
                        int nativeIndex = DataMgr.getIpBlockIndex(blockId);
                        row.Add("index", nativeIndex); // TODO - Go get index
                        row.Add("id", events[i].IpBlockId);
                        row.Add("cntFailedLogins", 0);
                        row.Add("cntUNames", 0);
                        List<Dictionary<string, object>> ipRow = new List<Dictionary<string, object>>();
                        row.Add("ips", ipRow);

                        this.blocks.Add(row);
                        for (int j = this.blocks.Count - 1; j >= 0; j--)
                        {
                            if (events[i].IpBlockId == U.getLong(this.blocks[j], "id", 0L))
                            {
                                idxBlock = j;
                                break;
                            }
                        }
                    }

                    // Move to next record if we failed to create block
                    if (idxBlock < 0)
                    {
                        L.err(location, "Failed to create block counts.");
                        continue;
                    }

                    // See if ip exists
                    long ipId = events[i].IpEventId;
                    int idxIp = -1;
                    List<Dictionary<string, object>> ips = (List<Dictionary<string, object>>)this.blocks[idxBlock]["Ips"];
                    for (int j = 0; j < ips.Count; j++)
                    {
                        if (ipId == U.getLong(ips[j], "id", 0))
                        {
                            idxIp = j;
                            break;
                        }
                    }

                    // Add ip if not found
                    if (idxIp < 0)
                    {
                        Dictionary<string, object> ipRow = new Dictionary<string, object>();
                        int nativeIndex = DataMgr.getIpIndex(ipId);
                        ipRow.Add("index", nativeIndex); // TODO - Go get index
                        ipRow.Add("id", ipId);
                        ipRow.Add("cntFailedLogins", 0);
                        ipRow.Add("cntUNames", 0);

                        List<Dictionary<string, object>> unameList = new List<Dictionary<string, object>>();
                        ipRow.Add("unames", unameList);

                        ips.Add(ipRow);
                        for (int j = ips.Count - 1; j >= 0; j--)
                        {
                            if (ipId == U.getLong(ips[i], "id", 0))
                            {
                                idxIp = j;
                                break;
                            }
                        }
                    }

                    // Move to next record if we failed to create ip
                    if (idxIp < 0)
                    {
                        L.err(location, "Failed to create ip counts.");
                        continue;
                    }

                    // Get a convenient reference to unames
                    List<Dictionary<string, object>> unames = (List<Dictionary<string, object>>)ips[idxIp]["unames"];

                    // See if uname exists, start by getting index first, use it to look locally
                    int nativeIndexUName = DataMgr.getUNameIndex(ipId, events[i].TargetUserName);

                    int idxUName = -1;
                    for (int j = 0; j < unames.Count; j++)
                    {
                        if (nativeIndexUName == U.getInt(unames[j], "index", -1))
                        {
                            idxUName = j;
                            break;
                        }
                    }

                    // Create uname if non-exist
                    if (idxUName < 0)
                    {
                        Dictionary<string, object> uname = new Dictionary<string, object>();
                        uname.Add("index", nativeIndexUName);
                        uname.Add("id", DataMgr.UNames[nativeIndexUName].UNameId);
                        uname.Add("cnt", 0);

                        unames.Add(uname);
                        for (int j = unames.Count - 1; j >= 0; j--)
                        {
                            if (nativeIndexUName == U.getInt(unames[j], "index", -1))
                            {
                                idxUName = j;
                                break;
                            }
                        }
                    }

                    // Increment Counts
                    try
                    {
                        int cnt = (int)this.blocks[idxBlock]["cntFailedLogins"];
                        cnt++;
                        this.blocks[idxBlock]["cntFailedLogins"] = cnt;
                    }
                    catch (Exception exIncr) { }

                    try
                    {
                        int cnt = (int)ips[idxIp]["cntFailedLogins"];
                        cnt++;
                        ips[idxIp]["cntFailedLogins"] = cnt;
                    }
                    catch (Exception exIncr) { }

                    try
                    {
                        int cnt = (int)unames[idxUName]["cnt"];
                        cnt++;
                        unames[idxUName]["cnt"] = cnt;
                    }
                    catch (Exception exIncr) { }
                }



                // Go through all data. Add derived counts to the high-level
                for (int idxBlock = 0; idxBlock < this.blocks.Count; idxBlock++)
                {
                    List<Dictionary<string, object>> ips = (List<Dictionary<string, object>>)this.blocks[idxBlock]["ips"];

                    if (this.blocks[idxBlock].ContainsKey("cntIps"))
                    {
                        this.blocks[idxBlock]["cntIps"] = ips.Count;
                    }
                    else 
                    {
                        this.blocks[idxBlock].Add("cntIps", ips.Count);
                    }

                    /*int cntUNames = 0;
                    for (int idxIp = 0; idxIp < ips.Count; idxIp++)
                    {
                        cntUNames += ((List<Dictionary<string, object>>)ips[idxIp]["unames"]).Count;

                        // TODO - Update uname count on Ip
                    }*/

                    // Update UName count on IpBlock
                }

                // Flag success for completing
                // TODO - Find a better validation of rows
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
