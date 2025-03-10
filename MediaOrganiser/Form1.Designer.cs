using MaterialSkin.Controls;
using System.Drawing;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using SizeF = System.Drawing.SizeF;

namespace MediaOrganiser
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        internal System.ComponentModel.IContainer components = null;


        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        internal void InitializeComponent()
        {
            btnBrowseFolder = new MaterialButton();
            txtFolderPath = new MaterialTextBox();
            btnScanFiles = new MaterialButton();
            progressScan = new MaterialProgressBar();
            folderBrowserDialog = new FolderBrowserDialog();
            lblStatus = new MaterialLabel();
            picCurrentImage = new PictureBox();
            btnPrevious = new MaterialButton();
            btnNext = new MaterialButton();
            btnBin = new MaterialButton();
            btnKeep = new MaterialButton();
            pnlButtons = new Panel();
            pnlCenterButtons = new Panel();
            btnOrganiseFiles = new MaterialButton();
            switchCopyOnly = new MaterialSwitch();
            ((System.ComponentModel.ISupportInitialize)picCurrentImage).BeginInit();
            pnlButtons.SuspendLayout();
            pnlCenterButtons.SuspendLayout();
            SuspendLayout();
            // 
            // btnBrowseFolder
            // 
            btnBrowseFolder.AutoSize = false;
            btnBrowseFolder.Location = new Point(26, 85); // Offset for Material form header
            btnBrowseFolder.Name = "btnBrowseFolder";
            btnBrowseFolder.Size = new Size(150, 46);
            btnBrowseFolder.TabIndex = 0;
            btnBrowseFolder.Text = "Browse...";
            btnBrowseFolder.Type = MaterialButton.MaterialButtonType.Contained;
            btnBrowseFolder.UseAccentColor = false;
            btnBrowseFolder.UseVisualStyleBackColor = true;
            btnBrowseFolder.Click += btnBrowseFolder_Click;
            // 
            // txtFolderPath
            // 
            txtFolderPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFolderPath.AnimateReadOnly = false;
            txtFolderPath.BorderStyle = BorderStyle.None;
            txtFolderPath.Depth = 0;
            txtFolderPath.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtFolderPath.LeadingIcon = null;
            txtFolderPath.Location = new Point(194, 80);
            txtFolderPath.MaxLength = 50;
            txtFolderPath.MouseState = MaterialSkin.MouseState.OUT;
            txtFolderPath.Multiline = false;
            txtFolderPath.Name = "txtFolderPath";
            txtFolderPath.ReadOnly = true;
            txtFolderPath.Size = new Size(1845, 50);
            txtFolderPath.TabIndex = 1;
            txtFolderPath.TabStop = false;
            txtFolderPath.TrailingIcon = null;
            //
            // btnScanFiles
            //
            btnScanFiles.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnScanFiles.AutoSize = false;
            btnScanFiles.Enabled = false;
            btnScanFiles.Location = new Point(2060, 85);
            btnScanFiles.Name = "btnScanFiles";
            btnScanFiles.Size = new Size(150, 46);
            btnScanFiles.TabIndex = 2;
            btnScanFiles.Text = "Scan Files";
            btnScanFiles.Type = MaterialButton.MaterialButtonType.Contained;
            btnScanFiles.UseAccentColor = true;
            btnScanFiles.UseVisualStyleBackColor = true;
            btnScanFiles.Click += btnScanFiles_Click;
            // 
            // progressScan
            // 
            progressScan.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressScan.Depth = 0;
            progressScan.Location = new Point(50, 1003);
            progressScan.MouseState = MaterialSkin.MouseState.HOVER;
            progressScan.Name = "progressScan";
            progressScan.Size = new Size(2151, 5);
            progressScan.TabIndex = 4;
            progressScan.BackColor = Color.White;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.Depth = 0;
            lblStatus.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblStatus.Location = new Point(50, 943);
            lblStatus.MouseState = MaterialSkin.MouseState.HOVER;
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1667, 32);
            lblStatus.TabIndex = 5;
            lblStatus.Text = "Ready";
            // 
            // picCurrentImage
            // 
            picCurrentImage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            picCurrentImage.BackColor = Color.LightGray;
            picCurrentImage.Location = new Point(50, 200); // Adjusted for material toolbar
            picCurrentImage.Name = "picCurrentImage";
            picCurrentImage.Size = new Size(2151, 723);
            picCurrentImage.SizeMode = PictureBoxSizeMode.Zoom;
            picCurrentImage.TabIndex = 6;
            picCurrentImage.TabStop = false;
            // 
            // btnPrevious
            // 
            btnPrevious.AutoSize = false;
            btnPrevious.Enabled = false;
            btnPrevious.Location = new Point(3, 3);
            btnPrevious.Name = "btnPrevious";
            btnPrevious.Size = new Size(150, 46);
            btnPrevious.TabIndex = 0;
            btnPrevious.Text = "Back";
            btnPrevious.Type = MaterialButton.MaterialButtonType.Outlined;
            btnPrevious.UseAccentColor = false;
            btnPrevious.UseVisualStyleBackColor = true;
            btnPrevious.Click += btnPrevious_Click;
            // 
            // btnNext
            // 
            btnNext.AutoSize = false;
            btnNext.Enabled = false;
            btnNext.Location = new Point(153, 3);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(150, 46);
            btnNext.TabIndex = 1;
            btnNext.Text = "Next";
            btnNext.Type = MaterialButton.MaterialButtonType.Outlined;
            btnNext.UseAccentColor = false;
            btnNext.UseVisualStyleBackColor = true;
            btnNext.Click += btnNext_Click;
            // 
            // btnBin
            // 
            btnBin.AutoSize = false;
            btnBin.Enabled = false;
            btnBin.Location = new Point(0, 0);
            btnBin.Name = "btnBin";
            btnBin.Size = new Size(150, 46);
            btnBin.TabIndex = 1;
            btnBin.Text = "Bin";
            btnBin.Type = MaterialButton.MaterialButtonType.Contained;
            btnBin.UseAccentColor = true;
            btnBin.UseVisualStyleBackColor = true;
            btnBin.Click += btnBin_Click;
            // 
            // btnKeep
            // 
            btnKeep.AutoSize = false;
            btnKeep.Enabled = false;
            btnKeep.Location = new Point(170, 0);
            btnKeep.Name = "btnKeep";
            btnKeep.Size = new Size(150, 46);
            btnKeep.TabIndex = 2;
            btnKeep.Text = "Keep";
            btnKeep.Type = MaterialButton.MaterialButtonType.Contained;
            btnKeep.UseAccentColor = false;
            btnKeep.UseVisualStyleBackColor = true;
            btnKeep.Click += btnKeep_Click;
            // 
            // pnlButtons
            // 
            pnlButtons.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlButtons.Controls.Add(pnlCenterButtons);
            pnlButtons.Controls.Add(btnPrevious);
            pnlButtons.Controls.Add(btnNext);
            pnlButtons.Location = new Point(50, 150);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(2151, 50);
            pnlButtons.TabIndex = 7;
            // 
            // pnlCenterButtons
            // 
            pnlCenterButtons.Anchor = AnchorStyles.None;
            pnlCenterButtons.Controls.Add(btnBin);
            pnlCenterButtons.Controls.Add(btnKeep);
            pnlCenterButtons.Location = new Point(975, 3);
            pnlCenterButtons.Name = "pnlCenterButtons";
            pnlCenterButtons.Size = new Size(320, 46);
            pnlCenterButtons.TabIndex = 3;
            // 
            // btnOrganiseFiles
            // 
            btnOrganiseFiles.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOrganiseFiles.AutoSize = false;
            btnOrganiseFiles.Enabled = false;
            btnOrganiseFiles.Location = new Point(2051, 943);
            btnOrganiseFiles.Name = "btnOrganiseFiles";
            btnOrganiseFiles.Size = new Size(150, 46);
            btnOrganiseFiles.TabIndex = 8;
            btnOrganiseFiles.Text = "Organise Files";
            btnOrganiseFiles.Type = MaterialButton.MaterialButtonType.Contained;
            btnOrganiseFiles.UseAccentColor = false;
            btnOrganiseFiles.UseVisualStyleBackColor = true;
            btnOrganiseFiles.Click += btnOrganiseFiles_Click;
            // 
            // switchCopyOnly
            // 
            switchCopyOnly.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            switchCopyOnly.AutoSize = true;
            switchCopyOnly.Depth = 0;
            switchCopyOnly.Location = new Point(1877, 952);
            switchCopyOnly.Margin = new Padding(0);
            switchCopyOnly.MouseLocation = new Point(-1, -1);
            switchCopyOnly.MouseState = MaterialSkin.MouseState.HOVER;
            switchCopyOnly.Name = "switchCopyOnly";
            switchCopyOnly.Ripple = true;
            switchCopyOnly.Size = new Size(154, 37);
            switchCopyOnly.TabIndex = 9;
            switchCopyOnly.Text = "Copy only";
            switchCopyOnly.UseVisualStyleBackColor = true;
            switchCopyOnly.CheckedChanged += switchCopyOnly_CheckedChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2235, 1061);
            Controls.Add(switchCopyOnly);
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
            DrawerAutoShow = true;
            DrawerBackgroundWithAccent = true;
            DrawerHighlightWithAccent = true;
            DrawerShowIconsWhenHidden = true;
            DrawerTabControl = null;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)picCurrentImage).EndInit();
            pnlButtons.ResumeLayout(false);
            pnlCenterButtons.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        internal MaterialButton btnBrowseFolder;
        internal MaterialTextBox txtFolderPath;
        internal MaterialButton btnScanFiles;
        internal MaterialProgressBar progressScan;
        internal FolderBrowserDialog folderBrowserDialog;
        internal MaterialLabel lblStatus;
        internal PictureBox picCurrentImage;
        internal Panel pnlButtons;
        internal Panel pnlCenterButtons;
        internal MaterialButton btnKeep;
        internal MaterialButton btnBin;
        internal MaterialButton btnPrevious;
        internal MaterialButton btnNext;
        internal MaterialButton btnOrganiseFiles;
        internal MaterialSwitch switchCopyOnly;
    }
}