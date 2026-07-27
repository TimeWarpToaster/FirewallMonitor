//Firewall Monitor v04
//(c) 2026 - TimeWarpToaster

//https://www.gnu.org/licenses/gpl-3.0.html

using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json.Linq;

namespace FWM_ClientViewer_03
{
    public class UName
    {
        public const string CLASSNAME = "UName";

        public long UNameId = 0L;
        public int Active = 0;
        public DateTime CreateDateTime = c.nDt;
        public string UserName = "";
        public long IpBlockId = 0L;
        public long IpId = 0L;
        public long Cnt = 0L;

        public UName()
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

        public UName(UName from, bool copyLists)
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
                this.UNameId = 0L;
                this.Active = 0;
                this.CreateDateTime = c.nDt;
                this.UserName = "";
                this.IpBlockId = 0L;
                this.IpId = 0L;
                this.Cnt = 0L;

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
                if (reader.BaseStream.Position < maxIdx) this.UNameId = reader.ReadInt64();
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
                        catch (Exception ex) { }
                    }
                }
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.UserName = U.decodeString(reader.ReadString());
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.IpBlockId = reader.ReadInt64();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.IpId = reader.ReadInt64();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.Cnt = reader.ReadInt64();
                else return retVal;

                // Flag success
                return true;
                retVal =
                    this.UNameId > 0 &&
                    this.IpId > 0 && // Require at-least an IpId, not too worried about block just now
                    this.UserName != null &&
                    this.UserName.Length > 0;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public bool fromClone(UName from, bool copyLists)
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

                this.UNameId = from.UNameId;
                this.Active = from.Active;
                this.CreateDateTime = from.CreateDateTime;
                this.UserName = from.UserName;
                this.IpBlockId = from.IpBlockId;
                this.IpId = from.IpId;
                this.Cnt = from.Cnt;

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
                if (idx < data.Count) this.UNameId = U.getLong(data, idx, 0L);
                else return retVal;

                idx++;
                if (idx < data.Count) this.Active = U.getInt(data, idx, 0);
                else return retVal;

                idx++;
                if (idx < data.Count) this.CreateDateTime = U.getDate(data, idx);
                else return retVal;

                idx++;
                if (idx < data.Count) this.UserName = U.getString(data, idx, "");
                else return retVal;

                idx++;
                if (idx < data.Count) this.IpBlockId = U.getLong(data, idx, 0L);
                else return retVal;

                idx++;
                if (idx < data.Count) this.IpId = U.getLong(data, idx, 0L);
                else return retVal;

                idx++;
                if (idx < data.Count) this.Cnt = U.getLong(data, idx, 0L);

                // Qualify result
                retVal = this.UNameId > 0 && this.IpId > 0 && this.UserName != null && this.UserName.Length > 0;
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

                this.UNameId = U.getLong(data, "UNameId", 0L);
                this.Active = U.getInt(data, "Active", 0);
                this.CreateDateTime = U.getDate(data, "CreateDateTime");
                this.UserName = U.getString(data, "UserName", "");
                this.IpBlockId = U.getLong(data, "IpBlockId", 0L);
                this.IpId = U.getLong(data, "IpId", 0L);
                this.Cnt = U.getLong(data, "Cnt", 0L);

                // Qualify result
                retVal = this.UNameId > 0 && this.IpId > 0 && this.UserName != null && this.UserName.Length > 0;
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


                writer.Write(this.UNameId);
                writer.Write(this.Active);
                writer.Write(this.CreateDateTime == null || this.CreateDateTime == c.nDt ? "" : CreateDateTime.ToString(TAG.DTF));
                writer.Write(U.encodeString(this.UserName));
                writer.Write(this.IpBlockId);
                writer.Write(this.IpId);
                writer.Write(this.Cnt);

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

                data.Add(this.UNameId);
                data.Add(this.Active);
                data.Add(this.CreateDateTime == null ? "" : this.CreateDateTime.ToString(TAG.DTF));
                data.Add(this.UserName);
                data.Add(this.IpBlockId);
                data.Add(this.IpId);
                data.Add(this.Cnt);

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

                data.Add("UNameId", this.UNameId);
                data.Add("Active", this.Active);
                data.Add("CreateDateTime", this.CreateDateTime == null ? "" : this.CreateDateTime.ToString(TAG.DTF));
                data.Add("UserName", this.UserName);
                data.Add("IpBlockId", this.IpBlockId);
                data.Add("IpId", this.IpId);
                data.Add("Cnt", this.Cnt);


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

                data.Add("UNameId", this.UNameId);
                //data.Add("Active", this.Active);
                data.Add("CreateDateTime", this.CreateDateTime == null ? "" : this.CreateDateTime.ToString(TAG.DTF));
                data.Add("UserName", this.UserName);
                data.Add("IpBlockId", this.IpBlockId);
                data.Add("IpId", this.IpId);
                data.Add("Cnt", this.Cnt);


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
