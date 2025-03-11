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
            picCurrentImage = new PictureBox();
            btnPrevious = new Button();
            btnNext = new Button();
            btnBin = new Button();
            btnKeep = new Button();
            pnlButtons = new Panel();
            pnlCenterButtons = new Panel();
            btnOrganiseFiles = new Button();
            chkCopyOnly = new CheckBox();
            chkSortByYear = new CheckBox();
            chkKeepParentFolder = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)picCurrentImage).BeginInit();
            pnlButtons.SuspendLayout();
            pnlCenterButtons.SuspendLayout();
            SuspendLayout();
            // 
            // btnBrowseFolder
            // 
            btnBrowseFolder.Location = new System.Drawing.Point(26, 28);
            btnBrowseFolder.Name = "btnBrowseFolder";
            btnBrowseFolder.Size = new System.Drawing.Size(150, 46);
            btnBrowseFolder.TabIndex = 0;
            btnBrowseFolder.Text = "Browse...";
            btnBrowseFolder.UseVisualStyleBackColor = true;
            btnBrowseFolder.Click += btnBrowseFolder_Click;
            // 
            // txtFolderPath
            // 
            txtFolderPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFolderPath.BorderStyle = BorderStyle.FixedSingle;
            txtFolderPath.Location = new System.Drawing.Point(194, 33);
            txtFolderPath.Name = "txtFolderPath";
            txtFolderPath.ReadOnly = true;
            txtFolderPath.Size = new System.Drawing.Size(2123, 39);
            txtFolderPath.TabIndex = 1;
            // 
            // btnScanFiles
            // 
            btnScanFiles.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnScanFiles.Enabled = false;
            btnScanFiles.Location = new System.Drawing.Point(2338, 29);
            btnScanFiles.Name = "btnScanFiles";
            btnScanFiles.Size = new System.Drawing.Size(150, 46);
            btnScanFiles.TabIndex = 2;
            btnScanFiles.Text = "Scan Files";
            btnScanFiles.UseVisualStyleBackColor = true;
            btnScanFiles.Click += btnScanFiles_Click;
            // 
            // progressScan
            // 
            progressScan.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressScan.Location = new System.Drawing.Point(50, 1380);
            progressScan.Name = "progressScan";
            progressScan.Size = new System.Drawing.Size(2429, 46);
            progressScan.TabIndex = 4;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.Location = new System.Drawing.Point(50, 1320);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(1945, 32);
            lblStatus.TabIndex = 5;
            lblStatus.Text = "Ready";
            // 
            // picCurrentImage
            // 
            picCurrentImage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            picCurrentImage.BackColor = System.Drawing.Color.FromArgb(250, 250, 250);
            picCurrentImage.BorderStyle = BorderStyle.FixedSingle;
            picCurrentImage.Location = new System.Drawing.Point(50, 150);
            picCurrentImage.Name = "picCurrentImage";
            picCurrentImage.Size = new System.Drawing.Size(2429, 1150);
            picCurrentImage.SizeMode = PictureBoxSizeMode.Zoom;
            picCurrentImage.TabIndex = 6;
            picCurrentImage.TabStop = false;
            // 
            // btnPrevious
            // 
            btnPrevious.Enabled = false;
            btnPrevious.Location = new System.Drawing.Point(3, 3);
            btnPrevious.Name = "btnPrevious";
            btnPrevious.Size = new System.Drawing.Size(150, 46);
            btnPrevious.TabIndex = 0;
            btnPrevious.Text = "Back";
            btnPrevious.UseVisualStyleBackColor = true;
            btnPrevious.Click += btnPrevious_Click;
            // 
            // btnNext
            // 
            btnNext.Enabled = false;
            btnNext.Location = new System.Drawing.Point(170, 3);
            btnNext.Name = "btnNext";
            btnNext.Size = new System.Drawing.Size(150, 46);
            btnNext.TabIndex = 1;
            btnNext.Text = "Next";
            btnNext.UseVisualStyleBackColor = true;
            btnNext.Click += btnNext_Click;
            // 
            // btnBin
            // 
            btnBin.Enabled = false;
            btnBin.Location = new System.Drawing.Point(0, 0);
            btnBin.Name = "btnBin";
            btnBin.Size = new System.Drawing.Size(150, 46);
            btnBin.TabIndex = 1;
            btnBin.Text = "Bin";
            btnBin.UseVisualStyleBackColor = true;
            btnBin.Click += btnBin_Click;
            // 
            // btnKeep
            // 
            btnKeep.Enabled = false;
            btnKeep.Location = new System.Drawing.Point(170, 0);
            btnKeep.Name = "btnKeep";
            btnKeep.Size = new System.Drawing.Size(150, 46);
            btnKeep.TabIndex = 2;
            btnKeep.Text = "Keep";
            btnKeep.UseVisualStyleBackColor = true;
            btnKeep.Click += btnKeep_Click;
            // 
            // pnlButtons
            // 
            pnlButtons.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlButtons.BackColor = System.Drawing.Color.FromArgb(240, 242, 245);
            pnlButtons.Controls.Add(pnlCenterButtons);
            pnlButtons.Controls.Add(btnPrevious);
            pnlButtons.Controls.Add(btnNext);
            pnlButtons.Location = new System.Drawing.Point(50, 90);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Padding = new Padding(5);
            pnlButtons.Size = new System.Drawing.Size(2429, 60);
            pnlButtons.TabIndex = 7;
            // 
            // pnlCenterButtons
            // 
            pnlCenterButtons.Anchor = AnchorStyles.None;
            pnlCenterButtons.Controls.Add(btnBin);
            pnlCenterButtons.Controls.Add(btnKeep);
            pnlCenterButtons.Location = new System.Drawing.Point(1114, 3);
            pnlCenterButtons.Name = "pnlCenterButtons";
            pnlCenterButtons.Size = new System.Drawing.Size(320, 46);
            pnlCenterButtons.TabIndex = 3;
            // 
            // btnOrganiseFiles
            // 
            btnOrganiseFiles.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOrganiseFiles.Enabled = false;
            btnOrganiseFiles.Location = new System.Drawing.Point(2329, 1320);
            btnOrganiseFiles.Name = "btnOrganiseFiles";
            btnOrganiseFiles.Size = new System.Drawing.Size(150, 46);
            btnOrganiseFiles.TabIndex = 8;
            btnOrganiseFiles.Text = "Organise Files";
            btnOrganiseFiles.UseVisualStyleBackColor = true;
            btnOrganiseFiles.Click += btnOrganiseFiles_Click;
            // 
            // chkCopyOnly
            // 
            chkCopyOnly.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            chkCopyOnly.AutoSize = true;
            chkCopyOnly.Location = new System.Drawing.Point(2155, 1329);
            chkCopyOnly.Name = "chkCopyOnly";
            chkCopyOnly.Size = new System.Drawing.Size(154, 36);
            chkCopyOnly.TabIndex = 9;
            chkCopyOnly.Text = "Copy only";
            chkCopyOnly.UseVisualStyleBackColor = true;
            chkCopyOnly.CheckedChanged += chkCopyOnly_Changed;
            // 
            // chkSortByYear
            // 
            chkSortByYear.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            chkSortByYear.AutoSize = true;
            chkSortByYear.Location = new System.Drawing.Point(1965, 1329);
            chkSortByYear.Name = "chkSortByYear";
            chkSortByYear.Size = new System.Drawing.Size(174, 36);
            chkSortByYear.TabIndex = 10;
            chkSortByYear.Text = "Sort by year";
            chkSortByYear.UseVisualStyleBackColor = true;
            // 
            // chkKeepParentFolder
            // 
            chkKeepParentFolder.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            chkKeepParentFolder.AutoSize = true;
            chkKeepParentFolder.Location = new System.Drawing.Point(1701, 1326);
            chkKeepParentFolder.Name = "chkKeepParentFolder";
            chkKeepParentFolder.Size = new System.Drawing.Size(246, 36);
            chkKeepParentFolder.TabIndex = 11;
            chkKeepParentFolder.Text = "Keep parent folder";
            chkKeepParentFolder.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(2513, 1438);
            Controls.Add(chkKeepParentFolder);
            Controls.Add(chkSortByYear);
            Controls.Add(chkCopyOnly);
            Controls.Add(btnOrganiseFiles);
            Controls.Add(pnlButtons);
            Controls.Add(picCurrentImage);
            Controls.Add(lblStatus);
            Controls.Add(progressScan);
            Controls.Add(btnScanFiles);
            Controls.Add(txtFolderPath);
            Controls.Add(btnBrowseFolder);
            Name = "Form1";
            Text = "Media Organiser";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)picCurrentImage).EndInit();
            pnlButtons.ResumeLayout(false);
            pnlCenterButtons.ResumeLayout(false);
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
        private PictureBox picCurrentImage;
        private Panel pnlButtons;
        private Panel pnlCenterButtons;
        private Button btnKeep;
        private Button btnBin;
        private Button btnPrevious;
        private Button btnNext;
        private Button btnOrganiseFiles;
        private CheckBox chkCopyOnly;
        private CheckBox chkSortByYear;
        private CheckBox chkKeepParentFolder;
    }
}