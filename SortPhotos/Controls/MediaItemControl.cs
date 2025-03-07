using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SortPhotos.Core;
using static SortPhotos.Core.Types;

namespace SortPhotos.UI
{
    public partial class MediaItemControl : UserControl
    {
        private MediaInfo mediaInfo;
        public event EventHandler<MediaInfo> KeepClicked;
        public event EventHandler<MediaInfo> BinClicked;
        public event EventHandler<MediaInfo> PlayClicked;

        public MediaItemControl(MediaInfo fileInfo)
        {
            InitializeComponent();
            mediaInfo = fileInfo;

            // Set up the control
            SetupControl();
        }

        private void SetupControl()
        {
            // Set file details
            lblFilename.Text = mediaInfo.FileName;
            lblPath.Text = Path.GetDirectoryName(mediaInfo.FullPath);
            lblSize.Text = FormatFileSize(mediaInfo.Size);
            lblDate.Text = mediaInfo.Date.ToString("yyyy-MM-dd");

            // Load thumbnail if it's an image
            if (mediaInfo.Category == FileCategory.Image)
            {
                try
                {
                    using (var img = Image.FromFile(mediaInfo.FullPath))
                    {
                        pictureBox.Image = new Bitmap(img, pictureBox.Width, pictureBox.Height);
                    }
                }
                catch
                {
                    pictureBox.Image = Properties.Resources.ImagePlaceholder;
                }

                btnPlay.Visible = false;
            }
            else if (mediaInfo.Category == FileCategory.Video)
            {
                pictureBox.Image = Properties.Resources.VideoPlaceholder;
                btnPlay.Visible = true;
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;

            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }

            return $"{number:n1} {suffixes[counter]}";
        }

        private void btnKeep_Click(object sender, EventArgs e)
        {
            KeepClicked?.Invoke(this, mediaInfo);
        }

        private void btnBin_Click(object sender, EventArgs e)
        {
            BinClicked?.Invoke(this, mediaInfo);
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (mediaInfo.Category == FileCategory.Video)
            {
                PlayClicked?.Invoke(this, mediaInfo);
            }
        }

        // Design part of the UserControl
        private void InitializeComponent()
        {
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.lblFilename = new System.Windows.Forms.Label();
            this.lblPath = new System.Windows.Forms.Label();
            this.lblSize = new System.Windows.Forms.Label();
            this.lblDate = new System.Windows.Forms.Label();
            this.btnKeep = new System.Windows.Forms.Button();
            this.btnBin = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(10, 10);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(100, 100);
            this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            // 
            // lblFilename
            // 
            this.lblFilename.AutoSize = true;
            this.lblFilename.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblFilename.Location = new System.Drawing.Point(120, 10);
            this.lblFilename.Name = "lblFilename";
            this.lblFilename.Size = new System.Drawing.Size(69, 17);
            this.lblFilename.TabIndex = 1;
            this.lblFilename.Text = "Filename";
            // 
            // lblPath
            // 
            this.lblPath.AutoSize = true;
            this.lblPath.Location = new System.Drawing.Point(120, 35);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(31, 15);
            this.lblPath.TabIndex = 2;
            this.lblPath.Text = "Path";
            // 
            // lblSize
            // 
            this.lblSize.AutoSize = true;
            this.lblSize.Location = new System.Drawing.Point(120, 55);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(27, 15);
            this.lblSize.TabIndex = 3;
            this.lblSize.Text = "Size";
            // 
            // lblDate
            // 
            this.lblDate.AutoSize = true;
            this.lblDate.Location = new System.Drawing.Point(120, 75);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(31, 15);
            this.lblDate.TabIndex = 4;
            this.lblDate.Text = "Date";
            // 
            // btnKeep
            // 
            this.btnKeep.Image = Properties.Resources.TickIcon;
            this.btnKeep.Location = new System.Drawing.Point(350, 30);
            this.btnKeep.Name = "btnKeep";
            this.btnKeep.Size = new System.Drawing.Size(60, 30);
            this.btnKeep.TabIndex = 5;
            this.btnKeep.Text = "Keep";
            this.btnKeep.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnKeep.UseVisualStyleBackColor = true;
            this.btnKeep.Click += new System.EventHandler(this.btnKeep_Click);
            // 
            // btnBin
            // 
            this.btnBin.Image = Properties.Resources.BinIcon;
            this.btnBin.Location = new System.Drawing.Point(350, 65);
            this.btnBin.Name = "btnBin";
            this.btnBin.Size = new System.Drawing.Size(60, 30);
            this.btnBin.TabIndex = 6;
            this.btnBin.Text = "Bin";
            this.btnBin.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnBin.UseVisualStyleBackColor = true;
            this.btnBin.Click += new System.EventHandler(this.btnBin_Click);
            // 
            // btnPlay
            // 
            this.btnPlay.Image = Properties.Resources.PlayIcon;
            this.btnPlay.Location = new System.Drawing.Point(280, 45);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(60, 30);
            this.btnPlay.TabIndex = 7;
            this.btnPlay.Text = "Play";
            this.btnPlay.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // MediaItemControl
            // 
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.lblFilename);
            this.Controls.Add(this.lblPath);
            this.Controls.Add(this.lblSize);
            this.Controls.Add(this.lblDate);
            this.Controls.Add(this.btnKeep);
            this.Controls.Add(this.btnBin);
            this.Controls.Add(this.btnPlay);
            this.Name = "MediaItemControl";
            this.Size = new System.Drawing.Size(430, 120);
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Label lblFilename;
        private System.Windows.Forms.Label lblPath;
        private System.Windows.Forms.Label lblSize;
        private System.Windows.Forms.Label lblDate;
        private System.Windows.Forms.Button btnKeep;
        private System.Windows.Forms.Button btnBin;
        private System.Windows.Forms.Button btnPlay;
    }
}