using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetFwTypeLib;

namespace FWM_ClientViewer_03
{
    public static class Firewall
    {
        public const string CLASSNAME = "Firewall";


        public static string[] anyProtocol = { "TCP", "UDP" };


        private static bool isAdminUser()
        {
            bool retValue = false;
            try
            {
                retValue = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception ex)
            {
                L.ex(CLASSNAME + ".isAdminUser", ex);
            }
            return retValue;
        }

        public static bool addFirewallRule(FWRow fw)
        {
            // ruleName should be unique. Using "MyRule"+ipaddr.Replace(".","_") as rule names for this app.
            // remoteAddress supports the following formats:
            // "1.2.3.4", "1.2.3.*", "1.2.3.0/24", and other formats supported by Windows Firewall rule
            // protocol can be set to "TCP", "UDP", ...
            // port can be set to "Any" or port number "8080"
            const string location = CLASSNAME + ".addFirewallRule";
            bool retValue = false;
            try
            {
                if (!c.isAppAdmin)
                {
                    L.err(location, "Ignoring request to add firewall rule as non-admin.");
                    return retValue;// Early Exit
                }
                else if (string.IsNullOrEmpty(fw.FWName) && string.IsNullOrEmpty(fw.IpAddress))
                {
                    L.d(location, "Skipping firewall rule with null or empty name/ip.");
                    return retValue;
                }

                bool errors = false;
                string temp = fw.Protocol.Trim().ToLower();
                if (temp.Equals("any") || temp.Equals("*") || temp.Equals("all"))
                {
                    // Block reasonable protocols upon wildcard
                    //L.logger(location, "Blocking all reasonable protocols for IP (" + fw.IpAddress + ").");
                    foreach (string s in anyProtocol)
                    {
                        string cmd = "/C netsh advfirewall firewall add rule name=\"" + fw.FWName + s.Substring(0, 1) + "\" dir=in action=block remoteip=" + fw.IpAddress + " remoteport=" + fw.Port + " protocol=" + s;
                        if (!execCmd(cmd, true, true))
                        {
                            errors = true;
                            if (c.debug) L.d(location, "Failed to create FW Rule (" + fw.FWName + s.Substring(0, 1) + ").");
                        }
                    }
                }
                else
                {
                    // Block specified protocol
                    string cmd = "/C netsh advfirewall firewall add rule name=\"" + fw.FWName + "\" dir=in action=block remoteip=" + fw.IpAddress + " remoteport=" + fw.Port + " protocol=" + fw.Protocol;
                    if (!execCmd(cmd, true, true))
                    {
                        errors = true;
                        if (c.debug) L.d(location, "Failed to create FW Rule (" + fw.FWName + ").");
                    }
                }

                if (errors)
                {
                    L.err(location, "Errors creating FW Rule set! Rule name (" + fw.FWName + "), Protocol (" + fw.Protocol + ").");
                }
                retValue = !errors;

            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }

        public static bool isFirewallRuleExisting(string ruleName)
        {
            const string location = CLASSNAME + ".isFirewallRuleExisting";
            bool retValue = false;
            try
            {
                string variant1 = ruleName + "T";//TCP rule using all/*/any protocol
                string variant2 = ruleName + "U";//UDP rule using all/*/any protocol

                Type tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                INetFwPolicy2 fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(tNetFwPolicy2);
                foreach (INetFwRule rule in fwPolicy2.Rules)
                {
                    if (rule.Name.IndexOf(ruleName) != -1)
                    {
                        retValue = true;
                    }
                    else if (rule.Name.IndexOf(variant1) != -1)
                    {
                        retValue = true;
                    }
                    else if (rule.Name.IndexOf(variant2) != -1)
                    {
                        retValue = true;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }


        public static int expireFirewallRules(ref List<FWRow> expiries, ProgressUpdater progressUpdate)
        {
            const string location = CLASSNAME + ".expireFirewallRules";
            int retValue = 0;// number of rules removed
            try
            {
                // Get all active DB rules with elapsed expirations
                L.l(location, "Expiring (" + expiries.Count + ") FW rules..");

                // Test for admin rights before attempting to update firewall
                c.isAppAdmin = isAdminUser();// refresh
                if (!c.isAppAdmin)
                {
                    L.err(location, "Ignoring request to expire firewall rules as non-admin.");
                    return retValue;// Early Exit
                }

                int msBetweenTestMin = U.GetSetting2("MSBetweenFWTestMin", 200);
                int msBetweenTestMax = U.GetSetting2("MSBetweenFWTestMax", 600);
                Random r = new Random();

                // Reinitialize our progress bar, now that we know how many to expire
                if (progressUpdate != null)
                {
                    if (progressUpdate.progressUpdater != null)
                    {
                        progressUpdate.progressUpdater.Report(0);
                    }
                    if (progressUpdate.minUpdater != null)
                    {
                        progressUpdate.minUpdater.Report(0);
                    }
                    if (progressUpdate.maxUpdater != null)
                    {
                        progressUpdate.maxUpdater.Report(expiries.Count);
                    }
                }
               

                // Iterate DB rule expiries
                for (int i = 0; i < expiries.Count; i++)
                {
                    FWRow row = expiries[i];
                    if (row == null) continue; //Loop
                    if (row.FWName == null || row.FWName.Length == 0) continue; //Loop
                    //if (row.Expiry == null || row.Expiry == c.nDt) continue; //Loop
                    // All user to deactivate Expired records, no harm
                    //if (row.Expired) continue;

                    // Set progress and message
                    if (progressUpdate.progressUpdater != null)
                    {
                        progressUpdate.progressUpdater.Report(i+1);
                    }
                    if (progressUpdate.messageUpdater != null)
                    {
                        progressUpdate.messageUpdater.Report("Expiring Rule:  " + row.FWName);
                    }
                    //Thread.Sleep(500);// TODO - See if message can update

                    // Check if rule exists
                    bool isARule = isFirewallRuleExisting(row.FWName);
                    if (!isARule)
                    {
                        // TODO - Decide later if this is an error condition
                        L.l(location, "Rule does not exist in FW when attempting to expire.");
                        continue;// Loop
                    }

                    string fwProtocol = row.Protocol.ToLower();
                    if (fwProtocol == "any" || fwProtocol == "all" || fwProtocol == "*")
                    {
                        foreach (string s in anyProtocol)
                        {
                            // Rules are appended with the leading character of protocol. e.g. FWName + "U" for UDP
                            string cmd = "/C netsh advfirewall firewall delete rule name=\"" + row.FWName + s.Substring(0, 1) + "\"";
                            if (!execCmd(cmd, true, true))
                            {
                                if (c.debug) L.d(location, "Failed to expire FW Rule (" + row.FWName + s.Substring(0, 1) + ").");
                            }
                            //Thread.Sleep(r.Next(msBetweenTestMin, msBetweenTestMax));
                        }
                    }
                    else
                    {
                        // Delete rule from firewall
                        string cmd = "/C netsh advfirewall firewall delete rule name=\"" + row.FWName + "\"";
                        if (!execCmd(cmd, true, true))
                        {
                            L.err(location, "Failed to exprire FW Rule (" + row.FWName + ").");
                        }
                    }

                    // See if rule still exists
                    isARule = isFirewallRuleExisting(row.FWName);
                    //L.l(location, "Rule (" + row.FWName + ") test isExisting (" + isARule + ").");

                    if (!isARule)
                    {
                        // Deactivate FW rule in DB
                        row.Deactivated = DateTime.Now;

                        //int cntRows = DataMgr.updateFWDeactivate(row);
                        //L.l(location, "Deactivated (" + cntRows + ") FW rows in memory.");

                        if (row.FWId > 0)
                        {
                            L.l(location, "Deactivated FW rule id (" + row.FWId + "), by name (" + row.FWName + ").");
                            retValue++;
                        }
                        else
                        {
                            // TODO - Decide later on error condition
                            L.l(location, "Failed to deactivate FW DB rule by name (" + row.FWName + ").");
                        }
                        //summary.CntFWExpired++;// accept FW removal enough for summary count
                    }
                    else
                    {
                        //summary.CntFWExpireFail++;
                    }
                    //Thread.Sleep(r.Next(msBetweenTestMin, msBetweenTestMax));
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }

        public static bool execCmd(string cmd, bool requireAdmin, bool waitForExit)
        {
            const string location = CLASSNAME + ".execCmd";
            bool retValue = false;
            try
            {
                if (requireAdmin && !c.isAppAdmin)
                {
                    L.err(location, "Ignoring request to run command as non-admin. Command (" + cmd + ").");
                }
                else
                {
                    using (Process RunCmd = new Process())
                    {
                        try
                        {
                            RunCmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            RunCmd.StartInfo.FileName = "cmd.exe";
                            RunCmd.StartInfo.Arguments = cmd;
                            RunCmd.Start();
                            if (waitForExit) RunCmd.WaitForExit();
                            retValue = true;
                        }
                        catch (Exception ex)
                        {
                            L.ex(location, ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retValue;
        }

    }
}
