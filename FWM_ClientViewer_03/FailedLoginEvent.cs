//Firewall Monitor v04
//(c) 2026 - TimeWarpToaster

//https://www.gnu.org/licenses/gpl-3.0.html

using System;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Text;
using System.Xml;

using Newtonsoft.Json.Linq;

namespace FWM_ClientViewer_03
{
    public class FailedLoginEvent
    {
        private const string CLASSNAME = "FailedLoginEvent";
        private bool isDebug = false;

        //public FailedLoginEvent output = null;

        public bool isFailedLogin = false;

        public long FailedLoginEventId = 0L;
        public long IpEventId = 0L;
        public long IpBlockId = 0L;
        public string EventId { get; set; }
        public string Version { get; set; }
        public string Level { get; set; }
        public string Task { get; set; }
        public string Opcode { get; set; }
        public string Keywords { get; set; }
        public string TimeCreated { get; set; }
        public string EventRecordId { get; set; }
        public string Correlation { get; set; }
        public string Execution { get; set; }
        public string Channel { get; set; }
        public string Computer { get; set; }
        public string SubjectUserSid { get; set; }
        public string SubjectUserName { get; set; }
        public string SubjectDomainName { get; set; }
        public string SubjectLogonId { get; set; }
        public string TargetUserSid { get; set; }
        public string TargetUserName { get; set; }
        public string TargetDomainName { get; set; }
        public string Status { get; set; }
        public string FailureReason { get; set; }
        public string SubStatus { get; set; }
        public string LogonType { get; set; }
        public string LogonProcessName { get; set; }
        public string AuthenticationPackageName { get; set; }
        public string WorkstationName { get; set; }
        public string TransmittedServices { get; set; }
        public string LmPackageName { get; set; }
        public string KeyLength { get; set; }
        public string ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string IpAddress { get; set; }
        public string IpPort { get; set; }
        public string KeywordsDisplayNames { get; set; }
        public DateTime CreateDateTime { get; set; }
        public double Latency { get; set; }


        public FailedLoginEvent()
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

