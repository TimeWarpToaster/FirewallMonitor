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
    public class DataIndex
    {
        public const string CLASSNAME = "DataIndex";

        List<IpBlockIndex> blocks = new List<IpBlockIndex>();

        private class IpBlockIndex
        {
            public int index = -1;
            public long IpBlockId = 0L;

            public List<IpEventIndex> Ips = new List<IpEventIndex>();
        }

        private class IpEventIndex
        {
            public int index = -1;
            public long IpEventId = 0L;
            public List<int> events = new List<int>();
            public List<int> unames = new List<int>();
        }

        public DataIndex()
        {
            const string location = CLASSNAME + ".Constructor";
            try
            {
                if (!this.init())
                {
                    L.err(location, "Failed to initialize indexes.");
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        public bool init()
        {
            const string location = CLASSNAME + ".init";
            bool retVal = false;
            try
            {
                this.blocks = new List<IpBlockIndex>();
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public bool initFromEvents(List<FailedLoginEvent> events)
        {
            const string location = CLASSNAME + ".initFromStorage";
            bool retVal = false;
            try
            {
                if (!this.init())
                {
                    L.err(location, "Failed to initialize indexes.");
                    return retVal;
                }

                for (int idxEvent = 0; idxEvent < events.Count; idxEvent++)
                {
                    long ipBlockId = events[idxEvent].IpBlockId;
                    long ipEventId = events[idxEvent].IpEventId;

                    // See if block exists in indexes
                    int idxBlock = -1;
                    for (int i = 0; i < this.blocks.Count; i++)
                    {
                        if (ipBlockId == this.blocks[i].IpBlockId)
                        {
                            idxBlock = i;// TODO - set conditionally on whether set
                            break;
                        }
                    }

                    if (idxBlock < 0)
                    {
                        IpBlockIndex blockIndex = new IpBlockIndex();
                        blockIndex.IpBlockId = ipBlockId;
                        blockIndex.Ips = new List<IpEventIndex>();
                        blockIndex.index = DataMgr.getIpBlockIndex(blockIndex.IpBlockId);
                        if (blockIndex.index < 0)
                        {
                            // Data should exist by now. Erroring here could be dangerous.
                            //L.err(location, "Failed to find block index.");
                        }
                        blocks.Add(blockIndex);
                        for (int i = blocks.Count - 1; i >= 0; i--)
                        {
                            if (ipBlockId == blocks[i].IpBlockId)
                            {
                                //blocks[i].index = i;
                                idxBlock = blocks[i].index;
                                break;
                            }
                        }
                        if (idxBlock < 0)
                        {
                            L.err(location, "Failed to indentify index of block id (" + ipBlockId + ").");

                            // TODO - Should continue with the next event
                            continue;
                        }
                    }

                    // See if IP exists in indexes
                    int idxIp = -1;
                    for (int i = 0; i < blocks[idxBlock].Ips.Count; i++)
                    {
                        if (ipEventId == blocks[idxBlock].Ips[i].IpEventId)
                        {
                            idxIp = i;
                            break;
                        }
                    }

                    if (idxIp < 0)
                    {
                        // Add Ip to indexes
                        IpEventIndex ipIndex = new IpEventIndex();
                        ipIndex.IpEventId = ipEventId;
                        ipIndex.events = new List<int>();
                        ipIndex.unames = new List<int>();
                        ipIndex.index = DataMgr.getIpIndex(ipEventId);
                        blocks[idxBlock].Ips.Add(ipIndex);

                        for (int i = blocks[idxBlock].Ips.Count - 1; i >= 0; i--)
                        {
                            if (ipEventId == blocks[idxBlock].Ips[i].IpEventId)
                            {
                                //blocks[idxBlock].Ips[i].index = DataMgr.getIpIndex(ipEventId);
                                idxIp = blocks[idxBlock].Ips[i].index;
                                break;
                            }
                        }
                        if (idxIp < 0)
                        {
                            L.err(location, "Failed to locate index for ip id (" + ipEventId + ").");
                            continue;
                        }
                    }

                    // Add an event index to IP
                    int originalIndex = DataMgr.getFailedLoginEventIndex(events[idxEvent].FailedLoginEventId);
                    if (originalIndex < 0) 
                    {
                        //L.err(location, "Failed to identify index of event record.");
                        continue;
                    }
                    //blocks[idxBlock].Ips[idxIp].events.Add(idxEvent);
                    blocks[idxBlock].Ips[idxIp].events.Add(originalIndex);

                    // Get a UName index
                    int idxUName = DataMgr.getUNameIndex(ipEventId, DataMgr.FailedLoginEvents[idxEvent].TargetUserName);
                    if (blocks[idxBlock].Ips[idxIp].unames.IndexOf(idxUName) < 0)
                    {
                        blocks[idxBlock].Ips[idxIp].unames.Add(idxUName);
                    }

                    // TODO - Is it good to lookup every uname, can unique be tracked instead?
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public bool initFromStorage()
        {
            const string location = CLASSNAME + ".initFromStorage";
            bool retVal = false;
            try
            {
                if (!this.init())
                {
                    L.err(location, "Failed to initialize indexes.");
                    return retVal;
                }

                for (int idxEvent = 0; idxEvent < DataMgr.FailedLoginEvents.Count; idxEvent++)
                {
                    long ipBlockId = DataMgr.FailedLoginEvents[idxEvent].IpBlockId;
                    long ipEventId = DataMgr.FailedLoginEvents[idxEvent].IpEventId;

                    // See if block exists in indexes
                    int idxBlock = -1;
                    for (int i = 0; i < this.blocks.Count; i++)
                    {
                        if (ipBlockId == this.blocks[i].IpBlockId)
                        {
                            idxBlock = i;// TODO - set conditionally on whether set
                            break;
                        }
                    }

                    if (idxBlock < 0)
                    {
                        IpBlockIndex blockIndex = new IpBlockIndex();
                        blockIndex.IpBlockId = ipBlockId;
                        blockIndex.Ips = new List<IpEventIndex>();
                        blockIndex.index = DataMgr.getIpBlockIndex(blockIndex.IpBlockId);
                        if (blockIndex.index < 0)
                        {
                            // Data should exist by now. Erroring here could be dangerous.
                            //L.err(location, "Failed to find block index.");
                        }
                        blocks.Add(blockIndex);
                        for (int i = blocks.Count - 1; i >= 0; i--)
                        {
                            if (ipBlockId == blocks[i].IpBlockId)
                            {
                                blocks[i].index = i;
                                idxBlock = blocks[i].index;
                                break;
                            }
                        }
                        if (idxBlock < 0)
                        {
                            L.err(location, "Failed to indentify index of block id (" + ipBlockId + ").");

                            // TODO - Should continue with the next event
                            continue;
                        }
                    }

                    // See if IP exists in indexes
                    int idxIp = -1;
                    for (int i = 0; i < blocks[idxBlock].Ips.Count; i++)
                    {
                        if (ipEventId == blocks[idxBlock].Ips[i].IpEventId)
                        {
                            idxIp = i;
                            break;
                        }
                    }

                    if (idxIp < 0)
                    {
                        // Add Ip to indexes
                        IpEventIndex ipIndex = new IpEventIndex();
                        ipIndex.IpEventId = ipEventId;
                        ipIndex.events = new List<int>();
                        ipIndex.unames = new List<int>();
                        blocks[idxBlock].Ips.Add(ipIndex);

                        for (int i = blocks[idxBlock].Ips.Count - 1; i >= 0; i--)
                        {
                            if (ipEventId == blocks[idxBlock].Ips[i].IpEventId)
                            {
                                blocks[idxBlock].Ips[i].index = DataMgr.getIpIndex(ipEventId);
                                idxIp = blocks[idxBlock].Ips[i].index;
                                break;
                            }
                        }
                        if (idxIp < 0)
                        {
                            L.err(location, "Failed to locate index for ip id (" + ipEventId + ").");
                            continue;
                        }
                    }

                    // Add an event index to IP
                    blocks[idxBlock].Ips[idxIp].events.Add(idxEvent);

                    // Get a UName index
                    int idxUName = DataMgr.getUNameIndex(ipEventId, DataMgr.FailedLoginEvents[idxEvent].TargetUserName);
                    if (blocks[idxBlock].Ips[idxIp].unames.IndexOf(idxUName) < 0)
                    {
                        blocks[idxBlock].Ips[idxIp].unames.Add(idxUName);
                    }

                    // TODO - Is it good to lookup every uname, can unique be tracked instead?
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
