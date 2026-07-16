using System;
using System.IO;
using System.Text;

using Newtonsoft.Json.Linq;

namespace FWM_Client_02
{
    public class Summary
    {
        public const string CLASSNAME = "Summary";

        public long SummaryId = 0L;
        public DateTime CreateDateTime { get; set; }
        public int Active = 0;
        public long CntFaccess = 0L;
        public int CntIpBlocks = 0;
        public int CntIps = 0;
        public DateTime AppStartDT { get; set; }
        public DateTime AppEndDT { get; set; }
        public float ElapsedRead = 0f;
        public float ElapsedSort = 0f;
        public float ElapsedFilter = 0f;
        public float ElapsedReport = 0f;
        public float ElapsedFW = 0f;
        public float ElapsedEmail = 0f;
        public string Rpt = "";
        public int CntFWAdd = 0;
        public int CntFWAddFail = 0;
        public int CntFWExisted = 0;
        public int CntFWExpired = 0;
        public int CntFWExpireFail = 0;
        public int CntFWProcessed = 0;
        public StringBuilder FWAddIps = new StringBuilder();
        public StringBuilder FWIpAddFailures = new StringBuilder();
        public StringBuilder FWNewRules = new StringBuilder();
        public StringBuilder FWExisted = new StringBuilder();


