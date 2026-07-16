using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FWM_ClientViewer_02
{
    public static class FileMgr
    {
        const string CLASSNAME = "FileMgr";

        private const string pathAppSettings = @".\Data\appSettings.bin";

        private static string pathApplication = @"D:\FWM\FWMGhostData\application.bin";
        private static string pathFailedLoginEvents = @"D:\FWM\FWMGhostData\datafile1.bin";
        private static string pathFWRow = @"D:\FWM\FWMGhostData\datafile2.bin";
        private static string pathIpBlock = @"D:\FWM\FWMGhostData\datafile3.bin";
        private static string pathIpEvent = @"D:\FWM\FWMGhostData\datafile4.bin";
        private static string pathSummary = @"D:\FWM\FWMGhostData\datafile5.bin";
        private static string pathUName = @"D:\FWM\FWMGhostData\datafile6.bin";
        private static string pathXRFSum = @"D:\FWM\FWMGhostData\datafile7.bin";

        // Locks
        private static Mutex mutexAppSettings = null;
        private static Mutex mutexFWRows = null;


        public class MyFile
        {
            public const string CLASSNAME = "MyFile";

            // Generall supplied fields
            public string objectType = "";
            public string path = "";

            // Header fields
            public string ApiVersion = "";
            public string CreateDateTime = "";
            public string AccessDateTime = "";
            public long CntRead = 0;
            public long CntWritten = 0;

            // Data
            public List<object> data = new List<object>();



            public MyFile()
            {
                const string location = CLASSNAME + ".Constructor";
                try
                {
                    if (!this.init(false))
                    {
                        L.err(location, "Failed to initialize object.");
                    }
                }
                catch (Exception ex)
                {
                    L.ex(location, ex);
                }
            }


            public bool init(bool headerFieldsOnly)
            {
                const string location = CLASSNAME + ".init";
                bool retVal = false;
                try
                {
                    // Generall supplied fields
                    if (!headerFieldsOnly)
                    {
                        this.objectType = "";
                        this.path = "";
                    }

                    // Header fields
                    this.ApiVersion = "";
                    this.CreateDateTime = "";
                    this.AccessDateTime = "";
                    this.CntRead = 0;
                    this.CntWritten = 0;

                    // Data
                    if (!headerFieldsOnly)
                    {
                        this.data = new List<object>();
                    }

                    // Flag success for completing
                    retVal = true;
                }
                catch (Exception ex)
                {
                    L.ex(location, ex);
                }
                return retVal;
            }

            // fromFileHeader - Advances the reader position to end of header data
            public bool fromFileHeader(ref BinaryReader reader)
            {
                const string location = CLASSNAME + ".fromFileHeader";
                bool retVal = false;
                try
                {
                    if (reader == null)
                    {
                        L.err(location, "Reader was null when reading header.");
                        return retVal; //Early Exit
                    }

                    // Only reads header fields from file
                    if (!this.init(true))
                    {
                        L.err(location, "Failed to preinitialize object data.");
                        return retVal; //Early Exit
                    }

                    this.ApiVersion = reader.ReadString();
                    this.CreateDateTime = reader.ReadString();
                    this.AccessDateTime = reader.ReadString();
                    this.CntRead = reader.ReadInt64();
                    this.CntWritten = reader.ReadInt64();

                    // Flag success for completing
                    retVal = true;
                }
                catch (Exception ex)
                {
                    L.ex(location, ex);
                }
                return retVal;
            }

            public bool toFileHeader(ref BinaryWriter writer)
            {
                const string location = CLASSNAME + ".toFileHeader";
                bool retVal = false;
                try
                {
                    if (writer == null)
                    {
                        L.err(location, "Writer was null at header output.");
                        return retVal; //Early Exit
                    }

                    writer.Write(this.ApiVersion);
                    writer.Write(this.CreateDateTime);
                    writer.Write(this.AccessDateTime);
                    writer.Write(this.CntRead);
                    writer.Write(this.CntWritten);

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


        // Build IpBlock
        // Get IpEvents for block
        // Build IpEvent
        // Get failed logins for ip
        // Build failed login event


        public static bool setPathApplication(string path)
        {
            if (path != null && path.Length > 0)
            {
                FileMgr.pathApplication = @path;
                return true;
            }
            return false;
        }
        public static bool setPathFailedLoginEvents(string path)
        {
            if (path != null && path.Length > 0)
            {
                FileMgr.pathFailedLoginEvents = @path;
                return true;
            }
            return false;
        }
        public static bool setPathFWRow(string path)
        {
            if (path != null && path.Length > 0)
            {
                FileMgr.pathFWRow = @path;
                return true;
            }
            return false;
        }
        public static bool setPathIpBlock(string path)
        {
            if (path != null && path.Length > 0)
            {
                FileMgr.pathIpBlock = @path;
                return true;
            }
            return false;
        }
        public static bool setPathIpEvent(string path)
        {
            if (path != null && path.Length > 0)
            {
                FileMgr.pathIpEvent = @path;
                return true;
            }
            return false;
        }
        public static bool setPathSummary(string path)
        {
            if (path != null && path.Length > 0)
            {
                FileMgr.pathSummary = @path;
                return true;
            }
            return false;
        }
        public static bool setPathUName(string path)
        {
            if (path != null && path.Length > 0)
            {
                FileMgr.pathUName = @path;
                return true;
            }
            return false;
        }
        public static bool setPathXRFSum(string path)
        {
            if (path != null && path.Length > 0)
            {
                FileMgr.pathXRFSum = @path;
                return true;
            }
            return false;
        }


        public static bool lockAppSettings(int secondsToWait)
        {
            const string location = CLASSNAME + ".lockFile";
            bool retVal = false;
            try
            {
                if (FileMgr.pathAppSettings == null || FileMgr.pathAppSettings.Length == 0)
                {
                    L.err(location, "Path not set.");
                    return retVal;
                }
                if (FileMgr.mutexAppSettings != null)
                {
                    // It will be up to caller to retry
                    L.err(location, "Path was already locked.");
                    return retVal;
                }

                string tempName = U.toBase64(FileMgr.pathAppSettings);
                if (tempName.Length == 0)
                {
                    L.err(location, "Failed to format file-lock.");
                    return retVal;
                }
                tempName = "Global\\" + tempName;

                bool createdNew = false;
                Mutex mutex = new Mutex(false, tempName, out createdNew);

                bool hasLock = mutex.WaitOne(1000 * secondsToWait);
                if (hasLock)
                {
                    // Flag success
                    FileMgr.mutexAppSettings = mutex;
                    retVal = true;
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static bool lockFWRows(int secondsToWait)
        {
            const string location = CLASSNAME + ".lockFWRows";
            bool retVal = false;
            try
            {
                if (FileMgr.pathFWRow == null || FileMgr.pathFWRow.Length == 0)
                {
                    L.err(location, "Path not set.");
                    return retVal;
                }
                if (FileMgr.mutexFWRows != null)
                {
                    // It will be up to caller to retry
                    L.err(location, "Path was already locked.");
                    return retVal;
                }

                string tempName = U.toBase64(FileMgr.pathFWRow);
                if (tempName.Length == 0)
                {
                    L.err(location, "Failed to format file-lock.");
                    return retVal;
                }
                tempName = "Global\\" + tempName;

                bool createdNew = false;
                Mutex mutex = new Mutex(false, tempName, out createdNew);

                bool hasLock = mutex.WaitOne(1000 * secondsToWait);
                if (hasLock)
                {
                    // Flag success
                    FileMgr.mutexFWRows = mutex;
                    retVal = true;
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static bool unlockAppSettings()
        {
            const string location = CLASSNAME + ".unlockAppSettings";
            bool retVal = false;
            try
            {
                if (FileMgr.pathAppSettings == null || FileMgr.pathAppSettings.Length == 0)
                {
                    L.err(location, "Path not set.");
                    return retVal;
                }
                if (FileMgr.mutexAppSettings == null)
                {
                    // It will be up to caller to retry
                    L.err(location, "Lock is not found.");
                    return retVal;
                }
                try
                {
                    FileMgr.mutexAppSettings.ReleaseMutex();
                    FileMgr.mutexAppSettings = null;
                    retVal = true;
                }
                catch (Exception exAppException)
                {
                    L.err(location, "Failed to release file-lock with error: " + exAppException.Message);
                }
            }
            catch (Exception ex) 
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static bool unlockFWRows()
        {
            const string location = CLASSNAME + ".unlockFWRows";
            bool retVal = false;
            try
            {
                if (FileMgr.pathFWRow == null || FileMgr.pathFWRow.Length == 0)
                {
                    L.err(location, "Path not set.");
                    return retVal;
                }
                if (FileMgr.mutexFWRows == null)
                {
                    // It will be up to caller to retry
                    L.err(location, "Lock is not found.");
                    return retVal;
                }
                try
                {
                    FileMgr.mutexFWRows.ReleaseMutex();
                    FileMgr.mutexFWRows = null;
                    retVal = true;
                }
                catch (Exception exAppException) 
                {
                    L.err(location, "Failed to release file-lock with error: " + exAppException.Message);
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }


        public static int writeListObject(List<object> rows, string objectType)
        {
            const string location = CLASSNAME + ".writeListObject";
            int retVal = 0;
            try
            {
                if (rows == null)
                {
                    L.err(location, "Input rows were null.");
                    return retVal; //Early Exit
                }
                if (rows.Count == 0)
                {
                    L.err(location, "Input rows were empty.");
                    return retVal; //Early Exit
                }

                if (objectType == null || objectType.Length == 0)
                {
                    L.err(location, "Input type was null or empty.");
                    return retVal; //Early Exit
                }


                // TODO - Decide whether returning on empty is valid here
                if (rows == null || rows.Count == 0)
                {
                    L.err(location, "Input rows were null or empty.");
                    return retVal; //Early Exit
                }


                // Create MyFile to hold header data

                // TODO - Fill header dynamically
                MyFile myFile = new MyFile();
                myFile.objectType = objectType;
                myFile.ApiVersion = "1";
                myFile.CreateDateTime = DateTime.Now.AddDays(-20).ToString(TAG.DTF);
                myFile.AccessDateTime = DateTime.Now.ToString(TAG.DTF);
                myFile.CntRead = 0;
                myFile.CntWritten = 1;

                // Select the path based on object type
                switch (myFile.objectType)
                {
                    case "FailedLoginEvent": myFile.path = @pathFailedLoginEvents; break;
                    case "FWRow": myFile.path = @pathFWRow; break;
                    case "IpBlock": myFile.path = @pathIpBlock; break;
                    case "IpEvent": myFile.path = @pathIpEvent; break;
                    case "Summary": myFile.path = @pathSummary; break;
                    case "UName": myFile.path = @pathUName; break;
                    case "XRFSum": myFile.path = @pathXRFSum; break;
                    default:
                        {
                            L.err(location, "Unknown object type (" + objectType + ").");
                        }
                        break;
                }
                if (myFile.path == null || myFile.path.Length == 0)
                {
                    L.err(location, "Failed to identify path for (" + objectType + ").");
                    return retVal; //Early Exit
                }

                StringBuilder sbErrors = new StringBuilder();
                BinaryWriter writer = null;
                using (writer = new BinaryWriter(File.Open(@myFile.path, FileMode.Create)))
                {
                    try
                    {
                        // Get file header
                        if (!myFile.toFileHeader(ref writer))
                        {
                            L.err(location, "Failed to read header from file.");

                            // TODO - Need to hard abort here
                        }

                        // Read data into output
                        // Iterate all objects
                        for (int i = 0; i < rows.Count; i++)
                        {
                            if (rows[i] == null)
                            {
                                if (sbErrors.Length > 0) sbErrors.Append(", ");
                                sbErrors.Append(i);
                                L.err(location, "Skipping null row at index (" + i + ").");
                                continue; //Loop
                            }

                            // Write base upon object type
                            bool success = false;
                            switch (objectType)
                            {
                                case "FailedLoginEvent": success = ((FailedLoginEvent)rows[i]).toBinary(ref writer); break;
                                case "FWRow": success = ((FWRow)rows[i]).toBinary(ref writer); break;
                                case "IpBlock": success = ((IpBlock)rows[i]).toBinary(ref writer); break;
                                case "IpEvent": success = ((IpEvent)rows[i]).toBinary(ref writer); break;
                                case "Summary": success = ((Summary)rows[i]).toBinary(ref writer); break;
                                case "UName": success = ((UName)rows[i]).toBinary(ref writer); break;
                                case "XRFSum": success = ((XRFSum)rows[i]).toBinary(ref writer); break;
                                default:
                                    {
                                        L.err(location, "Unknown object type (" + objectType + ").");
                                    }
                                    break;
                            }

                            if (!success)
                            {
                                if (sbErrors.Length > 0) sbErrors.Append(", ");
                                sbErrors.Append(i);
                                continue; //Loop
                            }

                            // Increment success count
                            retVal++;
                        }
                    }
                    catch (Exception exBin)
                    {
                        L.err(location, "Binary write error: " + exBin.Message);
                    }
                }
                if (sbErrors.Length > 0)
                {
                    L.err(location, "Indexes encountering errors: " + sbErrors.ToString());
                }
                sbErrors.Length = 0;
                sbErrors = null;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static List<object> readListObject(string objectType)
        {
            const string location = CLASSNAME + ".readListObject";
            List<object> retVal = new List<object>();
            try
            {
                // Validat object type
                if (objectType == null || objectType.Length == 0)
                {
                    L.err(location, "Input type was null or empty.");
                    return retVal; //Early Exit
                }

                // Start a MyFile object to hold ids and header data
                MyFile myFile = new MyFile();
                myFile.objectType = objectType;

                // Get a correct path
                switch (objectType)
                {
                    case "FailedLoginEvent": myFile.path = @pathFailedLoginEvents; break;
                    case "FWRow": myFile.path = @pathFWRow; break;
                    case "IpBlock": myFile.path = @pathIpBlock; break;
                    case "IpEvent": myFile.path = @pathIpEvent; break;
                    case "Summary": myFile.path = @pathSummary; break;
                    case "UName": myFile.path = @pathUName; break;
                    case "XRFSum": myFile.path = @pathXRFSum; break;
                    default:
                        {
                            L.err(location, "Unknown object type (" + objectType + ").");
                        }
                        break;
                }
                if (myFile.path == null || myFile.path.Length == 0)
                {
                    L.err(location, "Failed to identify path from object (" + objectType + ").");
                    return retVal; //Early Exit
                }
                //L.l(location, "Reading path: " + myFile.path);

                // This is a read-only operation, abort if file does not exist
                if (!File.Exists(@myFile.path))
                {
                    L.l(location, "File does not exist for (" + objectType + ").");
                    return retVal;
                }


                // Begin reading file
                List<object> output = new List<object>();
                StringBuilder sbErrors = new StringBuilder();
                BinaryReader reader = null;
                using (reader = new BinaryReader(File.Open(@myFile.path, FileMode.Open)))
                {
                    try
                    {
                        // Get file header
                        if (!myFile.fromFileHeader(ref reader))
                        {
                            L.err(location, "Failed to read header from file.");

                        }

                        // Read data into output
                        while (reader.BaseStream.Position < reader.BaseStream.Length - 1)
                        {
                            bool success = false;
                            switch (objectType)
                            {
                                case "FailedLoginEvent":
                                    {
                                        FailedLoginEvent obj = new FailedLoginEvent();
                                        success = obj.fromBinary(ref reader);
                                        if (success) output.Add(obj);
                                    }
                                    break;
                                case "FWRow":
                                    {
                                        FWRow obj = new FWRow();
                                        success = obj.fromBinary(ref reader);
                                        if (success) output.Add(obj);
                                    }
                                    break;
                                case "IpBlock":
                                    {
                                        IpBlock obj = new IpBlock();
                                        success = obj.fromBinary(ref reader);
                                        if (success) output.Add(obj);
                                    }
                                    break;
                                case "IpEvent":
                                    {
                                        IpEvent obj = new IpEvent();
                                        success = obj.fromBinary(ref reader);
                                        if (success) output.Add(obj);
                                    }
                                    break;
                                case "Summary":
                                    {
                                        Summary obj = new Summary();
                                        success = obj.fromBinary(ref reader);
                                        if (success) output.Add(obj);
                                    }
                                    break;
                                case "UName":
                                    {
                                        UName obj = new UName();
                                        success = obj.fromBinary(ref reader);
                                        if (success) output.Add(obj);
                                        else L.l(location, "Failed UName from file.");
                                    }
                                    break;
                                case "XRFSum":
                                    {
                                        XRFSum obj = new XRFSum();
                                        success = obj.fromBinary(ref reader);
                                        if (success) output.Add(obj);
                                    }
                                    break;
                                default:
                                    {
                                        L.err(location, "Unknown object type (" + objectType + ").");
                                    }
                                    break;
                            }
                        }
                    }
                    catch (Exception exBin)
                    {
                        L.err(location, "Binary read error: " + exBin.Message);
                    }
                }

                if (sbErrors.Length > 0)
                {
                    L.err(location, "Binary read index errors: " + sbErrors.ToString());
                }

                // Push results to caller
                myFile.data = output;
                retVal = myFile.data;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }


        public static string readAppSettings()
        {
            const string location = CLASSNAME + ".readAppSettings";
            string retVal = "";
            try
            {
                // Only read and return raw
                using (BinaryReader reader = new BinaryReader(File.Open(FileMgr.pathAppSettings, FileMode.Open)))
                {
                    try
                    {
                        retVal = reader.ReadString();
                    }
                    catch (Exception exWriter)
                    {
                        L.err(location, "File writer failed with error: " + exWriter.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static long writeAppSettings(string inVal)
        {
            const string location = CLASSNAME + ".writeAppSettings";
            long retVal = 0L;
            try
            {
                // Expect formatted data, write as-is
                using (BinaryWriter writer = new BinaryWriter(File.Open(FileMgr.pathAppSettings, FileMode.Create)))
                {
                    try
                    {
                        writer.Write(inVal);
                        retVal = inVal.Length;
                    }
                    catch (Exception exWriter)
                    {
                        L.err(location, "Writer encountered error: " + exWriter.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static List<FailedLoginEvent> readFailedLoginEvents()
        {
            const string location = CLASSNAME + ".readFailedLoginEvents";
            List<FailedLoginEvent> retVal = new List<FailedLoginEvent>();
            try
            {
                // Take out header data (read to row start)


                // Read rows
                List<object> rows = FileMgr.readListObject("FailedLoginEvent");
                if (rows == null)
                {
                    L.err(location, "Rows from storage were null.");
                    return retVal; //Early Exit
                }
                else if (rows.Count == 0)
                {
                    //L.err(location, "Rows from storage were empty.");
                    return retVal;
                }

                // Move data to final object
                for (int i = 0; i < rows.Count; i++)
                {
                    retVal.Add((FailedLoginEvent)rows[i]);
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static List<FWRow> readFWRows()
        {
            const string location = CLASSNAME + ".readFWRows";
            List<FWRow> retVal = new List<FWRow>();
            try
            {
                // Take out header data (read to row start)


                // Read rows
                List<object> rows = FileMgr.readListObject("FWRow");
                if (rows == null)
                {
                    L.err(location, "Rows from storage were null.");
                    return retVal; //Early Exit
                }
                else if (rows.Count == 0)
                {
                    //L.err(location, "Rows from storage were empty.");
                    return retVal;
                }

                // Move data to final object
                for (int i = 0; i < rows.Count; i++)
                {
                    retVal.Add((FWRow)rows[i]);
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static List<IpBlock> readIpBlock()
        {
            const string location = CLASSNAME + ".readIpBlock";
            List<IpBlock> retVal = new List<IpBlock>();
            try
            {
                // Take out header data (read to row start)


                // Read rows
                List<object> rows = FileMgr.readListObject("IpBlock");
                if (rows == null)
                {
                    L.err(location, "Rows from storage were null.");
                    return retVal; //Early Exit
                }
                else if (rows.Count == 0)
                {
                    //L.err(location, "Rows from storage were empty.");
                    return retVal;
                }

                // Move data to final object
                for (int i = 0; i < rows.Count; i++)
                {
                    retVal.Add((IpBlock)rows[i]);
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static List<IpEvent> readIpEvent()
        {
            const string location = CLASSNAME + ".readIpEvent";
            List<IpEvent> retVal = new List<IpEvent>();
            try
            {
                // Take out header data (read to row start)


                // Read rows
                List<object> rows = FileMgr.readListObject("IpEvent");
                if (rows == null)
                {
                    L.err(location, "Rows from storage were null.");
                    return retVal; //Early Exit
                }
                else if (rows.Count == 0)
                {
                    //L.err(location, "Rows from storage were empty.");
                    return retVal;
                }

                // Move data to final object
                for (int i = 0; i < rows.Count; i++)
                {
                    retVal.Add((IpEvent)rows[i]);
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static List<Summary> readSummary()
        {
            const string location = CLASSNAME + ".readSummary";
            List<Summary> retVal = new List<Summary>();
            try
            {
                // Read rows
                List<object> rows = FileMgr.readListObject("Summary");
                if (rows == null)
                {
                    L.err(location, "Rows from storage were null.");
                    return retVal; //Early Exit
                }
                else if (rows.Count == 0)
                {
                    //L.err(location, "Rows from storage were empty.");
                    return retVal;
                }

                // Move data to final object
                for (int i = 0; i < rows.Count; i++)
                {
                    retVal.Add((Summary)rows[i]);
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static DataMgr.FWMSystem readSystemFile()
        {
            const string location = CLASSNAME + ".readSystemFile";
            DataMgr.FWMSystem retVal = null;
            try
            {
                // TODO - Read system file into memory object




            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static List<UName> readUNames()
        {
            const string location = CLASSNAME + ".readUNames";
            List<UName> retVal = new List<UName>();
            try
            {
                // Read rows
                List<object> rows = FileMgr.readListObject("UName");
                if (rows == null)
                {
                    L.err(location, "Rows from storage were null.");
                    return retVal; //Early Exit
                }
                else if (rows.Count == 0)
                {
                    //L.err(location, "Rows from storage were empty.");
                    return retVal;
                }

                // Move data to final object
                for (int i = 0; i < rows.Count; i++)
                {
                    retVal.Add((UName)rows[i]);
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static List<XRFSum> readXRFSum()
        {
            const string location = CLASSNAME + ".readXRFSum";
            List<XRFSum> retVal = new List<XRFSum>();
            try
            {
                // Read rows
                List<object> rows = FileMgr.readListObject("XRFSum");
                if (rows == null)
                {
                    L.err(location, "Rows from storage were null.");
                    return retVal; //Early Exit
                }
                else if (rows.Count == 0)
                {
                    //L.err(location, "Rows from storage were empty.");
                    return retVal;
                }

                // Move data to final object
                for (int i = 0; i < rows.Count; i++)
                {
                    retVal.Add((XRFSum)rows[i]);
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static int writeFailedLoginEvents(List<FailedLoginEvent> rows)
        {
            const string location = CLASSNAME + ".writeFailedLoginEvents";
            int retVal = 0;
            try
            {
                // TODO - Decide whether returning on empty is valid here
                if (rows == null || rows.Count == 0)
                {
                    L.err(location, "Input rows were null or empty.");
                    return retVal; //Early Exit
                }
                List<object> objects = new List<object>();
                for (int i = 0; i < rows.Count; i++) objects.Add(rows[i]);
                retVal = FileMgr.writeListObject(objects, "FailedLoginEvent");

                L.l(location, "Wrote (" + retVal + ") objects.");
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static int writeFWRows(List<FWRow> rows)
        {
            const string location = CLASSNAME + ".writeFWRows";
            int retVal = 0;
            try
            {
                // TODO - Decide whether returning on empty is valid here
                if (rows == null || rows.Count == 0)
                {
                    L.err(location, "Input rows were null or empty.");
                    return retVal; //Early Exit
                }
                List<object> objects = new List<object>();
                for (int i = 0; i < rows.Count; i++) objects.Add(rows[i]);
                retVal = FileMgr.writeListObject(objects, "FWRow");

                L.l(location, "Wrote (" + retVal + ") objects.");
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static int writeIpBlock(List<IpBlock> rows)
        {
            const string location = CLASSNAME + ".writeIpBlock";
            int retVal = 0;
            try
            {
                // TODO - Decide whether returning on empty is valid here
                if (rows == null || rows.Count == 0)
                {
                    L.err(location, "Input rows were null or empty.");
                    return retVal; //Early Exit
                }
                List<object> objects = new List<object>();
                for (int i = 0; i < rows.Count; i++) objects.Add(rows[i]);
                retVal = FileMgr.writeListObject(objects, "IpBlock");

                L.l(location, "Wrote (" + retVal + ") objects.");
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static int writeIpEvent(List<IpEvent> rows)
        {
            const string location = CLASSNAME + ".writeIpEvent";
            int retVal = 0;
            try
            {
                // TODO - Decide whether returning on empty is valid here
                if (rows == null || rows.Count == 0)
                {
                    L.err(location, "Input rows were null or empty.");
                    return retVal; //Early Exit
                }
                List<object> objects = new List<object>();
                for (int i = 0; i < rows.Count; i++) objects.Add(rows[i]);
                retVal = FileMgr.writeListObject(objects, "IpEvent");

                L.l(location, "Wrote (" + retVal + ") objects.");
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static int writeSummary(List<Summary> rows)
        {
            const string location = CLASSNAME + ".writeSummary";
            int retVal = 0;
            try
            {
                // TODO - Decide whether returning on empty is valid here
                if (rows == null || rows.Count == 0)
                {
                    L.err(location, "Input rows were null or empty.");
                    return retVal; //Early Exit
                }
                List<object> objects = new List<object>();
                for (int i = 0; i < rows.Count; i++) objects.Add(rows[i]);
                retVal = FileMgr.writeListObject(objects, "Summary");

                L.l(location, "Wrote (" + retVal + ") objects.");
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static int writeUNames(List<UName> rows)
        {
            const string location = CLASSNAME + ".writeUNames";
            int retVal = 0;
            try
            {
                // TODO - Decide whether returning on empty is valid here
                if (rows == null || rows.Count == 0)
                {
                    L.err(location, "Input rows were null or empty.");
                    return retVal; //Early Exit
                }
                List<object> objects = new List<object>();
                for (int i = 0; i < rows.Count; i++) objects.Add(rows[i]);
                retVal = FileMgr.writeListObject(objects, "UName");

                L.l(location, "Wrote (" + retVal + ") objects.");
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static int writeXRFSum(List<XRFSum> rows)
        {
            const string location = CLASSNAME + ".writeXRFSum";
            int retVal = 0;
            try
            {
                // TODO - Decide whether returning on empty is valid here
                if (rows == null || rows.Count == 0)
                {
                    L.err(location, "Input rows were null or empty.");
                    return retVal; //Early Exit
                }
                List<object> objects = new List<object>();
                for (int i = 0; i < rows.Count; i++) objects.Add(rows[i]);
                retVal = FileMgr.writeListObject(objects, "XRFSum");

                L.l(location, "Wrote (" + retVal + ") objects.");
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }




        public static bool readAppData()
        {
            const string location = CLASSNAME + ".readAppData";
            bool retVal = false;
            try
            {
                if (!File.Exists(@pathApplication))
                {
                    L.err(location, "File did not exist for (AppData).");
                }
                else
                {
                    using (BinaryReader reader = new BinaryReader(File.Open(@pathApplication, FileMode.Open)))
                    {
                        try
                        {
                            long tempAppId = reader.ReadInt64();
                            string tempAppGuid = reader.ReadString();
                            string tempLastReadDate = U.decodeString(reader.ReadString());

                            if (tempAppId > 0)
                            {
                                U.appId = tempAppId;
                            }
                            if (tempAppGuid != null && tempAppGuid.Length > 0)
                            {
                                U.appGuid = tempAppGuid;
                            }
                            if (tempLastReadDate == null)
                            {
                                tempLastReadDate = "";
                            }

                            DateTime dtCheckTime = new DateTime();
                            DateTime dtLastRead = dtCheckTime;
                            if (tempLastReadDate.Length > 0)
                            {
                                try
                                {
                                    dtLastRead = DateTime.Parse(tempLastReadDate);
                                }
                                catch (Exception ex) { }
                            }
                            if (dtLastRead != null && dtLastRead != dtCheckTime)
                            {
                                U.sLastReadDate = tempLastReadDate;
                                U.LastReadDate = dtLastRead;
                            }
                            else
                            {
                                U.sLastReadDate = "";
                            }
                            L.l(location, "Last read date (" + U.sLastReadDate + ").");
                            retVal = true;
                        }
                        catch (Exception exReader)
                        {
                            L.err(location, "Reader failed with error: " + exReader.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static int writeAppData()
        {
            const string location = CLASSNAME + ".writeAppData";
            int retVal = 0;
            try
            {
                // For now, single row
                using (BinaryWriter writer = new BinaryWriter(File.Open(@pathApplication, FileMode.Create)))
                {
                    try
                    {
                        writer.Write(U.appId < 0 ? 0 : U.appId);
                        writer.Write(U.appGuid == null ? "" : U.appGuid);
                        writer.Write(U.sLastReadDate == null ? "" : U.encodeString(U.sLastReadDate));
                        retVal = 1;// int to be consistent with other writes
                    }
                    catch (Exception exWriter)
                    {
                        L.err(location, "Writer failed with error: " + exWriter.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }


        public static bool readAppDataBak()
        {
            const string location = CLASSNAME + ".readAppData";
            bool retVal = false;
            try
            {
                if (!File.Exists(@pathApplication))
                {
                    L.err(location, "App data did not exist in file.");
                }
                else
                {
                    using (BinaryReader reader = new BinaryReader(File.Open(@pathApplication, FileMode.Open)))
                    {
                        try
                        {
                            long tempAppId = reader.ReadInt32();
                            string tempAppGuid = reader.ReadString();

                            if (tempAppId > 0)
                            {
                                U.appId = tempAppId;
                            }
                            if (tempAppGuid != null && tempAppGuid.Length > 0)
                            {
                                U.appGuid = tempAppGuid;
                            }
                            retVal = true;
                        }
                        catch (Exception exReader)
                        {
                            L.err(location, "Reader failed with error: " + exReader.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static int writeAppDataBak()
        {
            const string location = CLASSNAME + ".writeAppData";
            int retVal = 0;
            try
            {
                // For now, single row
                using (BinaryWriter writer = new BinaryWriter(File.Open(@pathApplication, FileMode.Create)))
                {
                    try
                    {
                        writer.Write(U.appId < 0 ? 0 : U.appId);
                        writer.Write(U.appGuid == null ? "" : U.appGuid);
                        retVal = 1;// int to be consistent with other writes
                    }
                    catch (Exception exWriter)
                    {
                        L.err(location, "Writer failed with error: " + exWriter.Message);
                    }
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
