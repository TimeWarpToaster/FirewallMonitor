using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json.Linq;

namespace FWM_Client_03
{
    public class IpBlock
    {
        const string CLASSNAME = "IpBlock";
        public List<string> logsOut = new List<string>();

        public bool Status { get; set; }
        public long IpBlockId { get; set; }
        public bool Active { get; set; }
        public DateTime CreateDateTime { get; set; }
        public string BlockAddress { get; set; }
        public int CntAttempts { get; set; }
        public int CntFailedLogins { get; set; }
        public int CntScansFlagged { get; set; }
        public bool FlaggedThisScan { get; set; }
        public int CntIps { get; set; }
        //public Dictionary<string, IpEvent> IpEvents { get; set; }
        public List<IpEvent> IpEvents { get; set; }
        //public List<string> Ips { get; set; }
        public Dictionary<string, int> Ips { get; set; }
        public Dictionary<string, int> UserNames { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime LastTime { get; set; }
        public double Elapsed { get; set; }
        public double TotalLatency { get; set; }
        public double GreatestLatency { get; set; }
        public double LowestLatency { get; set; }
        public double AverageLatency { get; set; }



        public IpBlock()
        {
            const string location = CLASSNAME + ".Constructor";
            try
            {
                if (!this.init())
                {
                    L.err(location, "Failed to initialize " + CLASSNAME + " object.");
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        //public IpBlock(Dictionary<string, IpEvent> ipEvents)
        public IpBlock(List<IpEvent> ipEvents)
        {
            const string location = CLASSNAME + ".Constructor(dict)";
            try
            {
                if (!this.init())
                {
                    L.err(location, "Failed to init IpBlock!");
                }

                // Identify IP block by first-three, auto-corrects if full-IP is used
                if (!this.FromIpEvents(ipEvents))
                {
                    L.err(location, "Failed to set BlockAddress!");
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        public IpBlock(string blockAddress, List<IpEvent> ipEvents)
        {
            const string location = CLASSNAME + ".Constructor(str,list)";
            try
            {
                if (!this.init())
                {
                    L.err(location, "Failed to init IpBlock!");
                }

                // Identify IP block by first-three, auto-corrects if full-IP is used
                if (!this.setBlockAddress(blockAddress))
                {
                    L.err(location, "Failed to set BlockAddress!");
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
            bool retValue = false;
            try
            {
                this.Status = false;
                this.IpBlockId = 0L;
                this.Active = false;
                this.CreateDateTime = c.nDt;
                this.BlockAddress = "";
                this.CntAttempts = 0;
                this.CntFailedLogins = 0;
                this.CntScansFlagged = 0;
                this.FlaggedThisScan = false;
                this.CntIps = 0;
                //this.IpEvents = new Dictionary<string, IpEvent>();
                this.IpEvents = new List<IpEvent>();
                this.Ips = new Dictionary<string, int>();
                this.UserNames = new Dictionary<string, int>();
                this.StartTime = c.nDt;
                this.EndTime = c.nDt;
                this.LastTime = c.nDt;
                this.Elapsed = 0d;
                this.TotalLatency = -1d;
                this.GreatestLatency = -1d;
                this.LowestLatency = -1d;
                this.AverageLatency = -1d;

                // Flag success for completing
                retValue = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }

        public bool setBlockAddress(string blockAddress)
        {
            const string location = CLASSNAME + ".setBlockAddress";
            bool retValue = false;
            try
            {
                // Identify IP block by first-three, auto-corrects if full-IP is used
                try
                {
                    String[] split = blockAddress.Split('.');
                    blockAddress = split[0] + "." + split[1] + "." + split[2];
                    retValue = true;
                }
                catch (Exception ex) { }
                this.BlockAddress = blockAddress;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }

        public bool writeBlock()
        {
            const string location = CLASSNAME + ".writeBlock";
            bool retValue = false;
            try
            {
                if (!c.ipBlocks.ContainsKey(this.BlockAddress))
                {
                    c.ipBlocks.Add(this.BlockAddress, this);
                }
                else
                {
                    c.ipBlocks[this.BlockAddress] = this;
                }
                retValue = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }

        public bool FromIpEvents(List<IpEvent> ipEvents)
        {
            const string location = CLASSNAME + ".FromIpEvents";
            bool retValue = false;
            try
            {
                //L.logger(location, "Setting IP block from IpEvents dictionary.");
                this.IpEvents = ipEvents;
                this.CntIps = this.IpEvents.Count;

                // Create a temporary master list of failures for block
                List<FailedLoginEvent> fles = new List<FailedLoginEvent>();

                int cntFailedLogins = 0;
                for (int i = 0; i < this.IpEvents.Count; i++)
                //foreach (KeyValuePair<string, IpEvent> pair in this.IpEvents)
                {
                    IpEvent ipEvent = this.IpEvents[i];
                    cntFailedLogins += ipEvent.FailedLogins.Count;
                    //cntFailedLogins += pair.Value.FailedLogins.Count;
                    foreach (FailedLoginEvent fle in ipEvent.FailedLogins)
                    {
                        fles.Add(fle);
                    }
                    /*cntFailedLogins += pair.Value.FailedLogins.Count;
                    cntFailedLogins += pair.Value.FailedLogins.Count;
                    foreach (FailedLoginEvent fle in pair.Value.FailedLogins)
                    {
                        fles.Add(fle);
                    }*/
                }
                fles.Sort((pair1, pair2) => pair1.CreateDateTime.CompareTo(pair2.CreateDateTime));
                fles.Reverse();


                // Iterate events, track block level values
                DateTime lastTime = c.nDt;
                foreach (FailedLoginEvent fle in fles)
                {
                    if (string.IsNullOrEmpty(this.BlockAddress))
                    {
                        this.BlockAddress = c.getBlockAddress(fle.IpAddress);
                    }

                    if (this.LastTime > c.nDt && fle.CreateDateTime > c.nDt)
                    {
                        double latency = (lastTime - fle.CreateDateTime).TotalMilliseconds;
                        //totalLatency += fle.Latency;
                        if (latency > this.GreatestLatency) this.GreatestLatency = latency;
                        if (latency < this.LowestLatency || this.LowestLatency < 0) this.LowestLatency = latency;
                    }
                    this.LastTime = fle.CreateDateTime;

                    // Record start and end time for this IP
                    if (fle.CreateDateTime > this.EndTime) this.EndTime = fle.CreateDateTime;
                    if (fle.CreateDateTime <= this.StartTime || this.StartTime <= c.nDt) this.StartTime = fle.CreateDateTime;


                    // Determine if IP is known to block
                    if (this.Ips.ContainsKey(fle.IpAddress)) this.Ips[fle.IpAddress]++;
                    else this.Ips.Add(fle.IpAddress, 1);

                    // Determine if this UserName is known to block
                    if (this.UserNames.ContainsKey(fle.TargetUserName)) this.UserNames[fle.TargetUserName]++;
                    else this.UserNames.Add(fle.TargetUserName, 1);
                }

                if (this.StartTime > c.nDt && this.EndTime > c.nDt)
                {
                    this.Elapsed = (this.EndTime - this.StartTime).TotalSeconds;
                }
                this.AverageLatency = (fles.Count == 0) ? 0 : this.Elapsed / fles.Count;
                this.CntFailedLogins = fles.Count;


                // Free memory
                fles.Clear();
                fles = null;

                // Flag success for completing
                retValue = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }

        public bool fromBinary(ref BinaryReader reader)
        {
            const string location = CLASSNAME + ".fromBinary";
            bool retVal = false;
            try
            {
                if (reader == null)
                {
                    L.err(location, "Reader was null.");
                    return retVal; //Early Exit
                }

                if (!reader.BaseStream.CanRead)
                {
                    L.err(location, "Reader not available for read.");
                    return retVal; //Early Exit
                }

                if (!this.init())
                {
                    L.err(location, "Failed to preinitialize object.");
                    return retVal; //Early Exit
                }
                long maxIdx = reader.BaseStream.Length - 1;
                if (reader.BaseStream.Position < maxIdx) this.Status = reader.ReadBoolean();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.IpBlockId = reader.ReadInt64();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.Active = reader.ReadBoolean();
                else return retVal;
                this.CreateDateTime = c.nDt;
                if (reader.BaseStream.Position < maxIdx)
                {
                    string dtString = reader.ReadString();
                    if (dtString != null && dtString.Length > 0)
                    {
                        try
                        {
                            this.CreateDateTime = DateTime.Parse(dtString);
                        }
                        catch (Exception exConv) { }
                    }
                }
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.BlockAddress = U.decodeString(reader.ReadString());
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.CntAttempts = reader.ReadInt32();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.CntFailedLogins = reader.ReadInt32();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.CntScansFlagged = reader.ReadInt32();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.FlaggedThisScan = reader.ReadBoolean();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.CntIps = reader.ReadInt32();
                else return retVal;
                this.StartTime = c.nDt;
                if (reader.BaseStream.Position < maxIdx)
                {
                    string dtString = reader.ReadString();
                    if (dtString != null && dtString.Length > 0)
                    {
                        try
                        {
                            this.StartTime = DateTime.Parse(dtString);
                        }
                        catch (Exception exConv) { }
                    }
                }
                else return retVal;
                this.EndTime = c.nDt;
                if (reader.BaseStream.Position < maxIdx)
                {
                    string dtString = reader.ReadString();
                    if (dtString != null && dtString.Length > 0)
                    {
                        try
                        {
                            this.EndTime = DateTime.Parse(dtString);
                        }
                        catch (Exception exConv) { }
                    }
                }
                else return retVal;
                this.LastTime = c.nDt;
                if (reader.BaseStream.Position < maxIdx)
                {
                    string dtString = reader.ReadString();
                    if (dtString != null && dtString.Length > 0)
                    {
                        try
                        {
                            this.LastTime = DateTime.Parse(dtString);
                        }
                        catch (Exception exConv) { }
                    }
                }
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.Elapsed = reader.ReadDouble();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.TotalLatency = reader.ReadDouble();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.GreatestLatency = reader.ReadDouble();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.LowestLatency = reader.ReadDouble();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.AverageLatency = reader.ReadDouble();
                else return retVal;

                // Flag success for completing
                retVal = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public bool fromClone(IpBlock from, bool copyLists)
        {
            const string location = CLASSNAME + ".fromClone";
            bool retVal = false;
            try
            {
                if (from == null)
                {
                    L.err(location, "Input was null.");
                    return retVal;
                }
                if (!this.init())
                {
                    L.err(location, "Failed to initialize object before clone.");
                    return retVal;
                }

                this.logsOut = from.logsOut;

                this.Status = from.Status;
                this.IpBlockId = from.IpBlockId;
                this.Active = from.Active;
                this.CreateDateTime = from.CreateDateTime;
                this.BlockAddress = from.BlockAddress;
                this.CntAttempts = from.CntAttempts;
                this.CntFailedLogins = from.CntFailedLogins;
                this.CntScansFlagged = from.CntScansFlagged;
                this.FlaggedThisScan = from.FlaggedThisScan;
                this.CntIps = from.CntIps;
                if (copyLists)
                {
                    this.IpEvents = from.IpEvents;
                    this.Ips = from.Ips;
                    this.UserNames = from.UserNames;
                }
                else
                {
                    this.IpEvents = new List<IpEvent>();
                    this.Ips = new Dictionary<string, int>();
                    this.UserNames = new Dictionary<string, int>();
                }
                this.StartTime = from.StartTime;
                this.EndTime = from.EndTime;
                this.LastTime = from.LastTime;
                this.Elapsed = from.Elapsed;
                this.TotalLatency = from.TotalLatency;
                this.GreatestLatency = from.GreatestLatency;
                this.LowestLatency = from.LowestLatency;
                this.AverageLatency = from.AverageLatency;

                // Flag success for completing 
                retVal = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public bool fromJArray(JArray data)
        {
            const string location = CLASSNAME + ".fromJArray";
            bool retVal = false;
            try
            {
                if (data == null) return retVal;

                int idx = 0;
                if (idx < data.Count) this.Status = U.getBool(data, idx, false);
                else return retVal;

                idx++;
                if (idx < data.Count) this.IpBlockId = U.getLong(data, idx, 0L);
                else return retVal;

                idx++;
                if (idx < data.Count) this.Active = U.getBool(data, idx, false);
                else return retVal;

                idx++;
                if (idx < data.Count) this.CreateDateTime = U.getDate(data, idx);
                else return retVal;

                idx++;
                if (idx < data.Count) this.BlockAddress = U.getString(data, idx, "");
                else return retVal;

                idx++;
                if (idx < data.Count) this.CntAttempts = U.getInt(data, idx, 0);
                else return retVal;

                idx++;
                if (idx < data.Count) this.CntFailedLogins = U.getInt(data, idx, 0);
                else return retVal;

                idx++;
                if (idx < data.Count) this.CntScansFlagged = U.getInt(data, idx, 0);
                else return retVal;

                idx++;
                if (idx < data.Count) this.FlaggedThisScan = U.getBool(data, idx, false);
                else return retVal;

                idx++;
                if (idx < data.Count) this.CntIps = U.getInt(data, idx, 0);
                //public Dictionary<string, IpEvent> IpEvents { get; set; }
                //data.Add(this.IpEvents { get; set; }
                //public List<string> Ips { get; set; }
                //data.Add(this.Ips { get; set; }
                //data.Add(this.UserNames { get; set; }
                else return retVal;

                idx++;
                if (idx < data.Count) this.StartTime = U.getDate(data, idx);
                else return retVal;

                idx++;
                if (idx < data.Count) this.EndTime = U.getDate(data, idx);
                else return retVal;

                idx++;
                if (idx < data.Count) this.LastTime = U.getDate(data, idx);
                else return retVal;

                idx++;
                if (idx < data.Count) this.Elapsed = U.getDouble(data, idx, 0d);
                else return retVal;

                idx++;
                if (idx < data.Count) this.TotalLatency = U.getDouble(data, idx, 0d);
                else return retVal;

                idx++;
                if (idx < data.Count) this.GreatestLatency = U.getDouble(data, idx, 0d);
                else return retVal;

                idx++;
                if (idx < data.Count) this.LowestLatency = U.getDouble(data, idx, 0d);
                else return retVal;

                idx++;
                if (idx < data.Count) this.AverageLatency = U.getDouble(data, idx, 0d);

                // Qualify result
                retVal = this.IpBlockId > 0 && this.BlockAddress != null && this.BlockAddress.Length > 0;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public bool fromJObject(JObject data)
        {
            const string location = CLASSNAME + ".fromJArray";
            bool retVal = false;
            try
            {
                if (data == null) return retVal;

                this.Status = U.getBool(data, "Status", false);
                this.IpBlockId = U.getLong(data, "IpBlockId", 0L);
                this.Active = U.getBool(data, "Active", false);
                this.CreateDateTime = U.getDate(data, "CreateDateTime");
                this.BlockAddress = U.getString(data, "BlockAddress", "");
                this.CntAttempts = U.getInt(data, "CntAttempts", 0);
                this.CntFailedLogins = U.getInt(data, "CntFailedLogins", 0);
                this.CntScansFlagged = U.getInt(data, "CntScansFlagged", 0);
                this.FlaggedThisScan = U.getBool(data, "FlaggedThisScan", false);
                this.CntIps = U.getInt(data, "CntIps", 0);
                //public Dictionary<string, IpEvent> IpEvents { get; set; }
                //data.Add(this.IpEvents { get; set; }
                //public List<string> Ips { get; set; }
                //data.Add(this.Ips { get; set; }
                //data.Add(this.UserNames { get; set; }
                this.StartTime = U.getDate(data, "StartTime");
                this.EndTime = U.getDate(data, "EndTime");
                this.LastTime = U.getDate(data, "LastTime");
                this.Elapsed = U.getDouble(data, "Elapsed", 0d);
                this.TotalLatency = U.getDouble(data, "TotalLatency", 0d);
                this.GreatestLatency = U.getDouble(data, "GreatestLatency", 0d);
                this.LowestLatency = U.getDouble(data, "LowestLatency", 0d);
                this.AverageLatency = U.getDouble(data, "AverageLatency", 0d);



                // Qualify result
                retVal = this.IpBlockId > 0 && this.BlockAddress != null && this.BlockAddress.Length > 0;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public bool toBinary(ref BinaryWriter writer)
        {
            const string location = CLASSNAME + ".toBinary";
            bool retVal = false;
            try
            {
                if (writer == null)
                {
                    L.err(location, "Writer was null.");
                    return retVal;
                }

                writer.Write(this.Status);
                writer.Write(this.IpBlockId);
                writer.Write(this.Active);// TODO - Convert active to int
                writer.Write(this.CreateDateTime == null || this.CreateDateTime == c.nDt ? "" : CreateDateTime.ToString(TAG.DTF));
                writer.Write(this.BlockAddress == null ? "" : U.encodeString(this.BlockAddress));
                writer.Write(this.CntAttempts);
                writer.Write(this.CntFailedLogins);
                writer.Write(this.CntScansFlagged);
                writer.Write(this.FlaggedThisScan);
                writer.Write(this.CntIps);
                writer.Write(this.StartTime == null ? "" : this.StartTime.ToString(TAG.DTF));
                writer.Write(this.EndTime == null ? "" : this.EndTime.ToString(TAG.DTF));
                writer.Write(this.LastTime == null ? "" : this.LastTime.ToString(TAG.DTF));
                writer.Write(this.Elapsed);
                writer.Write(this.TotalLatency);
                writer.Write(this.GreatestLatency);
                writer.Write(this.LowestLatency);
                writer.Write(this.AverageLatency);

                // Flag success for completing
                retVal = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public JArray toJArray()
        {
            const string location = CLASSNAME + ".toJArray";
            JArray retVal = null;
            try
            {
                // Important Note:  By the time this is called, we are no longer concerned with nested relationships.
                // Child objects should already have a parent Id attached, and relationships are managed by Id lookups elsewhere.

                JArray data = new JArray();

                data.Add(this.Status);
                data.Add(this.IpBlockId);
                data.Add(this.Active);
                data.Add(this.CreateDateTime.ToString(TAG.DTF));
                data.Add(this.BlockAddress);
                data.Add(this.CntAttempts);
                data.Add(this.CntFailedLogins);
                data.Add(this.CntScansFlagged);
                data.Add(this.FlaggedThisScan);
                data.Add(this.CntIps);
                //public Dictionary<string, IpEvent> IpEvents { get; set; }
                //data.Add(this.IpEvents { get; set; }
                //public List<string> Ips { get; set; }
                //data.Add(this.Ips { get; set; }
                //data.Add(this.UserNames { get; set; }
                data.Add(this.StartTime.ToString(TAG.DTF));
                data.Add(this.EndTime.ToString(TAG.DTF));
                data.Add(this.LastTime.ToString(TAG.DTF));
                data.Add(this.Elapsed);
                data.Add(this.TotalLatency);
                data.Add(this.GreatestLatency);
                data.Add(this.LowestLatency);
                data.Add(this.AverageLatency);

                // Flag success for compleing
                retVal = data;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public JObject toJObject()
        {
            const string location = CLASSNAME + ".toJObject";
            JObject retVal = null;
            try
            {
                // Important Note:  By the time this is called, we are no longer concerned with nested relationships.
                // Child objects should already have a parent Id attached, and relationships are managed by Id lookups elsewhere.

                JObject data = new JObject();

                data.Add("Status", this.Status);
                data.Add("IpBlockId", this.IpBlockId);
                data.Add("Active", this.Active);
                data.Add("CreateDateTime", this.CreateDateTime.ToString(TAG.DTF));
                data.Add("BlockAddress", this.BlockAddress);
                data.Add("CntAttempts", this.CntAttempts);
                data.Add("CntFailedLogins", this.CntFailedLogins);
                data.Add("CntScansFlagged", this.CntScansFlagged);
                data.Add("FlaggedThisScan", this.FlaggedThisScan);
                data.Add("CntIps", this.CntIps);
                //public Dictionary<string, IpEvent> IpEvents { get; set; }
                //data.Add(this.IpEvents { get; set; }
                //public List<string> Ips { get; set; }
                //data.Add(this.Ips { get; set; }
                //data.Add(this.UserNames { get; set; }
                data.Add("StartTime", this.StartTime.ToString(TAG.DTF));
                data.Add("EndTime", this.EndTime.ToString(TAG.DTF));
                data.Add("LastTime", this.LastTime.ToString(TAG.DTF));
                data.Add("Elapsed", this.Elapsed);
                data.Add("TotalLatency", this.TotalLatency);
                data.Add("GreatestLatency", this.GreatestLatency);
                data.Add("LowestLatency", this.LowestLatency);
                data.Add("AverageLatency", this.AverageLatency);

                // Flag success for compleing
                retVal = data;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

    }
}