        public Summary()
        {
            const string location = CLASSNAME + ".Constructor";
            try
            {
                if (!this.init())
                {
                    L.err(location, "Failed to initialize summary object!");
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
                this.SummaryId = 0L;
                this.CreateDateTime = c.nDt;
                this.Active = 1;
                this.CntFaccess = 0L;
                this.CntIpBlocks = 0;
                this.CntIps = 0;
                //this.AppStartDT
                //this.AppEndDT 
                this.ElapsedRead = 0f;
                this.ElapsedSort = 0f;
                this.ElapsedFilter = 0f;
                this.ElapsedReport = 0f;
                this.ElapsedFW = 0f;
                this.ElapsedEmail = 0f;
                this.Rpt = "";
                this.CntFWAdd = 0;
                this.CntFWAddFail = 0;
                this.CntFWExisted = 0;
                this.CntFWExpired = 0;
                this.CntFWExpireFail = 0;
                this.CntFWProcessed = 0;
                this.FWAddIps = new StringBuilder();
                this.FWIpAddFailures = new StringBuilder();
                this.FWNewRules = new StringBuilder();
                this.FWExisted = new StringBuilder();

                long updateId = DataMgr.updateSummary(this);
                if (updateId <= 0)
                {
                    L.err(location, "Failed to update Id (" + updateId + ").");
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
                if (reader.BaseStream.Position < maxIdx) this.SummaryId = reader.ReadInt64();
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
                if (reader.BaseStream.Position < maxIdx) this.Active = reader.ReadInt32();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.CntFaccess = reader.ReadInt64();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.CntIpBlocks = reader.ReadInt32();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.CntIps = reader.ReadInt32();
                else return retVal;
                this.AppStartDT = c.nDt;
                if (reader.BaseStream.Position < maxIdx)
                {
                    string dtString = reader.ReadString();
                    if (dtString != null && dtString.Length > 0)
                    {
                        try
                        {
                            this.AppStartDT = DateTime.Parse(dtString);
                        }
                        catch (Exception exConv) { }
                    }
                }
                else return retVal;
                this.AppEndDT = c.nDt;
                if (reader.BaseStream.Position < maxIdx)
                {
                    string dtString = reader.ReadString();
                    if (dtString != null && dtString.Length > 0)
                    {
                        try
                        {
                            this.AppEndDT = DateTime.Parse(dtString);
                        }
                        catch (Exception exConv) { }
                    }
                }
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.ElapsedRead = (float)reader.ReadDouble();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.ElapsedSort = (float)reader.ReadDouble();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.ElapsedFilter = (float)reader.ReadDouble();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.ElapsedReport = (float)reader.ReadDouble();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.ElapsedFW = (float)reader.ReadDouble();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.ElapsedEmail = (float)reader.ReadDouble();
                else return retVal;
                //if (reader.BaseStream.Position < maxIdx) this.Rpt = reader.ReadString();
                //else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.CntFWAdd = reader.ReadInt32();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.CntFWAddFail = reader.ReadInt32();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.CntFWExisted = reader.ReadInt32();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.CntFWExpired = reader.ReadInt32();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.CntFWExpireFail = reader.ReadInt32();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.CntFWProcessed = reader.ReadInt32();
                else return retVal;
                /*this.FWAddIps = new StringBuilder();
                if (reader.BaseStream.Position < maxIdx)
                {
                    string s = reader.ReadString();
                    if (s != null && s.Length > 0)
                    {
                        try
                        {
                            this.FWAddIps = new StringBuilder(s);
                        }
                        catch (Exception exConv) { }
                    }
                }
                else return retVal;
                this.FWIpAddFailures = new StringBuilder();
                if (reader.BaseStream.Position < maxIdx)
                {
                    string s = reader.ReadString();
                    if (s != null && s.Length > 0)
                    {
                        try
                        {
                            this.FWIpAddFailures = new StringBuilder(s);
                        }
                        catch (Exception exConv) { }
                    }
                }
                else return retVal;
                this.FWNewRules = new StringBuilder();
                if (reader.BaseStream.Position < maxIdx)
                {
                    string s = reader.ReadString();
                    if (s != null && s.Length > 0)
                    {
                        try
                        {
                            this.FWNewRules = new StringBuilder(s);
                        }
                        catch (Exception exConv) { }
                    }
                }
                else return retVal;
                this.FWExisted = new StringBuilder();
                if (reader.BaseStream.Position < maxIdx)
                {
                    string s = reader.ReadString();
                    if (s != null && s.Length > 0)
                    {
                        try
                        {
                            this.FWExisted = new StringBuilder(s);
                        }
                        catch (Exception exConv) { }
                    }
                }
                else return retVal;
                */

                // Flag success for completing
                retVal = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public bool fromClone(Summary from, bool copyLists)
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


                this.SummaryId = from.SummaryId;
                this.CreateDateTime = from.CreateDateTime;
                this.Active = from.Active;
                this.CntFaccess = from.CntFaccess;
                this.CntIpBlocks = from.CntIpBlocks;
                this.CntIps = from.CntIps;
                this.AppStartDT = from.AppStartDT;
                this.AppEndDT = from.AppEndDT;
                this.ElapsedRead = from.ElapsedRead;
                this.ElapsedSort = from.ElapsedSort;
                this.ElapsedFilter = from.ElapsedFilter;
                this.ElapsedReport = from.ElapsedReport;
                this.ElapsedFW = from.ElapsedFW;
                this.ElapsedEmail = from.ElapsedEmail;
                this.Rpt = from.Rpt;
                this.CntFWAdd = from.CntFWAdd;
                this.CntFWAddFail = from.CntFWAddFail;
                this.CntFWExisted = from.CntFWExisted;
                this.CntFWExpired = from.CntFWExpired;
                this.CntFWExpireFail = from.CntFWExpireFail;
                this.CntFWProcessed = from.CntFWProcessed;
                if (copyLists) this.FWAddIps = from.FWAddIps;
                else this.FWAddIps = new StringBuilder();
                if (copyLists) this.FWIpAddFailures = from.FWIpAddFailures;
                else this.FWIpAddFailures = new StringBuilder();
                if (copyLists) this.FWNewRules = from.FWNewRules;
                else this.FWNewRules = new StringBuilder();
                if (copyLists) this.FWExisted = from.FWExisted;
                else this.FWExisted = new StringBuilder();

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
                if (idx < data.Count) this.SummaryId = U.getLong(data, idx, 0L);
                else return retVal;

                idx++;
                if (idx < data.Count) this.CreateDateTime = U.getDate(data, idx);
                else return retVal;

                idx++;
                if (idx < data.Count) this.Active = U.getInt(data, idx, 0);
                else return retVal;

                idx++;
                if (idx < data.Count) this.CntFaccess = U.getLong(data, idx, 0L);
                else return retVal;

                idx++;
                if (idx < data.Count) this.CntIpBlocks = U.getInt(data, idx, 0);
                else return retVal;

                idx++;
                if (idx < data.Count) this.CntIps = U.getInt(data, idx, 0);
                else return retVal;

                idx++;
                if (idx < data.Count) this.AppStartDT = U.getDate(data, idx);
                else return retVal;

                idx++;
                if (idx < data.Count) this.AppEndDT = U.getDate(data, idx);
                else return retVal;

                idx++;
                if (idx < data.Count) this.ElapsedRead = (float)U.getDouble(data, idx, 0d);
                else return retVal;

                idx++;
                if (idx < data.Count) this.ElapsedSort = (float)U.getDouble(data, idx, 0d);
                else return retVal;

                idx++;
                if (idx < data.Count) this.ElapsedFilter = (float)U.getDouble(data, idx, 0d);
                else return retVal;

                idx++;
                if (idx < data.Count) this.ElapsedReport = (float)U.getDouble(data, idx, 0d);
                else return retVal;

                idx++;
                if (idx < data.Count) this.ElapsedFW = (float)U.getDouble(data, idx, 0d);
                else return retVal;

                idx++;
                if (idx < data.Count) this.ElapsedEmail = (float)U.getDouble(data, idx, 0d);
                else return retVal;

                idx++;
                if (idx < data.Count) this.Rpt = U.getString(data, idx, "");
                else return retVal;

                idx++;
                if (idx < data.Count) this.CntFWAdd = U.getInt(data, idx, 0);
                else return retVal;

                idx++;
                if (idx < data.Count) this.CntFWAddFail = U.getInt(data, idx, 0);
                else return retVal;

                idx++;
                if (idx < data.Count) this.CntFWExisted = U.getInt(data, idx, 0);
                else return retVal;

                idx++;
                if (idx < data.Count) this.CntFWExpired = U.getInt(data, idx, 0);
                else return retVal;

                idx++;
                if (idx < data.Count) this.CntFWExpireFail = U.getInt(data, idx, 0);
                else return retVal;

                idx++;
                if (idx < data.Count) this.CntFWProcessed = U.getInt(data, idx, 0);
                else return retVal;

                idx++;
                if (idx < data.Count) this.FWAddIps = new StringBuilder(U.getString(data, idx, ""));
                else return retVal;

                idx++;
                if (idx < data.Count) this.FWIpAddFailures = new StringBuilder(U.getString(data, idx, ""));
                else return retVal;

                idx++;
                if (idx < data.Count) this.FWNewRules = new StringBuilder(U.getString(data, idx, ""));
                else return retVal;

                idx++;
                if (idx < data.Count) this.FWExisted = new StringBuilder(U.getString(data, idx, ""));

                // Qualify result
                retVal = this.SummaryId > 0 && this.Rpt != null && this.Rpt.Length > 0;
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

                this.SummaryId = U.getLong(data, "SummaryId", 0L);
                this.CreateDateTime = U.getDate(data, "CreateDateTime");
                this.Active = U.getInt(data, "Active", 0);
                this.CntFaccess = U.getLong(data, "CntFaccess", 0L);
                this.CntIpBlocks = U.getInt(data, "CntIpBlocks", 0);
                this.CntIps = U.getInt(data, "CntIps", 0);
                this.AppStartDT = U.getDate(data, "AppStartDT");
                this.AppEndDT = U.getDate(data, "AppEndDT");
                this.ElapsedRead = (float)U.getDouble(data, "ElapsedRead", 0d);
                this.ElapsedSort = (float)U.getDouble(data, "ElapsedSort", 0d);
                this.ElapsedFilter = (float)U.getDouble(data, "ElapsedFilter", 0d);
                this.ElapsedReport = (float)U.getDouble(data, "ElapsedReport", 0d);
                this.ElapsedFW = (float)U.getDouble(data, "ElapsedFW", 0d);
                this.ElapsedEmail = (float)U.getDouble(data, "ElapsedEmail", 0d);
                this.Rpt = U.getString(data, "Rpt", "");
                this.CntFWAdd = U.getInt(data, "CntFWAdd", 0);
                this.CntFWAddFail = U.getInt(data, "CntFWAddFail", 0);
                this.CntFWExisted = U.getInt(data, "CntFWExisted", 0);
                this.CntFWExpired = U.getInt(data, "CntFWExpired", 0);
                this.CntFWExpireFail = U.getInt(data, "CntFWExpireFail", 0);
                this.CntFWProcessed = U.getInt(data, "CntFWProcessed", 0);
                this.FWAddIps = new StringBuilder(U.getString(data, "FWAddIps", ""));
                this.FWIpAddFailures = new StringBuilder(U.getString(data, "FWIpAddFailures", ""));
                this.FWNewRules = new StringBuilder(U.getString(data, "FWNewRules", ""));
                this.FWExisted = new StringBuilder(U.getString(data, "FWExisted", ""));

                // Qualify result
                retVal = this.SummaryId > 0 && this.Rpt != null && this.Rpt.Length > 0;
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

                writer.Write(this.SummaryId);
                writer.Write(this.CreateDateTime == null || this.CreateDateTime == c.nDt ? "" : CreateDateTime.ToString(TAG.DTF));
                writer.Write(this.Active);
                writer.Write(this.CntFaccess);
                writer.Write(this.CntIpBlocks);
                writer.Write(this.CntIps);
                writer.Write(this.AppStartDT == null ? "" : this.AppStartDT.ToString(TAG.DTF));
                writer.Write(this.AppEndDT == null ? "" : this.AppEndDT.ToString(TAG.DTF));
                writer.Write((double)this.ElapsedRead);
                writer.Write((double)this.ElapsedSort);
                writer.Write((double)this.ElapsedFilter);
                writer.Write((double)this.ElapsedReport);
                writer.Write((double)this.ElapsedFW);
                writer.Write((double)this.ElapsedEmail);
                //writer.Write(this.Rpt);
                writer.Write(this.CntFWAdd);
                writer.Write(this.CntFWAddFail);
                writer.Write(this.CntFWExisted);
                writer.Write(this.CntFWExpired);
                writer.Write(this.CntFWExpireFail);
                writer.Write(this.CntFWProcessed);
                /*writer.Write(this.FWAddIps == null ? "" : this.FWAddIps.ToString());
                writer.Write(this.FWIpAddFailures == null ? "" : this.FWIpAddFailures.ToString());
                writer.Write(this.FWNewRules == null ? "" : this.FWNewRules.ToString());
                writer.Write(this.FWExisted == null ? "" : this.FWExisted.ToString());*/

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

                data.Add(this.SummaryId);
                data.Add(this.CreateDateTime == null ? "" : this.CreateDateTime.ToString(TAG.DTF));
                data.Add(this.Active);
                data.Add(this.CntFaccess);
                data.Add(this.CntIpBlocks);
                data.Add(this.CntIps);
                data.Add(this.AppStartDT == null ? "" : this.AppStartDT.ToString(TAG.DTF));
                data.Add(this.AppEndDT == null ? "" : this.AppEndDT.ToString(TAG.DTF));
                data.Add(this.ElapsedRead);
                data.Add(this.ElapsedSort);
                data.Add(this.ElapsedFilter);
                data.Add(this.ElapsedReport);
                data.Add(this.ElapsedFW);
                data.Add(this.ElapsedEmail);
                data.Add(this.Rpt);
                data.Add(this.CntFWAdd);
                data.Add(this.CntFWAddFail);
                data.Add(this.CntFWExisted);
                data.Add(this.CntFWExpired);
                data.Add(this.CntFWExpireFail);
                data.Add(this.CntFWProcessed);
                data.Add(this.FWAddIps == null ? "" : this.FWAddIps.ToString());
                data.Add(this.FWIpAddFailures == null ? "" : this.FWIpAddFailures.ToString());
                data.Add(this.FWNewRules == null ? "" : this.FWNewRules.ToString());
                data.Add(this.FWExisted == null ? "" : this.FWExisted.ToString());

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

                data.Add("SummaryId", this.SummaryId);
                data.Add("CreateDateTime", this.CreateDateTime == null ? "" : this.CreateDateTime.ToString(TAG.DTF));
                data.Add("Active", this.Active);
                data.Add("CntFaccess", this.CntFaccess);
                data.Add("CntIpBlocks", this.CntIpBlocks);
                data.Add("CntIps", this.CntIps);
                data.Add("AppStartDT", this.AppStartDT == null ? "" : this.AppStartDT.ToString(TAG.DTF));
                data.Add("AppEndDT", this.AppEndDT == null ? "" : this.AppEndDT.ToString(TAG.DTF));
                data.Add("ElapsedRead", this.ElapsedRead);
                data.Add("ElapsedSort", this.ElapsedSort);
                data.Add("ElapsedFilter", this.ElapsedFilter);
                data.Add("ElapsedReport", this.ElapsedReport);
                data.Add("ElapsedFW", this.ElapsedFW);
                data.Add("ElapsedEmail", this.ElapsedEmail);
                data.Add("Rpt", this.Rpt == null ? "" : this.Rpt);
                data.Add("CntFWAdd", this.CntFWAdd);
                data.Add("CntFWAddFail", this.CntFWAddFail);
                data.Add("CntFWExisted", this.CntFWExisted);
                data.Add("CntFWExpired", this.CntFWExpired);
                data.Add("CntFWExpireFail", this.CntFWExpireFail);
                data.Add("CntFWProcessed", this.CntFWProcessed);
                data.Add("FWAddIps", this.FWAddIps == null ? "" : this.FWAddIps.ToString());
                data.Add("FWIpAddFailures", this.FWIpAddFailures == null ? "" : this.FWIpAddFailures.ToString());
                data.Add("FWNewRules", this.FWNewRules == null ? "" : this.FWNewRules.ToString());
                data.Add("FWExisted", this.FWExisted == null ? "" : this.FWExisted.ToString());

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
