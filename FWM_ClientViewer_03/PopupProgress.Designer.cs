
namespace FWM_ClientViewer_03
{
    partial class PopupProgress
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblPopupProgressMessage = new System.Windows.Forms.Label();
            this.progressBarPopupProgress = new System.Windows.Forms.ProgressBar();
            this.btnPopupProgress1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblPopupProgressMessage
            // 
            this.lblPopupProgressMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPopupProgressMessage.Location = new System.Drawing.Point(12, 49);
            this.lblPopupProgressMessage.Name = "lblPopupProgressMessage";
            this.lblPopupProgressMessage.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.lblPopupProgressMessage.Size = new System.Drawing.Size(598, 124);
            this.lblPopupProgressMessage.TabIndex = 0;
            this.lblPopupProgressMessage.Text = "label1";
            this.lblPopupProgressMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressBarPopupProgress
            // 
            this.progressBarPopupProgress.Location = new System.Drawing.Point(59, 212);
            this.progressBarPopupProgress.Name = "progressBarPopupProgress";
            this.progressBarPopupProgress.Size = new System.Drawing.Size(500, 29);
            this.progressBarPopupProgress.TabIndex = 1;
            // 
            // btnPopupProgress1
            // 
            this.btnPopupProgress1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPopupProgress1.Location = new System.Drawing.Point(259, 269);
            this.btnPopupProgress1.Name = "btnPopupProgress1";
            this.btnPopupProgress1.Size = new System.Drawing.Size(100, 32);
            this.btnPopupProgress1.TabIndex = 2;
            this.btnPopupProgress1.Text = "Okay";
            this.btnPopupProgress1.UseVisualStyleBackColor = true;
            this.btnPopupProgress1.Click += new System.EventHandler(this.btnPopupProgress1_Click);
            // 
            // PopupProgress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(622, 333);
            this.Controls.Add(this.btnPopupProgress1);
            this.Controls.Add(this.progressBarPopupProgress);
            this.Controls.Add(this.lblPopupProgressMessage);
            this.Name = "PopupProgress";
            this.Text = "PopupProgress";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblPopupProgressMessage;
        private System.Windows.Forms.ProgressBar progressBarPopupProgress;
        private System.Windows.Forms.Button btnPopupProgress1;
    }
}