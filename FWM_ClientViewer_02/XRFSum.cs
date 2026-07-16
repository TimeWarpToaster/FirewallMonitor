using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json.Linq;

namespace FWM_ClientViewer_02
{
    public class XRFSum
    {
        public const string CLASSNAME = "XRFSum";

        public long XRFSumId = 0L;
        public long SummaryId = 0L;
        public long IpId = 0L;
        public long IpBlockId = 0L;
        public int Active = 0;
        public DateTime CreateDateTime = c.nDt;


        public XRFSum()
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

        public XRFSum(XRFSum from, bool copyLists)
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
            const string location = CLASSNAME + ".init";
            bool retVal = false;
            try
            {
                this.XRFSumId = 0L;
                this.SummaryId = 0L;
                this.IpId = 0L;
                this.IpBlockId = 0L;
                this.Active = 0;
                this.CreateDateTime = c.nDt;

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
                if (reader.BaseStream.Position < maxIdx) this.XRFSumId = reader.ReadInt64();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.SummaryId = reader.ReadInt64();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.IpId = reader.ReadInt64();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.IpBlockId = reader.ReadInt64();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.Active = reader.ReadInt32();
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

                // Flag result
                retVal = this.XRFSumId > 0 && this.SummaryId > 0;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public bool fromClone(XRFSum from, bool copyLists)
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
                    L.err(location, "Failed to initialize before clone.");
                    return retVal;
                }


                this.XRFSumId = from.XRFSumId;
                this.SummaryId = from.SummaryId;
                this.IpId = from.IpId;
                this.IpBlockId = from.IpBlockId;
                this.Active = from.Active;
                this.CreateDateTime = from.CreateDateTime;

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
                if (idx < data.Count) this.XRFSumId = U.getLong(data, idx, 0L);
                else return retVal;

                idx++;
                if (idx < data.Count) this.SummaryId = U.getLong(data, idx, 0L);
                else return retVal;

                idx++;
                if (idx < data.Count) this.IpId = U.getLong(data, idx, 0L);
                else return retVal;

                idx++;
                if (idx < data.Count) this.IpBlockId = U.getLong(data, idx, 0L);
                else return retVal;

                idx++;
                if (idx < data.Count) this.Active = U.getInt(data, idx, 0);
                else return retVal;

                idx++;
                if (idx < data.Count) this.CreateDateTime = U.getDate(data, idx);

                // Qualify result
                retVal = this.XRFSumId > 0 && this.IpId > 0;
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

                this.XRFSumId = U.getLong(data, "XRFSumId", 0L);
                this.SummaryId = U.getLong(data, "SummaryId", 0L);
                this.IpId = U.getLong(data, "IpId", 0L);
                this.IpBlockId = U.getLong(data, "IpBlockId", 0L);
                this.Active = U.getInt(data, "Active", 0);
                this.CreateDateTime = U.getDate(data, "CreateDateTime");

                // Qualify result
                retVal = this.XRFSumId > 0 && this.IpId > 0;
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

                writer.Write(this.XRFSumId);
                writer.Write(this.SummaryId);
                writer.Write(this.IpId);
                writer.Write(this.IpBlockId);
                writer.Write(this.Active);
                writer.Write(this.CreateDateTime == null || this.CreateDateTime == c.nDt ? "" : CreateDateTime.ToString(TAG.DTF));

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

                data.Add(this.XRFSumId);
                data.Add(this.SummaryId);
                data.Add(this.IpId);
                data.Add(this.IpBlockId);
                data.Add(this.Active);
                data.Add(this.CreateDateTime == null ? "" : this.CreateDateTime.ToString(TAG.DTF));

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

                data.Add("XRFSumId", this.XRFSumId);
                data.Add("SummaryId", this.SummaryId);
                data.Add("IpId", this.IpId);
                data.Add("IpBlockId", this.IpBlockId);
                data.Add("Active", this.Active);
                data.Add("CreateDateTime", this.CreateDateTime == null ? "" : this.CreateDateTime.ToString(TAG.DTF));

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
