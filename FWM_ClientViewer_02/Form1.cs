using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Newtonsoft.Json.Linq;

namespace FWM_ClientViewer_02
{
    public partial class Form1 : Form
    {
        public const string CLASSNAME = "Form1";

        public long minutesToScan = 43200; // day=1440, week=10080, 30d=43200
        DateTime dtStartScan { get; set; } // set at app init, update when querying

        List<Control> settingsInput = new List<Control>();

        JObject dataTreeJson = null;

        List<IpBlock> dataTreeBlocks = new List<IpBlock>();
        int idxDataTreeIpBlock = -1;// Specific to data tree selection
        int idxDataTreeIpEvent = -1;// Specific to data tree selection

        int dataTreePage = 0;
        int dataTreePageSize = 1000;


        public Form1()
        {
            const string location = CLASSNAME + ".Constructor";
            try
            {
                InitializeComponent();

                if (!initApp())
                {
                    L.err(location, "Failed to initialize application.");
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        public bool initApp()
        {
            const string location = CLASSNAME + ".initApp";
            bool retVal = false;
            try
            {
                this.dtStartScan = DateTime.Now.AddMinutes(-1 * minutesToScan);
                int cntCriticalErrors = 0;

                if (!FileMgr.lockAppSettings(60))
                {
                    // TODO - Message box to user letting them know app loading failed
                    MessageBox.Show("Application settings file was busy for too long. Please close and " +
                        "try loading when the console is not running.");
                }
                else
                {
                    if (!DataMgr.loadAppSettings())
                    {
                        L.err(location, "Failed to load application settings.");
                        cntCriticalErrors++;

                        // TODO - Decide on whether to halt all activity
                        // TODO - Decide what to do about LogPath and logging init
                    }
                    if (!FileMgr.unlockAppSettings())
                    {
                        L.err(location, "Failed to release lock on app settings file.");
                    }
                }

                // Enable logging
                if (!L.logInit(U.GetSetting("logPathViewer", ""), lbLogsOut, U.GetSetting("EnableFileLogging", true)))
                {
                    L.err(location, "Failed to fully initialize logging.");
                }

                // TODO - Application data may not have much context in a standalone, portable state.
                // Safest thing short-term, is probably to let it load empty and don't look at it.
                long tempAppId = U.GetSetting("appId", 0L);
                string tempAppGuid = U.GetSetting("appGuid", "");

                if (!FileMgr.readAppData())
                {
                    L.err(location, "Failed to read application data from file.");
                }

                // TODO - Figure out if writing is still needed, now that sets are gone, does it need to exist?
                /*if (FileMgr.writeAppData() <= 0)
                {
                    L.err(location, "Failed to save application data.");
                }*/

                // Load data from storage
                DateTime dtLoadDataStart = DateTime.Now;
                if (!DataMgr.loadAllData(true))
                {
                    L.err(location, "Failed to load some data from storage.");
                }
                double elapsedLoadAllData = (DateTime.Now - dtLoadDataStart).TotalMilliseconds;
                L.l(location, "Took (" + elapsedLoadAllData + ") ms to load all data from file.");

                // Set date times for viewing data
                DateTime dtTemp = DateTime.Now;
                dtpQueryEnd.Value = dtTemp;
                dtpQueryStart.Value = dtTemp.AddDays(-30);

                btnDataPageLower.Text = "\u25C0";
                btnDataPageHigher.Text = "\u25B6";


                // Display firewall rules from storage at app boot
                tabsMain.SelectedTab = tabFirewallRules;
                if (!showFirewallRulesAll())
                {
                    L.err(location, "Failed to display firewall rules at app boot.");
                }

                if (!loadSettingsTab())
                {
                    L.err(location, "Failed to load settings tab.");
                }

                // Log Errors
                if (cntCriticalErrors > 0)
                {
                    L.err(location, "Encountered (" + cntCriticalErrors + ") critical errors.");
                }

                // Flag success for no critical errors
                retVal = cntCriticalErrors == 0;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public bool configureListView(ListView lv)
        {
            const string location = CLASSNAME + ".configureListView";
            bool retVal = false;
            try
            {
                if (lv.InvokeRequired)
                {
                    lv.Invoke(new Action(() => this.configureListView(lv)));
                }
                else 
                {
                    // Configure ListView
                    lv.Items.Clear();
                    lv.Columns.Clear();
                    lv.View = View.Details;
                    lv.FullRowSelect = true;
                    lv.GridLines = true;
                }
                retVal = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }



        private bool dataTreeViewClear()
        {
            const string location = CLASSNAME + ".dataTreeViewClear";
            bool retVal = false;
            try
            {
                if (treeDataView.InvokeRequired)
                {
                    treeDataView.Invoke(new Action(() => this.dataTreeViewClear()));
                }
                else
                {
                    treeDataView.BeginUpdate();
                    treeDataView.Nodes.Clear();
                    treeDataView.EndUpdate();
                }
                retVal = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        private bool dataTreeViewSet(TreeNode[] nodes)
        {
            const string location = CLASSNAME + ".dataTreeViewSet";
            bool retVal = false;
            try
            {
                if (nodes == null)
                {
                    L.err(location, "Tree data was null at set.");
                }
                if (nodes.Length == 0)
                {
                    L.l(location, "There is nothing to show in the tree.");
                }
                else
                {
                    if (treeDataView.InvokeRequired)
                    {
                        treeDataView.Invoke(new Action(() => this.dataTreeViewSet(nodes)));
                    }
                    else
                    {
                        treeDataView.BeginUpdate();
                        treeDataView.Nodes.AddRange(nodes);
                        treeDataView.EndUpdate();
                    }
                }
                retVal = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        private bool dataTreeSelect(TreeViewEventArgs args)
        {
            const string location = CLASSNAME + ".dataTreeSelect";
            bool retVal = false;
            try
            {
                this.idxDataTreeIpBlock = -1;
                this.idxDataTreeIpEvent = -1;

                // Validate Input
                long itemId = 0;
                try
                {
                    itemId = (long)args.Node.Tag;
                }
                catch (Exception exConv)
                {
                    L.err(location, "Error converting item id: " + exConv.Message);
                }

                if (itemId <= 0)
                {
                    L.err(location, "Selected tree item id (" + itemId + ") was invalid.");
                    return retVal; //Early Exit
                }
                L.l(location, "Searching for item id (" + itemId + ").");

                // Validate Storage
                if (dataTreeJson == null)
                {
                    L.err(location, "Tree view data was null when handling click.");
                    return retVal; //Early Exit
                }

                if (this.dataTreeBlocks == null)
                {
                    L.err(location, "Query data was not loaded when selecting tree item.");
                    return retVal; //Early Exit
                }

                // Identify item from tree JSON in memory
                JArray blocks = (JArray)dataTreeJson["IpBlocks"];
                for (int idxBlock = 0; idxBlock <= blocks.Count; idxBlock++)
                {
                    JObject block = (JObject)blocks[idxBlock];
                    if (itemId == (long)block["ItemId"])
                    {
                        // Found item, query block
                        L.l(location, "Querying block (" + block["BlockAddress"] +
                            "), id (" + block["IpBlockId"] + ").");

                        // Retrieve block data from native memory
                        long blockId = U.getLong(block, "IpBlockId", -1L);
                        if (blockId < 0)
                        {
                            L.err(location, "Block id was invalid.");
                            return retVal;
                        }
                        for (int i = 0; i < this.dataTreeBlocks.Count; i++)
                        {
                            if (this.dataTreeBlocks[i] == null) continue;
                            if (blockId != this.dataTreeBlocks[i].IpBlockId) continue;
                            this.idxDataTreeIpBlock = i;
                        }

                        // Validate block index
                        if (this.idxDataTreeIpBlock < 0)
                        {
                            L.err(location, "Failed to locate block id (" + blockId + ") in memory.");
                            return retVal;
                        }

                        if (!this.setDataPage(1))
                        {
                            L.err(location, "Failed to update ui with block ips.");
                            return retVal;
                        }

                        // Stop looping
                        retVal = true;
                        return retVal;
                    }

                    // Validate Ips storage
                    if (block["Ips"] == null)
                    {
                        L.err(location, "Ips for block (" + block["BlockAddress"] + ") were null.");
                        continue; //Loop
                    }

                    // Search block for a matching Ip
                    JArray ips = (JArray)block["Ips"];
                    for (int idxIp = 0; idxIp < ips.Count; idxIp++)
                    {
                        JObject ip = (JObject)ips[idxIp];
                        if (itemId == (long)ip["ItemId"])
                        {
                            // Found item, query Ip
                            L.l(location, "Querying ip (" + ip["IpAddress"] +
                                "), id (" + ip["IpEventId"] + ").");

                            // Validate id
                            long ipEventId = U.getLong(ip, "IpEventId", -1L);
                            if (ipEventId < 0)
                            {
                                L.err(location, "Ip id was invalid.");
                                return retVal;
                            }

                            // Find query indexes
                            for (int idxB = 0; idxB < this.dataTreeBlocks.Count; idxB++)
                            {
                                if (this.dataTreeBlocks[idxB] == null) continue;
                                if (this.dataTreeBlocks[idxB].IpEvents == null) continue;

                                List<IpEvent> ipRows = this.dataTreeBlocks[idxB].IpEvents;
                                for (int idxI = 0; idxI < ipRows.Count; idxI++)
                                {
                                    if (ipRows[idxI] == null) continue;
                                    if (ipEventId != ipRows[idxI].IpEventId) continue;

                                    // Set query indexes for other operations
                                    this.idxDataTreeIpBlock = idxB;
                                    this.idxDataTreeIpEvent = idxI;
                                    break;
                                }
                                // Stop looping if we found indexes
                                if (this.idxDataTreeIpBlock >= 0 && this.idxDataTreeIpEvent >= 0)
                                {
                                    break;
                                }
                            }
                            if (this.idxDataTreeIpBlock < 0)
                            {
                                L.err(location, "Failed to locate ip block in memory.");
                                return retVal;
                            }
                            if (this.idxDataTreeIpEvent < 0)
                            {
                                L.err(location, "Failed to locate ip in memory.");
                                return retVal;
                            }

                            // Flag success on UI set
                            retVal = this.setDataPage(1);
                            if (!retVal)
                            {
                                L.err(location, "Failed to update ui with ip events.");
                            }

                            // Stop looping
                            return retVal;
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

        private bool dataTreeSelectBlock(int pageNumberFromLabel)
        {
            const string location = CLASSNAME + ".dataTreeSelectBlock";
            bool retVal = false;
            try
            {
                // Validate input and memory
                if (this.dataTreeBlocks == null || this.dataTreeBlocks.Count == 0)
                {
                    L.err(location, "Data for filtering tree not loaded.");
                    return retVal;
                }
                if (this.idxDataTreeIpBlock < 0)
                {
                    L.err(location, "Block index was invalid.");
                    return retVal;
                }
                if (this.idxDataTreeIpBlock >= this.dataTreeBlocks.Count)
                {
                    L.err(location, "Block index was invalid.");
                    return retVal;
                }
                if (this.dataTreeBlocks[this.idxDataTreeIpBlock].IpEvents == null)
                {
                    L.err(location, "Block ips was null in memory.");
                    return retVal;
                }

                Thread thread = new Thread(() => {
                    const string locThread = CLASSNAME + ".dataTreeSelectBlock.thread";
                    try
                    {
                        long blockId = this.dataTreeBlocks[this.idxDataTreeIpBlock].IpBlockId;
                        L.l(location, "Loading ips for ip block id (" + blockId + ").");

                        this.dataTreePage = pageNumberFromLabel - 1;// Internal indexes are 0-based, label indexes are 1-based
                        int pageLimit = (this.dataTreePage * this.dataTreePageSize) + 1000;

                        // Iterate a subset of indexes, that represent the current page
                        JArray rowsOut = new JArray();
                        List<IpEvent> ips = this.dataTreeBlocks[this.idxDataTreeIpBlock].IpEvents;
                        for (
                            int idxIp = (this.dataTreePage * this.dataTreePageSize);
                            idxIp < ips.Count &&
                            idxIp < pageLimit;
                            idxIp++
                        )
                        {
                            JObject jrow = ips[idxIp].toJObjectForGrid();
                            if (jrow == null || jrow.Count == 0)
                            {
                                L.err(location, "Failed to convert row for grid at block id (" + blockId + ").");
                                continue;
                            }
                            rowsOut.Add(jrow);
                        }

                        if (!toListView(lvTreeDataRows, rowsOut))
                        {
                            L.err(location, "Failed to push rows to view.");
                        }
                    }
                    catch (Exception ex)
                    {
                        L.ex(locThread, ex);
                    }
                });

                thread.Start();

                // Flag success for starting thread
                retVal = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        private bool dataTreeSelectIp(long ipEventId, int pageNumberFromLabel)
        {
            const string location = CLASSNAME + ".dataTreeSelectIp";
            bool retVal = false;
            try
            {
                if (ipEventId < 0)
                {
                    L.err(location, "Ip id was invalid.");
                    return retVal;
                }
                if (this.dataTreeBlocks == null || this.dataTreeBlocks.Count == 0)
                {
                    L.err(location, "Block data not loaded when selecting ip.");
                    return retVal;
                }
                if (this.idxDataTreeIpBlock < 0)
                {
                    L.err(location, "Block index was invalid.");
                    return retVal;
                }
                if (this.idxDataTreeIpBlock >= this.dataTreeBlocks.Count)
                {
                    L.err(location, "Block index was invalid.");
                    return retVal;
                }
                if (this.dataTreeBlocks[this.idxDataTreeIpBlock].IpEvents == null)
                {
                    L.err(location, "Block ips was null in memory.");
                    return retVal;
                }
                if (this.idxDataTreeIpEvent < 0)
                {
                    L.err(location, "Ip index was invalid.");
                    return retVal;
                }
                if (this.idxDataTreeIpEvent >= this.dataTreeBlocks[this.idxDataTreeIpBlock].IpEvents.Count)
                {
                    L.err(location, "Ip index was invalid.");
                    return retVal;
                }
                if (this.dataTreeBlocks[this.idxDataTreeIpBlock].IpEvents[this.idxDataTreeIpEvent].FailedLogins == null)
                {
                    L.err(location, "Ip events was null.");
                    return retVal;
                }


                Thread thread = new Thread(() =>
                {
                    const string locThread = CLASSNAME + ".dataTreeSelectIpThread";
                    try
                    {
                        this.dataTreePage = pageNumberFromLabel - 1;// Internal pages are 0-index, labels are 1-index
                        int pageLimit = (this.dataTreePage * this.dataTreePageSize) + 1000;

                        List<FailedLoginEvent> events =
                            this.dataTreeBlocks[this.idxDataTreeIpBlock].IpEvents[this.idxDataTreeIpEvent].FailedLogins;

                        JArray rowsOut = new JArray();
                        for (
                            int idxEvent = (this.dataTreePage * this.dataTreePageSize);
                            idxEvent < events.Count &&
                            idxEvent < pageLimit;
                            idxEvent++
                        )
                        {
                            if (events[idxEvent] == null) continue; // Silently skip bad failed login rows

                            JObject jrow = events[idxEvent].toJObjectForGrid();
                            if (jrow == null || jrow.Count == 0)
                            {
                                //L.err(location, "Failed to convert ip for grid.");
                                continue;
                            }
                            rowsOut.Add(jrow);
                        }

                        if (!toListView(lvTreeDataRows, rowsOut))
                        {
                            L.err(location, "Failed to push rows to view.");
                        }
                    }
                    catch (Exception ex)
                    {
                        L.ex(locThread, ex);
                    }
                });

                thread.Start();

                // Flag success for starting thread
                retVal = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public bool enableDataQueryUI(bool enabled)
        {
            const string location = CLASSNAME + ".enableDataQueryUI";
            bool retVal = false;
            try
            {
                if (dtpQueryStart.InvokeRequired)
                {
                    dtpQueryStart.Invoke(new Action(() => { dtpQueryStart.Enabled = enabled; }));
                }
                else 
                {
                    dtpQueryStart.Enabled = enabled;
                }

                if (dtpQueryEnd.InvokeRequired)
                {
                    dtpQueryEnd.Invoke(new Action(() => { dtpQueryEnd.Enabled = enabled; }));
                }
                else 
                {
                    dtpQueryEnd.Enabled = enabled;
                }

                if (btnQuery.InvokeRequired)
                {
                    btnQuery.Invoke(new Action(() => { btnQuery.Enabled = enabled; }));
                }
                else 
                {
                    btnQuery.Enabled = enabled;
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

        public bool enableDataExportUI(bool enabled)
        {
            const string location = CLASSNAME + ".enableDataExportUI";
            bool retVal = false;
            try
            {
                if (btnDataExportCSV.InvokeRequired)
                {
                    btnDataExportCSV.Invoke(new Action(() => { btnDataExportCSV.Enabled = enabled; }));
                }
                else
                {
                    btnDataExportCSV.Enabled = enabled;
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

        public bool enableDataPageUI(bool enabled)
        {
            const string location = CLASSNAME + ".enableDataPageUI";
            bool retVal = false;
            try
            {
                if (btnDataPageLower != null)
                {
                    if (btnDataPageLower.InvokeRequired)
                    {
                        btnDataPageLower.Invoke(new Action(() => { btnDataPageLower.Enabled = enabled; }));
                    }
                    else
                    {
                        btnDataPageLower.Enabled = enabled;
                    }
                }

                if (btnDataPageHigher != null)
                {
                    if (btnDataPageHigher.InvokeRequired)
                    {
                        btnDataPageHigher.Invoke(new Action(() => { btnDataPageHigher.Enabled = enabled; }));
                    }
                    else 
                    {
                        btnDataPageHigher.Enabled = enabled;
                    }
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

        public bool enableManageFirewallUI(bool enabled)
        {
            const string location = CLASSNAME + ".enableManageFirewallUI";
            bool retVal = false;
            try
            {
                if (btnFirewallRefresh.InvokeRequired)
                {
                    btnFirewallRefresh.Invoke(new Action(() => { btnFirewallRefresh.Enabled = enabled; } ));
                }
                else 
                {
                    btnFirewallRefresh.Enabled = enabled;
                }

                if (btnExpireFirewallRules.InvokeRequired)
                {
                    btnExpireFirewallRules.Invoke(new Action(() => { btnExpireFirewallRules.Enabled = enabled; }));
                }
                else 
                {
                    btnExpireFirewallRules.Enabled = enabled;
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

        public bool expireFirewallRules()
        {
            const string location = CLASSNAME + ".expireFirewallRules";
            bool retVal = false;
            PopupProgress progress = null;
            ProgressUpdater progressUpdate = null;
            Form1 parentForm = null;
            try
            {
                // Disable the Firewall UI
                if (!enableManageFirewallUI(false))
                {
                    L.err(location, "Failed to disable firewall management UI.");
                }

                // Get selected items
                ListView.SelectedListViewItemCollection items = lvFirewallRules.SelectedItems;
                L.l(location, "Found (" + items.Count + ") selected items.");

                DateTime expiryTime = DateTime.Now;

                // Get column of id field
                int idxId = -1;
                int idxIpAddress = -1;
                for (int i = 0; i < lvFirewallRules.Columns.Count; i++)
                {
                    if (lvFirewallRules.Columns[i].Text == "FWId")
                    {
                        idxId = i;
                    }
                    if (lvFirewallRules.Columns[i].Text == "IpAddress")
                    {
                        idxIpAddress = i;
                    }
                }

                // Start a progress popup. Two things can tie up the operation, the file could
                // be busy, or there could be a large number to deactivate
                progress = 
                    new PopupProgress(
                        "Expiring Firewall Rules",
                        "Acquiring file-lock on firewall rules. This could take over a minute if the Client happens to be actively working a large backlog.",
                        0,
                        100
                    );
                progress.Show();

                progressUpdate = progress.getProgressUpdater();


                parentForm = this;
                this.Enabled = false;

                // Iterate selected items into ids and addresses
                List<object[]> listToExpire = new List<object[]>();
                for (int i = 0; i < items.Count; i++)
                {
                    ListViewItem.ListViewSubItemCollection subItems = items[i].SubItems;

                    object[] properties = new object[2] { 0, "" };

                    long fwId = 0L;
                    string ipAddress = "";
                    if (idxId < subItems.Count)
                    {
                        string tempId = Convert.ToString(subItems[idxId].Text);
                        try
                        {
                            fwId = Convert.ToInt64(tempId);
                            properties[0] = fwId;
                        }
                        catch (Exception exConv) { }
                    }
                    if (idxIpAddress < subItems.Count)
                    {
                        ipAddress = Convert.ToString(subItems[idxIpAddress].Text);
                        properties[1] = ipAddress;
                    }

                    if (properties[0] != null && (long)properties[0] > 0 && properties[1] != null && ((string)properties[1]).Length > 0)
                    {
                        listToExpire.Add(properties);
                    }
                }

                //L.l(location, "Found (" + listToExpire.Count + ") storage rows out-of (" + items.Count + ") items.");


                // Start a thread now, so we can lock the FWRows file before processing data
                Thread thread = new Thread(() =>
                {
                    try
                    {

                        // Lock firewall file
                        if (!FileMgr.lockFWRows(30))// Wait upto 30-seconds for a file-lock 
                        {
                            // TODO - Call out to a message box on failure
                            if (progressUpdate.messageUpdater != null)
                            {
                                this.Enabled = true;// Reenable parent form UI
                                progressUpdate.messageUpdater.Report("Failed to acquire lock on file (was busy). Try again after a few moments.");
                            }
                            progress.showOkayButton(true);

                            L.err(location, "Failed to acquire lock on firewall file.");
                            return; // TODO - Decide on throwing error, this can happen
                        }

                        // Update progress message
                        if (progressUpdate.messageUpdater != null)
                        {
                            progressUpdate.messageUpdater.Report("Reading latest data.");
                        }
                        if (progressUpdate.progressUpdater != null)
                        {
                            progressUpdate.progressUpdater.Report(75);// set 75%
                        }

                        // Get latest data, to see if rules are current
                        DataMgr.FWRows = FileMgr.readFWRows();
                        if (DataMgr.FWRows == null)
                        {
                            L.l(location, "Rules from storage were empty.");
                            DataMgr.FWRows = new List<FWRow>();
                        }

                        // Iterate selected items into expiry
                        List<FWRow> rowsToExpire = new List<FWRow>();
                        for (int i = 0; i < listToExpire.Count; i++)
                        {

                            long fwId = 0L;
                            string ipAddress = "";
                            if (listToExpire[i] != null)
                            {
                                if (listToExpire[i][0] != null)
                                {
                                    fwId = Convert.ToInt64(listToExpire[i][0]);
                                }
                                if (listToExpire[i][1] != null)
                                {
                                    ipAddress = Convert.ToString(listToExpire[i][1]);
                                }
                            }
                            L.l(location, "Removing FWId (" + fwId + "), for Ip Address (" + ipAddress + ").");

                            int idxFwRow = DataMgr.getFWIndex(fwId);
                            if (idxFwRow < 0)
                            {
                                L.err(location, "Invalid firewall row index was negative.");
                                continue; //Loop
                            }
                            if (idxFwRow >= DataMgr.FWRows.Count)
                            {
                                L.err(location, "Index (" + idxFwRow + ") was out of range (" + DataMgr.FWRows.Count + ").");
                                continue;
                            }
                            if (ipAddress != DataMgr.FWRows[idxFwRow].IpAddress)
                            {
                                L.err(location, "Data mismatch between grid and memory at fw id (" + fwId + ").");
                                continue;
                            }

                            rowsToExpire.Add(DataMgr.FWRows[idxFwRow]);
                        }

                        // Update progress message
                        //L.l(location, "Updating progress.");
                        if (progressUpdate.progressUpdater != null)
                        {
                            progressUpdate.progressUpdater.Report(100);
                            Thread.Sleep(500); // Let the load be seen for a moment
                        }

                        if (rowsToExpire.Count == 0)
                        {
                            L.l(location, "Nothing was selected to expire.");
                        }
                        else
                        {
                            // Expire Rules
                            //L.l(location, "Expiring (" + rowsToExpire.Count + ") rules.");// TODO - Remove log
                            int cntRowsExpired = Firewall.expireFirewallRules(ref rowsToExpire, progressUpdate);
                            if (cntRowsExpired <= 0)
                            {
                                L.err(location, "Failed to expire (" + rowsToExpire.Count + ") firewall rules.");
                            }
                            else
                            {
                                L.l(location, "Expired (" + cntRowsExpired + ") of (" + rowsToExpire.Count + ") firewall rules.");
                            }

                            // Write changes to storage
                            int cntWritten = FileMgr.writeFWRows(DataMgr.FWRows);
                            L.l(location, "Saved (" + cntWritten + ") rows to storage.");
                        }

                        // Unlock firewall file
                        if (!FileMgr.unlockFWRows())
                        {
                            L.err(location, "Failed to unlock firewall rows.");
                        }

                        // Reload firewall UI
                        if (!showFirewallRulesAll())
                        {
                            L.err(location, "Failed to show firewall rules.");
                        }

                        // Reenable firewall UI
                        if (!enableManageFirewallUI(true))
                        {
                            L.err(location, "Failed to reenable firewall UI.");
                        }

                        // Reenable parent form
                        if (parentForm != null)
                        {
                            if (parentForm.InvokeRequired)
                            {
                                parentForm.Invoke(new Action(() => { parentForm.Enabled = true; }));
                            }
                            else
                            {
                                parentForm.Enabled = true;
                            }
                        }

                        // Remove progress popup
                        bool foundPopup = false;
                        for (int i = 0; i < progress.Controls.Count; i++)
                        {
                            if (progress.Controls[i].Name == "btnPopupProgress1")
                            {
                                // Close the progess popup if "okay" button is not visible (no error)
                                if (progress.Controls[i].Visible == false)
                                {
                                    progress.closePopup();
                                    foundPopup = true;
                                }
                            }
                        }
                        if (!foundPopup)
                        {
                            // Close the popup if we could not find the button to evaluate
                            progress.closePopup();
                        }
                    }
                    catch (Exception ex)
                    {
                        L.ex(location + ".thread", ex);
                        try
                        {
                            if (!FileMgr.unlockFWRows())
                            {
                                // Do Nothing, we try whether it existed or not when error occurred
                            }
                        }
                        catch (Exception ex2)
                        {
                            // Do Nothing
                        }
                        try
                        {
                            // Reenable parent form
                            if (parentForm != null)
                            {
                                if (parentForm.InvokeRequired)
                                {
                                    parentForm.Invoke(new Action(() => { parentForm.Enabled = true; }));
                                }
                                else
                                {
                                    parentForm.Enabled = true;
                                }
                            }
                        }
                        catch (Exception ex2) 
                        {
                            L.err(location, "Failed to reenable parent form with error: " + ex2.Message);
                        }
                        try
                        {
                            if (progress != null)
                            {
                                progress.closePopup();
                            }
                        }
                        catch (Exception ex2) { }
                    }
                });
                thread.Start();


                // Flag success for starting thread
                retVal = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
                try
                {
                    // Reenable firewall UI
                    if (!enableManageFirewallUI(true))
                    {
                        L.err(location, "Failed to reenable firewall UI.");
                    }
                }
                catch (Exception exReenable)
                {
                    L.ex(location, exReenable);
                }
                try
                {
                    this.Enabled = true;
                }
                catch (Exception ex2) { }
            }
            return retVal;
        }

        public JObject getDataTreeJson(List<IpBlock> blocksIn)
        {
            const string location = CLASSNAME + ".getDataTreeJson";
            JObject retVal = new JObject();
            try
            {
                JArray blocksOut = new JArray();

                // ItemId is awkward. It needs to increment before each item, not after each loop (eg between nested).
                // Uniqueness matters, the number and order do not.
                int itemId = 0;
                for (int idxBlock = 0; idxBlock < blocksIn.Count; idxBlock++)
                {
                    itemId++;
                    JObject block = new JObject();
                    block["IpBlockId"] = blocksIn[idxBlock].IpBlockId;
                    block["BlockAddress"] = blocksIn[idxBlock].BlockAddress;
                    block["DisplayName"] = block["BlockAddress"] + 
                        "  (" + blocksIn[idxBlock].CntIps + ")  (" + blocksIn[idxBlock].CntFailedLogins + ")";
                    block["ItemId"] = itemId;

                    JArray ipsOut = new JArray();

                    List<IpEvent> ips = blocksIn[idxBlock].IpEvents;
                    for (int idxIp = 0; idxIp < ips.Count; idxIp++)
                    {
                        itemId++;
                        JObject ip = new JObject();
                        ip["IpEventId"] = ips[idxIp].IpEventId;
                        ip["IpAddress"] = ips[idxIp].IpAddress;
                        ip["DisplayName"] = ip["IpAddress"] + "  (" + ips[idxIp].CntFailedLogins + ")";
                        ip["ItemId"] = itemId;
                        ipsOut.Add(ip);
                    }
                    block["Ips"] = ipsOut;
                    blocksOut.Add(block);
                }

                // Output Result
                retVal.Add("IpBlocks", blocksOut);
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public JArray getSettingsForDisplay()
        {
            const string location = CLASSNAME + ".getSettingsForDisplay";
            JArray retVal = new JArray();
            try
            {
                if (DataMgr.appSettings == null)
                {
                    L.err(location, "App setting were null.");
                    return retVal;
                }

                // Properties need to be checked individually
                JObject obj = new JObject();
                obj.Add("Key", "baseDirectory");
                obj.Add("Value", U.GetSetting("baseDirectory", ""));
                obj.Add("DisplayName", "Base Directory:  ");
                obj.Add("Description", 
                    "Directory where all of the application files are held");
                retVal.Add(obj);

                obj = new JObject();
                obj.Add("Key", "EnableFileLogging");
                obj.Add("Value", U.GetSetting("EnableFileLogging", true));
                obj.Add("DisplayName", "Enable File Logging");
                obj.Add("Description", "Turn on file logging. Creates flat file logs that include counts, ips, block, and firewall data.");
                retVal.Add(obj);

                obj = new JObject();
                obj.Add("Key", "EnableAutomaticReport");
                obj.Add("Value", U.GetSetting("EnableAutomaticReport", true));
                obj.Add("DisplayName", "Enable Automatic Report");
                obj.Add("Description", "The Client console app generates a report file when it runs, including" +
                    " ips, blocks, and rules inspected.");
                retVal.Add(obj);

                obj = new JObject();
                obj.Add("Key", "isHeadless");
                obj.Add("Value", U.GetSetting("isHeadless", true));
                obj.Add("DisplayName", "Is Headless?:  ");
                obj.Add("Description", 
                    "If you intend to use the console app manually, AND want the console to remain " + 
                    "open for viewing when complete, set Is Headless to False. However, Is Headless " + 
                    "must be True for the console to exit successfully, when automated " + 
                    "using Windows Task Scheduler or other means. This difference, is the app staying alive.");
                retVal.Add(obj);

                obj = new JObject();
                obj.Add("Key", "maxToProcess");
                obj.Add("Value", U.GetSetting("maxToProcess", 100000));
                obj.Add("DisplayName", "Max To Process:  ");
                obj.Add("Description", 
                    "Maximum number of EventLog events to process (in a single scan), while looking for " + 
                    "failed logins. An average file, of 20MB, might contain 25-35K " + 
                    "records. For normal use, set this large enough to process the " + 
                    "complete file.");
                retVal.Add(obj);

                obj = new JObject();
                obj.Add("Key", "instanceName");
                obj.Add("Value", U.GetSetting("instanceName", "FWMClient-Unset"));
                obj.Add("DisplayName", "Instance Name:  ");
                obj.Add("Description", 
                    "Used to identify the application. Pairs with the \"Multi-" + 
                    "Instance\" option.");
                retVal.Add(obj);

                obj = new JObject();
                obj.Add("Key", "allowMultiInstance");
                obj.Add("Value", U.GetSetting("allowMultiInstance", false));
                obj.Add("DisplayName", "Multi-Instance?:  ");
                obj.Add("Description", 
                    "Allows or disallows the console from running simultaneous " + 
                    "instances. When working on a single set of data, turn multi-" + 
                    "instance to False. If working multiple sets of data, each " + 
                    "with its own copy of the console, and its own paths, set " + 
                    "multi-instance to True for mining.");
                retVal.Add(obj);

                obj = new JObject();
                obj.Add("Key", "ApprovedIps");
                obj.Add("Value", U.GetSetting("ApprovedIps", ""));
                obj.Add("DisplayName", "Approved IPs:  ");
                obj.Add("Description", 
                    "A list of comma separated IPs to *not* monitor. Any events coming from these IPs are discarded. Full IP only, no wildcards.");
                retVal.Add(obj);

                obj = new JObject();
                obj.Add("Key", "EventFolder");
                obj.Add("Value", U.GetSetting("EventFolder", ""));
                obj.Add("DisplayName", "Event Folder:  ");
                obj.Add("Description", 
                    "Critical. Sets the folder where event logs can be found. " + 
                    "Generally, this is something like: \n" + 
                    "  \"C:\\Windows\\System32\\winevt\\Logs\\\"\n" + 
                    "Support for reading archived is extremely limited, and consists " + 
                    "of pointing to a path, that has a Security.evtx file in it. " + 
                    "Note the final slash.");
                retVal.Add(obj);

                obj = new JObject();
                obj.Add("Key", "ReportFilePrefix");
                obj.Add("Value", U.GetSetting("ReportFilePrefix", "Rpt_"));
                obj.Add("DisplayName", "Report File Prefix:  ");
                obj.Add("Description", 
                    "Sets a prefix for the report filename. Reports are generated in the " + 
                    "Reports folder of the base directory, with a prefix and timestamp " + 
                    "name.");
                retVal.Add(obj);

                obj = new JObject();
                obj.Add("Key", "IsManageFW");
                obj.Add("Value", U.GetSetting("IsManageFW", false));
                obj.Add("DisplayName", "Manage Firewall?:  ");
                obj.Add("Description", 
                    "Critical. Enables or disables managing the firewall automatically. " + 
                    "This consists of applying scan results, to create firewall rules " + 
                    "blocking an IP, and monitor and expire rules previously created. It " + 
                    "changes the system's connectivity to the world.");
                retVal.Add(obj);

                obj = new JObject();
                obj.Add("Key", "MinFailuresToBlock");
                obj.Add("Value", U.GetSetting("MinFailuresToBlock", 20));
                obj.Add("DisplayName", "Failures To Block After:  ");
                obj.Add("Description", 
                    "How many failures does it take, over the timespan set by \"Firewall " + 
                    "Minutes To Review\", to support taking firewall action against an IP? " + 
                    "The combination of these two-values, determines who is blocked. Setting " + 
                    "the count too low, can cause irritating blockages. Setting the time too " + 
                    "long, can block employees and nominal parties.");
                retVal.Add(obj);

                obj = new JObject();
                obj.Add("Key", "FWPrefix");
                obj.Add("Value", U.GetSetting("FWPrefix", "FWMRule"));
                obj.Add("DisplayName", "Firewall Rule Prefix:  ");
                obj.Add("Description", 
                    "Rules are created in Windows Firewall, with a leading prefix in the " + 
                    "name. This groups and readily identifies rules automatically generated " + 
                    "by the console app, when using built-in OS software for manually managing " + 
                    "the firewall.");
                retVal.Add(obj);
                /*
                obj = new JObject();
                obj.Add("Key", "MSBetweenFWTestMin");
                obj.Add("Value", U.GetSetting("MSBetweenFWTestMin", 30));
                obj.Add("DisplayName", "Min MS Between Firewall Tests:  ");
                obj.Add("Description", "The MS (millisecond) values are somewhat arbitrary, and are really only intended to prevent spamming the firewall with lookups and rule changes, in the event many changes are needed at-once.");
                retVal.Add(obj);

                obj = new JObject();
                obj.Add("Key", "MSBetweenFWTestMax");
                obj.Add("Value", U.GetSetting("MSBetweenFWTestMax", 60));
                obj.Add("DisplayName", "Max MS Between Firewall Tests:  ");
                obj.Add("Description", "");
                retVal.Add(obj);

                obj = new JObject();
                obj.Add("Key", "MSBetweenFWAddMin");
                obj.Add("Value", U.GetSetting("MSBetweenFWAddMin", 200));
                obj.Add("DisplayName", "Min MS Between Firewall Add:  ");
                obj.Add("Description", "");
                retVal.Add(obj);

                obj = new JObject();
                obj.Add("Key", "MSBetweenFWAddMax");
                obj.Add("Value", U.GetSetting("MSBetweenFWAddMax", 400));
                obj.Add("DisplayName", "Max MS Between Firewall Add:  ");
                obj.Add("Description", "");
                retVal.Add(obj);
                */
                obj = new JObject();
                obj.Add("Key", "FWMinutesToReview");
                obj.Add("Value", U.GetSetting("FWMinutesToReview", 10080));
                obj.Add("DisplayName", "Firewall Minutes To Review:  ");
                obj.Add("Description", "There is no right answer, to time to review versus number to find faulty. It depends upon how much time you find relevant. 10,080 is one-week.");
                retVal.Add(obj);

                obj = new JObject();
                obj.Add("Key", "FWExpireAfterMinutes");
                obj.Add("Value", U.GetSetting("FWExpireAfterMinutes", 10080));
                obj.Add("DisplayName", "Expire After Minutes:  ");
                obj.Add("Description", "The number of minutes after a created firewall rule, should be taken down and expired. This removes any barrier this app or the console placed against the IP.");
                retVal.Add(obj);
                /*
                obj = new JObject();
                obj.Add("Key", "FWExpireAfterDays");
                obj.Add("Value", U.GetSetting("FWExpireAfterDays", 30));
                obj.Add("DisplayName", "Expire After Days:  ");
                obj.Add("Description", "The number of days after a created firewall rule, should be taken down and expired. This removes any barrier this app or the console placed against the IP.");
                retVal.Add(obj);
                */
                /*
                obj = new JObject();
                obj.Add("Key", "FWPort");
                obj.Add("Value", U.GetSetting("FWPort", ""));
                obj.Add("DisplayName", "Firewall Port:  ");
                obj.Add("Description", "Not functional yet");
                retVal.Add(obj);
                */
                /*obj = new JObject();
                obj.Add("Key", "FWProtocol");
                obj.Add("Value", U.GetSetting("FWProtocol", ""));
                obj.Add("DisplayName", "Firewall Protocol:  ");
                obj.Add("Description", 
                    "Options are TCP, UDP, or ANY, where in the limited meaning of \"any\" " + 
                    "both are blocked.");
                retVal.Add(obj);*/


            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        private static List<IpBlock> groupByIpForUi(List<FailedLoginEvent> events)
        {
            // Counts are self-contained, according to the list. IP and IP Block attributes
            // reflect the list, and not storage. Counts and dates are from the list.
            const string location = CLASSNAME + ".groupByIpForUi";
            List<IpBlock> retVal = new List<IpBlock>();
            try
            {
                if (events == null)
                {
                    L.err(location, "Input events were null.");
                    return retVal; //Early Exit
                }

                // At this point, it should be certain that IPs and Blocks exist in memory, go get them only.

                List<IpBlock> blocks = new List<IpBlock>();
                int cntBadIpId = 0;
                int cntBadIpBlockId = 0;
                for (int idxEvent = 0; idxEvent < events.Count; idxEvent++)
                {
                    // Find the block
                    if (events[idxEvent].IpBlockId <= 0)
                    {
                        cntBadIpBlockId++;
                    }

                    int idxBlock = -1;
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if (blocks[i].IpBlockId == events[idxEvent].IpBlockId)
                        {
                            idxBlock = i;
                            break;
                        }
                    }

                    if (idxBlock < 0)
                    {
                        // Block does not exist in working data, go get from memory
                        string blockAddress = c.getBlockAddress(events[idxEvent].IpAddress);

                        IpBlock record = DataMgr.getIpBlockByBlockAddress(blockAddress, 1);
                        if (record == null)
                        {
                            //L.err(location, "Failed to locate block in memory.");
                            continue; //Loop
                        }

                        // Create an empty ip block, and migrate non derived values
                        IpBlock block = new IpBlock();
                        block.IpBlockId = record.IpBlockId;
                        block.Active = record.Active;
                        block.CreateDateTime = record.CreateDateTime;
                        block.BlockAddress = record.BlockAddress;
                        block.LastTime = record.LastTime;// this stays the same
                        block.IpEvents = new List<IpEvent>();

                        blocks.Add(block);
                        for (int i = blocks.Count - 1; i >= 0; i--)
                        {
                            if (block.IpBlockId == blocks[i].IpBlockId)
                            {
                                idxBlock = i;
                                break;
                            }
                        }
                        if (idxBlock < 0)
                        {
                            L.err(location, "Failed to identify block address (" + blockAddress + ").");
                            continue;
                        }
                    }

                    // Find IP on block
                    int idxIp = -1;
                    for (int i = 0; i < blocks[idxBlock].IpEvents.Count; i++)
                    {
                        if (blocks[idxBlock].IpEvents[i].IpEventId == events[idxEvent].IpEventId)
                        {
                            idxIp = i;
                            break;
                        }
                    }

                    if (idxIp < 0)
                    {
                        // IP does not exist in working data, go get from memory
                        IpEvent record = DataMgr.getIpByIpAddress(events[idxEvent].IpAddress, 1);
                        if (record == null)
                        {
                            L.err(location, "Failed to locate IP in memory.");
                            continue;
                        }
                        IpEvent ip = new IpEvent();
                        ip.IpEventId = record.IpEventId;
                        ip.IpBlockId = record.IpBlockId;
                        ip.Status = record.Status;
                        ip.IpId = record.IpId;
                        ip.Active = record.Active;
                        ip.CreateDateTime = record.CreateDateTime;
                        ip.IpAddress = record.IpAddress;
                        ip.BlockAddress = record.BlockAddress;
                        
                        // Add IP to Block
                        blocks[idxBlock].IpEvents.Add(ip);

                        for (int i = blocks[idxBlock].IpEvents.Count - 1; i >= 0; i--)
                        {
                            if (events[idxEvent].IpEventId == blocks[idxBlock].IpEvents[i].IpEventId)
                            {
                                idxIp = i;
                                break;
                            }
                        }
                        if (idxIp < 0)
                        {
                            L.err(location, "Failed to identify ip index on block.");
                            continue;
                        }
                    }

                    // Add current event to working data
                    if (blocks[idxBlock].IpEvents[idxIp].FailedLogins == null)
                    {
                        blocks[idxBlock].IpEvents[idxIp].FailedLogins = new List<FailedLoginEvent>();
                    }
                    blocks[idxBlock].IpEvents[idxIp].FailedLogins.Add(events[idxEvent]);

                    // Ensure username lists exist
                    if (blocks[idxBlock].UserNames == null)
                    {
                        blocks[idxBlock].UserNames = new Dictionary<string, int>();
                    }
                    if (blocks[idxBlock].IpEvents[idxIp].UserNames == null)
                    {
                        blocks[idxBlock].IpEvents[idxIp].UserNames = new Dictionary<string, int>();
                    }

                    // See if username exists
                    string uname = events[idxEvent].TargetUserName;
                    if (uname != null && uname.Length > 0)
                    {
                        if (blocks[idxBlock].UserNames.ContainsKey(uname))
                        {
                            blocks[idxBlock].UserNames[uname]++;
                        }
                        else 
                        {
                            blocks[idxBlock].UserNames.Add(uname, 1);
                        }

                        if (blocks[idxBlock].IpEvents[idxIp].UserNames.ContainsKey(uname))
                        {
                            blocks[idxBlock].IpEvents[idxIp].UserNames[uname]++;
                        }
                        else 
                        {
                            blocks[idxBlock].IpEvents[idxIp].UserNames.Add(uname, 1);
                        }
                    }
                }

                // Update overall counts
                for (int idxBlock = 0; idxBlock < blocks.Count; idxBlock++)
                {
                    blocks[idxBlock].CntIps = blocks[idxBlock].IpEvents.Count;
                    List<IpEvent> ips = blocks[idxBlock].IpEvents;

                    for (int idxIp = 0; idxIp < ips.Count; idxIp++)
                    {
                        // Set failed login counts
                        ips[idxIp].CntFailedLogins = ips[idxIp].FailedLogins.Count;

                        blocks[idxBlock].CntFailedLogins += ips[idxIp].CntFailedLogins;

                        ips[idxIp].PercentOfTotal = (ips[idxIp].CntFailedLogins / events.Count) * 100;

                        List<FailedLoginEvent> fles = ips[idxIp].FailedLogins;
                        for (int idxEvent = 0; idxEvent < fles.Count; idxEvent++)
                        {
                            if (
                                blocks[idxBlock].StartTime == null ||
                                blocks[idxBlock].StartTime == c.nDt ||
                                blocks[idxBlock].StartTime > fles[idxEvent].CreateDateTime
                            )
                            {
                                blocks[idxBlock].StartTime = fles[idxEvent].CreateDateTime;
                            }

                            if (
                                blocks[idxBlock].EndTime == null || 
                                blocks[idxBlock].EndTime == c.nDt ||
                                blocks[idxBlock].EndTime < fles[idxEvent].CreateDateTime
                            )
                            {
                                blocks[idxBlock].EndTime = fles[idxEvent].CreateDateTime;
                            }

                            if (
                                ips[idxIp].StartTime == null || 
                                ips[idxIp].StartTime == c.nDt ||
                                ips[idxIp].StartTime > fles[idxEvent].CreateDateTime
                            )
                            {
                                ips[idxIp].StartTime = fles[idxEvent].CreateDateTime;
                            }
                            if (
                                ips[idxIp].EndTime == null || 
                                ips[idxIp].EndTime == c.nDt ||
                                ips[idxIp].EndTime < fles[idxEvent].CreateDateTime
                            )
                            {
                                ips[idxIp].EndTime = fles[idxEvent].CreateDateTime;
                            }
                        }

                        // Get a count of usernames attempted for IP
                        if (ips[idxIp].UserNames != null)
                        {
                            ips[idxIp].UserNamesAttempted = ips[idxIp].UserNames.Count;
                        }

                        // Get Ip average latency
                        if (ips[idxIp].StartTime != null && ips[idxIp].EndTime != null && ips[idxIp].CntFailedLogins > 0)
                        {
                            ips[idxIp].Elapsed = (ips[idxIp].EndTime - ips[idxIp].StartTime).TotalMilliseconds;
                            ips[idxIp].AverageLatency = ips[idxIp].Elapsed / ips[idxIp].CntFailedLogins;
                        }
                    }

                    // Get IpBlock average latency
                    if (blocks[idxBlock].StartTime != null && blocks[idxBlock].EndTime != null && blocks[idxBlock].CntFailedLogins > 0)
                    {
                        blocks[idxBlock].Elapsed = (blocks[idxBlock].EndTime - blocks[idxBlock].StartTime).TotalMilliseconds;
                        blocks[idxBlock].AverageLatency = blocks[idxBlock].Elapsed / blocks[idxBlock].CntFailedLogins;
                    }
                }

                // Output Result
                retVal = blocks;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        private bool loadDataTreeView()
        {
            const string location = CLASSNAME + ".loadDataTreeView";
            bool retVal = false;
            try
            {
                if (this.dataTreeJson == null)
                {
                    L.err(location, "Data tree JSON was null when loading view.");
                    return retVal;
                }

                // Invoke a clear on UI tree data
                dataTreeViewClear();

                if (dataTreeJson.Count == 0)
                {
                    L.err(location, "Tree data was empty.");
                    return retVal; //Early Exit
                }
                if (!dataTreeJson.ContainsKey("IpBlocks"))
                {
                    L.err(location, "Ip blocks were missing from tree data.");
                    return retVal; //Early Exit
                }

                JArray ipBlocks = null;
                try
                {
                    ipBlocks = (JArray)dataTreeJson["IpBlocks"];
                }
                catch (Exception exConv) { }
                if (ipBlocks == null)
                {
                    L.err(location, "Ip blocks were null after conversion.");
                    return retVal; //Early Exit
                }
                if (ipBlocks.Count == 0)
                {
                    L.err(location, "Ip blocks were empty after conversion.");
                    return retVal; //Early Exit
                }


                // Iterate tree view data
                List<TreeNode> nodes = new List<TreeNode>();
                for (int idxBlock = 0; idxBlock < ipBlocks.Count; idxBlock++)
                {
                    // Push block to tree view
                    TreeNode blockNode = new TreeNode((string)ipBlocks[idxBlock]["DisplayName"])
                    { Tag = U.getLong((JObject)ipBlocks[idxBlock], "ItemId", 0L) };
                    nodes.Add(blockNode);

                    JArray ips = null;
                    try { ips = (JArray)ipBlocks[idxBlock]["Ips"]; }
                    catch (Exception exConv) { }
                    if (ips == null) continue;

                    for (int idxIp = 0; idxIp < ips.Count; idxIp++)
                    {
                        JObject ip = null;
                        try { ip = (JObject)ips[idxIp]; }
                        catch (Exception exConv) { }
                        if (ip == null) continue;

                        TreeNode ipNode = new TreeNode((string)ip["DisplayName"])
                        { Tag = U.getLong(ip, "ItemId", 0L) };
                        blockNode.Nodes.Add(ipNode);
                    }
                }

                TreeNode[] nodesOut = nodes.ToArray();

                // Flag success based upon setting UI
                retVal = dataTreeViewSet(nodesOut);
                if (!retVal)
                {
                    L.err(location, "Failed to set data tree.");
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public bool loadSettingsTab()
        {
            const string location = CLASSNAME + ".loadSettingsTab";
            bool retVal = false;
            try
            {
                // Validate memory
                if (DataMgr.appSettings == null || DataMgr.appSettings.Count == 0)
                {
                    L.err(location, "App settings were not loaded.");
                    return retVal;
                }

                // Start building a UI table
                TableLayoutPanel tbl = new TableLayoutPanel();
                tbl.Dock = DockStyle.Fill;
                tbl.ColumnCount = 3;
                tbl.RowCount = DataMgr.appSettings.Count + 2;// +1 for header, +1 for saving
                tbl.AutoScroll = true;

                for (int i = 0; i < tbl.ColumnCount; i++)
                {
                    tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                }
                for (int i = 0; i < tbl.RowCount; i++)
                {
                    tbl.RowStyles.Add(new RowStyle());
                }

                // Add a header row
                int row = 0;
                Label h1 = new Label() { Text = "Setting" };
                Label h2 = new Label() { Text = "Value" };
                Label h3 = new Label() { Text = "Description" };
                tbl.Controls.Add(h1, 0, row);
                tbl.Controls.Add(h2, 1, row);
                tbl.Controls.Add(h3, 2, row);
                row++;

                // Clear class level settings controls
                this.settingsInput = new List<Control>();

                // Iterate settings to display
                JArray jarr = getSettingsForDisplay();
                L.l(location, "Iterating (" + jarr.Count + ") settings to display.");

                for (int i = 0; i < jarr.Count; i++, row++)
                {
                    JObject jobj = (JObject)jarr[i];

                    Label label = new Label()
                    {
                        Text = U.getString(jobj, "DisplayName", ""),
                        Width = 200,
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    TextBox tb = new TextBox()
                    {
                        Name = "tbGeneric" + U.getString(jobj, "Key", i.ToString()),
                        Text = Convert.ToString(jobj["Value"]),
                        Width = 200
                    };
                    this.settingsInput.Add(tb);

                    // Create a scrollable region for long descriptions
                    Panel pnl = new Panel()
                    {
                        Width = 300,
                        Height = 100,
                        AutoScroll = true
                    };
                    Label lblDesc = new Label()
                    {
                        Text = U.getString(jobj, "Description", ""),
                        Height = 100,
                        Width = 300,
                        AutoSize = false
                    };
                    pnl.Controls.Add(lblDesc);

                    // Push elements to row
                    tbl.Controls.Add(label, 0, row);
                    tbl.Controls.Add(tb, 1, row);
                    tbl.Controls.Add(pnl, 2, row);
                }

                // Add a row for save button
                Label saveLabel = new Label();
                Button saveButton = new Button();
                Label spacer = new Label();
                saveButton.Text = "Save Settings";
                saveButton.Click += btnSaveSettings_Click;
                row++;

                tbl.Controls.Add(saveLabel, 0, row);
                tbl.Controls.Add(saveButton, 1, row);
                tbl.Controls.Add(spacer, 2, row);

                int verticalMargin = 5;
                foreach (Control ctrl in tbl.Controls)
                {
                    ctrl.Margin = new Padding(ctrl.Margin.Left, verticalMargin, ctrl.Margin.Right, verticalMargin);
                }

                tabSettings.Controls.Clear();
                tabSettings.Controls.Add(tbl);
                tabSettings.Invalidate();

                // Flag success for completing
                retVal = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }


        public void runQuery()
        {
            const string location = CLASSNAME + ".runQuery";
            try
            {
                if (dtpQueryStart == null || dtpQueryEnd == null)
                {
                    L.err(location, "UI was null at query.");
                    return;
                }

                DateTime dtStart = dtpQueryStart.Value;
                DateTime dtEnd = dtpQueryEnd.Value;

                L.l(location, "Starting query between (" + dtStart.ToString(TAG.DTF) + 
                    ") and (" + dtEnd.ToString(TAG.DTF) + ").");

                // Reload data from storage (get latest from console)
                if (!DataMgr.loadAllData(false))// false to skip firewall file
                {
                    L.err(location, "Failed to reload data prior to query.");
                }

                // Disable data query UI
                if (!enableDataQueryUI(false))
                {
                    L.err(location, "Failed to disable data query UI.");
                }
                if (!enableDataPageUI(false))
                {
                    L.err(location, "Failed to disable data page-buttons.");
                }
                this.dataTreePage = 0;
                if (!setDataPageLabels("1", "1"))
                {
                    L.err(location, "Failed to default page numbers for new query.");
                }

                // Clear existing data, it will no longer match tree selection
                if (!configureListView(lvTreeDataRows))
                {
                    L.err(location, "Failed to reset data listview.");
                }
                this.dataTreeBlocks = new List<IpBlock>();
                this.dataTreeJson = null;

                List<FailedLoginEvent> events = DataMgr.getFailedLoginEventsByDate(dtStart, dtEnd);
                L.l(location, "Grouping (" + events.Count + ") events.");

                List<IpBlock> blocks = groupByIpForUi(events);

                blocks.Sort((pair1, pair2) => pair1.CntFailedLogins.CompareTo(pair2.CntFailedLogins));
                blocks.Reverse();

                // Get data tree data
                JObject jDataTree = getDataTreeJson(blocks);
                if (jDataTree == null)
                {
                    L.err(location, "Failed to get data tree JSON.");
                }
                else
                {
                    // Make data available to everyone for lookup on event
                    this.dataTreeBlocks = blocks;
                    this.dataTreeJson = jDataTree;

                    // Update UI using data
                    if (!loadDataTreeView())
                    {
                        L.err(location, "Failed to update data tree in UI.");
                    }
                }

                // Notify user if there was no data
                if (blocks.Count == 0 || events.Count == 0)
                {
                    MessageBox.Show("No records were found between " + dtStart.ToString("yyyy-MM-dd") +
                        " and " + dtEnd.ToString("yyyy-MM-dd") + ".");
                }

                // Reenable data query UI
                if (!enableDataQueryUI(true))
                {
                    L.err(location, "Failed to reenable data query UI.");
                }
                if (!enableDataPageUI(true))
                {
                    L.err(location, "Failed to reenable data page-buttons.");
                }

                L.l(location, "Finished querying.");
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
                try
                {
                    // Reenable data query UI
                    if (!enableDataQueryUI(true))
                    {
                        L.err(location, "Failed to reenable data query UI.");
                    }
                    if (!enableDataPageUI(true))
                    {
                        L.err(location, "Failed to reenable data page-buttons.");
                    }
                }
                catch (Exception exReenable)
                {
                    L.ex(location, exReenable);
                }
            }
        }

        public bool saveSettingsTab()
        {
            const string location = CLASSNAME + ".saveSettingsTab";
            bool retVal = false;
            try
            {
                if (settingsInput == null)
                {
                    L.err(location, "Settings were null in UI memory.");
                    return retVal;
                }
                if (DataMgr.appSettings == null)
                {
                    L.err(location, "App settings were null in memory.");
                    return retVal;

                    // TODO - Decide on handling this better
                }
                L.l(location, "Processing (" + settingsInput.Count + ") settings.");

                int cntSaveErrors = 0;
                for (int i = 0; i < settingsInput.Count; i++)
                {
                    string key = settingsInput[i].Name.Replace("tbGeneric", "");
                    if (!U.SetSetting(key, settingsInput[i].Text))
                    {
                        cntSaveErrors++;
                        L.err(location, "Failed to set setting (" + key + ") in memory.");
                    }
                }

                if (!FileMgr.lockAppSettings(30))
                {
                    string errMsg = "Failed to acquire lock on settings file.";
                    L.err(location, errMsg);
                    MessageBox.Show(errMsg, "Save Settings");
                }
                else
                {
                    int cntWriteErrors = 0;
                    if (!DataMgr.saveAppSettings(DataMgr.appSettings))
                    {
                        cntWriteErrors++;
                        L.err(location, "Error writing app settings to file.");
                    }
                    if (!FileMgr.unlockAppSettings())
                    {
                        L.err(location, "Failed to release lock on app settings file.");
                    }

                    if (cntSaveErrors > 0 || cntWriteErrors > 0)
                    {
                        string errMsg = "Encountered (" + cntSaveErrors +
                            ") save errors and (" + cntWriteErrors + ") write errors.";
                        L.err(location, errMsg);
                        MessageBox.Show(errMsg, "Save Settings");
                    }
                    else
                    {
                        // Flag success
                        retVal = true;
                        MessageBox.Show("Changes saved successfully.", "Save Settings");
                    }
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

        public bool setDataPage(int pageNum)
        {
            const string location = CLASSNAME + ".setDataPage";
            bool retVal = false;
            try
            {
                // Validate input
                if (pageNum < 1)
                {
                    L.err(location, "Invalid page number (" + pageNum + ").");
                    return retVal;
                }

                // Validate indexes to data
                if (this.idxDataTreeIpBlock < 0)
                {
                    L.err(location, "Attempted to change pages without selection.");
                    return retVal;
                }

                // Validate data in memory
                if (this.dataTreeBlocks == null)
                {
                    L.err(location, "Data uninitialized when changing pages.");
                    return retVal;
                }
                if (this.idxDataTreeIpBlock >= this.dataTreeBlocks.Count)
                {
                    L.err(location, "Block index (" + this.idxDataTreeIpBlock + ") was out of range (" + this.dataTreeBlocks.Count + ").");
                    return retVal;
                }
                if (this.dataTreePageSize <= 0)
                {
                    L.err(location, "Page size was invalid (" + this.dataTreePageSize + ").");
                    return retVal;
                }
                IpBlock block = this.dataTreeBlocks[this.idxDataTreeIpBlock];
                if (block == null || block.IpEvents == null)
                {
                    L.err(location, "Block was uninitialized in data.");
                    return retVal;
                }

                // Disable data query UI
                if (!enableDataQueryUI(false))
                {
                    L.err(location, "Failed to disable data query UI.");
                }
                if (!enableDataPageUI(false))
                {
                    L.err(location, "Failed to disable data page-buttons.");
                }
                if (!enableDataExportUI(false))
                {
                    L.err(location, "Failed to disabled data export UI.");
                }

                // The Ip Id acts like a toggle, target ip instead of block if non-negative
                if (this.idxDataTreeIpEvent < 0)
                {
                    // Set page based upon IPs in block

                    int cntPages = (int)((double)block.IpEvents.Count / (double)this.dataTreePageSize) + 1;
                    if (pageNum <= cntPages)
                    {
                        // Set page number
                        string sPageNumber = Convert.ToString(pageNum);
                        string sOfPages = Convert.ToString(cntPages);
                        if (!setDataPageLabels(sPageNumber, sOfPages))
                        {
                            L.err(location, "Failed to set data paging labels.");
                        }
                        if (!dataTreeSelectBlock(pageNum))
                        {
                            L.err(location, "Failed to reload listview with block page.");
                        }
                        else
                        {
                            retVal = true;// Flag success on datawindow
                        }
                    }
                }
                else
                {
                    // Set page based upon events for ip
                    if (this.idxDataTreeIpEvent >= block.IpEvents.Count)
                    {
                        L.err(location, "Invalid ip index (" + this.idxDataTreeIpEvent + ") out of (" + block.IpEvents.Count + ").");
                    }
                    else
                    {
                        IpEvent ip = block.IpEvents[this.idxDataTreeIpEvent];
                        if (ip == null)
                        {
                            L.err(location, "Ip uninitialized at page set.");
                        }
                        else if (ip.FailedLogins == null)
                        {
                            L.err(location, "Failed logins for ip were uninitialized at page set.");
                        }
                        else
                        {
                            int cntPages =
                                (int)((double)block.IpEvents[this.idxDataTreeIpEvent].FailedLogins.Count / (double)this.dataTreePageSize) + 1;
                            if (pageNum <= cntPages)
                            {
                                string sPageNumber = Convert.ToString(pageNum);
                                string sOfPages = Convert.ToString(cntPages);
                                if (!setDataPageLabels(sPageNumber, sOfPages))
                                {
                                    L.err(location, "Failed to set data paging labels.");
                                }
                                if (!dataTreeSelectIp(ip.IpEventId, pageNum))
                                {
                                    L.err(location, "Failed to reload listview with ip page.");
                                }
                                else
                                {
                                    retVal = true;// Flag success on datawindow
                                }
                            }
                        }
                    }
                }

                // Reenable data query UI
                if (!enableDataQueryUI(true))
                {
                    L.err(location, "Failed to reenable data query UI.");
                }
                if (!enableDataPageUI(true))
                {
                    L.err(location, "Failed to reenable data page-buttons.");
                }
                if (!enableDataExportUI(true))
                {
                    L.err(location, "Failed to reenable data export UI.");
                }

                // Flag success for completing
                return retVal;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
                try
                {
                    // Reenable data query UI
                    if (!enableDataQueryUI(true))
                    {
                        L.err(location, "Failed to reenable data query UI.");
                    }
                    if (!enableDataPageUI(true))
                    {
                        L.err(location, "Failed to reenable data page-buttons.");
                    }
                    if (!enableDataExportUI(true))
                    {
                        L.err(location, "Failed to reenable data export UI.");
                    }
                }
                catch (Exception exReenable)
                {
                    L.ex(location, exReenable);
                }
            }
            return retVal;
        }

        public bool setDataPageLabels(string pageNumber, string ofPages)
        {
            const string location = CLASSNAME + ".setDataPageLabels";
            bool retVal = false;
            try
            {
                if (pageNumber == null) pageNumber = "";
                if (ofPages == null) ofPages = "";

                if (lblDataPageNumber != null)
                {
                    if (lblDataPageNumber.InvokeRequired)
                    {
                        lblDataPageNumber.Invoke(new Action(() => lblDataPageNumber.Text = pageNumber));
                    }
                    else 
                    {
                        lblDataPageNumber.Text = pageNumber;
                    }
                }

                if (lblDataOfPages != null)
                {
                    if (lblDataOfPages.InvokeRequired)
                    {
                        lblDataOfPages.Invoke(new Action(() => lblDataOfPages.Text = ofPages));
                    }
                    else
                    {
                        lblDataOfPages.Text = ofPages;
                    }
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

        public bool showFirewallRulesAll()
        {
            const string location = CLASSNAME + ".showFirewallRulesAll";
            bool retVal = false;
            try
            {
                // TODO - Add a reload of firewall data from storage HERE

                Thread thread = new Thread(() =>
                {
                    const string locThread = CLASSNAME + ".showFirewallRulesAll.thread";
                    try
                    {
                        // Get latest data, to see if rules are current
                        DataMgr.FWRows = FileMgr.readFWRows();
                        if (DataMgr.FWRows == null)
                        {
                            L.l(location, "Rules from storage were empty.");
                            DataMgr.FWRows = new List<FWRow>();// Instantiate static data if null
                        }
                        if (DataMgr.FWRows.Count == 0)
                        {
                            L.l(location, "Data from storage was empty.");
                            return;
                        }

                        //L.l(location, "Reviewing (" + DataMgr.FWRows.Count + ") known firewall rules.");

                        // Create a copy of the list and sort new-to-old
                        List<FWRow> sorted = new List<FWRow>();
                        for (int i = 0; i < DataMgr.FWRows.Count; i++)
                            sorted.Add(DataMgr.FWRows[i]);

                        sorted.Sort((pair1, pair2) => pair1.CreateDateTime.CompareTo(pair2.CreateDateTime));
                        sorted.Reverse();

                        JArray rows = new JArray();
                        for (int i = 0; i < sorted.Count; i++)
                        {
                            JObject temp = sorted[i].toJObjectForGrid();
                            if (temp != null)
                            {
                                rows.Add(temp);
                            }
                        }
                        if (!toListView(lvFirewallRules, rows))
                        {
                            L.err(location, "Failed to display firewall rows.");
                        }
                    }
                    catch (Exception exThread)
                    {
                        L.ex(locThread, exThread);
                    }
                });

                thread.Start();

                // Flag success for starting thread
                retVal = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public bool startQueryThread()
        {
            const string location = CLASSNAME + ".startQueryThread";
            bool retVal = false;
            try
            {
                Thread thread = new Thread(new ThreadStart(runQuery));
                thread.Start();

                // Flag success for completing
                retVal = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        private bool toCsvQuery()
        {
            const string location = CLASSNAME + ".toCsvQuery";
            bool retVal = false;
            try
            {
                // TODO - Push this whole method into a thread

                if (!enableDataQueryUI(false))
                {
                    L.err(location, "Failed to disable the data query UI.");
                }
                if (!enableDataPageUI(false))
                {
                    L.err(location, "Failed to disable data-paging UI.");
                }
                if (!enableDataExportUI(false))
                {
                    L.err(location, "Failed to disable data export UI.");
                }


                string fileName = "";
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    try
                    {
                        sfd.Filter = "CSV files (*.csv)|*.csv";
                        sfd.Title = "Export ListView Data";

                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            fileName = sfd.FileName;
                            string dataOut = toCsvString(lvTreeDataRows);
                            if (!toFile(fileName, dataOut))
                            {
                                L.err(location, "Failed saving csv of query to: " + fileName);
                            }
                            else
                            {
                                L.l(location, "Finished saving csv to: " + fileName);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        L.ex(location, ex);
                    }
                }

                if (!enableDataQueryUI(true))
                {
                    L.err(location, "Failed to reenable the data query UI.");
                }
                if (!enableDataPageUI(true))
                {
                    L.err(location, "Failed to reenable the data-paging UI.");
                }
                if (!enableDataExportUI(true))
                {
                    L.err(location, "Failed to reenable data export UI.");
                }

                // Flag success for completing
                retVal = true;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
                try
                {
                    if (!enableDataQueryUI(true))
                    {
                        L.err(location, "Failed to reenable the data query UI.");
                    }
                    if (!enableDataPageUI(true))
                    {
                        L.err(location, "Failed to reenable the data-paging UI.");
                    }
                    if (!enableDataExportUI(true))
                    {
                        L.err(location, "Failed to reenable data export UI.");
                    }
                }
                catch (Exception exReenable)
                {
                    L.ex(location, exReenable);
                }
            }
            return retVal;
        }

        private string toCsvString(ListView lv)
        {
            const string location = CLASSNAME + ".toCsvString";
            string retVal = "";
            try
            {
                if (lv == null)
                {
                    L.err(location, "Input was null.");
                    return retVal;
                }

                StringBuilder dataOut = new StringBuilder();
                bool headerStarted = false;
                for (int i = 0; i < lv.Columns.Count; i++)
                {
                    if (headerStarted) dataOut.Append(",");// Header has a non-column at start, stuff a comma at-first
                    dataOut.Append(lv.Columns[i].Text);
                    if (!headerStarted) headerStarted = true;
                }
                dataOut.Append("\n");

                foreach (ListViewItem item in lv.Items)
                {
                    bool rowStarted = false;
                    for (int i = 0; i < lv.Columns.Count; i++)
                    {
                        if (rowStarted) dataOut.Append(",");
                        dataOut.Append(U.escapeCsvField(i < item.SubItems.Count ? item.SubItems[i].Text : ""));
                        if (!rowStarted) rowStarted = true;
                    }
                    dataOut.Append("\n");
                }

                retVal = dataOut.ToString();
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        private bool toFile(string path, string contents)
        {
            const string location = CLASSNAME + ".toFile";
            bool retVal = false;
            try
            {
                if (path == null || path.Length == 0)
                {
                    L.err(location, "Path was invalid.");
                    return retVal;
                }

                if (contents == null) contents = "";

                using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
                {
                    try
                    {
                        sw.Write(contents);
                        retVal = true;
                    }
                    catch (Exception exStream)
                    {
                        L.ex(location, exStream);
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public bool toListView(ListView lv, JArray data)
        {
            const string location = CLASSNAME + ".toListView";
            bool retVal = false;
            try
            {
                if (lv == null)
                {
                    L.err(location, "Input listview was null.");
                    return retVal;
                }
                if (data == null)
                {
                    L.err(location, "Input data was null.");
                    return retVal;
                }
                if (data.Count == 0)
                {
                    L.l(location, "Input data was empty.");
                    retVal = true;// Flag success for this one
                    return retVal;
                }
                if (data[0] == null)
                {
                    // Consider the first object essential
                    L.l(location, "Input data was invalid.");
                    return retVal;
                }

                // Configure ListView
                if (!configureListView(lv))
                {
                    L.err(location, "Failed to preconfigure listview.");
                }

                // Setup header using keys from first object
                List<ColumnHeader> headers = new List<ColumnHeader>();
                List<string> keys = ((JObject)data[0]).Properties().Select(p => p.Name).ToList();

                // Iterate objects, expect keys to be constant for all objects
                int cntDisplayed = 0;
                List<ListViewItem> items = new List<ListViewItem>();
                for (int i = 0; i < data.Count; i++)
                {
                    ListViewItem lvi = new ListViewItem(Convert.ToString(i + 1));// with row number
                    
                    for (int col = 0; col < keys.Count; col++)
                    {
                        if (((JObject)data[i]).ContainsKey(keys[col]))
                        {
                            try
                            {
                                lvi.SubItems.Add(Convert.ToString(((JObject)data[i])[keys[col]]));
                            }
                            catch (Exception exAdd)
                            {
                                lvi.SubItems.Add("");
                            }
                        }
                        else
                        {
                            lvi.SubItems.Add("");
                        }
                    }
                    items.Add(lvi);
                    cntDisplayed++;
                }

                // NOTE - The column headers are sized differently. This is not necessary, but the closest
                // formatting so-far.
                if (lv.InvokeRequired)
                {
                    lv.Invoke(new Action(() => {
                        try
                        {
                            lv.BeginUpdate();
                            lv.Columns.Add("");
                            for (int i = 0; i < keys.Count; i++) lv.Columns.Add(keys[i]);
                            //foreach (ColumnHeader header in lv.Columns) header.Width = -2;// auto-size to -1 (content), -2 (header)

                            lv.Items.AddRange(items.ToArray());
                            lv.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                            lv.EndUpdate();
                        }
                        catch (Exception exUI)
                        {
                            L.ex(location, exUI);
                        }
                    }));
                }
                else
                {
                    lv.BeginUpdate();
                    lv.Columns.Add("");
                    for (int i = 0; i < keys.Count; i++) lv.Columns.Add(keys[i]);
                    foreach (ColumnHeader header in lv.Columns) header.Width = -2;// auto-size to -1 (content), -2 (header)

                    lv.Items.AddRange(items.ToArray());
                    //lv.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    lv.EndUpdate();
                }

                // Flag Success (critically)
                retVal = cntDisplayed == data.Count;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        private void btnClearLogs_Click(object sender, EventArgs e)
        {
            const string location = CLASSNAME + ".btnClearLogs_Click";
            try
            {
                long logLength = L.clearLogs();
                L.l(location, "Cleared (" + logLength + ") log length.");
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        private void btnDataExportCSV_Click(object sender, EventArgs e)
        {
            const string location = CLASSNAME + ".btnDataExportCSV_Click";
            try
            {
                L.l(location, "Preparing to export CSV to file.");
                if (!toCsvQuery())
                {
                    L.err(location, "Failed exporting query to CSV.");
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        private void btnDataPageHigher_Click(object sender, EventArgs e)
        {
            const string location = CLASSNAME + ".btnDataPageHigher_Click";
            try
            {
                int pageNumber = -1;
                try
                {
                    pageNumber = Convert.ToInt32(lblDataPageNumber.Text);
                }
                catch (Exception exConv) { }

                if (pageNumber > 0)
                {
                    pageNumber++;
                    if (!setDataPage(pageNumber))
                    {
                        //L.err(location, "Failed to set data page to (" + pageNumber + ").");
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        private void btnDataPageLower_Click(object sender, EventArgs e)
        {
            const string location = CLASSNAME + ".btnDataPageLower_Click";
            try
            {
                int pageNumber = -1;
                try
                {
                    pageNumber = Convert.ToInt32(lblDataPageNumber.Text);
                }
                catch (Exception exConv) { }

                // Lowest page number is one, only decrement if available
                if (pageNumber > 1)
                {
                    pageNumber--;
                    if (!setDataPage(pageNumber))
                    {
                        //L.err(location, "Failed to set data page to (" + pageNumber + ").");
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        private void btnFirewallExpireRules_Click(object sender, EventArgs e)
        {
            const string location = CLASSNAME + ".btnFirewallExpireRules_Click";
            try
            {
                L.l(location, "Expiring firewall rules.");
                if (!expireFirewallRules())
                {
                    L.err(location, "Failed to expire firewall rules.");
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        private void btnFirewallRefresh_Click(object sender, EventArgs e)
        {
            const string location = CLASSNAME + ".btnFirewallRefresh_Click";
            try
            {
                if (!showFirewallRulesAll())
                {
                    L.err(location, "Failed to display all firewall rules.");
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            const string location = CLASSNAME + ".btnQuery_Click";
            try
            {
                if (!startQueryThread())
                {
                    L.err(location, "Failed to run query.");
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            const string location = CLASSNAME + ".btnSaveSettings_Click";
            try
            {
                if (!saveSettingsTab())
                {
                    L.err(location, "Failed to save settings tab.");
                }
                else 
                {
                    L.l(location, "Finished saving settings tab.");
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string location = CLASSNAME + ".exitToolStripMenuItem_Click";
            try
            {
                L.l(location, "Application exiting from menu item.");
                Environment.Exit(0);
                L.err(location, "App failed to exit from menu item.");
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        private void treeDataView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            const string location = CLASSNAME + ".treeDataView_AfterSelect";
            try
            {
                if (!dataTreeSelect(e))
                {
                    L.err(location, "Failed to select item from tree view.");
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }
    }
}
