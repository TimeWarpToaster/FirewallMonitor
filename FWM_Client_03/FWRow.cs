using System;
using System.IO;

using Newtonsoft.Json.Linq;

namespace FWM_Client_03
{
    public class FWRow
    {
        public const string CLASSNAME = "FWRow";

        public long FWId { get; set; }

        public DateTime CreateDateTime { get; set; }
        public int Active { get; set; }
        public long IpId { get; set; }
        public long IpBlockId { get; set; }
        public string FWName { get; set; }
        public DateTime ActiveDate { get; set; }
        public DateTime Expiry { get; set; }// Automatically remove the rule after (console)
        public bool Expired { get; set; }// Flag a ruled was removed from firewall 
        public DateTime Deactivated { get; set; }// Flag user removed the rule, do not recreate
        public long TimesRefreshed { get; set; }
        public string Protocol { get; set; }
        public string IpAddress { get; set; }
        public string Port { get; set; }



        public bool init()
        {
            const string location = CLASSNAME + ".init";
            bool retVal = false;
            try
            {
                this.FWId = 0L;
                this.CreateDateTime = c.nDt;
                this.Active = 0;
                this.IpId = 0L;
                this.IpBlockId = 0L;
                this.FWName = "";
                this.ActiveDate = c.nDt;
                this.Expiry = c.nDt;
                this.Expired = false;
                this.Deactivated = c.nDt;
                this.TimesRefreshed = 0L;
                this.Protocol = "";
                this.IpAddress = "";
                this.Port = "";

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

                if (reader.BaseStream.Position < maxIdx) this.FWId = reader.ReadInt64();
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
                if (reader.BaseStream.Position < maxIdx) this.Active = reader.ReadInt32();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.IpId = reader.ReadInt64();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.IpBlockId = reader.ReadInt64();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.FWName = U.decodeString(reader.ReadString());
                else return retVal;
                this.ActiveDate = c.nDt;
                if (reader.BaseStream.Position < maxIdx)
                {
                    string dtString = reader.ReadString();
                    if (dtString != null && dtString.Length > 0)
                    {
                        try
                        {
                            this.ActiveDate = DateTime.Parse(dtString);
                        }
                        catch (Exception exConv) { }
                    }
                }
                else return retVal;
                this.Expiry = c.nDt;
                if (reader.BaseStream.Position < maxIdx)
                {
                    string dtString = reader.ReadString();
                    if (dtString != null && dtString.Length > 0)
                    {
                        try
                        {
                            this.Expiry = DateTime.Parse(dtString);
                        }
                        catch (Exception exConv) { }
                    }
                }
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.Expired = reader.ReadBoolean();
                else return retVal;
                this.Deactivated = c.nDt;
                if (reader.BaseStream.Position < maxIdx)
                {
                    string dtString = reader.ReadString();
                    //L.l(location, "Deactivated (" + this.Deactivated.ToString(TAG.DTF) + "), From file (" + dtString + ").");
                    if (dtString != null && dtString.Length > 0)
                    {
                        try
                        {
                            this.Deactivated = DateTime.Parse(dtString);
                        }
                        catch (Exception exConv) { }
                    }
                }
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.TimesRefreshed = reader.ReadInt64();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.Protocol = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.IpAddress = U.decodeString(reader.ReadString());
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.Port = reader.ReadString();
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

        public bool fromClone(FWRow from, bool copyLists)
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

                this.FWId = from.FWId;

                this.CreateDateTime = from.CreateDateTime;
                this.Active = from.Active;
                this.IpId = from.IpId;
                this.IpBlockId = from.IpBlockId;
                this.FWName = from.FWName;
                this.ActiveDate = from.ActiveDate;
                this.Expiry = from.Expiry;
                this.Expired = from.Expired;
                this.Deactivated = from.Deactivated;
                this.TimesRefreshed = from.TimesRefreshed;
                this.Protocol = from.Protocol;
                this.IpAddress = from.IpAddress;
                this.Port = from.Port;

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
                if (idx < data.Count) this.FWId = U.getLong(data, idx, 0L);
                else return retVal;

                idx++;
                if (idx < data.Count) this.CreateDateTime = U.getDate(data, idx);
                else return retVal;

                idx++;
                if (idx < data.Count) this.Active = U.getInt(data, idx, 0);
                else return retVal;

                idx++;
                if (idx < data.Count) this.IpId = U.getLong(data, idx, 0L);
                else return retVal;

                idx++;
                if (idx < data.Count) this.IpBlockId = U.getLong(data, idx, 0L);
                else return retVal;

                idx++;
                if (idx < data.Count) this.FWName = U.getString(data, idx, "");
                else return retVal;

                idx++;
                if (idx < data.Count) this.ActiveDate = U.getDate(data, idx);
                else return retVal;

                idx++;
                if (idx < data.Count) this.Expiry = U.getDate(data, idx);
                else return retVal;

                idx++;
                if (idx < data.Count) this.Expired = U.getBool(data, idx);
                else return retVal;

                idx++;
                if (idx < data.Count) this.Deactivated = U.getDate(data, idx);
                else return retVal;

                idx++;
                if (idx < data.Count) this.TimesRefreshed = U.getLong(data, idx, 0L);
                else return retVal;

                idx++;
                if (idx < data.Count) this.Protocol = U.getString(data, idx, "");
                else return retVal;

                idx++;
                if (idx < data.Count) this.IpAddress = U.getString(data, idx, "");
                else return retVal;

                idx++;
                if (idx < data.Count) this.Port = U.getString(data, idx, "");
                else return retVal;

                // Qualify result
                retVal = this.FWId > 0 && this.IpId > 0 && this.FWName != null && this.FWName.Length > 0;
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

                this.FWId = U.getLong(data, "FWId", 0L);
                this.CreateDateTime = U.getDate(data, "CreateDateTime");
                this.Active = U.getInt(data, "Active", 0);
                this.IpId = U.getLong(data, "IpId", 0L);
                this.IpBlockId = U.getLong(data, "IpBlockId", 0L);
                this.FWName = U.getString(data, "FWName", "");
                this.ActiveDate = U.getDate(data, "ActiveDate");
                this.Expiry = U.getDate(data, "Expiry");
                this.Expired = U.getBool(data, "Expired", false);
                this.Deactivated = U.getDate(data, "Deactivated");
                this.TimesRefreshed = U.getLong(data, "TimesRefreshed", 0L);
                this.Protocol = U.getString(data, "Protocol", "");
                this.IpAddress = U.getString(data, "IpAddress", "");
                this.Port = U.getString(data, "Port", "");

                // Qualify result
                retVal = this.FWId > 0 && this.IpId > 0 && this.FWName != null && this.FWName.Length > 0;
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

                writer.Write(this.FWId);
                writer.Write(this.CreateDateTime == null || this.CreateDateTime == c.nDt ? "" : CreateDateTime.ToString(TAG.DTF));
                writer.Write(this.Active);
                writer.Write(this.IpId);
                writer.Write(this.IpBlockId);
                writer.Write(this.FWName == null ? "" : U.encodeString(this.FWName));
                writer.Write(this.ActiveDate == null || this.ActiveDate == c.nDt ? "" : this.ActiveDate.ToString(TAG.DTF));
                writer.Write(this.Expiry == null || this.Expiry == c.nDt ? "" : this.Expiry.ToString(TAG.DTF));
                writer.Write(this.Expired);
                writer.Write(this.Deactivated == null || this.Deactivated == c.nDt ? "" : this.Deactivated.ToString(TAG.DTF));
                writer.Write(this.TimesRefreshed);
                writer.Write(this.Protocol == null ? "" : this.Protocol);
                writer.Write(this.IpAddress == null ? "" : U.encodeString(this.IpAddress));
                writer.Write(this.Port == null ? "" : this.Port);

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

                data.Add(this.FWId);
                data.Add(this.CreateDateTime.ToString(TAG.DTF));
                data.Add(this.Active);
                data.Add(this.IpId);
                data.Add(this.IpBlockId);
                data.Add(this.FWName);
                data.Add(this.ActiveDate.ToString(TAG.DTF));
                data.Add(this.Expiry.ToString(TAG.DTF));
                data.Add(this.Expired);
                data.Add(this.Deactivated.ToString(TAG.DTF));
                data.Add(this.TimesRefreshed);
                data.Add(this.Protocol);
                data.Add(this.IpAddress);
                data.Add(this.Port);

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

                data.Add("FWId", this.FWId);
                data.Add("CreateDateTime", this.CreateDateTime.ToString(TAG.DTF));
                data.Add("Active", this.Active);
                data.Add("IpId", this.IpId);
                data.Add("IpBlockId", this.IpBlockId);
                data.Add("FWName", this.FWName);
                data.Add("ActiveDate", this.ActiveDate.ToString(TAG.DTF));
                data.Add("Expiry", this.Expiry.ToString(TAG.DTF));
                data.Add("Expired", this.Expired);
                data.Add("Deactivated", this.Deactivated.ToString(TAG.DTF));
                data.Add("TimesRefreshed", this.TimesRefreshed);
                data.Add("Protocol", this.Protocol);
                data.Add("IpAddress", this.IpAddress);
                data.Add("Port", this.Port);

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
