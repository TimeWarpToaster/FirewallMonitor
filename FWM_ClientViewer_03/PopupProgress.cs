using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FWM_ClientViewer_03
{
    public partial class PopupProgress : Form
    {
        public const string CLASSNAME = "PopupProgress";

        public PopupProgress(string title, string message, int min, int max)
        {
            const string location = CLASSNAME + ".Constructor";
            try
            {
                InitializeComponent();

                if (!this.init(title, message, min, max))
                {
                    L.err(location, "Failed to initialize progress popup.");
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        private bool init(string title, string message, int min, int max)
        {
            const string location = CLASSNAME + ".init";
            bool retVal = false;
            try
            {
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.StartPosition = FormStartPosition.CenterParent;
                this.ControlBox = false;// Removes min, max, and close-X

                if (title != null && title.Length > 0)
                {
                    this.Text = title;
                }
                else 
                {
                    this.Text = "";
                }

                if (message != null && message.Length > 0)
                {
                    if (lblPopupProgressMessage != null)
                    {
                        if (lblPopupProgressMessage.InvokeRequired)
                        {
                            lblPopupProgressMessage.Invoke(new Action(() =>
                            {
                                lblPopupProgressMessage.Text = message;
                            }));
                        }
                        else
                        {
                            lblPopupProgressMessage.Text = message;
                        }
                    }
                }

                if (min >= 0 && max >= 0 && min <= max)
                {
                    if (progressBarPopupProgress != null)
                    {
                        if (progressBarPopupProgress.InvokeRequired)
                        {
                            progressBarPopupProgress.Invoke(new Action(() =>
                            {
                                progressBarPopupProgress.Minimum = min;
                                progressBarPopupProgress.Maximum = max;
                                progressBarPopupProgress.Value = min;
                            }));
                        }
                        else
                        {
                            progressBarPopupProgress.Minimum = min;
                            progressBarPopupProgress.Maximum = max;
                            progressBarPopupProgress.Value = min;
                        }

                        // Flag success for establishing mandatory progress bar
                        retVal = true;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public void closePopup()
        {
            const string location = CLASSNAME + ".closePopup";
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        try
                        {
                            this.Close();
                            this.Dispose();
                        }
                        catch (Exception ex) { }
                    }));
                }
                else 
                {
                    this.Close();
                    this.Dispose();
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        public ProgressUpdater getProgressUpdater()
        {
            const string location = CLASSNAME + ".getProgressUpdater";
            ProgressUpdater retVal = new ProgressUpdater();
            try
            {
                retVal.titleUpdater = new Progress<string>(value =>
                {
                    try
                    { 
                        this.updateTitle(value);
                    }
                    catch (Exception ex)
                    {
                        L.err(location, "Failed to update progress title with error: " + ex.Message);
                    }
                });
                retVal.messageUpdater = new Progress<string>(value =>
                {
                    try
                    { 
                        this.updateMessage(value);
                    }
                    catch (Exception ex)
                    {
                        L.err(location, "Failed to update progress message with error: " + ex.Message);
                    }
                });
                retVal.progressUpdater = new Progress<int>(value =>
                {
                    try
                    {
                        this.updateProgress(value);
                    }
                    catch (Exception ex)
                    {
                        L.err(location, "Failed to update progress with error: " + ex.Message);
                    }
                });
                retVal.maxUpdater = new Progress<int>(value =>
                {
                    try
                    {
                        this.updateMax(value);
                    }
                    catch (Exception ex)
                    {
                        L.err(location, "Failed to update max with error: " + ex.Message);
                    }
                });
                retVal.minUpdater = new Progress<int>(value =>
                {
                    try
                    {
                        this.updateMin(value);
                    }
                    catch (Exception ex)
                    {
                        L.err(location, "Failed to update min with error: " + ex.Message);
                    }
                });
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public void showOkayButton(bool show)
        {
            const string location = CLASSNAME + ".showOkayButton";
            try
            {
                if (btnPopupProgress1 == null) return;
                if (btnPopupProgress1.InvokeRequired)
                {
                    btnPopupProgress1.Invoke(new Action(() => { btnPopupProgress1.Visible = show; }));
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        public void updateMessage(string message)
        {
            const string location = CLASSNAME + "updateMessage";
            try
            {
                if (message != null)// Here we want to allow empty messages
                {
                    if (lblPopupProgressMessage != null)
                    {
                        lblPopupProgressMessage.Text = message;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        public void updateMax(int max)
        {
            const string location = CLASSNAME + ".updateMax";
            try
            {
                if (progressBarPopupProgress != null && max >= 0 && max >= progressBarPopupProgress.Minimum)
                {
                    progressBarPopupProgress.Maximum = max;
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        public void updateMin(int min)
        {
            const string location = CLASSNAME + ".updateMin";
            try
            {
                if (progressBarPopupProgress != null && min >= 0 && min <= progressBarPopupProgress.Maximum)
                {
                    progressBarPopupProgress.Minimum = min;
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        public void updateProgress(int value)
        {
            const string location = CLASSNAME + ".updateProgress";
            try
            {
                if (progressBarPopupProgress != null)
                {
                    if (
                        value >= progressBarPopupProgress.Minimum && 
                        value <= progressBarPopupProgress.Maximum
                    )
                    {
                        progressBarPopupProgress.Value = value;
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        public void updateTitle(string title)
        {
            const string location = CLASSNAME + ".updateTitle";
            try
            {
                if (title != null)
                {
                    this.Text = title;
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }

        private void btnPopupProgress1_Click(object sender, EventArgs e)
        {
            const string location = CLASSNAME + ".btnPopupProgress1_Click";
            try
            {
                L.l(location, "Closing progress from button.");
                this.Close();
                this.Dispose();
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }
    }

    public class ProgressUpdater
    {
        public bool isValid = false;
        public IProgress<string> titleUpdater = null;
        public IProgress<string> messageUpdater = null;
        public IProgress<int> progressUpdater = null;
        public IProgress<int> maxUpdater = null;
        public IProgress<int> minUpdater = null;
    }
}
