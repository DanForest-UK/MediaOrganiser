namespace MediaOrganiser
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

  
        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnBrowseFolder = new Button();
            txtFolderPath = new TextBox();
            btnScanFiles = new Button();
            progressScan = new ProgressBar();
            folderBrowserDialog = new FolderBrowserDialog();
            lblStatus = new Label();
            SuspendLayout();
            // 
            // btnBrowseFolder
            // 
            btnBrowseFolder.Location = new Point(26, 28);
            btnBrowseFolder.Name = "btnBrowseFolder";
            btnBrowseFolder.Size = new Size(150, 46);
            btnBrowseFolder.TabIndex = 0;
            btnBrowseFolder.Text = "Browse...";
            btnBrowseFolder.UseVisualStyleBackColor = true;
            btnBrowseFolder.Click += btnBrowseFolder_Click;
            // 
            // txtFolderPath
            // 
            txtFolderPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFolderPath.Location = new Point(194, 33);
            txtFolderPath.Name = "txtFolderPath";
            txtFolderPath.ReadOnly = true;
            txtFolderPath.Size = new Size(1845, 39);
            txtFolderPath.TabIndex = 1;
            // 
            // btnScanFiles
            // 
            btnScanFiles.Enabled = false;
            btnScanFiles.Location = new Point(2060, 29);
            btnScanFiles.Name = "btnScanFiles";
            btnScanFiles.Size = new Size(150, 46);
            btnScanFiles.TabIndex = 2;
            btnScanFiles.Text = "Scan Files";
            btnScanFiles.UseVisualStyleBackColor = true;
            btnScanFiles.Click += btnScanFiles_Click;
            // 
            // progressScan
            // 
            progressScan.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressScan.Location = new Point(50, 1003);
            progressScan.Name = "progressScan";
            progressScan.Size = new Size(2151, 46);
            progressScan.TabIndex = 4;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(50, 943);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(78, 32);
            lblStatus.TabIndex = 5;
            lblStatus.Text = "Ready";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2235, 1061);
            Controls.Add(lblStatus);
            Controls.Add(progressScan);
            Controls.Add(btnScanFiles);
            Controls.Add(txtFolderPath);
            Controls.Add(btnBrowseFolder);
            Name = "Form1";
            Text = "Media Organiser";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnBrowseFolder;
        private TextBox txtFolderPath;
        private Button btnScanFiles;
        private ProgressBar progressScan;
        private FolderBrowserDialog folderBrowserDialog;
        private Label lblStatus;
    }
}