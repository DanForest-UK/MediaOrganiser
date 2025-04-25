namespace MediaOrganiser;

partial class MainForm
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
        pnlCenterControls = new Panel();
        tbFileName = new TextBox();
        btnOrganiseFiles = new Button();
        chkCopyOnly = new CheckBox();
        chkSortByYear = new CheckBox();
        chkKeepParentFolder = new CheckBox();
        pnlOptionsBottom = new Panel();
        btnRotateLeft = new Button();
        btnRotateRight = new Button();
        ((System.ComponentModel.ISupportInitialize)picCurrentImage).BeginInit();
        pnlButtons.SuspendLayout();
        pnlCenterControls.SuspendLayout();
        pnlOptionsBottom.SuspendLayout();
        SuspendLayout();
        // 
        // btnBrowseFolder
        // 
        btnBrowseFolder.Location = new System.Drawing.Point(11, 13);
        btnBrowseFolder.Margin = new Padding(4, 2, 4, 2);
        btnBrowseFolder.Name = "btnBrowseFolder";
        btnBrowseFolder.Size = new System.Drawing.Size(121, 47);
        btnBrowseFolder.TabIndex = 0;
        btnBrowseFolder.Text = "Browse...";
        btnBrowseFolder.UseVisualStyleBackColor = true;
        btnBrowseFolder.Click += btnBrowseFolder_Click;
        // 
        // txtFolderPath
        // 
        txtFolderPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtFolderPath.BorderStyle = BorderStyle.FixedSingle;
        txtFolderPath.Location = new System.Drawing.Point(145, 16);
        txtFolderPath.Margin = new Padding(4, 2, 4, 2);
        txtFolderPath.Name = "txtFolderPath";
        txtFolderPath.ReadOnly = true;
        txtFolderPath.Size = new System.Drawing.Size(1330, 39);
        txtFolderPath.TabIndex = 1;
        // 
        // btnScanFiles
        // 
        btnScanFiles.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnScanFiles.Enabled = false;
        btnScanFiles.Location = new System.Drawing.Point(1484, 13);
        btnScanFiles.Margin = new Padding(4, 2, 4, 2);
        btnScanFiles.Name = "btnScanFiles";
        btnScanFiles.Size = new System.Drawing.Size(115, 47);
        btnScanFiles.TabIndex = 2;
        btnScanFiles.Text = "Scan Files";
        btnScanFiles.UseVisualStyleBackColor = true;
        btnScanFiles.Click += btnScanFiles_Click;
        // 
        // progressScan
        // 
        progressScan.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        progressScan.Location = new System.Drawing.Point(11, 832);
        progressScan.Margin = new Padding(4, 2, 4, 2);
        progressScan.Name = "progressScan";
        progressScan.Size = new System.Drawing.Size(1592, 32);
        progressScan.TabIndex = 4;
        // 
        // lblStatus
        // 
        lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblStatus.Location = new System.Drawing.Point(11, 781);
        lblStatus.Margin = new Padding(4, 0, 4, 0);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new System.Drawing.Size(828, 41);
        lblStatus.TabIndex = 5;
        lblStatus.Text = "Ready";
        // 
        // picCurrentImage
        // 
        picCurrentImage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        picCurrentImage.BackColor = System.Drawing.Color.FromArgb(250, 250, 250);
        picCurrentImage.BorderStyle = BorderStyle.FixedSingle;
        picCurrentImage.Location = new System.Drawing.Point(11, 134);
        picCurrentImage.Margin = new Padding(4, 2, 4, 2);
        picCurrentImage.Name = "picCurrentImage";
        picCurrentImage.Size = new System.Drawing.Size(1592, 631);
        picCurrentImage.SizeMode = PictureBoxSizeMode.Zoom;
        picCurrentImage.TabIndex = 6;
        picCurrentImage.TabStop = false;
        // 
        // btnPrevious
        // 
        btnPrevious.Enabled = false;
        btnPrevious.Location = new System.Drawing.Point(4, 4);
        btnPrevious.Margin = new Padding(4);
        btnPrevious.Name = "btnPrevious";
        btnPrevious.Size = new System.Drawing.Size(121, 45);
        btnPrevious.TabIndex = 0;
        btnPrevious.Text = "Back";
        btnPrevious.UseVisualStyleBackColor = true;
        btnPrevious.Click += btnPrevious_Click;
        // 
        // btnNext
        // 
        btnNext.Enabled = false;
        btnNext.Location = new System.Drawing.Point(134, 4);
        btnNext.Margin = new Padding(4);
        btnNext.Name = "btnNext";
        btnNext.Size = new System.Drawing.Size(121, 45);
        btnNext.TabIndex = 1;
        btnNext.Text = "Next";
        btnNext.UseVisualStyleBackColor = true;
        btnNext.Click += btnNext_Click;
        // 
        // btnBin
        // 
        btnBin.Enabled = false;
        btnBin.Location = new System.Drawing.Point(0, 0);
        btnBin.Margin = new Padding(4);
        btnBin.Name = "btnBin";
        btnBin.Size = new System.Drawing.Size(121, 45);
        btnBin.TabIndex = 1;
        btnBin.Text = "Bin";
        btnBin.UseVisualStyleBackColor = true;
        btnBin.Click += btnBin_Click;
        // 
        // btnKeep
        // 
        btnKeep.Enabled = false;
        btnKeep.Location = new System.Drawing.Point(126, 0);
        btnKeep.Margin = new Padding(4);
        btnKeep.Name = "btnKeep";
        btnKeep.Size = new System.Drawing.Size(118, 45);
        btnKeep.TabIndex = 2;
        btnKeep.Text = "Keep";
        btnKeep.UseVisualStyleBackColor = true;
        btnKeep.Click += btnKeep_Click;
        // 
        // pnlButtons
        // 
        pnlButtons.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pnlButtons.BackColor = System.Drawing.Color.FromArgb(240, 242, 245);
        pnlButtons.Controls.Add(pnlCenterControls);
        pnlButtons.Controls.Add(btnPrevious);
        pnlButtons.Controls.Add(btnNext);
        pnlButtons.Location = new System.Drawing.Point(11, 68);
        pnlButtons.Margin = new Padding(4);
        pnlButtons.Name = "pnlButtons";
        pnlButtons.Padding = new Padding(6);
        pnlButtons.Size = new System.Drawing.Size(1592, 58);
        pnlButtons.TabIndex = 7;
        // 
        // pnlCenterControls
        // 
        pnlCenterControls.Anchor = AnchorStyles.None;
        pnlCenterControls.Controls.Add(btnBin);
        pnlCenterControls.Controls.Add(btnKeep);
        pnlCenterControls.Controls.Add(tbFileName);
        pnlCenterControls.Location = new System.Drawing.Point(467, 4);
        pnlCenterControls.Name = "pnlCenterControls";
        pnlCenterControls.Size = new System.Drawing.Size(595, 45);
        pnlCenterControls.TabIndex = 5;
        // 
        // tbFileName
        // 
        tbFileName.Location = new System.Drawing.Point(250, 3);
        tbFileName.Name = "tbFileName";
        tbFileName.Size = new System.Drawing.Size(340, 39);
        tbFileName.TabIndex = 3;
        tbFileName.TextChanged += tbFileName_TextChanged;
        // 
        // btnOrganiseFiles
        // 
        btnOrganiseFiles.Anchor = AnchorStyles.Right;
        btnOrganiseFiles.Enabled = false;
        btnOrganiseFiles.Location = new System.Drawing.Point(615, 0);
        btnOrganiseFiles.Margin = new Padding(4);
        btnOrganiseFiles.Name = "btnOrganiseFiles";
        btnOrganiseFiles.Size = new System.Drawing.Size(130, 47);
        btnOrganiseFiles.TabIndex = 8;
        btnOrganiseFiles.Text = "Organise Files";
        btnOrganiseFiles.UseVisualStyleBackColor = true;
        btnOrganiseFiles.Click += btnOrganiseFiles_Click;
        // 
        // chkCopyOnly
        // 
        chkCopyOnly.Anchor = AnchorStyles.Right;
        chkCopyOnly.AutoSize = true;
        chkCopyOnly.Location = new System.Drawing.Point(453, 4);
        chkCopyOnly.Margin = new Padding(4);
        chkCopyOnly.Name = "chkCopyOnly";
        chkCopyOnly.Size = new System.Drawing.Size(154, 36);
        chkCopyOnly.TabIndex = 9;
        chkCopyOnly.Text = "Copy only";
        chkCopyOnly.UseVisualStyleBackColor = true;
        chkCopyOnly.CheckedChanged += chkCopyOnly_Changed;
        // 
        // chkSortByYear
        // 
        chkSortByYear.Anchor = AnchorStyles.Right;
        chkSortByYear.AutoSize = true;
        chkSortByYear.Location = new System.Drawing.Point(273, 4);
        chkSortByYear.Margin = new Padding(4);
        chkSortByYear.Name = "chkSortByYear";
        chkSortByYear.Size = new System.Drawing.Size(174, 36);
        chkSortByYear.TabIndex = 10;
        chkSortByYear.Text = "Sort by year";
        chkSortByYear.UseVisualStyleBackColor = true;
        chkSortByYear.CheckedChanged += ChkSortByYear_Changed;
        // 
        // chkKeepParentFolder
        // 
        chkKeepParentFolder.Anchor = AnchorStyles.Right;
        chkKeepParentFolder.AutoSize = true;
        chkKeepParentFolder.Location = new System.Drawing.Point(18, 4);
        chkKeepParentFolder.Margin = new Padding(4);
        chkKeepParentFolder.Name = "chkKeepParentFolder";
        chkKeepParentFolder.Size = new System.Drawing.Size(246, 36);
        chkKeepParentFolder.TabIndex = 11;
        chkKeepParentFolder.Text = "Keep parent folder";
        chkKeepParentFolder.UseVisualStyleBackColor = true;
        // 
        // pnlOptionsBottom
        // 
        pnlOptionsBottom.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        pnlOptionsBottom.Controls.Add(chkKeepParentFolder);
        pnlOptionsBottom.Controls.Add(chkSortByYear);
        pnlOptionsBottom.Controls.Add(chkCopyOnly);
        pnlOptionsBottom.Controls.Add(btnOrganiseFiles);
        pnlOptionsBottom.Location = new System.Drawing.Point(847, 781);
        pnlOptionsBottom.Margin = new Padding(4, 2, 4, 2);
        pnlOptionsBottom.Name = "pnlOptionsBottom";
        pnlOptionsBottom.Size = new System.Drawing.Size(756, 45);
        pnlOptionsBottom.TabIndex = 12;
        // 
        // btnRotateLeft
        // 
        btnRotateLeft.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnRotateLeft.Font = new Font("Segoe UI", 16F);
        btnRotateLeft.Location = new System.Drawing.Point(1432, 144);
        btnRotateLeft.Margin = new Padding(4);
        btnRotateLeft.Name = "btnRotateLeft";
        btnRotateLeft.Size = new System.Drawing.Size(81, 65);
        btnRotateLeft.TabIndex = 13;
        btnRotateLeft.Text = "↺";
        btnRotateLeft.UseVisualStyleBackColor = true;
        btnRotateLeft.Visible = false;
        btnRotateLeft.Click += btnRotateLeft_Click;
        // 
        // btnRotateRight
        // 
        btnRotateRight.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnRotateRight.Font = new Font("Segoe UI", 16F);
        btnRotateRight.Location = new System.Drawing.Point(1521, 144);
        btnRotateRight.Margin = new Padding(4);
        btnRotateRight.Name = "btnRotateRight";
        btnRotateRight.Size = new System.Drawing.Size(71, 65);
        btnRotateRight.TabIndex = 14;
        btnRotateRight.Text = "↻";
        btnRotateRight.UseVisualStyleBackColor = true;
        btnRotateRight.Visible = false;
        btnRotateRight.Click += btnRotateRight_Click;
        // 
        // Form1
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(1605, 881);
        Controls.Add(btnRotateLeft);
        Controls.Add(btnRotateRight);
        Controls.Add(pnlOptionsBottom);
        Controls.Add(picCurrentImage);
        Controls.Add(lblStatus);
        Controls.Add(progressScan);
        Controls.Add(pnlButtons);
        Controls.Add(btnScanFiles);
        Controls.Add(txtFolderPath);
        Controls.Add(btnBrowseFolder);
        Margin = new Padding(4);
        MinimumSize = new System.Drawing.Size(1590, 813);
        Name = "Form1";
        Text = "Media Organiser";
        Load += Form1_Load;
        ((System.ComponentModel.ISupportInitialize)picCurrentImage).EndInit();
        pnlButtons.ResumeLayout(false);
        pnlCenterControls.ResumeLayout(false);
        pnlCenterControls.PerformLayout();
        pnlOptionsBottom.ResumeLayout(false);
        pnlOptionsBottom.PerformLayout();
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
    private Button btnPrevious;
    private Button btnNext;
    private Button btnOrganiseFiles;
    private CheckBox chkCopyOnly;
    private CheckBox chkSortByYear;
    private CheckBox chkKeepParentFolder;
    private Panel pnlOptionsBottom;
    private Button btnRotateLeft;
    private Button btnRotateRight;
    private Panel pnlCenterControls;
    private Button btnBin;
    private Button btnKeep;
    private TextBox tbFileName;
}