        public FailedLoginEvent(FailedLoginEvent from, bool copyLists)
        {
            const string location = CLASSNAME + ".Constructor(obj)";
            try
            {
                // Logging in this class is bad due to volume of objects created
                if (from == null)
                {

                }
                else if (!this.fromClone(from, copyLists))
                {
                    //L.err(location, "Failed to clone object.");
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
                this.FailedLoginEventId = 0L;
                this.IpEventId = 0L;// TODO - Update viewer
                this.IpBlockId = 0L;
                //this.output = null;
                this.isFailedLogin = false;
                this.EventId = "";
                this.Version = "";
                this.Level = "";
                this.Task = "";
                this.Opcode = "";
                this.Keywords = "";
                this.TimeCreated = "";
                this.EventRecordId = "";
                this.Correlation = "";
                this.Execution = "";
                this.Channel = "";
                this.Computer = "";
                this.SubjectUserSid = "";
                this.SubjectUserName = "";
                this.SubjectDomainName = "";
                this.SubjectLogonId = "";
                this.TargetUserSid = "";
                this.TargetUserName = "";
                this.TargetDomainName = "";
                this.Status = "";
                this.FailureReason = "";
                this.SubStatus = "";
                this.LogonType = "";
                this.LogonProcessName = "";
                this.AuthenticationPackageName = "";
                this.WorkstationName = "";
                this.TransmittedServices = "";
                this.LmPackageName = "";
                this.KeyLength = "";
                this.ProcessId = "";
                this.ProcessName = "";
                this.IpAddress = "";
                this.IpPort = "";
                this.KeywordsDisplayNames = "";
                this.CreateDateTime = new DateTime();
                this.Latency = -1d;

                // Flag success for completing
                retVal = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
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
                if (reader.BaseStream.Position < maxIdx) this.FailedLoginEventId = reader.ReadInt64();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.IpEventId = reader.ReadInt64();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.IpBlockId = reader.ReadInt64();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.isFailedLogin = reader.ReadBoolean();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.EventId = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.Version = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.Level = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.Task = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.Opcode = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.Keywords = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.TimeCreated = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.EventRecordId = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.Correlation = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.Execution = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.Channel = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.Computer = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.SubjectUserSid = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.SubjectUserName = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.SubjectDomainName = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.SubjectLogonId = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.TargetUserSid = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.TargetUserName = U.decodeString(reader.ReadString());
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.TargetDomainName = U.decodeString(reader.ReadString());
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.Status = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.FailureReason = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.SubStatus = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.LogonType = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.LogonProcessName = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.AuthenticationPackageName = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.WorkstationName = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.TransmittedServices = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.LmPackageName = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.KeyLength = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.ProcessId = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.ProcessName = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.IpAddress = U.decodeString(reader.ReadString());
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.IpPort = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.KeywordsDisplayNames = reader.ReadString();
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
                        catch (Exception ex) { }
                    }
                }
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.Latency = reader.ReadDouble();
                else return retVal;


                // Flag success for completing
                retVal = true;
                return retVal;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public bool fromClone(FailedLoginEvent obj, bool copyLists)
        {
            const string location = CLASSNAME + ".fromClone";
            bool retVal = false;
            try
            {
                if (!this.init())
                {
                    //L.err(location, "Failed to initialize before cloning.");
                }
                this.isDebug = obj.isDebug;

                //this.output = obj.output;

                this.isFailedLogin = obj.isFailedLogin;

                this.FailedLoginEventId = obj.FailedLoginEventId;
                this.IpEventId = obj.IpEventId;
                this.IpBlockId = obj.IpBlockId;
                this.EventId = obj.EventId;
                this.Version = obj.Version;
                this.Level = obj.Level;
                this.Task = obj.Task;
                this.Opcode = obj.Opcode;
                this.Keywords = obj.Keywords;
                this.TimeCreated = obj.TimeCreated;
                this.EventRecordId = obj.EventRecordId;
                this.Correlation = obj.Correlation;
                this.Execution = obj.Execution;
                this.Channel = obj.Channel;
                this.Computer = obj.Computer;
                this.SubjectUserSid = obj.SubjectUserSid;
                this.SubjectUserName = obj.SubjectUserName;
                this.SubjectDomainName = obj.SubjectDomainName;
                this.SubjectLogonId = obj.SubjectLogonId;
                this.TargetUserSid = obj.TargetUserSid;
                this.TargetUserName = obj.TargetUserName;
                this.TargetDomainName = obj.TargetDomainName;
                this.Status = obj.Status;
                this.FailureReason = obj.FailureReason;
                this.SubStatus = obj.SubStatus;
                this.LogonType = obj.LogonType;
                this.LogonProcessName = obj.LogonProcessName;
                this.AuthenticationPackageName = obj.AuthenticationPackageName;
                this.WorkstationName = obj.WorkstationName;
                this.TransmittedServices = obj.TransmittedServices;
                this.LmPackageName = obj.LmPackageName;
                this.KeyLength = obj.KeyLength;
                this.ProcessId = obj.ProcessId;
                this.ProcessName = obj.ProcessName;
                this.IpAddress = obj.IpAddress;
                this.IpPort = obj.IpPort;
                this.KeywordsDisplayNames = obj.KeywordsDisplayNames;
                this.CreateDateTime = obj.CreateDateTime;
                this.Latency = obj.Latency;

                // Flag success for completing
                retVal = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public bool fromEventRecord(EventRecord record)
        {
            const string location = CLASSNAME + ".fromEventRecord";
            bool retValue = false;
            try
            {
                this.Keywords = Convert.ToString(record.Keywords);


                this.isFailedLogin =
                    (this.Keywords == "0x8010000000000000" || this.Keywords == "-9218868437227405312");
                if (!this.isFailedLogin)
                {
                    retValue = true;
                    return retValue;
                }

                try
                {
                    this.CreateDateTime = record.TimeCreated.Value;
                    this.TimeCreated = this.CreateDateTime.ToString();
                }
                catch (Exception exConv) { }
                //L.logger(location, "Time Created: " + this.TimeCreated + ", Create Date Time: " + Convert.ToString(this.CreateDateTime), TAG.GENERAL);

                /*for (int i = 0; i < record.Properties.Count; i++) {
                    try
                    {
                        L.logger(location, "Index (" + i + "), Value (" + Convert.ToString(record.Properties[i].Value) + ").", TAG.GENERAL);
                    }
                    catch (Exception exConv) { }
                }*/

                if (record.Properties.Count > 0)
                    this.SubjectUserSid = Convert.ToString(record.Properties[0].Value);
                if (record.Properties.Count > 1)
                    this.SubjectUserName = Convert.ToString(record.Properties[1].Value);
                if (record.Properties.Count > 2)
                    this.SubjectDomainName = Convert.ToString(record.Properties[2].Value);
                if (record.Properties.Count > 3)
                    this.SubjectLogonId = Convert.ToString(record.Properties[3].Value);
                if (record.Properties.Count > 4)
                    this.TargetUserSid = Convert.ToString(record.Properties[4].Value);
                if (record.Properties.Count > 5)
                    this.TargetUserName = Convert.ToString(record.Properties[5].Value);
                if (record.Properties.Count > 6)
                    this.TargetDomainName = Convert.ToString(record.Properties[6].Value);
                /*if (record.Properties.Count > 7)
                    //this.TimeCreated = record.TimeCreated.ToString();
                    //this.CreateDateTime = Convert.ToDateTime(record.TimeCreated);
                    try
                    {
                        this.TimeCreated = Convert.ToString(Convert.ToInt64((uint)record.Properties[7].Value));
                        this.CreateDateTime = DateTime.Parse(Convert.ToInt64((uint)record.Properties[7].Value).ToString());
                        //this.CreateDateTime = Convert.ToDateTime(record.Properties[7].Value.ToString());
                    }
                    catch (Exception exConv) { }*/
                //this.TimeCreated = this.CreateDateTime.ToString();

                if (record.Properties.Count > 8)
                    this.LogonType = Convert.ToString(record.Properties[8].Value);

                if (record.Properties.Count > 19)
                {
                    this.IpAddress = Convert.ToString(record.Properties[19].Value);
                    //L.logger(location, "Missing IP Address!");
                }
                if (record.Properties.Count > 20)
                    this.IpPort = Convert.ToString(record.Properties[20].Value);

                if (!string.IsNullOrEmpty(this.TimeCreated))
                {
                    try
                    {
                        this.CreateDateTime = DateTime.Parse(this.TimeCreated);
                    }
                    catch (Exception ex) { }
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

        public bool fromJArray(JArray data)
        {
            const string location = CLASSNAME + ".fromJArray";
            bool retVal = false;
            try
            {
                if (data == null) return retVal;

                int idx = 0;
                if (data.Count > idx) this.isFailedLogin = U.getBool(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.FailedLoginEventId = U.getLong(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.IpEventId = U.getLong(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.IpBlockId = U.getLong(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.EventId = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.Version = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.Level = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.Task = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.Opcode = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.Keywords = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.TimeCreated = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.EventRecordId = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.Correlation = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.Execution = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.Channel = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.Computer = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.SubjectUserSid = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.SubjectUserName = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.SubjectDomainName = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.SubjectLogonId = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.TargetUserSid = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.TargetUserName = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.TargetDomainName = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.Status = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.FailureReason = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.SubStatus = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.LogonType = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.LogonProcessName = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.AuthenticationPackageName = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.WorkstationName = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.TransmittedServices = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.LmPackageName = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.KeyLength = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.ProcessId = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.ProcessName = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.IpAddress = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.IpPort = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.KeywordsDisplayNames = U.getString(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.CreateDateTime = U.getDate(data, idx);
                else return retVal;

                idx++;
                if (data.Count > idx) this.Latency = U.getDouble(data, idx);
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

        public bool fromJObject(JObject data)
        {
            const string location = CLASSNAME + ".fromJArray";
            bool retVal = false;
            try
            {
                if (data == null) return retVal;

                this.isFailedLogin = U.getBool(data, "isFailedLogin", false);
                this.FailedLoginEventId = U.getLong(data, "FailedLoginEventId", 0L);
                this.IpEventId = U.getLong(data, "IpEventId", 0L);
                this.IpBlockId = U.getLong(data, "IpBlockId", 0L);
                this.EventId = U.getString(data, "EventId", "");
                this.Version = U.getString(data, "Version", "");
                this.Level = U.getString(data, "Level", "");
                this.Task = U.getString(data, "Task", "");
                this.Opcode = U.getString(data, "Opcode", "");
                this.Keywords = U.getString(data, "Keywords", "");
                this.TimeCreated = U.getString(data, "TimeCreated", "");
                this.EventRecordId = U.getString(data, "EventRecordId", "");
                this.Correlation = U.getString(data, "Correlation", "");
                this.Execution = U.getString(data, "Execution", "");
                this.Channel = U.getString(data, "Channel", "");
                this.Computer = U.getString(data, "Computer", "");
                this.SubjectUserSid = U.getString(data, "SubjectUserSid", "");
                this.SubjectUserName = U.getString(data, "SubjectUserName", "");
                this.SubjectDomainName = U.getString(data, "SubjectDomainName", "");
                this.SubjectLogonId = U.getString(data, "SubjectLogonId", "");
                this.TargetUserSid = U.getString(data, "TargetUserSid", "");
                this.TargetUserName = U.getString(data, "TargetUserName", "");
                this.TargetDomainName = U.getString(data, "TargetDomainName", "");
                this.Status = U.getString(data, "Status", "");
                this.FailureReason = U.getString(data, "FailureReason", "");
                this.SubStatus = U.getString(data, "SubStatus", "");
                this.LogonType = U.getString(data, "LogonType", "");
                this.LogonProcessName = U.getString(data, "LogonProcessName", "");
                this.AuthenticationPackageName = U.getString(data, "AuthenticationPackageName", "");
                this.WorkstationName = U.getString(data, "WorkstationName", "");
                this.TransmittedServices = U.getString(data, "TransmittedServices", "");
                this.LmPackageName = U.getString(data, "LmPackageName", "");
                this.KeyLength = U.getString(data, "KeyLength", "");
                this.ProcessId = U.getString(data, "ProcessId", "");
                this.ProcessName = U.getString(data, "ProcessName", "");
                this.IpAddress = U.getString(data, "IpAddress", "");
                this.IpPort = U.getString(data, "IpPort", "");
                this.KeywordsDisplayNames = U.getString(data, "KeywordsDisplayNames", "");
                this.CreateDateTime = U.getDate(data, "CreateDateTime");
                this.Latency = U.getDouble(data, "Latency", 0d);

                // Qualify result
                if (this.FailedLoginEventId > 0 && this.Keywords != null && this.Keywords.Length > 0)
                {
                    retVal = true;
                }
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

                writer.Write(this.FailedLoginEventId);
                writer.Write(this.IpEventId);
                writer.Write(this.IpBlockId);
                writer.Write(this.isFailedLogin);
                writer.Write(this.EventId == null ? "" : this.EventId);
                writer.Write(this.Version == null ? "" : this.Version);
                writer.Write(this.Level == null ? "" : this.Level);
                writer.Write(this.Task == null ? "" : this.Task);
                writer.Write(this.Opcode == null ? "" : this.Opcode);
                writer.Write(this.Keywords == null ? "" : this.Keywords);
                writer.Write(this.TimeCreated == null ? "" : this.TimeCreated);
                writer.Write(this.EventRecordId == null ? "" : this.EventRecordId);
                writer.Write(this.Correlation == null ? "" : this.Correlation);
                writer.Write(this.Execution == null ? "" : this.Execution);
                writer.Write(this.Channel == null ? "" : this.Channel);
                writer.Write(this.Computer == null ? "" : this.Computer);
                writer.Write(this.SubjectUserSid == null ? "" : this.SubjectUserSid);
                writer.Write(this.SubjectUserName == null ? "" : this.SubjectUserName);
                writer.Write(this.SubjectDomainName == null ? "" : this.SubjectDomainName);
                writer.Write(this.SubjectLogonId == null ? "" : this.SubjectLogonId);
                writer.Write(this.TargetUserSid == null ? "" : this.TargetUserSid);
                writer.Write(this.TargetUserName == null ? "" : U.encodeString(this.TargetUserName));
                writer.Write(this.TargetDomainName == null ? "" : U.encodeString(this.TargetDomainName));
                writer.Write(this.Status == null ? "" : this.Status);
                writer.Write(this.FailureReason == null ? "" : this.FailureReason);
                writer.Write(this.SubStatus == null ? "" : this.SubStatus);
                writer.Write(this.LogonType == null ? "" : this.LogonType);
                writer.Write(this.LogonProcessName == null ? "" : this.LogonProcessName);
                writer.Write(this.AuthenticationPackageName == null ? "" : this.AuthenticationPackageName);
                writer.Write(this.WorkstationName == null ? "" : this.WorkstationName);
                writer.Write(this.TransmittedServices == null ? "" : this.TransmittedServices);
                writer.Write(this.LmPackageName == null ? "" : this.LmPackageName);
                writer.Write(this.KeyLength == null ? "" : this.KeyLength);
                writer.Write(this.ProcessId == null ? "" : this.ProcessId);
                writer.Write(this.ProcessName == null ? "" : this.ProcessName);
                writer.Write(this.IpAddress == null ? "" : U.encodeString(this.IpAddress));
                writer.Write(this.IpPort == null ? "" : this.IpPort);
                writer.Write(this.KeywordsDisplayNames == null ? "" : this.KeywordsDisplayNames);
                writer.Write(this.CreateDateTime == null || this.CreateDateTime == c.nDt ? "" : CreateDateTime.ToString(TAG.DTF));
                writer.Write(this.Latency);

                // Note:  This corrupts the writer if fails to complete once started

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

                data.Add(this.isFailedLogin);
                data.Add(this.FailedLoginEventId);
                data.Add(this.IpEventId);
                data.Add(this.IpBlockId);
                data.Add(this.EventId);
                data.Add(this.Version);
                data.Add(this.Level);
                data.Add(this.Task);
                data.Add(this.Opcode);
                data.Add(this.Keywords);
                data.Add(this.TimeCreated);
                data.Add(this.EventRecordId);
                data.Add(this.Correlation);
                data.Add(this.Execution);
                data.Add(this.Channel);
                data.Add(this.Computer);
                data.Add(this.SubjectUserSid);
                data.Add(this.SubjectUserName);
                data.Add(this.SubjectDomainName);
                data.Add(this.SubjectLogonId);
                data.Add(this.TargetUserSid);
                data.Add(this.TargetUserName);
                data.Add(this.TargetDomainName);
                data.Add(this.Status);
                data.Add(this.FailureReason);
                data.Add(this.SubStatus);
                data.Add(this.LogonType);
                data.Add(this.LogonProcessName);
                data.Add(this.AuthenticationPackageName);
                data.Add(this.WorkstationName);
                data.Add(this.TransmittedServices);
                data.Add(this.LmPackageName);
                data.Add(this.KeyLength);
                data.Add(this.ProcessId);
                data.Add(this.ProcessName);
                data.Add(this.IpAddress);
                data.Add(this.IpPort);
                data.Add(this.KeywordsDisplayNames);
                data.Add(this.CreateDateTime.ToString(TAG.DTF));
                data.Add(this.Latency);


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

                data.Add("isFailedLogin", this.isFailedLogin); // TODO - Decide on string conversion for bools
                data.Add("FailedLoginEventId", this.FailedLoginEventId);
                data.Add("IpEventId", this.IpEventId);
                data.Add("IpBlockId", this.IpBlockId);
                data.Add("EventId", this.EventId);
                data.Add("Version", this.Version);
                data.Add("Level", this.Level);
                data.Add("Task", this.Task);
                data.Add("Opcode", this.Opcode);
                data.Add("Keywords", this.Keywords);
                data.Add("TimeCreated", this.TimeCreated);
                data.Add("EventRecordId", this.EventRecordId);
                data.Add("Correlation", this.Correlation);
                data.Add("Execution", this.Execution);
                data.Add("Channel", this.Channel);
                data.Add("Computer", this.Computer);
                data.Add("SubjectUserSid", this.SubjectUserSid);
                data.Add("SubjectUserName", this.SubjectUserName);
                data.Add("SubjectDomainName", this.SubjectDomainName);
                data.Add("SubjectLogonId", this.SubjectLogonId);
                data.Add("TargetUserSid", this.TargetUserSid);
                data.Add("TargetUserName", this.TargetUserName);
                data.Add("TargetDomainName", this.TargetDomainName);
                data.Add("Status", this.Status);
                data.Add("FailureReason", this.FailureReason);
                data.Add("SubStatus", this.SubStatus);
                data.Add("LogonType", this.LogonType);
                data.Add("LogonProcessName", this.LogonProcessName);
                data.Add("AuthenticationPackageName", this.AuthenticationPackageName);
                data.Add("WorkstationName", this.WorkstationName);
                data.Add("TransmittedServices", this.TransmittedServices);
                data.Add("LmPackageName", this.LmPackageName);
                data.Add("KeyLength", this.KeyLength);
                data.Add("ProcessId", this.ProcessId);
                data.Add("ProcessName", this.ProcessName);
                data.Add("IpAddress", this.IpAddress);
                data.Add("IpPort", this.IpPort);
                data.Add("KeywordsDisplayNames", this.KeywordsDisplayNames);
                data.Add("CreateDateTime", this.CreateDateTime.ToString(TAG.DTF));
                data.Add("Latency", this.Latency);


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

                //data.Add("isFailedLogin", this.isFailedLogin); // TODO - Decide on string conversion for bools
                data.Add("FailedLoginEventId", this.FailedLoginEventId);
                data.Add("IpEventId", this.IpEventId);
                data.Add("IpBlockId", this.IpBlockId);
                data.Add("TimeCreated", this.TimeCreated);
                data.Add("IpAddress", this.IpAddress);
                data.Add("TargetUserName", this.TargetUserName);
                data.Add("TargetDomainName", this.TargetDomainName);
                //data.Add("EventId", this.EventId);
                //data.Add("Version", this.Version);
                //data.Add("Level", this.Level);
                //data.Add("Task", this.Task);
                //data.Add("Opcode", this.Opcode);
                data.Add("Keywords", this.Keywords);
                //data.Add("EventRecordId", this.EventRecordId);
                //data.Add("Correlation", this.Correlation);
                //data.Add("Execution", this.Execution);
                //data.Add("Channel", this.Channel);
                //data.Add("Computer", this.Computer);
                data.Add("LogonType", this.LogonType);
                data.Add("SubjectUserSid", this.SubjectUserSid);
                //data.Add("SubjectUserName", this.SubjectUserName);
                //data.Add("SubjectDomainName", this.SubjectDomainName);
                data.Add("SubjectLogonId", this.SubjectLogonId);
                data.Add("TargetUserSid", this.TargetUserSid);
                //data.Add("Status", this.Status);
                //data.Add("FailureReason", this.FailureReason);
                //data.Add("SubStatus", this.SubStatus);
                //data.Add("LogonProcessName", this.LogonProcessName);
                //data.Add("AuthenticationPackageName", this.AuthenticationPackageName);
                //data.Add("WorkstationName", this.WorkstationName);
                //data.Add("TransmittedServices", this.TransmittedServices);
                //data.Add("LmPackageName", this.LmPackageName);
                //data.Add("KeyLength", this.KeyLength);
                //data.Add("ProcessId", this.ProcessId);
                //data.Add("ProcessName", this.ProcessName);
                //data.Add("IpPort", this.IpPort);
                //data.Add("KeywordsDisplayNames", this.KeywordsDisplayNames);
                data.Add("CreateDateTime", this.CreateDateTime.ToString(TAG.DTF));
                //data.Add("Latency", this.Latency);


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
