using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FWM_ClientViewer_02
{
    public class AppSettingsBak
    {
        public const string CLASSNAME = "AppSettings";

        // Shared between apps
        public long appId = 0L;
        public string appGuid = "";

        public string baseDirectory = "";
        //public string pathApplication = "";
        //public string pathAppSettings = "";
        //public string pathFailedLoginEvent = "";
        //public string pathFWRow = "";
        //public string pathIpBlock = "";
        //public string pathIpEvent = "";
        //public string pathSummary = "";
        //public string pathUName = "";
        //public string pathXRFSum = "";

        public string fileNameApplication = "application.bin";
        public string fileNameAppSettings = "appsettings.bin";
        public string fileNameFailedLoginEvent = "datafile1.bin";
        public string fileNameFWRow = "datafile2.bin";
        public string fileNameIpBlock = "datafile3.bin";
        public string fileNameIpEvent = "datafile4.bin";
        public string fileNameSummary = "datafile5.bin";
        public string fileNameUName = "datafile6.bin";
        public string fileNameXRFSum = "datafile7.bin";


        // Viewer settings
        public string logPathViewer = "";


        // Client settings
        public string logPathClient = "";
        public bool debugModeClient = false;

        public int maxToProcess = 100000;
        public string instanceName = "FWMClient02";
		public bool allowMultiInstance = false;
        public string ApprovedIps = "";

        public string EventFolder = "";
        public string ArchiveFolder = "";
        public string ReportPath = "";
        public string ReportFilePrefix = "Rpt_";


        public bool IsManageFW = true;
        public int MinFailuresToBlock = 20;
		public string FWPrefix = "FWMRule";
        public int MSBetweenFWTestMin = 30;
		public int MSBetweenFWTestMax = 60;
		public int MSBetweenFWAddMin = 200;
		public int MSBetweenFWAddMax = 400;
		public int FWMinutesToReview = 10080;
		public int FWExpireAfterDays = 30;
		public string FWPort = "ANY";

        public string FWProtocol = "ANY";







        /*public AppSettings() 
        {
            const string location = CLASSNAME + ".AppSettings";
            try
            {
                
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }*/

        public bool init()
        {
            const string location = CLASSNAME + ".init";
            bool retVal = false;
            try
            {

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
                    return retVal;
                }

                long maxIdx = reader.BaseStream.Length - 1;


                // Shared between apps
                if (reader.BaseStream.Position < maxIdx) this.appId = reader.ReadInt64();
                else return retVal;

                if (reader.BaseStream.Position < maxIdx) this.appGuid = reader.ReadString();
                else return retVal;

                if (reader.BaseStream.Position < maxIdx) this.baseDirectory = reader.ReadString();
                else return retVal;
                //public string pathApplication = "";
                //public string pathAppSettings = "";
                //public string pathFailedLoginEvent = "";
                //public string pathFWRow = "";
                //public string pathIpBlock = "";
                //public string pathIpEvent = "";
                //public string pathSummary = "";
                //public string pathUName = "";
                //public string pathXRFSum = "";

                if (reader.BaseStream.Position < maxIdx) this.fileNameApplication = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.fileNameAppSettings = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.fileNameFailedLoginEvent = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.fileNameFWRow = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.fileNameIpBlock = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.fileNameIpEvent = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.fileNameSummary = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.fileNameUName = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.fileNameXRFSum = reader.ReadString();
                else return retVal;


                // Viewer settings
                if (reader.BaseStream.Position < maxIdx) this.logPathViewer = reader.ReadString();
                else return retVal;


                // Client settings
                if (reader.BaseStream.Position < maxIdx) this.logPathClient = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.debugModeClient = reader.ReadBoolean();
                else return retVal;

                if (reader.BaseStream.Position < maxIdx) this.maxToProcess = reader.ReadInt32();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.instanceName = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.allowMultiInstance = reader.ReadBoolean();
                else return retVal;
                if (reader.BaseStream.Position<maxIdx) this.ApprovedIps = reader.ReadString();
                else return retVal;

                if (reader.BaseStream.Position < maxIdx) this.EventFolder = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.ArchiveFolder = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.ReportPath = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.ReportFilePrefix = reader.ReadString();
                else return retVal;


                if (reader.BaseStream.Position < maxIdx) this.IsManageFW = reader.ReadBoolean();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.MinFailuresToBlock = 20;
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.FWPrefix = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.MSBetweenFWTestMin = reader.ReadInt32();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.MSBetweenFWTestMax = reader.ReadInt32();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.MSBetweenFWAddMin = reader.ReadInt32();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.MSBetweenFWAddMax = reader.ReadInt32();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.FWMinutesToReview = reader.ReadInt32();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.FWExpireAfterDays = reader.ReadInt32();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.FWPort = reader.ReadString();
                else return retVal;
                if (reader.BaseStream.Position < maxIdx) this.FWProtocol = reader.ReadString();
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


                // Shared between apps
                writer.Write(this.appId);
                writer.Write(this.appGuid);

                writer.Write(this.baseDirectory);

                writer.Write(this.fileNameApplication);
                writer.Write(this.fileNameAppSettings);
                writer.Write(this.fileNameFailedLoginEvent);
                writer.Write(this.fileNameFWRow);
                writer.Write(this.fileNameIpBlock);
                writer.Write(this.fileNameIpEvent);
                writer.Write(this.fileNameSummary);
                writer.Write(this.fileNameUName);
                writer.Write(this.fileNameXRFSum);


                // Viewer settings
                writer.Write(this.logPathViewer);


                // Client settings
                writer.Write(this.logPathClient);
                writer.Write(this.debugModeClient);

                writer.Write(this.maxToProcess);
                writer.Write(this.instanceName);
                writer.Write(this.allowMultiInstance);
                writer.Write(this.ApprovedIps);

                writer.Write(this.EventFolder);
                writer.Write(this.ArchiveFolder);
                writer.Write(this.ReportPath);
                writer.Write(this.ReportFilePrefix);


                writer.Write(this.IsManageFW);
                writer.Write(this.MinFailuresToBlock);
                writer.Write(this.FWPrefix);
                writer.Write(this.MSBetweenFWTestMin);
                writer.Write(this.MSBetweenFWTestMax);
                writer.Write(this.MSBetweenFWAddMin);
                writer.Write(this.MSBetweenFWAddMax);
                writer.Write(this.FWMinutesToReview);
                writer.Write(this.FWExpireAfterDays);
                writer.Write(this.FWPort);

                writer.Write(this.FWProtocol);

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
