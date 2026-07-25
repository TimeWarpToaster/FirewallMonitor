using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json.Linq;

namespace FWM_ClientViewer_03
{
    public class IpEvent
    {
        private const string CLASSNAME = "IpEvent";
        //private bool isDebug = false;
        public List<string> logsOut = new List<string>();

        public long IpEventId { get; set; }
        public long IpBlockId { get; set; }
        public bool Status { get; set; }
        public long IpId { get; set; }
        public bool Active { get; set; }
        public DateTime CreateDateTime { get; set; }
        public string IpAddress { get; set; }
        public string BlockAddress { get; set; }
        public int CntAttempts { get; set; }
        public int CntFailedLogins { get; set; }
        public int CntScansFlagged { get; set; }
        public bool FlaggedThisScan { get; set; }
        public int UserNamesAttempted { get; set; }
        public Dictionary<string, int> UserNames { get; set; }
        public double PercentOfTotal { get; set; }
        public double GreatestLatency { get; set; }
        public double LowestLatency { get; set; }
        public double AverageLatency { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Elapsed { get; set; }
        public List<FailedLoginEvent> FailedLogins { get; set; }

        public IpEvent()
        {
            const string location = CLASSNAME + ".Constructor";
            try
            {
                if (!this.init())
                {
                    L.err(location, "Failed to init " + CLASSNAME + " object");
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        public IpEvent(List<FailedLoginEvent> fles)
        {
            const string location = CLASSNAME + ".Constructor";
            try
            {
                if (!this.init())
                {
                    L.err(location, "Failed to init IpEvent!");
                }

                if (!this.FromEventList(fles))
                {
                    L.err(location, "Failed to load from Event List!");
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        public IpEvent(List<FailedLoginEvent> fles, string ipAddress)
        {
            const string location = CLASSNAME + ".Constructor";
            try
            {
                if (!this.init())
                {
                    L.err(location, "Failed to init IpEvent!");
                }

                this.IpAddress = ipAddress;
                try
                {
                    String[] split = this.IpAddress.Split('.');
                    this.BlockAddress = split[0] + "." + split[1] + "." + split[2];
                }
                catch (Exception ex) { }

                if (!this.FromEventList(fles))
                {
                    L.err(location, "Failed to load from Event List!");
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        public IpEvent(IpEvent from, bool copyLists)
        {
            const string location = CLASSNAME + ".Constructor(obj)";
            try
            {
                if (from == null)
                {
                    L.err(location, "Input was null.");
                }
                else if (!this.fromClone(from, copyLists))
                {
                    L.err(location, "Failed to clone object.");
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        public bool init()
        {
            const string location = CLASSNAME + ".IpEvent";
            bool retValue = false;
            try
            {
                this.IpEventId = 0L;
                this.IpBlockId = 0L;
                this.Status = false;
                this.IpId = 0L;
                this.CreateDateTime = c.nDt;
                this.Active = false;
                this.IpAddress = "";
                this.BlockAddress = "";
                this.CntAttempts = 0;
                this.CntFailedLogins = 0;
                this.CntScansFlagged = 0;
                this.FlaggedThisScan = false;
                this.UserNamesAttempted = 0;
                this.UserNames = new Dictionary<string, int>();
                this.PercentOfTotal = 0;
                this.GreatestLatency = 0;
                this.LowestLatency = 0;
                this.AverageLatency = 0;
                this.StartTime = c.nDt;
                this.EndTime = c.nDt;
                this.Elapsed = 0;
                this.FailedLogins = new List<FailedLoginEvent>();

                // Flag success for completing
                retValue = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }

        public bool AddEvent(FailedLoginEvent evnt)
        {
            return this.AddEvent(evnt, false);
        }

        public bool AddEvent(FailedLoginEvent evnt, bool updateStats)
        {
            const string location = CLASSNAME + ".AddEvent";
            bool retValue = false;
            try
            {
                // Quit now if IpAddress does not exist
                if (evnt == null || string.IsNullOrEmpty(evnt.IpAddress))
                {
                    return retValue;// Early Exit
                }


                long cntBefore = this.FailedLogins.Count;

                //L.logger(location, "IP Stats - IP: " + this.IpAddress + "; Failed Logins: " + this.FailedLogins.Count);

                this.FailedLogins.Add(evnt);

                if (updateStats) this.UpdateStats();

                // Flag success for completing
                retValue = (cntBefore + 1 == this.FailedLogins.Count);
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }

        private bool addUserName(string userName)
        {
            const string location = CLASSNAME + ".addUserName";
            bool retValue = false;
            try
            {
                if (this.UserNames.ContainsKey(userName))
                {
                    this.UserNames[userName]++;
                }
                else
                {
                    this.UserNames.Add(userName, 1);
                    this.UserNamesAttempted++;
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }

        public bool FromEventList(List<FailedLoginEvent> fevs)
        {
            const string location = CLASSNAME + ".FromEventList";
            bool retValue = false;
            try
            {
                string error = "";

                this.FailedLogins = fevs;
                if (!this.UpdateStats())
                {
                    L.err(location, "Failed to reset object stats for IpEvent!");
                }



                // Flag success for completing without error
                if (error.Length == 0)
                {
                    this.Status = true;
                    retValue = true;
                }
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
                if (reader.BaseStream.Position < maxIdx) this.IpEventId = reader.ReadInt64();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.IpBlockId = reader.ReadInt64();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.Status = reader.ReadBoolean();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.IpId = reader.ReadInt64();
                else return retVal;
                this.CreateDateTime = c.nDt;
                if (reader.BaseStream.Position < maxIdx)
                {
                    this.CreateDateTime = c.nDt;
                    string dtString = reader.ReadString();
                    if (dtString != null && dtString.Length > 0)
                    {
                        try
                        {
                            this.CreateDateTime = DateTime.Parse(dtString);
                        }
                        catch (Exception ex) { }
                    }
                }
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.IpAddress = U.decodeString(reader.ReadString());
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
                if (reader.BaseStream.Position < maxIdx) this.UserNamesAttempted = reader.ReadInt32();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.PercentOfTotal = reader.ReadDouble();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.GreatestLatency = reader.ReadDouble();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.LowestLatency = reader.ReadDouble();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.AverageLatency = reader.ReadDouble();
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
                if (reader.BaseStream.Position < maxIdx) this.Elapsed = reader.ReadDouble();
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

        public bool fromClone(IpEvent from, bool copyLists)
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

                //private bool isDebug = false;
                this.logsOut = from.logsOut;

                this.IpEventId = from.IpEventId;
                this.IpBlockId = from.IpBlockId;
                this.Status = from.Status;
                this.IpId = from.IpId;
                this.Active = from.Active;
                this.CreateDateTime = from.CreateDateTime;
                this.IpAddress = from.IpAddress;
                this.BlockAddress = from.BlockAddress;
                this.CntAttempts = from.CntAttempts;
                this.CntFailedLogins = from.CntFailedLogins;
                this.CntScansFlagged = from.CntScansFlagged;
                this.FlaggedThisScan = from.FlaggedThisScan;
                this.UserNamesAttempted = from.UserNamesAttempted;
                if (copyLists) this.UserNames = from.UserNames;
                else this.UserNames = new Dictionary<string, int>();
                this.PercentOfTotal = from.PercentOfTotal;
                this.GreatestLatency = from.GreatestLatency;
                this.LowestLatency = from.LowestLatency;
                this.AverageLatency = from.AverageLatency;
                this.StartTime = from.StartTime;
                this.EndTime = from.EndTime;
                this.Elapsed = from.Elapsed;
                if (copyLists) this.FailedLogins = from.FailedLogins;
                else this.FailedLogins = new List<FailedLoginEvent>();

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
                if (idx < data.Count) this.IpEventId = U.getLong(data, idx, 0L);
                else return retVal;

                idx++;
                if (idx < data.Count) this.IpBlockId = U.getLong(data, idx, 0L);
                else return retVal;

                idx++;
                if (idx < data.Count) this.Status = U.getBool(data, idx, false);
                else return retVal;

                idx++;
                if (idx < data.Count) this.IpId = U.getLong(data, idx, 0L);
                else return retVal;

                idx++;
                if (idx < data.Count) this.Active = U.getBool(data, idx, false);
                else return retVal;

                idx++;
                if (idx < data.Count) this.CreateDateTime = U.getDate(data, idx);
                else return retVal;

                idx++;
                if (idx < data.Count) this.IpAddress = U.getString(data, idx, "");
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
                if (idx < data.Count) this.UserNamesAttempted = U.getInt(data, idx, 0);
                //public List<KeyValuePair<string, int>> UserNames { get; set; }
                //public Dictionary<string, int> UserNames { get; set; }
                else return retVal;

                idx++;
                if (idx < data.Count) this.PercentOfTotal = U.getDouble(data, idx, 0d);
                else return retVal;

                idx++;
                if (idx < data.Count) this.GreatestLatency = U.getDouble(data, idx, 0d);
                else return retVal;

                idx++;
                if (idx < data.Count) this.LowestLatency = U.getDouble(data, idx, 0d);
                else return retVal;

                idx++;
                if (idx < data.Count) this.AverageLatency = U.getDouble(data, idx, 0d);
                else return retVal;

                idx++;
                if (idx < data.Count) this.StartTime = U.getDate(data, idx);
                else return retVal;

                idx++;
                if (idx < data.Count) this.EndTime = U.getDate(data, idx);
                else return retVal;

                idx++;
                if (idx < data.Count) this.Elapsed = U.getDouble(data, idx, 0d);
                //public List<FailedLoginEvent> FailedLogins { get; set; }

                // Qualify result
                retVal = this.IpEventId > 0 && this.IpId > 0;
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

                this.IpEventId = U.getLong(data, "IpEventId");
                this.IpBlockId = U.getLong(data, "IpBlockId", 0L);
                this.Status = U.getBool(data, "Status");
                this.IpId = U.getLong(data, "IpId");
                this.Active = U.getBool(data, "Active");
                this.CreateDateTime = U.getDate(data, "CreateDateTime");
                this.IpAddress = U.getString(data, "IpAddress");
                this.BlockAddress = U.getString(data, "BlockAddress");
                this.CntAttempts = U.getInt(data, "CntAttempts");
                this.CntFailedLogins = U.getInt(data, "CntFailedLogins");
                this.CntScansFlagged = U.getInt(data, "CntScansFlagged");
                this.FlaggedThisScan = U.getBool(data, "FlaggedThisScan");
                this.UserNamesAttempted = U.getInt(data, "UserNamesAttempted");
                //public List<KeyValuePair<string, int>> UserNames { get; set; }
                //public Dictionary<string, int> UserNames { get; set; }
                this.PercentOfTotal = U.getDouble(data, "PercentOfTotal");
                this.GreatestLatency = U.getDouble(data, "GreatestLatency");
                this.LowestLatency = U.getDouble(data, "LowestLatency");
                this.AverageLatency = U.getDouble(data, "AverageLatency");
                this.StartTime = U.getDate(data, "StartTime");
                this.EndTime = U.getDate(data, "EndTime");
                this.Elapsed = U.getDouble(data, "Elapsed");
                //public List<FailedLoginEvent> FailedLogins { get; set; }

                // Qualify result
                retVal = this.IpEventId > 0 && this.IpId > 0;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public bool UpdateStats()
        {
            const string location = CLASSNAME + ".UpdateStats";
            bool retValue = false;
            try
            {
                string error = "";

                this.FailedLogins.Sort((pair1, pair2) => pair1.CreateDateTime.CompareTo(pair2.CreateDateTime));
                this.FailedLogins.Reverse();

                //L.logger(location, "IP Stats - IP: " + this.IpAddress + "; Failed Logins: " + this.FailedLogins.Count);


                double greatestLatency = -1d;
                double lowestLatency = -1d;
                double totalLatency = 0d;
                DateTime lastTime = c.nDt;
                foreach (FailedLoginEvent fle in this.FailedLogins)
                {
                    if (string.IsNullOrEmpty(this.IpAddress) && !string.IsNullOrEmpty(fle.IpAddress))
                    {
                        this.IpAddress = fle.IpAddress;
                        this.BlockAddress = c.getBlockAddress(this.IpAddress);
                    }

                    string userName = fle.TargetUserName;
                    if (!string.IsNullOrEmpty(userName))
                    {
                        this.addUserName(userName);
                    }

                    // Calculate latency for this IP-Retry
                    if (lastTime > c.nDt && fle.CreateDateTime > c.nDt)
                    {
                        double ipLatency = (lastTime - fle.CreateDateTime).TotalMilliseconds;
                        totalLatency += fle.Latency;
                        if (ipLatency > this.GreatestLatency) this.GreatestLatency = ipLatency;
                        if (ipLatency < this.LowestLatency || this.LowestLatency < 0) this.LowestLatency = ipLatency;

                    }
                    lastTime = fle.CreateDateTime;

                    // Record start and end time for this IP
                    if (fle.CreateDateTime > this.EndTime) this.EndTime = fle.CreateDateTime;
                    if (fle.CreateDateTime <= this.StartTime || this.StartTime <= c.nDt) this.StartTime = fle.CreateDateTime;


                }
                // Update latency stats at IP level
                this.GreatestLatency = (int)(greatestLatency > 0 ? greatestLatency : 0);
                this.LowestLatency = (int)(lowestLatency > 0 ? lowestLatency : 0);
                this.Elapsed = (this.EndTime - this.StartTime).TotalSeconds;
                this.AverageLatency = (this.FailedLogins.Count > 0) ? this.Elapsed / this.FailedLogins.Count : -1;
                this.CntFailedLogins = this.FailedLogins.Count;


                // Flag success for completing without error
                if (error.Length == 0)
                {
                    this.Status = true;
                    retValue = true;
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
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

                writer.Write(this.IpEventId);
                writer.Write(this.IpBlockId);
                writer.Write(this.Status);
                writer.Write(this.IpId);
                writer.Write(this.CreateDateTime == null || this.CreateDateTime == c.nDt ? "" : CreateDateTime.ToString(TAG.DTF));
                writer.Write(this.IpAddress == null ? "" : U.encodeString(this.IpAddress));
                writer.Write(this.BlockAddress == null ? "" : U.encodeString(this.BlockAddress));
                writer.Write(this.CntAttempts);
                writer.Write(this.CntFailedLogins);
                writer.Write(this.CntScansFlagged);
                writer.Write(this.FlaggedThisScan);
                writer.Write(this.UserNamesAttempted);
                writer.Write(this.PercentOfTotal);
                writer.Write(this.GreatestLatency);
                writer.Write(this.LowestLatency);
                writer.Write(this.AverageLatency);
                writer.Write(this.StartTime == null ? "" : this.StartTime.ToString(TAG.DTF));
                writer.Write(this.EndTime == null ? "" : this.EndTime.ToString(TAG.DTF));
                writer.Write(this.Elapsed);

                // TODO - Decide on recording logs
                // public List<string> logsOut = new List<string>();


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
                JArray data = new JArray();

                data.Add(this.IpEventId);
                data.Add(this.IpBlockId);
                data.Add(this.Status);
                data.Add(this.IpId);
                data.Add(this.Active);
                data.Add(this.CreateDateTime.ToString(TAG.DTF));
                data.Add(this.IpAddress);
                data.Add(this.BlockAddress);
                data.Add(this.CntAttempts);
                data.Add(this.CntFailedLogins);
                data.Add(this.CntScansFlagged);
                data.Add(this.FlaggedThisScan);
                data.Add(this.UserNamesAttempted);
                //public List<KeyValuePair<string, int>> UserNames { get; set; }
                //public Dictionary<string, int> UserNames { get; set; }
                data.Add(this.PercentOfTotal);
                data.Add(this.GreatestLatency);
                data.Add(this.LowestLatency);
                data.Add(this.AverageLatency);
                data.Add(this.StartTime.ToString(TAG.DTF));
                data.Add(this.EndTime.ToString(TAG.DTF));
                data.Add(this.Elapsed);
                //public List<FailedLoginEvent> FailedLogins { get; set; }

                // Flag success for completing
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
                JObject data = new JObject();

                data.Add("IpEventId", this.IpEventId);
                data.Add("BlockId", this.IpBlockId);
                data.Add("Status", this.Status);
                data.Add("IpId", this.IpId);
                data.Add("Active", this.Active);
                data.Add("CreateDateTime", this.CreateDateTime.ToString(TAG.DTF));
                data.Add("IpAddress", this.IpAddress);
                data.Add("BlockAddress", this.BlockAddress);
                data.Add("CntAttempts", this.CntAttempts);
                data.Add("CntFailedLogins", this.CntFailedLogins);
                data.Add("CntScansFlagged", this.CntScansFlagged);
                data.Add("FlaggedThisScan", this.FlaggedThisScan);
                data.Add("UserNamesAttempted", this.UserNamesAttempted);
                //public List<KeyValuePair<string, int>> UserNames { get; set; }
                //public Dictionary<string, int> UserNames { get; set; }
                data.Add("PercentOfTotal", this.PercentOfTotal);
                data.Add("GreatestLatency", this.GreatestLatency);
                data.Add("LowestLatency", this.LowestLatency);
                data.Add("AverageLatency", this.AverageLatency);
                data.Add("StartTime", this.StartTime.ToString(TAG.DTF));
                data.Add("EndTime", this.EndTime.ToString(TAG.DTF));
                data.Add("Elapsed", this.Elapsed);
                //public List<FailedLoginEvent> FailedLogins { get; set; }

                // Flag success for completing
                retVal = data;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public JObject toJObjectForGrid()
        {
            const string location = CLASSNAME + ".toJObjectForGrid";
            JObject retVal = null;
            try
            {
                JObject data = new JObject();

                data.Add("IpEventId", this.IpEventId);
                data.Add("BlockId", this.IpBlockId);
                //data.Add("Status", this.Status);
                //data.Add("IpId", this.IpId);
                //data.Add("Active", this.Active);
                data.Add("CreateDateTime", this.CreateDateTime.ToString(TAG.DTF));
                data.Add("IpAddress", this.IpAddress);
                data.Add("BlockAddress", this.BlockAddress);
                //data.Add("CntAttempts", this.CntAttempts);
                data.Add("CntFailedLogins", this.CntFailedLogins);
                //data.Add("CntScansFlagged", this.CntScansFlagged);
                data.Add("FlaggedThisScan", this.FlaggedThisScan);
                data.Add("UserNamesAttempted", this.UserNamesAttempted);
                //public List<KeyValuePair<string, int>> UserNames { get; set; }
                //public Dictionary<string, int> UserNames { get; set; }
                //data.Add("PercentOfTotal", this.PercentOfTotal);
                //data.Add("GreatestLatency", this.GreatestLatency);
                //data.Add("LowestLatency", this.LowestLatency);
                //data.Add("AverageLatency", this.AverageLatency);
                data.Add("StartTime", this.StartTime.ToString(TAG.DTF));
                data.Add("EndTime", this.EndTime.ToString(TAG.DTF));
                //data.Add("Elapsed", this.Elapsed);
                //public List<FailedLoginEvent> FailedLogins { get; set; }

                // Flag success for completing
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
